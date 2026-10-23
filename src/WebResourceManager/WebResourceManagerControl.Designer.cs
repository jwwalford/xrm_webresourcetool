namespace WebResourceManager
{
    partial class WebResourceManagerControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tsMain = new System.Windows.Forms.ToolStrip();
            this.tsbClose = new System.Windows.Forms.ToolStripButton();
            this.tssSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tslType = new System.Windows.Forms.ToolStripLabel();
            this.tscbType = new System.Windows.Forms.ToolStripComboBox();
            this.tsbLoad = new System.Windows.Forms.ToolStripButton();
            this.tsddOptions = new System.Windows.Forms.ToolStripDropDownButton();
            this.tsmiHideHidden = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiUnmanagedOnly = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiRunScripts = new System.Windows.Forms.ToolStripMenuItem();
            this.tssSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tslSearch = new System.Windows.Forms.ToolStripLabel();
            this.tstSearch = new System.Windows.Forms.ToolStripTextBox();
            this.tssSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbSave = new System.Windows.Forms.ToolStripButton();
            this.scMain = new System.Windows.Forms.SplitContainer();
            this.lvWebResources = new System.Windows.Forms.ListView();
            this.chName = new System.Windows.Forms.ColumnHeader();
            this.chDisplayName = new System.Windows.Forms.ColumnHeader();
            this.chType = new System.Windows.Forms.ColumnHeader();
            this.chManaged = new System.Windows.Forms.ColumnHeader();
            this.chModifiedOn = new System.Windows.Forms.ColumnHeader();
            this.cmsList = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiCopyName = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSave = new System.Windows.Forms.ToolStripMenuItem();
            this.ssList = new System.Windows.Forms.StatusStrip();
            this.tsslCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.pnlPreview = new System.Windows.Forms.Panel();
            this.lblNoPreview = new System.Windows.Forms.Label();
            this.pbImage = new System.Windows.Forms.PictureBox();
            this.tsPreview = new System.Windows.Forms.ToolStrip();
            this.tslPreviewTitle = new System.Windows.Forms.ToolStripLabel();
            this.tsbViewSource = new System.Windows.Forms.ToolStripButton();
            this.lblDetails = new System.Windows.Forms.Label();
            this.tsMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scMain)).BeginInit();
            this.scMain.Panel1.SuspendLayout();
            this.scMain.Panel2.SuspendLayout();
            this.scMain.SuspendLayout();
            this.cmsList.SuspendLayout();
            this.ssList.SuspendLayout();
            this.pnlPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbImage)).BeginInit();
            this.tsPreview.SuspendLayout();
            this.SuspendLayout();
            //
            // tsMain
            //
            this.tsMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbClose,
            this.tssSeparator1,
            this.tslType,
            this.tscbType,
            this.tsbLoad,
            this.tsddOptions,
            this.tssSeparator2,
            this.tslSearch,
            this.tstSearch,
            this.tssSeparator3,
            this.tsbSave});
            this.tsMain.Location = new System.Drawing.Point(0, 0);
            this.tsMain.Name = "tsMain";
            this.tsMain.Size = new System.Drawing.Size(1000, 25);
            this.tsMain.TabIndex = 0;
            //
            // tsbClose
            //
            this.tsbClose.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbClose.Name = "tsbClose";
            this.tsbClose.Text = "Close";
            this.tsbClose.Click += new System.EventHandler(this.tsbClose_Click);
            //
            // tslType
            //
            this.tslType.Name = "tslType";
            this.tslType.Text = "Type:";
            //
            // tscbType
            //
            this.tscbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tscbType.Name = "tscbType";
            this.tscbType.Size = new System.Drawing.Size(220, 25);
            //
            // tsbLoad
            //
            this.tsbLoad.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbLoad.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.tsbLoad.Name = "tsbLoad";
            this.tsbLoad.Text = "Load web resources";
            this.tsbLoad.Click += new System.EventHandler(this.tsbLoad_Click);
            //
            // tsddOptions
            //
            this.tsddOptions.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsddOptions.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiHideHidden,
            this.tsmiUnmanagedOnly,
            this.tsmiRunScripts});
            this.tsddOptions.Name = "tsddOptions";
            this.tsddOptions.Text = "Options";
            //
            // tsmiHideHidden
            //
            this.tsmiHideHidden.Checked = true;
            this.tsmiHideHidden.CheckOnClick = true;
            this.tsmiHideHidden.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tsmiHideHidden.Name = "tsmiHideHidden";
            this.tsmiHideHidden.Text = "Hide hidden (system) web resources";
            //
            // tsmiUnmanagedOnly
            //
            this.tsmiUnmanagedOnly.CheckOnClick = true;
            this.tsmiUnmanagedOnly.Name = "tsmiUnmanagedOnly";
            this.tsmiUnmanagedOnly.Text = "Unmanaged only";
            //
            // tsmiRunScripts
            //
            this.tsmiRunScripts.CheckOnClick = true;
            this.tsmiRunScripts.Name = "tsmiRunScripts";
            this.tsmiRunScripts.Text = "Run scripts in HTML preview";
            this.tsmiRunScripts.CheckedChanged += new System.EventHandler(this.tsmiRunScripts_CheckedChanged);
            //
            // tslSearch
            //
            this.tslSearch.Name = "tslSearch";
            this.tslSearch.Text = "Search:";
            //
            // tstSearch
            //
            this.tstSearch.Name = "tstSearch";
            this.tstSearch.Size = new System.Drawing.Size(180, 25);
            this.tstSearch.TextChanged += new System.EventHandler(this.tstSearch_TextChanged);
            //
            // tsbSave
            //
            this.tsbSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbSave.Enabled = false;
            this.tsbSave.Name = "tsbSave";
            this.tsbSave.Text = "Save to disk...";
            this.tsbSave.Click += new System.EventHandler(this.SaveSelected_Click);
            //
            // scMain
            //
            this.scMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scMain.Location = new System.Drawing.Point(0, 25);
            this.scMain.Name = "scMain";
            //
            // scMain.Panel1
            //
            this.scMain.Panel1.Controls.Add(this.lvWebResources);
            this.scMain.Panel1.Controls.Add(this.ssList);
            //
            // scMain.Panel2
            //
            this.scMain.Panel2.Controls.Add(this.pnlPreview);
            this.scMain.Panel2.Controls.Add(this.lblDetails);
            this.scMain.Panel2.Controls.Add(this.tsPreview);
            this.scMain.Size = new System.Drawing.Size(1000, 575);
            this.scMain.SplitterDistance = 520;
            this.scMain.TabIndex = 1;
            //
            // lvWebResources
            //
            this.lvWebResources.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chName,
            this.chDisplayName,
            this.chType,
            this.chManaged,
            this.chModifiedOn});
            this.lvWebResources.ContextMenuStrip = this.cmsList;
            this.lvWebResources.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvWebResources.FullRowSelect = true;
            this.lvWebResources.HideSelection = false;
            this.lvWebResources.MultiSelect = false;
            this.lvWebResources.Name = "lvWebResources";
            this.lvWebResources.TabIndex = 0;
            this.lvWebResources.UseCompatibleStateImageBehavior = false;
            this.lvWebResources.View = System.Windows.Forms.View.Details;
            this.lvWebResources.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvWebResources_ColumnClick);
            this.lvWebResources.SelectedIndexChanged += new System.EventHandler(this.lvWebResources_SelectedIndexChanged);
            //
            // chName
            //
            this.chName.Text = "Name";
            this.chName.Width = 250;
            //
            // chDisplayName
            //
            this.chDisplayName.Text = "Display name";
            this.chDisplayName.Width = 150;
            //
            // chType
            //
            this.chType.Text = "Type";
            this.chType.Width = 120;
            //
            // chManaged
            //
            this.chManaged.Text = "Managed";
            this.chManaged.Width = 65;
            //
            // chModifiedOn
            //
            this.chModifiedOn.Text = "Modified on";
            this.chModifiedOn.Width = 120;
            //
            // cmsList
            //
            this.cmsList.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiCopyName,
            this.tsmiSave});
            this.cmsList.Name = "cmsList";
            this.cmsList.Opening += new System.ComponentModel.CancelEventHandler(this.cmsList_Opening);
            //
            // tsmiCopyName
            //
            this.tsmiCopyName.Name = "tsmiCopyName";
            this.tsmiCopyName.Text = "Copy name";
            this.tsmiCopyName.Click += new System.EventHandler(this.tsmiCopyName_Click);
            //
            // tsmiSave
            //
            this.tsmiSave.Name = "tsmiSave";
            this.tsmiSave.Text = "Save to disk...";
            this.tsmiSave.Click += new System.EventHandler(this.SaveSelected_Click);
            //
            // ssList
            //
            this.ssList.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslCount});
            this.ssList.Name = "ssList";
            this.ssList.SizingGrip = false;
            this.ssList.TabIndex = 1;
            //
            // tsslCount
            //
            this.tsslCount.Name = "tsslCount";
            this.tsslCount.Text = "Pick a type and click Load web resources.";
            //
            // pnlPreview
            //
            this.pnlPreview.BackColor = System.Drawing.SystemColors.Window;
            this.pnlPreview.Controls.Add(this.pbImage);
            this.pnlPreview.Controls.Add(this.lblNoPreview);
            this.pnlPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPreview.Name = "pnlPreview";
            this.pnlPreview.TabIndex = 2;
            //
            // lblNoPreview
            //
            this.lblNoPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNoPreview.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblNoPreview.Name = "lblNoPreview";
            this.lblNoPreview.Text = "Select a web resource to preview it.";
            this.lblNoPreview.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // pbImage
            //
            this.pbImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbImage.Name = "pbImage";
            this.pbImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbImage.TabStop = false;
            this.pbImage.Visible = false;
            this.pbImage.Resize += new System.EventHandler(this.pbImage_Resize);
            //
            // tsPreview
            //
            this.tsPreview.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsPreview.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslPreviewTitle,
            this.tsbViewSource});
            this.tsPreview.Name = "tsPreview";
            this.tsPreview.TabIndex = 0;
            //
            // tslPreviewTitle
            //
            this.tslPreviewTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.tslPreviewTitle.Name = "tslPreviewTitle";
            this.tslPreviewTitle.Text = "Preview";
            //
            // tsbViewSource
            //
            this.tsbViewSource.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tsbViewSource.CheckOnClick = true;
            this.tsbViewSource.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbViewSource.Name = "tsbViewSource";
            this.tsbViewSource.Text = "View source";
            this.tsbViewSource.Visible = false;
            this.tsbViewSource.CheckedChanged += new System.EventHandler(this.tsbViewSource_CheckedChanged);
            //
            // lblDetails
            //
            this.lblDetails.AutoSize = false;
            this.lblDetails.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDetails.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblDetails.Name = "lblDetails";
            this.lblDetails.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.lblDetails.Size = new System.Drawing.Size(476, 36);
            //
            // WebResourceManagerControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.scMain);
            this.Controls.Add(this.tsMain);
            this.Name = "WebResourceManagerControl";
            this.Size = new System.Drawing.Size(1000, 600);
            this.Load += new System.EventHandler(this.WebResourceManagerControl_Load);
            this.tsMain.ResumeLayout(false);
            this.tsMain.PerformLayout();
            this.scMain.Panel1.ResumeLayout(false);
            this.scMain.Panel1.PerformLayout();
            this.scMain.Panel2.ResumeLayout(false);
            this.scMain.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scMain)).EndInit();
            this.scMain.ResumeLayout(false);
            this.cmsList.ResumeLayout(false);
            this.ssList.ResumeLayout(false);
            this.ssList.PerformLayout();
            this.pnlPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbImage)).EndInit();
            this.tsPreview.ResumeLayout(false);
            this.tsPreview.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip tsMain;
        private System.Windows.Forms.ToolStripButton tsbClose;
        private System.Windows.Forms.ToolStripSeparator tssSeparator1;
        private System.Windows.Forms.ToolStripLabel tslType;
        private System.Windows.Forms.ToolStripComboBox tscbType;
        private System.Windows.Forms.ToolStripButton tsbLoad;
        private System.Windows.Forms.ToolStripDropDownButton tsddOptions;
        private System.Windows.Forms.ToolStripMenuItem tsmiHideHidden;
        private System.Windows.Forms.ToolStripMenuItem tsmiUnmanagedOnly;
        private System.Windows.Forms.ToolStripMenuItem tsmiRunScripts;
        private System.Windows.Forms.ToolStripSeparator tssSeparator2;
        private System.Windows.Forms.ToolStripLabel tslSearch;
        private System.Windows.Forms.ToolStripTextBox tstSearch;
        private System.Windows.Forms.ToolStripSeparator tssSeparator3;
        private System.Windows.Forms.ToolStripButton tsbSave;
        private System.Windows.Forms.SplitContainer scMain;
        private System.Windows.Forms.ListView lvWebResources;
        private System.Windows.Forms.ColumnHeader chName;
        private System.Windows.Forms.ColumnHeader chDisplayName;
        private System.Windows.Forms.ColumnHeader chType;
        private System.Windows.Forms.ColumnHeader chManaged;
        private System.Windows.Forms.ColumnHeader chModifiedOn;
        private System.Windows.Forms.ContextMenuStrip cmsList;
        private System.Windows.Forms.ToolStripMenuItem tsmiCopyName;
        private System.Windows.Forms.ToolStripMenuItem tsmiSave;
        private System.Windows.Forms.StatusStrip ssList;
        private System.Windows.Forms.ToolStripStatusLabel tsslCount;
        private System.Windows.Forms.Panel pnlPreview;
        private System.Windows.Forms.Label lblNoPreview;
        private System.Windows.Forms.PictureBox pbImage;
        private System.Windows.Forms.ToolStrip tsPreview;
        private System.Windows.Forms.ToolStripLabel tslPreviewTitle;
        private System.Windows.Forms.ToolStripButton tsbViewSource;
        private System.Windows.Forms.Label lblDetails;
    }
}
