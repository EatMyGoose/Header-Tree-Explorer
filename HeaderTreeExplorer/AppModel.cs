using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Windows.Forms; //Msgbox

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

            TryWriteTextToFile(reportText, savePath);
        }

        public void GenerateDotHeaderGraph(string savePath)
        {
            var includeImpactDict = new Dictionary<MainForm.IncludeImpactCriteria, GraphVisualizationReport.IncludeWeightCriteria>
            {
                {MainForm.IncludeImpactCriteria.LOC,  GraphVisualizationReport.IncludeWeightCriteria.NumLOC},
                {MainForm.IncludeImpactCriteria.numFiles,  GraphVisualizationReport.IncludeWeightCriteria.NumFiles},
            };

            var includeWeightCriteria = includeImpactDict[viewController.GetIncludeImpactCriteria()];

            List<BaseDotNode> dotFileNodes = GraphVisualizationReport.Generate(
                selectedFiles.ToArray(),
                libraryDirectories.ToArray(),
                additionalIncludeDirectories.ToArray(),
                includeWeightCriteria
            );

            string reportFileContents = GraphVisualizationReport.GenerateDotFileFromDotNodes(dotFileNodes);

            TryWriteTextToFile(reportFileContents, savePath);
        }

        void RefreshFileList()
        {
            viewController.RedrawTreeview(selectedFiles.ToArray());
        }

        //"nothrow" wrapper, prints the builtin exception message if one is encountered.
        private void TryWriteTextToFile(string fileContents, string path) 
        {
            try
            {
                File.WriteAllText(path, fileContents);
            }
            catch(Exception ex)
            {
                //Notify user about failure.
                MessageBox.Show(
                    null,
                    $"Error writing to \"{path}\".\nDetails:\n{ex.ToString()}",
                    "Error writing to file",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
