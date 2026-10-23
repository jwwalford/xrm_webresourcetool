using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using McTools.Xrm.Connection;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using ScintillaNET;
using WebResourceManager.Banners;
using WebResourceManager.Models;
using WebResourceManager.Services;
using XrmToolBox.Extensibility;

namespace WebResourceManager
{
    public partial class WebResourceManagerControl : PluginControlBase
    {
        // NavigateToString is limited to roughly 2 MB; bigger pages go through a temp file.
        private const int MaxInlineHtmlLength = 1500000;

        private readonly ListViewColumnSorter sorter = new ListViewColumnSorter();
        private List<WebResourceItem> webResources = new List<WebResourceItem>();
        private WebResourceItem current;
        private Scintilla codeView;
        private WebView2 browser;
        private bool browserReady;
        private string pendingHtml;
        private string tempFolder;
        private readonly ImageList typeIcons = TypeIcons.CreateImageList();

        public WebResourceManagerControl()
        {
            InitializeComponent();

            lvWebResources.ListViewItemSorter = sorter;
            lvWebResources.SmallImageList = typeIcons;

            tscbType.ComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            tscbType.ComboBox.ItemHeight = TypeIcons.BadgeSize.Height + 2;
            tscbType.ComboBox.DrawItem += tscbType_DrawItem;
            tslPreviewTitle.ImageScaling = ToolStripItemImageScaling.None;

            codeView = new Scintilla
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                WrapMode = WrapMode.None,
                BorderStyle = BorderStyle.None
            };
            codeView.Margins[0].Width = 40;
            scPreview.Panel2.Controls.Add(codeView);
            scPreview.Panel2Collapsed = true;

            browser = new WebView2
            {
                Dock = DockStyle.Fill,
                Visible = false
            };
            pnlPreview.Controls.Add(browser);

            toolsPanel.AddGroup("HTML Tools", ToolGroupIcon.Html);
            toolsPanel.AddTool("Create a banner...", "Design a form banner and save it as an HTML web resource", OpenBannerWizard);
            toolsPanel.AddTool("Coming soon", "Placeholder for a future HTML tool", null);
            toolsPanel.AddTool("Coming soon", "Placeholder for a future HTML tool", null);
            toolsPanel.AddGroup("Image Tools", ToolGroupIcon.Image);
            toolsPanel.AddTool("Coming soon", "Placeholder for a future image tool", null);
            toolsPanel.AddTool("Coming soon", "Placeholder for a future image tool", null);
            toolsPanel.AddTool("Coming soon", "Placeholder for a future image tool", null);
        }

        private void OpenBannerWizard()
        {
            // Not connected is fine: the wizard can still design and export to disk
            var dataFolder = Path.Combine(Path.GetTempPath(), "XrmToolBox.WebResourceManager", "WebView2");
            using (var wizard = new BannerWizardForm(Service, dataFolder))
            {
                wizard.ShowDialog(this);
                if (wizard.SavedName != null)
                {
                    tsslCount.Text = "Saved " + wizard.SavedName + ". Load web resources to see it in the list.";
                    tcLeft.SelectedTab = tpExplorer;
                }
            }
        }

        private void WebResourceManagerControl_Load(object sender, EventArgs e)
        {
            foreach (var filter in WebResourceTypeFilter.Defaults())
            {
                tscbType.Items.Add(filter);
            }
            tscbType.SelectedIndex = 0;

            InitializeBrowser();
        }

        private async void InitializeBrowser()
        {
            try
            {
                tempFolder = Path.Combine(Path.GetTempPath(), "XrmToolBox.WebResourceManager");
                Directory.CreateDirectory(tempFolder);

                var environment = await CoreWebView2Environment.CreateAsync(null, Path.Combine(tempFolder, "WebView2"));
                await browser.EnsureCoreWebView2Async(environment);

                browser.CoreWebView2.Settings.IsScriptEnabled = tsmiRunScripts.Checked;
                browser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                browser.CoreWebView2.Settings.AreDevToolsEnabled = true;
                browserReady = true;

                if (pendingHtml != null)
                {
                    NavigateToHtml(pendingHtml);
                    pendingHtml = null;
                }
            }
            catch (Exception ex)
            {
                LogError("WebView2 could not start: {0}", ex);
                ShowNoPreview("HTML and SVG preview needs the Microsoft Edge WebView2 runtime.\r\n" + ex.Message);
            }
        }

