namespace HeaderTreeExplorer
{
    partial class AdditionalIncludeForm
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
            this.lbFileSelection = new System.Windows.Forms.ListBox();
            this.BtnAdd = new System.Windows.Forms.Button();
            this.BtnRemoveSelection = new System.Windows.Forms.Button();
            this.BtnClear = new System.Windows.Forms.Button();
            this.BtnFormCancel = new System.Windows.Forms.Button();
            this.BtnFormApply = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnSelectFolder = new System.Windows.Forms.Button();
            this.tbFolderInput = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbFileSelection
            // 
            this.lbFileSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbFileSelection.FormattingEnabled = true;
            this.lbFileSelection.ItemHeight = 24;
            this.lbFileSelection.Location = new System.Drawing.Point(17, 101);
            this.lbFileSelection.Name = "lbFileSelection";
            this.lbFileSelection.Size = new System.Drawing.Size(631, 436);
            this.lbFileSelection.TabIndex = 0;
            // 
            // BtnAdd
            // 
            this.BtnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnAdd.Location = new System.Drawing.Point(485, 27);
            this.BtnAdd.Name = "BtnAdd";
            this.BtnAdd.Size = new System.Drawing.Size(163, 33);
            this.BtnAdd.TabIndex = 1;
            this.BtnAdd.Text = "Add";
            this.BtnAdd.UseVisualStyleBackColor = true;
            this.BtnAdd.Click += new System.EventHandler(this.BtnAdd_Click);
            // 
            // BtnRemoveSelection
            // 
            this.BtnRemoveSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnRemoveSelection.Location = new System.Drawing.Point(452, 552);
            this.BtnRemoveSelection.Name = "BtnRemoveSelection";
            this.BtnRemoveSelection.Size = new System.Drawing.Size(196, 32);
            this.BtnRemoveSelection.TabIndex = 2;
            this.BtnRemoveSelection.Text = "Remove Selection";
            this.BtnRemoveSelection.UseVisualStyleBackColor = true;
            this.BtnRemoveSelection.Click += new System.EventHandler(this.BtnRemoveSelection_Click);
            // 
            // BtnClear
            // 
            this.BtnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BtnClear.Location = new System.Drawing.Point(17, 551);
            this.BtnClear.Name = "BtnClear";
            this.BtnClear.Size = new System.Drawing.Size(131, 33);
            this.BtnClear.TabIndex = 3;
            this.BtnClear.Text = "Clear All";
            this.BtnClear.UseVisualStyleBackColor = true;
            this.BtnClear.Click += new System.EventHandler(this.BtnClear_Click);
            // 
            // BtnFormCancel
            // 
            this.BtnFormCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnFormCancel.Location = new System.Drawing.Point(391, 610);
            this.BtnFormCancel.Name = "BtnFormCancel";
            this.BtnFormCancel.Size = new System.Drawing.Size(144, 34);
            this.BtnFormCancel.TabIndex = 4;
            this.BtnFormCancel.Text = "Cancel";
            this.BtnFormCancel.UseVisualStyleBackColor = true;
            this.BtnFormCancel.Click += new System.EventHandler(this.BtnFormCancel_Click);
            // 
            // BtnFormApply
            // 
            this.BtnFormApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnFormApply.Location = new System.Drawing.Point(541, 610);
            this.BtnFormApply.Name = "BtnFormApply";
            this.BtnFormApply.Size = new System.Drawing.Size(143, 34);
            this.BtnFormApply.TabIndex = 5;
            this.BtnFormApply.Text = "Apply";
            this.BtnFormApply.UseVisualStyleBackColor = true;
            this.BtnFormApply.Click += new System.EventHandler(this.BtnFormApply_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.btnSelectFolder);
            this.groupBox1.Controls.Add(this.tbFolderInput);
            this.groupBox1.Controls.Add(this.BtnAdd);
            this.groupBox1.Controls.Add(this.BtnClear);
            this.groupBox1.Controls.Add(this.BtnRemoveSelection);
            this.groupBox1.Controls.Add(this.lbFileSelection);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.MinimumSize = new System.Drawing.Size(650, 600);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(672, 600);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "File Selection:";
            // 
            // btnSelectFolder
            // 
            this.btnSelectFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectFolder.Location = new System.Drawing.Point(485, 66);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(163, 28);
            this.btnSelectFolder.TabIndex = 7;
            this.btnSelectFolder.Text = "Folder Browser";
            this.btnSelectFolder.UseVisualStyleBackColor = true;
            this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);
            // 
            // tbFolderInput
            // 
            this.tbFolderInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFolderInput.Location = new System.Drawing.Point(19, 31);
            this.tbFolderInput.Name = "tbFolderInput";
            this.tbFolderInput.Size = new System.Drawing.Size(460, 29);
            this.tbFolderInput.TabIndex = 8;
            this.tbFolderInput.TextChanged += new System.EventHandler(this.tbFolderInput_TextChanged);
            // 
            // AdditionalIncludeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(696, 656);
            this.Controls.Add(this.BtnFormApply);
            this.Controls.Add(this.BtnFormCancel);
            this.Controls.Add(this.groupBox1);
            this.Name = "AdditionalIncludeForm";
            this.Text = "AdditionalIncludeForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AdditionalIncludeForm_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lbFileSelection;
        private System.Windows.Forms.Button BtnAdd;
        private System.Windows.Forms.Button BtnRemoveSelection;
        private System.Windows.Forms.Button BtnClear;
        private System.Windows.Forms.Button BtnFormCancel;
        private System.Windows.Forms.Button BtnFormApply;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnSelectFolder;
        private System.Windows.Forms.TextBox tbFolderInput;
    }
}