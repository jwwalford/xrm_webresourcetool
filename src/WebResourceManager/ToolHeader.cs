using System.Drawing;
using System.Windows.Forms;

namespace WebResourceManager
{
    /// <summary>
    /// The white header at the top of a tool page: the icon, then the bold title
    /// with the description underneath, and the connection status on the right.
    /// A table lays them out, so they line up at any display scaling.
    /// </summary>
    internal class ToolHeader : Panel
    {
        private static readonly Color ConnectedColor = Color.FromArgb(16, 124, 16);
        private static readonly Color DisconnectedColor = Color.FromArgb(164, 38, 44);

        private readonly Label lblConnection;

        public ToolHeader(string title, string description, Image icon, int iconSize)
        {
            Dock = DockStyle.Top;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BackColor = Color.White;
            Padding = new Padding(12, 6, 12, 6);

            var table = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 3, RowCount = 2, Margin = new Padding(0) };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var picture = new PictureBox
            {
                Image = icon,
                SizeMode = PictureBoxSizeMode.CenterImage,
                Size = new Size(iconSize, iconSize),
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 0, 8, 0)
            };
            var titleLabel = new Label
            {
                Text = title,
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 2)
            };
            var descriptionLabel = new Label
            {
                Text = description,
                AutoSize = true,
                ForeColor = Color.DimGray,
                Margin = new Padding(0)
            };
            lblConnection = new Label
            {
                AutoSize = true,
                Anchor = AnchorStyles.Right,
                Margin = new Padding(12, 0, 0, 0)
            };

            table.Controls.Add(picture, 0, 0);
            table.Controls.Add(titleLabel, 1, 0);
            table.Controls.Add(descriptionLabel, 1, 1);
            table.Controls.Add(lblConnection, 2, 0);
            table.SetRowSpan(lblConnection, 2);
            Controls.Add(table);

            SetConnection(false, null);
        }

        /// <summary>
        /// Shows "Connected to [org]" in green, or "Not connected" in red.
        /// </summary>
        public void SetConnection(bool connected, string connectionName)
        {
            if (connected)
            {
                lblConnection.Text = "●  Connected to " + (string.IsNullOrEmpty(connectionName) ? "Dataverse" : connectionName);
                lblConnection.ForeColor = ConnectedColor;
            }
            else
            {
                lblConnection.Text = "●  Not connected";
                lblConnection.ForeColor = DisconnectedColor;
            }
        }
    }
}
