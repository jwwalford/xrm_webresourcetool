using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using WebResourceManager.Models;
using WebResourceManager.Services;

namespace WebResourceManager.Banners
{
    /// <summary>
    /// Two step wizard that designs a form banner and saves it as an HTML web
    /// resource (or exports it to disk). Step 1 names the web resource, step 2
    /// designs the banner with a live preview.
    /// </summary>
    public class BannerWizardForm : Form
    {
        // Web resource names allow letters, digits, underscore, hyphen, dot and forward slash
        private static readonly Regex ValidName = new Regex(@"^[A-Za-z0-9_\-\./]+$");

        private readonly Microsoft.Xrm.Sdk.IOrganizationService service;
        private readonly string webViewDataFolder;
        private readonly BannerSettings settings = new BannerSettings();
        private readonly Timer refreshTimer = new Timer { Interval = 150 };

        private int step;
        private bool previewReady;
        private bool galleryReady;
        private string lastGalleryHtml;

        // Shell
        private Label lblStepTitle;
        private Label lblStepHint;
        private Panel pnlDetails;
        private Panel pnlDesign;
        private Button btnBack;
        private Button btnNext;
        private Button btnExport;
        private Button btnSave;
        private Button btnCancel;
        private Label lblStatus;

        // Step 1
        private ComboBox cbSolution;
        private Label lblSolutionStatus;
        private Label lblPrefix;
        private TextBox txtName;
        private Label lblFullName;
        private TextBox txtDisplayName;
        private TextBox txtDescription;

        // Step 2
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

        public BannerWizardForm(Microsoft.Xrm.Sdk.IOrganizationService service, string webViewDataFolder)
        {
            this.service = service;
            this.webViewDataFolder = webViewDataFolder;

            Text = "New form banner";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(920, 680);
            MinimumSize = new Size(760, 560);
            Font = new Font("Segoe UI", 9F);
            ShowIcon = false;
            ShowInTaskbar = false;
            MinimizeBox = false;

            BuildShell();
            BuildDetailsStep();
            BuildDesignStep();

            refreshTimer.Tick += (s, e) =>
            {
                refreshTimer.Stop();
                RefreshPreview();
            };

            Load += BannerWizardForm_Load;
            FormClosed += (s, e) => refreshTimer.Dispose();

            ShowStep(0);
        }

        #region Layout

        private void BuildShell()
        {
            var header = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Color.White, Padding = new Padding(16, 10, 16, 6) };
            lblStepTitle = new Label { Dock = DockStyle.Top, Height = 24, Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
            lblStepHint = new Label { Dock = DockStyle.Top, Height = 22, ForeColor = Color.DimGray };
            header.Controls.Add(lblStepHint);
            header.Controls.Add(lblStepTitle);

            var divider = new Label { Dock = DockStyle.Top, Height = 1, BackColor = Color.LightGray };

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 48,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(10, 10, 10, 6)
            };

            btnCancel = new Button { Text = "Cancel", Width = 90, DialogResult = DialogResult.Cancel };
            btnSave = new Button { Text = "Save to Dataverse", Width = 130 };
            btnExport = new Button { Text = "Export HTML...", Width = 110 };
            btnNext = new Button { Text = "Next >", Width = 90 };
            btnBack = new Button { Text = "< Back", Width = 90 };
            lblStatus = new Label { AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 8, 12, 3) };

            btnNext.Click += BtnNext_Click;
            btnBack.Click += (s, e) => ShowStep(0);
            btnExport.Click += BtnExport_Click;
            btnSave.Click += BtnSave_Click;

            buttons.Controls.AddRange(new Control[] { btnCancel, btnSave, btnExport, btnNext, btnBack, lblStatus });
            CancelButton = btnCancel;

