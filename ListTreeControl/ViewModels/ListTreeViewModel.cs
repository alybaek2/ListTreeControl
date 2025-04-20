using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ListTree
{
    public class ListTreeViewModel
    {
        public ListTreeViewModel(IListTree realTree)
        {
            _listTree = realTree;
            _root = new DiagramBranchViewModel(null, _listTree, _listTree.Root);

            _invalidatedNodes = new Queue<DiagramConnectorViewModel>();

            _branches = new ObservableCollection<DiagramBranchViewModel>();
            _readOnlyBranches = new ReadOnlyObservableCollection<DiagramBranchViewModel>(_branches);

            _propertyChangedEventHandlers = new Dictionary<IListTreeNode, PropertyChangedEventHandler>();
            _collectionChanedEventHandlers = new Dictionary<IListTreeNode, NotifyCollectionChangedEventHandler>();

            AddNode(0, _listTree.Root, null);

            FixInvalidatedNodes();
        }

        public DiagramBranchViewModel Root
        {
            get
            {
                return _root;
            }
        }

        public ReadOnlyObservableCollection<DiagramBranchViewModel> Branches
        {
            get
            {
                return _readOnlyBranches;
            }
        }

        private void FixInvalidatedNodes()
        {
            while (_invalidatedNodes.Count > 0)
            {
                DiagramConnectorViewModel node = _invalidatedNodes.Dequeue();
                DiagramConnectorViewModel parentNode = node.Parent;

                DiagramConnectorViewModel left = node.Left;
                int subColumn = 0;
                if (left != null)
                {
                    subColumn = left.SubColumn + 1;
                }

                // To make the tree look nice, make sure a chid node can never be to the left of its parent.
                if (parentNode != null && subColumn < parentNode.SubColumn)
                {
                    subColumn = parentNode.SubColumn;
                }

                if (node.SubColumn != subColumn)
                {
                    node.SubColumn = subColumn;

                    Invalidate(node.Right);
                    Invalidate(node.LeftmostChild);
                }

                int row = 0;
                if (parentNode != null)
                {
                    row = parentNode.Row + 1;
                }

                if (node.Row != row)
                {
                    node.Row = row;

                    Invalidate(node._children);
                }
            }
        }

        private void Invalidate(IEnumerable<DiagramConnectorViewModel> nodes)
        {
            foreach (DiagramConnectorViewModel node in nodes)
            {
                Invalidate(node);
            }
        }

        private void Invalidate(DiagramConnectorViewModel node)
        {
            if (node != null)
            {
                _invalidatedNodes.Enqueue(node);
            }
        }

        private void AddPropertyChangedHandler(IListTreeNode node, DiagramBranchViewModel parent, DiagramBranchViewModel child)
        {
            void PropertyChanged(object sender, PropertyChangedEventArgs args) => ListTreeNode_PropertyChanged(args, parent, child);
            _propertyChangedEventHandlers.Add(node, PropertyChanged);
            node.PropertyChanged += PropertyChanged;
        }

        private void RemovePropertyChangedHandler(IListTreeNode node)
        {
            node.PropertyChanged -= _propertyChangedEventHandlers[node];
            _propertyChangedEventHandlers.Remove(node);
        }

        private void AddChildrenChangedHandler(IListTreeNode node, DiagramBranchViewModel viewModel)
        {
            void ChildrenChanged(object sender, NotifyCollectionChangedEventArgs args) => ListTreeViewModel_CollectionChanged(args, viewModel);
            _collectionChanedEventHandlers.Add(node, ChildrenChanged);
            node.Children.CollectionChanged += ChildrenChanged;
        }

        private void RemoveChildrenChangedHandler(IListTreeNode node)
        {
            node.Children.CollectionChanged -= _collectionChanedEventHandlers[node];
            _collectionChanedEventHandlers.Remove(node);
        }

        private void AddNode(int childIndex, IListTreeNode childNode, DiagramBranchViewModel parentViewModel)
        {
            Queue<(int, IListTreeNode, DiagramBranchViewModel)> queue = new Queue<(int, IListTreeNode, DiagramBranchViewModel)>();
            queue.Enqueue((childIndex, childNode, parentViewModel));

            while (queue.Count > 0)
            {
                (childIndex, childNode, parentViewModel) = queue.Dequeue();

                DiagramBranchViewModel viewModel = new DiagramBranchViewModel(parentViewModel, _listTree, childNode);
                _branches.Add(viewModel);

                parentViewModel?._children.Insert(childIndex, viewModel);
                Invalidate(viewModel.Right);

                // Call separate methods in order to create a closure for the event handlers.
                AddPropertyChangedHandler(childNode, parentViewModel, viewModel);
                AddChildrenChangedHandler(childNode, viewModel);

                if (parentViewModel != null)
                {
                    int expectedConnectorCount = viewModel.Node.VerticalIndex - parentViewModel.Node.VerticalIndex;
                    FixConnectorsUpwards(viewModel, expectedConnectorCount);
                }

                Invalidate(viewModel);

                for (int c = 0; c < childNode.Children.Count; c++)
                {
                    queue.Enqueue((c, childNode.Children[c], viewModel));
                }
            }
        }

        private void RemoveNode(int index, DiagramBranchViewModel parentBranchViewModel)
        {
            Invalidate(parentBranchViewModel._children[index].Right);

            DiagramConnectorViewModel childViewModel = parentBranchViewModel._children[index];

            Stack<(int, DiagramConnectorViewModel)> stack = new Stack<(int, DiagramConnectorViewModel)>();
            stack.Push((childViewModel._children.Count - 1, childViewModel));

            while (stack.Count > 0)
            {
                (int childIndex, DiagramConnectorViewModel viewModel) = stack.Pop();

                // Make sure we delete all the node's children before deleting the node itself.
                if (childIndex < 0)
                {
                    if (viewModel.Node != null)
                    {
                        RemoveChildrenChangedHandler(viewModel.Node);
                        RemovePropertyChangedHandler(viewModel.Node);

                        DiagramBranchViewModel branchViewModel = (DiagramBranchViewModel)viewModel;
                        _branches.Remove(branchViewModel);
                    }

                    viewModel.Parent._children.Remove(viewModel);
                    viewModel.Parent = null;
                }
                else
                {
                    stack.Push((childIndex - 1, viewModel));
                    stack.Push((viewModel._children[childIndex]._children.Count - 1, viewModel._children[childIndex]));
                }
            }
        }

        private void MoveNode(int oldIndex, int newIndex, DiagramConnectorViewModel parentViewModelNode)
        {
            DiagramConnectorViewModel viewModel = parentViewModelNode._children[oldIndex];
            parentViewModelNode._children.RemoveAt(oldIndex);
            parentViewModelNode._children.Insert(newIndex, viewModel);

            Invalidate(parentViewModelNode._children[Math.Min(newIndex, oldIndex)]);
        }

        private void ListTreeViewModel_CollectionChanged(NotifyCollectionChangedEventArgs e, DiagramBranchViewModel parentViewModel)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddNode(e.NewStartingIndex, (IListTreeNode)e.NewItems[0], parentViewModel);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveNode(e.OldStartingIndex, parentViewModel);
                    break;
                case NotifyCollectionChangedAction.Move:
                    MoveNode(e.OldStartingIndex, e.NewStartingIndex, parentViewModel);
                    break;
                default:
                    throw new Exception(String.Format("Unknown collection action '{0}'.", e.Action));
            }

            FixInvalidatedNodes();
        }

        private void ListTreeNode_PropertyChanged(PropertyChangedEventArgs e, DiagramBranchViewModel parentViewModel, DiagramBranchViewModel childViewModel)
        {
            if (e.PropertyName == nameof(IListTreeNode.VerticalIndex))
            {
                VerticalIndexChanged(parentViewModel, childViewModel);
            }
        }

        private void VerticalIndexChanged(DiagramBranchViewModel parentViewModel, DiagramBranchViewModel viewModel)
        {
            int expectedConnectorCount = viewModel.Node.VerticalIndex - parentViewModel.Node.VerticalIndex;
            FixConnectorsUpwards(viewModel, expectedConnectorCount);

            for (int c = 0; c < viewModel.Node.Children.Count; c++)
            {
                expectedConnectorCount = viewModel.Node.Children[c].VerticalIndex - viewModel.Node.VerticalIndex;
                FixConnectorsDownwards(viewModel._children[c], expectedConnectorCount);
            }

            FixInvalidatedNodes();
        }

        private void FixConnectorsDownwards(DiagramConnectorViewModel viewModel, int expectedConnectorCount)
        {
            if (expectedConnectorCount <= 0)
            {
                // This is likely due to a parent node's VerticalIndex being incremented before its child's
                // VerticalIndex is changed. Just ignore this case; eventually, this method will be called
                // once the child's VerticalIndex changes.
                return;
            }

            DiagramBranchViewModel branchViewModel;
            while ((branchViewModel = viewModel as DiagramBranchViewModel) == null)
            {
                viewModel = viewModel._children[0];
            }

            FixConnectorsUpwards(branchViewModel, expectedConnectorCount);
        }

        private void FixConnectorsUpwards(DiagramBranchViewModel branchViewModel, int expectedConnectorCount)
        {
            if (expectedConnectorCount <= 0)
            {
                // This is likely due to a parent node's VerticalIndex being incremented before its child's
                // VerticalIndex is changed. Just ignore this case; eventually, this method will be called
                // once the child's VerticalIndex changes.
                return;
            }

            int actualConnectorCount = branchViewModel.Connectors.Count;

            if (actualConnectorCount == expectedConnectorCount)
            {
                return;
            }

            if (actualConnectorCount > expectedConnectorCount)
            {
                while (branchViewModel.Connectors.Count > expectedConnectorCount)
                {
                    Invalidate(branchViewModel.Right);
                    branchViewModel.RemoveParentConnector();
                }
            }
            else if (actualConnectorCount < expectedConnectorCount)
            {
                while (branchViewModel.Connectors.Count < expectedConnectorCount)
                {
                    branchViewModel.AddParentConnector();
                    Invalidate(branchViewModel.Parent);
                    Invalidate(branchViewModel.Right);
                }
            }

            Invalidate(branchViewModel);
        }

        private readonly IListTree _listTree;

        private readonly ObservableCollection<DiagramBranchViewModel> _branches;
        private readonly ReadOnlyObservableCollection<DiagramBranchViewModel> _readOnlyBranches;

        private readonly DiagramBranchViewModel _root;

        private readonly Queue<DiagramConnectorViewModel> _invalidatedNodes;

        private readonly Dictionary<IListTreeNode, PropertyChangedEventHandler> _propertyChangedEventHandlers;
        private readonly Dictionary<IListTreeNode, NotifyCollectionChangedEventHandler> _collectionChanedEventHandlers;
    }
}
