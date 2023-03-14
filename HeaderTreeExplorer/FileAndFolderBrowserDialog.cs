using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using Microsoft.Win32;
using System.Runtime.InteropServices;

using System.Text.RegularExpressions;

namespace HeaderTreeExplorer
{
    public partial class FileAndFolderBrowserDialog : Form
    {

        #region private variables
        private enum ColumnIndex : int
        {
            Name = 0,
            Type = 1,
            LastModified = 2

        }

        bool[] ColumnSortedByAscending = { false, false, false }; //For listview sorting, each element corresponds to the column index specified above

        public delegate void RowDoubleClickedHandler(int index);
        private event RowDoubleClickedHandler OnRowDoubleClicked;

        private string currentFolder;
        FileSystemInfo[] currentFolderItems = new FileSystemInfo[] { }; //Unfiltered items within the currently selected folder
        FileSystemInfo[] displayedFilesAndDirectories = new FileSystemInfo[] {}; //Object indices will always maintain a 1-1 relationship with the listview
        
        private Stack<string> folderHistory = new Stack<string>();

        
        //(displayedTitle:string, extensions(including the dot):string[])
        Tuple<string, string[]>[] extensionFilters = new Tuple<string, string[]>[] { };
        Tuple<string, string[]>[] defaultExtensionFilter = new Tuple<string, string[]>[] { Tuple.Create("All Files(*.*)", new string[] { "*" }) };

        Regex currentFilterRegex = null; //null = no filter
        #endregion

        #region public variables
        public bool selectableFolders = false;
        public bool selectableFiles = true;
        public bool multiSelect = true;

        public string fileFilters = "All Files(*.*)|*.*";
        #endregion 

        public FileAndFolderBrowserDialog()
        {
            InitializeComponent();

            //setup columns
            lvDirectories.Columns.Add("Name");
            lvDirectories.Columns.Add("Type"); //["File"|"Folder"]
            lvDirectories.Columns.Add("Last Modified"); //Displayed format based on system settings

            //Event registration
            OnRowDoubleClicked += NavigateIntoClickedFolderInListView;

            //cbUseFilter.CheckedChanged += ReapplyFiltersAndUpdateListView;
            cbFileFilter.SelectedIndexChanged += ReapplyFiltersAndUpdateListView;
        }

        public void SetInitialDirectory(string path)
        {
            currentFolder = path;
        }
        
        //Initial form load
        private void FileAndFolderBrowserDialog_Load(object sender, EventArgs e)
        {
            if(!selectableFolders && !selectableFiles)
            {
                throw new ArgumentOutOfRangeException("Error creating FileAndFolderBrowserDialog: option to select both files & folders disabled.");
            }

            //Set title
            var titleCombination = new List<string>();
            if (selectableFiles) titleCombination.Add("Files");
            if (selectableFolders) titleCombination.Add("Folders");
            this.Text = $"Select {string.Join("/", titleCombination)}";

            //Multiselect
            lvDirectories.MultiSelect = multiSelect;

            //Populate file-filters
            var parsedExtensionFilters = Helpers.GetExtensionFilters(fileFilters);
            extensionFilters = parsedExtensionFilters.Length > 0? parsedExtensionFilters : defaultExtensionFilter;

            foreach(string filterDescription in extensionFilters.Select(pair => pair.Item1))
            {
                cbFileFilter.Items.Add(filterDescription);
            }
            cbFileFilter.SelectedIndex = 0; //guaranteed to have at least one item

            //Update listview
            string defaultDir = Helpers.GetDefaultDir();

            currentFolder = string.IsNullOrWhiteSpace(currentFolder) ?
                defaultDir :
                currentFolder;

            bool couldLoadInitialDir = TryNavigateIntoFolder(currentFolder, false);

            //If the requested directory cannot be read, load C:\ instead
            if (!couldLoadInitialDir)
            {
                bool couldLoadDefaultDir = TryNavigateIntoFolder(defaultDir, false);

                if (couldLoadDefaultDir)
                {
                    MessageBox.Show($"Unable to read \"{currentFolder}\", loading \"{defaultDir}\" instead.", "Unable to read directory",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Asterisk);
                }
                else //Leave listview unpopulated.
                {
                    MessageBox.Show($"Unable to read \"{currentFolder}\" or \"{defaultDir}\"", "Unable to read directory",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Asterisk);
                }

                return;
            }
        }

        public string[] GetSelectedFolders()
        {
            int[] selectedIndices = lvDirectories.SelectedIndices.Cast<int>().ToArray();
            return selectedIndices
                .Select(index => displayedFilesAndDirectories[index])
                .Where(fileInfo => Helpers.IsDir(fileInfo))
                .Select(fileInfo => fileInfo.FullName)
                .ToArray();
        }

        public string[] GetSelectedFiles()
        {
            int[] selectedIndices = lvDirectories.SelectedIndices.Cast<int>().ToArray();
            return selectedIndices
                .Select(index => displayedFilesAndDirectories[index])
                .Where(fileInfo => !Helpers.IsDir(fileInfo))
                .Select(fileInfo => fileInfo.FullName)
                .ToArray();
        }
        
