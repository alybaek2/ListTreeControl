using ListTree;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace ListTreeTest
{
    public class BasicListTreeTests
    {
        [Test]
        public void InsertNodes()
        {
            //Setup
            BasicListTree listTree = new BasicListTree("0");

            // Act
            IListTreeNode child0 = listTree.Root.Insert(0, "1", 1);
            IListTreeNode child1 = child0.Insert(0, "2", 2);
            IListTreeNode child2 = child0.Insert(0, "3", 2);

            // Verify
            Assert.AreEqual(new List<IListTreeNode> { child0 }, listTree.Root.Children);
            Assert.AreEqual(new List<IListTreeNode> { child2, child1 }, child0.Children);
            Assert.AreEqual(1, child0.VerticalIndex);
            Assert.AreEqual(2, child2.VerticalIndex);
            Assert.AreEqual(3, child1.VerticalIndex);
        }

        [Test]
        public void DeleteNode()
        {
            //Setup
            BasicListTree listTree = new BasicListTree("0");
            IListTreeNode child0 = listTree.Root.Insert(0, "1", 1);
            IListTreeNode child1 = child0.Insert(0, "2", 2);
            IListTreeNode child2 = child0.Insert(0, "3", 3);

            // Act
            child2.Delete();

            // Verify
            Assert.AreEqual(new List<IListTreeNode> { child0 }, listTree.Root.Children);
            Assert.AreEqual(new List<IListTreeNode> { child1 }, child0.Children);
            Assert.AreEqual(1, child0.VerticalIndex);
            Assert.AreEqual(2, child1.VerticalIndex);
        }

        [Test]
        public void UpdateChildIndex()
        {
            //Setup
            BasicListTree listTree = new BasicListTree("0");
            IListTreeNode child0 = listTree.Root.Insert(0, "1", 1);
            IListTreeNode child1 = child0.Insert(0, "2", 2);
            IListTreeNode child2 = child0.Insert(0, "3", 3);
            IListTreeNode child3 = child0.Insert(0, "4", 2);

            // Act
            child2.ChildIndex = 0;

            // Verify
            Assert.AreEqual(new List<IListTreeNode> { child0 }, listTree.Root.Children);
            Assert.AreEqual(new List<IListTreeNode> { child2, child3, child1 }, child0.Children);
            Assert.AreEqual(1, child0.VerticalIndex);
            Assert.AreEqual(2, child3.VerticalIndex);
            Assert.AreEqual(3, child1.VerticalIndex);
            Assert.AreEqual(4, child2.VerticalIndex);
        }

        [Test]
        public void UpdateVerticalIndex()
        {
            //Setup
            BasicListTree listTree = new BasicListTree("0");
            IListTreeNode child0 = listTree.Root.Insert(0, "1", 1);
            IListTreeNode child1 = child0.Insert(0, "2", 2);
            IListTreeNode child2 = child0.Insert(0, "3", 3);
            IListTreeNode child3 = child0.Insert(0, "4", 2);

            // Act
            child2.VerticalIndex = 2;

            // Verify
            Assert.AreEqual(new List<IListTreeNode> { child0 }, listTree.Root.Children);
            Assert.AreEqual(new List<IListTreeNode> { child3, child2, child1 }, child0.Children);
            Assert.AreEqual(1, child0.VerticalIndex);
            Assert.AreEqual(2, child2.VerticalIndex);
            Assert.AreEqual(3, child3.VerticalIndex);
            Assert.AreEqual(4, child1.VerticalIndex);
        }

        [Test]
        public void UpdateParent()
        {
            //Setup
            BasicListTree listTree = new BasicListTree("0");
            IListTreeNode child0 = listTree.Root.Insert(0, "1", 1);
            IListTreeNode child1 = listTree.Root.Insert(0, "2", 1);
            IListTreeNode child2 = child0.Insert(0, "3", 3);
            IListTreeNode child3 = child0.Insert(1, "4", 4);
            IListTreeNode child4 = child0.Insert(2, "5", 5);
            IListTreeNode child5 = child1.Insert(0, "6", 6);

            // Act
            child0.UpdateParent(child1, 0);

            // Verify
            Assert.AreEqual(new List<IListTreeNode> { child1 }, listTree.Root.Children);
            Assert.AreEqual(new List<IListTreeNode> { child0, child5 }, child1.Children);
            Assert.AreEqual(new List<IListTreeNode> { child2, child3, child4 }, child0.Children);
            Assert.AreEqual(2, child0.VerticalIndex);
            Assert.AreEqual(1, child1.VerticalIndex);
            Assert.AreEqual(3, child2.VerticalIndex);
            Assert.AreEqual(4, child3.VerticalIndex);
            Assert.AreEqual(5, child4.VerticalIndex);
            Assert.AreEqual(6, child5.VerticalIndex);
        }

        [Test]
        public void UpdateParentDescendent()
        {
            //Setup
            BasicListTree listTree = new BasicListTree("0");
            IListTreeNode child0 = listTree.Root.Insert(0, "1", 1);
            IListTreeNode child1 = child0.Insert(0, "2", 2);

            // Act and Verify
            Assert.Throws<Exception>(() => child0.UpdateParent(child1, 0));
        }

        [Test]
        public void UpdateParentBelow()
        {
            //Setup
            BasicListTree listTree = new BasicListTree("0");
            IListTreeNode child0 = listTree.Root.Insert(0, "1", 1);
            IListTreeNode child1 = listTree.Root.Insert(0, "2", 2);

            // Act and Verify
            Assert.Throws<Exception>(() => child0.UpdateParent(child1, 0));
        }
    }
}
