using ModCreator.Attributes;
using ModCreator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ModCreator.WindowData
{
    public class AddConfWindowData : CWindowData
    {
        private List<ConfigFileInfo> _allConfigs;
        private Dictionary<string, string> _configDescriptions;

        [NotifyMethod(nameof(FilterConfigurations))]
        public string SearchText { get; set; }

        public string FilePrefix { get; set; }

        public ObservableCollection<ConfigFileInfo> FilteredConfigs { get; set; }

        public ConfigFileInfo SelectedConfig { get; set; }

        public string SelectedConfigDescription => SelectedConfig?.Description ?? "No configuration selected.";

        public bool HasSelectedConfig => SelectedConfig != null;

        public string GetFileName() => string.IsNullOrWhiteSpace(FilePrefix) 
            ? SelectedConfig?.Name 
            : $"{FilePrefix}_{SelectedConfig?.Name}";

        public AddConfWindowData()
        {
            _allConfigs = new List<ConfigFileInfo>();
            FilteredConfigs = new ObservableCollection<ConfigFileInfo>();
            _configDescriptions = new Dictionary<string, string>();
            LoadDescriptionsFromDocs();
        }

        public void LoadConfigurations()
        {
            _allConfigs.Clear();

            try
            {
                // Get SampleConfs folder path
                var sampleConfsPath = Path.Combine(
                    Constants.RootDir,
                    "3385996759", "SampleConfs");

                sampleConfsPath = Path.GetFullPath(sampleConfsPath);

                if (Directory.Exists(sampleConfsPath))
                {
                    var jsonFiles = Directory.GetFiles(sampleConfsPath, "*.json");

                    foreach (var file in jsonFiles)
                    {
                        var fileName = Path.GetFileName(file);
                        var description = GetConfigDescription(fileName);

                        _allConfigs.Add(new ConfigFileInfo
                        {
                            Name = fileName,
                            FilePath = file,
                            Description = description
                        });
                    }
                }

                FilterConfigurations();
            }
            catch (Exception ex)
            {
                Helpers.DebugHelper.ShowError(ex, "Error", "Failed to load configurations");
            }
        }

        public void FilterConfigurations(string propName = null)
        {
            FilteredConfigs.Clear();

            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? _allConfigs
                : _allConfigs.Where(c => c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            foreach (var config in filtered.OrderBy(c => c.Name))
            {
                FilteredConfigs.Add(config);
            }
        }

        public string GetConfigDescription(string fileName)
        {
            // Try to get description from loaded docs
            if (_configDescriptions.TryGetValue(fileName, out var description))
            {
                return description;
            }

            // Fallback to pattern matching
            if (fileName.StartsWith("Battle"))
                return $"Battle system configuration - {fileName}";
            if (fileName.StartsWith("Item"))
                return $"Item configuration - {fileName}";
            if (fileName.StartsWith("Npc"))
                return $"NPC configuration - {fileName}";
            if (fileName.StartsWith("Role"))
                return $"Character/Role configuration - {fileName}";
            if (fileName.StartsWith("School"))
                return $"School/Sect configuration - {fileName}";
            if (fileName.StartsWith("Town"))
                return $"Town configuration - {fileName}";
            if (fileName.StartsWith("World"))
                return $"World configuration - {fileName}";
            if (fileName.StartsWith("Dungeon"))
                return $"Dungeon configuration - {fileName}";
            if (fileName.StartsWith("DLC"))
                return $"DLC content configuration - {fileName}";

            return $"Configuration file - {fileName}";
        }

        private void LoadDescriptionsFromDocs()
        {
            try
            {
                var docsPath = Path.Combine(Constants.DocsDir, "details");
                
                if (!Directory.Exists(docsPath))
                    return;

                var mdFiles = Directory.GetFiles(docsPath, "*.md");

                foreach (var mdFile in mdFiles)
                {
                    ParseMarkdownFile(mdFile);
                }
            }
            catch (Exception ex)
            {
                // Silently fail - descriptions will use fallback
                System.Diagnostics.Debug.WriteLine($"Failed to load descriptions: {ex.Message}");
            }
        }

        private void ParseMarkdownFile(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                
                // Pattern 1: - **FileName.json** - Description
                var pattern1 = new Regex(@"^-\s+\*\*(.+?\.json)\*\*\s+-\s+(.+)$", RegexOptions.IgnoreCase);
                
                // Pattern 2: - **FileName.json**
                //            Description on next line or same line after dash
                var pattern2 = new Regex(@"^-\s+\*\*(.+?\.json)\*\*(?:\s+-\s+(.+))?$", RegexOptions.IgnoreCase);

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    
                    // Try pattern 1 (with description on same line)
                    var match = pattern1.Match(line);
                    if (match.Success)
                    {
                        var fileName = match.Groups[1].Value;
                        var description = match.Groups[2].Value;
                        
                        if (!_configDescriptions.ContainsKey(fileName))
                        {
                            _configDescriptions[fileName] = description;
                        }
                        continue;
                    }

                    // Try pattern 2 (might have description or not)
                    match = pattern2.Match(line);
                    if (match.Success)
                    {
                        var fileName = match.Groups[1].Value;
                        var description = match.Groups.Count > 2 && !string.IsNullOrEmpty(match.Groups[2].Value)
                            ? match.Groups[2].Value
                            : null;

                        // If no description on same line, check if there's a paragraph after
                        if (description == null && i + 1 < lines.Length)
                        {
                            var nextLine = lines[i + 1].Trim();
                            // If next line is not another bullet or header, it might be a description
                            if (!nextLine.StartsWith("-") && !nextLine.StartsWith("#") && !string.IsNullOrEmpty(nextLine))
                            {
                                description = nextLine;
                            }
                        }

                        if (!_configDescriptions.ContainsKey(fileName))
                        {
                            _configDescriptions[fileName] = description ?? $"Configuration for {fileName}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to parse {filePath}: {ex.Message}");
            }
        }
    }
}
