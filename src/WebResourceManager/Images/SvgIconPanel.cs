using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using WebResourceManager.Banners;
using WebResourceManager.Models;
using WebResourceManager.Tools;

namespace WebResourceManager.Images
{
    /// <summary>
    /// Tool page for making an SVG icon web resource from the shared icon gallery:
    /// web resource details and appearance on top, the gallery and a preview at
    /// common icon sizes below.
    /// </summary>
    internal class SvgIconPanel : UserControl
    {
        private const string Caption = "Create an SVG icon";

        private readonly string webViewDataFolder;
        private readonly SvgIconSettings settings = new SvgIconSettings();
        private readonly Timer refreshTimer = new Timer { Interval = 150 };

        private Microsoft.Xrm.Sdk.IOrganizationService service;
        private bool loaded;
        private bool previewReady;
        private bool galleryReady;
        private string lastGalleryHtml;

        private WebResourceDetailsBox details;
        private Label lblIcon;
        private ComboBox cbBackground;
        private ColorPicker cpBackground;
        private WebView2 preview;
        private WebView2 gallery;
        private Label lblPreviewUnavailable;
        private Button btnExport;
        private Button btnSave;
        private Label lblStatus;
        private SaveProgress saveProgress;

        public SvgIconPanel(Microsoft.Xrm.Sdk.IOrganizationService service, string webViewDataFolder)
        {
            this.service = service;
            this.webViewDataFolder = webViewDataFolder;

            Font = new Font("Segoe UI", 9F);
            BackColor = SystemColors.Control;

            BuildLayout();
            UpdateIconLabel();
            UpdateBackgroundControls();

            refreshTimer.Tick += (s, e) =>
            {
                refreshTimer.Stop();
                RefreshPreview();
            };

            Load += SvgIconPanel_Load;
        }

        /// <summary>
        /// Raised after the icon is saved to Dataverse, with the web resource name.
        /// </summary>
        public event EventHandler<string> Saved;

        /// <summary>
        /// Raised when Save is clicked without a connection, so the host can open the connection dialog.
        /// </summary>
        public event EventHandler ConnectionRequested
        {
            add { details.ConnectionRequested += value; }
            remove { details.ConnectionRequested -= value; }
        }

        public void SetService(Microsoft.Xrm.Sdk.IOrganizationService newService)
        {
            service = newService;
            details.SetService(newService);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                refreshTimer.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Layout

        private void BuildLayout()
        {
            var header = ActionIcons.ToolHeader(Caption, "Pick an icon, set its colours and size, and save it as an SVG web resource. Export saves the .svg file.", ActionIcons.ToolSvg);

            var divider = new Label { Dock = DockStyle.Top, Height = 1, BackColor = Color.LightGray };

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 42,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(8, 8, 8, 4)
            };
            btnSave = new Button { Text = "Save to Dataverse", Width = 130, Enabled = false };
            btnExport = new Button { Text = "Export SVG...", Width = 100 };
            ActionIcons.Apply(btnSave, ActionIcons.Save);
            ActionIcons.Apply(btnExport, ActionIcons.Export);
            lblStatus = new Label { AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 8, 12, 3) };
            btnExport.Click += BtnExport_Click;
            btnSave.Click += BtnSave_Click;
            saveProgress = new SaveProgress(lblStatus);
            buttons.Controls.AddRange(new Control[] { btnSave, btnExport, saveProgress, lblStatus });

