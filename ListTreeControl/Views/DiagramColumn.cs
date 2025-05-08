using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ListTree
{
    public class DiagramColumn : Column
    {
        public DiagramColumn()
        {
            _cells = new Dictionary<Tuple<int, int>, DiagramCell>();

            Template = (DataTemplate)_resourceDictionary["DefaultTemplate"];
        }

        static DiagramColumn()
        {
            _resourceDictionary = new ResourceDictionary();
            _resourceDictionary.Source = new Uri("/ListTreeControl;component/Resources/DiagramColumnResources.xaml", UriKind.RelativeOrAbsolute);
        }

        internal override object GetUIElements()
        {
            _viewModel = new ListTreeViewModel(ListTreeControl.ListTree);

            _contentPresenter = new ContentPresenter
            {
                ContentTemplate = Template,
                Content = this
            };

            // Set the row and column.
            _contentPresenter.SetValue(Grid.RowProperty, 0);
            _contentPresenter.SetValue(Grid.RowSpanProperty, int.MaxValue);
            _contentPresenter.SetBinding(Grid.ColumnProperty, CreateBinding(nameof(Index), this));

            BranchViews = _viewModel.Branches.ActiveTransform(x => new DiagramBranchView(this, x));

            return _contentPresenter;
        }

        internal DiagramCell GetCell(int subColumnIndex, int rowIndex)
        {
            Tuple<int, int> key = new Tuple<int, int>(subColumnIndex, rowIndex);
            if (!_cells.TryGetValue(key, out DiagramCell cell))
            {
                cell =
                    new DiagramCell(
                        ListTreeControl.GetRow(rowIndex),
                        subColumnIndex
                        );

                _cells.Add(key, cell);
            }

            return cell;
        }

        public ReadOnlyObservableCollection<DiagramBranchView> BranchViews { get; private set; }
        public DataTemplate Template { get; set; }

        private ContentPresenter _contentPresenter;
        private ListTreeViewModel _viewModel;

        private readonly Dictionary<Tuple<int, int>, DiagramCell> _cells;

        private static ResourceDictionary _resourceDictionary;
    }
}
