using ModCreator.Commons;
using ModCreator.Enums;
using System;
using System.Collections.Generic;

namespace ModCreator.Models
{
    /// <summary>
    /// Represents a mod project
    /// </summary>
    public class ModProject : AutoNotifiableObject
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectPath { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string TitleImg { get; set; }
        public ProjectState State { get; set; } = ProjectState.Valid;
        public List<GlobalVariable> GlobalVariables { get; set; } = new List<GlobalVariable>();
    }
}
