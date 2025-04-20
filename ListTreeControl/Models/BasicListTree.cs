using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ListTree
{
    public partial class BasicListTree : IListTree
    {
        public BasicListTree(object rootData)
        {
            _verticalOrdering = new ObservableCollection<BasicListTreeNode>();
            _root = new BasicListTreeNode(this, null, rootData, 0);
            _verticalOrdering.Add(_root);

            _verticalNodes = new ReadOnlyListTreeNodeCollection(_verticalOrdering);
        }

        public IReadOnlyObservableList<IListTreeNode> NodesByVerticalIndex
        {
            get
            {
                return _verticalNodes;
            }
        }

        private class ReadOnlyListTreeNodeCollection : ReadOnlyObservableCollection<BasicListTreeNode>, IReadOnlyObservableList<BasicListTreeNode>
        {
            public ReadOnlyListTreeNodeCollection(ObservableCollection<BasicListTreeNode> list) : base(list) { }
        }

        private void UpdateVerticalIndex(BasicListTreeNode node, int newVerticalIndex)
        {
            int oldVerticalIndex = node.VerticalIndex;
            if (oldVerticalIndex == newVerticalIndex)
            {
                return;
            }

            if (newVerticalIndex <= node.Parent.VerticalIndex)
            {
                throw new ArgumentException(String.Format("Vertical index {0} must be greater than that of the node's parent ({1}).", newVerticalIndex, node.Parent.VerticalIndex), nameof(newVerticalIndex));
            }

            if (newVerticalIndex >= _verticalOrdering.Count)
            {
                throw new ArgumentException(String.Format("Vertical index {0} must be less than the total number of nodes ({1}).", newVerticalIndex, _verticalOrdering.Count), nameof(newVerticalIndex));
            }

            foreach (IListTreeNode child in node._children)
            {
                if (newVerticalIndex >= child.VerticalIndex)
                {
                    throw new ArgumentException(String.Format("Vertical index {0} must be less than those of all its children.", newVerticalIndex, child.VerticalIndex), nameof(newVerticalIndex));
                }
            }

            _verticalOrdering.Move(oldVerticalIndex, newVerticalIndex);

            void SetVerticalIndices(int lower, int upper)
            {
                for (int i = lower; i <= upper; i++)
                {
                    SetVerticalIndex(i);
                }
            }

            if (oldVerticalIndex < newVerticalIndex)
            {
                SetVerticalIndices(oldVerticalIndex, newVerticalIndex);
            }
            else
            {
                SetVerticalIndices(newVerticalIndex, oldVerticalIndex);
            }
        }

        private void SetVerticalIndex(int verticalIndex)
        {
            _verticalOrdering[verticalIndex].SetVerticalIndex(verticalIndex);
        }

        private bool UpdateChildIndex(BasicListTreeNode node, int newChildIndex)
        {
            BasicListTreeNode parent = node._parent;
            int oldChildIndex = parent._children.IndexOf(node);

            if (oldChildIndex < 0)
            {
                throw new Exception("Node cannot be found in its parent's children.");
            }

            if (oldChildIndex == newChildIndex)
            {
                return false;
            }

            parent._children.Move(oldChildIndex, newChildIndex);

            return true;
        }

        private BasicListTreeNode Insert(BasicListTreeNode parent, int childIndex, object data, int verticalIndex)
        {
            // If verticalIndex is too big, make it equal to the next available verticalIndex...
            if (verticalIndex > _verticalOrdering.Count)
            {
                throw new Exception("Child cannot have a vertical index bigger then the number of nodes in the tree.");
            }

            if (verticalIndex <= parent.VerticalIndex)
            {
                throw new Exception("Child cannot have vertical index equal to or less than its parent!");
            }

            BasicListTreeNode node = new BasicListTreeNode(this, parent, data, verticalIndex);

            _verticalOrdering.Insert(verticalIndex, node);
            parent._children.Insert(childIndex, node);

            for (int i = verticalIndex + 1; i < _verticalOrdering.Count; i++)
            {
                SetVerticalIndex(i);
            }

            return node;
        }

        private void UpdateParent(BasicListTreeNode node, BasicListTreeNode newParentNode, int newChildIndex)
        {
            if (newParentNode.IsDescendant(node))
            {
                throw new Exception("Can't move a node to one of its descendents.");
            }

            if (newParentNode.VerticalIndex > node.VerticalIndex)
            {
                throw new Exception("Can't move a node to a parent node that has a bigger vertical index.");
            }

            node._parent._children.Remove(node);
            newParentNode._children.Insert(newChildIndex, node);
            node._parent = newParentNode;
        }

        private void DeleteRecursive(BasicListTreeNode node)
        {
            IEnumerable<int> indicesToDelete = node.GetNodeAndDescendents().OrderByDescending(x => x.VerticalIndex).Select(x => x.VerticalIndex);

            foreach (int i in indicesToDelete)
            {
                _verticalOrdering.RemoveAt(i);
            }

            for (int i = node.VerticalIndex; i < _verticalOrdering.Count; i++)
            {
                SetVerticalIndex(i);
            }

            node._parent._children.Remove(node);
            node._parent = null;
        }

        public IListTreeNode Root
        {
            get
            {
                return _root;
            }
        }

        private readonly BasicListTreeNode _root;
        private readonly ObservableCollection<BasicListTreeNode> _verticalOrdering;
        private readonly ReadOnlyListTreeNodeCollection _verticalNodes;
    }
}
