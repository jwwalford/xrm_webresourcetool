using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using WebResourceManager.Models;
using WebResourceManager.Tools;

namespace WebResourceManager.Banners
{
    /// <summary>
    /// Single-step banner designer shown in the right panel when the Tools tab is
    /// active. Naming, design, icon choice and the live preview sit on one page;
    /// Save creates an HTML web resource and Export saves the file to disk.
    /// </summary>
    public class BannerDesignerPanel : UserControl
    {
        private const string Caption = "Create a banner";

        private readonly string webViewDataFolder;
        private readonly BannerSettings settings = new BannerSettings();
        private readonly Timer refreshTimer = new Timer { Interval = 150 };

        private Microsoft.Xrm.Sdk.IOrganizationService service;
        private bool previewReady;
        private bool galleryReady;
        private bool loaded;
        private string lastGalleryHtml;

        private Button btnExport;
        private Button btnSave;
        private Label lblStatus;
        private TableLayoutPanel layout;

        private WebResourceDetailsBox details;

        // Design
        private TextBox txtText;
        private ColorPicker cpBackground;
        private ColorPicker cpText;
        private NumericUpDown nudFontSize;
        private CheckBox chkBold;
        private ComboBox cbAlignment;
        private ComboBox cbIcon;
        private ColorPicker cpIcon;
        private CheckBox chkIconFilled;
        private NumericUpDown nudPreviewHeight;
        private Panel pnlPreviewHost;
        private WebView2 preview;
        private WebView2 gallery;
        private Label lblPreviewUnavailable;

        public BannerDesignerPanel(Microsoft.Xrm.Sdk.IOrganizationService service, string webViewDataFolder)
        {
            this.service = service;
            this.webViewDataFolder = webViewDataFolder;

            Font = new Font("Segoe UI", 9F);
            BackColor = SystemColors.Control;

            BuildLayout();

            refreshTimer.Tick += (s, e) =>
            {
                refreshTimer.Stop();
                RefreshPreview();
            };

            Load += BannerDesignerPanel_Load;
        }

        /// <summary>
        /// Raised after the banner is saved to Dataverse, with the web resource name.
        /// </summary>
        public event EventHandler<string> Saved;

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
            var header = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.White, Padding = new Padding(12, 6, 12, 4) };
            header.Controls.Add(new Label
            {
                Dock = DockStyle.Top,
                Height = 20,
                ForeColor = Color.DimGray,
                Text = "Name it, design it and pick an icon. Save creates the HTML web resource in Dataverse; Export saves the file."
            });
            header.Controls.Add(new Label { Dock = DockStyle.Top, Height = 24, Text = Caption, Font = new Font("Segoe UI", 12F, FontStyle.Bold) });

