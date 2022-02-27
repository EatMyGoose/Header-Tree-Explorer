using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HeaderTreeExplorer.Parser
{
    using FileNode = TNode<CppParserUtility.FileParams>;

    static class CppParserUtility
    {
        public class FileParams : IEquatable<FileParams>
        {
            public readonly string fullPath;
            public readonly int nLOCs;
            public readonly bool readSuccessfully;

            public FileParams(string _fullPath, bool _readSuccessfully, int _nLOCs)
            {
                fullPath = _fullPath;
                readSuccessfully = _readSuccessfully;
                nLOCs = _nLOCs;
            }

            public bool Equals(FileParams other) //Value comparison
            {
                return this.readSuccessfully == other.readSuccessfully &&
                       this.fullPath == other.fullPath &&
                       this.nLOCs == other.nLOCs;
            }
        }
        

        public enum HeaderType : Byte
        {
            File = 0,   //#include ""
            Library,    //#include <>
        }

        enum ParseStatus : Byte
        {
            None = 0,
            WithinSingleLineComment,
            WithinMultiLineComment
        }

        //Removes C style comments from a source file
        public static string StripComments(string original)
        {
            var stripped = new List<string>(); //To store the lines with the comments removed
            string[] lines = original.Split(new char[] { '\n' });

            ParseStatus status = ParseStatus.None;
            foreach(string line in lines)
            {
                int currentCharIndex = 0;
                string currentLine = "";
                while (currentCharIndex < line.Length)
                {
                    switch (status)
                    {
                        case ParseStatus.WithinSingleLineComment:
                            //Impossible to exit, just jump to next line
                            currentCharIndex = line.Length; 
                            status = ParseStatus.None;
                            break;
                        case ParseStatus.WithinMultiLineComment:
                            //Find corresponding "*/"
                            int endOfComment = line.IndexOf("*/", currentCharIndex);
                            if(endOfComment >= 0)
                            {
                                currentCharIndex = endOfComment + 2; //Advance in front of comment
                                status = ParseStatus.None;
                            }
                            else
                            {
                                currentCharIndex = line.Length; //Skip to end
                            }
                            break;
                        case ParseStatus.None:
                            //Find "//" or "/*"
                            int nextSingleLineComment = line.IndexOf(@"//", currentCharIndex);
                            int nextMultiLineComment = line.IndexOf(@"/*", currentCharIndex);

                            bool possibleSingleLineComment = nextSingleLineComment >= 0;
                            bool possibleMultiLineComment = nextMultiLineComment >= 0;
                    
                            ParseStatus nextStatus = ParseStatus.None;
                            int startOfNextSegment = line.Length;
                      
                            if(possibleMultiLineComment && possibleSingleLineComment)
                            {
                                //Whichever appeared first
                                nextStatus = (nextMultiLineComment < nextSingleLineComment) ? ParseStatus.WithinMultiLineComment : ParseStatus.WithinSingleLineComment;
                                startOfNextSegment = Math.Min(nextSingleLineComment, nextMultiLineComment);
                            }
                            else if(possibleMultiLineComment)
                            {
                                nextStatus = ParseStatus.WithinMultiLineComment;
                                startOfNextSegment = nextMultiLineComment;
                            }
                            else if (possibleSingleLineComment)
                            {
                                nextStatus = ParseStatus.WithinSingleLineComment;
                                startOfNextSegment = nextSingleLineComment;
                            }

                            int chars2Take = startOfNextSegment - currentCharIndex;
                            string nonCommentCharacters = line.Substring(currentCharIndex, chars2Take);
                            currentLine += nonCommentCharacters;

                            currentCharIndex = startOfNextSegment;
                            status = nextStatus;
                                break;
                    }
                }

                stripped.Add(currentLine);
            }
       
            return String.Join("\n", stripped);
        }

        //First match group = include type, Second match group = filename
        static Regex headerRegex = new Regex(@"#include\s+([<""])([^^<>:""|?*]+)[>""]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        //Performs comment removal before extraction.
        //List<(type:[Library|File], filename:string)>
        public static List<Tuple<HeaderType, string>> GetCppHeaders(string cppSourceWithoutComments)
        {
            var headers = new List<Tuple<HeaderType, string>>();

            foreach (Match match in headerRegex.Matches(cppSourceWithoutComments))
            {
                HeaderType type = (match.Groups[1].Value == "<") ? HeaderType.Library : HeaderType.File;
                string fileName = match.Groups[2].Value;
                headers.Add(new Tuple<HeaderType, string>(type, fileName));
            }

            return headers;
        }

        static char[] newlineSeparator = new char[] { '\n' }; 
        public static int CountNonEmptyLOCs(string cppSourceWithoutComments)
        {
            return cppSourceWithoutComments
                .Split(newlineSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Count(line => line.Trim().Length > 0); //AKA non-empty lines
        }

        static Tuple<Exception, string> TryReadFile(string fullPathName)
        {
            try
            {
                string contents = File.ReadAllText(fullPathName);
                return new Tuple<Exception, string>(null, contents);
            }
            catch(Exception ex)
            {
                return new Tuple<Exception, string>(ex, string.Empty);
            }
        }

        //Null if none could be found
        static string FindFullPathForFile(string path, string currentDir, string[] directories)
        {
            //Sanity check, since IsPathRooted throws if invalid path chars are detected
            if(path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                Console.Error.WriteLine($"Error, path<{path}> in directory<{currentDir}> has invalid path characters");
                return null;
            }

            //Already a full path
            if(Path.IsPathRooted(path))
            {
                return File.Exists(path) ? path : null;
            }

            //Search current dir.
            if(currentDir != null)
            {
                string absolutePath = Path.GetFullPath(Path.Combine(currentDir, path));
                if(File.Exists(absolutePath))
                {
                    return absolutePath;
                }
            }

            if(directories != null)
            {
                //Search additional include/lib directories
                var fullPathCandidates = directories.Select(dir => Path.Combine(dir, path));
                return fullPathCandidates.FirstOrDefault(fullPath => File.Exists(fullPath)); //Null if not found.
            }

            return null;
        }

        //filesAndIncludeNames - specify files that could not be loaded by setting the dict value to null
        static Dictionary<string, FileNode> BuildFileNodeGraph(Dictionary<string, string[]> filesAndIncludeNames, Dictionary<string, int> nLOCsPerFile)
        {
            var fileGraph = new Dictionary<string, FileNode>();
            //Construct nodes first
            foreach (var keyVal in filesAndIncludeNames)
            {
                bool successfullyParsed = keyVal.Value != null;
                string fileName = keyVal.Key; //Depending on whether the file could be opened, may or may not be a full pathname
                int nLOCs = nLOCsPerFile[fileName];
                fileGraph.Add(keyVal.Key, new FileNode(new FileParams(fileName, successfullyParsed, nLOCs), null));
            }

            //Then establish parent-child relationships
            foreach (var keyVal in filesAndIncludeNames)
            {
                //if (!fileGraph.ContainsKey(keyVal.Key)) continue; //Failed to even parse file, skip.
                FileNode fNode = fileGraph[keyVal.Key];
                string[] includesPaths = keyVal.Value ?? Array.Empty<string>(); ;
                List<FileNode> includes = includesPaths
                    .Select(path => fileGraph.ContainsKey(path) ? fileGraph[path] : null)
                    .Where(node => node != null)
                    .ToList();

                fNode.children = includes.ConvertAll(fInfo => (TNode<FileParams>) fInfo); //assign children.
            }

            return fileGraph;
        }

        //Requires full paths for all paths
        public static List<FileNode> ParseAllFiles(string[] fullFileNames, string[] libraryDirectories, string[] includeDirectories)
        {
            //dict<string:fullPath, (fullPathOfIncludedFiles:string[], nLOCs:int)>
            var parsedFiles = new Dictionary<string, Tuple<string[], int>>(); //For files which could not be parsed, the include list will be null.

            /*Currently unused, keeps a record of files that couldn't be opened for one reason or another*/
            //dict<string:fullPath, info:Exception> : Error history for files which could not be opened
            var errorHistory = new Dictionary<string, Exception>();
            //dict<fullPath:string, list of includes that couldn't be found: string[]>
            var includeErrorHistory = new Dictionary<string, string[]>();

            foreach(string srcFilePath in fullFileNames)
            {
                var remainingFiles = new Stack<string>();
                remainingFiles.Push(srcFilePath);

                while (remainingFiles.Count() > 0)
                {
                    string currentFile = remainingFiles.Pop();
                    if (parsedFiles.ContainsKey(currentFile)) continue;

                    string currentDir = Path.GetDirectoryName(currentFile);
                    Tuple<Exception, string> readResult = TryReadFile(currentFile);
                    Exception readException = readResult.Item1;
                    string fileText = readResult.Item2;
                     
                    if(readException != null) //Failed to open file
                    {
                        errorHistory.Add(currentFile, readException);
                        parsedFiles.Add(currentFile, Tuple.Create<string[], int>(null, 0));
                        continue;
                    }

                    //TODO: unify assignment of parsedFiles/locsInFiles instead of having them in multiple places
                    string strippedOfComments = StripComments(fileText);
                    int nNonEmptyLOCs = CountNonEmptyLOCs(strippedOfComments);

                    List<Tuple<HeaderType, string>> headerList = GetCppHeaders(strippedOfComments);
                    var headerPaths = headerList.Select(t =>
                    {
                        string[] additionalSearchDir = (t.Item1 == HeaderType.File) ? includeDirectories : libraryDirectories;
                        string headerIncludePath = t.Item2; //i.e. Header as specified in the #include definition
                        string headerFullPath = FindFullPathForFile(headerIncludePath, currentDir, additionalSearchDir); //Full path (if found), otherwise null
                        return Tuple.Create(headerIncludePath, headerFullPath);
                    });

                    string[] includedFiles = headerPaths.Select(t => {
                        string relPath = t.Item1;
                        string fullPath = t.Item2;
                        return fullPath ?? relPath; 
                    }).ToArray();
                    parsedFiles.Add(currentFile, Tuple.Create(includedFiles, nNonEmptyLOCs));

                    string[] unfindableIncludes = headerPaths.Where(t => t.Item2 == null).Select(t => t.Item1).ToArray();
                    if(unfindableIncludes.Length > 0)
                    {
                        includeErrorHistory.Add(currentFile, unfindableIncludes);
                        foreach(string relPath in unfindableIncludes)
                        {
                            //Mark them as parsed, but with null includes to signify read error
                            if (!parsedFiles.ContainsKey(relPath)) parsedFiles.Add(relPath, Tuple.Create<string[], int>(null, 0));
                        }
                    }

                    //Only search for header files that whose full path can be found on disk
                    var foundHeaderFiles = headerPaths.Where(t => t.Item2 != null).Select(t => t.Item2);
                    foreach(string fullIncludePath in foundHeaderFiles)
                    {
                        remainingFiles.Push(fullIncludePath);
                    }
                }
            }

            Dictionary<string, string[]> fileAndIncludeNames = parsedFiles.ToDictionary(kv => kv.Key, kv => kv.Value.Item1);
            Dictionary<string, int> fileLOCDict = parsedFiles.ToDictionary(kv => kv.Key, kv => kv.Value.Item2);

            Dictionary<string, FileNode> fileGraph = BuildFileNodeGraph(fileAndIncludeNames, fileLOCDict);
           
            return fullFileNames
                .Select(fullPath => fileGraph.ContainsKey(fullPath)? fileGraph[fullPath] : null)
                .ToList();
        }

        public static string PrependNextParentDirToPath(string relPath, string fullPath)
        {
            int relPathStartInd = fullPath.IndexOf(relPath);

            if(relPathStartInd < 0) //rel path isn't even part of fullpath
            {
                return relPath;
            }

            if(relPathStartInd == 0)//rel path == fullpath
            {
                return relPath;
            }

            string parentDir = fullPath.Substring(0, relPathStartInd);
            string directParentDir = Path.GetDirectoryName(parentDir);

            bool dirExists = directParentDir != null && directParentDir != string.Empty;

            if(!dirExists)
            {
                return fullPath; //Probably a root path without the drive letter.
            }

            return Path.Combine(parentDir, relPath); //Add direct parent dir.
        }

        public static Dictionary<string, string> GetShortestDistinctFilePaths(IEnumerable<string> fullPathNames)
        {
            Func<IEnumerable<string>, bool> AllElementsAreUnique = (IEnumerable<string> strSeq) =>
            {
                return strSeq.Distinct().Count() == strSeq.Count();
            };

            IEnumerable<KeyValuePair<string, string>> fullPathToAbbreviationDict =
                fullPathNames
                .Select(fullPath => new KeyValuePair<string, string>(fullPath, Path.GetFileName(fullPath)))
                .GroupBy(t => t.Value)      //Group by common file names & begin to resolve conflicts
                    .Select(grp => 
                    {
                        //key = fullPath
                        Dictionary<string, string> abbreviatedNamesDict = grp.ToDictionary(kv => kv.Key, kv => kv.Value);
                        HashSet<string> diffentiatableSet = new HashSet<string>(abbreviatedNamesDict.Keys.ToList());

                        //Progressively add parent directories to the filenames until they are all unique (if possible)
                        while (!AllElementsAreUnique(diffentiatableSet.Select(fullPath => abbreviatedNamesDict[fullPath])))
                        {
                            foreach(string fullPath in diffentiatableSet)
                            {
                                string prevAbbr = abbreviatedNamesDict[fullPath]; //i.e. folder1/file.h
                                string withAdditionalParent = PrependNextParentDirToPath(prevAbbr, fullPath); //i.e. folder2/folder1/file.h

                                if (prevAbbr == withAdditionalParent) //i.e. FullPath not specified or already at root.
                                {
                                    //Unable to make path more specific, remove it from the set of paths to be compared for distinctness
                                    diffentiatableSet.Remove(fullPath);
                                }

                                abbreviatedNamesDict[fullPath] = withAdditionalParent;
                            }
                        }

                        return abbreviatedNamesDict.ToList();
                    })
                .SelectMany(kvList => kvList);  //Flatten out keyValue lists

            return fullPathToAbbreviationDict.ToDictionary(kv => kv.Key, kv => kv.Value);
        }
    }
}