        private bool AtLeastOneItemSelected()
        {
            return  ((selectableFiles ? GetSelectedFiles().Length : 0) +
                    (selectableFolders ? GetSelectedFolders().Length : 0)) > 0;
        }

        private void EnableBackButtonIfNonEmptyHistory()
        {
            btnBack.Enabled = folderHistory.Count() > 0;
        }

        //Tries load & update the listview with the contents of the newly specified folder
        //If unsuccessful, the listview remains unchanged & returns false
        private bool TryNavigateIntoFolder(string path, bool appendPrevPathToHistory)
        {
            Tuple<bool, FileSystemInfo[]> subFolderInfo = Helpers.TryListDirectory(path);
            bool readSuccessful = subFolderInfo.Item1;

            if (!readSuccessful) return false;

            if(appendPrevPathToHistory) folderHistory.Push(currentFolder);

            Func<FileSystemInfo[], FileSystemInfo[]> activeFilters = GetActiveFileExtensionAndRegexFilters();
            UpdateListView(subFolderInfo.Item2, path, activeFilters);

            EnableBackButtonIfNonEmptyHistory();
            
            return true;
        }

        private void NavigateIntoClickedFolderInListView(int index)
        {
            FileSystemInfo clickedItem = displayedFilesAndDirectories[index];

            if (!Helpers.IsDir(clickedItem)) return; //double clicked on file, ignore

            //Retrieve items within clicked subfolder
            string subfolderPath = clickedItem.FullName;

            bool couldLoadDir = TryNavigateIntoFolder(subfolderPath, true);
            if(!couldLoadDir)
            {
                MessageBox.Show(
                    $"Error reading {subfolderPath}", 
                    "Error reading directory", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Exclamation);
                return;
            }
        }

