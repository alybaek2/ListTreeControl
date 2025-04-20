using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ListTree
{
    public interface IListTreeNode : INotifyPropertyChanged
    {
        IListTreeNode Insert(int childIndex, object data, int verticalIndex);
        void UpdateParent(IListTreeNode newParentNode, int newChildIndex);
        void Delete();

        IListTreeNode Parent { get; }
        object Data { get; }

        int ChildIndex { get; set; }
        int VerticalIndex { get; set; }

        IReadOnlyObservableList<IListTreeNode> Children { get; }
    }
}
