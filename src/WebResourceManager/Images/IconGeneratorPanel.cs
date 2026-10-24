using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WebResourceManager.Models;
using WebResourceManager.Tools;

namespace WebResourceManager.Images
{
    /// <summary>
    /// Tool page that turns an image into icon web resources: pick a source image,
    /// preview it at each standard icon size on light and dark backgrounds, tick the
    /// sizes to keep and save them as PNGs (one per size) or a single multi-size ICO.
    /// </summary>
    internal class IconGeneratorPanel : UserControl
    {
        private const string Caption = "Create an icon";

        private static readonly int[] DefaultSizes = { 16, 32 };
        private static readonly Color LightTile = Color.White;
        private static readonly Color DarkTile = Color.FromArgb(32, 32, 32);

        private readonly Dictionary<int, SizeCard> cards = new Dictionary<int, SizeCard>();

        private Microsoft.Xrm.Sdk.IOrganizationService service;
        private Bitmap source;
        private string sourceFileName;

        private WebResourceDetailsBox details;
        private PictureBox pbSource;
        private Label lblSource;
        private RadioButton rbFit;
        private RadioButton rbCrop;
        private RadioButton rbPng;
        private RadioButton rbIco;
        private Label lblNoSource;
        private Button btnExport;
        private Button btnSave;
        private Label lblStatus;
        private SaveProgress saveProgress;

        public IconGeneratorPanel(Microsoft.Xrm.Sdk.IOrganizationService service)
        {
            this.service = service;

            Font = new Font("Segoe UI", 9F);
            BackColor = SystemColors.Control;

            BuildLayout();
            UpdateFormat();
        }

        /// <summary>
        /// Raised after the icons are saved to Dataverse, with a short description of what was saved.
        /// </summary>
        public event EventHandler<string> Saved;

        public void SetService(Microsoft.Xrm.Sdk.IOrganizationService newService)
        {
            service = newService;
            details.SetService(newService);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                source?.Dispose();
                foreach (var card in cards.Values)
                {
                    card.Image?.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private IconFitMode Mode
        {
            get { return rbCrop.Checked ? IconFitMode.Crop : IconFitMode.Fit; }
        }

        private bool IsIco
        {
            get { return rbIco.Checked; }
        }

        private List<int> SelectedSizes
        {
            get { return cards.Where(c => c.Value.Check.Checked).Select(c => c.Key).OrderBy(s => s).ToList(); }
        }

        #region Layout

        private void BuildLayout()
        {
            var header = ActionIcons.ToolHeader(Caption, "Choose an image, tick the icon sizes you want and save them as web resources. Export saves the files instead.", ActionIcons.ToolIconSizes);

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

            // Details and source side by side, the size previews take the rest
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(8, 6, 8, 0) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 300));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            details = new WebResourceDetailsBox(service, "icon", "icons/my_icon", "My icon");
            details.StateChanged += (s, e) => UpdateButtons();
            details.DescribeName = DescribeName;

            layout.Controls.Add(details, 0, 0);
            layout.Controls.Add(BuildSourceGroup(), 1, 0);
            var sizesGroup = BuildSizesGroup();
            layout.Controls.Add(sizesGroup, 0, 1);
            layout.SetColumnSpan(sizesGroup, 2);

            Controls.Add(layout);
            Controls.Add(buttons);
            Controls.Add(divider);
            Controls.Add(header);
        }

        private GroupBox BuildSourceGroup()
        {
            var group = new GroupBox { Text = "Source image", Dock = DockStyle.Fill, Padding = new Padding(8) };

            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var btnChoose = new Button { Text = "Choose image...", Width = 120, Margin = new Padding(3, 3, 3, 6) };
            btnChoose.Click += BtnChoose_Click;
            lblSource = new Label { AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 8, 3, 3), Text = "PNG, JPG, GIF, BMP or ICO. A square image of 128px or more works best." , MaximumSize = new Size(320, 0) };
            var choosePanel = new FlowLayoutPanel { AutoSize = true, WrapContents = false, FlowDirection = FlowDirection.TopDown, Margin = new Padding(0) };
            choosePanel.Controls.Add(btnChoose);
            choosePanel.Controls.Add(lblSource);

            pbSource = new PictureBox
            {
                Size = new Size(96, 96),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Margin = new Padding(3, 3, 3, 6)
            };

            rbFit = new RadioButton { Text = "Fit (pad to square)", AutoSize = true, Checked = true, Margin = new Padding(3, 4, 12, 3) };
            rbCrop = new RadioButton { Text = "Crop to square", AutoSize = true, Margin = new Padding(3, 4, 12, 3) };
            rbFit.CheckedChanged += (s, e) => { if (rbFit.Checked) RenderPreviews(); };
            rbCrop.CheckedChanged += (s, e) => { if (rbCrop.Checked) RenderPreviews(); };
            var modePanel = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
            modePanel.Controls.Add(rbFit);
            modePanel.Controls.Add(rbCrop);

