using ModCreator.Commons;
using ModCreator.Enums;
using ModCreator.Models;
using System.Collections.Generic;

namespace ModCreator.WindowData
{
    /// <summary>
    /// Project editor window data layer
    /// </summary>
    public class ProjectEditorWindowData : CWindowData
    {
        #region Private Fields
        private ModProject _project;
        #endregion

        #region Properties

        /// <summary>
        /// Project being edited
        /// </summary>
        public ModProject Project
        {
            get => _project;
            set
            {
                _project = value;
                LoadModEvents();
            }
        }

        /// <summary>
        /// Current edit mode (UI or Code)
        /// </summary>
        public EditMode EditMode { get; set; } = EditMode.UI;

        /// <summary>
        /// Is UI mode active
        /// </summary>
        public bool IsUIMode => EditMode == EditMode.UI;

        /// <summary>
        /// Is Code mode active
        /// </summary>
        public bool IsCodeMode => EditMode == EditMode.Code;

        /// <summary>
        /// List of ModEvent files found in project
        /// </summary>
        public List<string> ModEvents { get; set; } = new List<string>();

        /// <summary>
        /// Project name (editable)
        /// </summary>
        public string ProjectName
        {
            get => Project?.ProjectName;
            set
            {
                if (Project != null && Project.ProjectName != value)
                {
                    Project.ProjectName = value;
                }
            }
        }

        /// <summary>
        /// Project description (editable)
        /// </summary>
        public string ProjectDescription
        {
            get => Project?.Description;
            set
            {
                if (Project != null && Project.Description != value)
                {
                    Project.Description = value;
                }
            }
        }

        #endregion

        #region Business Logic Methods

        /// <summary>
        /// Load ModEvent files from project directory
        /// </summary>
        public void LoadModEvents()
        {
            if (Project == null) return;

            // TODO: Scan project directory for ModEvent files
            // For now, use placeholder data
            ModEvents = new List<string>
            {
                "OnInit",
                "OnUpdate", 
                "OnDestroy"
            };
        }



        /// <summary>
        /// Save project changes
        /// </summary>
        public void SaveProject()
        {
            // TODO: Implement save logic
        }

        #endregion
    }
}
