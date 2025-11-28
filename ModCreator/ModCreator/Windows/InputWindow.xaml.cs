using ModCreator.WindowData;
using System.ComponentModel;
using System.Windows;

namespace ModCreator.Windows
{
    public partial class InputWindow : CWindow<InputWindowData>
    {
        public override InputWindowData InitData(CancelEventArgs e)
        {
            var data = new InputWindowData();
            data.New();
            
            // Focus on input field when loaded
            Loaded += (s, args) =>
            {
                var textBox = this.FindName("txtInput") as System.Windows.Controls.TextBox;
                textBox?.Focus();
            };
            
            return data;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}