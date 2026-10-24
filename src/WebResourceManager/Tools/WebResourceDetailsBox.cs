using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebResourceManager.Models;
using WebResourceManager.Services;

namespace WebResourceManager.Tools
{
    /// <summary>
    /// The "Web resource" group shared by the tools: solution (for the publisher
    /// prefix), name, display name and description. Loads the unmanaged solutions
    /// for the current connection.
    /// </summary>
    internal class WebResourceDetailsBox : GroupBox
    {
        // Web resource names allow letters, digits, underscore, hyphen, dot and forward slash
        private static readonly Regex ValidName = new Regex(@"^[A-Za-z0-9_\-\./]+$");

        // Runs of anything but letters and digits become one underscore in a generated name
        private static readonly Regex NonNameChars = new Regex(@"[^a-z0-9]+");

        private readonly string noun;
        private Microsoft.Xrm.Sdk.IOrganizationService service;
        private string[] extensions = { ".html", ".htm" };
        private bool solutionsRequested;

        // The name follows the display name until the user types in the name box
        private bool nameFollowsDisplayName = true;
        private bool settingName;

        private readonly ComboBox cbSolution;
        private readonly Label lblSolutionStatus;
        private readonly Label lblPrefix;
        private readonly TextBox txtName;
        private readonly Label lblFullName;
        private readonly TextBox txtDisplayName;
        private readonly TextBox txtDescription;

        /// <param name="noun">What's being created, for the hints, e.g. "banner".</param>
        public WebResourceDetailsBox(Microsoft.Xrm.Sdk.IOrganizationService service, string noun, string defaultName, string defaultDisplayName)
        {
            this.service = service;
            this.noun = noun;

            Text = "Web resource";
            Dock = DockStyle.Fill;
            Padding = new Padding(8);

            var table = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2 };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            cbSolution = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
            cbSolution.SelectedIndexChanged += (s, e) => { UpdateFullName(); OnStateChanged(); };
            lblSolutionStatus = new Label { AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 0, 3, 6), MaximumSize = new Size(320, 0) };

            lblPrefix = new Label { AutoSize = true, Margin = new Padding(3, 6, 0, 3), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtName = new TextBox { Width = 220, Text = defaultName };
            txtName.TextChanged += (s, e) =>
            {
                if (!settingName)
                {
                    // Typing a name by hand stops it following the display name; clearing it starts again
                    nameFollowsDisplayName = txtName.Text.Trim().Length == 0;
                }
                UpdateFullName();
            };
            var namePanel = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
            namePanel.Controls.Add(lblPrefix);
            namePanel.Controls.Add(txtName);
            lblFullName = new Label { AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 0, 3, 6), MaximumSize = new Size(320, 0) };

            txtDisplayName = new TextBox { Dock = DockStyle.Fill, Text = defaultDisplayName };
            txtDisplayName.TextChanged += (s, e) =>
            {
                if (nameFollowsDisplayName)
                {
                    NameFromDisplayName();
                }
                DisplayNameChanged?.Invoke(this, EventArgs.Empty);
            };
            txtDescription = new TextBox { Dock = DockStyle.Fill, Height = 48, Multiline = true, ScrollBars = ScrollBars.Vertical };

            AddRow(table, "Solution:", cbSolution);
            AddRow(table, string.Empty, lblSolutionStatus);
            AddRow(table, "Name:", namePanel);
            AddRow(table, string.Empty, lblFullName);
            AddRow(table, "Display name:", txtDisplayName);
            AddRow(table, "Description:", txtDescription);
            Controls.Add(table);

