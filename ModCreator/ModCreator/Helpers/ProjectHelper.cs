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
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PROJECTS_DATA_FILE));
        }

        /// <summary>
        /// Get the project template path
        /// </summary>
        public static string GetProjectTemplatePath()
        {
            return Path.GetFullPath(Path.Combine(Constants.ResourcesDir, PROJECT_TEMPLATE_NAME));
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

            // Apply replacements based on new-project-replacements.json
            ApplyProjectReplacements(projectPath, projectId);

            // Create project object
            var project = new ModProject
            {
                ProjectId = projectId,
                ProjectName = projectName,
                ProjectPath = projectPath,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                Description = description,
                GlobalVariables = new System.Collections.Generic.List<GlobalVariable>
                {
                    new GlobalVariable
                    {
                        Name = "MOD_VERSION",
                        Type = "string",
                        Value = "1.0.0",
                        Description = "Mod version"
                    }
                }
            };

            return project;
        }

        /// <summary>
        /// Apply replacements to project files based on new-project-replacements.json
        /// </summary>
        private static void ApplyProjectReplacements(string projectPath, string projectId)
        {
            try
            {
                // Read replacements configuration from embedded resource
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceName = "ModCreator.Resources.new-project-replacements.json";
                
                string json;
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        DebugHelper.Warning($"Embedded resource not found: {resourceName}");
                        return;
                    }
                    
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        json = reader.ReadToEnd();
                    }
                }

                var config = JsonConvert.DeserializeObject<ProjectReplacementsConfig>(json);
                if (config == null || config.Position == null || config.KeyValues == null)
                {
                    DebugHelper.Warning("Invalid replacements configuration");
                    return;
                }

                // Process each file in the Position list
                foreach (var relativePath in config.Position)
                {
                    var filePath = Path.Combine(projectPath, relativePath);
                    if (!File.Exists(filePath))
                    {
                        DebugHelper.Warning($"File not found for replacement: {filePath}");
                        continue;
                    }

                    // Read file content
                    var content = FileHelper.ReadTextFile(filePath);

                    // Apply all key-value replacements
                    foreach (var kvp in config.KeyValues)
                    {
                        var placeholder = kvp.Key;
                        var propertyPath = kvp.Value; // e.g., "ModCreator.Models.ModProject.ProjectId"
                        
                        // Get the replacement value based on property path
                        var replacementValue = GetReplacementValue(propertyPath, projectId);
                        
                        // Replace in content
                        content = content.Replace(placeholder, replacementValue);
                    }

                    // Write back to file
                    FileHelper.WriteTextFile(filePath, content);
                }
            }
            catch (Exception ex)
            {
                DebugHelper.Error($"Error applying project replacements: {ex.Message}");
            }
        }

        /// <summary>
        /// Get replacement value based on property path
        /// </summary>
        private static string GetReplacementValue(string propertyPath, string projectId)
        {
            // Property path format: "ModCreator.Models.ModProject.ProjectId"
            // For now, we only support ProjectId
            if (propertyPath.EndsWith(".ProjectId"))
            {
                return projectId;
            }

            // Add more property mappings as needed
            return string.Empty;
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
