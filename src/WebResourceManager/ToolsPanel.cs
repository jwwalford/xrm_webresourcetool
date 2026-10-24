using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WebResourceManager
{
    public enum ToolGroupIcon
    {
        Html,
        Image
    }

    /// <summary>
    /// The Tools tab: coloured group headers ("HTML Tools", "Image Tools") with a
    /// stack of large buttons under each. A tool added without an action is a
    /// disabled placeholder. The open tool's button is highlighted with
    /// <see cref="SetActive"/>.
    /// </summary>
    internal class ToolsPanel : Panel
    {
        private static readonly Color HeaderColor = Color.Black;
        private static readonly Color ButtonColor = Color.FromArgb(128, 128, 128);
        private static readonly Color ButtonHoverColor = Color.FromArgb(96, 96, 96);
        private static readonly Color ButtonDisabledColor = Color.FromArgb(200, 200, 200);
        private static readonly Color ActiveColor = Color.FromArgb(48, 48, 48);
        private static readonly Color AccentColor = Color.FromArgb(0, 120, 212);
        private const int AccentWidth = 6;

        private readonly TableLayoutPanel stack;
        private readonly ToolTip toolTip = new ToolTip();
        private Button activeButton;

        public ToolsPanel()
        {
            AutoScroll = true;
            BackColor = SystemColors.Window;
            Padding = new Padding(4);

            stack = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                Padding = new Padding(0)
            };
            stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            Controls.Add(stack);
        }

        public void AddGroup(string title, ToolGroupIcon icon)
        {
            var iconImage = DrawGroupIcon(icon);
            var header = new Panel
            {
                Height = 46,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, stack.Controls.Count == 0 ? 0 : 8, 0, 3),
                BackColor = HeaderColor,
                Font = new Font("Segoe UI", 16f, FontStyle.Regular)
            };
            header.Paint += (s, e) =>
            {
                e.Graphics.DrawImage(iconImage, 0, (header.Height - iconImage.Height) / 2);
                var textBounds = new Rectangle(iconImage.Width + 12, 0, header.Width - iconImage.Width - 12, header.Height);
                TextRenderer.DrawText(e.Graphics, title, header.Font, textBounds, Color.White,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
            };
            header.Disposed += (s, e) => iconImage.Dispose();
            AddRow(header);
        }

        /// <param name="onClick">Null adds a disabled placeholder.</param>
        /// <returns>The button, to pass to <see cref="SetActive"/>.</returns>
        public Button AddTool(string text, string description, Action onClick)
        {
            var button = new Button
            {
                Text = text,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(56, 0, 0, 0),
                Height = 48,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 3),
                FlatStyle = FlatStyle.Flat,
                BackColor = onClick != null ? ButtonColor : ButtonDisabledColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13f, FontStyle.Regular),
                Cursor = onClick != null ? Cursors.Hand : Cursors.Default,
                Enabled = onClick != null,
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = ButtonHoverColor;
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(64, 64, 64);

            if (onClick != null)
            {
                button.Click += (s, e) => onClick();
            }

            button.Paint += (s, e) =>
            {
                if (button == activeButton)
                {
                    using (var accent = new SolidBrush(AccentColor))
                    {
                        e.Graphics.FillRectangle(accent, 0, 0, AccentWidth, button.Height);
                    }
                }
            };

            toolTip.SetToolTip(button, description);
            AddRow(button);
            return button;
        }

        /// <summary>
        /// Highlights the button of the tool that's open: darker, bold, with an accent bar.
        /// </summary>
        public void SetActive(Button button)
        {
            if (activeButton == button)
            {
                return;
            }

            var previous = activeButton;
            activeButton = button;

            if (previous != null)
            {
                Style(previous, false);
            }

            if (button != null)
            {
                Style(button, true);
            }
        }

        private static void Style(Button button, bool active)
        {
            button.BackColor = active ? ActiveColor : ButtonColor;
            button.FlatAppearance.MouseOverBackColor = active ? ActiveColor : ButtonHoverColor;
            button.Font = new Font(button.Font, active ? FontStyle.Bold : FontStyle.Regular);
            button.Invalidate();
        }

        private void AddRow(Control control)
        {
            stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            stack.Controls.Add(control, 0, stack.RowCount++);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                toolTip.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Draws the header icon in white: a globe for HTML, a picture for images.
        /// </summary>
        private static Bitmap DrawGroupIcon(ToolGroupIcon icon)
        {
            const int size = 30;
            var bitmap = new Bitmap(size + 12, size);
            using (var g = Graphics.FromImage(bitmap))
            using (var pen = new Pen(Color.White, 2.2f))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TranslateTransform(10, 0);
                var bounds = new RectangleF(2, 2, size - 4, size - 4);

                if (icon == ToolGroupIcon.Html)
                {
                    g.DrawEllipse(pen, bounds);
                    g.DrawEllipse(pen, bounds.Left + bounds.Width * 0.27f, bounds.Top, bounds.Width * 0.46f, bounds.Height);
                    g.DrawLine(pen, bounds.Left, bounds.Top + bounds.Height / 2, bounds.Right, bounds.Top + bounds.Height / 2);
                    g.DrawLine(pen, bounds.Left + 3, bounds.Top + bounds.Height * 0.25f, bounds.Right - 3, bounds.Top + bounds.Height * 0.25f);
                    g.DrawLine(pen, bounds.Left + 3, bounds.Top + bounds.Height * 0.75f, bounds.Right - 3, bounds.Top + bounds.Height * 0.75f);
                }
                else
                {
                    g.DrawRectangle(pen, bounds.Left, bounds.Top + 2, bounds.Width, bounds.Height - 4);
                    using (var brush = new SolidBrush(Color.White))
                    {
                        g.FillEllipse(brush, bounds.Left + 5, bounds.Top + 6, 6, 6);
                        g.FillPolygon(brush, new[]
                        {
                            new PointF(bounds.Left + 3, bounds.Bottom - 4),
                            new PointF(bounds.Left + 11, bounds.Top + 13),
                            new PointF(bounds.Left + 16, bounds.Bottom - 9),
                            new PointF(bounds.Left + 19, bounds.Top + 11),
                            new PointF(bounds.Right - 3, bounds.Bottom - 4)
                        });
                    }
                }
            }

            return bitmap;
        }
    }
}
