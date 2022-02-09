using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HeaderTreeExplorer.Parser;

namespace HeaderTreeExplorer
{
    using Util = CppParserUtility;

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
            List<Util.FileNode> fileGraph = Util.ParseAllFiles(filePaths, libraryDirectories, includeDirectories);

            Dictionary<string, int> includeCounts = new Dictionary<string, int>();

            //Include each file once individually for each translation unit.
            foreach (Util.FileNode rootNode in fileGraph)
            {
                //To manage circular dependencies
                HashSet<string> visitedFiles = new HashSet<string>();
                Queue<Util.FileNode> frontier = new Queue<Util.FileNode>();
                frontier.Enqueue(rootNode);
                visitedFiles.Add(rootNode.fullPath);

                while(frontier.Count() > 0)
                {
                    Util.FileNode currentNode = frontier.Dequeue();

                    string filePath = currentNode.fullPath;
                    if (!includeCounts.ContainsKey(filePath))
                    {
                        includeCounts.Add(filePath, 0);
                    }
                    includeCounts[filePath] += 1;

                    foreach(Util.FileNode childNode in currentNode.includes)
                    {
                        bool hasNotBeenVisited = visitedFiles.Add(childNode.fullPath);
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
