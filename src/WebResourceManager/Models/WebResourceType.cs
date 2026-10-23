using System.Collections.Generic;
using System.Linq;

namespace WebResourceManager.Models
{
    /// <summary>
    /// Values of the webresource.webresourcetype option set.
    /// </summary>
    public enum WebResourceType
    {
        Html = 1,
        Css = 2,
        JavaScript = 3,
        Xml = 4,
        Png = 5,
        Jpg = 6,
        Gif = 7,
        Silverlight = 8,
        Xsl = 9,
        Ico = 10,
        Svg = 11,
        Resx = 12
    }

    public enum PreviewKind
    {
        None,
        Image,
        Browser,
        Text
    }

    public static class WebResourceTypeInfo
    {
        private static readonly Dictionary<WebResourceType, string> Labels = new Dictionary<WebResourceType, string>
        {
            { WebResourceType.Html, "Webpage (HTML)" },
            { WebResourceType.Css, "Style Sheet (CSS)" },
            { WebResourceType.JavaScript, "Script (JScript)" },
            { WebResourceType.Xml, "Data (XML)" },
            { WebResourceType.Png, "PNG format" },
            { WebResourceType.Jpg, "JPG format" },
            { WebResourceType.Gif, "GIF format" },
            { WebResourceType.Silverlight, "Silverlight (XAP)" },
            { WebResourceType.Xsl, "Style Sheet (XSL)" },
            { WebResourceType.Ico, "ICO format" },
            { WebResourceType.Svg, "Vector format (SVG)" },
            { WebResourceType.Resx, "String (RESX)" }
        };

        private static readonly Dictionary<WebResourceType, string> Extensions = new Dictionary<WebResourceType, string>
        {
            { WebResourceType.Html, ".html" },
            { WebResourceType.Css, ".css" },
            { WebResourceType.JavaScript, ".js" },
            { WebResourceType.Xml, ".xml" },
            { WebResourceType.Png, ".png" },
            { WebResourceType.Jpg, ".jpg" },
            { WebResourceType.Gif, ".gif" },
            { WebResourceType.Silverlight, ".xap" },
            { WebResourceType.Xsl, ".xsl" },
            { WebResourceType.Ico, ".ico" },
            { WebResourceType.Svg, ".svg" },
            { WebResourceType.Resx, ".resx" }
        };

        public static string GetLabel(WebResourceType type)
        {
            string label;
            return Labels.TryGetValue(type, out label) ? label : type.ToString();
        }

        public static string GetExtension(WebResourceType type)
        {
            string ext;
            return Extensions.TryGetValue(type, out ext) ? ext : string.Empty;
        }

        public static PreviewKind GetPreviewKind(WebResourceType type)
        {
            switch (type)
            {
                case WebResourceType.Png:
                case WebResourceType.Jpg:
                case WebResourceType.Gif:
                case WebResourceType.Ico:
                    return PreviewKind.Image;
                case WebResourceType.Html:
                case WebResourceType.Svg:
                    return PreviewKind.Browser;
                case WebResourceType.Css:
                case WebResourceType.JavaScript:
                case WebResourceType.Xml:
                case WebResourceType.Xsl:
                case WebResourceType.Resx:
                    return PreviewKind.Text;
                default:
                    return PreviewKind.None;
            }
        }

        public static IEnumerable<WebResourceType> All
        {
            get { return Labels.Keys.OrderBy(t => (int)t); }
        }
    }

    /// <summary>
    /// An entry in the type filter drop-down. An empty Types array means "all types".
    /// </summary>
    public class WebResourceTypeFilter
    {
        public WebResourceTypeFilter(string label, params WebResourceType[] types)
        {
            Label = label;
            Types = types;
        }

        public string Label { get; private set; }

        public WebResourceType[] Types { get; private set; }

        public override string ToString()
        {
            return Label;
        }

        /// <summary>
        /// The explorer focuses on HTML, CSS and images, so only those types are offered.
        /// </summary>
        public static IEnumerable<WebResourceTypeFilter> Defaults()
        {
            yield return new WebResourceTypeFilter("HTML, CSS and images", ExplorerTypes);
            yield return new WebResourceTypeFilter(WebResourceTypeInfo.GetLabel(WebResourceType.Html), WebResourceType.Html);
            yield return new WebResourceTypeFilter(WebResourceTypeInfo.GetLabel(WebResourceType.Css), WebResourceType.Css);
            yield return new WebResourceTypeFilter("All images (PNG, JPG, GIF, ICO, SVG)", ImageTypes);
            foreach (var type in ImageTypes)
            {
                yield return new WebResourceTypeFilter(WebResourceTypeInfo.GetLabel(type), type);
            }
        }

        private static readonly WebResourceType[] ImageTypes =
        {
            WebResourceType.Png, WebResourceType.Jpg, WebResourceType.Gif, WebResourceType.Ico, WebResourceType.Svg
        };

        private static readonly WebResourceType[] ExplorerTypes =
        {
            WebResourceType.Html, WebResourceType.Css,
            WebResourceType.Png, WebResourceType.Jpg, WebResourceType.Gif, WebResourceType.Ico, WebResourceType.Svg
        };
    }
}
