using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace ListTree
{
    public class SimpleColumn : Column
    {
        internal override object GetUIElements()
        {
            if (_uiElements == null)
            {
                ContentPresenter Create(NodeView nodeView)
                {
                    ContentPresenter contentPresenter = new ContentPresenter
                    {
                        ContentTemplate = Template,
                        Content = nodeView.Node.Data
                    };

                    contentPresenter.ApplyTemplate();

                    OnApplyTemplate(contentPresenter, nodeView);

                    // Set the row and column.
                    contentPresenter.SetBinding(Grid.RowProperty, CreateBinding(_rowIndexPath, nodeView));
                    contentPresenter.SetBinding(Grid.ColumnProperty, CreateBinding(nameof(Index), this));

                    return contentPresenter;
                }

                _uiElements = new CollectionContainer
                {
                    Collection = ListTreeControl.NodeViews.ActiveTransform(Create)
                };
            }

            return _uiElements;
        }

        internal virtual void OnApplyTemplate(ContentPresenter contentPresenter, NodeView nodeView)
        {
        }

        private const string _rowIndexPath = nameof(NodeView.Row) + "." + nameof(Row.Index);
        private CollectionContainer _uiElements;
    }
}
