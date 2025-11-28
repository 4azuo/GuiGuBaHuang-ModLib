using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.WindowData;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MessageBox = System.Windows.MessageBox;

namespace ModCreator.Windows
{
    public partial class ProjectEditorWindow : CWindow<ProjectEditorWindowData>
    {        
        [SupportedOSPlatform("windows6.1")]
        private void PopulateEventsComboBox()
        {
            var cmbEvents = this.FindName("cmbEvents") as ComboBox;
            if (cmbEvents == null || WindowData == null) return;

            cmbEvents.ItemsSource = WindowData.EventCategories.SelectMany(cat => cat.Events).ToList();
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
            var inputWindow = new InputWindow
            {
                Owner = this,
                WindowData = { WindowTitle = "Create New ModEvent", Label = "Class name:", InputValue = "NewModEvent" }
            };

            if (inputWindow.ShowDialog() != true) return;

            var className = inputWindow.WindowData.InputValue;
            
            if (string.IsNullOrWhiteSpace(className) || !System.Text.RegularExpressions.Regex.IsMatch(className, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            {
                MessageBox.Show("Invalid class name! Must start with letter and contain only letters, numbers, and underscores.", MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var modPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModCode", "ModMain", "Mod");
            Directory.CreateDirectory(modPath);

            var filePath = Path.Combine(modPath, $"{className}.cs");
            
            if (File.Exists(filePath))
            {
                MessageBox.Show($"A ModEvent with name '{className}' already exists!", MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var maxOrder = 0;
            if (WindowData.EventItems != null)
            {
                void TraverseItems(System.Collections.Generic.IEnumerable<FileItem> items)
                {
                    foreach (var item in items)
                    {
                        if (!item.IsFolder && item.FullPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                        {
                            var content = File.ReadAllText(item.FullPath);
                            var match = System.Text.RegularExpressions.Regex.Match(content, @"OrderIndex\s*=\s*(\d+)");
                            if (match.Success && int.TryParse(match.Groups[1].Value, out int order) && order > maxOrder)
                                maxOrder = order;
                        }
                        if (item.Children?.Any() == true)
                            TraverseItems(item.Children);
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

            File.WriteAllText(filePath, WindowData.GenerateModEventCode(newEvent));
            WindowData.LoadModEventFiles();
            
            MessageBox.Show($"ModEvent '{className}' created successfully!", MessageHelper.Get("Messages.Success.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CloneModEvent_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedModEvent == null) return;

            var newClassName = $"{WindowData.SelectedModEvent.FileName}_Copy";

            var inputWindow = new InputWindow
            {
                Owner = this,
                WindowData = { WindowTitle = "Clone ModEvent", Label = "New class name:", InputValue = newClassName }
            };

            if (inputWindow.ShowDialog() != true) return;

            newClassName = inputWindow.WindowData.InputValue;

            if (string.IsNullOrWhiteSpace(newClassName) || !System.Text.RegularExpressions.Regex.IsMatch(newClassName, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            {
                MessageBox.Show("Invalid class name!", MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var modPath = Path.GetDirectoryName(WindowData.SelectedModEvent.FilePath);
            var newFilePath = Path.Combine(modPath, $"{newClassName}.cs");

            if (File.Exists(newFilePath))
            {
                MessageBox.Show($"A ModEvent with name '{newClassName}' already exists!", MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var clonedEvent = new ModEventItem
            {
                OrderIndex = WindowData.SelectedModEvent.OrderIndex,
                CacheType = WindowData.SelectedModEvent.CacheType,
                WorkOn = WindowData.SelectedModEvent.WorkOn,
                SelectedEvent = WindowData.SelectedModEvent.SelectedEvent,
                ConditionLogic = WindowData.SelectedModEvent.ConditionLogic,
                FilePath = newFilePath
            };

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

            File.WriteAllText(newFilePath, WindowData.GenerateModEventCode(clonedEvent));
            WindowData.LoadModEventFiles();

            MessageBox.Show("ModEvent cloned successfully!", MessageHelper.Get("Messages.Success.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RenameModEvent_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedModEvent == null) return;

            var oldClassName = WindowData.SelectedModEvent.FileName;

            var inputWindow = new InputWindow
            {
                Owner = this,
                WindowData = { WindowTitle = "Rename ModEvent", Label = "New class name:", InputValue = oldClassName }
            };

            if (inputWindow.ShowDialog() != true) return;

            var newClassName = inputWindow.WindowData.InputValue;

            if (newClassName == oldClassName) return;

            if (string.IsNullOrWhiteSpace(newClassName) || !System.Text.RegularExpressions.Regex.IsMatch(newClassName, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            {
                MessageBox.Show("Invalid class name!", MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var oldFilePath = WindowData.SelectedModEvent.FilePath;
            var modPath = Path.GetDirectoryName(oldFilePath);
            var newFilePath = Path.Combine(modPath, $"{newClassName}.cs");

            if (File.Exists(newFilePath))
            {
                MessageBox.Show($"A ModEvent with name '{newClassName}' already exists!", MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            WindowData.SelectedModEvent.FilePath = newFilePath;
            WindowData.SaveModEvent();

            if (File.Exists(oldFilePath))
                File.Delete(oldFilePath);

            WindowData.LoadModEventFiles();

            MessageBox.Show("ModEvent renamed successfully!", MessageHelper.Get("Messages.Success.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteModEvent_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedModEvent == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete ModEvent '{WindowData.SelectedModEvent.FileName}'?", "Delete ModEvent", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            var filePath = WindowData.SelectedModEvent.FilePath;
            if (File.Exists(filePath))
                File.Delete(filePath);

            WindowData.LoadModEventFiles();
            WindowData.SelectedModEvent = null;

            MessageBox.Show("ModEvent deleted successfully!", MessageHelper.Get("Messages.Success.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveModEvent_Click(object sender, RoutedEventArgs e)
        {
            WindowData.SaveModEvent();
            MessageBox.Show("ModEvent saved successfully!", MessageHelper.Get("Messages.Success.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddEvent_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedModEvent == null) return;

            var selectWindow = new ModEventItemSelectWindow { Owner = this };
            selectWindow.WindowData.InitializeWithEvents(WindowData.EventCategories);

            if (selectWindow.ShowDialog() == true)
            {
                var selectedItem = selectWindow.WindowData.SelectedItem as EventInfoDisplay;
                if (selectedItem != null)
                    WindowData.SelectedModEvent.SelectedEvent = selectedItem.Name;
            }
        }

        private void AddCondition_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedModEvent == null) return;

            var selectWindow = new ModEventItemSelectWindow { Owner = this };
            selectWindow.WindowData.InitializeWithConditions(WindowData.AvailableConditions);

            if (selectWindow.ShowDialog() == true)
            {
                var conditionInfo = selectWindow.WindowData.SelectedItem as ConditionInfo;
                if (conditionInfo != null)
                {
                    WindowData.SelectedModEvent.Conditions.Add(new EventCondition
                    {
                        Name = conditionInfo.Name,
                        DisplayName = conditionInfo.DisplayName,
                        Description = conditionInfo.Description,
                        Code = conditionInfo.Code,
                        Order = WindowData.SelectedModEvent.Conditions.Count
                    });
                }
            }
        }

        private void RemoveCondition_Click(object sender, RoutedEventArgs e)
        {
            var condition = (sender as Button)?.Tag as EventCondition;
            if (condition == null || WindowData.SelectedModEvent == null) return;

            WindowData.SelectedModEvent.Conditions.Remove(condition);
        }

        private void AddAction_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedModEvent == null) return;

            var selectWindow = new ModEventItemSelectWindow { Owner = this };
            selectWindow.WindowData.InitializeWithActions(WindowData.AvailableActions);

            if (selectWindow.ShowDialog() == true)
            {
                var actionInfo = selectWindow.WindowData.SelectedItem as ActionInfo;
                if (actionInfo != null)
                {
                    WindowData.SelectedModEvent.Actions.Add(new EventAction
                    {
                        Name = actionInfo.Name,
                        DisplayName = actionInfo.DisplayName,
                        Description = actionInfo.Description,
                        Code = actionInfo.Code,
                        Order = WindowData.SelectedModEvent.Actions.Count
                    });
                }
            }
        }

        private void RemoveAction_Click(object sender, RoutedEventArgs e)
        {
            var action = (sender as Button)?.Tag as EventAction;
            if (action == null || WindowData.SelectedModEvent == null) return;

            WindowData.SelectedModEvent.Actions.Remove(action);
            
            for (int i = 0; i < WindowData.SelectedModEvent.Actions.Count; i++)
                WindowData.SelectedModEvent.Actions[i].Order = i;
        }

        private void OpenModEventFolder_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData?.Project == null) return;
            
            var modPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModCode", "ModMain", "Mod");
            Directory.CreateDirectory(modPath);
            System.Diagnostics.Process.Start("explorer.exe", modPath);
        }

        [SupportedOSPlatform("windows6.1")]
        private void ToggleGuiMode_Click(object sender, RoutedEventArgs e)
        {
            var guiPanel = this.FindName("guiModePanel") as Grid;
            var codePanel = this.FindName("codeModePanel") as Grid;
            var btnGui = this.FindName("btnGuiMode") as Button;
            var btnCode = this.FindName("btnCodeMode") as Button;

            if (guiPanel == null || codePanel == null || btnGui == null || btnCode == null) return;

            guiPanel.Visibility = Visibility.Visible;
            codePanel.Visibility = Visibility.Collapsed;
            WindowData.IsGuiMode = true;

            btnGui.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2E5090"));
            btnGui.Foreground = Brushes.White;
            btnCode.Background = Brushes.White;
            btnCode.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF666666"));
        }

        [SupportedOSPlatform("windows6.1")]
        private void ToggleCodeMode_Click(object sender, RoutedEventArgs e)
        {
            var guiPanel = this.FindName("guiModePanel") as Grid;
            var codePanel = this.FindName("codeModePanel") as Grid;
            var btnGui = this.FindName("btnGuiMode") as Button;
            var btnCode = this.FindName("btnCodeMode") as Button;

            if (guiPanel == null || codePanel == null || btnGui == null || btnCode == null) return;

            guiPanel.Visibility = Visibility.Collapsed;
            codePanel.Visibility = Visibility.Visible;
            WindowData.IsGuiMode = false;

            SetupEventSourceEditorBinding();

            btnCode.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2E5090"));
            btnCode.Foreground = Brushes.White;
            btnGui.Background = Brushes.White;
            btnGui.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF666666"));
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
            if (!(sender is ComboBox comboBox) || WindowData?.SelectedModEvent == null) return;

            var mode = comboBox.SelectedItem as string;
            var grpEventSelection = this.FindName("grpEventSelection") as GroupBox;
            var gridCustomEventName = this.FindName("gridCustomEventName") as Grid;
            
            // Get controls for OrderIndex, CacheType, and WorkOn
            var txtOrderIndexLabel = this.FindName("txtOrderIndexLabel") as TextBlock;
            var txtOrderIndex = this.FindName("txtOrderIndex") as TextBox;
            var txtCacheTypeLabel = this.FindName("txtCacheTypeLabel") as TextBlock;
            var cmbCacheType = this.FindName("cmbCacheType") as ComboBox;
            var txtWorkOnLabel = this.FindName("txtWorkOnLabel") as TextBlock;
            var cmbWorkOn = this.FindName("cmbWorkOn") as ComboBox;

            var isModEvent = mode == "ModEvent";
            
            if (grpEventSelection != null)
                grpEventSelection.Visibility = isModEvent ? Visibility.Visible : Visibility.Collapsed;
            if (gridCustomEventName != null)
                gridCustomEventName.Visibility = isModEvent ? Visibility.Collapsed : Visibility.Visible;
                
            var targetVisibility = isModEvent ? Visibility.Visible : Visibility.Collapsed;
            if (txtOrderIndexLabel != null) txtOrderIndexLabel.Visibility = targetVisibility;
            if (txtOrderIndex != null) txtOrderIndex.Visibility = targetVisibility;
            if (txtCacheTypeLabel != null) txtCacheTypeLabel.Visibility = targetVisibility;
            if (cmbCacheType != null) cmbCacheType.Visibility = targetVisibility;
            if (txtWorkOnLabel != null) txtWorkOnLabel.Visibility = targetVisibility;
            if (cmbWorkOn != null) cmbWorkOn.Visibility = targetVisibility;
        }

        // Event editor search/replace handlers
        [SupportedOSPlatform("windows6.1")]
        private void ReplaceInEventEditor_Click(object sender, RoutedEventArgs e)
        {
            var replacePanel = this.FindName("eventReplacePanel") as Border;
            if (replacePanel == null) return;

            replacePanel.Visibility = replacePanel.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;

            if (replacePanel.Visibility == Visibility.Visible)
                (this.FindName("txtEventFindText") as TextBox)?.Focus();
        }

        [SupportedOSPlatform("windows6.1")]
        private void TxtEventFindText_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txtFind = sender as TextBox;
            var editor = this.FindName("txtEventSourceEditor") as ICSharpCode.AvalonEdit.TextEditor;
            
            if (editor == null || txtFind == null || string.IsNullOrEmpty(txtFind.Text)) return;

            var index = editor.Text.IndexOf(txtFind.Text, 0, StringComparison.OrdinalIgnoreCase);

            if (index >= 0)
            {
                editor.Select(index, txtFind.Text.Length);
                editor.CaretOffset = index + txtFind.Text.Length;
                editor.ScrollToLine(editor.Document.GetLineByOffset(index).LineNumber);
            }
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
            var txtFind = this.FindName("txtEventFindText") as TextBox;
            
            if (editor == null || txtFind == null || string.IsNullOrEmpty(txtFind.Text)) return;

            var searchText = txtFind.Text;
            var index = editor.Text.IndexOf(searchText, editor.CaretOffset, StringComparison.OrdinalIgnoreCase);

            if (index == -1)
                index = editor.Text.IndexOf(searchText, 0, StringComparison.OrdinalIgnoreCase);

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

        [SupportedOSPlatform("windows6.1")]
        private void EventReplaceOne_Click(object sender, RoutedEventArgs e)
        {
            var editor = this.FindName("txtEventSourceEditor") as ICSharpCode.AvalonEdit.TextEditor;
            var txtFind = this.FindName("txtEventFindText") as TextBox;
            var txtReplace = this.FindName("txtEventReplaceText") as TextBox;
            
            if (editor == null || txtFind == null || txtReplace == null || string.IsNullOrEmpty(txtFind.Text)) return;

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

        [SupportedOSPlatform("windows6.1")]
        private void EventReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            var editor = this.FindName("txtEventSourceEditor") as ICSharpCode.AvalonEdit.TextEditor;
            var txtFind = this.FindName("txtEventFindText") as TextBox;
            var txtReplace = this.FindName("txtEventReplaceText") as TextBox;
            
            if (editor == null || txtFind == null || txtReplace == null || string.IsNullOrEmpty(txtFind.Text)) return;

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

        private void CloseEventReplacePanel_Click(object sender, RoutedEventArgs e)
        {
            (this.FindName("eventReplacePanel") as Border)?.SetValue(VisibilityProperty, Visibility.Collapsed);
        }
    }
}