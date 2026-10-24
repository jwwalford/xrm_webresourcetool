using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using WebResourceManager.Banners;
using WebResourceManager.Models;
using WebResourceManager.Tools;

namespace WebResourceManager.Images
{
    /// <summary>
    /// Tool page for uploading an image file (PNG, JPG, GIF, BMP, ICO or SVG) as a
    /// web resource: web resource details and customisation on top (format,
    /// resize, JPG quality), a preview of the image that will be saved below.
    /// </summary>
    internal class ImageUploadPanel : UserControl
    {
        private const string Caption = "Upload an image";

        // Dataverse's default maximum file size; larger web resources are usually rejected
        private const long SizeWarningBytes = 5L * 1024 * 1024;

        private static readonly Regex InvalidNameChars = new Regex(@"[^A-Za-z0-9_\-\./]");

        private readonly string webViewDataFolder;
        private readonly Timer refreshTimer = new Timer { Interval = 250 };

        private Microsoft.Xrm.Sdk.IOrganizationService service;
        private bool loaded;
        private bool previewReady;
        private bool updatingSize;

        // The chosen file
        private byte[] sourceBytes;
        private string sourceFileName;
        private WebResourceType? sourceType;
        private Bitmap sourceImage;

        // What Save and Export write
        private byte[] outputBytes;
        private WebResourceType outputType;
        private string outputProblem;

        private WebResourceDetailsBox details;
        private Label lblSource;
        private ComboBox cbFormat;
        private RadioButton rbOriginalSize;
        private RadioButton rbResize;
        private NumericUpDown nudWidth;
        private NumericUpDown nudHeight;
        private CheckBox chkKeepRatio;
        private NumericUpDown nudQuality;
        private ColorPicker cpBackground;
        private Label lblOutput;
        private WebView2 preview;
        private Label lblPreviewMessage;
        private Button btnExport;
        private Button btnSave;
        private Label lblStatus;
        private SaveProgress saveProgress;

        public ImageUploadPanel(Microsoft.Xrm.Sdk.IOrganizationService service, string webViewDataFolder)
        {
            this.service = service;
            this.webViewDataFolder = webViewDataFolder;

            Font = new Font("Segoe UI", 9F);
            BackColor = SystemColors.Control;

            BuildLayout();
            UpdateControls();

            refreshTimer.Tick += (s, e) =>
            {
                refreshTimer.Stop();
                RefreshOutput();
            };

            Load += ImageUploadPanel_Load;
        }

        /// <summary>
        /// Raised after the image is saved to Dataverse, with the web resource name.
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
                sourceImage?.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool IsSvg
        {
            get { return sourceType == WebResourceType.Svg; }
        }

        #region Layout

        private void BuildLayout()
        {
            var header = ActionIcons.ToolHeader(Caption, "Choose an image file, adjust its format and size if you need to, and save it as a web resource. Export saves the converted file.", ActionIcons.ToolUpload);

            var divider = new Label { Dock = DockStyle.Top, Height = 1, BackColor = Color.LightGray };

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 42,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(8, 8, 8, 4)
            };
            btnSave = new Button { Text = "Save to Dataverse", Width = 130, Enabled = false };
            btnExport = new Button { Text = "Export...", Width = 90, Enabled = false };
            ActionIcons.Apply(btnSave, ActionIcons.Save);
            ActionIcons.Apply(btnExport, ActionIcons.Export);
            lblStatus = new Label { AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 8, 12, 3) };
            btnExport.Click += BtnExport_Click;
            btnSave.Click += BtnSave_Click;
            saveProgress = new SaveProgress(lblStatus);
            buttons.Controls.AddRange(new Control[] { btnSave, btnExport, saveProgress, lblStatus });

