using ModCreator.Commons;
using ModCreator.Helpers;
using ModCreator.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ModCreator.Attributes;
using System.IO;
using System.Linq;

namespace ModCreator.WindowData
{
    /// <summary>
    /// Main window data layer - handles all business logic
    /// </summary>
    public class MainWindowData : CWindowData
    {
        #region Private Fields
        private List<ModProject> _allProjects = new List<ModProject>();
        private string _statusMessage;
        #endregion

        #region Properties
        
        /// <summary>
        /// Workplace directory path
        /// </summary>
        [NotifyMethod(nameof(SaveWorkplacePath))]
        public string WorkplacePath { get; set; }
        
        /// <summary>
        /// All projects loaded from data file
        /// </summary>
        [JsonIgnore]
        public List<ModProject> AllProjects
        {
            get => _allProjects;
            set
            {
                _allProjects = value;
                UpdateFilteredProjects();
            }
        }

        /// <summary>
        /// Filtered projects for display
        /// </summary>
        [JsonIgnore]
        public List<ModProject> FilteredProjects { get; set; } = new List<ModProject>();

        /// <summary>
        /// Currently selected project
        /// </summary>
        [JsonIgnore]
        public ModProject SelectedProject { get; set; }

        /// <summary>
        /// Search text for filtering
        /// </summary>
        [JsonIgnore]
        [NotifyMethod(nameof(UpdateFilteredProjects))]
        public string SearchText { get; set; }

        /// <summary>
        /// Status message
        /// </summary>
        [JsonIgnore]
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = $"{DateTime.Now:HH:mm:ss} - {value}";
            }
        }

        /// <summary>
        /// Total projects count
        /// </summary>
        [JsonIgnore]
        public int TotalCount => FilteredProjects?.Count ?? 0;

        /// <summary>
        /// Project details visibility
        /// </summary>
        [JsonIgnore]
        public bool HasSelectedProject => SelectedProject != null;

        /// <summary>
        /// No project selected visibility
        /// </summary>
        [JsonIgnore]
        public bool HasNoSelectedProject => SelectedProject == null;

        /// <summary>
        /// Check if selected project is valid (exists)
        /// </summary>
        [JsonIgnore]
        public bool IsSelectedProjectValid => SelectedProject?.State == Enums.ProjectState.Valid;

        /// <summary>
        /// Selected project details
        /// </summary>
        [JsonIgnore]
        public string ProjectName => SelectedProject?.ProjectName ?? "";
        
        [JsonIgnore]
        public string ProjectId => SelectedProject?.ProjectId ?? "";
        
        [JsonIgnore]
        public string ProjectPath => SelectedProject?.ProjectPath ?? "";
        
        [JsonIgnore]
        public string Description => string.IsNullOrEmpty(SelectedProject?.Description) ? "-" : SelectedProject.Description;

        #endregion

        #region Events

        public override void OnLoad()
        {
            LoadProjects();
        }

        #endregion

        #region Business Logic Methods

        /// <summary>
        /// Load all projects from data file
        /// </summary>
        public void LoadProjects()
        {
            try
            {
                AllProjects = ProjectHelper.LoadProjects();
                
                // Check project existence and update state
                foreach (var project in AllProjects)
                {
                    if (!Directory.Exists(project.ProjectPath))
                    {
                        project.State = Enums.ProjectState.ProjectNotFound;
                    }
                    else
                    {
                        project.State = Enums.ProjectState.Valid;
                    }
                }
                
                StatusMessage = MessageHelper.GetFormat("LoadedProjects", AllProjects.Count);
            }
            catch (Exception ex)
            {
                StatusMessage = MessageHelper.GetFormat("ErrorLoadingProjects", ex.Message);
                AllProjects = new List<ModProject>();
            }
        }

        /// <summary>
        /// Create new project
        /// </summary>
        public ModProject CreateProject(string projectName, string targetDirectory, string description)
        {
            var newProject = ProjectHelper.CreateProject(projectName, targetDirectory, description);
            AllProjects.Add(newProject);
            ProjectHelper.SaveProjects(AllProjects);
            UpdateFilteredProjects();
            StatusMessage = MessageHelper.GetFormat("CreatedProject", projectName);
            return newProject;
        }

        /// <summary>
        /// Delete project
        /// </summary>
        public void DeleteProject(bool deleteFiles)
        {
            if (SelectedProject != null)
            {
                var projectName = SelectedProject.ProjectName;
                ProjectHelper.DeleteProject(SelectedProject, deleteFiles);
                AllProjects.Remove(SelectedProject);
                ProjectHelper.SaveProjects(AllProjects);
                SelectedProject = null;
                UpdateFilteredProjects();
                StatusMessage = MessageHelper.GetFormat("DeletedProject", projectName);
            }
        }

        /// <summary>
        /// Open project folder in explorer
        /// </summary>
        public void OpenProjectFolder()
        {
            if (SelectedProject != null)
            {
                ProjectHelper.OpenProjectFolder(SelectedProject);
                StatusMessage = MessageHelper.GetFormat("OpenedFolder", SelectedProject.ProjectName);
            }
        }

        /// <summary>
        /// Update filtered projects based on search text
        /// </summary>
        public void UpdateFilteredProjects(string propName = null)
        {
            if (AllProjects == null)
            {
                FilteredProjects = new List<ModProject>();
                return;
            }

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredProjects = new List<ModProject>(AllProjects);
            }
            else
            {
                var searchLower = SearchText.ToLower();
                FilteredProjects = AllProjects.Where(p =>
                    p.ProjectName.ToLower().Contains(searchLower) ||
                    p.ProjectId.ToLower().Contains(searchLower) ||
                    (!string.IsNullOrEmpty(p.Description) && p.Description.ToLower().Contains(searchLower))
                ).ToList();
            }
        }

        /// <summary>
        /// Save workplace path to settings
        /// </summary>
        public void SaveWorkplacePath(string propName = null)
        {
            if (!string.IsNullOrWhiteSpace(WorkplacePath))
            {
                if (!Directory.Exists(WorkplacePath))
                {
                    StatusMessage = MessageHelper.GetFormat("WorkplacePathNotFound", WorkplacePath);
                    return;
                }
                
                Properties.Settings.Default.WorkplacePath = WorkplacePath;
                Properties.Settings.Default.Save();
                StatusMessage = MessageHelper.GetFormat("WorkplacePathSaved", WorkplacePath);
            }
        }

        #endregion
    }
}
