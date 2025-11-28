using ModCreator.Helpers;
using ModCreator.WindowData;
using System.IO;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;

namespace ModCreator.Windows
{
    public partial class ProjectEditorWindow : CWindow<ProjectEditorWindowData>
    {
        private Image titleImage;

        [SupportedOSPlatform("windows6.1")]
        private void SetupTitleImage()
        {
            LoadTitleImage();
        }

        [SupportedOSPlatform("windows6.1")]
        private void LoadTitleImage()
        {
            var container = this.FindName("TitleImageContainer") as Border;
            if (container == null || WindowData?.Project == null) return;

            if (titleImage != null)
            {
                container.Child = null;
                titleImage = null;
            }

            if (string.IsNullOrEmpty(WindowData.Project.TitleImg)) return;

            titleImage = new Image
            {
                Stretch = System.Windows.Media.Stretch.Uniform
            };

            try
            {
                var bitmap = BitmapHelper.LoadFromFile(WindowData.Project.TitleImg);
                if (bitmap != null)
                {
                    titleImage.Source = bitmap;
                    container.Child = titleImage;
                }
            }
            catch { }
        }

        [SupportedOSPlatform("windows6.1")]
        private void TitleImage_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select Title Image",
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif"
            };

            if (dialog.ShowDialog() == true)
            {
                WindowData.Project.TitleImg = dialog.FileName;
                LoadTitleImage();
            }
        }
        
        private void OpenProjectFolder_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData?.Project == null || !Directory.Exists(WindowData.Project.ProjectPath)) return;

            var projectPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject");
            var folderToOpen = Directory.Exists(projectPath) ? projectPath : WindowData.Project.ProjectPath;
            System.Diagnostics.Process.Start("explorer.exe", folderToOpen);
        }
    }
}