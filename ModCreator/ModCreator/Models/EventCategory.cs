using System.Collections.Generic;

namespace ModCreator.Models
{
    /// <summary>
    /// Represents event category from JSON
    /// </summary>
    public class EventCategory
    {
        public string Category { get; set; }
        public List<EventInfo> Events { get; set; } = new List<EventInfo>();
    }
}
