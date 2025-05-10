using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ListTree
{
    public class TextColumn : SimpleColumn
    {
        public TextColumn()
        {
            Template = (DataTemplate)Resources["TextTemplate"];
        }

        public string PropertyName { get; set; }

        internal override void OnApplyTemplate(ContentPresenter contentPresenter, NodeView nodeView)
        {
            TextBlock textBlock = (TextBlock)contentPresenter.ContentTemplate.FindName("textBlock", contentPresenter);

            Binding textBinding = new Binding(PropertyName)
            {
                Mode = BindingMode.OneWay,
                Source = nodeView.Node.Data
            };

            textBlock.SetBinding(TextBlock.TextProperty, textBinding);
        }
    }
}
