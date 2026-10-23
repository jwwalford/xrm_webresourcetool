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
