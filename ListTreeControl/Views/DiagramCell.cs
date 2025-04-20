using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ListTree
{
    public class DiagramCell : INotifyPropertyChanged
    {
        public DiagramCell(Row row, int subColumn)
        {
            _row = row;
            _subColumn = subColumn;

            _row.PropertyChanged += Row_PropertyChanged;
        }

        public Point Center
        {
            get
            {
                return
                    new Point
                    {
                        X = (_subColumn + 0.5) * 16,
                        Y = _row.Top + 0.5 * _row.Height
                    };
            }
        }

        public Point CenterTop
        {
            get
            {
                return
                    new Point
                    {
                        X = (_subColumn + 0.5) * 16,
                        Y = _row.Top
                    };
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Row_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Row.Top):
                case nameof(Row.Height):
                    OnPropertyChanged(nameof(Center));
                    OnPropertyChanged(nameof(CenterTop));
                    break;
            }
        }

        private readonly int _subColumn;
        private readonly Row _row;
    }
}
