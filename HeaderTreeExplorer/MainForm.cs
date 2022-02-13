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
        AppModel appModel;

        public MainForm()
        {
            InitializeComponent();

            appModel = new AppModel(this);

            cbReportType.SelectedIndex = 0; //Frequency Report

            RedrawTreeview(Array.Empty<string>());
        }

        public void RedrawTreeview(string[] newPaths)
        {
            char[] fileSeparatorTokens = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
            List<string[]> fragmentedPaths = newPaths.Select(fullPath => fullPath.Split(fileSeparatorTokens)).ToList();

            //Build the drive tree.
            const string driveRootDefaultValue = "";
            TNode<string> driveRoot = TNode<string>.GenerateTrie(fragmentedPaths, driveRootDefaultValue);

            //Condense Tree
            TNode<string> condensed = TNode<string>.CondenseTree(driveRoot, (string first, string second) => Path.Combine(first, second));

            //Copy to TreeNode
            TreeNode treeViewRoot = new TreeNode(condensed.nodeValue);

            Queue<TNode<string>> srcFrontier = new Queue<TNode<string>>();
            Queue<TreeNode> destFrontier = new Queue<TreeNode>();

            srcFrontier.Enqueue(condensed);
            destFrontier.Enqueue(treeViewRoot);
            while(srcFrontier.Count > 0)
            {
                TNode<string> srcNode = srcFrontier.Dequeue();
                TreeNode destNode = destFrontier.Dequeue();

                //Generate children
                foreach(TNode<string> child in srcNode.children)
                {
                    TreeNode newDestChild = destNode.Nodes.Add(child.nodeValue);
                    srcFrontier.Enqueue(child);
                    destFrontier.Enqueue(newDestChild);
                }
            }

            tvSelectedFiles.Nodes.Clear();
            tvSelectedFiles.Nodes.Add(treeViewRoot);
        }

        private void BtnLoadFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog of = new OpenFileDialog())
            {
                of.RestoreDirectory = true;
                of.Title = "Select Source Files";
                of.Filter = "C++ Source Files (*.h;*.cpp;*.hpp;*.cxx)|*.h;*.cpp;*.hpp;*.cxx|All files (*.*)|*.*";
                of.Multiselect = true;
                of.CheckPathExists = true;
                if(of.ShowDialog() == DialogResult.OK)
                {
                    appModel.AddFiles(of.FileNames);
                }
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
                {"Include Frequency",
                       new Tuple<string,Action<string>>(
                           "txt File (*.txt)|*.txt",
                            (string filename) => appModel.GenerateFrequencyReport(filename))
                },
                { "[.dot/.gv] Header Graph" ,
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
    }
}
