using System;
using System.ComponentModel;
using ModCreator.Enums;

namespace ModCreator.Models
{
    /// <summary>
    /// Represents a mod project
    /// </summary>
    public class ModProject : ModCreator.Commons.AutoNotifiableObject
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectPath { get; set; }
        public string ModId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string Description { get; set; }
        
        /// <summary>
        /// Project state (Valid or ProjectNotFound)
        /// </summary>
        public ProjectState State { get; set; } = ProjectState.Valid;
    }
}
