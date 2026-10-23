using System.Drawing;

namespace WebResourceManager.Banners
{
    public enum BannerAlignment
    {
        Left,
        Center
    }

    public class BannerSettings
    {
        public BannerSettings()
        {
            // Defaults match the classic dark form banner: black bar, light blue icon, white text
            Text = "My Banner";
            BackgroundColor = Color.Black;
            TextColor = Color.White;
            IconColor = Color.FromArgb(0x9D, 0xD9, 0xF3);
            Icon = BannerIcon.All[1];
            IconFilled = true;
            FontSize = 22;
            Alignment = BannerAlignment.Left;
        }

        public string Title { get; set; }

        public string Text { get; set; }

        public Color BackgroundColor { get; set; }

        public Color TextColor { get; set; }

        public BannerIcon Icon { get; set; }

        public Color IconColor { get; set; }

        /// <summary>
        /// Fills the icon shape with a translucent tint of the icon colour.
        /// </summary>
        public bool IconFilled { get; set; }

        public int FontSize { get; set; }

        public bool Bold { get; set; }

        public BannerAlignment Alignment { get; set; }
    }
}
