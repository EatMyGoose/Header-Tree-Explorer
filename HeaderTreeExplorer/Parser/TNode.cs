using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeaderTreeExplorer.Parser
{
    public class TNode<TValue> where TValue : IEquatable<TValue>
    {
        public TValue nodeValue = default(TValue);
        public List<TNode<TValue>> children = new List<TNode<TValue>>();

        public TNode(TValue _nodeValue, List<TNode<TValue>> _children = null)
        {
            nodeValue = _nodeValue;
            if (_children != null) children = _children;
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
