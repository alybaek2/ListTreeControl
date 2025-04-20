using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ListTree
{
    /// <summary>
    /// Interaction logic for ListTreeControl.xaml
    /// </summary>
    public partial class ListTreeControl : UserControl, INotifyPropertyChanged
    {
        public ListTreeControl()
        {
            InitializeComponent();

            _columns = new ObservableCollection<Column>();
            _columns.CollectionChanged += Columns_CollectionChanged;

            _ascending = false;

            _rows = new List<Row>();

            _rectangles = new ObservableCollection<Rectangle>();

            // Header grid stuff
            _headerColumnDefinitions = new CompositeCollection();
            HeaderColumnDefinitions = CollectionViewSource.GetDefaultView(_headerColumnDefinitions);

            _headerUIElements = new CompositeCollection();
            HeaderUIElements = CollectionViewSource.GetDefaultView(_headerUIElements);

            // Main grid stuff
            _columnDefinitions = new ObservableCollection<ColumnDefinition>();
            ColumnDefinitions = new ReadOnlyObservableCollection<ColumnDefinition>(_columnDefinitions);

            _rowDefinitions = new ObservableCollection<RowDefinition>();
            RowDefinitions = new ReadOnlyObservableCollection<RowDefinition>(_rowDefinitions);

            _uiElements = new CompositeCollection();
            UIElements = CollectionViewSource.GetDefaultView(_uiElements);

            // Cache for remembering header columns, since the main grid columns need them when they're created.
            _headerColumnDefinitionCache = new ConditionalWeakTable<Column, ColumnDefinition>();

            DataContext = this;
        }

        private void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int c = 0; c < _columns.Count; c++)
            {
                _columns[c].ListTreeControl = this;
                _columns[c].Index = c;
            }
        }

        internal int GetColumnIndex(Column column)
        {
            return _columns.IndexOf(column);
        }

        public void SetListTree(IListTree tree)
        {
            _listTree = tree;

            _uiElements.Clear();

            CompositeCollection columnUIElements = new CompositeCollection();
            TransformColumns(x => x.GetUIElements(), columnUIElements);
            _uiElements.Add(CreateContainer(columnUIElements));
            _uiElements.Add(CreateContainer(_rectangles));

            TransformColumns(CreateHeaderColumns, _headerColumnDefinitions);
            TransformColumns(CreateHeaderUIElements, _headerUIElements);

            TransformedObservableCollection<Column, ColumnDefinition>.Create(
                _columns,
                GetColumnDefinition,
                _columnDefinitions
                );

            _listTree.NodesByVerticalIndex.CollectionChanged += VerticalNodes_CollectionChanged;

            UpdateRowDefinitions();
            UpdateRectangles();
        }

        public ICollectionView UIElements
        {
            get;
        }

        public ICollectionView HeaderUIElements
        {
            get;
        }

        public ICollectionView HeaderColumnDefinitions
        {
            get;
        }

        public ReadOnlyObservableCollection<ColumnDefinition> ColumnDefinitions
        {
            get;
        }

        public IListTree ListTree
        {
            get
            {
                return _listTree;
            }
        }

        public bool Ascending
        {
            get
            {
                return _ascending;
            }

            set
            {
                if (_ascending == value)
                {
                    return;
                }

                _ascending = value;
                OnPropertyChanged();

                UpdateRows();
            }
        }

        public Row GetRow(int index)
        {
            while (_rows.Count <= index)
            {
                _rows.Add(new Row());
            }

            return _rows[index];
        }

        public ObservableCollection<Column> Columns
        {
            get
            {
                return _columns;
            }
        }

        public ReadOnlyObservableCollection<RowDefinition> RowDefinitions { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private ColumnDefinition GetHeaderColumnDefinition(Column column)
        {
            if (!_headerColumnDefinitionCache.TryGetValue(column, out ColumnDefinition headerGridColumn))
            {
                headerGridColumn = new ColumnDefinition
                {
                    Width = new GridLength(100)
                };

                _headerColumnDefinitionCache.Add(column, headerGridColumn);
            }

            return headerGridColumn;
        }

        private ColumnDefinition GetHeaderSplitterColumnDefinition()
        {
            ColumnDefinition headerSplitterGridColumn = new ColumnDefinition
            {
                Width = new GridLength(0)
            };

            return headerSplitterGridColumn;
        }

        private CollectionContainer CreateHeaderColumns(Column column)
        {
            ObservableCollection<ColumnDefinition> columnDefinitions = new ObservableCollection<ColumnDefinition>
            {
                GetHeaderColumnDefinition(column),
                GetHeaderSplitterColumnDefinition()
            };

            return CreateContainer(columnDefinitions);
        }

        private ColumnDefinition GetColumnDefinition(Column column)
        {
            ColumnDefinition headerGridColumn = GetHeaderColumnDefinition(column);


            Binding widthBinding = new Binding(ColumnDefinition.WidthProperty.Name)
            {
                Mode = BindingMode.OneWay,
                Source = headerGridColumn
            };

            ColumnDefinition gridColumn = new ColumnDefinition();

            gridColumn.SetBinding(ColumnDefinition.WidthProperty, widthBinding);

            return gridColumn;
        }

        private Button CreateHeaderButton(Column column)
        {
            TextBlock headerText = new TextBlock
            {
                Text = column.Name,
                HorizontalAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(2, 2, 2, 2)
            };

            Button headerButton = new Button
            {
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Content = headerText
            };

            void SetGridColumn()
            {
                headerButton.SetValue(Grid.ColumnProperty, column.Index * 2);
            }

            void ColumnPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(Column.Index))
                {
                    SetGridColumn();
                }
            }

            column.PropertyChanged += ColumnPropertyChanged;

            SetGridColumn();
            headerButton.Click += HeaderButton_Click;

            return headerButton;
        }

        private GridSplitter CreateHeaderSplitter(Column column)
        {
            GridSplitter splitter = new GridSplitter
            {
                Margin = new Thickness(-3, 0, 0, 0),
                BorderThickness = new Thickness(3, 0, 0, 0),
                BorderBrush = Brushes.Transparent,
                ResizeBehavior = GridResizeBehavior.PreviousAndCurrent,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Stretch,
                ResizeDirection = GridResizeDirection.Columns
            };

            void SetGridColumn()
            {
                splitter.SetValue(Grid.ColumnProperty, column.Index * 2 + 1);
            }

            void ColumnPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(Column.Index))
                {
                    SetGridColumn();
                }
            }

            column.PropertyChanged += ColumnPropertyChanged;

            SetGridColumn();
            splitter.SetValue(Panel.ZIndexProperty, 102);

            return splitter;
        }

        private CollectionContainer CreateHeaderUIElements(Column columnDef)
        {
            List<UIElement> uiElements = new List<UIElement>
            {
                CreateHeaderSplitter(columnDef),
                CreateHeaderButton(columnDef)
            };

            return CreateContainer(uiElements);
        }

        private void TransformColumns(Func<Column, object> transform, CompositeCollection compositeCollection)
        {
            TransformedObservableCollection<Column, object>.Create(
                _columns,
                transform,
                compositeCollection
                );
        }

        private CollectionContainer CreateContainer(System.Collections.IEnumerable collection)
        {
            CollectionContainer container = new CollectionContainer
            {
                Collection = collection
            };

            return container;
        }

        private void HeaderButton_Click(object sender, RoutedEventArgs e)
        {
            Ascending = !Ascending;
        }

        private void UpdateRows()
        {
            double top = 0;

            for (int r = 0; r < _rowDefinitions.Count; r++)
            {
                double height = _rowDefinitions[r].ActualHeight;

                void SetRowInfo(int rowIndex, double h)
                {
                    Row row = GetRow(rowIndex);
                    row.Height = h;
                    row.Index = r;
                    row.Top = top;
                }

                if (_ascending)
                {
                    top += height;

                    SetRowInfo(_rowDefinitions.Count - 1 - r, -height);
                }
                else
                {
                    SetRowInfo(r, height);

                    top += height;
                }
            }
        }

        private void VerticalNodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    UpdateRowDefinitions();
                    UpdateRectangles();
                    break;
            }
        }

        private void UpdateRowDefinitions()
        {
            int existingRows = _rowDefinitions.Count;
            int currentRows = _listTree.NodesByVerticalIndex.Count;

            while (existingRows != currentRows)
            {
                if (existingRows > currentRows)
                {
                    _rowDefinitions.RemoveAt(_rowDefinitions.Count - 1);
                }
                else
                {
                    _rowDefinitions.Add(new RowDefinition() { MinHeight = 16 });
                }

                existingRows = _rowDefinitions.Count;
            }
        }

        private void UpdateRectangles()
        {
            int existingRectangles = _rectangles.Count;
            int currentRectangles = _listTree.NodesByVerticalIndex.Count;

            while (existingRectangles != currentRectangles)
            {
                if (existingRectangles > currentRectangles)
                {
                    _rectangles.RemoveAt(_rectangles.Count - 1);
                }
                else
                {
                    _rectangles.Add(CreateBackgroundRectangle(existingRectangles));
                }

                existingRectangles = _rectangles.Count;
            }
        }

        private Rectangle CreateBackgroundRectangle(int index)
        {
            Brush brush = ((index % 2) == 0) ? Brushes.WhiteSmoke : null;

            Rectangle rectangle = new Rectangle
            {
                Fill = brush
            };

            rectangle.SetValue(Grid.RowProperty, index);
            rectangle.SetValue(Grid.ColumnSpanProperty, 10);
            rectangle.SetValue(Panel.ZIndexProperty, -1);

            return rectangle;
        }

        private void Grid_LayoutUpdated(object sender, EventArgs e)
        {
            UpdateRows();
        }

        private bool _ascending;
        private IListTree _listTree;

        private readonly ObservableCollection<Column> _columns;

        private readonly List<Row> _rows;

        private readonly ObservableCollection<Rectangle> _rectangles;

        private readonly CompositeCollection _uiElements;
        private readonly CompositeCollection _headerColumnDefinitions;
        private readonly CompositeCollection _headerUIElements;

        private readonly ObservableCollection<RowDefinition> _rowDefinitions;
        private readonly ObservableCollection<ColumnDefinition> _columnDefinitions;

        private readonly ConditionalWeakTable<Column, ColumnDefinition> _headerColumnDefinitionCache;
    }
}
