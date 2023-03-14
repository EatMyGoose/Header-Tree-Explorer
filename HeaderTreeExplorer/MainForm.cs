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

using HeaderTreeExplorer.Parser;

namespace HeaderTreeExplorer
{
    public partial class MainForm : Form
    {
        const string CB_OPT_FREQ = "Include Frequency";
        const string CB_OPT_HEADER_GRAPH = "[.dot/.gv] Header Graph";

        AppModel appModel;

        FileAndFolderBrowserDialog loadFilesAndFoldersDialog;

        public enum IncludeImpactCriteria
        {
            numFiles,
            LOC,
        }

        public MainForm()
        {
            InitializeComponent();

            appModel = new AppModel(this);

            loadFilesAndFoldersDialog = new FileAndFolderBrowserDialog
            {
                fileFilters = string.Join("|", new string[] {
                    "C++ header files(*.h, *.hpp)|*.h;*.hpp",
                    "C++ implementation files(*.cpp)|*.cpp",
                    "C++ header and impl files(*.cpp, *.h, *.hpp)|*.cpp;*.h;*.hpp",
                    "All Files(*.*)|*.*",
                }),

                selectableFiles = true,
                selectableFolders = true,
                multiSelect = true
            };

            cbReportType.SelectedIndexChanged += (object sender, EventArgs e) => UpdateIncludeImpactGroupboxVisibility(); 
            cbReportType.SelectedIndex = 0; //Frequency Report
            
            RedrawTreeview(Array.Empty<string>());
        }

        void UpdateIncludeImpactGroupboxVisibility()
        {
            bool selectedHeaderGraph = cbReportType.SelectedItem.ToString() == CB_OPT_HEADER_GRAPH;
            gbIncludeImpact.Enabled = selectedHeaderGraph;
        }

        public IncludeImpactCriteria GetIncludeImpactCriteria()
        {
            if(rbIncludeImpactFiles.Checked == true)
            {
                return IncludeImpactCriteria.numFiles;
            }
            else if(rbIncludeImpactLOC.Checked == true)
            {
                return IncludeImpactCriteria.LOC;
            }

            throw new NotImplementedException("Error, unknown include impact criteria selected within UI");
        }

        public void RedrawTreeview(string[] newPaths)
        {
            char[] fileSeparatorTokens = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
            List<string[]> fragmentedPaths = newPaths.Select(fullPath => fullPath.Split(fileSeparatorTokens)).ToList();

            //Build the drive tree.
            const string driveRootDefaultValue = "";
            TTrieNode<string> driveRoot = TTrieNode<string>.GenerateTrie(fragmentedPaths, newPaths.ToList(), driveRootDefaultValue);

            //Condense Tree
            TTrieNode<string> condensed = TTrieNode<string>.CondenseTree(driveRoot, (string first, string second) => Path.Combine(first, second));

            //Copy to TreeNode
            TreeNode treeViewRoot = new TreeNode(condensed.nodeValue);

            Queue<TTrieNode<string>> srcFrontier = new Queue<TTrieNode<string>>();
            Queue<TreeNode> destFrontier = new Queue<TreeNode>();

            srcFrontier.Enqueue(condensed);
            destFrontier.Enqueue(treeViewRoot);
            while(srcFrontier.Count > 0)
            {
                TTrieNode<string> srcNode = srcFrontier.Dequeue();
                TreeNode destNode = destFrontier.Dequeue();

                //Generate children
                foreach(TTrieNode<string> child in srcNode.children)
                {
                    TreeNode newDestChild = destNode.Nodes.Add(child.nodeValue);
                    newDestChild.Name = child.isLeaf ? child.leafValue : "";
                    
                    srcFrontier.Enqueue(child);
                    destFrontier.Enqueue(newDestChild);
                }
            }

            tvSelectedFiles.Nodes.Clear();
            tvSelectedFiles.Nodes.Add(treeViewRoot);
        }

        private static List<string> GetAllNestedFilesInSubfolders(IEnumerable<string> subFolders, Func<string, bool> fileFilter)
        {
            List<string> filesInSubfolders = new List<string>();

            Stack<string> unprocessedFolders = new Stack<string>(subFolders);
            while(unprocessedFolders.Count() > 0)
            {
                string currentFolder = unprocessedFolders.Pop();

                string[] files = new string[] { };
                string[] subfolders = new string[] { };
                try
                {
                    files = Directory.GetFiles(currentFolder);
                    subfolders = Directory.GetDirectories(currentFolder);
                }
                catch
                {
                    //Ignore.
                }

                filesInSubfolders.AddRange(
                    files.Where(fullPath => fileFilter(fullPath))
                );
            }
            return filesInSubfolders;
        }