            // Details | appearance on top, icons | preview below
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(8, 6, 8, 0) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 300));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            details = new WebResourceDetailsBox(service, "icon", "icons/my_icon", "My icon") { Extensions = new[] { ".svg" } };
            details.StateChanged += (s, e) => UpdateSaveState();

            var iconsGroup = new GroupBox { Text = "Icons (click to choose)", Dock = DockStyle.Fill, Padding = new Padding(8), MinimumSize = new Size(0, 120) };
            gallery = new WebView2 { Dock = DockStyle.Fill };
            iconsGroup.Controls.Add(gallery);

            var previewGroup = new GroupBox { Text = "Preview", Dock = DockStyle.Fill, Padding = new Padding(8) };
            preview = new WebView2 { Dock = DockStyle.Fill, DefaultBackgroundColor = Color.White };
            lblPreviewUnavailable = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Visible = false };
            previewGroup.Controls.Add(preview);
            previewGroup.Controls.Add(lblPreviewUnavailable);

            layout.Controls.Add(details, 0, 0);
            layout.Controls.Add(BuildAppearanceGroup(), 1, 0);
            layout.Controls.Add(iconsGroup, 0, 1);
            layout.Controls.Add(previewGroup, 1, 1);

            Controls.Add(layout);
            Controls.Add(buttons);
            Controls.Add(divider);
            Controls.Add(header);
        }

        private GroupBox BuildAppearanceGroup()
        {
            var group = new GroupBox { Text = "Appearance", Dock = DockStyle.Fill, Padding = new Padding(8) };
            var table = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2 };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            lblIcon = new Label { AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Margin = new Padding(3, 7, 3, 3) };

            var cpIcon = new ColorPicker(settings.IconColor);
            cpIcon.ColorChanged += (s, e) => { settings.IconColor = cpIcon.Color; QueueRefresh(); };
            var chkFilled = new CheckBox { Text = "Fill icon shape", AutoSize = true, Checked = settings.Filled, Margin = new Padding(12, 5, 3, 3) };
            chkFilled.CheckedChanged += (s, e) => { settings.Filled = chkFilled.Checked; QueueRefresh(); };

            var nudStroke = new NumericUpDown { Minimum = 0.5m, Maximum = 3m, Increment = 0.25m, DecimalPlaces = 2, Value = settings.StrokeWidth, Width = 60 };
            nudStroke.ValueChanged += (s, e) => { settings.StrokeWidth = nudStroke.Value; QueueRefresh(); };

            cbBackground = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130 };
            cbBackground.Items.AddRange(new object[] { "None (transparent)", "Circle", "Rounded square", "Square" });
            cbBackground.SelectedIndex = (int)settings.Background;
            cbBackground.SelectedIndexChanged += (s, e) =>
            {
                settings.Background = (SvgIconBackground)cbBackground.SelectedIndex;
                UpdateBackgroundControls();
                QueueRefresh();
            };
            cpBackground = new ColorPicker(settings.BackgroundColor);
            cpBackground.ColorChanged += (s, e) => { settings.BackgroundColor = cpBackground.Color; QueueRefresh(); };

            var nudSize = new NumericUpDown { Minimum = 12, Maximum = 512, Value = settings.Size, Width = 60 };
            nudSize.ValueChanged += (s, e) => { settings.Size = (int)nudSize.Value; QueueRefresh(); };

            AddRow(table, "Icon:", lblIcon);
            AddRow(table, "Icon colour:", Row(cpIcon, chkFilled));
            AddRow(table, "Line width:", Row(nudStroke, Hint("(2 matches the other tools)")));
            AddRow(table, "Background:", Row(cbBackground, cpBackground));
            AddRow(table, "Size:", Row(nudSize, Hint("px  (SVG scales cleanly; this is its default size)")));
            group.Controls.Add(table);
            return group;
        }

        private static void AddRow(TableLayoutPanel table, string label, Control control)
        {
            var row = table.RowCount;
            table.RowCount = row + 1;
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(3, 7, 3, 3) }, 0, row);
            table.Controls.Add(control, 1, row);
        }

        private static FlowLayoutPanel Row(params Control[] controls)
        {
            var row = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
            row.Controls.AddRange(controls);
            return row;
        }

        private static Label Hint(string text)
        {
            return new Label { Text = text, AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 6, 3, 3) };
        }

        private void UpdateIconLabel()
        {
            lblIcon.Text = settings.Icon.Label + "  (" + settings.Icon.Category + ")";
        }

        private void UpdateBackgroundControls()
        {
            cpBackground.Enabled = settings.Background != SvgIconBackground.None;
        }

        #endregion

        #region Loading and preview

        private async void SvgIconPanel_Load(object sender, EventArgs e)
        {
            // Load can fire again if the handle is re-created; set up only once
            if (loaded)
            {
                return;
            }

            loaded = true;
            try
            {
                var environment = await CoreWebView2Environment.CreateAsync(null, webViewDataFolder);

                await preview.EnsureCoreWebView2Async(environment);
                preview.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                preview.CoreWebView2.Profile.PreferredColorScheme = CoreWebView2PreferredColorScheme.Light;
                previewReady = true;

                await gallery.EnsureCoreWebView2Async(environment);
                gallery.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                gallery.CoreWebView2.WebMessageReceived += Gallery_WebMessageReceived;
                galleryReady = true;

                RefreshPreview();
            }
            catch (Exception ex)
            {
                preview.Visible = false;
                gallery.Visible = false;
                lblPreviewUnavailable.Text = "The preview needs the Microsoft Edge WebView2 runtime.\r\n" + ex.Message;
                lblPreviewUnavailable.Visible = true;
            }
        }

        private void QueueRefresh()
        {
            // Debounce so typing a colour or spinning a number doesn't reload on every change
            refreshTimer.Stop();
            refreshTimer.Start();
        }

        private void RefreshPreview()
        {
            if (previewReady)
            {
                preview.CoreWebView2.NavigateToString(SvgIconBuilder.BuildPreviewHtml(settings));
            }

            if (galleryReady)
            {
                // The selected tile takes the background colour when there is one, so the choice previews in place
                var tile = settings.Background != SvgIconBackground.None ? settings.BackgroundColor : Color.FromArgb(0xF3, 0xF2, 0xF1);
                var html = IconGallery.BuildHtml(settings.Icon, settings.Filled, tile, Color.FromArgb(0x32, 0x31, 0x30), settings.IconColor, false);
                if (html != lastGalleryHtml)
                {
                    lastGalleryHtml = html;
                    _ = IconGalleryView.ShowAsync(gallery, html);
                }
            }
        }

        private void Gallery_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var icon = IconGallery.FindByKey(e.TryGetWebMessageAsString());
            if (icon != null && icon != BannerIcon.None)
            {
                settings.Icon = icon;
                UpdateIconLabel();
                QueueRefresh();
            }
        }

        #endregion

        #region Save and export

        private void UpdateSaveState()
        {
            btnSave.Enabled = details.CanSave;
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (!details.ValidateName(Caption))
            {
                return;
            }

            using (var dialog = new SaveFileDialog
            {
                FileName = details.FullName.Split('/').Last(),
                Filter = "SVG files (*.svg)|*.svg|All files (*.*)|*.*",
                DefaultExt = "svg"
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    File.WriteAllText(dialog.FileName, SvgIconBuilder.Build(settings), new UTF8Encoding(false));
                    lblStatus.Text = "Exported to " + Path.GetFileName(dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Could not export the icon: " + ex.Message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (!details.EnsureConnected() || !details.CanSave || !details.ValidateName(Caption))
            {
                return;
            }

            var solution = details.SelectedSolution;
            SetBusy(true);
            try
            {
                var outcome = await WebResourcePublisher.SaveAsync(this, Caption, service, details, WebResourceType.Svg,
                    SvgIconBuilder.Build(settings), "this icon", saveProgress.Report);
                if (outcome == SaveOutcome.Cancelled)
                {
                    saveProgress.Reset();
                    return;
                }

                var name = details.FullName;
                Saved?.Invoke(this, name);
                saveProgress.Complete("Saved and published " + name);
                MessageBox.Show(this,
                    string.Format("{0} was {1} and published{2}.", name, outcome == SaveOutcome.Updated ? "updated" : "created",
                        solution.IsDefault ? string.Empty : " in " + solution.FriendlyName),
                    Caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                saveProgress.Fail();
                MessageBox.Show(this, "Could not save the icon: " + ex.Message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void SetBusy(bool busy)
        {
            UseWaitCursor = busy;
            btnExport.Enabled = !busy;
            btnSave.Enabled = !busy && details.CanSave;
        }

        #endregion
    }
}
