using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using HeaderTreeExplorer.Parser;

namespace HeaderTreeExplorer
{
    using Util = CppParserUtility;
    using FileNode = TNode<CppParserUtility.FileParams>;

    //TODO: Generalize directed graph class to minimize back & front conversions.
    abstract class BaseDotNode
    {
        Dictionary<string, string> attributes = new Dictionary<string, string>();

        public BaseDotNode[] outboundEdges = new BaseDotNode[]{};

        public string GenerateNodeAttr()
        {
            //space separated pairs of `key="value"`
            return $"[{String.Join(" ", attributes.Select(kv => kv.Key + '=' + '"' + kv.Value + '"'))}]";
        }

        protected bool AddAttribute(string key, string value)
        {
            if(!attributes.ContainsKey(key))
            {
                attributes.Add(key, value);
                return true;
            }
            else
            {
                Console.Error.WriteLine($"Error, inserting<{key},{value}> - key<{key}> already exists with value {attributes[key]}");
                return false;
            }
        }
    }

    //TODO: -> Change to an attribute list bucket, and create a "builder" class for nodes instead
    class BasicDotNode : BaseDotNode
    {
        protected string abbreviatedName = "";
        protected string fullPathName = "";

        public BasicDotNode(string _abbrName, string _fullPathName)
        {
            abbreviatedName = _abbrName;
            fullPathName = _fullPathName;

            AddAttribute("label", abbreviatedName);
            AddAttribute("tooltip", fullPathName);
        }
    }

    class ColouredDotNode : BasicDotNode
    {
        Color color = new Color();

        private static string GetColourHexString(Color color)
        {
            return $"#{color.R.ToString("x2")}{color.G.ToString("x2")}{color.B.ToString("x2")}{color.A.ToString("x2")}";
        }

        public ColouredDotNode(string _abbrName, string _fullPathName, Color _color)
            : base(_abbrName, _fullPathName)
        {
            color = _color;
            AddAttribute("style", "filled");
            AddAttribute("fillcolor", GetColourHexString(color));
        }
    }

    
    static class GraphVisualizationReport
    {

        public struct IncludeWeightParams
        {
            public readonly int directIncludeCount;  //number of files that include this particular file
            public readonly int totalFilesIncluded;  //number of files that this file directly/indirectly includes

            public readonly int totalLOCsIncluded;   //Total LOCs in the include tree of this file

            public IncludeWeightParams(int _directIncludeCount, int _totalFilesIncluded, int _totalLOCsIncluded)
            {
                directIncludeCount = _directIncludeCount;
                totalFilesIncluded = _totalFilesIncluded;
                totalLOCsIncluded = _totalLOCsIncluded;
            }

            public int GetFileBasedIncludeWeight()
            {
                return (1 + directIncludeCount) * (1 + totalFilesIncluded);
            }

            public int GetLOCBasedIncludeWeight()
            {
                return (1 + directIncludeCount) * (totalLOCsIncluded);
            }
        }

        static Color LerpColours(Color c1, Color c2, float fracOfC2)
        {
            float fracOfC1 = 1.0f - fracOfC2;
            Func<byte, byte, byte> LerpByte = (b1, b2) =>
            {
                return (byte)(fracOfC1 * b1 + fracOfC2 * b2);
            };

            return Color.FromArgb(
                LerpByte(c1.A, c2.A),
                LerpByte(c1.R, c2.R),
                LerpByte(c1.G, c2.G),
                LerpByte(c1.B, c2.B)
                );
        }

