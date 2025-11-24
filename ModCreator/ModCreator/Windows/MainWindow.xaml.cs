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
            data.StatusMessage = MessageHelper.Get("Ready");
            
            return data;
        }

        #region Event Handlers

        private void ProjectList_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (WindowData.SelectedProject == null) return;
            var project = WindowData.SelectedProject;
            if (!System.IO.Directory.Exists(project.ProjectPath))
            {
                DebugHelper.ShowWarning(
                    MessageHelper.GetFormat("ErrorProjectFolderMissing", project.ProjectPath),
                    MessageHelper.Get("Error"));
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
                DebugHelper.ShowError(ex, MessageHelper.Get("Error"), 
                    MessageHelper.GetFormat("ErrorUpdatingProject", ex.Message));
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
                            MessageHelper.Get("Success"), MessageBoxButton.OK, MessageBoxImage.Information);
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
                DebugHelper.ShowError(ex, MessageHelper.Get("Error"),
                    MessageHelper.GetFormat("ErrorCreatingProject", ex.Message));
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
                DebugHelper.ShowError(ex, MessageHelper.Get("Error"),
                    MessageHelper.GetFormat("ErrorOpeningFolder", ex.Message));
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
                DebugHelper.ShowError(ex, MessageHelper.Get("Error"),
                    MessageHelper.GetFormat("ErrorUpdatingProject", ex.Message));
            }
        }

        private void DeleteProject_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedProject == null) return;
            
            var result = MessageBox.Show(
                MessageHelper.GetFormat("ProjectDeleteMessage", WindowData.SelectedProject.ProjectName),
                MessageHelper.Get("ProjectDeleteTitle"),
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
                DebugHelper.ShowError(ex, MessageHelper.Get("Error"),
                    MessageHelper.GetFormat("ErrorDeletingProject", ex.Message));
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void BrowseWorkplace_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = MessageHelper.Get("SelectWorkplace");
                
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
                DebugHelper.ShowError(ex, MessageHelper.Get("Error"), MessageHelper.Get("ErrorOpeningHelpWindow"));
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
                DebugHelper.ShowError(ex, MessageHelper.Get("Error"), MessageHelper.Get("ErrorOpeningAboutWindow"));
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
