using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace HeaderTreeExplorer.Parser
{
    public struct Edge
    {
        public readonly int destId;
        public readonly int srcId;

        public Edge(int _destId = -1, int _srcId = -1)
        {
            destId = _destId;
            srcId = _srcId;
        }
    }

    //
    public class BiNode<TValue>
    {
        public int nodeId = -1;
        public List<Edge> edgeList = new List<Edge>();

        public TValue value = default(TValue);

        public BiNode(int _nodeId)
        {
            nodeId = _nodeId;
        }

        public BiNode(int _nodeId, TValue _value)
        {
            nodeId = _nodeId;
            value = _value;
        }
    }


    //Specialized class for nodes which only have outward edges
    public class TNode<TValue> where TValue : IEquatable<TValue>
    {
        public TValue nodeValue = default(TValue);
        public List<TNode<TValue>> children = new List<TNode<TValue>>();

        public TNode(TValue _nodeValue, List<TNode<TValue>> _children = null)
        {
            nodeValue = _nodeValue;
            if (_children != null) children = _children;
        }

        public static List<TNode<TValue>> GetAllNodes(IEnumerable<TNode<TValue>> startingNodes)
        {
            var visited = new HashSet<TNode<TValue>>();
            var frontier = new Queue<TNode<TValue>>();

            foreach(var startingNode in startingNodes)
            {
                if (visited.Contains(startingNode)) continue;

                visited.Add(startingNode);
                frontier.Enqueue(startingNode);

                while(frontier.Count() > 0)
                {
                    var nextNode = frontier.Dequeue();

                    foreach(var childNode in nextNode.children)
                    {
                        bool unvisited = visited.Add(childNode);
                        if (unvisited) frontier.Enqueue(childNode);
                    }
                }
            }

            return visited.ToList();
        }   
    }

    public class TTrieNode<TValue> where TValue : IEquatable<TValue>
    {
        public TValue nodeValue = default(TValue);
        public List<TTrieNode<TValue>> children = new List<TTrieNode<TValue>>();

        public TValue leafValue = default(TValue);
        public bool isLeaf = false;

        public TTrieNode(TValue _nodeValue, List<TTrieNode<TValue>> _children = null)
        {
            nodeValue = _nodeValue;
            if (_children != null) children = _children;
        }

        //Condenses (links all nodes with only 1 child into a single node) the tree *in-place*
        public static TTrieNode<TValue> CondenseTree(TTrieNode<TValue> root, Func<TValue, TValue, TValue> fnMergeValue)
        {
            HashSet<TTrieNode<TValue>> visitedNodes = new HashSet<TTrieNode<TValue>>();

            Stack<TTrieNode<TValue>> stack = new Stack<TTrieNode<TValue>>();
            stack.Push(root);

            //DFS
            while (stack.Count() > 0)
            {
                TTrieNode<TValue> currentNode = stack.Pop();

                if (visitedNodes.Contains(currentNode)) continue;

                //Merge with front possible
                if (currentNode.children.Count == 1 && !currentNode.isLeaf)
                {
                    //Join with child node
                    TTrieNode<TValue> directChild = currentNode.children.First();

                    //Merge with node in front & essentially remove the front node.
                    TValue mergedValue = fnMergeValue(currentNode.nodeValue, directChild.nodeValue);
                    currentNode.nodeValue = mergedValue;
                    currentNode.isLeaf = directChild.isLeaf;
                    currentNode.leafValue = directChild.leafValue;
                    currentNode.children = directChild.children;
                    //Then revisit node
                    stack.Push(currentNode);
                    continue;
                }

                visitedNodes.Add(currentNode);

                foreach (TTrieNode<TValue> child in currentNode.children)
                {
                    visitedNodes.Add(child);
                }
            }
            return root;
        }

        public static TTrieNode<TValue> GenerateTrie(List<TValue[]> sequenceList, List<TValue> leafValues, TValue rootValue)
        {
            Debug.Assert(sequenceList.Count() == leafValues.Count());

            TTrieNode<TValue> rootNode = new TTrieNode<TValue>(rootValue);

            foreach (Tuple<TValue[], TValue> sequenceLeafPair in sequenceList.Zip(leafValues, (seq, leaf) => Tuple.Create(seq, leaf)))
            {
                TValue[] sequence = sequenceLeafPair.Item1;
                TValue leafValue = sequenceLeafPair.Item2;

                if (sequence.Count() == 0) continue;

                TTrieNode<TValue> tail = rootNode;
                foreach (TValue element in sequence)
                {
                    TTrieNode<TValue> next = tail.children.FirstOrDefault(childNode => childNode.nodeValue.Equals(element));
                    if (next == null)
                    {
                        next = new TTrieNode<TValue>(element);
                        tail.children.Add(next);
                    }

                    tail = next;
                }

                tail.isLeaf = true;
                tail.leafValue = leafValue;
            }

            return rootNode;
        }

    }

}
