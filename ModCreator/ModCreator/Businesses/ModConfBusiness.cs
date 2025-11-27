using ModCreator.Helpers;
using ModCreator.WindowData;
using ModCreator.Windows;
using System;
using System.IO;
using System.Windows;

namespace ModCreator.Businesses
{
    /// <summary>
    /// Business logic for ModConf operations
    /// </summary>
    public class ModConfBusiness
    {
        private readonly ProjectEditorWindowData _windowData;
        private readonly Window _owner;

        public ModConfBusiness(ProjectEditorWindowData windowData, Window owner)
        {
            _windowData = windowData;
            _owner = owner;
        }

        public void AddConf()
        {
            try
            {
                var addConfWindow = new AddConfWindow
                {
                    Owner = _owner
                };

                if (addConfWindow.ShowDialog() == true)
                {
                    var selectedConfig = addConfWindow.WindowData.SelectedConfig;
                    if (selectedConfig != null)
                    {
                        var confPath = Path.Combine(_windowData.Project.ProjectPath, "ModProject", "ModConf");
                        string targetPath = confPath;
                        
                        if (_windowData.SelectedConfItem != null && _windowData.SelectedConfItem.IsFolder)
                        {
                            targetPath = _windowData.SelectedConfItem.FullPath;
                        }
                        else if (_windowData.SelectedConfItem != null && !_windowData.SelectedConfItem.IsFolder)
                        {
                            targetPath = Path.GetDirectoryName(_windowData.SelectedConfItem.FullPath);
                        }
                        
                        Directory.CreateDirectory(targetPath);

                        var fileName = addConfWindow.WindowData.GetFileName();
                        var destPath = Path.Combine(targetPath, fileName);
                        File.Copy(selectedConfig.FilePath, destPath, true);

                        _windowData.LoadConfFiles();
                        _windowData.SelectedConfFile = Path.GetRelativePath(confPath, destPath);
                    }
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to add configuration file");
            }
        }

        public void RemoveConf()
        {
            if (string.IsNullOrEmpty(_windowData.SelectedConfFile))
                return;

            try
            {
                var filePath = Path.Combine(_windowData.Project.ProjectPath, "ModProject", "ModConf", _windowData.SelectedConfFile);
                
                if (!File.Exists(filePath))
                {
                    MessageBox.Show(
                        $"File '{_windowData.SelectedConfFile}' does not exist!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    _windowData.LoadConfFiles();
                    return;
                }

                var result = MessageBox.Show(
                    $"Are you sure you want to delete '{_windowData.SelectedConfFile}'?",
                    "Delete Configuration",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    File.Delete(filePath);
                    _windowData.LoadConfFiles();
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to delete configuration file");
                _windowData.LoadConfFiles();
            }
        }

        public void CloneConf()
        {
            if (string.IsNullOrEmpty(_windowData.SelectedConfFile))
                return;

            try
            {
                var confPath = Path.Combine(_windowData.Project.ProjectPath, "ModProject", "ModConf");
                var sourceFile = Path.Combine(confPath, _windowData.SelectedConfFile);
                
                if (!File.Exists(sourceFile))
                {
                    MessageBox.Show(
                        $"Source file '{_windowData.SelectedConfFile}' does not exist!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    _windowData.LoadConfFiles();
                    return;
                }

                var sourceDir = Path.GetDirectoryName(sourceFile);
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(Path.GetFileName(_windowData.SelectedConfFile));
                var extension = Path.GetExtension(_windowData.SelectedConfFile);
                var newFileName = $"{fileNameWithoutExt}_copy{extension}";
                
                var counter = 1;
                while (File.Exists(Path.Combine(sourceDir, newFileName)))
                {
                    newFileName = $"{fileNameWithoutExt}_copy{counter}{extension}";
                    counter++;
                }

                var destFile = Path.Combine(sourceDir, newFileName);
                File.Copy(sourceFile, destFile);

                _windowData.LoadConfFiles();
                _windowData.SelectedConfFile = Path.GetRelativePath(confPath, destFile);

                MessageBox.Show(
                    $"Configuration file cloned successfully!\n\nNew file: {newFileName}",
                    MessageHelper.Get("Messages.Success.Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to clone configuration file");
                _windowData.LoadConfFiles();
            }
        }

        public void RenameConf()
        {
            if (string.IsNullOrEmpty(_windowData.SelectedConfFile))
                return;

            try
            {
                var oldFileName = _windowData.SelectedConfFile;
                var confPath = Path.Combine(_windowData.Project.ProjectPath, "ModProject", "ModConf");
                var oldFilePath = Path.Combine(confPath, oldFileName);

                if (!File.Exists(oldFilePath))
                {
                    MessageBox.Show(
                        $"Source file '{oldFileName}' does not exist!",
                        MessageHelper.Get("Messages.Error.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    _windowData.LoadConfFiles();
                    return;
                }

                var inputWindow = new InputWindow
                {
                    Owner = _owner
                };
                inputWindow.WindowData.WindowTitle = "Rename Configuration File";
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

                if (!newFileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    newFileName += ".json";
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

                _windowData.LoadConfFiles();
                _windowData.SelectedConfFile = Path.GetRelativePath(confPath, newFilePath);

                MessageBox.Show(
                    $"Configuration file renamed successfully!\n\nOld name: {oldFileName}\nNew name: {newFileName}",
                    MessageHelper.Get("Messages.Success.Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to rename configuration file");
                _windowData.LoadConfFiles();
            }
        }

        public void OpenModConfFolder()
        {
            try
            {
                var modConfPath = Path.Combine(_windowData.Project.ProjectPath, "ModProject", "ModConf");
                if (Directory.Exists(modConfPath))
                {
                    System.Diagnostics.Process.Start("explorer.exe", modConfPath);
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to open ModConf folder");
            }
        }
    }
}