            rbPng = new RadioButton { Text = "PNG, one per size", AutoSize = true, Checked = true, Margin = new Padding(3, 4, 12, 3) };
            rbIco = new RadioButton { Text = "ICO, one file with all sizes", AutoSize = true, Margin = new Padding(3, 4, 12, 3) };
            rbPng.CheckedChanged += (s, e) => UpdateFormat();
            var formatPanel = new FlowLayoutPanel { AutoSize = true, WrapContents = true, Margin = new Padding(0), MaximumSize = new Size(360, 0) };
            formatPanel.Controls.Add(rbPng);
            formatPanel.Controls.Add(rbIco);

            AddRow(table, "Image:", choosePanel);
            AddRow(table, string.Empty, pbSource);
            AddRow(table, "Shape:", modePanel);
            AddRow(table, "Save as:", formatPanel);

            group.Controls.Add(table);
            return group;
        }

        private GroupBox BuildSizesGroup()
        {
            var group = new GroupBox { Text = "Sizes", Dock = DockStyle.Fill, Padding = new Padding(8) };

            var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 26, WrapContents = false };
            var lnkAll = new LinkLabel { Text = "Select all", AutoSize = true, Margin = new Padding(3, 4, 12, 3) };
            var lnkNone = new LinkLabel { Text = "Clear", AutoSize = true, Margin = new Padding(3, 4, 12, 3) };
            lnkAll.LinkClicked += (s, e) => SetAllChecked(true);
            lnkNone.LinkClicked += (s, e) => SetAllChecked(false);
            bar.Controls.Add(new Label { Text = "Each icon is shown at actual size on a light and a dark background.", AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(0, 4, 12, 3) });
            bar.Controls.Add(lnkAll);
            bar.Controls.Add(lnkNone);

            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = SystemColors.ControlLightLight, Padding = new Padding(4) };
            foreach (var size in IconResizer.StandardSizes)
            {
                var card = new SizeCard(size, DefaultSizes.Contains(size));
                card.Check.CheckedChanged += (s, e) => { details.RefreshNameHint(); UpdateButtons(); };
                cards[size] = card;
                flow.Controls.Add(card.Panel);
            }

            lblNoSource = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 24,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.DimGray,
                Text = "Choose a source image to see the icons."
            };

            group.Controls.Add(flow);
            group.Controls.Add(lblNoSource);
            group.Controls.Add(bar);
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

        #endregion

        #region Source and previews

        private void BtnChoose_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { Filter = IconResizer.OpenFilter, Title = "Choose the icon's source image" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                Bitmap image;
                try
                {
                    image = IconResizer.Load(dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Could not open the image: " + ex.Message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                source?.Dispose();
                source = image;
                sourceFileName = Path.GetFileName(dialog.FileName);
                pbSource.Image = source;

                var note = source.Width != source.Height ? " Not square, so choose Fit or Crop." :
                    source.Width < IconResizer.StandardSizes.Max() ? " Smaller sizes will look better than the larger ones." : string.Empty;
                lblSource.Text = string.Format("{0} ({1} x {2}).{3}", sourceFileName, source.Width, source.Height, note);
                lblNoSource.Visible = false;

                RenderPreviews();
                UpdateButtons();
            }
        }

        private void RenderPreviews()
        {
            foreach (var card in cards)
            {
                var old = card.Value.Image;
                card.Value.Image = source != null ? IconResizer.Resize(source, card.Key, Mode) : null;
                card.Value.Invalidate();
                old?.Dispose();
            }
        }

        private void SetAllChecked(bool check)
        {
            foreach (var card in cards.Values)
            {
                card.Check.Checked = check;
            }
        }

        private void UpdateFormat()
        {
            details.Extensions = IsIco ? new[] { ".ico" } : new[] { ".png" };
            UpdateButtons();
        }

        private string DescribeName(string fullName)
        {
            var sizes = SelectedSizes;
            if (IsIco || sizes.Count == 0)
            {
                return "Web resource name: " + fullName;
            }

            if (sizes.Count == 1)
            {
                return "Web resource name: " + SizedName(fullName, sizes[0]);
            }

            return string.Format("Creates {0} for sizes {1}", SizedName(fullName, "{size}"), string.Join(", ", sizes));
        }

        private static string SizedName(string fullName, object size)
        {
            // my_icon.png becomes my_icon_16.png
            var dot = fullName.LastIndexOf('.');
            return fullName.Substring(0, dot) + "_" + size + fullName.Substring(dot);
        }

        private void UpdateButtons()
        {
            var ready = source != null && SelectedSizes.Count > 0;
            btnExport.Enabled = ready;
            btnSave.Enabled = ready && details.CanSave;
        }

        #endregion

        #region Save and export

        /// <summary>
        /// The web resources to create: one PNG per size, or one ICO with every size.
        /// </summary>
        private List<WebResourceContent> BuildContent()
        {
            var sizes = SelectedSizes;
            var fullName = details.FullName;
            var displayName = details.DisplayName;

            if (IsIco)
            {
                return new List<WebResourceContent>
                {
                    new WebResourceContent
                    {
                        Name = fullName,
                        DisplayName = displayName,
                        Type = WebResourceType.Ico,
                        Base64Content = Convert.ToBase64String(IconResizer.ToIco(sizes.Select(s => cards[s].Image)))
                    }
                };
            }

            return sizes.Select(size => new WebResourceContent
            {
                Name = SizedName(fullName, size),
                DisplayName = string.Format("{0} ({1}x{1})", displayName, size),
                Type = WebResourceType.Png,
                Base64Content = Convert.ToBase64String(IconResizer.ToPng(cards[size].Image))
            }).ToList();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (!details.ValidateName(Caption))
            {
                return;
            }

            var items = BuildContent();
            try
            {
                if (IsIco)
                {
                    using (var dialog = new SaveFileDialog
                    {
                        FileName = items[0].Name.Split('/').Last(),
                        Filter = "Icon files (*.ico)|*.ico|All files (*.*)|*.*",
                        DefaultExt = "ico"
                    })
                    {
                        if (dialog.ShowDialog(this) != DialogResult.OK)
                        {
                            return;
                        }

                        File.WriteAllBytes(dialog.FileName, Convert.FromBase64String(items[0].Base64Content));
                        lblStatus.Text = "Exported to " + Path.GetFileName(dialog.FileName);
                    }
                    return;
                }

                using (var dialog = new FolderBrowserDialog { Description = "Choose a folder for the icon PNG files" })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }

                    foreach (var item in items)
                    {
                        File.WriteAllBytes(Path.Combine(dialog.SelectedPath, item.Name.Split('/').Last()), Convert.FromBase64String(item.Base64Content));
                    }

                    lblStatus.Text = string.Format("Exported {0} file{1} to {2}", items.Count, items.Count == 1 ? string.Empty : "s", dialog.SelectedPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Could not export: " + ex.Message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (!details.CanSave || !details.ValidateName(Caption))
            {
                return;
            }

            var solution = details.SelectedSolution;
            var items = BuildContent();
            SetBusy(true);
            try
            {
                var result = await WebResourcePublisher.SaveManyAsync(this, Caption, service, details, items, saveProgress.Report);
                if (result == null)
                {
                    saveProgress.Reset();
                    return;
                }

                var summary = items.Count == 1 ? items[0].Name : string.Format("{0} icon web resources", items.Count);
                Saved?.Invoke(this, summary);
                saveProgress.Complete("Saved and published " + summary);

                var parts = new List<string>();
                if (result.Item1 > 0) parts.Add(result.Item1 + " created");
                if (result.Item2 > 0) parts.Add(result.Item2 + " updated");
                MessageBox.Show(this,
                    string.Format("Saved and published{0}:\r\n\r\n{1}\r\n\r\n({2})",
                        solution.IsDefault ? string.Empty : " in " + solution.FriendlyName,
                        string.Join("\r\n", items.Select(i => i.Name)),
                        string.Join(", ", parts)),
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
            btnExport.Enabled = !busy && source != null && SelectedSizes.Count > 0;
            btnSave.Enabled = !busy && source != null && SelectedSizes.Count > 0 && details.CanSave;
        }

        #endregion

        /// <summary>
        /// One size in the gallery: a checkbox and the icon drawn at actual size on
        /// a light and a dark tile.
        /// </summary>
        private class SizeCard
        {
            private readonly Panel lightTile;
            private readonly Panel darkTile;

            public SizeCard(int size, bool isChecked)
            {
                var tile = Math.Max(size + 24, 56);

                Check = new CheckBox
                {
                    Text = string.Format("{0} x {0}", size),
                    AutoSize = true,
                    Checked = isChecked,
                    Location = new Point(4, 4)
                };

                lightTile = CreateTile(tile, LightTile, new Point(4, 30));
                darkTile = CreateTile(tile, DarkTile, new Point(tile + 10, 30));

                Panel = new Panel
                {
                    Size = new Size(tile * 2 + 14, tile + 36),
                    Margin = new Padding(4),
                    BackColor = SystemColors.Control
                };
                Panel.Controls.Add(Check);
                Panel.Controls.Add(lightTile);
                Panel.Controls.Add(darkTile);

                // Clicking anywhere on the card toggles the size
                lightTile.Click += (s, e) => Check.Checked = !Check.Checked;
                darkTile.Click += (s, e) => Check.Checked = !Check.Checked;
            }

            public Panel Panel { get; }

            public CheckBox Check { get; }

            public Bitmap Image { get; set; }

            public void Invalidate()
            {
                lightTile.Invalidate();
                darkTile.Invalidate();
            }

            private Panel CreateTile(int tile, Color back, Point location)
            {
                var panel = new Panel { Size = new Size(tile, tile), Location = location, BackColor = back, Cursor = Cursors.Hand };
                panel.Paint += (s, e) =>
                {
                    if (Image == null)
                    {
                        return;
                    }

                    // Drawn 1:1 so the preview shows exactly what the icon will look like
                    e.Graphics.DrawImageUnscaled(Image, (tile - Image.Width) / 2, (tile - Image.Height) / 2);
                };
                return panel;
            }
        }
    }
}
