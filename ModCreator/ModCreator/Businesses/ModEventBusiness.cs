using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.WindowData;
using ModCreator.Windows;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace ModCreator.Businesses
{
    /// <summary>
    /// Business logic for ModEvent operations
    /// </summary>
    public class ModEventBusiness
    {
        private readonly ProjectEditorWindowData _windowData;
        private readonly Window _owner;

        public ModEventBusiness(ProjectEditorWindowData windowData, Window owner)
        {
            _windowData = windowData;
            _owner = owner;
        }

        public void CreateModEvent()
        {
            try
            {
                var inputWindow = new InputWindow
                {
                    Owner = _owner
                };
                inputWindow.WindowData.WindowTitle = "Create ModEvent";
                inputWindow.WindowData.Label = "Event File Name (without extension):";
                inputWindow.WindowData.InputValue = "MyModEvent";

                if (inputWindow.ShowDialog() != true)
                    return;

                var fileName = inputWindow.WindowData.InputValue;

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    MessageBox.Show(
                        "File name cannot be empty!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var invalidChars = Path.GetInvalidFileNameChars();
                if (fileName.IndexOfAny(invalidChars) >= 0)
                {
                    MessageBox.Show(
                        "File name contains invalid characters!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var modEventPath = Path.Combine(_windowData.Project.ProjectPath, "ModProject", "ModCode", "ModEvent");
                Directory.CreateDirectory(modEventPath);

                var filePath = Path.Combine(modEventPath, $"{fileName}.cs");

                if (File.Exists(filePath))
                {
                    MessageBox.Show(
                        $"A file with the name '{fileName}.cs' already exists!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Calculate next OrderIndex
                int maxOrder = 0;
                if (_windowData.EventItems.Any())
                {
                    maxOrder = GetMaxOrderIndex(_windowData.EventItems);
                }

                // Create new ModEvent
                var newEvent = new ModEventItem
                {
                    EventMode = "ModEvent",
                    CacheType = _windowData.CacheTypes.FirstOrDefault() ?? "Local",
                    WorkOn = _windowData.WorkOnTypes.FirstOrDefault() ?? "All",
                    OrderIndex = maxOrder + 1,
                    FilePath = filePath
                };

                // Generate and save code
                _windowData.GenerateModEventCode(newEvent);

                // Reload events
                _windowData.LoadModEventFiles();

                MessageBox.Show(
                    $"ModEvent created successfully!\n\nFile: {fileName}.cs",
                    MessageHelper.Get("Messages.Success.Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to create ModEvent");
            }
        }

        public void CloneModEvent()
        {
            if (_windowData.SelectedModEvent == null)
                return;

            try
            {
                var sourceFile = _windowData.SelectedModEvent.FilePath;
                var sourceDir = Path.GetDirectoryName(sourceFile);
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(sourceFile);
                var extension = Path.GetExtension(sourceFile);

                var newFileName = $"{fileNameWithoutExt}_copy{extension}";
                var counter = 1;
                while (File.Exists(Path.Combine(sourceDir, newFileName)))
                {
                    newFileName = $"{fileNameWithoutExt}_copy{counter}{extension}";
                    counter++;
                }

                var destFile = Path.Combine(sourceDir, newFileName);

                // Clone ModEvent
                var clonedEvent = new ModEventItem
                {
                    EventMode = _windowData.SelectedModEvent.EventMode,
                    CustomEventName = _windowData.SelectedModEvent.CustomEventName,
                    CacheType = _windowData.SelectedModEvent.CacheType,
                    WorkOn = _windowData.SelectedModEvent.WorkOn,
                    OrderIndex = _windowData.SelectedModEvent.OrderIndex + 1,
                    SelectedEvent = _windowData.SelectedModEvent.SelectedEvent,
                    ConditionLogic = _windowData.SelectedModEvent.ConditionLogic,
                    FilePath = destFile
                };

                // Clone conditions and actions
                foreach (var condition in _windowData.SelectedModEvent.Conditions)
                {
                    clonedEvent.Conditions.Add(new EventCondition
                    {
                        Name = condition.Name,
                        DisplayName = condition.DisplayName,
                        Description = condition.Description,
                        Code = condition.Code,
                        Order = condition.Order
                    });
                }

                foreach (var action in _windowData.SelectedModEvent.Actions)
                {
                    clonedEvent.Actions.Add(new EventAction
                    {
                        Name = action.Name,
                        DisplayName = action.DisplayName,
                        Description = action.Description,
                        Code = action.Code,
                        Order = action.Order
                    });
                }

                // Generate and save
                _windowData.GenerateModEventCode(clonedEvent);
                _windowData.LoadModEventFiles();

                MessageBox.Show(
                    $"ModEvent cloned successfully!\n\nNew file: {Path.GetFileName(destFile)}",
                    MessageHelper.Get("Messages.Success.Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to clone ModEvent");
            }
        }

        public void RenameModEvent()
        {
            if (_windowData.SelectedModEvent == null)
                return;

            try
            {
                var oldFilePath = _windowData.SelectedModEvent.FilePath;
                var oldFileName = Path.GetFileNameWithoutExtension(oldFilePath);

                var inputWindow = new InputWindow
                {
                    Owner = _owner
                };
                inputWindow.WindowData.WindowTitle = "Rename ModEvent";
                inputWindow.WindowData.Label = "New file name (without extension):";
                inputWindow.WindowData.InputValue = oldFileName;

                if (inputWindow.ShowDialog() != true)
                    return;

                var newFileName = inputWindow.WindowData.InputValue;

                if (string.IsNullOrWhiteSpace(newFileName))
                {
                    MessageBox.Show(
                        "File name cannot be empty!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var newFilePath = Path.Combine(Path.GetDirectoryName(oldFilePath), $"{newFileName}.cs");

                if (File.Exists(newFilePath))
                {
                    MessageBox.Show(
                        $"A file with the name '{newFileName}.cs' already exists!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                File.Move(oldFilePath, newFilePath);
                _windowData.LoadModEventFiles();

                MessageBox.Show(
                    $"ModEvent renamed successfully!\n\nOld name: {oldFileName}.cs\nNew name: {newFileName}.cs",
                    MessageHelper.Get("Messages.Success.Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to rename ModEvent");
            }
        }

        public void DeleteModEvent()
        {
            if (_windowData.SelectedModEvent == null)
                return;

            try
            {
                var filePath = _windowData.SelectedModEvent.FilePath;
                var fileName = Path.GetFileName(filePath);

                var result = MessageBox.Show(
                    $"Are you sure you want to delete '{fileName}'?",
                    "Delete ModEvent",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    File.Delete(filePath);
                    _windowData.LoadModEventFiles();

                    MessageBox.Show(
                        $"ModEvent deleted successfully!",
                        MessageHelper.Get("Messages.Success.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to delete ModEvent");
            }
        }

        public void SaveModEvent()
        {
            if (_windowData.SelectedModEvent == null)
                return;

            try
            {
                _windowData.GenerateModEventCode(_windowData.SelectedModEvent);

                MessageBox.Show(
                    "ModEvent saved successfully!",
                    MessageHelper.Get("Messages.Success.Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to save ModEvent");
            }
        }

        public void AddCondition(string conditionName)
        {
            if (_windowData.SelectedModEvent == null || string.IsNullOrEmpty(conditionName))
                return;

            try
            {
                var conditionInfo = _windowData.AvailableConditions.FirstOrDefault(c => c.Name == conditionName);
                if (conditionInfo == null)
                    return;

                var newCondition = new EventCondition
                {
                    Name = conditionInfo.Name,
                    DisplayName = conditionInfo.DisplayName,
                    Description = conditionInfo.Description,
                    Code = conditionInfo.Code,
                    Order = _windowData.SelectedModEvent.Conditions.Count
                };

                _windowData.SelectedModEvent.Conditions.Add(newCondition);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to add condition");
            }
        }

        public void RemoveCondition(EventCondition condition)
        {
            if (_windowData.SelectedModEvent == null || condition == null)
                return;

            try
            {
                _windowData.SelectedModEvent.Conditions.Remove(condition);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to remove condition");
            }
        }

        public void AddAction(string actionName)
        {
            if (_windowData.SelectedModEvent == null || string.IsNullOrEmpty(actionName))
                return;

            try
            {
                var actionInfo = _windowData.AvailableActions.FirstOrDefault(a => a.Name == actionName);
                if (actionInfo == null)
                    return;

                var newAction = new EventAction
                {
                    Name = actionInfo.Name,
                    DisplayName = actionInfo.DisplayName,
                    Description = actionInfo.Description,
                    Code = actionInfo.Code,
                    Order = _windowData.SelectedModEvent.Actions.Count
                };

                _windowData.SelectedModEvent.Actions.Add(newAction);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to add action");
            }
        }

        public void RemoveAction(EventAction action)
        {
            if (_windowData.SelectedModEvent == null || action == null)
                return;

            try
            {
                _windowData.SelectedModEvent.Actions.Remove(action);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to remove action");
            }
        }

        public void OpenModEventFolder()
        {
            try
            {
                var modEventPath = Path.Combine(_windowData.Project.ProjectPath, "ModProject", "ModCode", "ModEvent");
                if (Directory.Exists(modEventPath))
                {
                    System.Diagnostics.Process.Start("explorer.exe", modEventPath);
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to open ModEvent folder");
            }
        }

        private int GetMaxOrderIndex(System.Collections.ObjectModel.ObservableCollection<FileItem> items)
        {
            int max = 0;
            
            foreach (var item in items)
            {
                // If it's a file, parse it
                if (!item.IsFolder && File.Exists(item.FullPath))
                {
                    try
                    {
                        var content = File.ReadAllText(item.FullPath);
                        var match = System.Text.RegularExpressions.Regex.Match(content, @"OrderIndex\s*=\s*(\d+)");
                        if (match.Success && int.TryParse(match.Groups[1].Value, out int orderIndex))
                        {
                            if (orderIndex > max)
                                max = orderIndex;
                        }
                    }
                    catch { }
                }
                
                // Recursively check children
                if (item.Children != null && item.Children.Any())
                {
                    var childMax = GetMaxOrderIndex(item.Children);
                    if (childMax > max)
                        max = childMax;
                }
            }
            
            return max;
        }
    }
}
