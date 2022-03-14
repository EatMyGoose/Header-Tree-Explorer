namespace HeaderTreeExplorer
{
    partial class FileAndFolderBrowserDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lvDirectories = new System.Windows.Forms.ListView();
            this.tbCurrentDirectory = new System.Windows.Forms.TextBox();
            this.lblCurrentDirectory = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbRegexAppliedToFolder = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cbFileFilter = new System.Windows.Forms.ComboBox();
            this.tbRegexFilter = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbUseFilter = new System.Windows.Forms.CheckBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnBack = new System.Windows.Forms.Button();
            this.tipLblRegexFilter = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvDirectories
            // 
            this.lvDirectories.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvDirectories.GridLines = true;
            this.lvDirectories.Location = new System.Drawing.Point(12, 214);
            this.lvDirectories.Name = "lvDirectories";
            this.lvDirectories.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lvDirectories.Size = new System.Drawing.Size(1012, 490);
            this.lvDirectories.TabIndex = 0;
            this.lvDirectories.UseCompatibleStateImageBehavior = false;
            this.lvDirectories.View = System.Windows.Forms.View.Details;
            this.lvDirectories.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvDirectories_ColumnClick);
            this.lvDirectories.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lvDirectories_ItemSelectionChanged);
            this.lvDirectories.DoubleClick += new System.EventHandler(this.lvDirectories_DoubleClick);
            // 
            // tbCurrentDirectory
            // 
            this.tbCurrentDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCurrentDirectory.Location = new System.Drawing.Point(218, 40);
            this.tbCurrentDirectory.Name = "tbCurrentDirectory";
            this.tbCurrentDirectory.Size = new System.Drawing.Size(770, 29);
            this.tbCurrentDirectory.TabIndex = 1;
            this.tbCurrentDirectory.Leave += new System.EventHandler(this.tbCurrentDirectory_Leave);
            // 
            // lblCurrentDirectory
            // 
            this.lblCurrentDirectory.AutoSize = true;
            this.lblCurrentDirectory.Location = new System.Drawing.Point(47, 41);
            this.lblCurrentDirectory.Name = "lblCurrentDirectory";
            this.lblCurrentDirectory.Size = new System.Drawing.Size(165, 25);
            this.lblCurrentDirectory.TabIndex = 2;
            this.lblCurrentDirectory.Text = "Current Directory:";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.cbRegexAppliedToFolder);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.cbFileFilter);
            this.groupBox1.Controls.Add(this.tbRegexFilter);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.cbUseFilter);
            this.groupBox1.Controls.Add(this.lblCurrentDirectory);
            this.groupBox1.Controls.Add(this.tbCurrentDirectory);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1012, 196);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Options:";
            // 
            // cbRegexAppliedToFolder
            // 
            this.cbRegexAppliedToFolder.AutoSize = true;
            this.cbRegexAppliedToFolder.Enabled = false;
            this.cbRegexAppliedToFolder.Location = new System.Drawing.Point(421, 120);
            this.cbRegexAppliedToFolder.Name = "cbRegexAppliedToFolder";
            this.cbRegexAppliedToFolder.Size = new System.Drawing.Size(287, 29);
            this.cbRegexAppliedToFolder.TabIndex = 10;
            this.cbRegexAppliedToFolder.Text = "Apply Regex Filter to Folders";
            this.tipLblRegexFilter.SetToolTip(this.cbRegexAppliedToFolder, "Apply Regex filter to folder names as well");
            this.cbRegexAppliedToFolder.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(25, 158);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(187, 25);
            this.label3.TabIndex = 9;
            this.label3.Text = "File Extension Filter:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(133, 155);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 25);
            this.label2.TabIndex = 8;
            // 
            // cbFileFilter
            // 
            this.cbFileFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbFileFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbFileFilter.FormattingEnabled = true;
            this.cbFileFilter.Location = new System.Drawing.Point(218, 155);
            this.cbFileFilter.Name = "cbFileFilter";
            this.cbFileFilter.Size = new System.Drawing.Size(770, 32);
            this.cbFileFilter.TabIndex = 7;
            // 
            // tbRegexFilter
            // 
            this.tbRegexFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbRegexFilter.Enabled = false;
            this.tbRegexFilter.Location = new System.Drawing.Point(218, 81);
            this.tbRegexFilter.Name = "tbRegexFilter";
            this.tbRegexFilter.Size = new System.Drawing.Size(770, 29);
            this.tbRegexFilter.TabIndex = 5;
            this.tbRegexFilter.Leave += new System.EventHandler(this.tbRegexFilter_Leave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 81);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(206, 25);
            this.label1.TabIndex = 4;
            this.label1.Text = "Filename Regex Filter:";
            this.tipLblRegexFilter.SetToolTip(this.label1, "Regex pattern used to filter the files (based on filename) within the current dir" +
        "ectory");
            // 
            // cbUseFilter
            // 
            this.cbUseFilter.AutoSize = true;
            this.cbUseFilter.Location = new System.Drawing.Point(218, 120);
            this.cbUseFilter.Name = "cbUseFilter";
            this.cbUseFilter.Size = new System.Drawing.Size(181, 29);
            this.cbUseFilter.TabIndex = 3;
            this.cbUseFilter.Text = "Use Regex Filter";
            this.tipLblRegexFilter.SetToolTip(this.cbUseFilter, "Enable/Disable regex filter. Regex filter is applied after file extension filteri" +
        "ng");
            this.cbUseFilter.UseVisualStyleBackColor = true;
            this.cbUseFilter.CheckedChanged += new System.EventHandler(this.cbUseFilter_CheckedChanged);
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Enabled = false;
            this.btnOk.Location = new System.Drawing.Point(935, 719);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(89, 41);
            this.btnOk.TabIndex = 4;
            this.btnOk.Text = "Open";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(813, 720);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(111, 41);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnBack
            // 
            this.btnBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnBack.Location = new System.Drawing.Point(12, 719);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(118, 40);
            this.btnBack.TabIndex = 6;
            this.btnBack.Text = "Prev. Dir";
            this.btnBack.UseVisualStyleBackColor = true;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // tipLblRegexFilter
            // 
            this.tipLblRegexFilter.ToolTipTitle = "Regex Filter:";
            // 
            // FileAndFolderBrowserDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1036, 776);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lvDirectories);
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "FileAndFolderBrowserDialog";
            this.Text = "Select Files/Folders";
            this.Load += new System.EventHandler(this.FileAndFolderBrowserDialog_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvDirectories;
        private System.Windows.Forms.TextBox tbCurrentDirectory;
        private System.Windows.Forms.Label lblCurrentDirectory;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox cbUseFilter;
        private System.Windows.Forms.TextBox tbRegexFilter;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbFileFilter;
        private System.Windows.Forms.CheckBox cbRegexAppliedToFolder;
        private System.Windows.Forms.ToolTip tipLblRegexFilter;
    }
}