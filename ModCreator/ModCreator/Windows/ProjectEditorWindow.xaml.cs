using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.WindowData;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace ModCreator.Windows
{
    public partial class ProjectEditorWindow : CWindow<ProjectEditorWindowData>
    {
        /// <summary>
        /// Project to edit - set before showing dialog
        /// </summary>
        public ModProject ProjectToEdit { get; set; }

        private void ProjectEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Now ProjectToEdit has been set by the caller
            if (ProjectToEdit != null && WindowData != null)
            {
                WindowData.Project = ProjectToEdit;
            }
        }

        public override ProjectEditorWindowData InitData(CancelEventArgs e)
        {
            var data = new ProjectEditorWindowData();
            data.New();
            
            Loaded += ProjectEditorWindow_Loaded;
            
            return data;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WindowData.SaveProject();
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"),
                    MessageHelper.GetFormat("Messages.Error.ErrorUpdatingProject", ex.Message));
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #region Tab 2: ModConf Event Handlers

        private void AddConf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addConfWindow = new AddConfWindow
                {
                    Owner = this
                };

                if (addConfWindow.ShowDialog() == true)
                {
                    var selectedConfig = addConfWindow.WindowData.SelectedConfig;
                    if (selectedConfig != null)
                    {
                        // Copy the selected configuration file to ModConf folder
                        var confPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModConf");
                        Directory.CreateDirectory(confPath);

                        var fileName = addConfWindow.WindowData.GetFileName();
                        var destPath = Path.Combine(confPath, fileName);
                        File.Copy(selectedConfig.FilePath, destPath, true);

                        // Reload configuration list
                        WindowData.LoadConfFiles();

                        // Select the newly added file
                        WindowData.SelectedConfFile = fileName;
                    }
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to add configuration file");
            }
        }

        private void RemoveConf_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(WindowData.SelectedConfFile))
                return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{WindowData.SelectedConfFile}'?",
                "Delete Configuration",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var filePath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModConf", WindowData.SelectedConfFile);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        // Reload configuration list
                        WindowData.LoadConfFiles();
                    }
                }
                catch (Exception ex)
                {
                    DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to delete configuration file");
                }
            }
        }

        #endregion

        #region Tab 3: ModImg Event Handlers

        [SupportedOSPlatform("windows6.1")]
        private void ImportImage_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All Files|*.*";
                dialog.Title = "Select Image to Import";
                dialog.Multiselect = true;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        var imgDir = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModImg");
                        Directory.CreateDirectory(imgDir);

                        foreach (var file in dialog.FileNames)
                        {
                            var fileName = Path.GetFileName(file);
                            var destPath = Path.Combine(imgDir, fileName);
                            File.Copy(file, destPath, true);
                        }

                        // Reload image list
                        WindowData.LoadImageFiles();
                    }
                    catch (Exception ex)
                    {
                        DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to import image");
                    }
                }
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void ExportImage_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(WindowData.SelectedImageFile))
                return;

            using (var dialog = new SaveFileDialog())
            {
                dialog.FileName = WindowData.SelectedImageFile;
                dialog.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All Files|*.*";
                dialog.Title = "Export Image";

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        var sourcePath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModImg", WindowData.SelectedImageFile);
                        File.Copy(sourcePath, dialog.FileName, true);
                        MessageBox.Show("Image exported successfully!", 
                            MessageHelper.Get("Messages.Success.Title"), 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to export image");
                    }
                }
            }
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(WindowData.SelectedImageFile))
                return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{WindowData.SelectedImageFile}'?",
                "Delete Image",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Get file path before clearing selection
                    var filePath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModImg", WindowData.SelectedImageFile);
                    
                    // Clear selection (this releases the BitmapImage since it's frozen)
                    WindowData.SelectedImageFile = null;
                    
                    // Delete file - no lock since BitmapImage was loaded with CacheOption.OnLoad and Freeze()
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        // Reload image list after deletion
                        WindowData.LoadImageFiles();
                    }
                }
                catch (Exception ex)
                {
                    DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to delete image");
                }
            }
        }

        #endregion

        #region Tab 1, 2, 3: Folder and Refresh Event Handlers

        private void OpenProjectFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (WindowData?.Project != null && Directory.Exists(WindowData.Project.ProjectPath))
                {
                    var projectPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject");
                    if (Directory.Exists(projectPath))
                    {
                        System.Diagnostics.Process.Start("explorer.exe", projectPath);
                    }
                    else
                    {
                        System.Diagnostics.Process.Start("explorer.exe", WindowData.Project.ProjectPath);
                    }
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to open folder");
            }
        }

        private void OpenModConfFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (WindowData?.Project != null)
                {
                    var confPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModConf");
                    Directory.CreateDirectory(confPath);
                    System.Diagnostics.Process.Start("explorer.exe", confPath);
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to open ModConf folder");
            }
        }

        private void OpenModImgFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (WindowData?.Project != null)
                {
                    var imgPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModImg");
                    Directory.CreateDirectory(imgPath);
                    System.Diagnostics.Process.Start("explorer.exe", imgPath);
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to open ModImg folder");
            }
        }

        private void RefreshTab_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WindowData?.ReloadProjectData();
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to refresh");
            }
        }

        #endregion

        #region Tab 4: Global Variables Event Handlers

        private void GenerateVariablesCode_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Generate code from EventTemplate.tmp
            MessageBox.Show(MessageHelper.Get("Windows.ProjectEditorWindow.FeatureComingSoon"), 
                MessageHelper.Get("Messages.Info.Title"), 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }

        #endregion
    }
}
