using ListTree;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ListTreeTest
{
    internal class ListTreeViewModelTests
    {
        [Test]
        public void InsertAndDeleteNodes()
        {
            // Setup
            BasicListTree listTree = new BasicListTree("0");
            ListTreeViewModel viewModel = new ListTreeViewModel(listTree);

            // Act and Verify
            IListTreeNode child0 = listTree.Root.Insert(0, "1", 1);
            CheckViewModel(viewModel, listTree);

            IListTreeNode child1 = listTree.Root.Insert(1, "2", 2);
            CheckViewModel(viewModel, listTree);

            IListTreeNode child2 = listTree.Root.Insert(2, "3", 1);
            CheckViewModel(viewModel, listTree);

            IListTreeNode child3 = child1.Insert(0, "4", 4);
            CheckViewModel(viewModel, listTree);

            IListTreeNode child4 = child1.Insert(0, "5", 4);
            CheckViewModel(viewModel, listTree);

            IListTreeNode child5 = child1.Insert(1, "6", 5);
            CheckViewModel(viewModel, listTree);

            child2.ChildIndex = 0;
            CheckViewModel(viewModel, listTree);

            child1.ChildIndex = 1;
            CheckViewModel(viewModel, listTree);

            child0.VerticalIndex = 1;
            CheckViewModel(viewModel, listTree);

            child1.UpdateParent(child2, 0);
            CheckViewModel(viewModel, listTree);

            child5.Delete();
            CheckViewModel(viewModel, listTree);

            child1.Delete();
            CheckViewModel(viewModel, listTree);

            IListTreeNode child6 = listTree.Root.Insert(1, "7", 3);
            CheckViewModel(viewModel, listTree);

            child6.UpdateParent(child0, 0);
            CheckViewModel(viewModel, listTree);
        }

        [Test]
        public void VerticalUp()
        {
            // Setup
            BasicListTree listTree = new BasicListTree("0");
            IListTreeNode child0 = listTree.Root.Insert(0, "1", 1);
            IListTreeNode child1 = listTree.Root.Insert(0, "2", 2);
            ListTreeViewModel viewModel = new ListTreeViewModel(listTree);

            // Act
            DiagramBranchViewModel branchViewModel = viewModel.Branches.First(x => x.Node == child1);
            branchViewModel.DecrementVerticalIndex.Execute(null);

            // Verify
            Assert.False(branchViewModel.DecrementVerticalIndex.CanExecute(null));
            Assert.True(branchViewModel.IncrementVerticalIndex.CanExecute(null));
            Assert.AreEqual(1, child1.VerticalIndex);
            Assert.AreEqual(1, branchViewModel.Row);
        }

        [Test]
        public void VerticalDown()
        {
            // Setup
            BasicListTree listTree = new BasicListTree("0");
            IListTreeNode child0 = listTree.Root.Insert(0, "1", 1);
            IListTreeNode child1 = listTree.Root.Insert(0, "2", 2);
            ListTreeViewModel viewModel = new ListTreeViewModel(listTree);

            // Act
            DiagramBranchViewModel branchViewModel = viewModel.Branches.First(x => x.Node == child0);
            branchViewModel.IncrementVerticalIndex.Execute(null);

            // Verify
            Assert.True(branchViewModel.DecrementVerticalIndex.CanExecute(null));
            Assert.False(branchViewModel.IncrementVerticalIndex.CanExecute(null));
            Assert.AreEqual(2, child0.VerticalIndex);
            Assert.AreEqual(2, branchViewModel.Row);
        }

        [Test]
        public void Delete()
        {
            // Setup
            BasicListTree listTree = new BasicListTree("0");
            IListTreeNode child0 = listTree.Root.Insert(0, "1", 1);
            IListTreeNode child1 = listTree.Root.Insert(0, "2", 2);
            IListTreeNode child2 = child1.Insert(0, "3", 3);
            IListTreeNode child3 = child1.Insert(0, "4", 4);
            ListTreeViewModel viewModel = new ListTreeViewModel(listTree);

            // Act
            DiagramBranchViewModel branchViewModel = viewModel.Branches.First(x => x.Node == child1);
            branchViewModel.Delete.Execute(null);

            // Verify
            Assert.AreEqual(0, child0.Children.Count);
            Assert.False(viewModel.Branches.Contains(branchViewModel));
            Assert.False(viewModel.Branches.Any(x => x.Parent == branchViewModel));
            CheckViewModel(viewModel, listTree);
        }

        [Test]
        public void HorizontalLeft()
        {
            // Setup
            BasicListTree listTree = new BasicListTree("0");
            IListTreeNode child0 = listTree.Root.Insert(0, "1", 1);
            IListTreeNode child1 = listTree.Root.Insert(1, "2", 2);
            ListTreeViewModel viewModel = new ListTreeViewModel(listTree);

            // Act
            DiagramBranchViewModel branchViewModel = viewModel.Branches.First(x => x.Node == child1);
            branchViewModel.DecrementChildIndex.Execute(null);

            // Verify
            Assert.AreEqual(0, child1.ChildIndex);
            Assert.AreEqual(0, branchViewModel.ChildIndex);
            Assert.False(branchViewModel.DecrementChildIndex.CanExecute(null));
            Assert.True(branchViewModel.IncrementChildIndex.CanExecute(null));
            Assert.AreEqual(1, child0.ChildIndex);
        }

        [Test]
        public void HorizontalRight()
        {
            // Setup
            BasicListTree listTree = new BasicListTree("0");
            IListTreeNode child0 = listTree.Root.Insert(0, "1", 1);
            IListTreeNode child1 = listTree.Root.Insert(1, "2", 2);
            ListTreeViewModel viewModel = new ListTreeViewModel(listTree);

            // Act
            DiagramBranchViewModel branchViewModel = viewModel.Branches.First(x => x.Node == child0);
            branchViewModel.IncrementChildIndex.Execute(null);

            // Verify
            Assert.AreEqual(1, child0.ChildIndex);
            Assert.AreEqual(1, branchViewModel.ChildIndex);
            Assert.False(branchViewModel.IncrementChildIndex.CanExecute(null));
            Assert.True(branchViewModel.DecrementChildIndex.CanExecute(null));
            Assert.AreEqual(0, child1.ChildIndex);
        }

        private void CheckViewModel(ListTreeViewModel viewModel, IListTree listTree)
        {
            List<int> lastXValues = new List<int>(listTree.NodesByVerticalIndex.Count);
            for (int j = 0; j < listTree.NodesByVerticalIndex.Count; j++)
            {
                lastXValues.Add(-1);
            }

            Stack<IListTreeNode> stack = new Stack<IListTreeNode>();
            stack.Push(listTree.Root);

            while (stack.Count > 0)
            {
                IListTreeNode node = stack.Pop();

                DiagramBranchViewModel branchViewModel = viewModel.Branches.First(x => x.Node.VerticalIndex == node.VerticalIndex);
                Assert.IsNotNull(branchViewModel);

                int parentVerticalIndex = (node.Parent != null) ? node.Parent.VerticalIndex : 0;
                int expectedConnectorCount = node.VerticalIndex - parentVerticalIndex;
                int actualConnectorCount = branchViewModel.Connectors.Count;

                Assert.AreEqual(expectedConnectorCount, actualConnectorCount);

                int expectedY = parentVerticalIndex + 1;

                for (int i = 0; i < expectedConnectorCount; i++)
                {
                    DiagramConnectorViewModel connectorViewModel = branchViewModel.Connectors.First(c => c.Row == expectedY);
                    Assert.IsNotNull(connectorViewModel);

                    int expectedX = Math.Max(lastXValues[expectedY] + 1, lastXValues[expectedY - 1]);
                    lastXValues[expectedY] = expectedX;

                    Assert.AreEqual(expectedX, connectorViewModel.SubColumn);
                    Assert.AreEqual(expectedY, connectorViewModel.Row);

                    expectedY++;
                }

                // Do a depth-first traversal.
                for (int c = node.Children.Count - 1; c >= 0; c--)
                {
                    stack.Push(node.Children[c]);
                }
            }
        }
    }
}
