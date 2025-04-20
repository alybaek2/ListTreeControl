using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ListTree
{
    public class TextColumn : Column
    {
        public TextColumn()
        {
        }

        public string PropertyName { get; set; }

        internal override object GetUIElements()
        {
            if (ListTreeControl.ListTree == null)
            {
                return null;
            }

            _views = ListTreeControl.ListTree.NodesByVerticalIndex.ActiveTransform(x => new NodeView(ListTreeControl, x));
            _textBlocks = _views.ActiveTransform(CreateTextBlock);

            CollectionContainer container = new CollectionContainer
            {
                Collection = _textBlocks
            };

            return container;
        }

        private TextBlock CreateTextBlock(NodeView view)
        {
            TextBlock textBlock = new TextBlock
            {
                TextTrimming = TextTrimming.CharacterEllipsis,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(1, 1, 1, 1)
            };

            Binding textBinding = new Binding(PropertyName)
            {
                Mode = BindingMode.OneWay,
                Source = view.Node.Data
            };

            textBlock.SetBinding(TextBlock.TextProperty, textBinding);

            // Set the row and column.
            textBlock.SetBinding(Grid.RowProperty, CreateBinding(_rowIndexPath, view));
            textBlock.SetBinding(Grid.ColumnProperty, CreateBinding(nameof(Index), this));

            return textBlock;
        }

        static private string _rowIndexPath = String.Format("{0}.{1}", nameof(NodeView.Row), nameof(Row.Index));

        private ReadOnlyObservableCollection<NodeView> _views;
        private ReadOnlyObservableCollection<TextBlock> _textBlocks;
    }
}
