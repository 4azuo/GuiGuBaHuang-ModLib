using ModCreator.Businesses;
using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.WindowData;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace ModCreator.Windows
{
    public partial class ProjectEditorWindow : CWindow<ProjectEditorWindowData>
    {
        // Business layer instances
        private ModConfBusiness _modConfBusiness;
        private ModImgBusiness _modImgBusiness;
        private GlobalVariablesBusiness _globalVariablesBusiness;
        private ModEventBusiness _modEventBusiness;

        /// <summary>
        /// Project to edit - set before showing dialog
        /// </summary>
        public ModProject ProjectToEdit { get; set; }

        /// <summary>
        /// Event raised when the window is closed to notify parent window to refresh
        /// </summary>
        public event EventHandler ProjectUpdated;

        [SupportedOSPlatform("windows6.1")]
        public override ProjectEditorWindowData InitData(CancelEventArgs e)
        {
            var data = new ProjectEditorWindowData();
            data.New();
            
            Loaded += ProjectEditorWindow_Loaded;
            
            return data;
        }

        [SupportedOSPlatform("windows6.1")]
        private void ProjectEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Now ProjectToEdit has been set by the caller
            if (ProjectToEdit != null && WindowData != null)
            {
                WindowData.Project = ProjectToEdit;

                // Initialize business layer
                _modConfBusiness = new ModConfBusiness(WindowData, this);
                _modImgBusiness = new ModImgBusiness(WindowData, this);
                _globalVariablesBusiness = new GlobalVariablesBusiness(WindowData, this);
                _modEventBusiness = new ModEventBusiness(WindowData, this);

                // Setup AvalonEdit binding
                SetupAvalonEditBinding();
                
                // Setup Variables Source Editor binding
                SetupVariablesSourceBinding();
                
                // Setup Title Image
                SetupTitleImage();
                
                // Populate Events ComboBox
                PopulateEventsComboBox();
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
                    case 4: // Tab 5: ModEvent
                        WindowData.LoadModEventFiles();
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
    }
}
