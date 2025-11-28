using ModCreator.Models;
using ModCreator.WindowData;
using System.Windows;

namespace ModCreator.Windows
{
    public partial class HelperWindow : CWindow<HelperWindowData>
    {
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is DocItem docItem && !docItem.IsFolder)
            {
                WindowData.SelectedDoc = docItem;
            }
        }
    }
}