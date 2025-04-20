using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ListTree
{
    // A set of attached properties that allow the RowDefinitions, ColumnDefinitions, and Children
    // properties of the Grid control to be bound to an ObservableCollection (or anything implementing
    // INotifyCollectionChanged).
    public class GridExtensions
    {
        static GridExtensions()
        {
            _collectionChangedHandlers = new ConditionalWeakTable<IList, NotifyCollectionChangedEventHandler>();
        }

        public static readonly DependencyProperty RowsSourceProperty =
            DependencyProperty.RegisterAttached(
                "RowsSource",
                typeof(IEnumerable),
                typeof(GridExtensions),
                new PropertyMetadata(RowsSourceChanged));

        public static IEnumerable GetRowsSource(Grid grid)
        {
            return (IEnumerable)grid.GetValue(RowsSourceProperty);
        }

        public static void SetRowsSource(Grid grid, IEnumerable value)
        {
            grid.SetValue(RowsSourceProperty, value);
        }

        public static void RowsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Grid grid = (Grid)d;

            SourceChanged(e, grid.RowDefinitions);
        }

        public static readonly DependencyProperty ColumnsSourceProperty =
            DependencyProperty.RegisterAttached(
                "ColumnsSource",
                typeof(IEnumerable),
                typeof(GridExtensions),
                new PropertyMetadata(ColumnsSourceChanged));

        public static IEnumerable GetColumnsSource(Grid grid)
        {
            return (IEnumerable)grid.GetValue(ColumnsSourceProperty);
        }

        public static void SetColumnsSource(Grid grid, IEnumerable value)
        {
            grid.SetValue(ColumnsSourceProperty, value);
        }

        public static void ColumnsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Grid grid = (Grid)d;

            SourceChanged(e, grid.ColumnDefinitions);
        }

        public static readonly DependencyProperty ChildrenSourceProperty =
            DependencyProperty.RegisterAttached(
                "ChildrenSource",
                typeof(IEnumerable),
                typeof(GridExtensions),
                new PropertyMetadata(ChildrenSourceChanged));

        public static IEnumerable GetChildrenSource(Grid grid)
        {
            return (IEnumerable)grid.GetValue(ChildrenSourceProperty);
        }

        public static void SetChildrenSource(Grid grid, IEnumerable value)
        {
            grid.SetValue(ChildrenSourceProperty, value);
        }

        public static void ChildrenSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Grid grid = (Grid)d;

            SourceChanged(e, grid.Children);
        }

        public static void SourceChanged(DependencyPropertyChangedEventArgs e, IList collection)
        {
            IEnumerable newValue = (IEnumerable)e.NewValue;
            IEnumerable oldValue = (IEnumerable)e.OldValue;

            if (oldValue != null)
            {
                if (_collectionChangedHandlers.TryGetValue(collection, out NotifyCollectionChangedEventHandler handler))
                {
                    if (oldValue is INotifyCollectionChanged ch)
                    {
                        ch.CollectionChanged -= handler;
                        _collectionChangedHandlers.Remove(collection);
                    }
                }
            }

            collection.Clear();

            if (newValue != null)
            {
                foreach (object item in newValue)
                {
                    collection.Add(item);
                }

                if (newValue is INotifyCollectionChanged ch)
                {
                    void CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
                    {
                        GridExtensions_SourceCollectionChanged(collection, sender, args);
                    }

                    _collectionChangedHandlers.Add(collection, CollectionChanged);
                    ch.CollectionChanged += CollectionChanged;
                }
            }
        }

        private static void GridExtensions_SourceCollectionChanged(IList collection, object sender, NotifyCollectionChangedEventArgs e)
        {
            IEnumerable sourceCollection = (IEnumerable)sender;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            collection.Insert(e.NewStartingIndex + i, e.NewItems[i]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            collection.RemoveAt(e.OldStartingIndex);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        collection.Clear();

                        foreach (object col in sourceCollection)
                        {
                            collection.Add(col);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            collection[e.NewStartingIndex + i] = e.NewItems[i];
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {
                        ArrayList movedItems = new ArrayList();
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            movedItems.Add(e.NewItems[i]);
                            collection.RemoveAt(e.OldStartingIndex);
                        }

                        for (int i = 0; i < movedItems.Count; i++)
                        {
                            collection.Insert(e.NewStartingIndex + i, movedItems[i]);
                        }
                    }
                    break;
                default:
                    throw new Exception(String.Format("Unknown CollectionChanged event type: {0}", e.Action));
            }
        }

        private static readonly ConditionalWeakTable<IList, NotifyCollectionChangedEventHandler> _collectionChangedHandlers;
    }
}