            UpdateFullName();
        }

        /// <summary>
        /// Raised when the solution list loads or the selection changes, so the owner can enable Save.
        /// </summary>
        public event EventHandler StateChanged;

        public event EventHandler DisplayNameChanged;

        /// <summary>
        /// Optional text for the line under the name, given the full name. Used when
        /// a tool saves more than one web resource from a single name.
        /// </summary>
        public Func<string, string> DescribeName { get; set; }

        /// <summary>
        /// Allowed file extensions; the first is appended to the name when it has none of them.
        /// </summary>
        public string[] Extensions
        {
            get { return extensions; }
            set
            {
                extensions = value;
                UpdateFullName();
            }
        }

        public SolutionOption SelectedSolution
        {
            get { return cbSolution.SelectedItem as SolutionOption; }
        }

        /// <summary>
        /// True when Save can be clicked. Without a connection Save stays available,
        /// so clicking it can ask XrmToolBox to connect (see <see cref="EnsureConnected"/>).
        /// </summary>
        public bool CanSave
        {
            get { return service == null || SelectedSolution != null; }
        }

        /// <summary>
        /// Raised by <see cref="EnsureConnected"/> when there's no connection.
        /// </summary>
        public event EventHandler ConnectionRequested;

        /// <summary>
        /// Returns true when connected. Otherwise asks the owner to open the
        /// connection dialog and returns false, so the save stops for now.
        /// </summary>
        public bool EnsureConnected()
        {
            if (service != null)
            {
                return true;
            }

            ConnectionRequested?.Invoke(this, EventArgs.Empty);
            return false;
        }

        public string DisplayName
        {
            get { return txtDisplayName.Text.Trim(); }
        }

        public string Description
        {
            get { return txtDescription.Text.Trim(); }
        }

        /// <summary>
        /// The full web resource name: publisher prefix + name + extension.
        /// </summary>
        public string FullName
        {
            get
            {
                var name = txtName.Text.Trim();
                var solution = SelectedSolution;
                if (solution != null && !string.IsNullOrEmpty(solution.Prefix) &&
                    !name.StartsWith(solution.Prefix + "_", StringComparison.OrdinalIgnoreCase))
                {
                    name = solution.Prefix + "_" + name;
                }

                if (!extensions.Any(ext => name.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                {
                    name += extensions[0];
                }

                return name;
            }
        }

        /// <summary>
        /// Checks the name and fills in a missing display name. Returns false and
        /// tells the user what's wrong if the name can't be used.
        /// </summary>
        public bool ValidateName(string caption)
        {
            var name = txtName.Text.Trim();
            if (name.Length == 0)
            {
                ShowValidation("Enter a name for the web resource.", caption);
                return false;
            }

            if (!ValidName.IsMatch(name))
            {
                ShowValidation("The name can only contain letters, numbers, underscores, hyphens, dots and forward slashes.", caption);
                return false;
            }

            if (DisplayName.Length == 0)
            {
                txtDisplayName.Text = name.Split('/').Last();
            }

            return true;
        }

        /// <summary>
        /// Replaces the name and display name, e.g. with a file's name when one is chosen.
        /// </summary>
        public void SetName(string name, string displayName)
        {
            SetNameText(name);
            nameFollowsDisplayName = true;
            txtDisplayName.Text = displayName;
        }

        /// <summary>
        /// Turns the display name into the last part of the name, keeping any folder:
        /// "Form Banner" becomes "banners/form_banner".
        /// </summary>
        private void NameFromDisplayName()
        {
            var slug = NonNameChars.Replace(txtDisplayName.Text.Trim().ToLowerInvariant(), "_").Trim('_');
            if (slug.Length == 0)
            {
                return;
            }

            var current = txtName.Text.Trim();
            var slash = current.LastIndexOf('/');
            SetNameText(slash >= 0 ? current.Substring(0, slash + 1) + slug : slug);
        }

        private void SetNameText(string name)
        {
            settingName = true;
            try
            {
                txtName.Text = name;
            }
            finally
            {
                settingName = false;
            }
        }

        /// <summary>
        /// Points the box at a new connection (or none) and reloads the solution list.
        /// </summary>
        public void SetService(Microsoft.Xrm.Sdk.IOrganizationService newService)
        {
            service = newService;
            if (solutionsRequested)
            {
                LoadSolutions();
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // The handle can be re-created; load the solutions only the first time
            if (!solutionsRequested)
            {
                solutionsRequested = true;
                LoadSolutions();
            }
        }

        private async void LoadSolutions()
        {
            cbSolution.Items.Clear();
            OnStateChanged();

            if (service == null)
            {
                lblSolutionStatus.Text = "Not connected to Dataverse. Save to Dataverse asks you to connect, or you can export the " + noun + " to disk.";
                cbSolution.Enabled = false;
                UpdateFullName();
                return;
            }

            var requested = service;
            lblSolutionStatus.Text = "Loading solutions...";
            cbSolution.Enabled = false;
            try
            {
                var solutions = await Task.Run(() => new WebResourceRepository(requested).GetUnmanagedSolutions());
                if (requested != service)
                {
                    // The connection changed while loading; a newer load is under way
                    return;
                }

                cbSolution.Items.AddRange(solutions.Cast<object>().ToArray());
                if (cbSolution.Items.Count > 0)
                {
                    cbSolution.SelectedIndex = 0;
                }

                cbSolution.Enabled = true;
                lblSolutionStatus.Text = "The publisher prefix comes from the solution. The Default solution doesn't add the " + noun + " to a solution.";
            }
            catch (Exception ex)
            {
                lblSolutionStatus.Text = "Could not load solutions: " + ex.Message;
            }

            UpdateFullName();
            OnStateChanged();
        }

        private void UpdateFullName()
        {
            var solution = SelectedSolution;
            lblPrefix.Text = solution != null && !string.IsNullOrEmpty(solution.Prefix) ? solution.Prefix + "_" : string.Empty;
            if (txtName.Text.Trim().Length == 0)
            {
                lblFullName.Text = string.Empty;
            }
            else
            {
                lblFullName.Text = DescribeName != null ? DescribeName(FullName) : "Web resource name: " + FullName;
            }
        }

        /// <summary>
        /// Refreshes the line under the name, e.g. after the owner changes what DescribeName returns.
        /// </summary>
        public void RefreshNameHint()
        {
            UpdateFullName();
        }

        private void OnStateChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ShowValidation(string message, string caption)
        {
            MessageBox.Show(this, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtName.Focus();
        }

        private static void AddRow(TableLayoutPanel table, string label, Control control)
        {
            var row = table.RowCount;
            table.RowCount = row + 1;
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(3, 7, 3, 3) }, 0, row);
            table.Controls.Add(control, 1, row);
        }
    }
}
