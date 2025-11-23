using ModCreator.Helpers;
using ModCreator.WindowData;
using System.ComponentModel;
using System.Windows;

namespace ModCreator.Windows
{
    public partial class NewProjectWindow : CWindow<NewProjectWindowData>
    {
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!WindowData.CanCreate)
            {
                System.Windows.MessageBox.Show(
                    "Please fill in all required fields!",
                    MessageHelper.Error,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }


            var workplacePath = WindowData.WorkplacePath;
            if (string.IsNullOrWhiteSpace(workplacePath))
            {
                System.Windows.MessageBox.Show(
                    "Workplace path is not set!",
                    MessageHelper.Error,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            // ProjectId will be generated inside CreateProject
            WindowData.CreateProject(workplacePath);

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
