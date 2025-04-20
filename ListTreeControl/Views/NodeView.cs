using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ListTree
{
    public class NodeView : INotifyPropertyChanged
    {
        public NodeView(ListTreeControl listTreeCtrl, IListTreeNode listTreeNode)
        {
            _listTreeCtrl = listTreeCtrl;
            _listTreeNode = listTreeNode;

            listTreeNode.PropertyChanged += ListTreeNode_PropertyChanged;
        }

        public Row Row
        {
            get
            {
                return _listTreeCtrl.GetRow(_listTreeNode.VerticalIndex);
            }
        }

        public IListTreeNode Node
        {
            get
            {
                return _listTreeNode;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ListTreeNode_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IListTreeNode.VerticalIndex))
            {
                OnPropertyChanged(nameof(Row));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ListTreeControl _listTreeCtrl;
        private readonly IListTreeNode _listTreeNode;
    }
}
