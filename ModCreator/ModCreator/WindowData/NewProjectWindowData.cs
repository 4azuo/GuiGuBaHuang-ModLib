using ModCreator.Commons;
using ModCreator.Helpers;
using ModCreator.Attributes;
using System.ComponentModel;
using ModCreator.Models;
using System.Reflection;

namespace ModCreator.WindowData
{
    /// <summary>
    /// New project window data layer
    /// </summary>
    public class NewProjectWindowData : CWindowData
    {
        #region Properties


        /// <summary>
        /// Project Name
        /// </summary>
        [NotifyMethod(nameof(ValidateInput))]
        public string ProjectName { get; set; }

        /// <summary>
        /// Project Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Can create project (validation)
        /// </summary>
        public bool CanCreate { get; set; }

        /// <summary>
        /// Workplace path for new project
        /// </summary>
        public string WorkplacePath { get; set; }

        /// <summary>
        /// The created ModProject (after creation)
        /// </summary>
        public ModProject CreatedProject { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Validate input fields
        /// </summary>
        public void ValidateInput(object obj, PropertyInfo prop, object oldValue, object newValue)
        {
            CanCreate = !string.IsNullOrWhiteSpace(ProjectName);
        }

        /// <summary>
        /// Create and store a new ModProject
        /// </summary>
        public void CreateProject(string workplacePath)
        {
            var targetDirectory = workplacePath;
            CreatedProject = ModCreator.Helpers.ProjectHelper.CreateProject(ProjectName, targetDirectory, Description);
        }

        #endregion
    }
}
