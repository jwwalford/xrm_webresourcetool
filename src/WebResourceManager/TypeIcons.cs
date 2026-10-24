using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using WebResourceManager.Models;

namespace WebResourceManager
{
    /// <summary>
    /// Builds a small coloured badge (e.g. "PNG", "JS") for each web resource type.
    /// Badges are drawn at runtime so there are no image resources to maintain.
    /// </summary>
    internal static class TypeIcons
    {
        public static readonly Size BadgeSize = new Size(30, 16);

        private static readonly Dictionary<WebResourceType, Tuple<string, Color>> Badges = new Dictionary<WebResourceType, Tuple<string, Color>>
        {
            { WebResourceType.Html, Badge("HTML", 227, 76, 38) },
            { WebResourceType.Css, Badge("CSS", 38, 77, 228) },
            { WebResourceType.JavaScript, Badge("JS", 214, 170, 0) },
            { WebResourceType.Xml, Badge("XML", 0, 128, 128) },
            { WebResourceType.Png, Badge("PNG", 46, 139, 87) },
            { WebResourceType.Jpg, Badge("JPG", 34, 120, 160) },
            { WebResourceType.Gif, Badge("GIF", 142, 68, 173) },
            { WebResourceType.Silverlight, Badge("XAP", 112, 112, 112) },
            { WebResourceType.Xsl, Badge("XSL", 0, 100, 90) },
            { WebResourceType.Ico, Badge("ICO", 96, 125, 139) },
            { WebResourceType.Svg, Badge("SVG", 255, 140, 0) },
            { WebResourceType.Resx, Badge("RESX", 104, 33, 122) }
        };

        /// <summary>
        /// Creates an image list keyed by the type's integer value as a string.
        /// </summary>
        public static ImageList CreateImageList()
        {
            var list = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = BadgeSize
            };

            foreach (var type in WebResourceTypeInfo.All)
            {
                list.Images.Add(Key(type), Draw(type));
            }

            return list;
        }

        public static string Key(WebResourceType type)
        {
            return ((int)type).ToString();
        }

        public static Bitmap Draw(WebResourceType type)
        {
            Tuple<string, Color> badge;
            if (!Badges.TryGetValue(type, out badge))
            {
                badge = Badge("?", 128, 128, 128);
            }

            var bitmap = new Bitmap(BadgeSize.Width, BadgeSize.Height);
            using (var g = Graphics.FromImage(bitmap))
            using (var path = RoundedRectangle(new Rectangle(0, 1, BadgeSize.Width - 1, BadgeSize.Height - 3), 3))
            using (var fill = new SolidBrush(badge.Item2))
            using (var font = new Font("Segoe UI", badge.Item1.Length > 3 ? 6f : 6.75f, FontStyle.Bold, GraphicsUnit.Point))
            using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.FillPath(fill, path);
                g.DrawString(badge.Item1, font, Brushes.White, new RectangleF(0, 1, BadgeSize.Width, BadgeSize.Height - 2), format);
            }

            return bitmap;
        }

        /// <summary>
        /// A document glyph (page with a folded corner) in a badge-sized slot, for
        /// filters that mix text and image types.
        /// </summary>
        public static Bitmap DrawDocument()
        {
            var bitmap = new Bitmap(BadgeSize.Width, BadgeSize.Height);
            using (var g = Graphics.FromImage(bitmap))
            using (var page = new GraphicsPath())
            using (var fill = new SolidBrush(Color.FromArgb(232, 240, 250)))
            using (var outline = new Pen(Color.FromArgb(38, 77, 228), 1f))
            using (var lines = new Pen(Color.FromArgb(120, 140, 180), 1f))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // 11 x 14 page centred in the slot, top-right corner folded
                const float left = 9.5f, top = 0.5f, right = 20.5f, bottom = 14.5f, fold = 4f;
                page.AddLine(left, top, right - fold, top);
                page.AddLine(right - fold, top, right, top + fold);
                page.AddLine(right, top + fold, right, bottom);
                page.AddLine(right, bottom, left, bottom);
                page.CloseFigure();
                g.FillPath(fill, page);
                g.DrawPath(outline, page);
                g.DrawLines(outline, new[] { new PointF(right - fold, top), new PointF(right - fold, top + fold), new PointF(right, top + fold) });

                g.SmoothingMode = SmoothingMode.None;
                for (var y = 7; y <= 12; y += 2)
                {
                    g.DrawLine(lines, left + 2, y, right - 2, y);
                }
            }

            return bitmap;
        }

        /// <summary>
        /// A picture glyph (frame with a sun and hills) in a badge-sized slot, for
        /// filters that cover every image type.
        /// </summary>
        public static Bitmap DrawPicture()
        {
            var bitmap = new Bitmap(BadgeSize.Width, BadgeSize.Height);
            using (var g = Graphics.FromImage(bitmap))
            using (var sky = new SolidBrush(Color.FromArgb(214, 236, 250)))
            using (var frame = new Pen(Color.FromArgb(46, 139, 87), 1f))
            using (var sun = new SolidBrush(Color.FromArgb(255, 180, 0)))
            using (var hills = new SolidBrush(Color.FromArgb(46, 139, 87)))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // 16 x 13 frame centred in the slot
                var bounds = new RectangleF(7.5f, 1.5f, 15f, 12f);
                g.FillRectangle(sky, bounds);
                g.FillEllipse(sun, 17f, 3.5f, 3.5f, 3.5f);
                g.FillPolygon(hills, new[]
                {
                    new PointF(bounds.Left, bounds.Bottom), new PointF(12.5f, 7f), new PointF(16f, 10.5f),
                    new PointF(18f, 8.5f), new PointF(bounds.Right, bounds.Bottom)
                });
                g.DrawRectangle(frame, bounds.X, bounds.Y, bounds.Width, bounds.Height);
            }

            return bitmap;
        }

        private static Tuple<string, Color> Badge(string label, int r, int g, int b)
        {
            return Tuple.Create(label, Color.FromArgb(r, g, b));
        }

        private static GraphicsPath RoundedRectangle(Rectangle bounds, int radius)
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
    }
}
