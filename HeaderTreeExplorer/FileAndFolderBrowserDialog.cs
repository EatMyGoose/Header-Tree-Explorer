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

        private const string defaultDir = @"C:\";
        public string initialDirectory = String.Empty;

        private Stack<string> folderHistory = new Stack<string>();

        public bool selectableFolders = false;
        public bool selectableFiles = true;
        public bool multiSelect = true;

        public string fileFilters = "All Files(*.*)|*.*";

        //(displayedTitle:string, extensions(including the dot):string[])
        private Tuple<string, string[]>[] extensionFilters = new Tuple<string, string[]>[] { };
        private Tuple<string, string[]>[] defaultExtensionFilter = new Tuple<string, string[]>[] { Tuple.Create("All Files(*.*)", new string[] { "*" }) };

        Regex currentFilterRegex = null; //null = no filter, 

        public static bool IsDir(FileSystemInfo fs)
        {
            return Convert.ToBoolean(fs.Attributes & FileAttributes.Directory);
        }

        private Tuple<string, string[]>[] GetExtensionFilters(string format)
        {
            string[] tokenList = format.Split('|');

            //(displayedTitle:string, extensions(without the dot):string[])
            var extensionFilters = new List<Tuple<string, string[]>>();

            //Must process in pairs, only iterate up to the highest set of pairs
            int maxPairLength = (tokenList.Length % 2 == 0) ? tokenList.Length : tokenList.Length - 1;
            for (int index = 0; index < maxPairLength; index += 2)
            {
                string description = tokenList[index]; 
                string extensions = tokenList[index + 1];

                string[] extensionList = extensions
                    .Split(';') //"*.*;*.csv;" => [*.*, *.csv]
                    .Select(str => str.Split('.'))  
                    .Where(strPair => strPair.Length == 2)
                    .Select(strPair => "." + strPair[1].ToLower().Trim())
                    .ToArray();

                extensionFilters.Add(new Tuple<string, string[]>(description, extensionList));
            }
            return extensionFilters.ToArray();
        }

        [DllImport("shell32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern bool SHGetPathFromIDListW(
            IntPtr pidl,
            [MarshalAs(UnmanagedType.LPTStr)]
            StringBuilder pszPath);

        private static string GetPathFromIDList(byte[] idList, int offset)
        {
            int nBytesToCopy = idList.Length - offset;

            int buffer = Math.Max(nBytesToCopy, 520); 
            var sb = new StringBuilder(buffer);

            IntPtr ptr = Marshal.AllocHGlobal(idList.Length);
            
            Marshal.Copy(idList, offset, ptr, idList.Length - offset);

            bool result = SHGetPathFromIDListW(ptr, sb);
            Marshal.FreeHGlobal(ptr);

            return result ? sb.ToString() : string.Empty;
        }

        public static string GetDefaultDir()
        {
            bool installedOSAboveVista = Environment.OSVersion.Version.Major >= 6;
            string defaultFileBrowserDialogFolderRegKey = installedOSAboveVista ?
                @"Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\LastVisitedPidlMRU" :
                @"Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\LastVisitedMRU\";

            var regValues = Registry.CurrentUser.OpenSubKey(defaultFileBrowserDialogFolderRegKey, false);

            string currentAppName = System.AppDomain.CurrentDomain.FriendlyName;
            int wCharAppNameLength = currentAppName.Length * 2;

            foreach (string valueName in regValues.GetValueNames())
            {
                byte[] data = (byte[])regValues.GetValue(valueName);
                if (data == null) continue;

                string entryAppName = Encoding.Unicode.GetString(data, 0, wCharAppNameLength);

                if(entryAppName == currentAppName)
                {
                    //Matching registry entry
                    int offset = (entryAppName.Length + 1) * 2; //wchars + null terminator
                    string lastAccessedFile = GetPathFromIDList(data, offset);

                    return string.IsNullOrEmpty(lastAccessedFile) ? defaultDir : lastAccessedFile;
                }
            }

            //No matching registry entry
            return defaultDir;
        }

        public FileAndFolderBrowserDialog()
        {
            InitializeComponent();

            //setup columns

            lvDirectories.Columns.Add("Name");
            lvDirectories.Columns.Add("Type"); //["File"|"Folder"]
            lvDirectories.Columns.Add("Last Modified"); //Displayed format based on system settings

            //Event registration
            OnRowDoubleClicked += NavigateIntoClickedFolderInListView;

            cbUseFilter.CheckedChanged += ReapplyFiltersAndUpdateListView;
            cbFileFilter.SelectedIndexChanged += ReapplyFiltersAndUpdateListView;
        }

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
            var parsedExtensionFilters = GetExtensionFilters(fileFilters);
            extensionFilters = parsedExtensionFilters.Length > 0? parsedExtensionFilters : defaultExtensionFilter;

            foreach(string filterDescription in extensionFilters.Select(pair => pair.Item1))
            {
                cbFileFilter.Items.Add(filterDescription);
            }
            cbFileFilter.SelectedIndex = 0; //guaranteed to have at least one item

            //Update listview
            initialDirectory = string.IsNullOrWhiteSpace(initialDirectory) ? GetDefaultDir() : initialDirectory;

            bool couldLoadInitialDir = TryNavigateIntoFolder(initialDirectory, false);

            //If the requested directory cannot be read, load C:\ instead
            if (!couldLoadInitialDir)
            {
                bool couldLoadDefaultDir = TryNavigateIntoFolder(defaultDir, false);

                if (couldLoadDefaultDir)
                {
                    MessageBox.Show($"Unable to read \"{initialDirectory}\", loading \"{defaultDir}\" instead.", "Unable to read directory",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Asterisk);
                }
                else //Leave listview unpopulated.
                {
                    MessageBox.Show($"Unable to read \"{initialDirectory}\" or \"{defaultDir}\"", "Unable to read directory",
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
                .Where(fileInfo => IsDir(fileInfo))
                .Select(fileInfo => fileInfo.FullName)
                .ToArray();
        }

        public string[] GetSelectedFiles()
        {
            int[] selectedIndices = lvDirectories.SelectedIndices.Cast<int>().ToArray();
            return selectedIndices
                .Select(index => displayedFilesAndDirectories[index])
                .Where(fileInfo => !IsDir(fileInfo))
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
            Tuple<bool, FileSystemInfo[]> subFolderInfo = TryListDirectory(path);
            bool readSuccessful = subFolderInfo.Item1;

            if (!readSuccessful) return false;


            if(appendPrevPathToHistory) folderHistory.Push(currentFolder);
     
            UpdateListView(subFolderInfo.Item2, path);

            EnableBackButtonIfNonEmptyHistory();
            
            return true;
        }

        private void NavigateIntoClickedFolderInListView(int index)
        {
            FileSystemInfo clickedItem = displayedFilesAndDirectories[index];

            if (!IsDir(clickedItem)) return; //double clicked on file, ignore

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

        private FileSystemInfo[] ApplyFileExtensionFilters(FileSystemInfo[] rawSelection, string[] selectedExtensions)
        {
            return rawSelection
                .Where(fs =>
                {
                    if (IsDir(fs)) return true; //Only filter files

                    string fileExtension = Path.GetExtension(fs.Name).ToLower().Trim();

                    return selectedExtensions.Any(filter => filter == ".*" || filter == fileExtension); 
                })
                .ToArray();
        }

        private FileSystemInfo[] ApplyRegexFilters(FileSystemInfo[] rawSelection, Regex regPattern, bool applyToFolders)
        {
            return rawSelection
                .Where(fs =>
                {
                    if (regPattern == null) return true;

                    if (IsDir(fs) && !applyToFolders) return true;

                    return regPattern.IsMatch(fs.Name);
                })
                .ToArray();
        }

        private void UpdateListView(FileSystemInfo[] directories, string currentDirectory)
        {
            //Update displayed directory
            string fullPath = Path.GetFullPath(currentDirectory); 
            currentFolder = fullPath;
            tbCurrentDirectory.Text = fullPath;

            //Populate listview
            lvDirectories.Items.Clear();

            //Apply file extension filters first
            string[] selectedExtensionFilters = extensionFilters[cbFileFilter.SelectedIndex].Item2;
            var selectionAfterFileFilters = ApplyFileExtensionFilters(directories, selectedExtensionFilters);

            //Then apply regex filters
            Regex reg = cbUseFilter.Checked ? currentFilterRegex : null; //Disable regex filtering if not enabled
            bool applyToFolders = cbRegexAppliedToFolder.Checked; 
            var selectionAfterRegexFilter = ApplyRegexFilters(selectionAfterFileFilters, reg , applyToFolders);

            RepopulateListView(selectionAfterRegexFilter);
        }

        private static Tuple<bool, FileSystemInfo[]> TryListDirectory(string dirPath)
        {
            if(!Directory.Exists(dirPath))
            {
                return Tuple.Create(false, new FileSystemInfo[]{ });
            }

            var dirInfo = new DirectoryInfo(dirPath);
            FileSystemInfo[] fileInfo = dirInfo.GetFileSystemInfos();

            return Tuple.Create(true, fileInfo);
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

            TryNavigateIntoFolder(currentFolder, false); //refresh view
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
            displayedFilesAndDirectories = fileList;

            lvDirectories.Items.Clear();

            foreach (FileSystemInfo fsInfo in fileList)
            {
                string[] fields = new string[]{
                    fsInfo.Name,
                    IsDir(fsInfo) ? "Folder" : "File",
                    fsInfo.LastWriteTime.ToString()
                };

                ListViewItem item = new ListViewItem(fields);
                lvDirectories.Items.Add(item);
            }

            lvDirectories.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
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
                        int relativeIntOrder = Convert.ToInt32(IsDir(fs1)) - Convert.ToInt32(IsDir(fs2));
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
