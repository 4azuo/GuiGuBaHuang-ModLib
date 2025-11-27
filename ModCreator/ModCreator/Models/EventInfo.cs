using System.Collections.Generic;

namespace ModCreator.Models
{
    /// <summary>
    /// Represents event metadata from JSON
    /// </summary>
    public class EventInfo
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Signature { get; set; }
        public List<string> Parameters { get; set; } = new List<string>();
    }
}