        // Estimate the impact on compilation by multiplying the times a file is included by the number of includes 
        // (number of nodes it is directly/indirectly connected to)
        static Dictionary<string, IncludeWeightParams> GetIncludeWeight(List<FileNode> graphNodeSet)
        {

            //Construct bidirectional graph (nodes with both inbound and outbound edges)
            List<BiNode<CppParserUtility.FileParams>> biNodes = graphNodeSet
                .Select((node, index) =>  new BiNode<CppParserUtility.FileParams>(index, node.nodeValue))
                .ToList();

            Dictionary<FileNode, int> node2IndexDict = new Dictionary<FileNode, int>();
            for (int index = 0; index < graphNodeSet.Count(); index++)
            {
                node2IndexDict.Add(graphNodeSet[index], index);
            }

            //Construct edges
            for (int index = 0; index < graphNodeSet.Count(); index++)
            {
                var outwardEdges = graphNodeSet[index].children;
                var srcNode = biNodes[index];
                foreach (FileNode fn in outwardEdges)
                {
                    int destIndex = node2IndexDict[fn];
                    var destNode = biNodes[destIndex];

                    //forward edge
                    srcNode.edgeList.Add(new Edge(destIndex, index));
                    //add edge on the destination node as well
                    destNode.edgeList.Add(new Edge(destIndex, index));
                }
            }

            bool[] visitedIndices = new bool[biNodes.Count()];  //To avoid repeated memory allocations
            //(nodeIdx:int) => (nConnectedNodes:int, nLOCs:int)
            Func<int, Tuple<int,int>> countConnectedNodes = (int nodeIndex) =>
            {
                Array.Clear(visitedIndices, 0, visitedIndices.Length);
                visitedIndices[nodeIndex] = true;

                int nConnectedNodes = 0;
                int nTotalLOCs = biNodes[nodeIndex].value.nLOCs;
                Queue<int> frontier = new Queue<int>();
                frontier.Enqueue(nodeIndex);

                while(frontier.Count() > 0)
                {
                    int nextIdx = frontier.Dequeue();
                    var currentNode = biNodes[nextIdx];
                    foreach(Edge e in currentNode.edgeList)
                    {
                        int destNodeIdx = e.destId;
                        if (e.srcId == currentNode.nodeId &&
                           !visitedIndices[destNodeIdx])
                        {
                            frontier.Enqueue(destNodeIdx);
                            visitedIndices[destNodeIdx] = true;
                            nConnectedNodes += 1;
                            nTotalLOCs += biNodes[destNodeIdx].value.nLOCs;
                        }
                    }
                }

                return Tuple.Create(nConnectedNodes, nTotalLOCs);
            };

            //Bi-directional graph complete, now compilation impact by multiplying the no. of inward edges (no. of times the file was included)
            //by how many other nodes it is indirectly or directly connected to.
            //(fullPath:string, compilationImpact:int)
            var compilationImpactDict = new Dictionary<string, IncludeWeightParams>();

            for(int i = 0; i < biNodes.Count(); i++)
            {
                string fullFilename = biNodes[i].value.fullPath;
                //Essentially n^2 complexity, no simple way to reduce time complexity since this is a directed graph

                Tuple<int, int> nIncludesAndLOCs = countConnectedNodes(i);
                int nFilesIncluded = nIncludesAndLOCs.Item1;
                int nLOCsIncluded = nIncludesAndLOCs.Item2;
                
                //Number of inbound edges = no. of times this file is included
                //TODO: -> instead of just direct includes, count the no. of files that indirectly include it as well?
                int nTimesIncluded = biNodes[i].edgeList.Count(edge => edge.destId == i) + 1;  
                compilationImpactDict[fullFilename] = new IncludeWeightParams(nTimesIncluded, nFilesIncluded, nLOCsIncluded);
            }

            return compilationImpactDict;
        }

        public enum IncludeWeightCriteria
        {
            NumLOC, //Count using no. of lines of code included
            NumFiles //Count using no. of files included
        }

