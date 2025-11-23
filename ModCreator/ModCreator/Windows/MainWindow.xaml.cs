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
            data.StatusMessage = MessageHelper.Ready;
            
            return data;
        }

        #region Event Handlers

        private void ProjectList_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (WindowData.SelectedProject == null) return;
            var project = WindowData.SelectedProject;
            if (!System.IO.Directory.Exists(project.ProjectPath))
            {
                MessageBox.Show(MessageHelper.GetFormat("ErrorProjectFolderMissing", project.ProjectPath),
                    MessageHelper.Error, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                var editorWindow = new Windows.ProjectEditorWindow
                {
                    Owner = this,
                    ProjectToEdit = project
                };
                if (editorWindow.ShowDialog() == true)
                {
                    WindowData.UpdateFilteredProjects();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(MessageHelper.GetFormat("ErrorUpdatingProject", ex.Message),
                    MessageHelper.Error, MessageBoxButton.OK, MessageBoxImage.Error);
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
                        ModCreator.Helpers.ProjectHelper.SaveProjects(WindowData.AllProjects);
                        WindowData.UpdateFilteredProjects();
                        MessageBox.Show(MessageHelper.GetFormat("ProjectSuccessMessage", newProject.ProjectPath), 
                            MessageHelper.Success, MessageBoxButton.OK, MessageBoxImage.Information);
                        // Open editor window for new project
                        var editorWindow = new Windows.ProjectEditorWindow
                        {
                            Owner = this,
                            ProjectToEdit = newProject
                        };
                        editorWindow.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(MessageHelper.GetFormat("ErrorCreatingProject", ex.Message + "\n\n" + ex.StackTrace), 
                    MessageHelper.Error, MessageBoxButton.OK, MessageBoxImage.Error);
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
                WindowData.OpenProjectFolder();
            }
            catch (Exception ex)
            {
                MessageBox.Show(MessageHelper.GetFormat("ErrorOpeningFolder", ex.Message), 
                    MessageHelper.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditProject_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedProject == null) return;
            
            try
            {
                var editorWindow = new Windows.ProjectEditorWindow
                {
                    Owner = this,
                    ProjectToEdit = WindowData.SelectedProject
                };
                
                if (editorWindow.ShowDialog() == true)
                {
                    WindowData.UpdateFilteredProjects();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(MessageHelper.GetFormat("ErrorUpdatingProject", ex.Message), 
                    MessageHelper.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteProject_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedProject == null) return;
            
            var result = MessageBox.Show(
                MessageHelper.GetFormat("ProjectDeleteMessage", WindowData.SelectedProject.ProjectName),
                MessageHelper.ProjectDeleteTitle,
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Cancel)
                return;

            try
            {
                bool deleteFiles = result == MessageBoxResult.Yes;
                WindowData.DeleteProject(deleteFiles);
            }
            catch (Exception ex)
            {
                MessageBox.Show(MessageHelper.GetFormat("ErrorDeletingProject", ex.Message), 
                    MessageHelper.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void BrowseWorkplace_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = MessageHelper.SelectWorkplace;
                
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
                System.Diagnostics.Debug.WriteLine("[MainWindow] Help_Click called");
                var helpWindow = new Windows.HelperWindow
                {
                    Owner = this
                };
                System.Diagnostics.Debug.WriteLine("[MainWindow] HelperWindow created, calling ShowDialog");
                helpWindow.ShowDialog();
                System.Diagnostics.Debug.WriteLine("[MainWindow] ShowDialog returned");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainWindow] ERROR in Help_Click: {ex.Message}");
                MessageBox.Show(
                    $"Error opening Help window:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    MessageHelper.Error,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[MainWindow] About_Click called");
                var aboutWindow = new Windows.AboutWindow
                {
                    Owner = this
                };
                System.Diagnostics.Debug.WriteLine("[MainWindow] AboutWindow created, calling ShowDialog");
                aboutWindow.ShowDialog();
                System.Diagnostics.Debug.WriteLine("[MainWindow] ShowDialog returned");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainWindow] ERROR in About_Click: {ex.Message}");
                MessageBox.Show(
                    $"Error opening About window:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    MessageHelper.Error,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
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