        public override void ClosingPlugin(PluginCloseInfo info)
        {
            base.ClosingPlugin(info);
            if (!info.Cancel)
            {
                browser.Dispose();
            }
        }

        public override void UpdateConnection(Microsoft.Xrm.Sdk.IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);

            // A different environment means the current list no longer applies
            webResources.Clear();
            lvWebResources.Items.Clear();
            ClearPreview();
            tsslCount.Text = "Connected to " + detail?.ConnectionName + ". Pick a type and click Load web resources.";
        }

        private void tsbClose_Click(object sender, EventArgs e)
        {
            CloseTool();
        }

        private void tsbLoad_Click(object sender, EventArgs e)
        {
            tcLeft.SelectedTab = tpExplorer;
            ExecuteMethod(LoadWebResources);
        }

        private void LoadWebResources()
        {
            var filter = tscbType.SelectedItem as WebResourceTypeFilter;
            var types = filter != null ? filter.Types : new WebResourceType[0];
            var hideHidden = tsmiHideHidden.Checked;
            var unmanagedOnly = tsmiUnmanagedOnly.Checked;

            ClearPreview();
            lvWebResources.Items.Clear();
            tsbLoad.Enabled = false;

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Loading web resources...",
                Work = (worker, args) =>
                {
                    args.Result = new WebResourceRepository(Service).GetWebResources(types, hideHidden, unmanagedOnly);
                },
                PostWorkCallBack = args =>
                {
                    tsbLoad.Enabled = true;

                    if (args.Error != null)
                    {
                        ShowErrorDialog(args.Error, "Could not load web resources");
                        return;
                    }

                    webResources = (List<WebResourceItem>)args.Result;
                    PopulateList();
                }
            });
        }

        private void PopulateList()
        {
            var search = tstSearch.Text.Trim();
            var visible = string.IsNullOrEmpty(search)
                ? webResources
                : webResources.Where(w => Contains(w.Name, search) || Contains(w.DisplayName, search)).ToList();

            lvWebResources.BeginUpdate();
            lvWebResources.Items.Clear();
            lvWebResources.Items.AddRange(visible.Select(ToListViewItem).ToArray());
            lvWebResources.EndUpdate();

            tsslCount.Text = visible.Count == webResources.Count
                ? string.Format("{0} web resource(s)", webResources.Count)
                : string.Format("{0} of {1} web resource(s)", visible.Count, webResources.Count);
        }

        private static bool Contains(string value, string search)
        {
            return value != null && value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void tscbType_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index < 0)
            {
                return;
            }

            var filter = (WebResourceTypeFilter)tscbType.Items[e.Index];
            var iconBounds = new Rectangle(e.Bounds.Left + 2, e.Bounds.Top + (e.Bounds.Height - TypeIcons.BadgeSize.Height) / 2, TypeIcons.BadgeSize.Width, TypeIcons.BadgeSize.Height);

            // Only single-type entries get a badge; "All types" and "All images" stay text-only
            if (filter.Types.Length == 1)
            {
                e.Graphics.DrawImage(typeIcons.Images[TypeIcons.Key(filter.Types[0])], iconBounds);
            }

            var textBounds = new Rectangle(iconBounds.Right + 4, e.Bounds.Top, e.Bounds.Right - iconBounds.Right - 4, e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, filter.Label, e.Font, textBounds, e.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
            e.DrawFocusRectangle();
        }

        private static ListViewItem ToListViewItem(WebResourceItem item)
        {
            var lvi = new ListViewItem(item.Name) { Tag = item, ImageKey = TypeIcons.Key(item.Type) };
            lvi.SubItems.Add(item.DisplayName ?? string.Empty);
            lvi.SubItems.Add(item.TypeLabel);
            lvi.SubItems.Add(item.IsManaged ? "Yes" : "No");
            lvi.SubItems.Add(item.ModifiedOn.HasValue ? item.ModifiedOn.Value.ToString("g") : string.Empty);
            return lvi;
        }

        private void tstSearch_TextChanged(object sender, EventArgs e)
        {
            if (tstSearch.Text.Length > 0)
            {
                tcLeft.SelectedTab = tpExplorer;
            }

            PopulateList();
        }

        private void lvWebResources_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == sorter.SortColumn)
            {
                sorter.Order = sorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                sorter.SortColumn = e.Column;
                sorter.Order = SortOrder.Ascending;
            }

            lvWebResources.Sort();
        }

        private void lvWebResources_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = SelectedItem;
            tsbSave.Enabled = item != null;

            if (item == null)
            {
                return;
            }

            current = item;
            ShowDetails(item);

            if (item.Content != null)
            {
                ShowPreview(item);
                return;
            }

            ShowNoPreview("Loading content...");
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Loading " + item.Name + "...",
                Work = (worker, args) =>
                {
                    item.Content = new WebResourceRepository(Service).GetContent(item.Id) ?? string.Empty;
                },
                PostWorkCallBack = args =>
                {
                    if (args.Error != null)
                    {
                        ShowNoPreview("Could not load content: " + args.Error.Message);
                        return;
                    }

                    // Ignore results for an item the user has already moved away from
                    if (current == item)
                    {
                        ShowPreview(item);
                    }
                }
            });
        }

        private WebResourceItem SelectedItem
        {
            get
            {
                return lvWebResources.SelectedItems.Count == 1
                    ? lvWebResources.SelectedItems[0].Tag as WebResourceItem
                    : null;
            }
        }

        private void ShowDetails(WebResourceItem item)
        {
            tslPreviewTitle.Text = item.Name;
            tslPreviewTitle.Image = typeIcons.Images[TypeIcons.Key(item.Type)];
            var details = string.Format("{0}  |  {1}  |  Modified {2}{3}",
                item.TypeLabel,
                item.IsManaged ? "Managed" : "Unmanaged",
                item.ModifiedOn.HasValue ? item.ModifiedOn.Value.ToString("g") : "-",
                string.IsNullOrEmpty(item.ModifiedBy) ? string.Empty : " by " + item.ModifiedBy);

            if (!string.IsNullOrEmpty(item.Description))
            {
                details += "\r\n" + item.Description;
            }

            lblDetails.Text = details;
        }

        /// <summary>
        /// Fills the live preview (top) and the code preview (bottom). Raster images
        /// have no code, so the bottom pane is collapsed for them.
        /// </summary>
        private void ShowPreview(WebResourceItem item)
        {
            if (string.IsNullOrEmpty(item.Content))
            {
                ShowNoPreview("This web resource has no content.");
                return;
            }

            try
            {
                switch (item.Type)
                {
                    case WebResourceType.Png:
                    case WebResourceType.Jpg:
                    case WebResourceType.Gif:
                    case WebResourceType.Ico:
                        ShowImage(item);
                        ShowCode(null);
                        break;
                    case WebResourceType.Html:
                    case WebResourceType.Svg:
                    case WebResourceType.Css:
                        ShowBrowser(item);
                        ShowCode(item);
                        break;
                    default:
                        // Fallback: the explorer only lists HTML, CSS and images
                        ShowNoPreviewMessage(item.PreviewKind == PreviewKind.Text
                            ? "No live preview for " + item.TypeLabel + "."
                            : item.TypeLabel + " web resources can't be previewed. Use Save to disk to inspect it.");
                        ShowCode(item.PreviewKind == PreviewKind.Text ? item : null);
                        break;
                }
            }
            catch (Exception ex)
            {
                ShowNoPreview("Could not preview this web resource: " + ex.Message);
            }
        }

        private void ShowImage(WebResourceItem item)
        {
            var bytes = WebResourceRepository.DecodeBytes(item.Content);
            Image image;
            using (var stream = new MemoryStream(bytes))
            {
                if (item.Type == WebResourceType.Ico)
                {
                    using (var icon = new Icon(stream))
                    {
                        image = icon.ToBitmap();
                    }
                }
                else
                {
                    // Copy so the image doesn't depend on the stream staying open
                    using (var loaded = Image.FromStream(stream))
                    {
                        image = new Bitmap(loaded);
                    }
                }
            }

            var previous = pbImage.Image;
            pbImage.Image = image;
            previous?.Dispose();

            lblDetails.Text += string.Format("  |  {0} x {1} px, {2:N1} KB", image.Width, image.Height, bytes.Length / 1024.0);
            UpdateImageSizeMode();
            ShowLive(pbImage);
        }

        private void pbImage_Resize(object sender, EventArgs e)
        {
            UpdateImageSizeMode();
        }

        private void UpdateImageSizeMode()
        {
            var image = pbImage.Image;
            if (image == null)
            {
                return;
            }

            // Shrink large images to fit, but don't blow small icons up
            pbImage.SizeMode = image.Width > pbImage.ClientSize.Width || image.Height > pbImage.ClientSize.Height
                ? PictureBoxSizeMode.Zoom
                : PictureBoxSizeMode.CenterImage;
        }

        private void ShowBrowser(WebResourceItem item)
        {
            var text = WebResourceRepository.DecodeText(item.Content);
            string html;
            switch (item.Type)
            {
                case WebResourceType.Svg:
                    html = "<!DOCTYPE html><html><head><meta charset=\"utf-8\"><style>html,body{margin:0;height:100%;display:flex;align-items:center;justify-content:center;background:#fff}svg{max-width:100%;max-height:100%}</style></head><body>" + text + "</body></html>";
                    break;
                case WebResourceType.Css:
                    html = CssSamplePage.Build(text);
                    break;
                default:
                    html = text;
                    break;
            }

            ShowLive(browser);

            if (!browserReady)
            {
                pendingHtml = html;
                return;
            }

            NavigateToHtml(html);
        }

        private void NavigateToHtml(string html)
        {
            if (html.Length <= MaxInlineHtmlLength)
            {
                browser.CoreWebView2.NavigateToString(html);
                return;
            }

            var file = Path.Combine(tempFolder, "preview.html");
            File.WriteAllText(file, html);
            browser.CoreWebView2.Navigate(new Uri(file).AbsoluteUri);
        }

        private void tsmiRunScripts_CheckedChanged(object sender, EventArgs e)
        {
            if (!browserReady)
            {
                return;
            }

            browser.CoreWebView2.Settings.IsScriptEnabled = tsmiRunScripts.Checked;
            if (current != null && current.Content != null && browser.Visible)
            {
                ShowPreview(current);
            }
        }

        /// <param name="item">Null collapses the code pane.</param>
        private void ShowCode(WebResourceItem item)
        {
            scPreview.Panel2Collapsed = item == null;
            if (item == null)
            {
                return;
            }

            CodeStyler.Apply(codeView, item.Type);

            codeView.ReadOnly = false;
            codeView.Text = WebResourceRepository.DecodeText(item.Content);
            codeView.ReadOnly = true;
            codeView.EmptyUndoBuffer();
            codeView.GotoPosition(0);

            // Widen the line number margin to fit the line count
            var digits = codeView.Lines.Count.ToString().Length;
            codeView.Margins[0].Width = codeView.TextWidth(Style.LineNumber, new string('9', digits + 1)) + 4;
        }

        /// <summary>
        /// Shows a message in place of the live preview and hides the code pane.
        /// </summary>
        private void ShowNoPreview(string message)
        {
            ShowNoPreviewMessage(message);
            ShowCode(null);
        }

        private void ShowNoPreviewMessage(string message)
        {
            lblNoPreview.Text = message;
            ShowLive(lblNoPreview);
        }

        private void ClearPreview()
        {
            current = null;
            tsbSave.Enabled = false;
            tslPreviewTitle.Text = "Preview";
            tslPreviewTitle.Image = null;
            lblDetails.Text = string.Empty;
            ShowNoPreview("Select a web resource to preview it.");
        }

        private void ShowLive(Control control)
        {
            foreach (var view in new Control[] { lblNoPreview, pbImage, browser })
            {
                view.Visible = view == control;
            }
        }

        private void cmsList_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = SelectedItem == null;
        }

        private void tsmiCopyName_Click(object sender, EventArgs e)
        {
            var item = SelectedItem;
            if (item != null)
            {
                Clipboard.SetText(item.Name);
            }
        }

        private void SaveSelected_Click(object sender, EventArgs e)
        {
            var item = SelectedItem;
            if (item == null)
            {
                return;
            }

            var extension = WebResourceTypeInfo.GetExtension(item.Type);
            var fileName = item.Name.Split('/').Last();
            if (!fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                fileName += extension;
            }

            using (var dialog = new SaveFileDialog { FileName = fileName, Filter = "All files (*.*)|*.*" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                var path = dialog.FileName;
                if (item.Content != null)
                {
                    File.WriteAllBytes(path, WebResourceRepository.DecodeBytes(item.Content));
                    return;
                }

                WorkAsync(new WorkAsyncInfo
                {
                    Message = "Downloading " + item.Name + "...",
                    Work = (worker, args) =>
                    {
                        item.Content = new WebResourceRepository(Service).GetContent(item.Id) ?? string.Empty;
                        File.WriteAllBytes(path, WebResourceRepository.DecodeBytes(item.Content));
                    },
                    PostWorkCallBack = args =>
                    {
                        if (args.Error != null)
                        {
                            ShowErrorDialog(args.Error, "Could not save web resource");
                        }
                    }
                });
            }
        }
    }
}
