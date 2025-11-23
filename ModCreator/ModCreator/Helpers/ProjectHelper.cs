using ModCreator.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace ModCreator.Helpers
{
    /// <summary>
    /// Helper class for project operations
    /// </summary>
    public static class ProjectHelper
    {
        private const string PROJECTS_DATA_FILE = "projects.json";
        private const string PROJECT_TEMPLATE_NAME = "ModProject_0hKMNX";
        
        /// <summary>
        /// Get the projects data file path
        /// </summary>
        public static string GetProjectsDataFilePath()
        {
            // Save projects.json in GUIGUBAHUANG-MODLIB/ModCreator/projects.json
            var modCreatorDir = Path.Combine(ModCreator.Constants.RootDir, "ModCreator");
            if (!Directory.Exists(modCreatorDir))
                Directory.CreateDirectory(modCreatorDir);
            return Path.Combine(modCreatorDir, PROJECTS_DATA_FILE);
        }

        /// <summary>
        /// Get the project template path
        /// </summary>
        public static string GetProjectTemplatePath()
        {
            // Use absolute path to ProjectTemplate folder
            var templatePath = Path.Combine(ModCreator.Constants.RootDir, "ProjectTemplate", PROJECT_TEMPLATE_NAME);
            return templatePath;
        }

        /// <summary>
        /// Load all projects from data file
        /// </summary>
        public static List<ModProject> LoadProjects()
        {
            var filePath = GetProjectsDataFilePath();
            if (!File.Exists(filePath))
                return new List<ModProject>();

            try
            {
                var json = FileHelper.ReadTextFile(filePath);
                return JsonConvert.DeserializeObject<List<ModProject>>(json) ?? new List<ModProject>();
            }
            catch
            {
                return new List<ModProject>();
            }
        }

        /// <summary>
        /// Save all projects to data file
        /// </summary>
        public static void SaveProjects(List<ModProject> projects)
        {
            var filePath = GetProjectsDataFilePath();
            var json = JsonConvert.SerializeObject(projects, Formatting.Indented);
            FileHelper.WriteTextFile(filePath, json);
        }

        /// <summary>
        /// Create a new project from template
        /// </summary>
        public static ModProject CreateProject(string projectName, string targetDirectory, string description = "")
        {
            var templatePath = GetProjectTemplatePath();
            if (!Directory.Exists(templatePath))
            {
                throw new DirectoryNotFoundException($"Project template not found at: {templatePath}");
            }

            // Generate project ID
            var projectId = FileHelper.GenerateProjectId();
            var projectFolderName = $"ModProject_{projectId}";
            var projectPath = Path.Combine(targetDirectory, projectFolderName);

            // Check if project already exists
            if (Directory.Exists(projectPath))
            {
                throw new InvalidOperationException($"Project directory already exists: {projectPath}");
            }

            // Copy template
            FileHelper.CopyDirectory(templatePath, projectPath);

            // Create project object
            var project = new ModProject
            {
                ProjectId = projectId,
                ProjectName = projectName,
                ProjectPath = projectPath,
                ModId = "", // Will be set when mod is configured
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                Description = description
            };

            return project;
        }

        /// <summary>
        /// Update project info
        /// </summary>
        public static void UpdateProject(ModProject project)
        {
            //Todo
        }

        /// <summary>
        /// Delete a project
        /// </summary>
        public static void DeleteProject(ModProject project, bool deleteFiles = false)
        {
            var projects = LoadProjects();
            projects.RemoveAll(p => p.ProjectId == project.ProjectId);
            SaveProjects(projects);

            if (deleteFiles && Directory.Exists(project.ProjectPath))
            {
                Directory.Delete(project.ProjectPath, true);
            }
        }

        /// <summary>
        /// Check if project path is valid
        /// </summary>
        public static bool IsProjectValid(ModProject project)
        {
            return project != null && 
                   !string.IsNullOrEmpty(project.ProjectPath) && 
                   Directory.Exists(project.ProjectPath);
        }

        /// <summary>
        /// Open project folder in explorer
        /// </summary>
        public static void OpenProjectFolder(ModProject project)
        {
            if (IsProjectValid(project))
            {
                System.Diagnostics.Process.Start("explorer.exe", project.ProjectPath);
            }
        }
    }
}
