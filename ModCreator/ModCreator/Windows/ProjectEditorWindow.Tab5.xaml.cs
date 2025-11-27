using ModCreator.Businesses;
using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.WindowData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.MessageBox;

namespace ModCreator.Windows
{
    public partial class ProjectEditorWindow : CWindow<ProjectEditorWindowData>
    {        
        [SupportedOSPlatform("windows6.1")]
        private void PopulateEventsComboBox()
        {
            var cmbEvents = this.FindName("cmbEvents") as System.Windows.Controls.ComboBox;
            if (cmbEvents == null || WindowData == null) return;

            // Flatten all events from all categories
            var allEvents = WindowData.EventCategories.SelectMany(cat => cat.Events).ToList();
            cmbEvents.ItemsSource = allEvents;
        }

        private void TreeView_EventSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is FileItem fileItem)
            {
                WindowData.SelectedEventItem = fileItem;
            }
        }

        private void CreateModEvent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var inputWindow = new InputWindow { Owner = this };
                inputWindow.WindowData.WindowTitle = "Create New ModEvent";
                inputWindow.WindowData.Label = "Class name:";
                inputWindow.WindowData.InputValue = "NewModEvent";

                if (inputWindow.ShowDialog() != true)
                    return;

                var className = inputWindow.WindowData.InputValue;
                
                // Validate class name
                if (string.IsNullOrWhiteSpace(className) || !System.Text.RegularExpressions.Regex.IsMatch(className, @"^[A-Za-z_][A-Za-z0-9_]*$"))
                {
                    MessageBox.Show("Invalid class name! Must start with letter and contain only letters, numbers, and underscores.",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var modPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModCode", "ModMain", "Mod");
                Directory.CreateDirectory(modPath);

                var filePath = Path.Combine(modPath, $"{className}.cs");
                
                if (File.Exists(filePath))
                {
                    MessageBox.Show($"A ModEvent with name '{className}' already exists!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Create new ModEvent with template
                // Calculate max OrderIndex from existing events
                var maxOrder = 0;
                if (WindowData.EventItems != null)
                {
                    void TraverseItems(IEnumerable<FileItem> items)
                    {
                        foreach (var item in items)
                        {
                            if (!item.IsFolder && item.FullPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    var content = File.ReadAllText(item.FullPath);
                                    var match = System.Text.RegularExpressions.Regex.Match(content, @"OrderIndex\s*=\s*(\d+)");
                                    if (match.Success && int.TryParse(match.Groups[1].Value, out int order))
                                    {
                                        if (order > maxOrder)
                                            maxOrder = order;
                                    }
                                }
                                catch { }
                            }
                            if (item.Children != null && item.Children.Any())
                            {
                                TraverseItems(item.Children);
                            }
                        }
                    }
                    TraverseItems(WindowData.EventItems);
                }

                var newEvent = new ModEventItem
                {
                    OrderIndex = maxOrder + 1,
                    CacheType = "Local",
                    WorkOn = "All",
                    SelectedEvent = "OnTimeUpdate1000ms",
                    ConditionLogic = "AND",
                    FilePath = filePath
                };

                var code = WindowData.GenerateModEventCode(newEvent);
                File.WriteAllText(filePath, code);

                WindowData.LoadModEventFiles();
                
                MessageBox.Show($"ModEvent '{className}' created successfully!",
                    MessageHelper.Get("Messages.Success.Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to create ModEvent");
            }
        }

        private void CloneModEvent_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedModEvent == null) return;

            try
            {
                var oldClassName = WindowData.SelectedModEvent.FileName;
                var newClassName = $"{oldClassName}_Copy";

                var inputWindow = new InputWindow { Owner = this };
                inputWindow.WindowData.WindowTitle = "Clone ModEvent";
                inputWindow.WindowData.Label = "New class name:";
                inputWindow.WindowData.InputValue = newClassName;

                if (inputWindow.ShowDialog() != true)
                    return;

                newClassName = inputWindow.WindowData.InputValue;

                if (string.IsNullOrWhiteSpace(newClassName) || !System.Text.RegularExpressions.Regex.IsMatch(newClassName, @"^[A-Za-z_][A-Za-z0-9_]*$"))
                {
                    MessageBox.Show("Invalid class name!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var modPath = Path.GetDirectoryName(WindowData.SelectedModEvent.FilePath);
                var newFilePath = Path.Combine(modPath, $"{newClassName}.cs");

                if (File.Exists(newFilePath))
                {
                    MessageBox.Show($"A ModEvent with name '{newClassName}' already exists!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Clone the event
                var clonedEvent = new ModEventItem
                {
                    OrderIndex = WindowData.SelectedModEvent.OrderIndex,
                    CacheType = WindowData.SelectedModEvent.CacheType,
                    WorkOn = WindowData.SelectedModEvent.WorkOn,
                    SelectedEvent = WindowData.SelectedModEvent.SelectedEvent,
                    ConditionLogic = WindowData.SelectedModEvent.ConditionLogic,
                    FilePath = newFilePath
                };

                // Clone conditions and actions
                foreach (var condition in WindowData.SelectedModEvent.Conditions)
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

                foreach (var action in WindowData.SelectedModEvent.Actions)
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

                var code = WindowData.GenerateModEventCode(clonedEvent);
                File.WriteAllText(newFilePath, code);

                WindowData.LoadModEventFiles();

                MessageBox.Show($"ModEvent cloned successfully!",
                    MessageHelper.Get("Messages.Success.Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to clone ModEvent");
            }
        }

        private void RenameModEvent_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedModEvent == null) return;

            try
            {
                var oldClassName = WindowData.SelectedModEvent.FileName;

                var inputWindow = new InputWindow { Owner = this };
                inputWindow.WindowData.WindowTitle = "Rename ModEvent";
                inputWindow.WindowData.Label = "New class name:";
                inputWindow.WindowData.InputValue = oldClassName;

                if (inputWindow.ShowDialog() != true)
                    return;

                var newClassName = inputWindow.WindowData.InputValue;

                if (newClassName == oldClassName)
                    return;

                if (string.IsNullOrWhiteSpace(newClassName) || !System.Text.RegularExpressions.Regex.IsMatch(newClassName, @"^[A-Za-z_][A-Za-z0-9_]*$"))
                {
                    MessageBox.Show("Invalid class name!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var oldFilePath = WindowData.SelectedModEvent.FilePath;
                var modPath = Path.GetDirectoryName(oldFilePath);
                var newFilePath = Path.Combine(modPath, $"{newClassName}.cs");

                if (File.Exists(newFilePath))
                {
                    MessageBox.Show($"A ModEvent with name '{newClassName}' already exists!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Update class name and save
                WindowData.SelectedModEvent.FilePath = newFilePath;
                WindowData.SaveModEvent();

                // Delete old file
                if (File.Exists(oldFilePath))
                {
                    File.Delete(oldFilePath);
                }

                WindowData.LoadModEventFiles();

                MessageBox.Show($"ModEvent renamed successfully!",
                    MessageHelper.Get("Messages.Success.Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to rename ModEvent");
            }
        }

        private void DeleteModEvent_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedModEvent == null) return;

            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete ModEvent '{WindowData.SelectedModEvent.FileName}'?",
                    "Delete ModEvent",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;

                var filePath = WindowData.SelectedModEvent.FilePath;
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                WindowData.LoadModEventFiles();
                WindowData.SelectedModEvent = null;

                MessageBox.Show("ModEvent deleted successfully!",
                    MessageHelper.Get("Messages.Success.Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to delete ModEvent");
            }
        }

        private void SaveModEvent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WindowData.SaveModEvent();
                
                MessageBox.Show("ModEvent saved successfully!",
                    MessageHelper.Get("Messages.Success.Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to save ModEvent");
            }
        }

        private void AddEvent_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedModEvent == null) return;

            try
            {
                var selectWindow = new ModEventItemSelectWindow { Owner = this };
                selectWindow.WindowData.InitializeWithEvents(WindowData.EventCategories);

                if (selectWindow.ShowDialog() == true)
                {
                    var selectedItem = selectWindow.WindowData.SelectedItem as EventInfoDisplay;
                    if (selectedItem != null)
                    {
                        WindowData.SelectedModEvent.SelectedEvent = selectedItem.Name;
                    }
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to add event");
            }
        }

        private void AddCondition_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedModEvent == null) return;

            try
            {
                var selectWindow = new ModEventItemSelectWindow { Owner = this };
                selectWindow.WindowData.InitializeWithConditions(WindowData.AvailableConditions);

                if (selectWindow.ShowDialog() == true)
                {
                    var conditionInfo = selectWindow.WindowData.SelectedItem as ConditionInfo;
                    if (conditionInfo != null)
                    {
                        var condition = new EventCondition
                        {
                            Name = conditionInfo.Name,
                            DisplayName = conditionInfo.DisplayName,
                            Description = conditionInfo.Description,
                            Code = conditionInfo.Code,
                            Order = WindowData.SelectedModEvent.Conditions.Count
                        };

                        WindowData.SelectedModEvent.Conditions.Add(condition);
                    }
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to add condition");
            }
        }

        private void RemoveCondition_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            var condition = button?.Tag as EventCondition;
            if (condition == null || WindowData.SelectedModEvent == null) return;

            WindowData.SelectedModEvent.Conditions.Remove(condition);
        }

        private void AddAction_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedModEvent == null) return;

            try
            {
                var selectWindow = new ModEventItemSelectWindow { Owner = this };
                selectWindow.WindowData.InitializeWithActions(WindowData.AvailableActions);

                if (selectWindow.ShowDialog() == true)
                {
                    var actionInfo = selectWindow.WindowData.SelectedItem as ActionInfo;
                    if (actionInfo != null)
                    {
                        var action = new EventAction
                        {
                            Name = actionInfo.Name,
                            DisplayName = actionInfo.DisplayName,
                            Description = actionInfo.Description,
                            Code = actionInfo.Code,
                            Order = WindowData.SelectedModEvent.Actions.Count
                        };

                        WindowData.SelectedModEvent.Actions.Add(action);
                    }
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to add action");
            }
        }

        private void RemoveAction_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            var action = button?.Tag as EventAction;
            if (action == null || WindowData.SelectedModEvent == null) return;

            WindowData.SelectedModEvent.Actions.Remove(action);
            
            // Reorder remaining actions
            for (int i = 0; i < WindowData.SelectedModEvent.Actions.Count; i++)
            {
                WindowData.SelectedModEvent.Actions[i].Order = i;
            }
        }

        private void OpenModEventFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (WindowData?.Project != null)
                {
                    var modPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModCode", "ModMain", "Mod");
                    Directory.CreateDirectory(modPath);
                    System.Diagnostics.Process.Start("explorer.exe", modPath);
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to open Mod folder");
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void ToggleGuiMode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var guiPanel = this.FindName("guiModePanel") as Grid;
                var codePanel = this.FindName("codeModePanel") as Grid;
                var btnGui = this.FindName("btnGuiMode") as System.Windows.Controls.Button;
                var btnCode = this.FindName("btnCodeMode") as System.Windows.Controls.Button;

                if (guiPanel == null || codePanel == null || btnGui == null || btnCode == null) return;

                guiPanel.Visibility = Visibility.Visible;
                codePanel.Visibility = Visibility.Collapsed;
                WindowData.IsGuiMode = true;

                // Update button styles
                btnGui.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF2E5090"));
                btnGui.Foreground = System.Windows.Media.Brushes.White;
                btnCode.Background = System.Windows.Media.Brushes.White;
                btnCode.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF666666"));
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to switch to GUI mode");
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void ToggleCodeMode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var guiPanel = this.FindName("guiModePanel") as Grid;
                var codePanel = this.FindName("codeModePanel") as Grid;
                var btnGui = this.FindName("btnGuiMode") as System.Windows.Controls.Button;
                var btnCode = this.FindName("btnCodeMode") as System.Windows.Controls.Button;

                if (guiPanel == null || codePanel == null || btnGui == null || btnCode == null) return;

                guiPanel.Visibility = Visibility.Collapsed;
                codePanel.Visibility = Visibility.Visible;
                WindowData.IsGuiMode = false;

                // Setup AvalonEdit if not already done
                SetupEventSourceEditorBinding();

                // Update button styles
                btnCode.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF2E5090"));
                btnCode.Foreground = System.Windows.Media.Brushes.White;
                btnGui.Background = System.Windows.Media.Brushes.White;
                btnGui.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF666666"));
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to switch to Code mode");
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void SetupEventSourceEditorBinding()
        {
            var editor = this.FindName("txtEventSourceEditor") as ICSharpCode.AvalonEdit.TextEditor;
            if (editor == null || editor.Tag != null) return; // Already setup

            editor.Tag = "setup"; // Mark as setup

            // Load C# syntax highlighting
            AvalonHelper.LoadCSharpSyntaxHighlighting(editor);

            // Subscribe to property changes
            WindowData.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(WindowData.EventSourceContent))
                {
                    if (editor.Text != WindowData.EventSourceContent)
                    {
                        editor.Text = WindowData.EventSourceContent ?? string.Empty;
                    }
                }
            };

            // Subscribe to editor changes
            editor.TextChanged += (s, e) =>
            {
                if (WindowData != null && editor.Text != WindowData.EventSourceContent)
                {
                    WindowData.EventSourceContent = editor.Text;
                }
            };
        }

        // Drag/drop for conditions
        private EventCondition _draggedCondition;

        private void Conditions_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var listBox = sender as System.Windows.Controls.ListBox;
            if (listBox == null) return;

            var item = ItemsControl.ContainerFromElement(listBox, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null)
            {
                _draggedCondition = item.Content as EventCondition;
                if (_draggedCondition != null)
                {
                    DragDrop.DoDragDrop(item, _draggedCondition, System.Windows.DragDropEffects.Move);
                }
            }
        }

        private void Conditions_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (_draggedCondition == null || WindowData.SelectedModEvent == null) return;

            var listBox = sender as System.Windows.Controls.ListBox;
            if (listBox == null) return;

            var targetItem = ItemsControl.ContainerFromElement(listBox, e.OriginalSource as DependencyObject) as ListBoxItem;
            var targetCondition = targetItem?.Content as EventCondition;

            if (targetCondition != null && targetCondition != _draggedCondition)
            {
                var conditions = WindowData.SelectedModEvent.Conditions;
                int oldIndex = conditions.IndexOf(_draggedCondition);
                int newIndex = conditions.IndexOf(targetCondition);

                if (oldIndex >= 0 && newIndex >= 0)
                {
                    conditions.RemoveAt(oldIndex);
                    conditions.Insert(newIndex, _draggedCondition);
                    
                    // Update order
                    for (int i = 0; i < conditions.Count; i++)
                    {
                        conditions[i].Order = i;
                    }
                }
            }

            _draggedCondition = null;
        }

        // Drag/drop for actions
        private EventAction _draggedAction;

        private void Actions_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var listBox = sender as System.Windows.Controls.ListBox;
            if (listBox == null) return;

            var item = ItemsControl.ContainerFromElement(listBox, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null)
            {
                _draggedAction = item.Content as EventAction;
                if (_draggedAction != null)
                {
                    DragDrop.DoDragDrop(item, _draggedAction, System.Windows.DragDropEffects.Move);
                }
            }
        }

        private void Actions_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (_draggedAction == null || WindowData.SelectedModEvent == null) return;

            var listBox = sender as System.Windows.Controls.ListBox;
            if (listBox == null) return;

            var targetItem = ItemsControl.ContainerFromElement(listBox, e.OriginalSource as DependencyObject) as ListBoxItem;
            var targetAction = targetItem?.Content as EventAction;

            if (targetAction != null && targetAction != _draggedAction)
            {
                var actions = WindowData.SelectedModEvent.Actions;
                int oldIndex = actions.IndexOf(_draggedAction);
                int newIndex = actions.IndexOf(targetAction);

                if (oldIndex >= 0 && newIndex >= 0)
                {
                    actions.RemoveAt(oldIndex);
                    actions.Insert(newIndex, _draggedAction);
                    
                    // Update order
                    for (int i = 0; i < actions.Count; i++)
                    {
                        actions[i].Order = i;
                    }
                }
            }

            _draggedAction = null;
        }

        // Number validation
        private void NumberOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        // Event mode selection changed
        [SupportedOSPlatform("windows6.1")]
        private void EventMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as System.Windows.Controls.ComboBox;
            if (comboBox == null || WindowData?.SelectedModEvent == null) return;

            var mode = comboBox.SelectedItem as string;
            var grpEventSelection = this.FindName("grpEventSelection") as System.Windows.Controls.GroupBox;
            var gridCustomEventName = this.FindName("gridCustomEventName") as Grid;
            
            // Get controls for OrderIndex, CacheType, and WorkOn
            var txtOrderIndexLabel = this.FindName("txtOrderIndexLabel") as System.Windows.Controls.TextBlock;
            var txtOrderIndex = this.FindName("txtOrderIndex") as System.Windows.Controls.TextBox;
            var txtCacheTypeLabel = this.FindName("txtCacheTypeLabel") as System.Windows.Controls.TextBlock;
            var cmbCacheType = this.FindName("cmbCacheType") as System.Windows.Controls.ComboBox;
            var txtWorkOnLabel = this.FindName("txtWorkOnLabel") as System.Windows.Controls.TextBlock;
            var cmbWorkOn = this.FindName("cmbWorkOn") as System.Windows.Controls.ComboBox;

            if (mode == "ModEvent")
            {
                // Show event selection, hide custom event name
                if (grpEventSelection != null)
                    grpEventSelection.Visibility = Visibility.Visible;
                if (gridCustomEventName != null)
                    gridCustomEventName.Visibility = Visibility.Collapsed;
                    
                // Show OrderIndex, CacheType, and WorkOn
                if (txtOrderIndexLabel != null) txtOrderIndexLabel.Visibility = Visibility.Visible;
                if (txtOrderIndex != null) txtOrderIndex.Visibility = Visibility.Visible;
                if (txtCacheTypeLabel != null) txtCacheTypeLabel.Visibility = Visibility.Visible;
                if (cmbCacheType != null) cmbCacheType.Visibility = Visibility.Visible;
                if (txtWorkOnLabel != null) txtWorkOnLabel.Visibility = Visibility.Visible;
                if (cmbWorkOn != null) cmbWorkOn.Visibility = Visibility.Visible;
            }
            else if (mode == "NonEvent")
            {
                // Hide event selection, show custom event name
                if (grpEventSelection != null)
                    grpEventSelection.Visibility = Visibility.Collapsed;
                if (gridCustomEventName != null)
                    gridCustomEventName.Visibility = Visibility.Visible;
                    
                // Hide OrderIndex, CacheType, and WorkOn
                if (txtOrderIndexLabel != null) txtOrderIndexLabel.Visibility = Visibility.Collapsed;
                if (txtOrderIndex != null) txtOrderIndex.Visibility = Visibility.Collapsed;
                if (txtCacheTypeLabel != null) txtCacheTypeLabel.Visibility = Visibility.Collapsed;
                if (cmbCacheType != null) cmbCacheType.Visibility = Visibility.Collapsed;
                if (txtWorkOnLabel != null) txtWorkOnLabel.Visibility = Visibility.Collapsed;
                if (cmbWorkOn != null) cmbWorkOn.Visibility = Visibility.Collapsed;
            }
        }

        // Event editor search/replace handlers
        [SupportedOSPlatform("windows6.1")]
        private void ReplaceInEventEditor_Click(object sender, RoutedEventArgs e)
        {
            var replacePanel = this.FindName("eventReplacePanel") as Border;
            if (replacePanel == null) return;

            replacePanel.Visibility = replacePanel.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;

            if (replacePanel.Visibility == Visibility.Visible)
            {
                var txtFind = this.FindName("txtEventFindText") as System.Windows.Controls.TextBox;
                txtFind?.Focus();
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void TxtEventFindText_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txtFind = sender as System.Windows.Controls.TextBox;
            var editor = this.FindName("txtEventSourceEditor") as ICSharpCode.AvalonEdit.TextEditor;
            
            if (editor == null || txtFind == null || string.IsNullOrEmpty(txtFind.Text))
                return;

            try
            {
                var searchText = txtFind.Text;
                var text = editor.Text;
                var index = text.IndexOf(searchText, 0, StringComparison.OrdinalIgnoreCase);

                if (index >= 0)
                {
                    editor.Select(index, searchText.Length);
                    editor.CaretOffset = index + searchText.Length;
                    editor.ScrollToLine(editor.Document.GetLineByOffset(index).LineNumber);
                }
            }
            catch { }
        }

        [SupportedOSPlatform("windows6.1")]
        private void TxtEventFindText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                e.Handled = true;
                EventFindNext_Click(sender, e);
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void EventFindNext_Click(object sender, RoutedEventArgs e)
        {
            var editor = this.FindName("txtEventSourceEditor") as ICSharpCode.AvalonEdit.TextEditor;
            var txtFind = this.FindName("txtEventFindText") as System.Windows.Controls.TextBox;
            
            if (editor == null || txtFind == null || string.IsNullOrEmpty(txtFind.Text))
                return;

            try
            {
                var searchText = txtFind.Text;
                var text = editor.Text;
                var startIndex = editor.CaretOffset;
                var index = text.IndexOf(searchText, startIndex, StringComparison.OrdinalIgnoreCase);

                if (index == -1)
                {
                    index = text.IndexOf(searchText, 0, StringComparison.OrdinalIgnoreCase);
                }

                if (index >= 0)
                {
                    editor.Select(index, searchText.Length);
                    editor.CaretOffset = index + searchText.Length;
                    editor.ScrollToLine(editor.Document.GetLineByOffset(index).LineNumber);
                }
                else
                {
                    MessageBox.Show($"Cannot find '{searchText}'", "Find", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to find text");
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void EventReplaceOne_Click(object sender, RoutedEventArgs e)
        {
            var editor = this.FindName("txtEventSourceEditor") as ICSharpCode.AvalonEdit.TextEditor;
            var txtFind = this.FindName("txtEventFindText") as System.Windows.Controls.TextBox;
            var txtReplace = this.FindName("txtEventReplaceText") as System.Windows.Controls.TextBox;
            
            if (editor == null || txtFind == null || txtReplace == null || string.IsNullOrEmpty(txtFind.Text))
                return;

            try
            {
                var searchText = txtFind.Text;
                var replaceText = txtReplace.Text;

                if (editor.SelectedText.Equals(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    var offset = editor.SelectionStart;
                    editor.Document.Replace(offset, editor.SelectionLength, replaceText);
                    editor.CaretOffset = offset + replaceText.Length;
                }

                EventFindNext_Click(sender, e);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to replace text");
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void EventReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            var editor = this.FindName("txtEventSourceEditor") as ICSharpCode.AvalonEdit.TextEditor;
            var txtFind = this.FindName("txtEventFindText") as System.Windows.Controls.TextBox;
            var txtReplace = this.FindName("txtEventReplaceText") as System.Windows.Controls.TextBox;
            
            if (editor == null || txtFind == null || txtReplace == null || string.IsNullOrEmpty(txtFind.Text))
                return;

            try
            {
                var searchText = txtFind.Text;
                var replaceText = txtReplace.Text;
                var text = editor.Text;
                var count = 0;
                var index = 0;
                var offset = 0;

                while ((index = text.IndexOf(searchText, index, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    editor.Document.Replace(index + offset, searchText.Length, replaceText);
                    offset += replaceText.Length - searchText.Length;
                    index += searchText.Length;
                    count++;
                }

                MessageBox.Show($"Replaced {count} occurrence(s)", "Replace All", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to replace all");
            }
        }

        private void CloseEventReplacePanel_Click(object sender, RoutedEventArgs e)
        {
            var replacePanel = this.FindName("eventReplacePanel") as Border;
            if (replacePanel != null)
            {
                replacePanel.Visibility = Visibility.Collapsed;
            }
        }
    }
}
