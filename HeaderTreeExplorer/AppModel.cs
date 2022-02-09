using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HeaderTreeExplorer
{
    class AppModel
    {
        public delegate void FileSelectionChangedHandler();
        public event FileSelectionChangedHandler OnFileSelectionChanged;

        #region owned members
        HashSet<string> selectedFiles = new HashSet<string>();
        HashSet<string> libraryDirectories = new HashSet<string>();
        HashSet<string> additionalIncludeDirectories = new HashSet<string>();

        #endregion owned members

        #region references
        MainForm viewController; //Reference to view controller
        #endregion region references

        public AppModel(MainForm _viewController)
        {
            viewController = _viewController;
            OnFileSelectionChanged += RefreshFileList;
        }

        public string[] GetLibraryDirectories()
        {
            return libraryDirectories.ToArray();
        }

        public void SetLibraryDirectories(IEnumerable<string> directoryList)
        {
            libraryDirectories = new HashSet<string>(directoryList);
        }

        public string[] GetAdditionalIncludeDirectories()
        {
            return additionalIncludeDirectories.ToArray();
        }

        public void SetAdditionalIncludeDirectories(IEnumerable<string> directoryList)
        {
            additionalIncludeDirectories = new HashSet<string>(directoryList);
        }

        public void AddFiles(string[] additionalFiles)
        {
            selectedFiles.UnionWith(additionalFiles);
            OnFileSelectionChanged();
        }

        public void ClearFileSelection()
        {
            selectedFiles.Clear();
            OnFileSelectionChanged();
        }

        public void DeleteFileSelection(string[] toBeDeleted)
        {
            selectedFiles.ExceptWith(toBeDeleted);
            OnFileSelectionChanged();
        }

        public void GenerateFrequencyReport(string savePath)
        {
            FreqencyReport.FileInfo[] sortedFileFreqList = FreqencyReport.Generate(
                selectedFiles.ToArray(),
                libraryDirectories.ToArray(),
                additionalIncludeDirectories.ToArray()
                );

            var reportLines = sortedFileFreqList.Select(fInfo => $"{fInfo.IncludeCount, 6}:{fInfo.PathName}");
            var reportText = String.Join("\n", reportLines);

            File.WriteAllText(savePath, reportText);
        }

        void RefreshFileList()
        {
            viewController.RedrawTreeview(selectedFiles.ToArray());
        }
   
    }
}
