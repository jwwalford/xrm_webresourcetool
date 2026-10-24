using System.Drawing;
using System.Globalization;
using System.Net;
using System.Text;
using WebResourceManager.Banners;

namespace WebResourceManager.Images
{
    internal enum SvgIconBackground
    {
        None,
        Circle,
        RoundedSquare,
        Square
    }

    internal class SvgIconSettings
    {
        public BannerIcon Icon { get; set; } = IconGallery.FindByKey("document");

        public Color IconColor { get; set; } = Color.FromArgb(0x00, 0x7A, 0xCC);

        public bool Filled { get; set; }

        public decimal StrokeWidth { get; set; } = 2m;

        public SvgIconBackground Background { get; set; } = SvgIconBackground.None;

        public Color BackgroundColor { get; set; } = Color.FromArgb(222, 236, 249);

        /// <summary>
        /// Width and height written into the SVG, in px.
        /// </summary>
        public int Size { get; set; } = 32;
    }

    /// <summary>
    /// Builds a standalone SVG web resource from a gallery icon. Colours are
    /// written out in full (no currentColor) so the file looks the same wherever
    /// it is used, including in an img tag or as a ribbon or entity icon.
    /// </summary>
    internal static class SvgIconBuilder
    {
        public static readonly int[] PreviewSizes = { 16, 24, 32, 48, 64 };

        public static string Build(SvgIconSettings settings)
        {
            return Build(settings, settings.Size);
        }

        private static string Build(SvgIconSettings settings, int size)
        {
            var color = BannerHtmlBuilder.ToHex(settings.IconColor);
            var hasBackground = settings.Background != SvgIconBackground.None;

            // The icon is drawn on a 24-unit grid; a background adds a 4-unit margin round it
            var viewBox = hasBackground ? "-4 -4 32 32" : "0 0 24 24";
            var stroke = settings.StrokeWidth.ToString("0.##", CultureInfo.InvariantCulture);

            var svg = new StringBuilder();
            svg.AppendFormat("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{0}\" height=\"{0}\" viewBox=\"{1}\">", size, viewBox);
            svg.AppendFormat("<title>{0}</title>", WebUtility.HtmlEncode(settings.Icon.Label));

            if (hasBackground)
            {
                var background = BannerHtmlBuilder.ToHex(settings.BackgroundColor);
                switch (settings.Background)
                {
                    case SvgIconBackground.Circle:
                        svg.AppendFormat("<circle cx=\"12\" cy=\"12\" r=\"16\" fill=\"{0}\"/>", background);
                        break;
                    case SvgIconBackground.RoundedSquare:
                        svg.AppendFormat("<rect x=\"-4\" y=\"-4\" width=\"32\" height=\"32\" rx=\"7\" fill=\"{0}\"/>", background);
                        break;
                    default:
                        svg.AppendFormat("<rect x=\"-4\" y=\"-4\" width=\"32\" height=\"32\" fill=\"{0}\"/>", background);
                        break;
                }
            }

            svg.AppendFormat("<g fill=\"{0}\"{1} stroke=\"{2}\" stroke-width=\"{3}\" stroke-linecap=\"round\" stroke-linejoin=\"round\">",
                settings.Filled ? color : "none", settings.Filled ? " fill-opacity=\"0.35\"" : string.Empty, color, stroke);
            svg.Append(settings.Icon.Markup);
            svg.Append("</g></svg>");
            return svg.ToString();
        }

        /// <summary>
        /// Preview page: the SVG at its own size and the common icon sizes, on a
        /// light and a dark background.
        /// </summary>
        public static string BuildPreviewHtml(SvgIconSettings settings)
        {
            var html = new StringBuilder();
            html.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><style>");
            html.Append("body{margin:0;padding:8px;font-family:'Segoe UI',sans-serif;font-size:11px;color:#605e5c;background:#fff}");
            html.Append(".row{display:flex;flex-wrap:wrap;gap:8px;margin-bottom:8px}");
            html.Append(".tile{display:flex;flex-direction:column;align-items:center;justify-content:flex-end;gap:6px;padding:10px;border-radius:4px;min-width:56px}");
            html.Append(".light{background:#fff;border:1px solid #e1dfdd}.dark{background:#202020;color:#c8c6c4}");
            html.Append(".tile svg{display:block}h4{margin:2px 0 6px;font-size:11px;font-weight:600;text-transform:uppercase;letter-spacing:.04em}");
            html.Append("</style></head><body>");

            var sizes = new System.Collections.Generic.List<int>(PreviewSizes);
            if (!sizes.Contains(settings.Size))
            {
                sizes.Add(settings.Size);
                sizes.Sort();
            }

            foreach (var theme in new[] { "light", "dark" })
            {
                html.AppendFormat("<h4>On {0}</h4><div class=\"row\">", theme == "light" ? "light" : "dark");
                foreach (var size in sizes)
                {
                    html.AppendFormat("<div class=\"tile {0}\">{1}<span>{2}px{3}</span></div>",
                        theme, Build(settings, size), size, size == settings.Size ? " (saved)" : string.Empty);
                }
                html.Append("</div>");
            }

            html.Append("</body></html>");
            return html.ToString();
        }
    }
}
