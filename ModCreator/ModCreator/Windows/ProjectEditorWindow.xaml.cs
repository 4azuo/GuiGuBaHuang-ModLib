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
using MessageBox = System.Windows.MessageBox;

namespace ModCreator.Windows
{
    public partial class ProjectEditorWindow : CWindow<ProjectEditorWindowData>
    {
        /// <summary>
        /// Project to edit - set before showing dialog
        /// </summary>
        public ModProject ProjectToEdit { get; set; }

        /// <summary>
        /// Event raised when the window is closed to notify parent window to refresh
        /// </summary>
        public event EventHandler ProjectUpdated;

        [SupportedOSPlatform("windows6.1")]
        private void ProjectEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Now ProjectToEdit has been set by the caller
            if (ProjectToEdit != null && WindowData != null)
            {
                WindowData.Project = ProjectToEdit;
                
                // Setup AvalonEdit binding
                SetupAvalonEditBinding();
                
                // Setup Variables Source Editor binding
                SetupVariablesSourceBinding();
            }

            // Subscribe to Closed event to notify parent window
            Closed += ProjectEditorWindow_Closed;
        }

        private void ProjectEditorWindow_Closed(object sender, EventArgs e)
        {
            // Notify parent window to refresh project list
            ProjectUpdated?.Invoke(this, EventArgs.Empty);
        }

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

        [SupportedOSPlatform("windows6.1")]
        private void SetupVariablesSourceBinding()
        {
            var editor = this.FindName("txtVariablesSource") as ICSharpCode.AvalonEdit.TextEditor;
            if (editor == null) return;

            // Load C# syntax highlighting
            AvalonHelper.LoadCSharpSyntaxHighlighting(editor);
            
            // Load content from ModCreatorChildVars.cs if exists
            LoadVariablesSourceFile();
        }

