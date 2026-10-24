using System;
using System.Drawing;
using System.Windows.Forms;

namespace WebResourceManager.Tools
{
    /// <summary>
    /// Progress bar shown next to a tool's Save button while a web resource is
    /// checked, saved and published. It writes each step to the tool's status
    /// label and hides itself shortly after the save finishes.
    /// </summary>
    internal class SaveProgress : ProgressBar
    {
        private const string FailedText = "Save failed";

        private readonly Label statusLabel;
        private readonly Timer hideTimer = new Timer { Interval = 2500 };

        public SaveProgress(Label statusLabel)
        {
            this.statusLabel = statusLabel;

            Minimum = 0;
            Maximum = 100;
            Style = ProgressBarStyle.Continuous;
            Size = new Size(160, 16);
            Margin = new Padding(3, 9, 8, 3);
            Visible = false;

            // Only the failure message is red; anything written after it (e.g. an export) goes back to grey
            statusLabel.TextChanged += (s, e) =>
            {
                if (statusLabel.Text != FailedText)
                {
                    statusLabel.ForeColor = Color.DimGray;
                }
            };

            hideTimer.Tick += (s, e) =>
            {
                hideTimer.Stop();
                Visible = false;
            };
        }

        /// <summary>
        /// Shows a step: its status text and how far through the save it is (0 to 100).
        /// </summary>
        public void Report(string status, int percent)
        {
            hideTimer.Stop();
            statusLabel.Text = status;
            Value = Math.Max(Minimum, Math.Min(Maximum, percent));
            Visible = true;
        }

        public void Complete(string status)
        {
            Report(status, 100);
            hideTimer.Start();
        }

        /// <summary>
        /// The user cancelled (e.g. declined to replace): clear the bar and the status.
        /// </summary>
        public void Reset()
        {
            hideTimer.Stop();
            Visible = false;
            Value = 0;
            statusLabel.Text = string.Empty;
        }

        public void Fail()
        {
            hideTimer.Stop();
            Visible = false;
            Value = 0;
            statusLabel.ForeColor = Color.Firebrick;
            statusLabel.Text = FailedText;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                hideTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
