using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HeaderTreeExplorer.Parser;

namespace HeaderTreeExplorer
{
    using Util = CppParserUtility;
    using FileNode = TNode<CppParserUtility.FileParams>;

    static class FreqencyReport
    {

        public class FileInfo
        {
            public string PathName;
            public int IncludeCount;

            public FileInfo(string _pathName, int _includeCount)
            {
                PathName = _pathName;
                IncludeCount = _includeCount;
            }
        }

        //Returns a list of files sorted in descending order of include frequency.
        public static FileInfo[] Generate(string[] filePaths, string[] libraryDirectories, string[] includeDirectories)
        {
            
            List<FileNode> fileGraph = Util.ParseAllFiles(filePaths, libraryDirectories, includeDirectories);

            Dictionary<string, int> includeCounts = new Dictionary<string, int>();

            //Include each file once individually for each translation unit.
            foreach (FileNode rootNode in fileGraph)
            {
                //To manage circular dependencies
                HashSet<string> visitedFiles = new HashSet<string>();
                Queue<FileNode> frontier = new Queue<FileNode>();
                frontier.Enqueue(rootNode);
                visitedFiles.Add(rootNode.nodeValue.fullPath);

                while(frontier.Count() > 0)
                {
                    FileNode currentNode = frontier.Dequeue();

                    string filePath = currentNode.nodeValue.fullPath;
                    if (!includeCounts.ContainsKey(filePath))
                    {
                        includeCounts.Add(filePath, 0);
                    }
                    includeCounts[filePath] += 1;

                    foreach(FileNode childNode in currentNode.children)
                    {
                        bool hasNotBeenVisited = visitedFiles.Add(childNode.nodeValue.fullPath);
                        if(hasNotBeenVisited) frontier.Enqueue(childNode);
                    }
                }
            }

            return includeCounts
                .Select(keyValue => new FileInfo(keyValue.Key, keyValue.Value))
                .OrderByDescending(fInfo => fInfo.IncludeCount)
                .ToArray();
        }
    }
}
