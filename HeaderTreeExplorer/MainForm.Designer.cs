namespace HeaderTreeExplorer
{
    partial class MainForm
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
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnImportVSProject = new System.Windows.Forms.Button();
            this.BtnDeleteAll = new System.Windows.Forms.Button();
            this.BtnLoadFile = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tvSelectedFiles = new System.Windows.Forms.TreeView();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.gbIncludeImpact = new System.Windows.Forms.GroupBox();
            this.rbIncludeImpactLOC = new System.Windows.Forms.RadioButton();
            this.rbIncludeImpactFiles = new System.Windows.Forms.RadioButton();
            this.BtnReportGenerate = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.cbReportType = new System.Windows.Forms.ComboBox();
            this.btnIncludeDirectories = new System.Windows.Forms.Button();
            this.btnLibDirectories = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnDeleteSelection = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.gbIncludeImpact.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1256, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.btnDeleteSelection);
            this.groupBox1.Controls.Add(this.btnImportVSProject);
            this.groupBox1.Controls.Add(this.BtnDeleteAll);
            this.groupBox1.Controls.Add(this.BtnLoadFile);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.tvSelectedFiles);
            this.groupBox1.Location = new System.Drawing.Point(12, 28);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(869, 616);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Settings";
            // 
            // btnImportVSProject
            // 
            this.btnImportVSProject.Location = new System.Drawing.Point(210, 33);
            this.btnImportVSProject.Name = "btnImportVSProject";
            this.btnImportVSProject.Size = new System.Drawing.Size(172, 34);
            this.btnImportVSProject.TabIndex = 4;
            this.btnImportVSProject.Text = "Load .vcxproj";
            this.btnImportVSProject.UseVisualStyleBackColor = true;
            this.btnImportVSProject.Click += new System.EventHandler(this.btnImportVSProject_Click);
            // 
            // BtnDeleteAll
            // 
            this.BtnDeleteAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnDeleteAll.Location = new System.Drawing.Point(388, 32);
            this.BtnDeleteAll.Name = "BtnDeleteAll";
            this.BtnDeleteAll.Size = new System.Drawing.Size(139, 35);
            this.BtnDeleteAll.TabIndex = 3;
            this.BtnDeleteAll.Text = "Delete All";
            this.BtnDeleteAll.UseVisualStyleBackColor = true;
            this.BtnDeleteAll.Click += new System.EventHandler(this.BtnDeleteAll_Click);
            // 
            // BtnLoadFile
            // 
            this.BtnLoadFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnLoadFile.Location = new System.Drawing.Point(706, 32);
            this.BtnLoadFile.Name = "BtnLoadFile";
            this.BtnLoadFile.Size = new System.Drawing.Size(139, 36);
            this.BtnLoadFile.TabIndex = 2;
            this.BtnLoadFile.Text = "Load Files";
            this.BtnLoadFile.UseVisualStyleBackColor = true;
            this.BtnLoadFile.Click += new System.EventHandler(this.BtnLoadFile_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(135, 25);
            this.label1.TabIndex = 1;
            this.label1.Text = "Selected Files";
            // 
            // tvSelectedFiles
            // 
            this.tvSelectedFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tvSelectedFiles.CheckBoxes = true;
            this.tvSelectedFiles.Location = new System.Drawing.Point(6, 74);
            this.tvSelectedFiles.Name = "tvSelectedFiles";
            this.tvSelectedFiles.Size = new System.Drawing.Size(839, 536);
            this.tvSelectedFiles.TabIndex = 0;
            this.tvSelectedFiles.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tvSelectedFiles_AfterCheck);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.gbIncludeImpact);
            this.groupBox2.Controls.Add(this.BtnReportGenerate);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.cbReportType);
            this.groupBox2.Location = new System.Drawing.Point(914, 28);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(330, 475);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Report Generator";
            // 
            // gbIncludeImpact
            // 
            this.gbIncludeImpact.Controls.Add(this.rbIncludeImpactLOC);
            this.gbIncludeImpact.Controls.Add(this.rbIncludeImpactFiles);
            this.gbIncludeImpact.Location = new System.Drawing.Point(22, 133);
            this.gbIncludeImpact.Name = "gbIncludeImpact";
            this.gbIncludeImpact.Size = new System.Drawing.Size(290, 105);
            this.gbIncludeImpact.TabIndex = 3;
            this.gbIncludeImpact.TabStop = false;
            this.gbIncludeImpact.Text = "Include Impact Criteria";
            // 
            // rbIncludeImpactLOC
            // 
            this.rbIncludeImpactLOC.AutoSize = true;
            this.rbIncludeImpactLOC.Checked = true;
            this.rbIncludeImpactLOC.Location = new System.Drawing.Point(16, 62);
            this.rbIncludeImpactLOC.Name = "rbIncludeImpactLOC";
            this.rbIncludeImpactLOC.Size = new System.Drawing.Size(79, 29);
            this.rbIncludeImpactLOC.TabIndex = 2;
            this.rbIncludeImpactLOC.TabStop = true;
            this.rbIncludeImpactLOC.Text = "LOC";
            this.rbIncludeImpactLOC.UseVisualStyleBackColor = true;
            // 
            // rbIncludeImpactFiles
            // 
            this.rbIncludeImpactFiles.AutoSize = true;
            this.rbIncludeImpactFiles.Location = new System.Drawing.Point(16, 27);
            this.rbIncludeImpactFiles.Name = "rbIncludeImpactFiles";
            this.rbIncludeImpactFiles.Size = new System.Drawing.Size(134, 29);
            this.rbIncludeImpactFiles.TabIndex = 1;
            this.rbIncludeImpactFiles.Text = "No. of Files";
            this.rbIncludeImpactFiles.UseVisualStyleBackColor = true;
            // 
            // BtnReportGenerate
            // 
            this.BtnReportGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnReportGenerate.Location = new System.Drawing.Point(22, 84);
            this.BtnReportGenerate.Name = "BtnReportGenerate";
            this.BtnReportGenerate.Size = new System.Drawing.Size(290, 35);
            this.BtnReportGenerate.TabIndex = 2;
            this.BtnReportGenerate.Text = "Generate";
            this.BtnReportGenerate.UseVisualStyleBackColor = true;
            this.BtnReportGenerate.Click += new System.EventHandler(this.BtnReportGenerate_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 25);
            this.label2.TabIndex = 1;
            this.label2.Text = "Type:";
            // 
            // cbReportType
            // 
            this.cbReportType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbReportType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbReportType.FormattingEnabled = true;
            this.cbReportType.Items.AddRange(new object[] {
            "Include Frequency",
            "[.dot/.gv] Header Graph"});
            this.cbReportType.Location = new System.Drawing.Point(86, 34);
            this.cbReportType.Name = "cbReportType";
            this.cbReportType.Size = new System.Drawing.Size(226, 32);
            this.cbReportType.TabIndex = 0;
            // 
            // btnIncludeDirectories
            // 
            this.btnIncludeDirectories.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnIncludeDirectories.Location = new System.Drawing.Point(22, 80);
            this.btnIncludeDirectories.Name = "btnIncludeDirectories";
            this.btnIncludeDirectories.Size = new System.Drawing.Size(282, 37);
            this.btnIncludeDirectories.TabIndex = 4;
            this.btnIncludeDirectories.Text = "Additional Include Directories";
            this.btnIncludeDirectories.UseVisualStyleBackColor = true;
            this.btnIncludeDirectories.Click += new System.EventHandler(this.btnIncludeDirectories_Click);
            // 
            // btnLibDirectories
            // 
            this.btnLibDirectories.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLibDirectories.Location = new System.Drawing.Point(22, 37);
            this.btnLibDirectories.Name = "btnLibDirectories";
            this.btnLibDirectories.Size = new System.Drawing.Size(282, 37);
            this.btnLibDirectories.TabIndex = 5;
            this.btnLibDirectories.Text = "Library Directories";
            this.btnLibDirectories.UseVisualStyleBackColor = true;
            this.btnLibDirectories.Click += new System.EventHandler(this.btnLibDirectories_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.btnLibDirectories);
            this.groupBox3.Controls.Add(this.btnIncludeDirectories);
            this.groupBox3.Location = new System.Drawing.Point(914, 509);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(330, 135);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Configure:";
            // 
            // btnDeleteSelection
            // 
            this.btnDeleteSelection.Location = new System.Drawing.Point(532, 32);
            this.btnDeleteSelection.Name = "btnDeleteSelection";
            this.btnDeleteSelection.Size = new System.Drawing.Size(169, 35);
            this.btnDeleteSelection.TabIndex = 5;
            this.btnDeleteSelection.Text = "Delete Selection";
            this.btnDeleteSelection.UseVisualStyleBackColor = true;
            this.btnDeleteSelection.Click += new System.EventHandler(this.btnDeleteSelection_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1256, 656);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.toolStrip1);
            this.MinimumSize = new System.Drawing.Size(500, 500);
            this.Name = "MainForm";
            this.Text = "Header Tree Explorer";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.gbIncludeImpact.ResumeLayout(false);
            this.gbIncludeImpact.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button BtnLoadFile;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TreeView tvSelectedFiles;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button BtnReportGenerate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbReportType;
        private System.Windows.Forms.Button BtnDeleteAll;
        private System.Windows.Forms.Button btnIncludeDirectories;
        private System.Windows.Forms.Button btnLibDirectories;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox gbIncludeImpact;
        private System.Windows.Forms.RadioButton rbIncludeImpactLOC;
        private System.Windows.Forms.RadioButton rbIncludeImpactFiles;
        private System.Windows.Forms.Button btnImportVSProject;
        private System.Windows.Forms.Button btnDeleteSelection;
    }
}

