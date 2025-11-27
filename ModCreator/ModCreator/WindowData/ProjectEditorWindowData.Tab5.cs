using ModCreator.Attributes;
using ModCreator.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ModCreator.WindowData
{
    /// <summary>
    /// Project editor window data layer - Tab 5 (ModEvent)
    /// </summary>
    public partial class ProjectEditorWindowData : CWindowData
    {
        #region Tab 5 Properties

        /// <summary>
        /// Tree structure of ModEvent files
        /// </summary>
        public ObservableCollection<FileItem> EventItems { get; set; } = [];

        /// <summary>
        /// Selected ModEvent item in tree
        /// </summary>
        [NotifyMethod(nameof(OnEventItemSelected))]
        public FileItem SelectedEventItem { get; set; }

        /// <summary>
        /// Selected ModEvent for editing
        /// </summary>
        [NotifyMethod(nameof(LoadModEventContent))]
        public ModEventItem SelectedModEvent { get; set; }

        /// <summary>
        /// Source code content for Code mode
        /// </summary>
        public string EventSourceContent { get; set; }

        /// <summary>
        /// Check if an event file is selected
        /// </summary>
        public bool HasSelectedEventFile => SelectedModEvent != null;

        /// <summary>
        /// Current view mode: true = GUI, false = Code
        /// </summary>
        public bool IsGuiMode { get; set; } = true;

        /// <summary>
        /// Available event categories and events from JSON
        /// </summary>
        public List<EventCategory> EventCategories { get; set; } = new List<EventCategory>();

        /// <summary>
        /// Available conditions from JSON
        /// </summary>
        public List<ConditionInfo> AvailableConditions { get; set; } = new List<ConditionInfo>();

        /// <summary>
        /// Available actions from JSON
        /// </summary>
        public List<ActionInfo> AvailableActions { get; set; } = new List<ActionInfo>();

        /// <summary>
        /// Event categories from modevent-cats.json
        /// </summary>
        public List<string> EventCategoryList { get; set; } = new List<string>();

        /// <summary>
        /// Condition categories from modevent-cats.json
        /// </summary>
        public List<string> ConditionCategoryList { get; set; } = new List<string>();

        /// <summary>
        /// Action categories from modevent-cats.json
        /// </summary>
        public List<string> ActionCategoryList { get; set; } = new List<string>();

        /// <summary>
        /// Available CacheType values
        /// </summary>
        public List<string> CacheTypes { get; set; } = [];

        /// <summary>
        /// Available WorkOn values
        /// </summary>
        public List<string> WorkOnTypes { get; set; } = [];

        /// <summary>
        /// Event mode options
        /// </summary>
        public List<string> EventModeOptions { get; set; } = new List<string> { "ModEvent", "NonEvent" };

        /// <summary>
        /// Condition logic options
        /// </summary>
        public List<string> ConditionLogicOptions { get; set; } = new List<string> { "AND", "OR" };

        #endregion

        #region Tab 5 Methods

        /// <summary>
        /// Load ModEvent files from Mod folder
        /// </summary>
        public void LoadModEventFiles()
        {
            if (Project == null) return;

            try
            {
                var modPath = Path.Combine(Project.ProjectPath, "ModProject", "ModCode", "ModMain", "Mod");
                
                if (!Directory.Exists(modPath))
                {
                    Directory.CreateDirectory(modPath);
                }

                EventItems.Clear();
                var items = BuildEventFileTree(modPath, modPath);
                foreach (var item in items)
                {
                    EventItems.Add(item);
                }
            }
            catch
            {
                EventItems.Clear();
            }
        }

        /// <summary>
        /// Build event file tree structure
        /// </summary>
        private List<FileItem> BuildEventFileTree(string rootPath, string currentPath, FileItem parent = null)
        {
            var items = new List<FileItem>();

            // Add subdirectories
            var directories = Directory.GetDirectories(currentPath).OrderBy(d => d);
            foreach (var dir in directories)
            {
                var dirName = Path.GetFileName(dir);
                var folderItem = new FileItem
                {
                    Name = dirName,
                    FullPath = dir,
                    IsFolder = true,
                    Parent = parent
                };

                var children = BuildEventFileTree(rootPath, dir, folderItem);
                foreach (var child in children)
                {
                    folderItem.Children.Add(child);
                }

                items.Add(folderItem);
            }

            // Add .cs files
            var files = Directory.GetFiles(currentPath, "*.cs").OrderBy(f => f);
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var fileItem = new FileItem
                {
                    Name = fileName,
                    FullPath = file,
                    IsFolder = false,
                    Parent = parent
                };
                items.Add(fileItem);
            }

            return items;
        }

        /// <summary>
        /// Called when event item is selected
        /// </summary>
        public void OnEventItemSelected(object obj, PropertyInfo prop, object oldValue, object newValue)
        {
            if (SelectedEventItem != null && !SelectedEventItem.IsFolder)
            {
                LoadModEventFromFile(SelectedEventItem.FullPath);
            }
            else
            {
                SelectedModEvent = null;
            }
        }

        /// <summary>
        /// Load ModEvent from file
        /// </summary>
        private void LoadModEventFromFile(string filePath)
        {
            if (!File.Exists(filePath)) return;

            try
            {
                var content = File.ReadAllText(filePath);
                var modEvent = ParseModEventFromSource(content, filePath);
                SelectedModEvent = modEvent;
            }
            catch
            {
                SelectedModEvent = null;
            }
        }

        /// <summary>
        /// Parse ModEvent from C# source code
        /// </summary>
        private ModEventItem ParseModEventFromSource(string source, string filePath)
        {
            var modEvent = new ModEventItem
            {
                FilePath = filePath
            };

            // Parse Cache attribute
            var cacheMatch = System.Text.RegularExpressions.Regex.Match(source, 
                @"\[Cache\([""'](.+?)[""'],\s*CacheType\s*=\s*(.+?),\s*WorkOn\s*=\s*(.+?),\s*OrderIndex\s*=\s*(\d+)\)\]");
            if (cacheMatch.Success)
            {
                modEvent.CacheType = cacheMatch.Groups[2].Value.Trim();
                modEvent.WorkOn = cacheMatch.Groups[3].Value.Trim();
                modEvent.OrderIndex = int.Parse(cacheMatch.Groups[4].Value);
            }

            // Parse event method
            var eventMatch = System.Text.RegularExpressions.Regex.Match(source, 
                @"public override void (On\w+)\([^)]*\)");
            if (eventMatch.Success)
            {
                modEvent.SelectedEvent = eventMatch.Groups[1].Value;
            }

            // Parse condition logic
            var conditionMatch = System.Text.RegularExpressions.Regex.Match(source, 
                @"return (.+?);", System.Text.RegularExpressions.RegexOptions.Singleline);
            if (conditionMatch.Success)
            {
                var conditionCode = conditionMatch.Groups[1].Value.Trim();
                modEvent.ConditionLogic = conditionCode.Contains("&&") ? "AND" : "OR";
                
                // Parse individual conditions
                var separator = modEvent.ConditionLogic == "AND" ? "&&" : "||";
                var conditionParts = conditionCode.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                int order = 0;
                foreach (var part in conditionParts)
                {
                    var condition = new EventCondition
                    {
                        Code = part.Trim().TrimEnd(';'),
                        Order = order++
                    };
                    modEvent.Conditions.Add(condition);
                }
            }

            // Parse actions
            var actionMatches = System.Text.RegularExpressions.Regex.Matches(source, 
                @"(?:^|\n)\s+(.+?;)(?=\s*(?:\n|$))", System.Text.RegularExpressions.RegexOptions.Multiline);
            int actionOrder = 0;
            foreach (System.Text.RegularExpressions.Match match in actionMatches)
            {
                var actionCode = match.Groups[1].Value.Trim();
                if (!actionCode.StartsWith("if") && !actionCode.StartsWith("return") && !actionCode.Contains("CheckCondition"))
                {
                    var action = new EventAction
                    {
                        Code = actionCode,
                        Order = actionOrder++
                    };
                    modEvent.Actions.Add(action);
                }
            }

            return modEvent;
        }

        /// <summary>
        /// Load ModEvent content for editing
        /// </summary>
        public void LoadModEventContent(object obj, PropertyInfo prop, object oldValue, object newValue)
        {
            if (SelectedModEvent != null && File.Exists(SelectedModEvent.FilePath))
            {
                EventSourceContent = File.ReadAllText(SelectedModEvent.FilePath);
            }
            else
            {
                EventSourceContent = string.Empty;
            }
        }

        /// <summary>
        /// Save ModEvent to file
        /// </summary>
        public void SaveModEvent()
        {
            if (SelectedModEvent == null || string.IsNullOrEmpty(SelectedModEvent.FilePath))
                return;

            try
            {
                string content;
                if (IsGuiMode)
                {
                    // Generate code from GUI
                    content = GenerateModEventCode(SelectedModEvent);
                }
                else
                {
                    // Use source code from editor
                    content = EventSourceContent;
                }

                File.WriteAllText(SelectedModEvent.FilePath, content);
            }
            catch
            {
                // Handle error
            }
        }

        /// <summary>
        /// Generate ModEvent C# code from ModEventItem
        /// </summary>
        public string GenerateModEventCode(ModEventItem modEvent)
        {
            var templatePath = Path.Combine(Project.ProjectPath, "EventTemplate.tmp");
            var contentTemplatePath = Path.Combine(Project.ProjectPath, "EventTemplateContent.tmp");

            if (!File.Exists(templatePath) || !File.Exists(contentTemplatePath))
                return string.Empty;

            var template = File.ReadAllText(templatePath);
            var contentTemplate = File.ReadAllText(contentTemplatePath);

            // Find event info
            var eventInfo = EventCategories.SelectMany(c => c.Events)
                .FirstOrDefault(e => e.Name == modEvent.SelectedEvent);

            if (eventInfo == null)
                return string.Empty;

            // Generate condition code
            var conditionCode = GenerateConditionCode(modEvent);

            // Generate action code
            var actionCode = GenerateActionCode(modEvent);

            // Fill content template
            var eventContent = contentTemplate
                .Replace("#EVENTMETHOD#", eventInfo.Signature)
                .Replace("#CONDITION#", conditionCode)
                .Replace("#ACTION#", actionCode);

            // Fill main template
            var code = template
                .Replace("#CLASSNAME#", modEvent.FileName)
                .Replace("#CACHETYPE#", modEvent.CacheType)
                .Replace("#WORKON#", modEvent.WorkOn)
                .Replace("#ORDERINDEX#", modEvent.OrderIndex.ToString())
                .Replace("#EVENTCONTENT#", eventContent);

            return code;
        }

        /// <summary>
        /// Generate condition code
        /// </summary>
        private string GenerateConditionCode(ModEventItem modEvent)
        {
            if (modEvent.Conditions == null || modEvent.Conditions.Count == 0)
                return "true";

            var separator = modEvent.ConditionLogic == "AND" ? " && " : " || ";
            var conditions = modEvent.Conditions.OrderBy(c => c.Order)
                .Select(c => $"({c.Code})");

            return string.Join(separator, conditions);
        }

        /// <summary>
        /// Generate action code
        /// </summary>
        private string GenerateActionCode(ModEventItem modEvent)
        {
            if (modEvent.Actions == null || modEvent.Actions.Count == 0)
                return "// No actions";

            var actions = modEvent.Actions.OrderBy(a => a.Order)
                .Select(a => $"        {a.Code}");

            return string.Join("\n", actions);
        }

        /// <summary>
        /// Load event/condition/action definitions from JSON resources
        /// </summary>
        public void LoadModEventResources()
        {
            try
            {
                var resourcePrefix = "ModCreator.Resources.";

                // Load events
                EventCategories = Helpers.ResourceHelper.ReadEmbeddedResource<List<EventCategory>>(resourcePrefix + "modevent-events.json");

                // Load conditions
                AvailableConditions = Helpers.ResourceHelper.ReadEmbeddedResource<List<ConditionInfo>>(resourcePrefix + "modevent-conditions.json");

                // Load actions
                AvailableActions = Helpers.ResourceHelper.ReadEmbeddedResource<List<ActionInfo>>(resourcePrefix + "modevent-actions.json");

                // Load cache types
                CacheTypes = Helpers.ResourceHelper.ReadEmbeddedResource<List<string>>(resourcePrefix + "modevent-cachetype.json");

                // Load work on types
                WorkOnTypes = Helpers.ResourceHelper.ReadEmbeddedResource<List<string>>(resourcePrefix + "modevent-workon.json");

                // Load category lists
                var cats = Helpers.ResourceHelper.ReadEmbeddedResource<Dictionary<string, List<string>>>(resourcePrefix + "modevent-cats.json");
                EventCategoryList = cats["EventCategories"];
                ConditionCategoryList = cats["ConditionCategories"];
                ActionCategoryList = cats["ActionCategories"];
            }
            catch
            {
                // Handle error
            }
        }

        #endregion
    }
}