        //Returns every node within the graph
        public static List<BaseDotNode> Generate(string[] filePaths, string[] libraryDirectories, string[] includeDirectories, IncludeWeightCriteria weightCriteria)
        {
            List<FileNode> fileGraph = Util.ParseAllFiles(filePaths, libraryDirectories, includeDirectories);
            //bfs graph to get all filenames, then obtain their abbreviations
            List<FileNode> nodeList = FileNode.GetAllNodes(fileGraph);

            var fullPathList = nodeList.Select(fNode => fNode.nodeValue.fullPath);
            Dictionary<string,string> fullPathToAbbrDict = Util.GetShortestDistinctFilePaths(fullPathList);

            Func<IncludeWeightParams, int> GetWeight = (IncludeWeightParams weightInfo) => 
            {
                switch (weightCriteria)
                {
                    case IncludeWeightCriteria.NumFiles:
                        return weightInfo.GetFileBasedIncludeWeight();
                    case IncludeWeightCriteria.NumLOC:
                        return weightInfo.GetLOCBasedIncludeWeight();
                    default:
                        throw new NotImplementedException("Error, unhandled include weight criteria");
                }
            };

            Dictionary<string, IncludeWeightParams> includeImpactDict = GetIncludeWeight(nodeList);
            int maxWeight = includeImpactDict.Max(kv => GetWeight(kv.Value));

            //Then proceed to convert it to a BaseDotNode graph 
            //Return all nodes within the graph as a list.

            //Setup mapping between the references of the original graph + the converted graph
            Dictionary<FileNode, BaseDotNode> fNodeToBaseNodeDict = new Dictionary<FileNode, BaseDotNode>(); 

            List<BaseDotNode> dotNodes = nodeList.Select(fNode => {
                string fullPath = fNode.nodeValue.fullPath;
                string abbreviatedPath = fullPathToAbbrDict[fullPath];
                int nLOCs = fNode.nodeValue.nLOCs;
                IncludeWeightParams includeWeightParams = includeImpactDict[fullPath];

                string tooltip = string.Join("\n",
                        $"Full Path:{fullPathToAbbrDict[fullPath]}",
                        $"Total no. of Files Included:{includeWeightParams.totalFilesIncluded}",
                        $"LOCs(self):{nLOCs}",
                        $"LOCs(total included):{includeWeightParams.totalLOCsIncluded}"
                        );

                float includeWeightFrac = (float)GetWeight(includeWeightParams) / (float)maxWeight; // (most minor impact) 0 - 1 (heaviest impact)

                Color minorImpactColour = Color.FromArgb(255, 240, 241, 255); // off-white
                Color majorImpactColour = Color.FromArgb(255, 255, 107, 107); // dark pink

                Color nodeColor = LerpColours(minorImpactColour, majorImpactColour, includeWeightFrac);

                BaseDotNode convertedNode = new ColouredDotNode(abbreviatedPath, tooltip, nodeColor); //Edges not established yet
                //Set relationship
                fNodeToBaseNodeDict.Add(fNode, convertedNode);
                return convertedNode;
            }).ToList();

            //Add edges to DotNode graph.
            List<BaseDotNode> dotNodesWithEdges = nodeList.Zip(dotNodes, (fNode, baseNode) =>
            {
                baseNode.outboundEdges = fNode.children
                    .Select(childFNode => fNodeToBaseNodeDict[childFNode])
                    .ToArray();
               
                return baseNode;
            }).ToList();

            return dotNodesWithEdges;
        }

        public static string GenerateDotFileFromDotNodes(List<BaseDotNode> dotNodes)
        {
            //dict<node-object, string-id> : Generate a unique id for each node.
            var nodes = new Dictionary<BaseDotNode, string>();
            Func<string> GenerateNewNodeName = () => { return $"n{nodes.Count()}"; };
            
            foreach(BaseDotNode baseNode in dotNodes)
            {
                if (nodes.ContainsKey(baseNode)) continue; //Already explored

                Queue<BaseDotNode> frontier = new Queue<BaseDotNode>();
                nodes.Add(baseNode, GenerateNewNodeName());
                while(frontier.Count() > 0)
                {
                    BaseDotNode currentNode = frontier.Dequeue();
                    foreach(BaseDotNode adjNode in currentNode.outboundEdges)
                    {
                        if (nodes.ContainsKey(adjNode)) continue; //Already explored

                        frontier.Enqueue(adjNode);
                        nodes.Add(adjNode, GenerateNewNodeName());
                    }
                }
            }

            var flattenedNodes = nodes.OrderBy(keyVal => Int32.Parse(keyVal.Value.Substring(1))); //Orderby by ascending order of node ID

            StringBuilder fileBuilder = new StringBuilder();
            fileBuilder.Append("digraph {\n");

            //Write node definitions
            fileBuilder.AppendLine("//Node definitions\n\n");
            foreach (KeyValuePair<BaseDotNode,string> node in flattenedNodes)
            {
                string nodeId = node.Value;
                string nodeAttr = node.Key.GenerateNodeAttr();
                fileBuilder.AppendLine($@"{nodeId} {nodeAttr};");
            }

            //Write node edges.
            fileBuilder.AppendLine("//Edges\n\n");
            foreach (KeyValuePair<BaseDotNode, string> node in flattenedNodes)
            {
                string srcId = node.Value;
                BaseDotNode srcNode = node.Key;
                if (srcNode.outboundEdges.Count() == 0) continue; //Terminal node, skip

                foreach (BaseDotNode sinkNode in srcNode.outboundEdges)
                {
                    string destId = nodes[sinkNode];
                    fileBuilder.AppendLine($@"{srcId} -> {destId};");
                }
                fileBuilder.AppendLine(); //Separator between each source node for clarity.
            }

           
            //End graph definition
            fileBuilder.Append(@"}");

            return fileBuilder.ToString();
        }
    }
}
