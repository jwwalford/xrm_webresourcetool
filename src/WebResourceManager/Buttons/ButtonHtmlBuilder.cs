using System.Globalization;
using System.Net;
using System.Text;
using WebResourceManager.Banners;

namespace WebResourceManager.Buttons
{
    /// <summary>
    /// Builds a self-contained HTML page for a form button. The page reaches the
    /// form through parent.Xrm, which works because web resources on a form are
    /// served from the same origin as the form.
    /// </summary>
    public static class ButtonHtmlBuilder
    {
        public static string Build(ButtonSettings settings)
        {
            var fill = settings.SizeMode == ButtonSizeMode.Fill;

            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset=\"utf-8\">");
            html.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
            html.AppendFormat("<title>{0}</title>", WebUtility.HtmlEncode(settings.Title ?? settings.Text ?? "Button")).AppendLine();
            html.AppendLine("<style>");
            html.AppendLine("html, body { margin: 0; padding: 0; height: 100%; overflow: hidden; background: transparent; }");
            html.AppendFormat("body {{ display: flex; align-items: center; justify-content: {0}; box-sizing: border-box; padding: {1}; }}",
                JustifyContent(settings.Position), fill ? "0" : "4px").AppendLine();
            html.AppendFormat(
                "button {{ display: inline-flex; align-items: center; justify-content: center; gap: 0.5em; box-sizing: border-box; width: {0}; height: {1}; max-width: 100%; max-height: 100%; padding: 0 16px; border: none; border-radius: {2}px; background: {3}; color: {4}; font-family: 'Segoe UI', 'Segoe UI Web (West European)', -apple-system, BlinkMacSystemFont, Roboto, 'Helvetica Neue', sans-serif; font-size: {5}px; font-weight: {6}; cursor: pointer; transition: filter 0.15s; }}",
                fill ? "100%" : settings.Width + "px",
                fill ? "100%" : settings.Height + "px",
                settings.CornerRadius,
                BannerHtmlBuilder.ToHex(settings.BackgroundColor),
                BannerHtmlBuilder.ToHex(settings.TextColor),
                settings.FontSize,
                settings.Bold ? 600 : 400).AppendLine();
            html.AppendLine("button:hover { filter: brightness(0.9); }");
            html.AppendLine("button:active { filter: brightness(0.8); }");
            html.AppendLine("button:focus-visible { outline: 2px solid #000; outline-offset: 2px; }");
            html.AppendFormat("button svg {{ flex: none; width: 1.3em; height: 1.3em; color: {0}; }}", BannerHtmlBuilder.ToHex(settings.IconColor)).AppendLine();
            html.AppendLine("button span { white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            html.Append("<button type=\"button\" id=\"button\"");
            if (!string.IsNullOrWhiteSpace(settings.Tooltip))
            {
                html.AppendFormat(" title=\"{0}\"", WebUtility.HtmlEncode(settings.Tooltip.Trim()));
            }
            html.AppendLine(">");

            var icon = settings.Icon ?? BannerIcon.None;
            if (!string.IsNullOrEmpty(icon.Markup))
            {
                html.AppendLine(BannerHtmlBuilder.BuildSvg(icon, settings.IconFilled, null));
            }

            if (!string.IsNullOrEmpty(settings.Text))
            {
                html.AppendFormat("<span>{0}</span>", WebUtility.HtmlEncode(settings.Text)).AppendLine();
            }

            html.AppendLine("</button>");

            var action = BuildAction(settings);
            if (action != null)
            {
                html.AppendLine("<script>");
                html.AppendLine("document.getElementById(\"button\").addEventListener(\"click\", function () {");
                html.AppendLine("  var Xrm = window.parent && window.parent.Xrm;");
                html.AppendLine("  var formContext = Xrm && Xrm.Page;");
                html.AppendLine("  try {");
                foreach (var line in action.Replace("\r\n", "\n").Split('\n'))
                {
                    html.Append("    ").AppendLine(line);
                }
                html.AppendLine("  } catch (e) {");
                html.AppendLine("    console.error(e);");
                html.AppendLine("    if (Xrm && Xrm.Navigation) { Xrm.Navigation.openErrorDialog({ message: e.message }); }");
                html.AppendLine("  }");
                html.AppendLine("});");
                html.AppendLine("</script>");
            }

            html.AppendLine("</body>");
            html.AppendLine("</html>");
            return html.ToString();
        }

        /// <summary>
        /// The body of the click handler, or null when the button has no action.
        /// </summary>
        private static string BuildAction(ButtonSettings settings)
        {
            switch (settings.Action)
            {
                case ButtonAction.CustomScript:
                    // A literal "</script" would end the script block early
                    return string.IsNullOrWhiteSpace(settings.Script)
                        ? null
                        : settings.Script.TrimEnd().Replace("</script", "<\\/script");
                case ButtonAction.OpenUrl:
                    return string.Format("window.open({0}, \"_blank\");", ToJsString((settings.Url ?? string.Empty).Trim()));
                case ButtonAction.SaveRecord:
                    return "if (formContext) { formContext.data.save(); }";
                default:
                    return null;
            }
        }

        private static string JustifyContent(ButtonPosition position)
        {
            switch (position)
            {
                case ButtonPosition.Center:
                    return "center";
                case ButtonPosition.Right:
                    return "flex-end";
                default:
                    return "flex-start";
            }
        }

        /// <summary>
        /// A double-quoted JavaScript string literal that is also safe inside a script tag.
        /// </summary>
        private static string ToJsString(string value)
        {
            var result = new StringBuilder("\"");
            foreach (var c in value)
            {
                switch (c)
                {
                    case '\\': result.Append("\\\\"); break;
                    case '"': result.Append("\\\""); break;
                    case '\r': result.Append("\\r"); break;
                    case '\n': result.Append("\\n"); break;
                    case '<': result.Append("\\x3c"); break;
                    default:
                        if (c < 0x20)
                        {
                            result.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            result.Append(c);
                        }
                        break;
                }
            }

            return result.Append('"').ToString();
        }
    }
}
