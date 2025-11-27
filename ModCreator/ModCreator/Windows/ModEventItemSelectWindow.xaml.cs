using ModCreator.WindowData;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace ModCreator.Windows
{
    public partial class ModEventItemSelectWindow : CWindow<ModEventItemSelectWindowData>
    {
        public override ModEventItemSelectWindowData InitData(CancelEventArgs e)
        {
            var data = new ModEventItemSelectWindowData();
            return data;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedItem != null)
            {
                DialogResult = true;
                Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (WindowData.SelectedItem != null)
            {
                DialogResult = true;
                Close();
            }
        }
    }
}
