using ModCreator.Commons;

namespace ModCreator.Models
{
    /// <summary>
    /// Global variable model
    /// </summary>
    public class GlobalVariable : AutoNotifiableObject
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
    }
}
