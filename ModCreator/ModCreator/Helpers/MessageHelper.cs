using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace ModCreator.Helpers
{
    /// <summary>
    /// Helper class for loading and accessing localized messages from messages.json
    /// </summary>
    public static class MessageHelper
    {
        private static Dictionary<string, string> _messages;
        private static readonly object _lock = new object();

        /// <summary>
        /// Load messages from the JSON file
        /// </summary>
        static MessageHelper()
        {
            LoadMessages();
        }

        /// <summary>
        /// Load or reload messages from messages.json
        /// </summary>
        public static void LoadMessages()
        {
            lock (_lock)
            {
                try
                {
                    var jsonPath = ModCreator.Constants.MessagesFilePath;
                    
                    if (!File.Exists(jsonPath))
                    {
                        throw new FileNotFoundException($"Message file not found: {jsonPath}");
                    }

                    var jsonContent = File.ReadAllText(jsonPath);
                    _messages = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading messages: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    _messages = new Dictionary<string, string>();
                }
            }
        }

        /// <summary>
        /// Get a message by key
        /// </summary>
        /// <param name="key">The message key</param>
        /// <returns>The message string, or the key itself if not found</returns>
        public static string Get(string key)
        {
            if (_messages == null)
            {
                LoadMessages();
            }

            if (_messages.TryGetValue(key, out var value))
            {
                return value;
            }

            return $"[{key}]"; // Return key in brackets if not found
        }

        /// <summary>
        /// Get a formatted message with parameters
        /// </summary>
        /// <param name="key">The message key</param>
        /// <param name="args">Format arguments</param>
        /// <returns>The formatted message string</returns>
        public static string GetFormat(string key, params object[] args)
        {
            var message = Get(key);
            try
            {
                return string.Format(message, args);
            }
            catch (FormatException)
            {
                return message; // Return unformatted if formatting fails
            }
        }

        // Direct property accessors for common messages
        public static string AppTitle => Get("AppTitle");
        public static string Browse => Get("Browse");
        public static string Cancel => Get("Cancel");
        public static string Create => Get("Create");
        public static string CreateNew => Get("CreateNew");
        public static string EditInfo => Get("EditInfo");
        public static string Error => Get("Error");
        public static string LoadedProjects => Get("LoadedProjects");
        public static string NewProjectTitle => Get("NewProjectTitle");
        public static string NoProjectSelected => Get("NoProjectSelected");
        public static string OpenFolder => Get("OpenFolder");
        public static string ProjectDeleted => Get("ProjectDeleted");
        public static string ProjectDeleteMessage => Get("ProjectDeleteMessage");
        public static string ProjectDeleteTitle => Get("ProjectDeleteTitle");
        public static string ProjectDetails => Get("ProjectDetails");
        public static string ProjectDetailsDescription => Get("ProjectDetailsDescription");
        public static string ProjectDetailsId => Get("ProjectDetailsId");
        public static string ProjectDetailsModId => Get("ProjectDetailsModId");
        public static string ProjectDetailsName => Get("ProjectDetailsName");
        public static string ProjectDetailsPath => Get("ProjectDetailsPath");
        public static string ProjectFieldDescription => Get("ProjectFieldDescription");
        public static string ProjectFieldName => Get("ProjectFieldName");
        public static string ProjectList => Get("ProjectList");
        public static string Ready => Get("Ready");
        public static string Refresh => Get("Refresh");
        public static string RemoveProject => Get("RemoveProject");
        public static string Save => Get("Save");
        public static string SearchPlaceholder => Get("SearchPlaceholder");
        public static string Success => Get("Success");
        public static string Workplace => Get("Workplace");
        public static string WorkplacePath => Get("WorkplacePath");
        public static string SelectWorkplace => Get("SelectWorkplace");
        public static string About => Get("About");
        public static string Help => Get("Help");
        public static string PleaseSetWorkplacePath => Get("PleaseSetWorkplacePath");
        public static string CreateNewProject => Get("CreateNewProject");
        public static string RefreshList => Get("RefreshList");
        public static string Actions => Get("Actions");
        public static string Info => Get("Info");
        public static string TotalProjects => Get("TotalProjects");
        public static string Template => Get("Template");
        public static string HeaderProjectName => Get("HeaderProjectName");
        public static string HeaderDescription => Get("HeaderDescription");
        public static string HeaderId => Get("HeaderId");
        public static string HeaderState => Get("HeaderState");
        public static string HeaderCreated => Get("HeaderCreated");
        public static string HeaderModified => Get("HeaderModified");
        public static string HeaderActions => Get("HeaderActions");
        public static string GridOpenFolder => Get("GridOpenFolder");
        public static string GridEditInfo => Get("GridEditInfo");
        public static string GridDelete => Get("GridDelete");
        public static string TooltipOpenFolder => Get("TooltipOpenFolder");
        public static string TooltipEditInfo => Get("TooltipEditInfo");
        public static string TooltipDelete => Get("TooltipDelete");
        public static string Version => Get("Version");
        public static string Author => Get("Author");
        public static string Repository => Get("Repository");
        public static string License => Get("License");
        public static string Features => Get("Features");
        public static string FeaturesList => Get("FeaturesList");
        public static string Close => Get("Close");
        public static string HelpTitle => Get("HelpTitle");
        public static string HelpSubtitle => Get("HelpSubtitle");
        public static string Topics => Get("Topics");
        public static string AppName => Get("AppName");
        public static string AppSubtitle => Get("AppSubtitle");
        public static string ProjectIdLabel => Get("ProjectIdLabel");
        public static string ProjectEditorTitle => Get("ProjectEditorTitle");
        public static string EditModeLabel => Get("EditModeLabel");
        public static string UIModeLabel => Get("UIModeLabel");
        public static string CodeModeLabel => Get("CodeModeLabel");
        public static string CodeModeNote => Get("CodeModeNote");
        public static string ModEventsHeader => Get("ModEventsHeader");
        public static string AddEvent => Get("AddEvent");
        public static string EditEvent => Get("EditEvent");
        public static string RemoveEvent => Get("RemoveEvent");
        public static string EventEditorHeader => Get("EventEditorHeader");
        public static string UIModeEditorTitle => Get("UIModeEditorTitle");
        public static string UIModeEditorSubtitle => Get("UIModeEditorSubtitle");
        public static string CodeModeEditorTitle => Get("CodeModeEditorTitle");
        public static string CodeModeEditorSubtitle => Get("CodeModeEditorSubtitle");
        public static string FeatureComingSoon => Get("FeatureComingSoon");
        public static string CodeEditorPlaceholder => Get("CodeEditorPlaceholder");
    }
}
