using ModCreator.Attributes;
using ModCreator.Helpers;
using ModCreator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace ModCreator.WindowData
{
    /// <summary>
    /// Project editor window data layer - Tab 3 (ModImg)
    /// </summary>
    public partial class ProjectEditorWindowData : CWindowData
    {
        #region Tab 3 Properties

        /// <summary>
        /// List of image files in ModImg directory
        /// </summary>
        public List<string> ImageFiles { get; set; } = new List<string>();

        /// <summary>
        /// Tree structure of image files and folders
        /// </summary>
        public ObservableCollection<FileItem> ImageItems { get; set; } = new ObservableCollection<FileItem>();

        /// <summary>
        /// Supported image extensions loaded from image-extensions.json
        /// </summary>
        public List<ImageExtension> ImageExtensions { get; set; } = ResourceHelper.ReadEmbeddedResource<List<ImageExtension>>("ModCreator.Resources.image-extensions.json");

        /// <summary>
        /// Selected image file (legacy support)
        /// </summary>
        [NotifyMethod(nameof(OnSelectedImageChanged))]
        public string SelectedImageFile { get; set; }

        /// <summary>
        /// Selected image item in TreeView
        /// </summary>
        [NotifyMethod(nameof(OnImageItemSelected))]
        public FileItem SelectedImageItem { get; set; }

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
                return BitmapHelper.LoadFromFile(filePath);
            }
        }

        /// <summary>
        /// Check if an image file is selected
        /// </summary>
        public bool HasSelectedImageFile => !string.IsNullOrEmpty(SelectedImageFile);

        /// <summary>
        /// Check if an image item is selected
        /// </summary>
        public bool HasSelectedImageItem => SelectedImageItem != null;

        #endregion

        #region Tab 3 Methods

        /// <summary>
        /// Called when TreeView image item is selected
        /// </summary>
        public void OnImageItemSelected(object obj, PropertyInfo prop, object oldValue, object newValue)
        {
            if (SelectedImageItem == null || SelectedImageItem.IsFolder)
            {
                SelectedImageFile = null;
                return;
            }

            // Update SelectedImageFile for backward compatibility
            SelectedImageFile = SelectedImageItem.RelativePath;
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
            ImageItems.Clear();
            if (Project == null) return;

            var imgDir = Path.Combine(Project.ProjectPath, "ModProject", "ModImg");
            if (Directory.Exists(imgDir))
            {
                if (ImageExtensions.Count == 0)
                {
                    throw new InvalidOperationException("ImageExtensions not loaded. Cannot load image files.");
                }

                // Load flat list (for legacy support)
                ImageFiles = Directory.EnumerateFiles(imgDir, "*", SearchOption.AllDirectories)
                    .Where(f => ImageExtensions.Any(ext => ext.Extension == Path.GetExtension(f).ToLower()))
                    .Select(f => Path.GetRelativePath(imgDir, f))
                    .ToList();

                // Build tree structure
                var items = BuildImageFileTree(imgDir, imgDir);
                foreach (var item in items)
                {
                    ImageItems.Add(item);
                }

                // Clear selection if the selected file no longer exists
                if (!string.IsNullOrEmpty(SelectedImageFile))
                {
                    var fullPath = Path.Combine(imgDir, SelectedImageFile);
                    if (!File.Exists(fullPath))
                    {
                        SelectedImageFile = null;
                        SelectedImageItem = null;
                    }
                }
            }
            else
            {
                // Clear selection if directory doesn't exist
                SelectedImageFile = null;
                SelectedImageItem = null;
            }
        }

        /// <summary>
        /// Build tree structure from image directory
        /// </summary>
        private List<FileItem> BuildImageFileTree(string rootPath, string currentPath, FileItem parent = null)
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
                var children = BuildImageFileTree(rootPath, dir, folderItem);
                foreach (var child in children)
                {
                    folderItem.Children.Add(child);
                }

                items.Add(folderItem);
            }

            // Add image files with supported extensions
            var imageFiles = Directory.GetFiles(currentPath)
                .Where(f => ImageExtensions.Any(ext => ext.Extension == Path.GetExtension(f).ToLower()))
                .OrderBy(f => f);
            
            foreach (var file in imageFiles)
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

        #endregion
    }
}
