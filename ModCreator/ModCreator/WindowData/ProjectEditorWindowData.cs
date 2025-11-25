using ModCreator.Attributes;
using ModCreator.Commons;
using ModCreator.Enums;
using ModCreator.Helpers;
using ModCreator.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Windows.Media.Imaging;

namespace ModCreator.WindowData
{
    /// <summary>
    /// Project editor window data layer
    /// </summary>
    public class ProjectEditorWindowData : CWindowData
    {
        #region Properties

        /// <summary>
        /// Project being edited
        /// </summary>
        [NotifyMethod(nameof(LoadProjectData))]
        public ModProject Project { get; set; }

        #region Tab 2: ModConf

        /// <summary>
        /// List of configuration files in ModConf directory
        /// </summary>
        public List<string> ConfFiles { get; set; } = new List<string>();

        /// <summary>
        /// Selected configuration file
        /// </summary>
        [NotifyMethod(nameof(LoadConfContent))]
        public string SelectedConfFile { get; set; }

        /// <summary>
        /// Content of selected configuration file
        /// </summary>
        public string SelectedConfContent { get; set; }

        /// <summary>
        /// Check if a conf file is selected
        /// </summary>
        public bool HasSelectedConfFile => !string.IsNullOrEmpty(SelectedConfFile);

        #endregion

        #region Tab 3: ModImg

        /// <summary>
        /// List of image files in ModImg directory
        /// </summary>
        public List<string> ImageFiles { get; set; } = new List<string>();

        /// <summary>
        /// Supported image extensions loaded from image-extensions.json
        /// </summary>
        public List<ImageExtension> ImageExtensions { get; set; } = Helpers.MasterDataHelper.LoadImageExtensions();

        /// <summary>
        /// Selected image file
        /// </summary>
        [NotifyMethod(nameof(OnSelectedImageChanged))]
        public string SelectedImageFile { get; set; }

        /// <summary>
        /// BitmapImage for selected image (loaded without file locking)
        /// </summary>
        public BitmapImage SelectedImagePath
        {
            get
            {
                if (string.IsNullOrEmpty(SelectedImageFile) || Project == null)
                    return null;

                var filePath = Path.Combine(Project.ProjectPath, "ModProject", "ModImg", SelectedImageFile);
                if (!File.Exists(filePath))
                    return null;

                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // Load into memory, release file handle
                    bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
                    bitmap.EndInit();
                    bitmap.Freeze(); // Make it cross-thread accessible and ensure file is released
                    return bitmap;
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Check if an image file is selected
        /// </summary>
        public bool HasSelectedImageFile => !string.IsNullOrEmpty(SelectedImageFile);

        #endregion

        #region Tab 4: Global Variables

        public ObservableCollection<GlobalVariable> GlobalVariables { get; set; } = new ObservableCollection<GlobalVariable>();

        /// <summary>
        /// Available variable types loaded from var-types.json
        /// </summary>
        public List<VarType> VarTypes { get; set; } = Helpers.MasterDataHelper.LoadVarTypes();

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
            return !ObjectHelper.ArePropertiesEqual(Project, _originalProject);
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
            ObjectHelper.CopyProperties(_originalProject, Project);

            // Reload UI from restored data
            LoadGlobalVariables();
        }

        #endregion

        #endregion

        #region Business Logic Methods

        /// <summary>
        /// Load all project data when project is set
        /// </summary>
        public void LoadProjectData(object obj, PropertyInfo prop, object oldValue, object newValue)
        {
            if (Project == null) return;

            LoadConfFiles();
            LoadImageFiles();
            LoadGlobalVariables();
            
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
        /// Load configuration files from ModConf directory
        /// </summary>
        public void LoadConfFiles()
        {
            ConfFiles.Clear();
            if (Project == null) return;

            var confDir = Path.Combine(Project.ProjectPath, "ModProject", "ModConf");
            if (Directory.Exists(confDir))
            {
                ConfFiles = Directory.GetFiles(confDir, "*.json").ToList();
                
                // Clear selection if the selected file no longer exists
                if (!string.IsNullOrEmpty(SelectedConfFile) && !ConfFiles.Contains(SelectedConfFile))
                {
                    SelectedConfFile = null;
                }
            }
            else
            {
                // Clear selection if directory doesn't exist
                SelectedConfFile = null;
            }
        }

        /// <summary>
        /// Load content of selected configuration file
        /// </summary>
        public void LoadConfContent(object obj, PropertyInfo prop, object oldValue, object newValue)
        {
            if (string.IsNullOrEmpty(SelectedConfFile) || Project == null)
            {
                SelectedConfContent = string.Empty;
                return;
            }

            var filePath = Path.Combine(Project.ProjectPath, "ModProject", "ModConf", SelectedConfFile);
            if (File.Exists(filePath))
            {
                SelectedConfContent = File.ReadAllText(filePath);
            }
            else
            {
                SelectedConfContent = string.Empty;
            }
        }

        /// <summary>
        /// Save content of selected configuration file
        /// </summary>
        public void SaveConfContent()
        {
            if (string.IsNullOrEmpty(SelectedConfFile) || Project == null || string.IsNullOrEmpty(SelectedConfContent))
                return;

            var filePath = Path.Combine(Project.ProjectPath, "ModProject", "ModConf", SelectedConfFile);
            File.WriteAllText(filePath, SelectedConfContent);
        }

        /// <summary>
        /// Called when SelectedImageFile changes to trigger SelectedImagePath update
        /// </summary>
        public void OnSelectedImageChanged(object obj, PropertyInfo prop, object oldValue, object newValue)
        {
            // Trigger SelectedImagePath property change notification
            // This causes the Image control to reload with the new bitmap
        }

        /// <summary>
        /// Load image files from ModImg directory
        /// </summary>
        public void LoadImageFiles()
        {
            ImageFiles.Clear();
            if (Project == null) return;

            var imgDir = Path.Combine(Project.ProjectPath, "ModProject", "ModImg");
            if (Directory.Exists(imgDir))
            {
                if (ImageExtensions.Count == 0)
                {
                    throw new InvalidOperationException("ImageExtensions not loaded. Cannot load image files.");
                }

                // Load files for each supported extension separately to avoid loading all files
                ImageFiles = Directory.EnumerateFiles(imgDir, "*", SearchOption.TopDirectoryOnly)
                    .Where(f => ImageExtensions.Any(ext => ext.Extension == Path.GetExtension(f).ToLower())).ToList();

                // Clear selection if the selected file no longer exists
                if (!string.IsNullOrEmpty(SelectedImageFile) && !ImageFiles.Contains(SelectedImageFile))
                {
                    SelectedImageFile = null;
                }
            }
            else
            {
                // Clear selection if directory doesn't exist
                SelectedConfFile = null;
            }
        }

        public void LoadGlobalVariables()
        {
            GlobalVariables = new ObservableCollection<GlobalVariable>(Project.GlobalVariables);
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

        /// <summary>
        /// Save global variables to file
        /// </summary>
        public void SaveGlobalVariables()
        {
            if (Project == null) return;

            // Sync GlobalVariables ObservableCollection back to Project.GlobalVariables
            Project.GlobalVariables = new List<GlobalVariable>(GlobalVariables);
        }

        #endregion
    }
}
