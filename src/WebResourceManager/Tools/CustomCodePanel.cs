using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using ScintillaNET;
using WebResourceManager.Models;

namespace WebResourceManager.Tools
{
    /// <summary>
    /// Tool page for writing an HTML or CSS web resource by hand: web resource
    /// details top left, live preview top right, code editor below.
    /// </summary>
    internal class CustomCodePanel : UserControl
    {
        private const string Caption = "Create custom HTML/CSS";

        private const string HtmlStarter =
@"<!DOCTYPE html>
<html>
<head>
  <meta charset=""utf-8"">
  <title>My web resource</title>
  <style>
    body { margin: 0; padding: 16px; font-family: 'Segoe UI', sans-serif; }
  </style>
</head>
<body>
  <h1>Hello</h1>
  <p>Start typing your HTML here.</p>
</body>
</html>
";

        private const string CssStarter =
@"/* The preview applies this stylesheet to a sample page */
body {
  font-family: 'Segoe UI', sans-serif;
  color: #323130;
}

h1 {
  color: #0078d4;
}
";

        private readonly Timer refreshTimer = new Timer { Interval = 300 };
        private readonly string webViewDataFolder;

        private Microsoft.Xrm.Sdk.IOrganizationService service;
        private WebResourceType type = WebResourceType.Html;
        private bool loaded;
        private bool previewReady;

        private WebResourceDetailsBox details;
        private RadioButton rbHtml;
        private RadioButton rbCss;
        private WebView2 preview;
        private Label lblPreviewUnavailable;
        private Scintilla editor;
        private Button btnExport;
        private Button btnSave;
        private Label lblStatus;
        private SaveProgress saveProgress;

        public CustomCodePanel(Microsoft.Xrm.Sdk.IOrganizationService service, string webViewDataFolder)
        {
            this.service = service;
            this.webViewDataFolder = webViewDataFolder;

            Font = new Font("Segoe UI", 9F);
            BackColor = SystemColors.Control;

            BuildLayout();
            SetEditorText(HtmlStarter);

            refreshTimer.Tick += (s, e) =>
            {
                refreshTimer.Stop();
                RefreshPreview();
            };

            Load += CustomCodePanel_Load;
        }

        /// <summary>
        /// Raised after the web resource is saved to Dataverse, with its name.
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
            var header = ActionIcons.ToolHeader(Caption, "Write HTML or CSS and watch the preview update. Save creates the web resource in Dataverse; Export saves the file.", ActionIcons.ToolCode);

