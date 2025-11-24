using ModCreator.Attributes;
using ModCreator.Commons;
using ModCreator.Enums;
using ModCreator.Models;
using System;
using System.Collections.Generic;
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
        public List<ImageExtension> ImageExtensions { get; set; } = new List<ImageExtension>();

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

        /// <summary>
        /// List of global variables
        /// </summary>
        public List<GlobalVariable> GlobalVariables { get; set; } = new List<GlobalVariable>();

        /// <summary>
        /// Available variable types loaded from var-types.json
        /// </summary>
        public List<VarType> VarTypes { get; set; } = new List<VarType>();

        #endregion

        #endregion

        #region Business Logic Methods

        /// <summary>
        /// Load all project data when project is set
        /// </summary>
        public void LoadProjectData(string propName = "")
        {
            if (Project == null) return;

            LoadVarTypes();
            LoadImageExtensions();
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
        public void LoadConfFiles()
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
        public void LoadConfContent(string propName = "")
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
        public void OnSelectedImageChanged(string propName = "")
        {
            // Trigger SelectedImagePath property change notification
            // This causes the Image control to reload with the new bitmap
        }

        /// <summary>
        /// Load supported image extensions from image-extensions.json
        /// </summary>
        public void LoadImageExtensions()
        {
            ImageExtensions.Clear();

            var assembly = Assembly.GetExecutingAssembly();
            const string resourceName = "ModCreator.Resources.image-extensions.json";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var json = reader.ReadToEnd();
                        var extensions = JsonSerializer.Deserialize<List<ImageExtension>>(json);
                        if (extensions != null)
                        {
                            ImageExtensions.AddRange(extensions);
                        }
                    }
                }
            }
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

                var supportedExtensions = ImageExtensions.Select(e => e.Extension.ToLower()).ToList();

                var files = Directory.GetFiles(imgDir, "*.*")
                    .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLower()))
                    .Select(Path.GetFileName)
                    .ToList();
                ImageFiles = files;
            }
        }

        /// <summary>
        /// Load variable types from var-types.json
        /// </summary>
        public void LoadVarTypes()
        {
            VarTypes.Clear();

            var assembly = Assembly.GetExecutingAssembly();
            const string resourceName = "ModCreator.Resources.var-types.json";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var json = reader.ReadToEnd();
                        var types = JsonSerializer.Deserialize<List<VarType>>(json);
                        if (types != null)
                        {
                            VarTypes.AddRange(types);
                        }
                    }
                }
            }
        }

        public void LoadGlobalVariables()
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
        public void SaveGlobalVariables()
        {
            // TODO: Implement save to file
        }

        #endregion
    }
}
