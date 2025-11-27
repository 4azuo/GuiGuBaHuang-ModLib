using ModCreator.Businesses;
using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.WindowData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
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
            try
            {
                var imgPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModImg");
                
                // Determine parent path
                string parentPath = imgPath;
                if (WindowData.SelectedImageItem != null && WindowData.SelectedImageItem.IsFolder)
                {
                    parentPath = WindowData.SelectedImageItem.FullPath;
                }
                else if (WindowData.SelectedImageItem != null && !WindowData.SelectedImageItem.IsFolder)
                {
                    parentPath = Path.GetDirectoryName(WindowData.SelectedImageItem.FullPath);
                }

                // Show InputWindow to get folder name
                var inputWindow = new InputWindow
                {
                    Owner = this
                };
                inputWindow.WindowData.WindowTitle = "Create New Folder";
                inputWindow.WindowData.Label = "Folder name:";
                inputWindow.WindowData.InputValue = "NewFolder";

                if (inputWindow.ShowDialog() != true)
                    return;

                var folderName = inputWindow.WindowData.InputValue;

                // Validate folder name
                if (string.IsNullOrWhiteSpace(folderName))
                {
                    MessageBox.Show(
                        "Folder name cannot be empty!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Check if folder name contains invalid characters
                var invalidChars = Path.GetInvalidFileNameChars();
                if (folderName.IndexOfAny(invalidChars) >= 0)
                {
                    MessageBox.Show(
                        "Folder name contains invalid characters!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var newFolderPath = Path.Combine(parentPath, folderName);

                // Check if folder already exists
                if (Directory.Exists(newFolderPath))
                {
                    MessageBox.Show(
                        $"A folder with the name '{folderName}' already exists!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Create the folder
                Directory.CreateDirectory(newFolderPath);

                // Reload image tree
                WindowData.LoadImageFiles();

                MessageBox.Show(
                    $"Folder '{folderName}' created successfully!",
                    MessageHelper.Get("Messages.Success.Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to create folder");
            }
        }

        private void DeleteImageFolder_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedImageItem == null || !WindowData.SelectedImageItem.IsFolder)
                return;

            try
            {
                var folderPath = WindowData.SelectedImageItem.FullPath;
                var folderName = WindowData.SelectedImageItem.Name;

                // Check if folder exists
                if (!Directory.Exists(folderPath))
                {
                    MessageBox.Show(
                        $"Folder '{folderName}' does not exist!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    WindowData.LoadImageFiles();
                    return;
                }

                // Check if folder has contents
                var hasContents = Directory.GetFileSystemEntries(folderPath).Length > 0;
                var warningMessage = hasContents
                    ? $"Are you sure you want to delete folder '{folderName}' and all its contents?"
                    : $"Are you sure you want to delete folder '{folderName}'?";

                var result = MessageBox.Show(
                    warningMessage,
                    "Delete Folder",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    Directory.Delete(folderPath, true);
                    
                    // Reload image tree
                    WindowData.LoadImageFiles();

                    MessageBox.Show(
                        $"Folder '{folderName}' deleted successfully!",
                        MessageHelper.Get("Messages.Success.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to delete folder");
                WindowData.LoadImageFiles();
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void ImportImage_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                // Build filter from ImageExtensions
                var extensionPatterns = string.Join(";", WindowData.ImageExtensions.Select(ext => $"*{ext.Extension}"));
                dialog.Filter = $"Image Files|{extensionPatterns}";
                
                dialog.Title = "Select Image to Import";
                dialog.Multiselect = true;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        // Determine target directory based on selected item
                        var imgPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModImg");
                        string targetPath = imgPath;
                        
                        if (WindowData.SelectedImageItem != null && WindowData.SelectedImageItem.IsFolder)
                        {
                            targetPath = WindowData.SelectedImageItem.FullPath;
                        }
                        else if (WindowData.SelectedImageItem != null && !WindowData.SelectedImageItem.IsFolder)
                        {
                            targetPath = Path.GetDirectoryName(WindowData.SelectedImageItem.FullPath);
                        }
                        
                        Directory.CreateDirectory(targetPath);

                        foreach (var file in dialog.FileNames)
                        {
                            var fileName = Path.GetFileName(file);
                            var destPath = Path.Combine(targetPath, fileName);
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
                
                // Build filter from ImageExtensions
                var extensionPatterns = string.Join(";", WindowData.ImageExtensions.Select(ext => $"*{ext.Extension}"));
                dialog.Filter = $"Image Files|{extensionPatterns}";
                
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
    }
}
