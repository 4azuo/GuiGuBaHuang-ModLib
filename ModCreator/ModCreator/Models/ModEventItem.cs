using System.Collections.Generic;
using ModCreator.Commons;

namespace ModCreator.Models
{
    /// <summary>
    /// Represents a ModEvent with its configuration
    /// </summary>
    public class ModEventItem : AutoNotifiableObject
    {
        /// <summary>
        /// Event mode: "ModEvent" or "NonEvent"
        /// </summary>
        public string EventMode { get; set; } = "ModEvent";

        /// <summary>
        /// Custom event method name (for NonEvent mode)
        /// </summary>
        public string CustomEventName { get; set; }

        /// <summary>
        /// Cache type for the event (e.g., Local, Global)
        /// </summary>
        public string CacheType { get; set; }

        /// <summary>
        /// WorkOn type (e.g., All, Local, Global)
        /// </summary>
        public string WorkOn { get; set; }

        /// <summary>
        /// Order index for event execution
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// Selected event method name
        /// </summary>
        public string SelectedEvent { get; set; }

        /// <summary>
        /// Condition logic: "AND" or "OR"
        /// </summary>
        public string ConditionLogic { get; set; }

        /// <summary>
        /// List of conditions for this event
        /// </summary>
        public List<EventCondition> Conditions { get; set; } = new List<EventCondition>();

        /// <summary>
        /// List of actions for this event
        /// </summary>
        public List<EventAction> Actions { get; set; } = new List<EventAction>();

        /// <summary>
        /// Full file path to the ModEvent .cs file
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// File name without path (without extension, used as class name)
        /// </summary>
        public string FileName => System.IO.Path.GetFileNameWithoutExtension(FilePath);
    }
}
