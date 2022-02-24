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
        public BaseDotNode[] outboundEdges = new BaseDotNode[]{};
        public abstract string GenerateNodeAttr();
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
        }

        public override string GenerateNodeAttr()
        {
            return $"[label=\"{abbreviatedName}\" tooltip=\"{fullPathName}\"]";
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
        }

        public override string GenerateNodeAttr()
        {

            return $"[label=\"{abbreviatedName}\" tooltip=\"{fullPathName}\" fillcolor=\"{GetColourHexString(color)}\" style=filled]";
        }
    }

    
    static class GraphVisualizationReport
    {

        public static Color LerpColours(Color c1, Color c2, float fracOfC2)
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
        public static Dictionary<string, int> GetIncludeWeight(List<FileNode> graphNodeSet)
        {

            //Construct bidirectional graph (nodes with both inbound and outbound edges)
            List<BiNode<string>> biNodes = graphNodeSet
                .Select((node, index) =>  new BiNode<string>(index, node.nodeValue.fullPath))
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
                BiNode<string> srcNode = biNodes[index];
                foreach (FileNode fn in outwardEdges)
                {
                    int destIndex = node2IndexDict[fn];
                    BiNode<string> destNode = biNodes[destIndex];

                    //forward edge
                    srcNode.edgeList.Add(new Edge(destIndex, index));
                    //add edge on the destination node as well
                    destNode.edgeList.Add(new Edge(destIndex, index));
                }
            }

            bool[] visitedIndices = new bool[biNodes.Count()];  //To avoid repeated memory allocations
            Func<int, int> countConnectedNodes = (int nodeIndex) =>
            {
                Array.Clear(visitedIndices, 0, visitedIndices.Length);
                visitedIndices[nodeIndex] = true;

                int nConnectedNodes = 0;
                Queue<int> frontier = new Queue<int>();
                frontier.Enqueue(nodeIndex);
                while(frontier.Count() > 0)
                {
                    int nextIdx = frontier.Dequeue();
                    BiNode<string> currentNode = biNodes[nextIdx];
                    foreach(Edge e in currentNode.edgeList)
                    {
                        if(e.srcId == currentNode.nodeId &&
                           !visitedIndices[e.destId])
                        {
                            frontier.Enqueue(e.destId);
                            visitedIndices[e.destId] = true;
                            nConnectedNodes += 1;
                        }
                    }
                }

                return nConnectedNodes;
            };

            //Bi-directional graph complete, now compilation impact by multiplying the no. of inward edges (no. of times the file was included)
            //by how many other nodes it is indirectly or directly connected to.
            //(fullPath:string, compilationImpact:int)
            Dictionary<string, int> compilationImpactDict = new Dictionary<string, int>();

            for(int i = 0; i < biNodes.Count(); i++)
            {
                string fullFilename = biNodes[i].value;
                //Essentially n^2 complexity, no simple way to reduce time complexity since this is a directed graph

                //Number of connected nodes = no. of files that this file has to include, 
                //add 1 to factor in the cost of the current file as well
                int nNumberOfIncludes = 1 + countConnectedNodes(i);
                //Number of inbound edges = no. of times this file is included
                //TODO: -> instead of just direct includes, count the no. of files that indirectly include it as well?
                int nTimesIncluded = biNodes[i].edgeList.Count(edge => edge.destId == i) + 1;  
                int compilationImpact = nNumberOfIncludes * nTimesIncluded;
                compilationImpactDict[fullFilename] = compilationImpact;
            }

            return compilationImpactDict;
        }

        //Returns every node within the graph
        public static List<BaseDotNode> Generate(string[] filePaths, string[] libraryDirectories, string[] includeDirectories)
        {
            List<FileNode> fileGraph = Util.ParseAllFiles(filePaths, libraryDirectories, includeDirectories);
            //bfs graph to get all filenames, then obtain their abbreviations
            List<FileNode> nodeList = FileNode.GetAllNodes(fileGraph);

            var fullPathList = nodeList.Select(fNode => fNode.nodeValue.fullPath);
            Dictionary<string,string> fullPathToAbbrDict = Util.GetShortestDistinctFilePaths(fullPathList);

            Dictionary<string, int> includeImpactDict = GetIncludeWeight(nodeList);
            int maxWeight = includeImpactDict.Max(kv => kv.Value);

            //Then proceed to convert it to a BaseDotNode graph 
            //Return all nodes within the graph as a list.

            //Setup mapping between the references of the original graph + the converted graph
            Dictionary<FileNode, BaseDotNode> fNodeToBaseNodeDict = new Dictionary<FileNode, BaseDotNode>(); 

            List<BaseDotNode> dotNodes = nodeList.Select(fNode => {
                string fullPath = fNode.nodeValue.fullPath;
                string abbreviatedPath = fullPathToAbbrDict[fullPath];

                float includeWeightFrac = (float)includeImpactDict[fullPath] / (float)maxWeight; // (most minor impact) 0 - 1 (heaviest impact)

                Color minorImpactColour = Color.FromArgb(255, 240, 241, 255); // off-white
                Color majorImpactColour = Color.FromArgb(255, 255, 107, 107); // dark pink

                Color nodeColor = LerpColours(minorImpactColour, majorImpactColour, includeWeightFrac);

                BaseDotNode convertedNode = new ColouredDotNode(abbreviatedPath, fullPath, nodeColor); //Edges not established yet
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
