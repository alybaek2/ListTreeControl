using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ListTree
{
    public partial class BasicListTree
    {
        private class BasicListTreeNode : IListTreeNode
        {
            public BasicListTreeNode(BasicListTree listTree, BasicListTreeNode parent, object data, int verticalIndex)
            {
                _parent = parent;
                _data = data;
                _verticalIndex = verticalIndex;
                _children = new ObservableCollection<BasicListTreeNode>();
                _readOnlyChildren = new ReadOnlyListTreeNodeCollection(_children);
                _listTree = listTree;
            }

            public IListTreeNode Parent
            {
                get
                {
                    return _parent;
                }
            }

            public object Data
            {
                get
                {
                    return _data;
                }
            }

            public int ChildIndex
            {
                get
                {
                    // The root node always has child index 0.
                    if (Parent == null)
                    {
                        return 0;
                    }

                    return _parent.GetChildIndex(this);
                }

                set
                {
                    _listTree.UpdateChildIndex(this, value);
                }
            }

            public int VerticalIndex
            {
                get
                {
                    return _verticalIndex;
                }

                set
                {
                    if (value == _verticalIndex)
                    {
                        return;
                    }

                    _listTree.UpdateVerticalIndex(this, value);
                    OnPropertyChanged();
                }
            }

            public IReadOnlyObservableList<IListTreeNode> Children
            {
                get
                {
                    return _readOnlyChildren;
                }
            }

            public List<IListTreeNode> GetNodeAndDescendents()
            {
                List<IListTreeNode> allNodes = new List<IListTreeNode>
                {
                    this
                };

                for (int n = 0; n < allNodes.Count; n++)
                {
                    allNodes.AddRange(allNodes[n].Children);
                }

                return allNodes;
            }

            private int GetChildIndex(BasicListTreeNode child)
            {
                int index = _children.IndexOf(child);
                if (index < 0)
                {
                    throw new ArgumentException("Specified node is not a child of this node.", nameof(child));
                }

                return index;
            }

            public IListTreeNode Insert(int childIndex, object data, int verticalIndex)
            {
                return _listTree.Insert(this, childIndex, data, verticalIndex);
            }

            public void Delete()
            {
                _listTree.DeleteRecursive(this);
            }

            public void UpdateParent(IListTreeNode newParentNode, int newChildIndex)
            {
                _listTree.UpdateParent(this, (BasicListTreeNode)newParentNode, newChildIndex);
            }

            protected void OnPropertyChanged([CallerMemberName] string name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }

            public event PropertyChangedEventHandler PropertyChanged;

            internal bool IsDescendant(BasicListTreeNode node)
            {
                for (BasicListTreeNode n = _parent; n != null; n = n._parent)
                {
                    if (node == n)
                    {
                        return true;
                    }
                }

                return false;
            }

            internal void SetVerticalIndex(int verticalIndex)
            {
                if (_verticalIndex == verticalIndex)
                {
                    return;
                }

                _verticalIndex = verticalIndex;
                OnPropertyChanged(nameof(VerticalIndex));
            }

            internal BasicListTreeNode _parent;
            internal ObservableCollection<BasicListTreeNode> _children;
            internal ReadOnlyListTreeNodeCollection _readOnlyChildren;
            private readonly object _data;

            private int _verticalIndex;
            private readonly BasicListTree _listTree;
        }
    }
}
