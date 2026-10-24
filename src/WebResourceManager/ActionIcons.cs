using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WebResourceManager
{
    /// <summary>
    /// 16px icons for the toolbar and tool buttons (Close, Load, Save, Export).
    /// Drawn at runtime like the type badges, so there are no image resources.
    /// </summary>
    internal static class ActionIcons
    {
        private const int Size = 16;

        private static readonly Color Red = Color.FromArgb(209, 52, 56);
        private static readonly Color Blue = Color.FromArgb(0, 120, 212);
        private static readonly Color Green = Color.FromArgb(16, 124, 16);

        public static readonly Bitmap Close = Draw(DrawClose);
        public static readonly Bitmap Load = Draw(DrawLoad);
        public static readonly Bitmap Save = Draw(DrawSave);
        public static readonly Bitmap Export = Draw(DrawExport);

        // 20px black glyphs shown before each tool page's title
        public static readonly Bitmap ToolBanner = DrawTool(DrawToolBanner);
        public static readonly Bitmap ToolButton = DrawTool(DrawToolButton);
        public static readonly Bitmap ToolCode = DrawTool(DrawToolCode);
        public static readonly Bitmap ToolIconSizes = DrawTool(DrawToolIconSizes);
        public static readonly Bitmap ToolSvg = DrawTool(DrawToolSvg);
        public static readonly Bitmap ToolUpload = DrawTool(DrawToolUpload);

        private const int ToolSize = 20;

        // Toolbox shown above "Pick a tool on the left to start", in the same grey as the text
        public static readonly Bitmap EmptyStateToolbox = DrawEmptyStateToolbox(64);

        /// <summary>
        /// The white header at the top of a tool page: icon, title, description
        /// and the connection status on the right.
        /// </summary>
        public static ToolHeader ToolHeader(string title, string description, Image icon)
        {
            return new ToolHeader(title, description, icon, ToolSize);
        }

        /// <summary>
        /// Puts an icon before a button's text and widens the button to make room.
        /// </summary>
        public static void Apply(Button button, Image icon)
        {
            button.Image = icon;
            button.ImageAlign = ContentAlignment.MiddleLeft;
            button.TextImageRelation = TextImageRelation.ImageBeforeText;
            button.Width += Size + 6;
        }

        private static Bitmap Draw(Action<Graphics> paint)
        {
            var bitmap = new Bitmap(Size, Size);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                paint(g);
            }
            return bitmap;
        }

        private static Bitmap DrawTool(Action<Graphics, Pen> paint)
        {
            var bitmap = new Bitmap(ToolSize, ToolSize);
            using (var g = Graphics.FromImage(bitmap))
            using (var pen = new Pen(Color.Black, 1.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                paint(g, pen);
            }
            return bitmap;
        }

        /// <summary>
        /// A bar with a coloured strip on the left and a line of text.
        /// </summary>
        private static void DrawToolBanner(Graphics g, Pen pen)
        {
            using (var path = RoundedRectangle(new RectangleF(1.5f, 5.5f, 17f, 9f), 1.5f))
            {
                g.DrawPath(pen, path);
            }
            g.FillRectangle(Brushes.Black, 1.5f, 5.5f, 3.5f, 9f);
            g.DrawLine(pen, 8f, 8.5f, 15.5f, 8.5f);
            g.DrawLine(pen, 8f, 11.5f, 13f, 11.5f);
        }

        /// <summary>
        /// A rounded button with a pointer clicking it.
        /// </summary>
        private static void DrawToolButton(Graphics g, Pen pen)
        {
            using (var path = RoundedRectangle(new RectangleF(1.5f, 3.5f, 17f, 8f), 3f))
            {
                g.DrawPath(pen, path);
            }
            g.FillPolygon(Brushes.Black, new[]
            {
                new PointF(10f, 8f), new PointF(10f, 18.5f), new PointF(12.5f, 16f), new PointF(14.5f, 19.5f),
                new PointF(16f, 18.7f), new PointF(14f, 15.3f), new PointF(17.3f, 15f)
            });
        }

        /// <summary>
        /// Code brackets: &lt;/&gt;.
        /// </summary>
        private static void DrawToolCode(Graphics g, Pen pen)
        {
            g.DrawLines(pen, new[] { new PointF(6f, 5f), new PointF(1.5f, 10f), new PointF(6f, 15f) });
            g.DrawLines(pen, new[] { new PointF(14f, 5f), new PointF(18.5f, 10f), new PointF(14f, 15f) });
            g.DrawLine(pen, 11.5f, 3.5f, 8.5f, 16.5f);
        }

        /// <summary>
        /// Three squares of growing size, for icon sizes.
        /// </summary>
        private static void DrawToolIconSizes(Graphics g, Pen pen)
        {
            g.DrawRectangle(pen, 1f, 15.5f, 3f, 3f);
            g.DrawRectangle(pen, 5.5f, 13.5f, 5f, 5f);
            g.DrawRectangle(pen, 12f, 11f, 7.5f, 7.5f);
        }

        /// <summary>
        /// A vector curve with its two anchor points and a handle.
        /// </summary>
        private static void DrawToolSvg(Graphics g, Pen pen)
        {
            g.DrawBezier(pen, 3.5f, 16.5f, 5f, 4f, 15f, 4f, 16.5f, 16.5f);
            g.DrawLine(pen, 3.5f, 4.5f, 16.5f, 4.5f);
            g.FillRectangle(Brushes.Black, 1.5f, 14.5f, 4f, 4f);
            g.FillRectangle(Brushes.Black, 14.5f, 14.5f, 4f, 4f);
            g.FillEllipse(Brushes.Black, 8f, 2.5f, 4f, 4f);
        }

        /// <summary>
        /// A picture with an upload arrow.
        /// </summary>
        private static void DrawToolUpload(Graphics g, Pen pen)
        {
            g.DrawLines(pen, new[] { new PointF(1.5f, 11f), new PointF(1.5f, 18.5f), new PointF(18.5f, 18.5f), new PointF(18.5f, 11f) });
            g.DrawLine(pen, 10f, 14.5f, 10f, 3f);
            g.DrawLines(pen, new[] { new PointF(5.5f, 7f), new PointF(10f, 2f), new PointF(14.5f, 7f) });
        }

        /// <summary>
        /// A toolbox: handle, lid and body with two latches.
        /// </summary>
        private static Bitmap DrawEmptyStateToolbox(int size)
        {
            var bitmap = new Bitmap(size, size);
            var scale = size / 64f;
            using (var g = Graphics.FromImage(bitmap))
            using (var pen = new Pen(SystemColors.GrayText, 3.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            using (var brush = new SolidBrush(SystemColors.GrayText))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.ScaleTransform(scale, scale);

                // Handle
                using (var handle = RoundedRectangle(new RectangleF(22f, 9f, 20f, 13f), 4f))
                {
                    g.DrawPath(pen, handle);
                }

                // Body, with the lid split across the top third
                using (var body = RoundedRectangle(new RectangleF(5f, 20f, 54f, 36f), 5f))
                {
                    g.DrawPath(pen, body);
                }
                g.DrawLine(pen, 5f, 33f, 59f, 33f);

                // Latches
                g.FillRectangle(brush, 15f, 29f, 6f, 9f);
                g.FillRectangle(brush, 43f, 29f, 6f, 9f);
            }
            return bitmap;
        }

        private static GraphicsPath RoundedRectangle(RectangleF bounds, float radius)
        {
            var diameter = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static void DrawClose(Graphics g)
        {
            using (var pen = new Pen(Red, 2.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(pen, 3.5f, 3.5f, 12.5f, 12.5f);
                g.DrawLine(pen, 12.5f, 3.5f, 3.5f, 12.5f);
            }
        }

        /// <summary>
        /// Circular refresh arrow.
        /// </summary>
        private static void DrawLoad(Graphics g)
        {
            using (var pen = new Pen(Blue, 2f))
            using (var brush = new SolidBrush(Blue))
            {
                g.DrawArc(pen, 2.5f, 2.5f, 11f, 11f, -60, 300);
                // Arrow head at the open end of the arc (top right)
                g.FillPolygon(brush, new[] { new PointF(15.5f, 1.5f), new PointF(15.5f, 7.5f), new PointF(9.5f, 7.5f) });
            }
        }

        /// <summary>
        /// Floppy disk.
        /// </summary>
        private static void DrawSave(Graphics g)
        {
            using (var body = new SolidBrush(Blue))
            using (var white = new SolidBrush(Color.White))
            using (var path = new GraphicsPath())
            {
                path.AddLines(new[] { new PointF(1.5f, 1.5f), new PointF(12f, 1.5f), new PointF(14.5f, 4f), new PointF(14.5f, 14.5f), new PointF(1.5f, 14.5f) });
                path.CloseFigure();
                g.FillPath(body, path);

                g.SmoothingMode = SmoothingMode.None;
                g.FillRectangle(white, 4, 2, 7, 4);          // Shutter
                g.FillRectangle(body, 8, 3, 2, 2);           // Shutter slot
                g.FillRectangle(white, 4, 9, 8, 5);          // Label
            }
        }

        /// <summary>
        /// Arrow leaving an open tray.
        /// </summary>
        private static void DrawExport(Graphics g)
        {
            using (var tray = new Pen(Color.FromArgb(96, 96, 96), 1.6f) { LineJoin = LineJoin.Round })
            using (var arrow = new Pen(Green, 2f))
            using (var head = new SolidBrush(Green))
            {
                g.DrawLines(tray, new[] { new PointF(2f, 8.5f), new PointF(2f, 14f), new PointF(14f, 14f), new PointF(14f, 8.5f) });
                g.DrawLine(arrow, 8f, 11f, 8f, 5f);
                g.FillPolygon(head, new[] { new PointF(8f, 0.5f), new PointF(12.5f, 6f), new PointF(3.5f, 6f) });
            }
        }
    }
}
