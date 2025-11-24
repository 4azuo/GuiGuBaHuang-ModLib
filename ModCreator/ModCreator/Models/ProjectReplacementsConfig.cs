using System.Collections.Generic;

namespace ModCreator.Models
{
    /// <summary>
    /// Configuration class for project replacements
    /// </summary>
    public class ProjectReplacementsConfig
    {
        /// <summary>
        /// List of file paths (relative to project root) to apply replacements to
        /// </summary>
        public List<string> Position { get; set; }

        /// <summary>
        /// Dictionary of placeholder keys and their corresponding property paths
        /// Example: { "#PROJECTID#": "ModCreator.Models.ModProject.ProjectId" }
        /// </summary>
        public Dictionary<string, string> KeyValues { get; set; }
    }
}