            var divider = new Label { Dock = DockStyle.Top, Height = 1, BackColor = Color.LightGray };

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 42,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(8, 8, 8, 4)
            };
            btnSave = new Button { Text = "Save to Dataverse", Width = 130, Enabled = false };
            btnExport = new Button { Text = "Export...", Width = 90 };
            ActionIcons.Apply(btnSave, ActionIcons.Save);
            ActionIcons.Apply(btnExport, ActionIcons.Export);
            lblStatus = new Label { AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 8, 12, 3) };
            btnExport.Click += BtnExport_Click;
            btnSave.Click += BtnSave_Click;
            saveProgress = new SaveProgress(lblStatus);
            buttons.Controls.AddRange(new Control[] { btnSave, btnExport, saveProgress, lblStatus });

            // Details and preview side by side, the editor takes the rest
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(8, 6, 8, 0) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 300));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            details = new WebResourceDetailsBox(service, "web resource", "custom/my_page", "My page");
            details.StateChanged += (s, e) => UpdateSaveState();

            layout.Controls.Add(details, 0, 0);
            layout.Controls.Add(BuildPreviewGroup(), 1, 0);
            var editorGroup = BuildEditorGroup();
            layout.Controls.Add(editorGroup, 0, 1);
            layout.SetColumnSpan(editorGroup, 2);

            Controls.Add(layout);
            Controls.Add(buttons);
            Controls.Add(divider);
            Controls.Add(header);
        }

        private GroupBox BuildPreviewGroup()
        {
            var group = new GroupBox { Text = "Preview", Dock = DockStyle.Fill, Padding = new Padding(8) };
            // White behind the page, so HTML without its own background previews as it would on a form
            preview = new WebView2 { Dock = DockStyle.Fill, DefaultBackgroundColor = Color.White };
            lblPreviewUnavailable = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Visible = false };
            group.Controls.Add(preview);
            group.Controls.Add(lblPreviewUnavailable);
            return group;
        }

        private GroupBox BuildEditorGroup()
        {
            var group = new GroupBox { Text = "Code", Dock = DockStyle.Fill, Padding = new Padding(8) };

            var typeBar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 28, WrapContents = false };
            rbHtml = new RadioButton { Text = "HTML", AutoSize = true, Checked = true, Margin = new Padding(3, 4, 12, 3) };
            rbCss = new RadioButton { Text = "CSS", AutoSize = true, Margin = new Padding(3, 4, 12, 3) };
            rbHtml.CheckedChanged += (s, e) => { if (rbHtml.Checked) SetType(WebResourceType.Html); };
            rbCss.CheckedChanged += (s, e) => { if (rbCss.Checked) SetType(WebResourceType.Css); };
            typeBar.Controls.Add(new Label { Text = "Type:", AutoSize = true, Margin = new Padding(0, 6, 6, 3) });
            typeBar.Controls.Add(rbHtml);
            typeBar.Controls.Add(rbCss);

            editor = new Scintilla
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                TabWidth = 2,
                UseTabs = false,
                IndentationGuides = IndentView.LookBoth
            };
            editor.TextChanged += (s, e) => QueueRefresh();
            editor.CharAdded += Editor_CharAdded;
            CodeStyler.Apply(editor, type);

            group.Controls.Add(editor);
            group.Controls.Add(typeBar);
            return group;
        }

        #endregion

        #region Editor

        private void SetType(WebResourceType newType)
        {
            if (newType == type)
            {
                return;
            }

            // Swap the starter text if it hasn't been touched, so CSS doesn't start with an HTML page
            var untouched = editor.Text == HtmlStarter || editor.Text == CssStarter || editor.Text.Trim().Length == 0;

            type = newType;
            details.Extensions = type == WebResourceType.Css ? new[] { ".css" } : new[] { ".html", ".htm" };
            CodeStyler.Apply(editor, type);
            UpdateLineNumberMargin();

            if (untouched)
            {
                SetEditorText(type == WebResourceType.Css ? CssStarter : HtmlStarter);
            }
            else
            {
                QueueRefresh();
            }
        }

        private void SetEditorText(string text)
        {
            editor.Text = text;
            editor.EmptyUndoBuffer();
            UpdateLineNumberMargin();
        }

        private void UpdateLineNumberMargin()
        {
            var digits = Math.Max(2, editor.Lines.Count.ToString().Length);
            editor.Margins[0].Width = editor.TextWidth(Style.LineNumber, new string('9', digits + 1)) + 4;
        }

        /// <summary>
        /// Keeps the indent of the previous line when Enter is pressed.
        /// </summary>
        private void Editor_CharAdded(object sender, CharAddedEventArgs e)
        {
            if (e.Char != '\n')
            {
                return;
            }

            var line = editor.CurrentLine;
            if (line > 0)
            {
                editor.Lines[line].Indentation = editor.Lines[line - 1].Indentation;
                // Indentation is in spaces because UseTabs is off
                editor.GotoPosition(editor.Lines[line].Position + editor.Lines[line].Indentation);
            }

            UpdateLineNumberMargin();
        }

        #endregion

        #region Preview

        private async void CustomCodePanel_Load(object sender, EventArgs e)
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
                preview.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;

                // Don't let a dark Windows theme switch pages that support dark mode to black
                preview.CoreWebView2.Profile.PreferredColorScheme = CoreWebView2PreferredColorScheme.Light;
                previewReady = true;
                RefreshPreview();
            }
            catch (Exception ex)
            {
                preview.Visible = false;
                lblPreviewUnavailable.Text = "The preview needs the Microsoft Edge WebView2 runtime.\r\n" + ex.Message;
                lblPreviewUnavailable.Visible = true;
            }
        }

        private void QueueRefresh()
        {
            // Debounce so the preview doesn't reload on every keystroke
            refreshTimer.Stop();
            refreshTimer.Start();
        }

        private void RefreshPreview()
        {
            if (!previewReady)
            {
                return;
            }

            var html = type == WebResourceType.Css ? CssSamplePage.Build(editor.Text) : editor.Text;

            // NavigateToString is limited to about 2 MB; anything bigger goes through a temp file
            if (html.Length <= 1500000)
            {
                preview.CoreWebView2.NavigateToString(html);
                return;
            }

            var file = Path.Combine(Path.GetTempPath(), "XrmToolBox.WebResourceManager", "custom-preview.html");
            File.WriteAllText(file, html);
            preview.CoreWebView2.Navigate(new Uri(file).AbsoluteUri);
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

            var isCss = type == WebResourceType.Css;
            using (var dialog = new SaveFileDialog
            {
                FileName = details.FullName.Split('/').Last(),
                Filter = isCss ? "CSS files (*.css)|*.css|All files (*.*)|*.*" : "HTML files (*.html)|*.html|All files (*.*)|*.*",
                DefaultExt = isCss ? "css" : "html"
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    File.WriteAllText(dialog.FileName, editor.Text, new UTF8Encoding(false));
                    lblStatus.Text = "Exported to " + Path.GetFileName(dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Could not export: " + ex.Message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                var outcome = await WebResourcePublisher.SaveAsync(this, Caption, service, details, type, editor.Text,
                    "this code", saveProgress.Report);
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
                MessageBox.Show(this, "Could not save: " + ex.Message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
