using ModCreator.Helpers;
using ModCreator.WindowData;
using System.ComponentModel;
using System.Windows;

namespace ModCreator.Windows
{
    public partial class NewProjectWindow : CWindow<NewProjectWindowData>
    {
        public override NewProjectWindowData InitData(CancelEventArgs e)
        {
            var data = new NewProjectWindowData();
            data.New();
            
            // Load saved author from settings
            data.Author = Properties.Settings.Default.Author ?? string.Empty;
            
            return data;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!WindowData.CanCreate)
            {
                DebugHelper.ShowWarning(MessageHelper.Get("Messages.Error.ErrorFillRequiredFields"), MessageHelper.Get("Messages.Error.Title"));
                return;
            }


            var workplacePath = WindowData.WorkplacePath;
            if (string.IsNullOrWhiteSpace(workplacePath))
            {
                DebugHelper.ShowWarning(MessageHelper.Get("Messages.Error.ErrorWorkplaceNotSet"), MessageHelper.Get("Messages.Error.Title"));
                return;
            }
            // ProjectId will be generated inside CreateProject
            WindowData.CreateProject(workplacePath);

            // Save author to settings for next time
            Properties.Settings.Default.Author = WindowData.Author;
            Properties.Settings.Default.Save();

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