            // Details and customisation side by side, the preview takes the rest
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(8, 6, 8, 0) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 300));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            details = new WebResourceDetailsBox(service, "image", "images/my_image", "My image") { Extensions = new[] { ".png" } };
            details.StateChanged += (s, e) => UpdateButtons();

            var previewGroup = new GroupBox { Text = "Preview (actual size)", Dock = DockStyle.Fill, Padding = new Padding(8) };
            preview = new WebView2 { Dock = DockStyle.Fill, DefaultBackgroundColor = Color.White };
            lblPreviewMessage = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.DimGray,
                Text = "Choose an image to see it here."
            };
            previewGroup.Controls.Add(preview);
            previewGroup.Controls.Add(lblPreviewMessage);

            layout.Controls.Add(details, 0, 0);
            layout.Controls.Add(BuildCustomiseGroup(), 1, 0);
            layout.Controls.Add(previewGroup, 0, 1);
            layout.SetColumnSpan(previewGroup, 2);

            Controls.Add(layout);
            Controls.Add(buttons);
            Controls.Add(divider);
            Controls.Add(header);
        }

        private GroupBox BuildCustomiseGroup()
        {
            var group = new GroupBox { Text = "Image", Dock = DockStyle.Fill, Padding = new Padding(8) };
            var table = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2 };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var btnChoose = new Button { Text = "Choose image...", Width = 120, Margin = new Padding(3, 3, 3, 3) };
            btnChoose.Click += BtnChoose_Click;
            lblSource = new Label { AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 3, 3, 6), MaximumSize = new Size(360, 0), Text = "PNG, JPG, GIF, BMP, ICO or SVG." };
            var choosePanel = new FlowLayoutPanel { AutoSize = true, WrapContents = false, FlowDirection = FlowDirection.TopDown, Margin = new Padding(0) };
            choosePanel.Controls.Add(btnChoose);
            choosePanel.Controls.Add(lblSource);

            cbFormat = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
            cbFormat.SelectedIndexChanged += (s, e) => { UpdateControls(); QueueRefresh(); };

            rbOriginalSize = new RadioButton { Text = "Original", AutoSize = true, Checked = true, Margin = new Padding(3, 5, 8, 3) };
            rbResize = new RadioButton { Text = "Resize to:", AutoSize = true, Margin = new Padding(3, 5, 3, 3) };
            rbOriginalSize.CheckedChanged += (s, e) => { UpdateControls(); QueueRefresh(); };
            nudWidth = new NumericUpDown { Minimum = 1, Maximum = 8000, Value = 100, Width = 60 };
            nudHeight = new NumericUpDown { Minimum = 1, Maximum = 8000, Value = 100, Width = 60 };
            nudWidth.ValueChanged += (s, e) => KeepRatio(true);
            nudHeight.ValueChanged += (s, e) => KeepRatio(false);
            chkKeepRatio = new CheckBox { Text = "Keep proportions", AutoSize = true, Checked = true, Margin = new Padding(3, 5, 3, 3) };
            chkKeepRatio.CheckedChanged += (s, e) => KeepRatio(true);

            nudQuality = new NumericUpDown { Minimum = 1, Maximum = 100, Value = 90, Width = 60 };
            nudQuality.ValueChanged += (s, e) => QueueRefresh();
            cpBackground = new ColorPicker(Color.White);
            cpBackground.ColorChanged += (s, e) => QueueRefresh();

            lblOutput = new Label { AutoSize = true, Margin = new Padding(3, 7, 3, 3), MaximumSize = new Size(360, 0), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };

            AddRow(table, "File:", choosePanel);
            AddRow(table, "Save as:", cbFormat);
            AddRow(table, "Size:", Row(rbOriginalSize, rbResize, nudWidth, Hint("x"), nudHeight, Hint("px")));
            AddRow(table, string.Empty, chkKeepRatio);
            AddRow(table, "JPG quality:", Row(nudQuality, Hint("%   Fill for transparent areas:"), cpBackground));
            AddRow(table, "Result:", lblOutput);

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

        #endregion

        #region Choosing and customising

        private void BtnChoose_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { Filter = ImageFileConverter.OpenFilter, Title = "Choose an image" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                var extension = Path.GetExtension(dialog.FileName);
                var type = ImageFileConverter.TypeForExtension(extension);
                byte[] bytes;
                Bitmap image = null;
                try
                {
                    bytes = File.ReadAllBytes(dialog.FileName);
                    if (type != WebResourceType.Svg)
                    {
                        image = IconResizer.Load(dialog.FileName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Could not open the image: " + ex.Message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                sourceImage?.Dispose();
                sourceImage = image;
                sourceBytes = bytes;
                sourceType = type;
                sourceFileName = Path.GetFileName(dialog.FileName);

                lblSource.Text = image != null
                    ? string.Format("{0} ({1} x {2}, {3})", sourceFileName, image.Width, image.Height, ImageFileConverter.FormatSize(bytes.Length))
                    : string.Format("{0} ({1})", sourceFileName, ImageFileConverter.FormatSize(bytes.Length));

                // Name the web resource after the file
                var baseName = InvalidNameChars.Replace(Path.GetFileNameWithoutExtension(dialog.FileName), "_");
                details.SetName("images/" + baseName, Path.GetFileNameWithoutExtension(dialog.FileName));

                FillFormats();

                updatingSize = true;
                if (image != null)
                {
                    nudWidth.Value = Math.Min(nudWidth.Maximum, image.Width);
                    nudHeight.Value = Math.Min(nudHeight.Maximum, image.Height);
                }
                updatingSize = false;
                rbOriginalSize.Checked = true;

                UpdateControls();
                RefreshOutput();
            }
        }

        private void FillFormats()
        {
            cbFormat.Items.Clear();
            if (IsSvg)
            {
                cbFormat.Items.Add(new FormatChoice("SVG (kept as it is)", WebResourceType.Svg, true));
            }
            else
            {
                if (sourceType.HasValue)
                {
                    cbFormat.Items.Add(new FormatChoice(string.Format("Keep {0}", sourceType.Value.ToString().ToUpperInvariant()), sourceType.Value, true));
                }

                // BMP isn't a web resource type, so it has to be converted
                foreach (var type in new[] { WebResourceType.Png, WebResourceType.Jpg, WebResourceType.Gif })
                {
                    if (type != sourceType)
                    {
                        cbFormat.Items.Add(new FormatChoice("Convert to " + type.ToString().ToUpperInvariant(), type, false));
                    }
                }
            }

            cbFormat.SelectedIndex = 0;
        }

        private void KeepRatio(bool widthChanged)
        {
            if (updatingSize || sourceImage == null)
            {
                return;
            }

            if (chkKeepRatio.Checked)
            {
                updatingSize = true;
                if (widthChanged)
                {
                    nudHeight.Value = Clamp(nudHeight, Math.Round(nudWidth.Value * sourceImage.Height / sourceImage.Width));
                }
                else
                {
                    nudWidth.Value = Clamp(nudWidth, Math.Round(nudHeight.Value * sourceImage.Width / sourceImage.Height));
                }
                updatingSize = false;
            }

            QueueRefresh();
        }

        private static decimal Clamp(NumericUpDown control, decimal value)
        {
            return Math.Max(control.Minimum, Math.Min(control.Maximum, value));
        }

        private void UpdateControls()
        {
            var raster = sourceImage != null;
            var format = cbFormat.SelectedItem as FormatChoice;
            cbFormat.Enabled = raster;
            rbOriginalSize.Enabled = raster;
            rbResize.Enabled = raster;
            nudWidth.Enabled = raster && rbResize.Checked;
            nudHeight.Enabled = raster && rbResize.Checked;
            chkKeepRatio.Enabled = raster && rbResize.Checked;

            var jpg = raster && format != null && format.Type == WebResourceType.Jpg;
            nudQuality.Enabled = jpg;
            cpBackground.Enabled = jpg;

            if (format != null)
            {
                details.Extensions = format.Type == WebResourceType.Jpg ? new[] { ".jpg", ".jpeg" } : new[] { ImageFileConverter.ExtensionFor(format.Type) };
            }
        }

        private void QueueRefresh()
        {
            refreshTimer.Stop();
            refreshTimer.Start();
        }

        /// <summary>
        /// Works out the bytes to save. Unchanged images keep the original file
        /// byte for byte (so GIF animation and JPG quality survive).
        /// </summary>
        private void RefreshOutput()
        {
            outputBytes = null;
            outputProblem = null;
            var format = cbFormat.SelectedItem as FormatChoice;
            if (sourceBytes == null || format == null)
            {
                UpdateButtons();
                return;
            }

            outputType = format.Type;
            var resize = rbResize.Checked && sourceImage != null &&
                (nudWidth.Value != sourceImage.Width || nudHeight.Value != sourceImage.Height);
            try
            {
                if (format.IsOriginal && !resize)
                {
                    outputBytes = sourceBytes;
                }
                else if (outputType == WebResourceType.Ico && (nudWidth.Value > 256 || nudHeight.Value > 256 || !resize && (sourceImage.Width > 256 || sourceImage.Height > 256)))
                {
                    outputProblem = "ICO images can be at most 256 x 256. Resize the image or convert it to PNG.";
                }
                else if (resize)
                {
                    using (var resized = ImageFileConverter.Resize(sourceImage, (int)nudWidth.Value, (int)nudHeight.Value))
                    {
                        outputBytes = ImageFileConverter.Encode(resized, outputType, (int)nudQuality.Value, cpBackground.Color);
                    }
                }
                else
                {
                    outputBytes = ImageFileConverter.Encode(sourceImage, outputType, (int)nudQuality.Value, cpBackground.Color);
                }
            }
            catch (Exception ex)
            {
                outputProblem = "Could not convert the image: " + ex.Message;
            }

            if (outputBytes != null)
            {
                var size = sourceImage == null ? string.Empty
                    : resize ? string.Format("{0} x {1}, ", nudWidth.Value, nudHeight.Value)
                    : string.Format("{0} x {1}, ", sourceImage.Width, sourceImage.Height);
                lblOutput.ForeColor = outputBytes.Length > SizeWarningBytes ? Color.Firebrick : SystemColors.ControlText;
                lblOutput.Text = string.Format("{0}, {1}{2}{3}", outputType.ToString().ToUpperInvariant(), size, ImageFileConverter.FormatSize(outputBytes.Length),
                    outputBytes.Length > SizeWarningBytes ? ". Larger than Dataverse's default 5 MB limit, so saving may fail." : string.Empty);
            }
            else
            {
                lblOutput.ForeColor = Color.Firebrick;
                lblOutput.Text = outputProblem;
            }

            UpdateButtons();
            ShowPreview();
        }

        #endregion

        #region Preview

        private async void ImageUploadPanel_Load(object sender, EventArgs e)
        {
            // Load can fire again if the handle is re-created; set up only once
            if (loaded)
            {
                return;
            }

            loaded = true;
            preview.Visible = false;
            try
            {
                var environment = await CoreWebView2Environment.CreateAsync(null, webViewDataFolder);
                await preview.EnsureCoreWebView2Async(environment);
                preview.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                preview.CoreWebView2.Profile.PreferredColorScheme = CoreWebView2PreferredColorScheme.Light;
                previewReady = true;
                ShowPreview();
            }
            catch (Exception ex)
            {
                lblPreviewMessage.Text = "The preview needs the Microsoft Edge WebView2 runtime.\r\n" + ex.Message;
            }
        }

        private void ShowPreview()
        {
            if (!previewReady || outputBytes == null)
            {
                preview.Visible = false;
                lblPreviewMessage.Visible = true;
                if (sourceBytes != null && outputProblem != null)
                {
                    lblPreviewMessage.Text = outputProblem;
                }
                return;
            }

            // Images can be larger than NavigateToString allows, so the page and image go through temp files
            var folder = Path.Combine(Path.GetTempPath(), "XrmToolBox.WebResourceManager", "upload-preview");
            Directory.CreateDirectory(folder);
            var imageFile = "preview-" + Guid.NewGuid().ToString("N") + ImageFileConverter.ExtensionFor(outputType);
            foreach (var old in Directory.GetFiles(folder))
            {
                try { File.Delete(old); } catch (IOException) { } catch (UnauthorizedAccessException) { }
            }
            File.WriteAllBytes(Path.Combine(folder, imageFile), outputBytes);

            var html = new StringBuilder();
            html.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><style>");
            html.Append("body{margin:0;padding:12px;font-family:'Segoe UI',sans-serif;font-size:11px;color:#605e5c;background:#fff}");
            html.Append(".row{display:flex;gap:12px;align-items:flex-start;flex-wrap:wrap}");
            html.Append(".tile{padding:12px;border:1px solid #e1dfdd;border-radius:4px;overflow:auto;max-width:100%;box-sizing:border-box}");
            html.Append(".check{background-color:#fff;background-image:linear-gradient(45deg,#e1dfdd 25%,transparent 25%),linear-gradient(-45deg,#e1dfdd 25%,transparent 25%),linear-gradient(45deg,transparent 75%,#e1dfdd 75%),linear-gradient(-45deg,transparent 75%,#e1dfdd 75%);background-size:16px 16px;background-position:0 0,0 8px,8px -8px,-8px 0}");
            html.Append(".dark{background:#202020;color:#c8c6c4}img{display:block;image-rendering:auto}h4{margin:0 0 6px;font-size:11px;font-weight:600;text-transform:uppercase;letter-spacing:.04em}");
            html.Append("</style></head><body><div class=\"row\">");
            foreach (var tile in new[] { "check|On transparent", "dark|On dark" })
            {
                var parts = tile.Split('|');
                html.AppendFormat("<div class=\"tile {0}\"><h4>{1}</h4><img src=\"{2}\" alt=\"\"></div>", parts[0], parts[1], WebUtility.HtmlEncode(imageFile));
            }
            html.Append("</div></body></html>");

            var page = Path.Combine(folder, "preview.html");
            File.WriteAllText(page, html.ToString(), new UTF8Encoding(false));

            lblPreviewMessage.Visible = false;
            preview.Visible = true;
            preview.CoreWebView2.Navigate(new Uri(page).AbsoluteUri);
        }

        #endregion

        #region Save and export

        private void UpdateButtons()
        {
            var ready = outputBytes != null;
            btnExport.Enabled = ready;
            btnSave.Enabled = ready && details.CanSave;
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (outputBytes == null || !details.ValidateName(Caption))
            {
                return;
            }

            var extension = ImageFileConverter.ExtensionFor(outputType);
            using (var dialog = new SaveFileDialog
            {
                FileName = details.FullName.Split('/').Last(),
                Filter = string.Format("{0} files (*{1})|*{1}|All files (*.*)|*.*", outputType.ToString().ToUpperInvariant(), extension),
                DefaultExt = extension.TrimStart('.')
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    File.WriteAllBytes(dialog.FileName, outputBytes);
                    lblStatus.Text = "Exported to " + Path.GetFileName(dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Could not export the image: " + ex.Message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (outputBytes == null || !details.EnsureConnected() || !details.CanSave || !details.ValidateName(Caption))
            {
                return;
            }

            var solution = details.SelectedSolution;
            var bytes = outputBytes;
            SetBusy(true);
            try
            {
                var outcome = await WebResourcePublisher.SaveBytesAsync(this, Caption, service, details, outputType, bytes,
                    "this image", saveProgress.Report);
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
                MessageBox.Show(this, "Could not save the image: " + ex.Message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void SetBusy(bool busy)
        {
            UseWaitCursor = busy;
            btnExport.Enabled = !busy && outputBytes != null;
            btnSave.Enabled = !busy && outputBytes != null && details.CanSave;
        }

        #endregion

        private class FormatChoice
        {
            public FormatChoice(string label, WebResourceType type, bool isOriginal)
            {
                Label = label;
                Type = type;
                IsOriginal = isOriginal;
            }

            public string Label { get; }

            public WebResourceType Type { get; }

            /// <summary>
            /// True for the file's own format, which is saved unchanged unless resized.
            /// </summary>
            public bool IsOriginal { get; }

            public override string ToString()
            {
                return Label;
            }
        }
    }
}
