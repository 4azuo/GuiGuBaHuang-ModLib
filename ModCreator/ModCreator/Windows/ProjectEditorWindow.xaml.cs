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

        public override ProjectEditorWindowData InitData(CancelEventArgs e)
        {
            var data = new ProjectEditorWindowData();
            data.New();
            
            // Set project if provided
            if (ProjectToEdit != null)
            {
                data.Project = ProjectToEdit;
            }
            
            return data;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WindowData.SaveProject();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Error"),
                    MessageHelper.GetFormat("ErrorUpdatingProject", ex.Message));
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #region Tab 2: ModConf Event Handlers

        private void AddConf_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Show AddConfWindow dialog
            MessageBox.Show(MessageHelper.Get("FeatureComingSoon"), 
                MessageHelper.Get("Info"), 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
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
                        WindowData.ConfFiles.Remove(WindowData.SelectedConfFile);
                        WindowData.SelectedConfFile = null;
                    }
                }
                catch (Exception ex)
                {
                    DebugHelper.ShowError(ex, MessageHelper.Get("Error"), "Failed to delete configuration file");
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
                        WindowData.ReloadProjectData();
                    }
                    catch (Exception ex)
                    {
                        DebugHelper.ShowError(ex, MessageHelper.Get("Error"), "Failed to import image");
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
                        var sourcePath = WindowData.SelectedImagePath;
                        File.Copy(sourcePath, dialog.FileName, true);
                        MessageBox.Show("Image exported successfully!", 
                            MessageHelper.Get("Success"), 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        DebugHelper.ShowError(ex, MessageHelper.Get("Error"), "Failed to export image");
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
                    var filePath = WindowData.SelectedImagePath;
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        WindowData.ImageFiles.Remove(WindowData.SelectedImageFile);
                        WindowData.SelectedImageFile = null;
                    }
                }
                catch (Exception ex)
                {
                    DebugHelper.ShowError(ex, MessageHelper.Get("Error"), "Failed to delete image");
                }
            }
        }

        #endregion

        #region Tab 4: Global Variables Event Handlers

        private void GenerateVariablesCode_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Generate code from EventTemplate.tmp
            MessageBox.Show(MessageHelper.Get("FeatureComingSoon"), 
                MessageHelper.Get("Info"), 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }

        #endregion
    }
}
