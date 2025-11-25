using ModCreator.Commons;
using ModCreator.Models;
using ModCreator.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ModCreator.WindowData
{
    /// <summary>
    /// Helper window data layer - displays documentation
    /// </summary>
    public class HelperWindowData : CWindowData
    {
        #region Properties

        /// <summary>
        /// List of documentation items
        /// </summary>
        public List<DocItem> DocItems { get; set; } = new List<DocItem>();

        /// <summary>
        /// Selected documentation item
        /// </summary>
        [NotifyMethod(nameof(LoadDocContent))]
        public DocItem SelectedDoc { get; set; }

        /// <summary>
        /// Documentation content
        /// </summary>
        public string DocContent { get; set; }

        #endregion

        #region Lifecycle Methods

        public override void OnLoad()
        {
            base.OnLoad();
            LoadDocumentList();
        }

        #endregion

        #region Business Logic Methods

        /// <summary>
        /// Load list of documentation files
        /// </summary>
        public void LoadDocumentList()
        {
            try
            {
                var docsPath = Constants.DocsDir;

                if (Directory.Exists(docsPath))
                {
                    DocItems = BuildDocTree(docsPath, docsPath);

                    // Select first file
                    var firstFile = FindFirstFile(DocItems);
                    if (firstFile != null)
                    {
                        SelectedDoc = firstFile;
                    }
                }
                else
                {
                    DocContent = "Documentation folder not found.\n\nExpected location: " + docsPath;
                }
            }
            catch (Exception ex)
            {
                DocContent = $"Error loading documentation:\n\n{ex.Message}";
            }
        }

        /// <summary>
        /// Build tree structure from directory
        /// </summary>
        private List<DocItem> BuildDocTree(string rootPath, string currentPath)
        {
            var items = new List<DocItem>();

            // Add subdirectories
            var directories = Directory.GetDirectories(currentPath).OrderBy(d => d);
            foreach (var dir in directories)
            {
                var dirName = Path.GetFileName(dir);
                var folderItem = new DocItem
                {
                    Title = dirName,
                    IsFolder = true,
                    FilePath = dir
                };

                // Recursively add children
                var children = BuildDocTree(rootPath, dir);
                foreach (var child in children)
                {
                    folderItem.Children.Add(child);
                }

                items.Add(folderItem);
            }

            // Add markdown files
            var mdFiles = Directory.GetFiles(currentPath, "*.md").OrderBy(f => f);
            foreach (var file in mdFiles)
            {
                var fileItem = new DocItem
                {
                    Title = Path.GetFileNameWithoutExtension(file),
                    FilePath = file,
                    IsFolder = false
                };
                items.Add(fileItem);
            }

            return items;
        }

        /// <summary>
        /// Find first file in tree
        /// </summary>
        private DocItem FindFirstFile(List<DocItem> items)
        {
            foreach (var item in items)
            {
                if (!item.IsFolder)
                    return item;

                if (item.Children.Count > 0)
                {
                    var found = FindFirstFile(item.Children.ToList());
                    if (found != null)
                        return found;
                }
            }
            return null;
        }

        /// <summary>
        /// Load content of selected documentation file
        /// </summary>
        public void LoadDocContent(object obj, PropertyInfo prop, object oldValue, object newValue)
        {
            if (SelectedDoc == null)
            {
                DocContent = string.Empty;
                return;
            }

            try
            {
                DocContent = File.ReadAllText(SelectedDoc.FilePath);
            }
            catch (Exception ex)
            {
                DocContent = $"Error loading file:\n\n{ex.Message}";
            }
        }

        #endregion
    }
}
