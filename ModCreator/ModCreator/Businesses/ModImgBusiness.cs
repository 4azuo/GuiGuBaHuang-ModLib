using ModCreator.Helpers;
using ModCreator.WindowData;
using ModCreator.Windows;
using System;
using System.IO;
using System.Windows;

namespace ModCreator.Businesses
{
    /// <summary>
    /// Business logic for ModImg operations
    /// </summary>
    public class ModImgBusiness
    {
        private readonly ProjectEditorWindowData _windowData;
        private readonly Window _owner;

        public ModImgBusiness(ProjectEditorWindowData windowData, Window owner)
        {
            _windowData = windowData;
            _owner = owner;
        }

        public void AddImage()
        {
            try
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All Files (*.*)|*.*",
                    Multiselect = true
                };

                if (dialog.ShowDialog() == true)
                {
                    var imgPath = Path.Combine(_windowData.Project.ProjectPath, "ModProject", "ModImg");
                    string targetPath = imgPath;

                    if (_windowData.SelectedImageItem != null && _windowData.SelectedImageItem.IsFolder)
                    {
                        targetPath = _windowData.SelectedImageItem.FullPath;
                    }
                    else if (_windowData.SelectedImageItem != null && !_windowData.SelectedImageItem.IsFolder)
                    {
                        targetPath = Path.GetDirectoryName(_windowData.SelectedImageItem.FullPath);
                    }

                    Directory.CreateDirectory(targetPath);

                    foreach (var file in dialog.FileNames)
                    {
                        var fileName = Path.GetFileName(file);
                        var destPath = Path.Combine(targetPath, fileName);
                        File.Copy(file, destPath, true);
                    }

                    _windowData.LoadImageFiles();
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to add image file");
            }
        }

        public void RemoveImage()
        {
            if (string.IsNullOrEmpty(_windowData.SelectedImageFile))
                return;

            try
            {
                var filePath = Path.Combine(_windowData.Project.ProjectPath, "ModProject", "ModImg", _windowData.SelectedImageFile);

                if (!File.Exists(filePath))
                {
                    MessageBox.Show(
                        $"File '{_windowData.SelectedImageFile}' does not exist!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    _windowData.LoadImageFiles();
                    return;
                }

                var result = MessageBox.Show(
                    $"Are you sure you want to delete '{_windowData.SelectedImageFile}'?",
                    "Delete Image",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    File.Delete(filePath);
                    _windowData.LoadImageFiles();
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to delete image file");
                _windowData.LoadImageFiles();
            }
        }

        public void RenameImage()
        {
            if (string.IsNullOrEmpty(_windowData.SelectedImageFile))
                return;

            try
            {
                var oldFileName = _windowData.SelectedImageFile;
                var imgPath = Path.Combine(_windowData.Project.ProjectPath, "ModProject", "ModImg");
                var oldFilePath = Path.Combine(imgPath, oldFileName);

                if (!File.Exists(oldFilePath))
                {
                    MessageBox.Show(
                        $"Source file '{oldFileName}' does not exist!",
                        MessageHelper.Get("Messages.Error.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    _windowData.LoadImageFiles();
                    return;
                }

                var inputWindow = new InputWindow
                {
                    Owner = _owner
                };
                inputWindow.WindowData.WindowTitle = "Rename Image File";
                inputWindow.WindowData.Label = "New file name:";
                inputWindow.WindowData.InputValue = oldFileName;

                if (inputWindow.ShowDialog() != true)
                    return;

                var newFileName = inputWindow.WindowData.InputValue;

                if (string.IsNullOrWhiteSpace(newFileName))
                {
                    MessageBox.Show(
                        "File name cannot be empty!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var invalidChars = Path.GetInvalidFileNameChars();
                if (newFileName.IndexOfAny(invalidChars) >= 0)
                {
                    MessageBox.Show(
                        "File name contains invalid characters!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (string.Equals(oldFileName, newFileName, StringComparison.OrdinalIgnoreCase))
                    return;

                var newFilePath = Path.Combine(Path.GetDirectoryName(oldFilePath), newFileName);

                if (File.Exists(newFilePath))
                {
                    MessageBox.Show(
                        $"A file with the name '{newFileName}' already exists!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                File.Move(oldFilePath, newFilePath);

                _windowData.LoadImageFiles();
                _windowData.SelectedImageFile = Path.GetRelativePath(imgPath, newFilePath);

                MessageBox.Show(
                    $"Image file renamed successfully!\n\nOld name: {oldFileName}\nNew name: {newFileName}",
                    MessageHelper.Get("Messages.Success.Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to rename image file");
                _windowData.LoadImageFiles();
            }
        }

        public void OpenModImgFolder()
        {
            try
            {
                var modImgPath = Path.Combine(_windowData.Project.ProjectPath, "ModProject", "ModImg");
                if (Directory.Exists(modImgPath))
                {
                    System.Diagnostics.Process.Start("explorer.exe", modImgPath);
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to open ModImg folder");
            }
        }
    }
}
