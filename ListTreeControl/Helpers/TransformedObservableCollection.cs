using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace ListTree
{
    public class TransformedObservableCollection<S, T>
        where S : class
        where T : class
    {
        private TransformedObservableCollection(IEnumerable sourceCollection, Func<S, T> transform, IList targetCollection)
        {
            if (!(sourceCollection is INotifyCollectionChanged notifyCollectionChanged))
            {
                throw new ArgumentException(String.Format("Source collection does not implement {0}.", nameof(INotifyCollectionChanged)), nameof(sourceCollection));
            }

            _transform = transform;
            _lookupTable = new ConditionalWeakTable<S, T>();
            _sourceCollection = sourceCollection;
            _targetCollection = targetCollection;

            // Initial sync.
            Reset();

            // Set up ongoing sync.
            notifyCollectionChanged.CollectionChanged += SourceCollectionChanged;
        }

        static public TransformedObservableCollection<S, T> Create(IEnumerable sourceCollection, Func<S, T> transform, IList targetCollection)
        {
            TransformedObservableCollection<S, T> collection = new TransformedObservableCollection<S, T>(sourceCollection, transform, targetCollection);

            return collection;
        }

        private void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Add(e.NewStartingIndex, e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Remove(e.OldStartingIndex, e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Reset();
                    break;
                case NotifyCollectionChangedAction.Move:
                    Move(e.OldStartingIndex, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Replace(e.OldStartingIndex, e.NewItems);
                    break;
                default:
                    throw new Exception(String.Format("Unexpected collection changed action: {0}.", e.Action));
            }
        }

        private T GetTargetItem(S sourceItem)
        {
            return _lookupTable.GetValue(sourceItem, x => _transform(x));
        }

        private void Add(int index, IEnumerable sourceItems)
        {
            foreach (S sourceItem in sourceItems)
            {
                T targetItem = GetTargetItem(sourceItem);
                _targetCollection.Insert(index, targetItem);
                index++;
            }
        }

        private void Remove(int index, IList sourceItems)
        {
            for (int i = 0; i < sourceItems.Count; i++)
            {
                _targetCollection.RemoveAt(index);
            }
        }

        private void Reset()
        {
            _targetCollection.Clear();
            Add(0, _sourceCollection);
        }

        private void Move(int oldIndex, int newIndex)
        {
            // Optimize this in case the target collection is an ObservableCollection.
            if (_targetCollection is ObservableCollection<T> observableCollection)
            {
                observableCollection.Move(oldIndex, newIndex);
                return;
            }

            object movedItem = _targetCollection[oldIndex];
            _targetCollection.RemoveAt(oldIndex);
            _targetCollection.Insert(newIndex, movedItem);
        }

        private void Replace(int index, IList newItems)
        {
            foreach (object sourceItem in newItems)
            {
                _targetCollection[index] = GetTargetItem((S)sourceItem);
                index++;
            }
        }

        private readonly IEnumerable _sourceCollection;
        private readonly IList _targetCollection;

        private readonly Func<S, T> _transform;
        private readonly ConditionalWeakTable<S, T> _lookupTable;
    }
}
