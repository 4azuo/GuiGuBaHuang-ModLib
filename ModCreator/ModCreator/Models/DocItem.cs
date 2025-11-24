using ModCreator.Commons;
using System.Collections.ObjectModel;

namespace ModCreator.Models
{
    /// <summary>
    /// Documentation item
    /// </summary>
    public class DocItem : AutoNotifiableObject
    {
        public string Title { get; set; }
        public string FilePath { get; set; }
        public bool IsFolder { get; set; }
        public ObservableCollection<DocItem> Children { get; set; } = [];
    }
}
