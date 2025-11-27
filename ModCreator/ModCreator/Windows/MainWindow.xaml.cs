using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.WindowData;
using System;
using System.ComponentModel;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace ModCreator
{
    public partial class MainWindow : Windows.CWindow<MainWindowData>
    {
        public override MainWindowData InitData(CancelEventArgs e)
        {
            var data = new MainWindowData();
            
            // Load workplace path
            var savedPath = Properties.Settings.Default.WorkplacePath;
            if (string.IsNullOrWhiteSpace(savedPath))
            {
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                savedPath = System.IO.Path.Combine(documentsPath, Constants.DEFAULT_WORKPLACE_DIR);
                
                if (!System.IO.Directory.Exists(savedPath))
                {
                    System.IO.Directory.CreateDirectory(savedPath);
                }
                
                Properties.Settings.Default.WorkplacePath = savedPath;
                Properties.Settings.Default.Save();
            }
            
            data.WorkplacePath = savedPath;
            data.OnLoad();
            data.StatusMessage = MessageHelper.Get("Messages.Info.Ready");
            
            return data;
        }

        #region Event Handlers

        private void ProjectList_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Check if double-click is on a data row, not on header or empty space
            var row = e.OriginalSource as System.Windows.FrameworkElement;
            if (row == null) return;
            
            // Walk up the visual tree to find DataGridRow
            while (row != null && !(row is System.Windows.Controls.DataGridRow))
            {
                row = System.Windows.Media.VisualTreeHelper.GetParent(row) as System.Windows.FrameworkElement;
            }
            
            // If not clicked on a DataGridRow, ignore
            if (row == null) return;
            
            if (WindowData.SelectedProject == null) return;
            var project = WindowData.SelectedProject;
            if (!System.IO.Directory.Exists(project.ProjectPath))
            {
                DebugHelper.ShowWarning(
                    MessageHelper.GetFormat("Messages.Error.ErrorProjectFolderMissing", project.ProjectPath),
                    MessageHelper.Get("Messages.Error.Title"));
                return;
            }
            try
            {
                var editorWindow = new Windows.ProjectEditorWindow
                {
                    Owner = this,
                    ProjectToEdit = project
                };
                editorWindow.ProjectUpdated += (s, args) => WindowData.LoadProjects();
                editorWindow.Show();
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), 
                    MessageHelper.GetFormat("Messages.Error.ErrorUpdatingProject", ex.Message));
            }
        }

        private void CreateProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Show NewProjectWindow dialog
                var dialog = new Windows.NewProjectWindow
                {
                    Owner = this
                };
                dialog.WindowData.WorkplacePath = WindowData.WorkplacePath;
                
                if (dialog.ShowDialog() == true)
                {
                    var projectData = dialog.WindowData;
                    var newProject = projectData.CreatedProject;
                    if (newProject != null)
                    {
                        WindowData.AllProjects.Add(newProject);
                        ProjectHelper.SaveProjects(WindowData.AllProjects);
                        WindowData.UpdateFilteredProjects(WindowData, null, null, null);
                        MessageBox.Show(MessageHelper.GetFormat("Messages.Success.ProjectSuccessMessage", newProject.ProjectPath), 
                            MessageHelper.Get("Messages.Success.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
                        // Open editor window for new project
                        var editorWindow = new Windows.ProjectEditorWindow
                        {
                            Owner = this,
                            ProjectToEdit = newProject
                        };
                        editorWindow.ProjectUpdated += (s, args) => WindowData.LoadProjects();
                        editorWindow.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"),
                    MessageHelper.GetFormat("Messages.Error.ErrorCreatingProject", ex.Message));
            }
        }

        private void RefreshProjects_Click(object sender, RoutedEventArgs e)
        {
            WindowData.LoadProjects();
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (WindowData.SelectedProject != null && 
                    !System.IO.Directory.Exists(WindowData.SelectedProject.ProjectPath))
                {
                    MessageBox.Show(
                        MessageHelper.GetFormat("Messages.Error.ErrorProjectFolderMissing", WindowData.SelectedProject.ProjectPath),
                        MessageHelper.Get("Messages.Error.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
                WindowData.OpenProjectFolder();
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"),
                    MessageHelper.GetFormat("Messages.Error.ErrorOpeningFolder", ex.Message));
            }
        }

        private void EditProject_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedProject == null) return;
            
            if (!System.IO.Directory.Exists(WindowData.SelectedProject.ProjectPath))
            {
                MessageBox.Show(
                    MessageHelper.GetFormat("Messages.Error.ErrorProjectFolderMissing", WindowData.SelectedProject.ProjectPath),
                    MessageHelper.Get("Messages.Error.Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            
            try
            {
                var editorWindow = new Windows.ProjectEditorWindow
                {
                    Owner = this,
                    ProjectToEdit = WindowData.SelectedProject
                };
                editorWindow.Show();
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"),
                    MessageHelper.GetFormat("Messages.Error.ErrorUpdatingProject", ex.Message));
            }
        }

        private void DeleteProject_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedProject == null) return;
            
            var folderExists = System.IO.Directory.Exists(WindowData.SelectedProject.ProjectPath);
            var message = folderExists 
                ? MessageHelper.GetFormat("Messages.Confirmation.ProjectDeleteMessage", WindowData.SelectedProject.ProjectName)
                : MessageHelper.GetFormat("Messages.Confirmation.ProjectDeleteMessageNoFolder", WindowData.SelectedProject.ProjectName);
            
            var result = MessageBox.Show(
                message,
                MessageHelper.Get("Messages.Confirmation.ProjectDeleteTitle"),
                folderExists ? MessageBoxButton.YesNoCancel : MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Cancel)
                return;

            try
            {
                bool deleteFiles = folderExists && (result == MessageBoxResult.Yes);
                WindowData.DeleteProject(deleteFiles);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"),
                    MessageHelper.GetFormat("Messages.Error.ErrorDeletingProject", ex.Message));
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void BrowseWorkplace_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = MessageHelper.Get("Windows.MainWindow.SelectWorkplace");
                
                if (!string.IsNullOrEmpty(WindowData.WorkplacePath))
                {
                    dialog.SelectedPath = WindowData.WorkplacePath;
                }

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    WindowData.WorkplacePath = dialog.SelectedPath;
                }
            }
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var helpWindow = new Windows.HelperWindow
                {
                    Owner = this
                };
                helpWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), MessageHelper.Get("Messages.Error.ErrorOpeningHelpWindow"));
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var aboutWindow = new Windows.AboutWindow
                {
                    Owner = this
                };
                aboutWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), MessageHelper.Get("Messages.Error.ErrorOpeningAboutWindow"));
            }
        }

        #region Grid Action Handlers

        private void GridOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            var project = button?.DataContext as ModProject;
            if (project != null)
            {
                WindowData.SelectedProject = project;
                OpenFolder_Click(sender, e);
            }
        }

        private void GridEditProject_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            var project = button?.DataContext as ModProject;
            if (project != null)
            {
                WindowData.SelectedProject = project;
                EditProject_Click(sender, e);
            }
        }

        private void GridDeleteProject_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            var project = button?.DataContext as ModProject;
            if (project != null)
            {
                WindowData.SelectedProject = project;
                DeleteProject_Click(sender, e);
            }
        }

        #endregion

        #endregion
    }
}
