using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using ScintillaNET;
using WebResourceManager.Banners;
using WebResourceManager.Models;
using WebResourceManager.Tools;

namespace WebResourceManager.Buttons
{
    /// <summary>
    /// Single-step designer for a form button, shown in the right panel when the
    /// Tools tab is active: web resource details, look, icon, click action and a
    /// live preview on one page. Save creates an HTML web resource; Export saves
    /// the file to disk.
    /// </summary>
    internal class ButtonDesignerPanel : UserControl
    {
        private const string Caption = "Create a button";

        private readonly string webViewDataFolder;
        private readonly ButtonSettings settings = new ButtonSettings();
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

        // Look
        private ComboBox cbIcon;
        private NumericUpDown nudWidth;
        private NumericUpDown nudHeight;
        private ComboBox cbPosition;

        // Click action
        private ComboBox cbAction;
        private Panel pnlUrl;
        private TextBox txtUrl;
        private Scintilla scriptEditor;
        private Label lblActionHint;

        // Preview
        private NumericUpDown nudPreviewHeight;
        private WebView2 preview;
        private WebView2 gallery;
        private Label lblPreviewUnavailable;

        public ButtonDesignerPanel(Microsoft.Xrm.Sdk.IOrganizationService service, string webViewDataFolder)
        {
            this.service = service;
            this.webViewDataFolder = webViewDataFolder;

            Font = new Font("Segoe UI", 9F);
            BackColor = SystemColors.Control;

            BuildLayout();
            UpdateSizeControls();
            UpdateActionControls();

            refreshTimer.Tick += (s, e) =>
            {
                refreshTimer.Stop();
                RefreshPreview();
            };

            Load += ButtonDesignerPanel_Load;
        }

        /// <summary>
        /// Raised after the button is saved to Dataverse, with the web resource name.
        /// </summary>
        public event EventHandler<string> Saved;

        /// <summary>
        /// Points the designer at a new connection (or none) and reloads the solution list.
        /// </summary>
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
            var header = ActionIcons.ToolHeader(Caption, "Style the button, pick an icon and choose what it does. Save creates the HTML web resource in Dataverse; Export saves the file.", ActionIcons.ToolButton);

