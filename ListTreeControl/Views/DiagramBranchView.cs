using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ListTree
{
    public class DiagramBranchView : INotifyPropertyChanged
    {
        public DiagramBranchView(DiagramColumn diagramColumn, DiagramBranchViewModel viewModel)
        {
            _diagramColumn = diagramColumn;
            ViewModel = viewModel;
            Connectors = viewModel.Connectors.ActiveTransform(x => new DiagramConnectorView(_diagramColumn, x));

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        public DiagramBranchViewModel ViewModel
        {
            get;
        }

        public DiagramCell Cell
        {
            get
            {
                return _diagramColumn.GetCell(ViewModel.SubColumn, ViewModel.Row);
            }
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DiagramConnectorViewModel.SubColumn):
                case nameof(DiagramConnectorViewModel.Row):
                    OnPropertyChanged(nameof(Cell));
                    break;
            }
        }

        public ReadOnlyObservableCollection<DiagramConnectorView> Connectors
        {
            get;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private readonly DiagramColumn _diagramColumn;
    }
}