            var divider = new Label { Dock = DockStyle.Top, Height = 1, BackColor = Color.LightGray };

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 42,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(8, 8, 8, 4)
            };
            btnSave = new Button { Text = "Save to Dataverse", Width = 130 };
            btnExport = new Button { Text = "Export HTML...", Width = 110 };
            ActionIcons.Apply(btnSave, ActionIcons.Save);
            ActionIcons.Apply(btnExport, ActionIcons.Export);
            lblStatus = new Label { AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 8, 12, 3) };
            btnExport.Click += BtnExport_Click;
            btnSave.Click += BtnSave_Click;
            buttons.Controls.AddRange(new Control[] { btnSave, btnExport, lblStatus });

            // Top row: details and banner settings side by side; then icons; then preview
            layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3, Padding = new Padding(8, 6, 8, 0) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            // Fixed height fits the eight banner settings rows
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 275));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 160)); // sized to the preview height in LayoutPreview

            var detailsGroup = BuildDetailsGroup();
            var settingsGroup = BuildSettingsGroup();

            var iconsGroup = new GroupBox { Text = "Icons (click to choose)", Dock = DockStyle.Fill, Padding = new Padding(8), MinimumSize = new Size(0, 120) };
            gallery = new WebView2 { Dock = DockStyle.Fill };
            iconsGroup.Controls.Add(gallery);

            var previewGroup = BuildPreviewGroup();

            layout.Controls.Add(detailsGroup, 0, 0);
            layout.Controls.Add(settingsGroup, 1, 0);
            layout.Controls.Add(iconsGroup, 0, 1);
            layout.SetColumnSpan(iconsGroup, 2);
            layout.Controls.Add(previewGroup, 0, 2);
            layout.SetColumnSpan(previewGroup, 2);

            Controls.Add(layout);
            Controls.Add(buttons);
            Controls.Add(divider);
            Controls.Add(header);

            LayoutPreview();
        }

        private GroupBox BuildDetailsGroup()
        {
            details = new WebResourceDetailsBox(service, "banner", "banners/form_banner", "Form banner");
            details.StateChanged += (s, e) => UpdateSaveState();
            details.DisplayNameChanged += (s, e) => { settings.Title = details.DisplayName; QueueRefresh(); };
            settings.Title = details.DisplayName;
            return details;
        }

        private GroupBox BuildSettingsGroup()
        {
            var group = new GroupBox { Text = "Banner", Dock = DockStyle.Fill, Padding = new Padding(8) };
            var table = NewFormTable();

            txtText = new TextBox { Dock = DockStyle.Fill, Text = settings.Text };
            txtText.TextChanged += (s, e) => { settings.Text = txtText.Text; QueueRefresh(); };

            cpBackground = new ColorPicker(settings.BackgroundColor);
            cpBackground.ColorChanged += (s, e) => { settings.BackgroundColor = cpBackground.Color; QueueRefresh(); };

            cpText = new ColorPicker(settings.TextColor);
            cpText.ColorChanged += (s, e) => { settings.TextColor = cpText.Color; QueueRefresh(); };

            nudFontSize = new NumericUpDown { Minimum = 10, Maximum = 48, Value = settings.FontSize, Width = 60 };
            nudFontSize.ValueChanged += (s, e) => { settings.FontSize = (int)nudFontSize.Value; QueueRefresh(); };
            chkBold = new CheckBox { Text = "Bold", AutoSize = true, Checked = settings.Bold, Margin = new Padding(12, 5, 3, 3) };
            chkBold.CheckedChanged += (s, e) => { settings.Bold = chkBold.Checked; QueueRefresh(); };
            var fontPanel = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
            fontPanel.Controls.Add(nudFontSize);
            fontPanel.Controls.Add(new Label { Text = "px", AutoSize = true, Margin = new Padding(0, 6, 3, 3) });
            fontPanel.Controls.Add(chkBold);

            cbAlignment = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
            cbAlignment.Items.AddRange(new object[] { "Left", "Centre" });
            cbAlignment.SelectedIndex = settings.Alignment == BannerAlignment.Center ? 1 : 0;
            cbAlignment.SelectedIndexChanged += (s, e) =>
            {
                settings.Alignment = cbAlignment.SelectedIndex == 1 ? BannerAlignment.Center : BannerAlignment.Left;
                QueueRefresh();
            };

            cbIcon = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
            cbIcon.Items.AddRange(BannerIcon.All.Cast<object>().ToArray());
            cbIcon.SelectedItem = settings.Icon;
            cbIcon.SelectedIndexChanged += (s, e) => { settings.Icon = cbIcon.SelectedItem as BannerIcon; QueueRefresh(); };

            cpIcon = new ColorPicker(settings.IconColor);
            cpIcon.ColorChanged += (s, e) => { settings.IconColor = cpIcon.Color; QueueRefresh(); };

            chkIconFilled = new CheckBox { Text = "Fill icon shape", AutoSize = true, Checked = settings.IconFilled };
            chkIconFilled.CheckedChanged += (s, e) => { settings.IconFilled = chkIconFilled.Checked; QueueRefresh(); };

            AddRow(table, "Text:", txtText);
            AddRow(table, "Text colour:", cpText);
            AddRow(table, "Background:", cpBackground);
            AddRow(table, "Font size:", fontPanel);
            AddRow(table, "Alignment:", cbAlignment);
            AddRow(table, "Icon:", cbIcon);
            AddRow(table, "Icon colour:", cpIcon);
            AddRow(table, string.Empty, chkIconFilled);
            group.Controls.Add(table);
            return group;
        }

        private GroupBox BuildPreviewGroup()
        {
            var previewGroup = new GroupBox { Text = "Preview", Dock = DockStyle.Fill, Padding = new Padding(8) };
            var previewOptions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 30, WrapContents = false };
            nudPreviewHeight = new NumericUpDown { Minimum = 24, Maximum = 200, Value = 50, Width = 60 };
            nudPreviewHeight.ValueChanged += (s, e) => LayoutPreview();
            previewOptions.Controls.Add(new Label { Text = "Preview height:", AutoSize = true, Margin = new Padding(0, 6, 3, 3) });
            previewOptions.Controls.Add(nudPreviewHeight);
            previewOptions.Controls.Add(new Label
            {
                Text = "px  (on the form, the row count sets the height)",
                AutoSize = true,
                ForeColor = Color.DimGray,
                Margin = new Padding(0, 6, 3, 3)
            });

            // Grey surround so the banner's edges are visible, like a form section
            pnlPreviewHost = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(0xF3, 0xF2, 0xF1), Padding = new Padding(12) };
            preview = new WebView2 { Dock = DockStyle.Top };
            lblPreviewUnavailable = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Visible = false };
            pnlPreviewHost.Controls.Add(lblPreviewUnavailable);
            pnlPreviewHost.Controls.Add(preview);

            previewGroup.Controls.Add(pnlPreviewHost);
            previewGroup.Controls.Add(previewOptions);

            return previewGroup;
        }

        private static TableLayoutPanel NewFormTable()
        {
            var table = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2 };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            return table;
        }

        private static void AddRow(TableLayoutPanel table, string label, Control control)
        {
            var row = table.RowCount;
            table.RowCount = row + 1;
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(3, 7, 3, 3) }, 0, row);
            table.Controls.Add(control, 1, row);
        }

        private void LayoutPreview()
        {
            preview.Height = (int)nudPreviewHeight.Value;

            // Room for the group caption, the height option and the grey surround
            layout.RowStyles[2].Height = preview.Height + 100;
        }

        #endregion

        private void UpdateSaveState()
        {
            btnSave.Enabled = details.CanSave;
        }

        #region Loading

        private void BannerDesignerPanel_Load(object sender, EventArgs e)
        {
            // Load can fire again if the handle is re-created; set up only once
            if (loaded)
            {
                return;
            }

            loaded = true;
            InitializeWebViews();
        }

        /// <summary>
        /// Points the designer at a new connection (or none) and reloads the solution list.
        /// </summary>
        public void SetService(Microsoft.Xrm.Sdk.IOrganizationService newService)
        {
            service = newService;
            details.SetService(newService);
        }

        private async void InitializeWebViews()
        {
            try
            {
                var environment = await CoreWebView2Environment.CreateAsync(null, webViewDataFolder);

                await preview.EnsureCoreWebView2Async(environment);
                preview.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
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

        #endregion

        #region Preview

        private void QueueRefresh()
        {
            // Debounce so typing and dragging in the colour dialog don't reload on every keystroke
            refreshTimer.Stop();
            refreshTimer.Start();
        }

        private void RefreshPreview()
        {
            if (previewReady)
            {
                preview.CoreWebView2.NavigateToString(BannerHtmlBuilder.Build(settings));
            }

            if (galleryReady)
            {
                var html = IconGallery.BuildHtml(settings.Icon, settings.IconFilled, settings.BackgroundColor, settings.TextColor, settings.IconColor);
                if (html != lastGalleryHtml)
                {
                    lastGalleryHtml = html;
                    gallery.CoreWebView2.NavigateToString(html);
                }
            }
        }

        private void Gallery_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var key = e.TryGetWebMessageAsString();
            var icon = IconGallery.FindByKey(key);
            if (icon != null)
            {
                // Updates settings and the preview through the combo box handler
                cbIcon.SelectedItem = icon;
            }
        }

        #endregion

        #region Save and export

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (!details.ValidateName(Caption))
            {
                return;
            }

            var fileName = details.FullName.Split('/').Last();
            using (var dialog = new SaveFileDialog
            {
                FileName = fileName,
                Filter = "HTML files (*.html)|*.html|All files (*.*)|*.*",
                DefaultExt = "html"
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    File.WriteAllText(dialog.FileName, BannerHtmlBuilder.Build(settings), new UTF8Encoding(false));
                    lblStatus.Text = "Exported to " + Path.GetFileName(dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Could not export the banner: " + ex.Message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (!details.CanSave || !details.ValidateName(Caption))
            {
                return;
            }

            var solution = details.SelectedSolution;
            SetBusy(true, null);
            try
            {
                var outcome = await WebResourcePublisher.SaveAsync(this, Caption, service, details, WebResourceType.Html,
                    BannerHtmlBuilder.Build(settings), "this banner", status => lblStatus.Text = status);
                if (outcome == SaveOutcome.Cancelled)
                {
                    lblStatus.Text = string.Empty;
                    return;
                }

                var name = details.FullName;
                Saved?.Invoke(this, name);
                lblStatus.Text = "Saved and published " + name;
                MessageBox.Show(this,
                    string.Format("{0} was {1} and published{2}.\r\n\r\nAdd it to a form as a web resource; set the row count to control the banner height and turn off \"Display label on the form\" and the border.",
                        name, outcome == SaveOutcome.Updated ? "updated" : "created", solution.IsDefault ? string.Empty : " in " + solution.FriendlyName),
                    Caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = string.Empty;
                MessageBox.Show(this, "Could not save the banner: " + ex.Message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false, null);
            }
        }

        private void SetBusy(bool busy, string message)
        {
            UseWaitCursor = busy;
            btnSave.Enabled = !busy;
            btnExport.Enabled = !busy;
            if (message != null)
            {
                lblStatus.Text = message;
            }

            if (!busy)
            {
                UpdateSaveState();
            }
        }

        #endregion
    }
}
