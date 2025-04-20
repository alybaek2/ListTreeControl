using ListTree;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ListTreeTest
{
    [TestFixture]
    public class TransformedObservableCollectionTests
    {
        [SetUp]
        public void Setup()
        {
            // Setup
            _sourceCollection = new ObservableCollection<string>
                {
                    "0",
                    "1",
                    "2",
                    "3"
                };

            _readOnlySourceCollection = new ReadOnlyObservableCollection<string>(_sourceCollection);

            _targetCollection = _readOnlySourceCollection.ActiveTransform(TestTransform);
        }

        [Test]
        public void InitialSync()
        {
            // Verify
            CheckTargetCollection();
        }

        [Test]
        public void AddItems()
        {
            // Act
            _sourceCollection.Add("0");
            _sourceCollection.Add("1");
            _sourceCollection.Add("2");
            _sourceCollection.Add("3");

            // Verify
            CheckTargetCollection();
        }

        [Test]
        public void RemoveItems()
        {
            // Act
            _sourceCollection.RemoveAt(1);
            _sourceCollection.RemoveAt(1);

            // Verify
            CheckTargetCollection();
        }

        [Test]
        public void MoveItems()
        {
            // Act
            _sourceCollection.Move(2, 1);

            // Verify
            CheckTargetCollection();
        }

        [Test]
        public void ClearItems()
        {
            // Act
            _sourceCollection.Clear();

            // Verify
            CheckTargetCollection();
        }

        private void CheckTargetCollection()
        {
            IEnumerable<string> expected = _sourceCollection.Select(TestTransform);
            CollectionAssert.AreEqual(expected, _targetCollection);
        }

        private static string TestTransform(string x)
        {
            return String.Format("Test {0}", x);
        }

        private ObservableCollection<string> _sourceCollection;
        private ReadOnlyObservableCollection<string> _readOnlySourceCollection;
        private ReadOnlyObservableCollection<string> _targetCollection;
    }
}