            var divider = new Label { Dock = DockStyle.Top, Height = 1, BackColor = Color.LightGray };

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 42,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(8, 8, 8, 4)
            };
            btnSave = new Button { Text = "Save to Dataverse", Width = 130, Enabled = false };
            btnExport = new Button { Text = "Export HTML...", Width = 110 };
            ActionIcons.Apply(btnSave, ActionIcons.Save);
            ActionIcons.Apply(btnExport, ActionIcons.Export);
            lblStatus = new Label { AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 8, 12, 3) };
            btnExport.Click += BtnExport_Click;
            btnSave.Click += BtnSave_Click;
            buttons.Controls.AddRange(new Control[] { btnSave, btnExport, lblStatus });

            // Details | look on top, icons | click action in the middle, preview at the bottom
            layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3, Padding = new Padding(8, 6, 8, 0) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 345)); // fits the ten look rows
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 160)); // sized to the preview height in LayoutPreview

            details = new WebResourceDetailsBox(service, "button", "buttons/form_button", "Form button");
            details.StateChanged += (s, e) => UpdateSaveState();
            details.DisplayNameChanged += (s, e) => { settings.Title = details.DisplayName; QueueRefresh(); };
            settings.Title = details.DisplayName;

            var iconsGroup = new GroupBox { Text = "Icons (click to choose)", Dock = DockStyle.Fill, Padding = new Padding(8), MinimumSize = new Size(0, 120) };
            gallery = new WebView2 { Dock = DockStyle.Fill };
            iconsGroup.Controls.Add(gallery);

            var previewGroup = BuildPreviewGroup();

            layout.Controls.Add(details, 0, 0);
            layout.Controls.Add(BuildLookGroup(), 1, 0);
            layout.Controls.Add(iconsGroup, 0, 1);
            layout.Controls.Add(BuildActionGroup(), 1, 1);
            layout.Controls.Add(previewGroup, 0, 2);
            layout.SetColumnSpan(previewGroup, 2);

            Controls.Add(layout);
            Controls.Add(buttons);
            Controls.Add(divider);
            Controls.Add(header);

            LayoutPreview();
        }

        private GroupBox BuildLookGroup()
        {
            var group = new GroupBox { Text = "Button", Dock = DockStyle.Fill, Padding = new Padding(8) };
            var table = NewFormTable();

            var txtText = new TextBox { Dock = DockStyle.Fill, Text = settings.Text };
            txtText.TextChanged += (s, e) => { settings.Text = txtText.Text; QueueRefresh(); };

            var txtTooltip = new TextBox { Dock = DockStyle.Fill };
            txtTooltip.TextChanged += (s, e) => { settings.Tooltip = txtTooltip.Text; QueueRefresh(); };

            var cpText = new ColorPicker(settings.TextColor);
            cpText.ColorChanged += (s, e) => { settings.TextColor = cpText.Color; QueueRefresh(); };

            var cpBackground = new ColorPicker(settings.BackgroundColor);
            cpBackground.ColorChanged += (s, e) => { settings.BackgroundColor = cpBackground.Color; QueueRefresh(); };

            var nudFontSize = new NumericUpDown { Minimum = 8, Maximum = 36, Value = settings.FontSize, Width = 60 };
            nudFontSize.ValueChanged += (s, e) => { settings.FontSize = (int)nudFontSize.Value; QueueRefresh(); };
            var chkBold = new CheckBox { Text = "Bold", AutoSize = true, Checked = settings.Bold, Margin = new Padding(12, 5, 3, 3) };
            chkBold.CheckedChanged += (s, e) => { settings.Bold = chkBold.Checked; QueueRefresh(); };
            var fontPanel = Row(nudFontSize, Hint("px"), chkBold);

            cbIcon = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
            cbIcon.Items.AddRange(BannerIcon.All.Cast<object>().ToArray());
            cbIcon.SelectedItem = settings.Icon;
            cbIcon.SelectedIndexChanged += (s, e) => { settings.Icon = cbIcon.SelectedItem as BannerIcon; QueueRefresh(); };

            var cpIcon = new ColorPicker(settings.IconColor);
            cpIcon.ColorChanged += (s, e) => { settings.IconColor = cpIcon.Color; QueueRefresh(); };
            var chkIconFilled = new CheckBox { Text = "Fill icon shape", AutoSize = true, Checked = settings.IconFilled, Margin = new Padding(12, 5, 3, 3) };
            chkIconFilled.CheckedChanged += (s, e) => { settings.IconFilled = chkIconFilled.Checked; QueueRefresh(); };

            var nudCorners = new NumericUpDown { Minimum = 0, Maximum = 100, Value = settings.CornerRadius, Width = 60 };
            nudCorners.ValueChanged += (s, e) => { settings.CornerRadius = (int)nudCorners.Value; QueueRefresh(); };
            var cornersPanel = Row(nudCorners, Hint("px  (0 is square; 50+ gives a pill shape)"));

            var rbFill = new RadioButton { Text = "Fill the control", AutoSize = true, Checked = settings.SizeMode == ButtonSizeMode.Fill, Margin = new Padding(3, 5, 8, 3) };
            var rbFixed = new RadioButton { Text = "Fixed:", AutoSize = true, Checked = settings.SizeMode == ButtonSizeMode.Fixed, Margin = new Padding(3, 5, 3, 3) };
            rbFill.CheckedChanged += (s, e) =>
            {
                settings.SizeMode = rbFill.Checked ? ButtonSizeMode.Fill : ButtonSizeMode.Fixed;
                UpdateSizeControls();
                QueueRefresh();
            };
            nudWidth = new NumericUpDown { Minimum = 24, Maximum = 1200, Value = settings.Width, Width = 60 };
            nudWidth.ValueChanged += (s, e) => { settings.Width = (int)nudWidth.Value; QueueRefresh(); };
            nudHeight = new NumericUpDown { Minimum = 16, Maximum = 400, Value = settings.Height, Width = 55 };
            nudHeight.ValueChanged += (s, e) => { settings.Height = (int)nudHeight.Value; QueueRefresh(); };
            var sizePanel = Row(rbFill, rbFixed, nudWidth, Hint("x"), nudHeight, Hint("px"));

            cbPosition = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
            cbPosition.Items.AddRange(new object[] { "Left", "Centre", "Right" });
            cbPosition.SelectedIndex = (int)settings.Position;
            cbPosition.SelectedIndexChanged += (s, e) => { settings.Position = (ButtonPosition)cbPosition.SelectedIndex; QueueRefresh(); };

            AddRow(table, "Text:", txtText);
            AddRow(table, "Tooltip:", txtTooltip);
            AddRow(table, "Text colour:", cpText);
            AddRow(table, "Background:", cpBackground);
            AddRow(table, "Font size:", fontPanel);
            AddRow(table, "Icon:", cbIcon);
            AddRow(table, "Icon colour:", Row(cpIcon, chkIconFilled));
            AddRow(table, "Corners:", cornersPanel);
            AddRow(table, "Size:", sizePanel);
            AddRow(table, "Position:", cbPosition);
            group.Controls.Add(table);
            return group;
        }

        private GroupBox BuildActionGroup()
        {
            var group = new GroupBox { Text = "When clicked", Dock = DockStyle.Fill, Padding = new Padding(8) };

            cbAction = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
            cbAction.Items.AddRange(new object[] { "Do nothing (add your own script later)", "Run JavaScript", "Open a URL in a new tab", "Save the record" });
            cbAction.SelectedIndex = (int)settings.Action;
            cbAction.SelectedIndexChanged += (s, e) =>
            {
                settings.Action = (ButtonAction)cbAction.SelectedIndex;
                UpdateActionControls();
                QueueRefresh();
            };
            var actionRow = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 30, WrapContents = false };
            actionRow.Controls.Add(new Label { Text = "Action:", AutoSize = true, Margin = new Padding(0, 6, 6, 3) });
            actionRow.Controls.Add(cbAction);

            pnlUrl = new Panel { Dock = DockStyle.Top, Height = 30 };
            txtUrl = new TextBox { Text = settings.Url, Dock = DockStyle.Fill };
            txtUrl.TextChanged += (s, e) => { settings.Url = txtUrl.Text; QueueRefresh(); };
            pnlUrl.Controls.Add(txtUrl);
            pnlUrl.Controls.Add(new Label { Text = "URL:", Dock = DockStyle.Left, Width = 48, TextAlign = ContentAlignment.MiddleLeft });

            lblActionHint = new Label { Dock = DockStyle.Top, Height = 34, ForeColor = Color.DimGray };

            scriptEditor = new Scintilla
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                TabWidth = 2,
                UseTabs = false
            };
            CodeStyler.Apply(scriptEditor, WebResourceType.JavaScript);
            scriptEditor.Margins[0].Width = scriptEditor.TextWidth(Style.LineNumber, "999") + 4;
            scriptEditor.Text = settings.Script;
            scriptEditor.EmptyUndoBuffer();
            scriptEditor.TextChanged += (s, e) => { settings.Script = scriptEditor.Text; QueueRefresh(); };

            group.Controls.Add(scriptEditor);
            group.Controls.Add(lblActionHint);
            group.Controls.Add(pnlUrl);
            group.Controls.Add(actionRow);
            return group;
        }

        private GroupBox BuildPreviewGroup()
        {
            var previewGroup = new GroupBox { Text = "Preview", Dock = DockStyle.Fill, Padding = new Padding(8) };
            nudPreviewHeight = new NumericUpDown { Minimum = 24, Maximum = 200, Value = 50, Width = 60 };
            nudPreviewHeight.ValueChanged += (s, e) => LayoutPreview();
            var previewOptions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 30, WrapContents = false };
            previewOptions.Controls.Add(new Label { Text = "Control height:", AutoSize = true, Margin = new Padding(0, 6, 3, 3) });
            previewOptions.Controls.Add(nudPreviewHeight);
            previewOptions.Controls.Add(Hint("px  (on the form, the row count sets the height; clicking in the preview doesn't run the action)"));

            // Grey surround like a form section, so the control's edges are visible
            var host = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(0xF3, 0xF2, 0xF1), Padding = new Padding(12) };
            preview = new WebView2 { Dock = DockStyle.Top };
            lblPreviewUnavailable = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Visible = false };
            host.Controls.Add(lblPreviewUnavailable);
            host.Controls.Add(preview);

            previewGroup.Controls.Add(host);
            previewGroup.Controls.Add(previewOptions);
            return previewGroup;
        }

        private static TableLayoutPanel NewFormTable()
        {
            var table = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2 };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
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

        private static FlowLayoutPanel Row(params Control[] controls)
        {
            var row = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
            row.Controls.AddRange(controls);
            return row;
        }

        private static Label Hint(string text)
        {
            return new Label { Text = text, AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(0, 6, 3, 3) };
        }

        private void LayoutPreview()
        {
            preview.Height = (int)nudPreviewHeight.Value;

            // Room for the group caption, the height option and the grey surround
            layout.RowStyles[2].Height = preview.Height + 100;
        }

        private void UpdateSizeControls()
        {
            var isFixed = settings.SizeMode == ButtonSizeMode.Fixed;
            nudWidth.Enabled = isFixed;
            nudHeight.Enabled = isFixed;
            cbPosition.Enabled = isFixed;
        }

        private void UpdateActionControls()
        {
            pnlUrl.Visible = settings.Action == ButtonAction.OpenUrl;
            scriptEditor.Visible = settings.Action == ButtonAction.CustomScript;

            switch (settings.Action)
            {
                case ButtonAction.CustomScript:
                    lblActionHint.Text = "Runs in the button's frame. Xrm and formContext refer to the form, so you can read fields, save or navigate.";
                    break;
                case ButtonAction.OpenUrl:
                    lblActionHint.Text = "Opens the address in a new browser tab.";
                    break;
                case ButtonAction.SaveRecord:
                    lblActionHint.Text = "Saves the record the form is showing, the same as the Save command.";
                    break;
                default:
                    lblActionHint.Text = "The button has no click handler. Edit the HTML later or attach one from a form script.";
                    break;
            }
        }

        #endregion

        #region Loading and preview

        private async void ButtonDesignerPanel_Load(object sender, EventArgs e)
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
                // The action needs the form's Xrm, which the preview doesn't have
                preview.CoreWebView2.Settings.IsScriptEnabled = false;
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
            // Debounce so typing and dragging in the colour dialog don't reload on every keystroke
            refreshTimer.Stop();
            refreshTimer.Start();
        }

        private void RefreshPreview()
        {
            if (previewReady)
            {
                preview.CoreWebView2.NavigateToString(ButtonHtmlBuilder.Build(settings));
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
            var icon = IconGallery.FindByKey(e.TryGetWebMessageAsString());
            if (icon != null)
            {
                // Updates settings and the preview through the combo box handler
                cbIcon.SelectedItem = icon;
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
                    File.WriteAllText(dialog.FileName, ButtonHtmlBuilder.Build(settings), new UTF8Encoding(false));
                    lblStatus.Text = "Exported to " + Path.GetFileName(dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Could not export the button: " + ex.Message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            SetBusy(true);
            try
            {
                var outcome = await WebResourcePublisher.SaveAsync(this, Caption, service, details, WebResourceType.Html,
                    ButtonHtmlBuilder.Build(settings), "this button", status => lblStatus.Text = status);
                if (outcome == SaveOutcome.Cancelled)
                {
                    lblStatus.Text = string.Empty;
                    return;
                }

                var name = details.FullName;
                Saved?.Invoke(this, name);
                lblStatus.Text = "Saved and published " + name;
                MessageBox.Show(this,
                    string.Format("{0} was {1} and published{2}.\r\n\r\nAdd it to a form as a web resource; set the row count for the height and turn off \"Display label on the form\" and the border.",
                        name, outcome == SaveOutcome.Updated ? "updated" : "created", solution.IsDefault ? string.Empty : " in " + solution.FriendlyName),
                    Caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = string.Empty;
                MessageBox.Show(this, "Could not save the button: " + ex.Message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
