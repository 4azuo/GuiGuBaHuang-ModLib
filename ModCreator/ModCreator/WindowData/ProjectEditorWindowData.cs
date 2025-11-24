using ModCreator.Commons;
using ModCreator.Enums;
using ModCreator.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModCreator.WindowData
{
    /// <summary>
    /// Project editor window data layer
    /// </summary>
    public class ProjectEditorWindowData : CWindowData
    {
        #region Private Fields
        private ModProject _project;
        private string _selectedConfFile;
        private string _selectedConfContent;
        private string _selectedImageFile;
        #endregion

        #region Properties

        /// <summary>
        /// Project being edited
        /// </summary>
        public ModProject Project
        {
            get => _project;
            set
            {
                _project = value;
                LoadProjectData();
            }
        }

        /// <summary>
        /// Project name (editable)
        /// </summary>
        public string ProjectName
        {
            get => Project?.ProjectName;
            set
            {
                if (Project != null && Project.ProjectName != value)
                {
                    Project.ProjectName = value;
                }
            }
        }

        /// <summary>
        /// Project description (editable)
        /// </summary>
        public string ProjectDescription
        {
            get => Project?.Description;
            set
            {
                if (Project != null && Project.Description != value)
                {
                    Project.Description = value;
                }
            }
        }

        #region Tab 2: ModConf

        /// <summary>
        /// List of configuration files in ModConf directory
        /// </summary>
        public List<string> ConfFiles { get; set; } = new List<string>();

        /// <summary>
        /// Selected configuration file
        /// </summary>
        public string SelectedConfFile
        {
            get => _selectedConfFile;
            set
            {
                _selectedConfFile = value;
                LoadConfContent();
            }
        }

        /// <summary>
        /// Content of selected configuration file
        /// </summary>
        public string SelectedConfContent
        {
            get => _selectedConfContent;
            set => _selectedConfContent = value;
        }

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
        /// Selected image file
        /// </summary>
        public string SelectedImageFile
        {
            get => _selectedImageFile;
            set => _selectedImageFile = value;
        }

        /// <summary>
        /// Full path to selected image
        /// </summary>
        public string SelectedImagePath
        {
            get
            {
                if (string.IsNullOrEmpty(SelectedImageFile) || Project == null)
                    return null;
                return Path.Combine(Project.ProjectPath, "ModProject", "ModImg", SelectedImageFile);
            }
        }

        /// <summary>
        /// Check if an image file is selected
        /// </summary>
        public bool HasSelectedImageFile => !string.IsNullOrEmpty(SelectedImageFile);

        #endregion

        #region Tab 4: Global Variables

        /// <summary>
        /// List of global variables
        /// </summary>
        public List<GlobalVariable> GlobalVariables { get; set; } = new List<GlobalVariable>();

        #endregion

        #endregion

        #region Business Logic Methods

        /// <summary>
        /// Load all project data when project is set
        /// </summary>
        private void LoadProjectData()
        {
            if (Project == null) return;

            LoadConfFiles();
            LoadImageFiles();
            LoadGlobalVariables();
        }

        /// <summary>
        /// Reload all project data (public method for external calls)
        /// </summary>
        public void ReloadProjectData()
        {
            LoadProjectData();
        }

        /// <summary>
        /// Load configuration files from ModConf directory
        /// </summary>
        private void LoadConfFiles()
        {
            ConfFiles.Clear();
            if (Project == null) return;

            var confDir = Path.Combine(Project.ProjectPath, "ModProject", "ModConf");
            if (Directory.Exists(confDir))
            {
                var files = Directory.GetFiles(confDir, "*.json")
                    .Select(Path.GetFileName)
                    .ToList();
                ConfFiles = files;
            }
        }

        /// <summary>
        /// Load content of selected configuration file
        /// </summary>
        private void LoadConfContent()
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
        /// Load image files from ModImg directory
        /// </summary>
        private void LoadImageFiles()
        {
            ImageFiles.Clear();
            if (Project == null) return;

            var imgDir = Path.Combine(Project.ProjectPath, "ModProject", "ModImg");
            if (Directory.Exists(imgDir))
            {
                var files = Directory.GetFiles(imgDir, "*.*")
                    .Where(f => new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" }
                        .Contains(Path.GetExtension(f).ToLower()))
                    .Select(Path.GetFileName)
                    .ToList();
                ImageFiles = files;
            }
        }

        /// <summary>
        /// Load global variables from file or create default
        /// </summary>
        private void LoadGlobalVariables()
        {
            GlobalVariables.Clear();
            // TODO: Load from file or database
            // For now, add some sample data
            GlobalVariables.Add(new GlobalVariable
            {
                Name = "MOD_VERSION",
                Type = "string",
                Value = "1.0.0",
                Description = "Mod version"
            });
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
        private void SaveGlobalVariables()
        {
            // TODO: Implement save to file
        }

        #endregion
    }

    /// <summary>
    /// Global variable model
    /// </summary>
    public class GlobalVariable
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
    }
}
