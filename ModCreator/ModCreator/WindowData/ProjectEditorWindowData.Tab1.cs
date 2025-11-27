using ModCreator.Attributes;
using ModCreator.Models;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Reflection;

namespace ModCreator.WindowData
{
    /// <summary>
    /// Project editor window data layer - Tab 1 (Project Info)
    /// </summary>
    public partial class ProjectEditorWindowData : CWindowData
    {
        #region Tab 1 Properties

        /// <summary>
        /// Project being edited
        /// </summary>
        [NotifyMethod(nameof(LoadProjectData))]
        public ModProject Project { get; set; }

        #endregion

        #region Backup/Restore

        /// <summary>
        /// Backup of original project state for Cancel operation
        /// </summary>
        internal ModProject _originalProject;

        /// <summary>
        /// Check if there are unsaved changes
        /// </summary>
        public bool HasUnsavedChanges()
        {
            if (Project == null || _originalProject == null) return false;
            return !Helpers.ObjectHelper.ArePropertiesEqual(Project, _originalProject);
        }

        /// <summary>
        /// Create backup of current project state
        /// </summary>
        public void BackupProject()
        {
            if (Project == null) return;

            // Deep clone using Newtonsoft.Json serialization
            var json = JsonConvert.SerializeObject(Project);
            _originalProject = JsonConvert.DeserializeObject<ModProject>(json);
        }

        /// <summary>
        /// Restore project from backup
        /// </summary>
        public void RestoreProject()
        {
            if (_originalProject == null || Project == null) return;

            // Deep clone the backup back to Project
            Helpers.ObjectHelper.CopyProperties(_originalProject, Project);

            // Reload UI from restored data
            LoadGlobalVariables();
        }

        #endregion

        #region Tab 1 Methods

        /// <summary>
        /// Load all project data when project is set
        /// </summary>
        public void LoadProjectData(object obj, PropertyInfo prop, object oldValue, object newValue)
        {
            if (Project == null) return;

            LoadConfFiles();
            LoadImageFiles();
            LoadGlobalVariables();
            LoadModEventFiles();
            LoadModEventResources();
            
            // Create backup for Cancel operation
            BackupProject();
        }

        /// <summary>
        /// Reload all project data (public method for external calls)
        /// </summary>
        public void ReloadProjectData()
        {
            LoadProjectData(this, null, null, null);
        }

        /// <summary>
        /// Save project changes
        /// </summary>
        public void SaveProject()
        {
            if (Project == null) return;

            // Save conf content if any is being edited
            if (HasSelectedConfFile && !string.IsNullOrEmpty(SelectedConfContent))
            {
                SaveConfContent();
            }

            // Save global variables
            SaveGlobalVariables();

            // Update project metadata
            Project.LastModifiedDate = System.DateTime.Now;
            Helpers.ProjectHelper.SaveProjects(Helpers.ProjectHelper.LoadProjects());
        }

        #endregion
    }
}
