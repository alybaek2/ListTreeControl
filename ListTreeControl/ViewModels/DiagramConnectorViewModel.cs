using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ListTree
{
    public class DiagramConnectorViewModel : INotifyPropertyChanged
    {
        public DiagramConnectorViewModel(DiagramConnectorViewModel parent, IListTreeNode node)
        {
            _children = new List<DiagramConnectorViewModel>();
            _parent = parent;
            _node = node;
        }

        public IListTreeNode Node
        {
            get
            {
                return _node;
            }
        }

        public DiagramConnectorViewModel Parent
        {
            get
            {
                return _parent;
            }

            set
            {
                if (_parent == value)
                {
                    return;
                }

                _parent = value;
                OnPropertyChanged();
            }
        }

        public DiagramConnectorViewModel ReplaceChild(int index, DiagramConnectorViewModel newChild)
        {
            DiagramConnectorViewModel oldChild = _children[index];
            if (ReferenceEquals(oldChild, newChild))
            {
                return null;
            }

            _children[index] = newChild;
            newChild.Parent = this;

            oldChild.Parent = null;

            return oldChild;
        }

        private int GetChildIndex(DiagramConnectorViewModel child)
        {
            int index = _children.IndexOf(child);
            if (index < 0)
            {
                throw new ArgumentException("Specified view model is not a child of this view model.", nameof(child));
            }

            return index;
        }

        public int ChildIndex
        {
            get
            {
                return _parent.GetChildIndex(this);
            }
        }

        public int SubColumn
        {
            get
            {
                return _subColumn;
            }

            internal set
            {
                if (_subColumn == value)
                {
                    return;
                }

                _subColumn = value;
                OnPropertyChanged();
            }
        }

        public int Row
        {
            get
            {
                return _row;
            }

            internal set
            {
                if (_row == value)
                {
                    return;
                }

                _row = value;
                OnPropertyChanged();
            }
        }

        internal DiagramConnectorViewModel Left
        {
            get
            {
                return GetLateralNode(true);
            }
        }

        internal DiagramConnectorViewModel Right
        {
            get
            {
                return GetLateralNode(false);
            }
        }

        internal DiagramConnectorViewModel LeftmostChild
        {
            get
            {
                if (_children.Count > 0)
                {
                    return _children[0];
                }

                return null;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private class ListTreeTraverser
        {
            public ListTreeTraverser(int level, DiagramConnectorViewModel node, int childIndex, bool parentAlreadyVisited)
            {
                Level = level;
                Node = node;
                ChildIndex = childIndex;
                ParentAlreadyVisited = parentAlreadyVisited;
            }

            public int Level;
            public DiagramConnectorViewModel Node;
            public int ChildIndex;
            public bool ParentAlreadyVisited;
        }

        private DiagramConnectorViewModel GetLateralNode(bool left)
        {
            bool IsIndexValid(List<DiagramConnectorViewModel> list, int index)
            {
                return (index >= 0) && (index < list.Count);
            }

            if (Parent == null)
            {
                return null;
            }

            int direction = (left ? -1 : 1);

            // Don't bother creating a stack if we can just grab a sibling node.
            int nextChildIndex = ChildIndex + direction;
            if (IsIndexValid(Parent._children, nextChildIndex))
            {
                return Parent._children[nextChildIndex];
            }

            const int level = 0;

            Stack<ListTreeTraverser> stack = new Stack<ListTreeTraverser>();
            stack.Push(new ListTreeTraverser(level, Parent, nextChildIndex, false));

            while (stack.Count > 0)
            {
                ListTreeTraverser ltt = stack.Peek();

                // Do we have any children left to check?
                if (IsIndexValid(ltt.Node._children, ltt.ChildIndex))
                {
                    DiagramConnectorViewModel child = ltt.Node._children[ltt.ChildIndex];
                    if (ltt.Level == level)
                    {
                        // We've found it!
                        return child;
                    }

                    // Check this node's children if it has any.
                    if (child._children.Count > 0)
                    {
                        int first = left ? child._children.Count - 1 : 0;

                        stack.Push(new ListTreeTraverser(ltt.Level + 1, child, first, true));
                    }

                    // Move to the next child.
                    ltt.ChildIndex += direction;
                }
                else if (!ltt.ParentAlreadyVisited && ltt.Node.Parent != null)
                {
                    ltt.Level--;
                    ltt.ChildIndex = ltt.Node.ChildIndex + direction;
                    ltt.Node = ltt.Node.Parent;
                }
                else
                {
                    stack.Pop();
                }
            }

            return null;
        }

        internal readonly List<DiagramConnectorViewModel> _children;
        private DiagramConnectorViewModel _parent;
        private readonly IListTreeNode _node;
        private int _subColumn;
        private int _row;
    }
}
