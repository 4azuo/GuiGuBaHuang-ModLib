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
                DebugHelper.ShowWarning(MessageHelper.Get("ErrorFillRequiredFields"), MessageHelper.Get("Error"));
                return;
            }


            var workplacePath = WindowData.WorkplacePath;
            if (string.IsNullOrWhiteSpace(workplacePath))
            {
                DebugHelper.ShowWarning(MessageHelper.Get("ErrorWorkplaceNotSet"), MessageHelper.Get("Error"));
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
