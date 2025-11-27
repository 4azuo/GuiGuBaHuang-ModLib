using ModCreator.Attributes;
using ModCreator.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ModCreator.WindowData
{
    /// <summary>
    /// Project editor window data layer - Tab 2 (ModConf)
    /// </summary>
    public partial class ProjectEditorWindowData : CWindowData
    {
        #region Tab 2 Properties

        /// <summary>
        /// List of configuration files in ModConf directory
        /// </summary>
        public List<string> ConfFiles { get; set; } = new List<string>();

        /// <summary>
        /// Tree structure of configuration files and folders
        /// </summary>
        public ObservableCollection<FileItem> ConfItems { get; set; } = new ObservableCollection<FileItem>();

        /// <summary>
        /// Selected configuration file (legacy support)
        /// </summary>
        [NotifyMethod(nameof(LoadConfContent))]
        public string SelectedConfFile { get; set; }

        /// <summary>
        /// Selected file item in TreeView
        /// </summary>
        [NotifyMethod(nameof(OnConfItemSelected))]
        public FileItem SelectedConfItem { get; set; }

        /// <summary>
        /// Content of selected configuration file
        /// </summary>
        public string SelectedConfContent { get; set; }

        /// <summary>
        /// Check if a conf file is selected
        /// </summary>
        public bool HasSelectedConfFile => !string.IsNullOrEmpty(SelectedConfFile);

        /// <summary>
        /// Check if a conf item is selected
        /// </summary>
        public bool HasSelectedConfItem => SelectedConfItem != null;

        #endregion

        #region Tab 2 Methods

        /// <summary>
        /// Load configuration files from ModConf directory
        /// </summary>
        public void LoadConfFiles()
        {
            ConfFiles.Clear();
            ConfItems.Clear();
            if (Project == null) return;

            var confDir = Path.Combine(Project.ProjectPath, "ModProject", "ModConf");
            if (Directory.Exists(confDir))
            {
                // Load flat list (for legacy support)
                ConfFiles = Directory.GetFiles(confDir, "*.json", SearchOption.AllDirectories)
                    .Select(f => Path.GetRelativePath(confDir, f))
                    .ToList();
                
                // Build tree structure
                var items = BuildFileTree(confDir, confDir);
                foreach (var item in items)
                {
                    ConfItems.Add(item);
                }
                
                // Clear selection if the selected file no longer exists
                if (!string.IsNullOrEmpty(SelectedConfFile))
                {
                    var fullPath = Path.Combine(confDir, SelectedConfFile);
                    if (!File.Exists(fullPath))
                    {
                        SelectedConfFile = null;
                        SelectedConfItem = null;
                    }
                }
            }
            else
            {
                // Clear selection if directory doesn't exist
                SelectedConfFile = null;
                SelectedConfItem = null;
            }
        }

        /// <summary>
        /// Build tree structure from directory
        /// </summary>
        private List<FileItem> BuildFileTree(string rootPath, string currentPath, FileItem parent = null)
        {
            var items = new List<FileItem>();

            // Add subdirectories
            var directories = Directory.GetDirectories(currentPath).OrderBy(d => d);
            foreach (var dir in directories)
            {
                var dirName = Path.GetFileName(dir);
                var folderItem = new FileItem
                {
                    Name = dirName,
                    FullPath = dir,
                    RelativePath = Path.GetRelativePath(rootPath, dir),
                    IsFolder = true,
                    Parent = parent
                };

                // Recursively add children
                var children = BuildFileTree(rootPath, dir, folderItem);
                foreach (var child in children)
                {
                    folderItem.Children.Add(child);
                }

                items.Add(folderItem);
            }

            // Add JSON files
            var jsonFiles = Directory.GetFiles(currentPath, "*.json").OrderBy(f => f);
            foreach (var file in jsonFiles)
            {
                var fileItem = new FileItem
                {
                    Name = Path.GetFileName(file),
                    FullPath = file,
                    RelativePath = Path.GetRelativePath(rootPath, file),
                    IsFolder = false,
                    Parent = parent
                };
                items.Add(fileItem);
            }

            return items;
        }

        /// <summary>
        /// Called when TreeView item is selected
        /// </summary>
        public void OnConfItemSelected(object obj, PropertyInfo prop, object oldValue, object newValue)
        {
            if (SelectedConfItem == null || SelectedConfItem.IsFolder)
            {
                SelectedConfFile = null;
                return;
            }

            // Update SelectedConfFile for backward compatibility
            SelectedConfFile = SelectedConfItem.RelativePath;
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

        #endregion
    }
}
