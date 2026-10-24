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