        [SupportedOSPlatform("windows6.1")]
        private void LoadVariablesSourceFile()
        {
            try
            {
                if (WindowData?.Project == null) return;
                
                var editor = this.FindName("txtVariablesSource") as ICSharpCode.AvalonEdit.TextEditor;
                if (editor == null) return;

                var filePath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModCode", "ModMain", "Const", "ModCreatorChildVars.cs");
                
                if (File.Exists(filePath))
                {
                    editor.Text = File.ReadAllText(filePath);
                }
                else
                {
                    editor.Text = "// File not found. Generate code first using 'Generate Code' button.";
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to load variables source file");
            }
        }

        [SupportedOSPlatform("windows6.1")]
        public override ProjectEditorWindowData InitData(CancelEventArgs e)
        {
            var data = new ProjectEditorWindowData();
            data.New();
            
            Loaded += ProjectEditorWindow_Loaded;
            
            return data;
        }

        [SupportedOSPlatform("windows6.1")]
        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Handle Ctrl+F to open replace panel
            if (e.Key == System.Windows.Input.Key.F && 
                (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
            {
                if (WindowData?.HasSelectedConfFile == true)
                {
                    e.Handled = true;
                    ReplaceInEditor_Click(sender, e);
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Confirmation dialog
                var result = MessageBox.Show(
                    "Are you sure you want to save all changes?",
                    "Confirm Save",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                WindowData.SaveProject();
                MessageBox.Show(
                    MessageHelper.GetFormat("Messages.Success.UpdatedProject", WindowData.Project.ProjectName), 
                    MessageHelper.Get("Messages.Success.Title"), 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
                
                // Update backup after successful save
                WindowData.BackupProject();
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"),
                    MessageHelper.GetFormat("Messages.Error.ErrorUpdatingProject", ex.Message));
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            // Check if there are unsaved changes
            if (WindowData.HasUnsavedChanges())
            {
                var result = MessageBox.Show(
                    "You have unsaved changes. Do you want to discard them?",
                    "Confirm Cancel",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;

                // Restore from backup
                WindowData.RestoreProject();
            }

            Close();
        }

        #region Tab 2: ModConf Event Handlers

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

        #endregion

        #region Tab 2: ModConf - Editor Actions

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

        #endregion

        #region Tab 3: ModImg Event Handlers

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

        #endregion

        #region Tab 2: ModConf Event Handlers

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

        #endregion

        #region Tab 1: Project Info Event Handlers

        [SupportedOSPlatform("windows6.1")]
        private void TitleImage_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                // Open file dialog to select new image
                using (var openFileDialog = new OpenFileDialog())
                {
                    var extensionPatterns = string.Join(";", WindowData.ImageExtensions.Select(ext => $"*{ext.Extension}"));
                    openFileDialog.Filter = $"Image Files|{extensionPatterns}";
                    openFileDialog.Title = "Select Title Image";
                    openFileDialog.RestoreDirectory = true;

                    if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        var selectedImagePath = openFileDialog.FileName;
                        if (!File.Exists(selectedImagePath))
                        {
                            MessageBox.Show(
                                "Selected file does not exist!",
                                MessageHelper.Get("Messages.Error.Title"),
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            return;
                        }

                        // Update project's TitleImg path
                        var targetPath = Path.GetFullPath(Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModProjectPreview.png"));
                        
                        // Copy selected image to target location, overwriting existing file
                        File.Copy(selectedImagePath, targetPath, true);

                        // Update the project's TitleImg property
                        WindowData.Project.TitleImg = targetPath;

                        // Notify UI to reload the image
                        WindowData.RefreshTitleImage();

                        MessageBox.Show(
                            "Title image updated successfully!",
                            MessageHelper.Get("Messages.Success.Title"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to update title image");
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

        [SupportedOSPlatform("windows6.1")]
        private void RefreshTab_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the currently selected tab to determine what to refresh
                var tabControl = this.FindName("tabControl") as System.Windows.Controls.TabControl;
                if (tabControl == null || WindowData == null)
                {
                    WindowData?.ReloadProjectData();
                    return;
                }

                var selectedIndex = tabControl.SelectedIndex;
                switch (selectedIndex)
                {
                    case 0: // Tab 1: Project Info
                        // No reload needed - project info is bound directly
                        break;
                    case 1: // Tab 2: ModConf
                        WindowData.LoadConfFiles();
                        break;
                    case 2: // Tab 3: ModImg
                        WindowData.LoadImageFiles();
                        break;
                    case 3: // Tab 4: Global Variables
                        WindowData.LoadGlobalVariables();
                        break;
                    default:
                        // For any other tabs, reload all data
                        WindowData.ReloadProjectData();
                        break;
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to refresh");
            }
        }

        #endregion

        #region Tab 4: Global Variables Event Handlers

        private void AddVariable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if there's already a variable with empty name
                if (WindowData.GlobalVariables.Any(v => string.IsNullOrWhiteSpace(v.Name)))
                {
                    MessageBox.Show(
                        "Please complete the existing variable with empty name before adding a new one!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Add new empty variable
                var newVar = new GlobalVariable
                {
                    Name = "",
                    Type = "dynamic",
                    Value = "",
                    Description = ""
                };
                WindowData.GlobalVariables.Add(newVar);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to add variable");
            }
        }

        private void DataGrid_RowEditEnding(object sender, System.Windows.Controls.DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == System.Windows.Controls.DataGridEditAction.Commit)
            {
                var variable = e.Row.Item as GlobalVariable;
                if (variable != null && string.IsNullOrWhiteSpace(variable.Name))
                {
                    // Prevent committing if Name is empty
                    e.Cancel = true;
                    MessageBox.Show(
                        "Variable name cannot be empty!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    
                    // If it's a new row with all empty fields, remove it
                    if (string.IsNullOrWhiteSpace(variable.Type) && 
                        string.IsNullOrWhiteSpace(variable.Value) && 
                        string.IsNullOrWhiteSpace(variable.Description))
                    {
                        WindowData.GlobalVariables.Remove(variable);
                    }
                }
            }
        }

        private void GenerateVariablesCode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (WindowData.Project == null)
                {
                    MessageBox.Show("No project loaded!", 
                        MessageHelper.Get("Messages.Error.Title"), 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                    return;
                }

                // Save variables first
                WindowData.SaveGlobalVariables();

                // Check if there are any variables to generate
                if (WindowData.GlobalVariables == null || WindowData.GlobalVariables.Count == 0)
                {
                    MessageBox.Show("No variables to generate. Please add variables first.", 
                        MessageHelper.Get("Messages.Warning.Title"), 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                    return;
                }

                // Validate that all variables have names
                var emptyVars = WindowData.GlobalVariables.Where(v => string.IsNullOrWhiteSpace(v.Name)).ToList();
                if (emptyVars.Any())
                {
                    MessageBox.Show("All variables must have names. Please complete all variables before generating code.", 
                        MessageHelper.Get("Messages.Warning.Title"), 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                    return;
                }

                // Read template files from project path
                var varTemplatePath = Path.Combine(WindowData.Project.ProjectPath, "VarTemplate.tmp");
                var varTemplateContentPath = Path.Combine(WindowData.Project.ProjectPath, "VarTemplateContent.tmp");

                if (!File.Exists(varTemplatePath))
                {
                    MessageBox.Show($"Template file not found: {varTemplatePath}\n\nPlease ensure VarTemplate.tmp exists in the project root.", 
                        MessageHelper.Get("Messages.Error.Title"), 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                    return;
                }

                if (!File.Exists(varTemplateContentPath))
                {
                    MessageBox.Show($"Template file not found: {varTemplateContentPath}\n\nPlease ensure VarTemplateContent.tmp exists in the project root.", 
                        MessageHelper.Get("Messages.Error.Title"), 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                    return;
                }

                // Read templates
                var varTemplate = File.ReadAllText(varTemplatePath);
                var varTemplateContent = File.ReadAllText(varTemplateContentPath);

                // Generate variable properties
                var variableProperties = new System.Text.StringBuilder();
                foreach (var variable in WindowData.GlobalVariables)
                {
                    if (string.IsNullOrWhiteSpace(variable.Name))
                        continue;

                    var propertyCode = varTemplateContent
                        .Replace("#VARTYPE#", variable.Type ?? "string")
                        .Replace("#VARNAME#", variable.Name)
                        .Replace("#VARVALUE#", FormatVariableValue(variable));

                    // Add description as comment if available
                    if (!string.IsNullOrWhiteSpace(variable.Description))
                    {
                        variableProperties.AppendLine($"        // {variable.Description}");
                    }

                    variableProperties.AppendLine($"        {propertyCode.Trim()}");
                }

                // Replace placeholder in main template
                var generatedCode = varTemplate.Replace("#VARIABLES#", variableProperties.ToString());

                // Save to file
                var outputPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModCode", "ModMain", "Const", "ModCreatorChildVars.cs");
                var outputDir = Path.GetDirectoryName(outputPath);
                
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                File.WriteAllText(outputPath, generatedCode);

                MessageBox.Show($"Variables code generated successfully!\n\nOutput file: {outputPath}", 
                    MessageHelper.Get("Messages.Info.Title"), 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), 
                    "Failed to generate variables code");
            }
        }

        /// <summary>
        /// Format variable value for code generation
        /// </summary>
        private string FormatVariableValue(GlobalVariable variable)
        {
            var varType = variable.Type?.ToLower();
            
            // Default values when Value is empty
            var defaultValues = new Dictionary<string, string>
            {
                ["string"] = "\"\"",
                ["bool"] = "false",
                ["int"] = "0",
                ["long"] = "0L",
                ["float"] = "0f",
                ["double"] = "0.0"
            };

            if (string.IsNullOrWhiteSpace(variable.Value))
            {
                if (defaultValues.TryGetValue(varType ?? "", out var defaultValue))
                    return defaultValue;
                return "null"; // Arrays and unknown types default to null
            }

            var value = variable.Value.Trim();
            
            // Format value based on type
            return varType switch
            {
                "string" => value.StartsWith("\"") && value.EndsWith("\"") ? value : $"\"{value}\"",
                "bool" => value.ToLower() is "true" or "false" ? value.ToLower() : "false",
                "float" => value.EndsWith("f") || value.EndsWith("F") ? value : $"{value}f",
                "long" => value.EndsWith("L") || value.EndsWith("l") ? value : $"{value}L",
                _ => value // Arrays, dynamic and other types return as-is
            };
        }

        private void CloneVariable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as System.Windows.Controls.Button;
                var variable = button?.Tag as GlobalVariable;
                
                if (variable == null) return;

                // Clone the variable
                var clonedVar = new GlobalVariable
                {
                    Name = variable.Name + "_copy",
                    Type = variable.Type,
                    Value = variable.Value,
                    Description = variable.Description
                };

                // Find index of original variable and insert after it
                var index = WindowData.GlobalVariables.IndexOf(variable);
                if (index >= 0)
                {
                    WindowData.GlobalVariables.Insert(index + 1, clonedVar);
                }
                else
                {
                    WindowData.GlobalVariables.Add(clonedVar);
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to clone variable");
            }
        }

        private void RemoveVariable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as System.Windows.Controls.Button;
                var variable = button?.Tag as GlobalVariable;
                
                if (variable == null) return;

                var result = MessageBox.Show(
                    $"Are you sure you want to remove variable '{variable.Name}'?",
                    "Confirm Remove",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    WindowData.GlobalVariables.Remove(variable);
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to remove variable");
            }
        }

        [SupportedOSPlatform("windows7.0")]
        private void ToggleGridView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dgVariables = this.FindName("dgGlobalVariables") as System.Windows.Controls.DataGrid;
                var txtSource = this.FindName("txtVariablesSource") as ICSharpCode.AvalonEdit.TextEditor;
                var btnGrid = this.FindName("btnGridView") as System.Windows.Controls.Button;
                var btnSource = this.FindName("btnSourceView") as System.Windows.Controls.Button;

                if (dgVariables != null && txtSource != null && btnGrid != null && btnSource != null)
                {
                    dgVariables.Visibility = Visibility.Visible;
                    txtSource.Visibility = Visibility.Collapsed;
                    
                    // Update button styles
                    btnGrid.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF2E5090"));
                    btnGrid.Foreground = System.Windows.Media.Brushes.White;
                    btnSource.Background = System.Windows.Media.Brushes.White;
                    btnSource.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF666666"));
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to switch to grid view");
            }
        }

        [SupportedOSPlatform("windows7.0")]
        private void ToggleSourceView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dgVariables = this.FindName("dgGlobalVariables") as System.Windows.Controls.DataGrid;
                var txtSource = this.FindName("txtVariablesSource") as ICSharpCode.AvalonEdit.TextEditor;
                var btnGrid = this.FindName("btnGridView") as System.Windows.Controls.Button;
                var btnSource = this.FindName("btnSourceView") as System.Windows.Controls.Button;

                if (dgVariables != null && txtSource != null && btnGrid != null && btnSource != null)
                {
                    // Load/Refresh C# source file
                    LoadVariablesSourceFile();

                    dgVariables.Visibility = Visibility.Collapsed;
                    txtSource.Visibility = Visibility.Visible;
                    
                    // Update button styles
                    btnSource.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF2E5090"));
                    btnSource.Foreground = System.Windows.Media.Brushes.White;
                    btnGrid.Background = System.Windows.Media.Brushes.White;
                    btnGrid.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF666666"));
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to switch to source view");
            }
        }

        #endregion
    }
}