        private void BtnLoadFile_Click(object sender, EventArgs e)
        {
            if (loadFilesAndFoldersDialog.ShowDialog() == DialogResult.OK)
            {
                string[] filenames = loadFilesAndFoldersDialog.GetSelectedFiles();
                //For any selected subfolders, just recursively scan through them and find all
                //files matching the filename filters that were specified
                List<string> filesInSelectedDirectories = GetAllNestedFilesInSubfolders(
                        loadFilesAndFoldersDialog.GetSelectedFolders(),
                        loadFilesAndFoldersDialog.GetCurrentlySelectedExtensionFilter()
                    );
                
                appModel.AddFiles(filenames.Concat(filesInSelectedDirectories).ToArray());
            }
        }

        private void BtnDeleteAll_Click(object sender, EventArgs e)
        {
            DialogResult userOption = MessageBox.Show(
                "Clear selection (Y/N)\nConfirm?", 
                "Confirm Action", 
                MessageBoxButtons.OKCancel, 
                MessageBoxIcon.Exclamation);

            if (userOption == DialogResult.OK)
            {
                appModel.ClearFileSelection();
            }
        }

        private void BtnReportGenerate_Click(object sender, EventArgs e)
        {
            var reportTypeHandlers = new Dictionary<string, Tuple<string,Action<string>>>()
            {
                {CB_OPT_FREQ,
                       new Tuple<string,Action<string>>(
                           "txt File (*.txt)|*.txt",
                            (string filename) => appModel.GenerateFrequencyReport(filename))
                },
                {CB_OPT_HEADER_GRAPH,
                    new Tuple<string, Action<string>>(
                        "Dot File (*.gv;*.dot)|*.gv;*.dot",
                        (string filename) => appModel.GenerateDotHeaderGraph(filename))
                }
            };

            string selectedReportName = cbReportType.SelectedItem.ToString();
            
            if(!reportTypeHandlers.ContainsKey(selectedReportName))
            {
                MessageBox.Show(
                    $"Error, \"{selectedReportName}\" has not been implemented yet", 
                    "Report not implemented", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Information);
                return;
            }

            string saveFilterFormats = reportTypeHandlers[selectedReportName].Item1;
            Action<string> fnReportGenerator = reportTypeHandlers[selectedReportName].Item2;

            using (SaveFileDialog sf = new SaveFileDialog())
            {
                sf.RestoreDirectory = true;
                sf.Title = "Select save destination";
                sf.Filter = saveFilterFormats;
                if(sf.ShowDialog() == DialogResult.OK)
                {
                    fnReportGenerator(sf.FileName);
                }
            }   
        }

        private void btnLibDirectories_Click(object sender, EventArgs e)
        {
            var libDirSelectionForm = new AdditionalIncludeForm(
                appModel.GetLibraryDirectories(), 
                "Configure Library Directory Paths"
            );

            libDirSelectionForm.OnApply += (string[] newSelection) =>
            {
                appModel.SetLibraryDirectories(newSelection);
            };

            libDirSelectionForm.ShowDialog();
        }

        private void btnIncludeDirectories_Click(object sender, EventArgs e)
        {
            var additionalIncludeForm = new AdditionalIncludeForm(
                appModel.GetAdditionalIncludeDirectories(),
                "Configure Additional Include Directory Paths"
            );

            additionalIncludeForm.OnApply += (string[] newSelection) =>
            {
                appModel.SetAdditionalIncludeDirectories(newSelection);
            };

            additionalIncludeForm.ShowDialog();
        }

        private void btnImportVSProject_Click(object sender, EventArgs e)
        {
            using (var of = new OpenFileDialog())
            {
                of.Filter = "VS Project Files (vcxproj)|*.vcxproj";
                of.Title = "Select VS C++ Project";

                if(of.ShowDialog() == DialogResult.OK)
                {
                    appModel.IncludeAllHeadersWithinVSProject(of.FileName);
                }
            }
        }

        private static void RecursivelyCheckChildrenNodes(TreeNode node, bool isChecked)
        {
            foreach(TreeNode childNode in node.Nodes)
            {
                childNode.Checked = isChecked;
                RecursivelyCheckChildrenNodes(childNode, isChecked);
            }
        }
        
        private void tvSelectedFiles_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if(e.Action != TreeViewAction.Unknown)

            tvSelectedFiles.BeginUpdate();
            RecursivelyCheckChildrenNodes(e.Node, e.Node.Checked);
            tvSelectedFiles.EndUpdate();
        }
        
        void GetAllSelectedFilesInListview(TreeNode root, ref List<string> files)
        {
            if(root.Checked)
            {
                files.Add(root.Name);
            }

            foreach(TreeNode childNode in root.Nodes)
            {
                GetAllSelectedFilesInListview(childNode, ref files);
            }
        }

        private void btnDeleteSelection_Click(object sender, EventArgs e)
        {
            if (tvSelectedFiles.Nodes.Count == 0) return;

            List<string> selectedFiles = new List<string>();

            GetAllSelectedFilesInListview(tvSelectedFiles.Nodes[0], ref selectedFiles);

            appModel.DeleteFileSelection(selectedFiles.ToArray());
        }
    }
}
