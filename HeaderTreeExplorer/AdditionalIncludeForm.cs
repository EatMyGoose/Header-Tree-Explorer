using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace HeaderTreeExplorer
{
    public partial class AdditionalIncludeForm : Form
    {
        public delegate void OnApplyChangesHandler(string[] newFileSelection);
        public event OnApplyChangesHandler OnApply;

        delegate void OnSelectionChangedHandler(SortedSet<string> newSelection);
        event OnSelectionChangedHandler OnSelectionChanged;

        #region owned_members
        //To detect whether unsaved changes exist
        SortedSet<string> lastSavedSelection = new SortedSet<string>(); 
        //The actual selection reflected within the listview
        SortedSet<string> currentDirectoryList = new SortedSet<string>();
        Color originalTbColour = new Color();
        #endregion

        public AdditionalIncludeForm(string[] startingFileList, string formCaption)
        {
            InitializeComponent();

            if(startingFileList != null)
            {
                currentDirectoryList = new SortedSet<string>(startingFileList);
                lastSavedSelection = new SortedSet<string>(startingFileList);
            }

            OnSelectionChanged += PopulateListBox;
            OnApply += UpdateLastSavedSelection;

            //Set form title if specified.
            this.Text = formCaption?? "Select Directories";

            //Save original BG color for textbox
            originalTbColour = tbFolderInput.BackColor;

            //Allow multiple selections
            lbFileSelection.SelectionMode = SelectionMode.MultiExtended;

            //Initial population
            PopulateListBox(currentDirectoryList);
        }
        
        void UpdateLastSavedSelection(string[] newSelection)
        {
            lastSavedSelection = new SortedSet<string>(newSelection);
        }

        void PopulateListBox(SortedSet<string> items)
        {
            //Remove all
            lbFileSelection.Items.Clear();
            //Repopulate
            lbFileSelection.Items.AddRange(items.ToArray());
        }

        //UI Handlers

        //Cancel (Close Form)
        private void BtnFormCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //Apply Changes
        private void BtnFormApply_Click(object sender, EventArgs e)
        {
            OnApply(currentDirectoryList.ToArray());
        }

        
        private void tbFolderInput_TextChanged(object sender, EventArgs e)
        {
            //Validate input
            string userInput = tbFolderInput.Text;
            bool entryIsFolder = Directory.Exists(userInput);
            tbFolderInput.BackColor = entryIsFolder ? originalTbColour : Color.Salmon;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string userInput = tbFolderInput.Text;
            bool entryIsFolder = Directory.Exists(userInput);
            if(!entryIsFolder)
            {
                MessageBox.Show("Specified Path is not a valid folder", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool isNotADuplicate = currentDirectoryList.Add(userInput);

            if(!isNotADuplicate)
            {
                MessageBox.Show("Warning: Directory already exists within selection", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            OnSelectionChanged(currentDirectoryList);
        }

        //Select new directories
        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fb = new FolderBrowserDialog())
            {
                if(fb.ShowDialog() == DialogResult.OK)
                {
                    bool newFolderAdded = currentDirectoryList.Add(fb.SelectedPath);
                    if (newFolderAdded) OnSelectionChanged(currentDirectoryList);
                }
            }
        }

        private void BtnRemoveSelection_Click(object sender, EventArgs e)
        {
            var selectedItems = lbFileSelection.SelectedItems; 
            foreach(var item in selectedItems)
            {
                bool itemFoundAndRemoved = currentDirectoryList.Remove(item.ToString());
                Debug.Assert(itemFoundAndRemoved, "Error, item not found");
            }

            OnSelectionChanged(currentDirectoryList);
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            DialogResult hResult = MessageBox.Show("Clear existing selection\nConfirm Action(Y/N)?", "Confirm Action", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
            if (hResult == DialogResult.OK)
            {
                currentDirectoryList.Clear();
                OnSelectionChanged(currentDirectoryList);
            }
        }

        private void AdditionalIncludeForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool unappliedChanges = !currentDirectoryList.SequenceEqual(lastSavedSelection);

            if(unappliedChanges)
            {
                DialogResult hResult = MessageBox.Show(
                    "Save changes(Yes/Discard)?", 
                    "Unsaved Changes", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question
                );

                if(hResult == DialogResult.Yes)
                {
                    OnApply(currentDirectoryList.ToArray());
                }
            }
        }

    }
}
