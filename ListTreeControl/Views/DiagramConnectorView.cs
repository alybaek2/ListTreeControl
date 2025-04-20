using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ListTree
{
    public class DiagramConnectorView : INotifyPropertyChanged
    {
        public DiagramConnectorView(DiagramColumn diagramColumn, DiagramConnectorViewModel nodeViewModel)
        {
            _diagramColumn = diagramColumn;
            _nodeViewModel = nodeViewModel;

            SetParent(nodeViewModel.Parent);

            _nodeViewModel.PropertyChanged += NodeViewModel_PropertyChanged;
        }

        public DiagramCell Start
        {
            get
            {
                if (_nodeViewModel.Parent == null)
                {
                    return null;
                }

                return _diagramColumn.GetCell(_nodeViewModel.SubColumn, _nodeViewModel.Row);
            }
        }

        public DiagramCell End
        {
            get
            {
                if (_nodeViewModel.Parent == null)
                {
                    return null;
                }

                return _diagramColumn.GetCell(_nodeViewModel.Parent.SubColumn, _nodeViewModel.Parent.Row);
            }
        }

        private void NodeViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DiagramConnectorViewModel.SubColumn):
                case nameof(DiagramConnectorViewModel.Row):
                    OnPropertyChanged(nameof(Start));
                    break;
                case nameof(DiagramConnectorViewModel.Parent):
                    SetParent(_nodeViewModel.Parent);
                    OnPropertyChanged(nameof(End));
                    break;
            }
        }

        private void Parent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DiagramConnectorViewModel.SubColumn):
                case nameof(DiagramConnectorViewModel.Row):
                    OnPropertyChanged(nameof(End));
                    break;
            }
        }

        private void SetParent(DiagramConnectorViewModel value)
        {
            if (_nodeViewModelParent == value)
            {
                return;
            }

            if (_nodeViewModelParent != null)
            {
                _nodeViewModelParent.PropertyChanged -= Parent_PropertyChanged;
            }

            _nodeViewModelParent = value;

            if (_nodeViewModelParent != null)
            {
                _nodeViewModelParent.PropertyChanged += Parent_PropertyChanged;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private readonly DiagramConnectorViewModel _nodeViewModel;
        private readonly DiagramColumn _diagramColumn;
        private DiagramConnectorViewModel _nodeViewModelParent;
    }
}
