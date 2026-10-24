using System.Drawing;
using WebResourceManager.Banners;

namespace WebResourceManager.Buttons
{
    public enum ButtonSizeMode
    {
        /// <summary>The button stretches to fill the web resource control.</summary>
        Fill,

        /// <summary>The button has a fixed width and height inside the control.</summary>
        Fixed
    }

    public enum ButtonPosition
    {
        Left,
        Center,
        Right
    }

    public enum ButtonAction
    {
        /// <summary>No click handler; wire one up yourself later.</summary>
        None,

        /// <summary>Runs JavaScript with Xrm and formContext available.</summary>
        CustomScript,

        /// <summary>Opens a URL in a new tab.</summary>
        OpenUrl,

        /// <summary>Saves the record the form is showing.</summary>
        SaveRecord
    }

    public class ButtonSettings
    {
        public const string DefaultScript =
            "// Xrm and formContext are available here.\r\n" +
            "Xrm.Navigation.openAlertDialog({ text: \"Button clicked on \" + formContext.data.entity.getEntityName() });\r\n";

        public ButtonSettings()
        {
            Text = "Click me";
            BackgroundColor = Color.FromArgb(0x00, 0x7A, 0xCC);
            TextColor = Color.White;
            IconColor = Color.White;
            Icon = BannerIcon.None;
            FontSize = 14;
            Bold = true;
            CornerRadius = 4;
            SizeMode = ButtonSizeMode.Fill;
            Width = 160;
            Height = 36;
            Position = ButtonPosition.Left;
            Action = ButtonAction.CustomScript;
            Script = DefaultScript;
            Url = "https://";
        }

        public string Title { get; set; }

        public string Text { get; set; }

        /// <summary>Shown when the mouse rests on the button. Optional.</summary>
        public string Tooltip { get; set; }

        public Color BackgroundColor { get; set; }

        public Color TextColor { get; set; }

        public BannerIcon Icon { get; set; }

        public Color IconColor { get; set; }

        public bool IconFilled { get; set; }

        public int FontSize { get; set; }

        public bool Bold { get; set; }

        /// <summary>Corner radius in pixels. 0 is square; a large value gives a pill shape.</summary>
        public int CornerRadius { get; set; }

        public ButtonSizeMode SizeMode { get; set; }

        /// <summary>Width in pixels when <see cref="SizeMode"/> is Fixed.</summary>
        public int Width { get; set; }

        /// <summary>Height in pixels when <see cref="SizeMode"/> is Fixed.</summary>
        public int Height { get; set; }

        /// <summary>Where a fixed-size button sits across the control.</summary>
        public ButtonPosition Position { get; set; }

        public ButtonAction Action { get; set; }

        public string Script { get; set; }

        public string Url { get; set; }
    }
}
