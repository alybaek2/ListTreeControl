using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace ListTree
{
    public class DiagramBranchViewModel : DiagramConnectorViewModel
    {
        public DiagramBranchViewModel(DiagramConnectorViewModel parent, IListTree tree, IListTreeNode node) : base(parent, node)
        {
            _connectors = new ObservableCollection<DiagramConnectorViewModel>();
            _readOnlyConnectors = new ReadOnlyObservableCollection<DiagramConnectorViewModel>(_connectors);
            _node = node;

            if (parent != null)
            {
                _connectors.Add(this);
            }

            DecrementVerticalIndex = new Command(
                p =>
                {
                    _node.VerticalIndex--;
                },
                p =>
                {
                    if (_node.Parent == null)
                    {
                        return false;
                    }

                    return _node.Parent.VerticalIndex < (_node.VerticalIndex - 1);
                }
            );

            IncrementVerticalIndex = new Command(
                p =>
                {
                    _node.VerticalIndex++;
                },
                p =>
                {
                    int minChildIndex = -1;
                    if (node.Children.Count > 0)
                    {
                        minChildIndex = node.Children.Min(x => x.VerticalIndex);

                        return (_node.VerticalIndex + 1) < minChildIndex;
                    }

                    return (_node.VerticalIndex + 1) < tree.NodesByVerticalIndex.Count;
                }
            );

            Delete = new Command(
                p =>
                {
                    node.Delete();
                },
                p =>
                {
                    return _node.Parent != null;
                }
            );

            DecrementChildIndex = new Command(
                p =>
                {
                    node.ChildIndex--;
                },
                p =>
                {
                    if (node.Parent != null)
                    {
                        return node.ChildIndex > 0;
                    }

                    return false;
                }
            );

            IncrementChildIndex = new Command(
                p =>
                {
                    node.ChildIndex++;
                },
                p =>
                {
                    if (node.Parent != null)
                    {
                        return (node.ChildIndex + 1) < node.Parent.Children.Count;
                    }

                    return false;
                }
            );
        }

        public ReadOnlyObservableCollection<DiagramConnectorViewModel> Connectors
        {
            get
            {
                return _readOnlyConnectors;
            }
        }

        public ICommand DecrementVerticalIndex { get; private set; }
        public ICommand IncrementVerticalIndex { get; private set; }
        public ICommand Delete { get; private set; }
        public ICommand DecrementChildIndex { get; private set; }
        public ICommand IncrementChildIndex { get; private set; }

        internal bool RemoveParentConnector()
        {
            if (!(Parent is DiagramBranchViewModel))
            {
                DiagramConnectorViewModel parentConnector = Parent;
                int parentIndex = parentConnector.ChildIndex;

                DiagramConnectorViewModel newParent = parentConnector.Parent;

                newParent.ReplaceChild(parentIndex, this);
                Parent = newParent;
                parentConnector.Parent = null;
                parentConnector._children.Clear();

                _connectors.RemoveAt(1);

                return true;
            }

            return false;
        }

        internal bool AddParentConnector()
        {
            DiagramConnectorViewModel parent = Parent;
            int childIndex = ChildIndex;

            DiagramConnectorViewModel connectorNode = new DiagramConnectorViewModel(parent, null);

            parent.ReplaceChild(childIndex, connectorNode);
            Parent = connectorNode;
            connectorNode._children.Add(this);

            _connectors.Insert(1, connectorNode);

            return true;
        }

        private readonly ObservableCollection<DiagramConnectorViewModel> _connectors;
        private readonly ReadOnlyObservableCollection<DiagramConnectorViewModel> _readOnlyConnectors;

        private readonly IListTreeNode _node;
    }
}