        private void tbCurrentDirectory_Leave(object sender, EventArgs e)
        {
            string userSpecifiedPath = tbCurrentDirectory.Text;
            bool valueChanged = userSpecifiedPath != currentFolder;
            bool pathIsDir = Directory.Exists(userSpecifiedPath);

            bool absPathIsDifferent = 
                pathIsDir && 
                Path.GetFullPath(userSpecifiedPath) != Path.GetFullPath(currentFolder);

            if( valueChanged && 
                pathIsDir && 
                absPathIsDifferent)
            {
                //Try to enter directory
                bool couldLoadNewFolder = TryNavigateIntoFolder(userSpecifiedPath, true);
               
                if (couldLoadNewFolder) return; //success, skip the reversion step
            }
           
            /*-Reversion-*/
            //(failure or no change in path, revert)

            if(!pathIsDir)
            {
                //Not an accessible path
                MessageBox.Show(
                    $"Error: Specified path \"{userSpecifiedPath}\"is not a folder",
                    "Invalid folder path",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            //revert back to previous dir.
            tbCurrentDirectory.Text = currentFolder;
        }

        public Func<string, bool> GetCurrentlySelectedExtensionFilter()
        {
            string[] currentlySelectedFileExtensions = extensionFilters[cbFileFilter.SelectedIndex].Item2;

            return (string pathName) =>
            {
                string fileExtension = Path.GetExtension(pathName).ToLower().Trim();

                return currentlySelectedFileExtensions.Any(ext => ext == ".*" || ext == fileExtension);
            };
        }

        private Func<FileSystemInfo[], FileSystemInfo[]> GetActiveFileExtensionAndRegexFilters()
        {
            return (FileSystemInfo[] originalList) =>
            {
                //Apply file extension filters first
                string[] selectedExtensionFilters = extensionFilters[cbFileFilter.SelectedIndex].Item2;
                FileSystemInfo[] selectionAfterFileExtensionFilters = Helpers.ApplyFileExtensionFilters(originalList, selectedExtensionFilters);

                //Then apply regex filters
                Regex reg = cbUseFilter.Checked ? currentFilterRegex : null; //Disable regex filtering if not enabled
                bool applyToFolders = cbRegexAppliedToFolder.Checked;
                FileSystemInfo[] selectionAfterRegexFilter = Helpers.ApplyRegexFilters(selectionAfterFileExtensionFilters, reg, applyToFolders);

                return selectionAfterRegexFilter;
            };
        }

        private void UpdateListView(FileSystemInfo[] directories, string currentDirectory, Func<FileSystemInfo[], FileSystemInfo[]> filter)
        {
            //Update displayed directory
            string fullPath = Path.GetFullPath(currentDirectory); 
            currentFolder = fullPath;
            tbCurrentDirectory.Text = fullPath;

            FileSystemInfo[] filteredSelection = filter == null ? directories : filter(directories);
            
            //Then repopulate the listview
            RepopulateListView(filteredSelection);
        }

        private void lvDirectories_DoubleClick(object sender, EventArgs e)
        {
            bool itemClicked = lvDirectories.FocusedItem != null;
            int clickedIndex = itemClicked? lvDirectories.FocusedItem.Index : -1;

            if(itemClicked)
            {
                OnRowDoubleClicked.Invoke(clickedIndex);
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (folderHistory.Count() == 0) return; 

            string previousDir = folderHistory.Peek();
            bool couldReturnToPrevFolder = TryNavigateIntoFolder(previousDir, false);

            if (!couldReturnToPrevFolder)
            {
                MessageBox.Show(
                    $"Error, unable to read previous directory\"{previousDir}\"",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return;
            }

            //Able to traverse to prev. dir, pop off history stack
            folderHistory.Pop();

            EnableBackButtonIfNonEmptyHistory();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void lvDirectories_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            btnOk.Enabled = AtLeastOneItemSelected();
        }

        private void cbUseFilter_CheckedChanged(object sender, EventArgs e)
        {
            bool regexFilterEnabled = cbUseFilter.Checked;
            tbRegexFilter.Enabled = regexFilterEnabled;
            cbRegexAppliedToFolder.Enabled = regexFilterEnabled;

            TryNavigateIntoFolder(currentFolder, false); //Reapply filters by refreshing view
        }

        private void ReapplyFiltersAndUpdateListView(object sender, EventArgs e)
        {
            //Reload to apply the filters
            TryNavigateIntoFolder(currentFolder, false);
        }

        private void tbRegexFilter_Leave(object sender, EventArgs e)
        {
            string userSpecifiedRegexPattern = tbRegexFilter.Text;

            if(userSpecifiedRegexPattern == String.Empty)
            {
                currentFilterRegex = null;
                TryNavigateIntoFolder(currentFolder, false); //refresh view
                return;
            }

            Regex compiledRegexPattern = null;

            try
            {
                compiledRegexPattern = new Regex(userSpecifiedRegexPattern, RegexOptions.Compiled);
                tbRegexFilter.BackColor = Color.White;
            }
            catch
            {
                //Invalid regex
                compiledRegexPattern = null;
                tbRegexFilter.BackColor = Color.Pink;
            }

            currentFilterRegex = compiledRegexPattern;
            TryNavigateIntoFolder(currentFolder, false); //refresh view
        }

        //Repopulates Listview according to the order of the given array +
        //Updates "displayedFilesAndDirectories" attribute.
        private void RepopulateListView(FileSystemInfo[] fileList)
        {
            lvDirectories.SuspendLayout();

            displayedFilesAndDirectories = fileList;

            lvDirectories.Items.Clear();

            foreach (FileSystemInfo fsInfo in fileList)
            {
                string[] fields = new string[]{
                    fsInfo.Name,
                    Helpers.IsDir(fsInfo) ? "Folder" : "File",
                    fsInfo.LastWriteTime.ToString()
                };

                ListViewItem item = new ListViewItem(fields);
                lvDirectories.Items.Add(item);
            }

            lvDirectories.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

            lvDirectories.ResumeLayout();
        }

        private class FileSystemInfoComparer : IComparer<FileSystemInfo>
        {
            Func<FileSystemInfo, FileSystemInfo, int> fnComparer = null;
            public FileSystemInfoComparer(Func<FileSystemInfo, FileSystemInfo, int> _fnComparer)
            {
                fnComparer = _fnComparer;
            }

            public int Compare(FileSystemInfo x, FileSystemInfo y)
            {
                if (fnComparer == null) return 0; //no ordering.

                return fnComparer(x, y);
            }
        }

        private void lvDirectories_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            //Reorder
            int columnIndex = e.Column;

            //Flip status
            ColumnSortedByAscending[columnIndex] = !ColumnSortedByAscending[columnIndex];
            bool sortedByAscending = ColumnSortedByAscending[columnIndex];

            FileSystemInfoComparer sortCondition = null;

            switch (columnIndex)
            {
                case (int)ColumnIndex.Name:
                    sortCondition = new FileSystemInfoComparer((FileSystemInfo fs1, FileSystemInfo fs2) =>
                    {
                        return (sortedByAscending? 1 : -1) * String.Compare(fs1.Name, fs2.Name);
                    });
                    break;
                case (int)ColumnIndex.Type:
                    sortCondition = new FileSystemInfoComparer((FileSystemInfo fs1, FileSystemInfo fs2) =>
                    {
                        int relativeIntOrder = Convert.ToInt32(Helpers.IsDir(fs1)) - Convert.ToInt32(Helpers.IsDir(fs2));
                        return (sortedByAscending ? 1 : -1) * relativeIntOrder;
                    });
                    break;
                case (int)ColumnIndex.LastModified:
                    sortCondition = new FileSystemInfoComparer((FileSystemInfo fs1, FileSystemInfo fs2) =>
                    {
                        return (sortedByAscending ? 1 : -1) * DateTime.Compare(fs1.LastWriteTime, fs2.LastWriteTime);
                    });
                    break;
            }

            FileSystemInfo[] newSortedOrder = displayedFilesAndDirectories
                .OrderBy(fs => fs, sortCondition)
                .ToArray();

            RepopulateListView(newSortedOrder);
        }
    }
}
