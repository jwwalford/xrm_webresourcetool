using System.Drawing;
using System.Net;
using System.Text;

namespace WebResourceManager.Banners
{
    /// <summary>
    /// Builds a self-contained HTML page for a banner. The banner fills the web
    /// resource frame, so its height follows the row count set on the form.
    /// </summary>
    public static class BannerHtmlBuilder
    {
        public static string Build(BannerSettings settings)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset=\"utf-8\">");
            html.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
            html.AppendFormat("<title>{0}</title>", WebUtility.HtmlEncode(settings.Title ?? settings.Text ?? "Banner")).AppendLine();
            html.AppendLine("<style>");
            html.AppendLine("html, body { margin: 0; padding: 0; height: 100%; overflow: hidden; }");
            html.AppendFormat(
                ".banner {{ display: flex; align-items: center; justify-content: {0}; gap: 0.5em; box-sizing: border-box; height: 100%; min-height: 32px; padding: 4px 16px; background: {1}; color: {2}; font-family: 'Segoe UI', 'Segoe UI Web (West European)', -apple-system, BlinkMacSystemFont, Roboto, 'Helvetica Neue', sans-serif; font-size: {3}px; font-weight: {4}; }}",
                settings.Alignment == BannerAlignment.Center ? "center" : "flex-start",
                ToHex(settings.BackgroundColor),
                ToHex(settings.TextColor),
                settings.FontSize,
                settings.Bold ? 600 : 400).AppendLine();
            html.AppendFormat(".banner svg {{ flex: none; width: 1.3em; height: 1.3em; color: {0}; }}", ToHex(settings.IconColor)).AppendLine();
            html.AppendLine(".banner span { white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("<div class=\"banner\">");

            var icon = settings.Icon ?? BannerIcon.None;
            if (!string.IsNullOrEmpty(icon.Markup))
            {
                html.AppendLine(BuildSvg(icon, settings.IconFilled, null));
            }

            html.AppendFormat("<span>{0}</span>", WebUtility.HtmlEncode(settings.Text ?? string.Empty)).AppendLine();
            html.AppendLine("</div>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            return html.ToString();
        }

        /// <summary>
        /// Builds the standalone SVG for an icon. Size is optional; without it the
        /// SVG takes its size from CSS.
        /// </summary>
        public static string BuildSvg(BannerIcon icon, bool filled, int? size)
        {
            var fill = filled ? "currentColor\" fill-opacity=\"0.35" : "none";
            var dimensions = size.HasValue ? string.Format(" width=\"{0}\" height=\"{0}\"", size.Value) : string.Empty;
            return string.Format(
                "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\"{0} fill=\"{1}\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\">{2}</svg>",
                dimensions, fill, icon.Markup);
        }

        public static string ToHex(Color color)
        {
            return string.Format("#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
        }

        public static bool TryParseHex(string text, out Color color)
        {
            color = Color.Empty;
            var value = (text ?? string.Empty).Trim().TrimStart('#');
            if (value.Length == 3)
            {
                value = new string(new[] { value[0], value[0], value[1], value[1], value[2], value[2] });
            }

            int rgb;
            if (value.Length != 6 || !int.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out rgb))
            {
                return false;
            }

            color = Color.FromArgb((rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
            return true;
        }
    }
}
