using ModCreator.Commons;
using System.Collections.ObjectModel;

namespace ModCreator.Models
{
    /// <summary>
    /// File or folder item for TreeView display
    /// </summary>
    public class FileItem : AutoNotifiableObject
    {
        /// <summary>
        /// Display name (file or folder name)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Full file path
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// Relative path from root
        /// </summary>
        public string RelativePath { get; set; }

        /// <summary>
        /// Is this a folder?
        /// </summary>
        public bool IsFolder { get; set; }

        /// <summary>
        /// Child items (for folders)
        /// </summary>
        public ObservableCollection<FileItem> Children { get; set; } = new ObservableCollection<FileItem>();

        /// <summary>
        /// Parent folder item
        /// </summary>
        public FileItem Parent { get; set; }
    }
}
