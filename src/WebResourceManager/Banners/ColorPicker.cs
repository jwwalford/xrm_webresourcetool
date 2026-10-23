using System;
using System.Drawing;
using System.Windows.Forms;

namespace WebResourceManager.Banners
{
    /// <summary>
    /// A colour swatch button (opens the colour dialog) next to a hex text box.
    /// </summary>
    internal class ColorPicker : FlowLayoutPanel
    {
        private readonly Button swatch;
        private readonly TextBox hex;
        private Color color;
        private bool updating;

        public ColorPicker(Color initial)
        {
            AutoSize = true;
            WrapContents = false;
            Margin = new Padding(0);

            swatch = new Button
            {
                Size = new Size(32, 23),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(3, 3, 6, 3),
                UseVisualStyleBackColor = false
            };
            swatch.FlatAppearance.BorderColor = Color.Gray;
            swatch.Click += Swatch_Click;

            hex = new TextBox { Width = 80, CharacterCasing = CharacterCasing.Upper, MaxLength = 7 };
            hex.TextChanged += Hex_TextChanged;
            hex.Leave += (s, e) => SetHexText();

            Controls.Add(swatch);
            Controls.Add(hex);

            Color = initial;
        }

        public event EventHandler ColorChanged;

        public Color Color
        {
            get { return color; }
            set
            {
                color = Color.FromArgb(value.R, value.G, value.B);
                swatch.BackColor = color;
                SetHexText();
                ColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void SetHexText()
        {
            updating = true;
            hex.Text = BannerHtmlBuilder.ToHex(color);
            hex.BackColor = SystemColors.Window;
            updating = false;
        }

        private void Hex_TextChanged(object sender, EventArgs e)
        {
            if (updating)
            {
                return;
            }

            Color parsed;
            if (BannerHtmlBuilder.TryParseHex(hex.Text, out parsed))
            {
                hex.BackColor = SystemColors.Window;
                color = parsed;
                swatch.BackColor = parsed;
                ColorChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                hex.BackColor = Color.MistyRose;
            }
        }

        private void Swatch_Click(object sender, EventArgs e)
        {
            using (var dialog = new ColorDialog { Color = color, FullOpen = true, AnyColor = true })
            {
                if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
                {
                    Color = dialog.Color;
                }
            }
        }
    }
}
