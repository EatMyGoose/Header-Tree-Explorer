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

    //TODO: Generalize directed graph class to minimize back & front conversions.
    abstract class BaseDotNode
    {
        public BaseDotNode[] outboundEdges = new BaseDotNode[]{};
        public abstract string GenerateNodeAttr();
    }

    //TODO: ->  Add "Top Level" Node to represent the selected source files, which are rendered at the top of the dot graph for readibility.
    class BasicDotNode : BaseDotNode
    {
        string abbreviatedName = "";
        string fullPathName = "";

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
    
    static class GraphVisualizationReport
    {
        public static List<BaseDotNode> Generate(string[] filePaths, string[] libraryDirectories, string[] includeDirectories)
        {
            List<FileNode> fileGraph = Util.ParseAllFiles(filePaths, libraryDirectories, includeDirectories);
            //bfs graph to get all filenames, then obtain their abbreviations
            List<FileNode> nodeList = FileNode.GetAllNodes(fileGraph);

            var fullPathList = nodeList.Select(fNode => fNode.nodeValue.fullPath);
            Dictionary<string,string> fullPathToAbbrDict = Util.GetShortestDistinctFilePaths(fullPathList);

            //Then proceed to convert it to a BaseDotNode graph 
            //Return all nodes within the graph as a list.

            //Setup mapping between the references of the original graph + the converted graph
            Dictionary<FileNode, BaseDotNode> fNodeToBaseNodeDict = new Dictionary<FileNode, BaseDotNode>(); 

            List<BaseDotNode> dotNodes = nodeList.Select(fNode => {
                string fullPath = fNode.nodeValue.fullPath;
                string abbreviatedPath = fullPathToAbbrDict[fullPath];
                BaseDotNode convertedNode = new BasicDotNode(abbreviatedPath, fullPath); //Edges not established yet
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
