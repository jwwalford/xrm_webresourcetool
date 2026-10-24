using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.WinForms;

namespace WebResourceManager.Banners
{
    /// <summary>
    /// Loads the icon gallery into its WebView2. The gallery is rebuilt whenever the
    /// selection or colours change, so this carries the scroll position across the
    /// reload instead of jumping back to the top.
    /// </summary>
    internal static class IconGalleryView
    {
        public static async Task ShowAsync(WebView2 gallery, string html)
        {
            var scrollTop = 0d;
            try
            {
                var result = await gallery.CoreWebView2.ExecuteScriptAsync("window.scrollY");
                double.TryParse(result, NumberStyles.Float, CultureInfo.InvariantCulture, out scrollTop);
            }
            catch (Exception)
            {
                // Nothing loaded yet, so start at the top
            }

            if (scrollTop > 0)
            {
                html = html.Replace("</body>", "<script>window.scrollTo(0," + scrollTop.ToString(CultureInfo.InvariantCulture) + ")</script></body>");
            }

            gallery.CoreWebView2.NavigateToString(html);
        }
    }
}
