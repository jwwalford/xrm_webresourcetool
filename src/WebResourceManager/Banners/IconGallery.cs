using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;

namespace WebResourceManager.Banners
{
    /// <summary>
    /// The clickable icon grid shown in a WebView2 by the banner and button
    /// designers. Clicking an icon posts its key back through the WebView2
    /// message channel; <see cref="FindByKey"/> turns it back into an icon.
    /// </summary>
    internal static class IconGallery
    {
        /// <summary>
        /// Builds the grid, drawn in the current colours so it previews the result.
        /// The "None" choice is left out when includeNone is false.
        /// </summary>
        public static string BuildHtml(BannerIcon selectedIcon, bool filled, Color background, Color text, Color iconColor, bool includeNone = true)
        {
            var html = new StringBuilder();
            html.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><style>");
            html.Append("body{margin:0;padding:6px;font-family:'Segoe UI',sans-serif;font-size:11px;background:#fff}");
            html.Append(".grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(76px,1fr));gap:6px}");
            html.AppendFormat("button{{display:flex;flex-direction:column;align-items:center;gap:4px;padding:8px 4px;border:2px solid transparent;border-radius:6px;background:{0};color:{1};cursor:pointer;font:inherit}}",
                BannerHtmlBuilder.ToHex(background), BannerHtmlBuilder.ToHex(text));
            html.Append("button:hover{border-color:#8a8886}button.selected{border-color:#0078d4;box-shadow:0 0 0 2px #c7e0f4}");
            html.AppendFormat("button svg{{color:{0}}}", BannerHtmlBuilder.ToHex(iconColor));
            html.Append(".none{width:28px;height:28px;display:flex;align-items:center;justify-content:center;font-size:18px;opacity:.6}");
            html.Append("h4{margin:8px 2px 4px;font-size:11px;font-weight:600;color:#605e5c;text-transform:uppercase;letter-spacing:.04em}h4:first-child{margin-top:0}");
            html.Append("</style></head><body>");

            // Grouped by category, in the order categories first appear in the list
            foreach (var group in BannerIcon.All.Where(i => includeNone || i != BannerIcon.None).GroupBy(i => i.Category))
            {
                html.AppendFormat("<h4>{0}</h4><div class=\"grid\">", WebUtility.HtmlEncode(group.Key));
                foreach (var icon in group)
                {
                    var selected = selectedIcon == icon || (selectedIcon == null && icon == BannerIcon.None);
                    html.AppendFormat("<button type=\"button\"{0} title=\"{1}\" onclick=\"window.chrome.webview.postMessage('{2}')\">",
                        selected ? " class=\"selected\"" : string.Empty, WebUtility.HtmlEncode(icon.Label), icon.Key);
                    html.Append(string.IsNullOrEmpty(icon.Markup)
                        ? "<span class=\"none\">&#8856;</span>"
                        : BannerHtmlBuilder.BuildSvg(icon, filled, 28));
                    html.AppendFormat("<span>{0}</span></button>", WebUtility.HtmlEncode(icon == BannerIcon.None ? "None" : icon.Label));
                }

                html.Append("</div>");
            }

            html.Append("</body></html>");
            return html.ToString();
        }

        public static BannerIcon FindByKey(string key)
        {
            return BannerIcon.All.FirstOrDefault(i => i.Key == key);
        }
    }
}
