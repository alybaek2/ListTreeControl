using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ListTree
{
    public class Row : INotifyPropertyChanged
    {
        public Row()
        {
        }

        public double Top
        {
            get
            {
                return _top;
            }

            set
            {
                if (_top == value)
                {
                    return;
                }

                _top = value;
                OnPropertyChanged();
            }
        }

        public double Height
        {
            get
            {
                return _height;
            }

            set
            {
                if (_height == value)
                {
                    return;
                }

                _height = value;
                OnPropertyChanged();
            }
        }

        public int Index
        {
            get
            {
                return _index;
            }

            internal set
            {
                if (_index == value)
                {
                    return;
                }

                _index = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private int _index;
        private double _top;
        private double _height;
    }
}
