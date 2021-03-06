using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        //Condenses (links all nodes with only 1 child into a single node) the tree *in-place*
        public static TNode<TValue> CondenseTree(TNode<TValue> root, Func<TValue, TValue, TValue> fnMergeValue)
        {
            HashSet<TNode<TValue>> visitedNodes = new HashSet<TNode<TValue>>();
            Stack<TNode<TValue>> stack = new Stack<TNode<TValue>>();
            stack.Push(root);
            //BFS
            while (stack.Count() > 0)
            {
                TNode<TValue> currentNode = stack.Pop();

                if (visitedNodes.Contains(currentNode)) continue;

                //Merge with front possible
                if (currentNode.children.Count == 1)
                {
                    //Join with child node
                    TNode<TValue> directChild = currentNode.children.First();

                    //Merge with node in front & essentially remove the front node.
                    TValue mergedValue = fnMergeValue(currentNode.nodeValue, directChild.nodeValue);
                    currentNode.nodeValue = mergedValue;
                    currentNode.children = directChild.children;
                    //Then revisit node
                    stack.Push(currentNode);
                    continue;
                }

                visitedNodes.Add(currentNode);

                foreach (TNode<TValue> child in currentNode.children)
                {
                    visitedNodes.Add(child);
                }
            }
            return root;
        }

      
        public static TNode<TValue> GenerateTrie(List<TValue[]> sequenceList, TValue rootValue)
        {
            TNode<TValue> rootNode = new TNode<TValue>(rootValue);

            foreach (TValue[] sequence in sequenceList)
            {
                TNode<TValue> tail = rootNode;
                foreach (TValue element in sequence)
                {
                    TNode<TValue> next = tail.children.FirstOrDefault(childNode => childNode.nodeValue.Equals(element));
                    if (next == null)
                    {
                        next = new TNode<TValue>(element);
                        tail.children.Add(next);
                    }

                    tail = next;
                }
            }

            return rootNode;
        }

    }

}
