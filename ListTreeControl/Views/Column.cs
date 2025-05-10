using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace ListTree
{
    public abstract class Column : INotifyPropertyChanged
    {
        public Column()
        {
            _width = 100;
        }

        static Column()
        {
            Resources = new ResourceDictionary
            {
                Source = new Uri("/ListTreeControl;component/Resources/ColumnResources.xaml", UriKind.RelativeOrAbsolute)
            };
        }

        public string Name { get; set; }
        internal ListTreeControl ListTreeControl { get; set; }
        internal abstract object GetUIElements();

        public double Width
        {
            get
            {
                return _width;
            }

            set
            {
                if (_width == value)
                {
                    return;
                }

                _width = value;
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

        protected static Binding CreateBinding(string sourcePropertyName, object source)
        {
            return
                new Binding(sourcePropertyName)
                {
                    Mode = BindingMode.OneWay,
                    Source = source,
                    Converter = null,
                    ConverterParameter = null
                };
        }

        public DataTemplate Template { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected static ResourceDictionary Resources;

        private double _width;
        private int _index;
    }
}
