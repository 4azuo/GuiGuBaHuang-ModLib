using ModCreator.Helpers;
using ModCreator.WindowData;
using System.ComponentModel;
using System.Windows;

namespace ModCreator.Windows
{
    public partial class AddConfWindow : CWindow<AddConfWindowData>
    {
        public override AddConfWindowData InitData(CancelEventArgs e)
        {
            var data = new AddConfWindowData();
            data.LoadConfigurations();
            return data;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedConfig != null)
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
    }
}
