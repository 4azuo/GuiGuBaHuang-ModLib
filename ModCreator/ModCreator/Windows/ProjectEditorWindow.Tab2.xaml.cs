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
        [SupportedOSPlatform("windows6.1")]
        private void SetupAvalonEditBinding()
        {
            var editor = this.FindName("txtJsonEditor") as ICSharpCode.AvalonEdit.TextEditor;
            if (editor == null) return;

            // Load custom JSON syntax highlighting
            AvalonHelper.LoadJsonSyntaxHighlighting(editor);

            // Subscribe to property changes for SelectedConfContent
            WindowData.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(WindowData.SelectedConfContent))
                {
                    // Update AvalonEdit text when property changes
                    if (editor.Text != WindowData.SelectedConfContent)
                    {
                        editor.Text = WindowData.SelectedConfContent ?? string.Empty;
                    }
                }
            };

            // Subscribe to AvalonEdit text changes
            editor.TextChanged += (s, e) =>
            {
                if (WindowData != null && editor.Text != WindowData.SelectedConfContent)
                {
                    WindowData.SelectedConfContent = editor.Text;
                }
            };
        }

        private void TreeView_ConfSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is FileItem fileItem)
            {
                WindowData.SelectedConfItem = fileItem;
            }
        }

        private void CreateFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var confPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModConf");
                
                // Determine parent path
                string parentPath = confPath;
                if (WindowData.SelectedConfItem != null && WindowData.SelectedConfItem.IsFolder)
                {
                    parentPath = WindowData.SelectedConfItem.FullPath;
                }
                else if (WindowData.SelectedConfItem != null && !WindowData.SelectedConfItem.IsFolder)
                {
                    parentPath = Path.GetDirectoryName(WindowData.SelectedConfItem.FullPath);
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

                // Reload configuration tree
                WindowData.LoadConfFiles();

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

        private void DeleteFolder_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedConfItem == null || !WindowData.SelectedConfItem.IsFolder)
                return;

            try
            {
                var folderPath = WindowData.SelectedConfItem.FullPath;
                var folderName = WindowData.SelectedConfItem.Name;

                // Check if folder exists
                if (!Directory.Exists(folderPath))
                {
                    MessageBox.Show(
                        $"Folder '{folderName}' does not exist!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    WindowData.LoadConfFiles();
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
                    
                    // Reload configuration tree
                    WindowData.LoadConfFiles();

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
                WindowData.LoadConfFiles();
            }
        }

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
                        // Determine target directory based on selected item
                        var confPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModConf");
                        string targetPath = confPath;
                        
                        if (WindowData.SelectedConfItem != null && WindowData.SelectedConfItem.IsFolder)
                        {
                            targetPath = WindowData.SelectedConfItem.FullPath;
                        }
                        else if (WindowData.SelectedConfItem != null && !WindowData.SelectedConfItem.IsFolder)
                        {
                            targetPath = Path.GetDirectoryName(WindowData.SelectedConfItem.FullPath);
                        }
                        
                        Directory.CreateDirectory(targetPath);

                        var fileName = addConfWindow.WindowData.GetFileName();
                        var destPath = Path.Combine(targetPath, fileName);
                        File.Copy(selectedConfig.FilePath, destPath, true);

                        // Reload configuration list
                        WindowData.LoadConfFiles();

                        // Select the newly added file - calculate relative path
                        WindowData.SelectedConfFile = Path.GetRelativePath(confPath, destPath);
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

            try
            {
                var filePath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModConf", WindowData.SelectedConfFile);
                
                // Check if file exists before attempting to delete
                if (!File.Exists(filePath))
                {
                    MessageBox.Show(
                        $"File '{WindowData.SelectedConfFile}' does not exist!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    WindowData.LoadConfFiles();
                    return;
                }

                var result = MessageBox.Show(
                    $"Are you sure you want to delete '{WindowData.SelectedConfFile}'?",
                    "Delete Configuration",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    File.Delete(filePath);
                    // Reload configuration list
                    WindowData.LoadConfFiles();
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to delete configuration file");
                WindowData.LoadConfFiles();
            }
        }

        private void CloneConf_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(WindowData.SelectedConfFile))
                return;

            try
            {
                var confPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModConf");
                var sourceFile = Path.Combine(confPath, WindowData.SelectedConfFile);
                
                // Check if source file exists
                if (!File.Exists(sourceFile))
                {
                    MessageBox.Show(
                        $"Source file '{WindowData.SelectedConfFile}' does not exist!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    WindowData.LoadConfFiles();
                    return;
                }

                // Get directory where the file is located
                var sourceDir = Path.GetDirectoryName(sourceFile);

                // Generate new filename with _copy suffix
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(Path.GetFileName(WindowData.SelectedConfFile));
                var extension = Path.GetExtension(WindowData.SelectedConfFile);
                var newFileName = $"{fileNameWithoutExt}_copy{extension}";
                
                // If file already exists, add number suffix
                var counter = 1;
                while (File.Exists(Path.Combine(sourceDir, newFileName)))
                {
                    newFileName = $"{fileNameWithoutExt}_copy{counter}{extension}";
                    counter++;
                }

                var destFile = Path.Combine(sourceDir, newFileName);
                File.Copy(sourceFile, destFile);

                // Reload configuration list and select the cloned file
                WindowData.LoadConfFiles();
                WindowData.SelectedConfFile = Path.GetRelativePath(confPath, destFile);

                MessageBox.Show(
                    $"Configuration file cloned successfully!\n\nNew file: {newFileName}",
                    MessageHelper.Get("Messages.Success.Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to clone configuration file");
                WindowData.LoadConfFiles();
            }
        }

        private void RenameConf_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(WindowData.SelectedConfFile))
                return;

            try
            {
                var oldFileName = WindowData.SelectedConfFile;
                var confPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModConf");
                var oldFilePath = Path.Combine(confPath, oldFileName);

                // Check if source file exists
                if (!File.Exists(oldFilePath))
                {
                    MessageBox.Show(
                        $"Source file '{oldFileName}' does not exist!",
                        MessageHelper.Get("Messages.Error.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    WindowData.LoadConfFiles();
                    return;
                }

                // Show InputWindow to get new filename
                var inputWindow = new InputWindow
                {
                    Owner = this
                };
                inputWindow.WindowData.WindowTitle = "Rename Configuration File";
                inputWindow.WindowData.Label = "New file name:";
                inputWindow.WindowData.InputValue = oldFileName;

                if (inputWindow.ShowDialog() != true)
                    return;

                var newFileName = inputWindow.WindowData.InputValue;

                // Validate new filename
                if (string.IsNullOrWhiteSpace(newFileName))
                {
                    MessageBox.Show(
                        "File name cannot be empty!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Check if filename contains invalid characters
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

                // Ensure .json extension
                if (!newFileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    newFileName += ".json";
                }

                // If names are the same, no need to rename
                if (oldFileName == newFileName)
                {
                    return;
                }

                var newFilePath = Path.Combine(confPath, newFileName);

                // Check if new filename already exists
                if (File.Exists(newFilePath))
                {
                    MessageBox.Show(
                        $"A file with the name '{newFileName}' already exists!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Rename the file
                File.Move(oldFilePath, newFilePath);

                // Reload configuration list and select the renamed file
                WindowData.LoadConfFiles();
                WindowData.SelectedConfFile = newFileName;

                MessageBox.Show(
                    $"Configuration file renamed successfully!\n\nOld: {oldFileName}\nNew: {newFileName}",
                    MessageHelper.Get("Messages.Success.Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to rename configuration file");
                WindowData.LoadConfFiles();
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void ReplaceInEditor_Click(object sender, RoutedEventArgs e)
        {
            var replacePanel = this.FindName("replacePanel") as System.Windows.Controls.Border;
            if (replacePanel == null) return;

            try
            {
                // Toggle replace panel visibility
                if (replacePanel.Visibility == Visibility.Collapsed)
                {
                    replacePanel.Visibility = Visibility.Visible;
                    var txtFindText = this.FindName("txtFindText") as System.Windows.Controls.TextBox;
                    txtFindText?.Focus();
                }
                else
                {
                    replacePanel.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to toggle replace panel");
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void TxtFindText_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var txtFindText = sender as System.Windows.Controls.TextBox;
            var editor = this.FindName("txtJsonEditor") as ICSharpCode.AvalonEdit.TextEditor;
            
            if (editor == null || txtFindText == null || string.IsNullOrEmpty(txtFindText.Text))
                return;

            try
            {
                var searchText = txtFindText.Text;
                var text = editor.Text;

                // Always search from beginning when text changes
                var index = text.IndexOf(searchText, 0, StringComparison.OrdinalIgnoreCase);

                if (index >= 0)
                {
                    editor.Select(index, searchText.Length);
                    editor.CaretOffset = index + searchText.Length;
                    editor.ScrollToLine(editor.Document.GetLineByOffset(index).LineNumber);
                }
            }
            catch
            {
                // Silently ignore errors during text change
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void TxtFindText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                e.Handled = true;
                FindNext_Click(sender, e);
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void FindNext_Click(object sender, RoutedEventArgs e)
        {
            var editor = this.FindName("txtJsonEditor") as ICSharpCode.AvalonEdit.TextEditor;
            var txtFindText = this.FindName("txtFindText") as System.Windows.Controls.TextBox;
            
            if (editor == null || txtFindText == null || string.IsNullOrEmpty(txtFindText.Text))
                return;

            try
            {
                var searchText = txtFindText.Text;
                var text = editor.Text;
                var startIndex = editor.CaretOffset;

                var index = text.IndexOf(searchText, startIndex, StringComparison.OrdinalIgnoreCase);
                if (index == -1)
                {
                    // Search from beginning if not found after caret
                    index = text.IndexOf(searchText, 0, StringComparison.OrdinalIgnoreCase);
                }

                if (index >= 0)
                {
                    editor.Select(index, searchText.Length);
                    editor.CaretOffset = index + searchText.Length;
                    editor.ScrollToLine(editor.Document.GetLineByOffset(index).LineNumber);
                }
                else
                {
                    MessageBox.Show(
                        $"Cannot find '{searchText}'",
                        "Find",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to find text");
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void ReplaceOne_Click(object sender, RoutedEventArgs e)
        {
            var editor = this.FindName("txtJsonEditor") as ICSharpCode.AvalonEdit.TextEditor;
            var txtFindText = this.FindName("txtFindText") as System.Windows.Controls.TextBox;
            var txtReplaceText = this.FindName("txtReplaceText") as System.Windows.Controls.TextBox;
            
            if (editor == null || txtFindText == null || txtReplaceText == null || string.IsNullOrEmpty(txtFindText.Text))
                return;

            try
            {
                var searchText = txtFindText.Text;
                var replaceText = txtReplaceText.Text;

                // Check if current selection matches search text
                if (editor.SelectedText.Equals(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    var offset = editor.SelectionStart;
                    editor.Document.Replace(offset, editor.SelectionLength, replaceText);
                    editor.CaretOffset = offset + replaceText.Length;
                }

                // Find next occurrence
                FindNext_Click(sender, e);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to replace text");
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void ReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            var editor = this.FindName("txtJsonEditor") as ICSharpCode.AvalonEdit.TextEditor;
            var txtFindText = this.FindName("txtFindText") as System.Windows.Controls.TextBox;
            var txtReplaceText = this.FindName("txtReplaceText") as System.Windows.Controls.TextBox;
            
            if (editor == null || txtFindText == null || txtReplaceText == null || string.IsNullOrEmpty(txtFindText.Text))
                return;

            try
            {
                var searchText = txtFindText.Text;
                var replaceText = txtReplaceText.Text;
                var text = editor.Text;

                var count = 0;
                var index = 0;
                var offset = 0;

                while ((index = text.IndexOf(searchText, index, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    editor.Document.Replace(index + offset, searchText.Length, replaceText);
                    offset += replaceText.Length - searchText.Length;
                    index += searchText.Length;
                    count++;
                }

                MessageBox.Show(
                    $"Replaced {count} occurrence(s)",
                    "Replace All",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to replace all");
            }
        }

        private void CloseReplacePanel_Click(object sender, RoutedEventArgs e)
        {
            var replacePanel = this.FindName("replacePanel") as System.Windows.Controls.Border;
            if (replacePanel != null)
            {
                replacePanel.Visibility = Visibility.Collapsed;
            }
        }

        private void OpenInNotepad_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(WindowData.SelectedConfFile))
                return;

            try
            {
                var filePath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModConf", WindowData.SelectedConfFile);
                
                if (!File.Exists(filePath))
                {
                    MessageBox.Show(
                        $"File '{WindowData.SelectedConfFile}' does not exist!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Try common Notepad++ installation paths
                var notepadPlusPlusPaths = new[]
                {
                    @"C:\Program Files\Notepad++\notepad++.exe",
                    @"C:\Program Files (x86)\Notepad++\notepad++.exe",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Notepad++\notepad++.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Notepad++\notepad++.exe")
                };

                string notepadPlusPlusPath = null;
                foreach (var path in notepadPlusPlusPaths)
                {
                    if (File.Exists(path))
                    {
                        notepadPlusPlusPath = path;
                        break;
                    }
                }

                if (notepadPlusPlusPath != null)
                {
                    System.Diagnostics.Process.Start(notepadPlusPlusPath, $"\"{filePath}\"");
                }
                else
                {
                    var result = MessageBox.Show(
                        "Notepad++ not found. Would you like to open with Notepad instead?",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start("notepad.exe", filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to open in Notepad++");
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
    }
}