            pnlDetails = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };
            pnlDesign = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 8, 12, 4) };

            Controls.Add(pnlDesign);
            Controls.Add(pnlDetails);
            Controls.Add(buttons);
            Controls.Add(divider);
            Controls.Add(header);
        }

        private void BuildDetailsStep()
        {
            var table = NewFormTable();

            cbSolution = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 420 };
            cbSolution.SelectedIndexChanged += (s, e) => UpdateFullName();
            lblSolutionStatus = new Label { AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 0, 3, 8) };

            lblPrefix = new Label { AutoSize = true, Margin = new Padding(3, 6, 0, 3), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtName = new TextBox { Width = 300 };
            txtName.TextChanged += (s, e) => UpdateFullName();
            var namePanel = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
            namePanel.Controls.Add(lblPrefix);
            namePanel.Controls.Add(txtName);
            lblFullName = new Label { AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 0, 3, 8) };

            txtDisplayName = new TextBox { Width = 420 };
            txtDescription = new TextBox { Width = 420, Height = 70, Multiline = true, ScrollBars = ScrollBars.Vertical };

            AddRow(table, "Solution:", cbSolution);
            AddRow(table, string.Empty, lblSolutionStatus);
            AddRow(table, "Name:", namePanel);
            AddRow(table, string.Empty, lblFullName);
            AddRow(table, "Display name:", txtDisplayName);
            AddRow(table, "Description:", txtDescription);

            txtName.Text = "banners/form_banner";
            txtDisplayName.Text = "Form banner";

            pnlDetails.Controls.Add(table);
        }

        private void BuildDesignStep()
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 420));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 170));

            // Settings
            var settingsGroup = new GroupBox { Text = "Banner", Dock = DockStyle.Fill, Padding = new Padding(8) };
            var table = NewFormTable();

            txtText = new TextBox { Width = 260, Text = settings.Text };
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
            settingsGroup.Controls.Add(table);

            // Icon gallery
            var iconsGroup = new GroupBox { Text = "Icons (click to choose)", Dock = DockStyle.Fill, Padding = new Padding(8) };
            gallery = new WebView2 { Dock = DockStyle.Fill };
            iconsGroup.Controls.Add(gallery);

            // Preview
            var previewGroup = new GroupBox { Text = "Preview", Dock = DockStyle.Fill, Padding = new Padding(8) };
            var previewOptions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 30, WrapContents = false };
            nudPreviewHeight = new NumericUpDown { Minimum = 24, Maximum = 200, Value = 50, Width = 60 };
            nudPreviewHeight.ValueChanged += (s, e) => LayoutPreview();
            previewOptions.Controls.Add(new Label { Text = "Preview height:", AutoSize = true, Margin = new Padding(0, 6, 3, 3) });
            previewOptions.Controls.Add(nudPreviewHeight);
            previewOptions.Controls.Add(new Label
            {
                Text = "px  (on the form, the banner fills the height set by the web resource's row count)",
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

            layout.Controls.Add(settingsGroup, 0, 0);
            layout.Controls.Add(iconsGroup, 1, 0);
            layout.Controls.Add(previewGroup, 0, 1);
            layout.SetColumnSpan(previewGroup, 2);

            pnlDesign.Controls.Add(layout);
            LayoutPreview();
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
        }

        #endregion

        #region Steps

        private void ShowStep(int index)
        {
            step = index;
            pnlDetails.Visible = index == 0;
            pnlDesign.Visible = index == 1;
            btnBack.Visible = index == 1;
            btnNext.Visible = index == 0;
            btnExport.Visible = index == 1;
            btnSave.Visible = index == 1;
            btnCancel.Text = index == 1 ? "Close" : "Cancel";
            lblStatus.Text = string.Empty;

            if (index == 0)
            {
                lblStepTitle.Text = "Step 1 of 2: Name your banner";
                lblStepHint.Text = "Choose where the HTML web resource will live and what it's called.";
                AcceptButton = btnNext;
            }
            else
            {
                lblStepTitle.Text = "Step 2 of 2: Design your banner";
                lblStepHint.Text = "Pick colours, text and an icon. Save creates the web resource in Dataverse; Export saves the HTML file.";
                AcceptButton = null;
                QueueRefresh();
            }

            UpdateSaveState();
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            var name = txtName.Text.Trim();
            if (name.Length == 0)
            {
                ShowValidation("Enter a name for the web resource.", txtName);
                return;
            }

            if (!ValidName.IsMatch(name))
            {
                ShowValidation("The name can only contain letters, numbers, underscores, hyphens, dots and forward slashes.", txtName);
                return;
            }

            if (txtDisplayName.Text.Trim().Length == 0)
            {
                txtDisplayName.Text = name.Split('/').Last();
            }

            settings.Title = txtDisplayName.Text.Trim();
            ShowStep(1);
        }

        private void ShowValidation(string message, Control focus)
        {
            MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            focus.Focus();
        }

        private SolutionOption SelectedSolution
        {
            get { return cbSolution.SelectedItem as SolutionOption; }
        }

        /// <summary>
        /// The full web resource name: publisher prefix + name + .html
        /// </summary>
        private string FullName
        {
            get
            {
                var name = txtName.Text.Trim();
                var solution = SelectedSolution;
                if (solution != null && !string.IsNullOrEmpty(solution.Prefix) &&
                    !name.StartsWith(solution.Prefix + "_", StringComparison.OrdinalIgnoreCase))
                {
                    name = solution.Prefix + "_" + name;
                }

                if (!name.EndsWith(".html", StringComparison.OrdinalIgnoreCase) && !name.EndsWith(".htm", StringComparison.OrdinalIgnoreCase))
                {
                    name += ".html";
                }

                return name;
            }
        }

        private void UpdateFullName()
        {
            var solution = SelectedSolution;
            lblPrefix.Text = solution != null && !string.IsNullOrEmpty(solution.Prefix) ? solution.Prefix + "_" : string.Empty;
            lblFullName.Text = txtName.Text.Trim().Length > 0 ? "Web resource name: " + FullName : string.Empty;
        }

        private void UpdateSaveState()
        {
            btnSave.Enabled = service != null && SelectedSolution != null;
        }

        #endregion

        #region Loading

        private async void BannerWizardForm_Load(object sender, EventArgs e)
        {
            InitializeWebViews();

            if (service == null)
            {
                lblSolutionStatus.Text = "Not connected to Dataverse. You can still design the banner and export it.";
                cbSolution.Enabled = false;
                UpdateFullName();
                return;
            }

            lblSolutionStatus.Text = "Loading solutions...";
            cbSolution.Enabled = false;
            try
            {
                var solutions = await Task.Run(() => new WebResourceRepository(service).GetUnmanagedSolutions());
                cbSolution.Items.AddRange(solutions.Cast<object>().ToArray());
                if (cbSolution.Items.Count > 0)
                {
                    cbSolution.SelectedIndex = 0;
                }

                cbSolution.Enabled = true;
                lblSolutionStatus.Text = "The publisher prefix comes from the solution. The Default solution doesn't add the banner to a solution.";
            }
            catch (Exception ex)
            {
                lblSolutionStatus.Text = "Could not load solutions: " + ex.Message;
            }

            UpdateFullName();
            UpdateSaveState();
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
                var html = BuildGalleryHtml();
                if (html != lastGalleryHtml)
                {
                    lastGalleryHtml = html;
                    gallery.CoreWebView2.NavigateToString(html);
                }
            }
        }

        /// <summary>
        /// A grid of icon buttons drawn in the current colours. Clicking one posts
        /// its key back through the WebView2 message channel.
        /// </summary>
        private string BuildGalleryHtml()
        {
            var html = new StringBuilder();
            html.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><style>");
            html.Append("body{margin:0;padding:6px;font-family:'Segoe UI',sans-serif;font-size:11px;background:#fff}");
            html.Append(".grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(76px,1fr));gap:6px}");
            html.AppendFormat("button{{display:flex;flex-direction:column;align-items:center;gap:4px;padding:8px 4px;border:2px solid transparent;border-radius:6px;background:{0};color:{1};cursor:pointer;font:inherit}}",
                BannerHtmlBuilder.ToHex(settings.BackgroundColor), BannerHtmlBuilder.ToHex(settings.TextColor));
            html.Append("button:hover{border-color:#8a8886}button.selected{border-color:#0078d4;box-shadow:0 0 0 2px #c7e0f4}");
            html.AppendFormat("button svg{{color:{0}}}", BannerHtmlBuilder.ToHex(settings.IconColor));
            html.Append(".none{width:28px;height:28px;display:flex;align-items:center;justify-content:center;font-size:18px;opacity:.6}");
            html.Append("</style></head><body><div class=\"grid\">");

            foreach (var icon in BannerIcon.All)
            {
                var selected = settings.Icon == icon || (settings.Icon == null && icon == BannerIcon.None);
                html.AppendFormat("<button type=\"button\"{0} title=\"{1}\" onclick=\"window.chrome.webview.postMessage('{2}')\">",
                    selected ? " class=\"selected\"" : string.Empty, WebUtility.HtmlEncode(icon.Label), icon.Key);
                html.Append(string.IsNullOrEmpty(icon.Markup)
                    ? "<span class=\"none\">&#8856;</span>"
                    : BannerHtmlBuilder.BuildSvg(icon, settings.IconFilled, 28));
                html.AppendFormat("<span>{0}</span></button>", WebUtility.HtmlEncode(icon == BannerIcon.None ? "None" : icon.Label));
            }

            html.Append("</div></body></html>");
            return html.ToString();
        }

        private void Gallery_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var key = e.TryGetWebMessageAsString();
            var icon = BannerIcon.All.FirstOrDefault(i => i.Key == key);
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
            var fileName = FullName.Split('/').Last();
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
                    MessageBox.Show(this, "Could not export the banner: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            var solution = SelectedSolution;
            if (service == null || solution == null)
            {
                return;
            }

            var name = FullName;
            var displayName = txtDisplayName.Text.Trim();
            var description = txtDescription.Text.Trim();
            var html = BannerHtmlBuilder.Build(settings);
            var repository = new WebResourceRepository(service);

            SetBusy(true, "Checking for an existing web resource...");
            try
            {
                var existing = await Task.Run(() => repository.FindByName(name));
                if (existing != null)
                {
                    if (existing.Type != WebResourceType.Html)
                    {
                        MessageBox.Show(this, string.Format("A {0} web resource named {1} already exists. Choose another name.", existing.TypeLabel, name),
                            Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        lblStatus.Text = string.Empty;
                        return;
                    }

                    var prompt = string.Format("A web resource named {0} already exists{1}.\r\n\r\nReplace its content with this banner?",
                        name, existing.IsManaged ? " and is managed, so this adds an unmanaged customisation on top" : string.Empty);
                    if (MessageBox.Show(this, prompt, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    {
                        lblStatus.Text = string.Empty;
                        return;
                    }
                }

                SetBusy(true, "Saving and publishing...");
                var existingId = existing != null ? existing.Id : (Guid?)null;
                await Task.Run(() => repository.SaveHtml(existingId, name, displayName, description, html, solution.UniqueName));

                SavedName = name;
                lblStatus.Text = "Saved and published " + name;
                MessageBox.Show(this,
                    string.Format("{0} was {1} and published{2}.\r\n\r\nAdd it to a form as a web resource; set the row count to control the banner height and turn off \"Display label on the form\" and the border.",
                        name, existing != null ? "updated" : "created", solution.IsDefault ? string.Empty : " in " + solution.FriendlyName),
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = string.Empty;
                MessageBox.Show(this, "Could not save the banner: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false, null);
            }
        }

        /// <summary>
        /// Name of the last web resource saved to Dataverse, or null if nothing was saved.
        /// </summary>
        public string SavedName { get; private set; }

        private void SetBusy(bool busy, string message)
        {
            UseWaitCursor = busy;
            btnSave.Enabled = !busy;
            btnExport.Enabled = !busy;
            btnBack.Enabled = !busy;
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
