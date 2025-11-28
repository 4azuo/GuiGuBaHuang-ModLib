using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.WindowData;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace ModCreator.Windows
{
    public partial class ProjectEditorWindow : CWindow<ProjectEditorWindowData>
    {
        private void TreeView_ImageSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is FileItem fileItem)
            {
                WindowData.SelectedImageItem = fileItem;
            }
        }

        private void CreateImageFolder_Click(object sender, RoutedEventArgs e)
        {
            var imgPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModImg");
            
            string parentPath = imgPath;
            if (WindowData.SelectedImageItem != null)
            {
                parentPath = WindowData.SelectedImageItem.IsFolder
                    ? WindowData.SelectedImageItem.FullPath
                    : Path.GetDirectoryName(WindowData.SelectedImageItem.FullPath);
            }

            var inputWindow = new InputWindow
            {
                Owner = this,
                WindowData = { WindowTitle = "Create New Folder", Label = "Folder name:", InputValue = "NewFolder" }
            };

            if (inputWindow.ShowDialog() != true) return;

            var folderName = inputWindow.WindowData.InputValue;

            if (string.IsNullOrWhiteSpace(folderName))
            {
                MessageBox.Show("Folder name cannot be empty!", MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (folderName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                MessageBox.Show("Folder name contains invalid characters!", MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newFolderPath = Path.Combine(parentPath, folderName);

            if (Directory.Exists(newFolderPath))
            {
                MessageBox.Show($"A folder with the name '{folderName}' already exists!", MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Directory.CreateDirectory(newFolderPath);
            WindowData.LoadImageFiles();
            MessageBox.Show($"Folder '{folderName}' created successfully!", MessageHelper.Get("Messages.Success.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteImageFolder_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedImageItem == null || !WindowData.SelectedImageItem.IsFolder) return;

            var folderPath = WindowData.SelectedImageItem.FullPath;
            var folderName = WindowData.SelectedImageItem.Name;

            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show($"Folder '{folderName}' does not exist!", MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                WindowData.LoadImageFiles();
                return;
            }

            var hasContents = Directory.GetFileSystemEntries(folderPath).Length > 0;
            var warningMessage = hasContents
                ? $"Are you sure you want to delete folder '{folderName}' and all its contents?"
                : $"Are you sure you want to delete folder '{folderName}'?";

            var result = MessageBox.Show(warningMessage, "Delete Folder", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                Directory.Delete(folderPath, true);
                WindowData.LoadImageFiles();
                MessageBox.Show($"Folder '{folderName}' deleted successfully!", MessageHelper.Get("Messages.Success.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void ImportImage_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new OpenFileDialog
            {
                Filter = $"Image Files|{string.Join(";", WindowData.ImageExtensions.Select(ext => $"*{ext.Extension}"))}",
                Title = "Select Image to Import",
                Multiselect = true
            })
            {
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                var imgPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModImg");
                string targetPath = imgPath;
                
                if (WindowData.SelectedImageItem != null)
                {
                    targetPath = WindowData.SelectedImageItem.IsFolder
                        ? WindowData.SelectedImageItem.FullPath
                        : Path.GetDirectoryName(WindowData.SelectedImageItem.FullPath);
                }
                
                Directory.CreateDirectory(targetPath);

                foreach (var file in dialog.FileNames)
                {
                    var destPath = Path.Combine(targetPath, Path.GetFileName(file));
                    File.Copy(file, destPath, true);
                }

                WindowData.LoadImageFiles();
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void ExportImage_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(WindowData.SelectedImageFile)) return;

            using (var dialog = new SaveFileDialog
            {
                FileName = WindowData.SelectedImageFile,
                Filter = $"Image Files|{string.Join(";", WindowData.ImageExtensions.Select(ext => $"*{ext.Extension}"))}",
                Title = "Export Image"
            })
            {
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                var sourcePath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModImg", WindowData.SelectedImageFile);
                File.Copy(sourcePath, dialog.FileName, true);
                MessageBox.Show("Image exported successfully!", MessageHelper.Get("Messages.Success.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(WindowData.SelectedImageFile)) return;

            var result = MessageBox.Show($"Are you sure you want to delete '{WindowData.SelectedImageFile}'?", "Delete Image", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var filePath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModImg", WindowData.SelectedImageFile);
                WindowData.SelectedImageFile = null;
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    WindowData.LoadImageFiles();
                }
            }
        }

        private void OpenModImgFolder_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData?.Project == null) return;
            
            var imgPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModImg");
            Directory.CreateDirectory(imgPath);
            System.Diagnostics.Process.Start("explorer.exe", imgPath);
        }
    }
}