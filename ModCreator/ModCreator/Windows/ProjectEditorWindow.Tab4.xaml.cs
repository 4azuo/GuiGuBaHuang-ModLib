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
        private void SetupVariablesSourceBinding()
        {
            var editor = this.FindName("txtVariablesSource") as ICSharpCode.AvalonEdit.TextEditor;
            if (editor == null) return;

            // Load C# syntax highlighting
            AvalonHelper.LoadCSharpSyntaxHighlighting(editor);
            
            // Load content from ModCreatorChildVars.cs if exists
            LoadVariablesSourceFile();
        }

        [SupportedOSPlatform("windows6.1")]
        private void LoadVariablesSourceFile()
        {
            try
            {
                if (WindowData?.Project == null) return;
                
                var editor = this.FindName("txtVariablesSource") as ICSharpCode.AvalonEdit.TextEditor;
                if (editor == null) return;

                var filePath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModCode", "ModMain", "Const", "ModCreatorChildVars.cs");
                
                if (File.Exists(filePath))
                {
                    editor.Text = File.ReadAllText(filePath);
                }
                else
                {
                    editor.Text = "// File not found. Generate code first using 'Generate Code' button.";
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to load variables source file");
            }
        }

        private void AddVariable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if there's already a variable with empty name
                if (WindowData.GlobalVariables.Any(v => string.IsNullOrWhiteSpace(v.Name)))
                {
                    MessageBox.Show(
                        "Please complete the existing variable with empty name before adding a new one!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Add new empty variable
                var newVar = new GlobalVariable
                {
                    Name = "",
                    Type = "dynamic",
                    Value = "",
                    Description = ""
                };
                WindowData.GlobalVariables.Add(newVar);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to add variable");
            }
        }

        private void DataGrid_RowEditEnding(object sender, System.Windows.Controls.DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var variable = e.Row.Item as GlobalVariable;
                if (variable != null && string.IsNullOrWhiteSpace(variable.Name))
                {
                    // Prevent committing if Name is empty
                    e.Cancel = true;
                    MessageBox.Show(
                        "Variable name cannot be empty!",
                        MessageHelper.Get("Messages.Warning.Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    
                    // If it's a new row with all empty fields, remove it
                    if (string.IsNullOrWhiteSpace(variable.Type) && 
                        string.IsNullOrWhiteSpace(variable.Value) && 
                        string.IsNullOrWhiteSpace(variable.Description))
                    {
                        WindowData.GlobalVariables.Remove(variable);
                    }
                }
            }
        }

        private void GenerateVariablesCode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Save variables first
                WindowData.SaveGlobalVariables();

                // Check if there are any variables to generate
                if (WindowData.GlobalVariables == null || WindowData.GlobalVariables.Count == 0)
                {
                    MessageBox.Show("No variables to generate. Please add variables first.", 
                        MessageHelper.Get("Messages.Warning.Title"), 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                    return;
                }

                // Validate that all variables have names
                var emptyVars = WindowData.GlobalVariables.Where(v => string.IsNullOrWhiteSpace(v.Name)).ToList();
                if (emptyVars.Any())
                {
                    MessageBox.Show("All variables must have names. Please complete all variables before generating code.", 
                        MessageHelper.Get("Messages.Warning.Title"), 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                    return;
                }

                // Read template files from project path
                var varTemplatePath = Path.Combine(WindowData.Project.ProjectPath, "VarTemplate.tmp");
                var varTemplateContentPath = Path.Combine(WindowData.Project.ProjectPath, "VarTemplateContent.tmp");

                if (!File.Exists(varTemplatePath))
                {
                    MessageBox.Show($"Template file not found: {varTemplatePath}\n\nPlease ensure VarTemplate.tmp exists in the project root.", 
                        MessageHelper.Get("Messages.Error.Title"), 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                    return;
                }

                if (!File.Exists(varTemplateContentPath))
                {
                    MessageBox.Show($"Template file not found: {varTemplateContentPath}\n\nPlease ensure VarTemplateContent.tmp exists in the project root.", 
                        MessageHelper.Get("Messages.Error.Title"), 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                    return;
                }

                // Read templates
                var varTemplate = File.ReadAllText(varTemplatePath);
                var varTemplateContent = File.ReadAllText(varTemplateContentPath);

                // Generate variable properties
                var variableProperties = new System.Text.StringBuilder();
                foreach (var variable in WindowData.GlobalVariables)
                {
                    if (string.IsNullOrWhiteSpace(variable.Name))
                        continue;

                    var propertyCode = varTemplateContent
                        .Replace("#VARTYPE#", variable.Type ?? "string")
                        .Replace("#VARNAME#", variable.Name)
                        .Replace("#VARVALUE#", FormatVariableValue(variable));

                    // Add description as comment if available
                    if (!string.IsNullOrWhiteSpace(variable.Description))
                    {
                        variableProperties.AppendLine($"        // {variable.Description}");
                    }

                    variableProperties.AppendLine($"        {propertyCode.Trim()}");
                }

                // Replace placeholder in main template
                var generatedCode = varTemplate.Replace("#VARIABLES#", variableProperties.ToString());

                // Save to file
                var outputPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModCode", "ModMain", "Const", "ModCreatorChildVars.cs");
                var outputDir = Path.GetDirectoryName(outputPath);
                
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                File.WriteAllText(outputPath, generatedCode);

                MessageBox.Show($"Variables code generated successfully!\n\nOutput file: {outputPath}", 
                    MessageHelper.Get("Messages.Info.Title"), 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), 
                    "Failed to generate variables code");
            }
        }

        /// <summary>
        /// Format variable value for code generation
        /// </summary>
        private string FormatVariableValue(GlobalVariable variable)
        {
            var varType = variable.Type?.ToLower();
            
            // Default values when Value is empty
            var defaultValues = new Dictionary<string, string>
            {
                ["string"] = "\"\"",
                ["bool"] = "false",
                ["int"] = "0",
                ["long"] = "0L",
                ["float"] = "0f",
                ["double"] = "0.0"
            };

            if (string.IsNullOrWhiteSpace(variable.Value))
            {
                if (defaultValues.TryGetValue(varType ?? "", out var defaultValue))
                    return defaultValue;
                return "null"; // Arrays and unknown types default to null
            }

            var value = variable.Value.Trim();
            
            // Format value based on type
            return varType switch
            {
                "string" => value.StartsWith("\"") && value.EndsWith("\"") ? value : $"\"{value}\"",
                "bool" => value.ToLower() is "true" or "false" ? value.ToLower() : "false",
                "float" => value.EndsWith("f") || value.EndsWith("F") ? value : $"{value}f",
                "long" => value.EndsWith("L") || value.EndsWith("l") ? value : $"{value}L",
                _ => value // Arrays, dynamic and other types return as-is
            };
        }

        private void CloneVariable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as System.Windows.Controls.Button;
                var variable = button?.Tag as GlobalVariable;
                
                if (variable == null) return;

                // Clone the variable
                var clonedVar = new GlobalVariable
                {
                    Name = variable.Name + "_copy",
                    Type = variable.Type,
                    Value = variable.Value,
                    Description = variable.Description
                };

                // Find index of original variable and insert after it
                var index = WindowData.GlobalVariables.IndexOf(variable);
                if (index >= 0)
                {
                    WindowData.GlobalVariables.Insert(index + 1, clonedVar);
                }
                else
                {
                    WindowData.GlobalVariables.Add(clonedVar);
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to clone variable");
            }
        }

        private void RemoveVariable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as System.Windows.Controls.Button;
                var variable = button?.Tag as GlobalVariable;
                
                if (variable == null) return;

                var result = MessageBox.Show(
                    $"Are you sure you want to remove variable '{variable.Name}'?",
                    "Confirm Remove",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    WindowData.GlobalVariables.Remove(variable);
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to remove variable");
            }
        }

        [SupportedOSPlatform("windows7.0")]
        private void ToggleGridView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dgVariables = this.FindName("dgGlobalVariables") as System.Windows.Controls.DataGrid;
                var txtSource = this.FindName("txtVariablesSource") as ICSharpCode.AvalonEdit.TextEditor;
                var btnGrid = this.FindName("btnGridView") as System.Windows.Controls.Button;
                var btnSource = this.FindName("btnSourceView") as System.Windows.Controls.Button;

                if (dgVariables != null && txtSource != null && btnGrid != null && btnSource != null)
                {
                    dgVariables.Visibility = Visibility.Visible;
                    txtSource.Visibility = Visibility.Collapsed;
                    
                    // Update button styles
                    btnGrid.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF2E5090"));
                    btnGrid.Foreground = System.Windows.Media.Brushes.White;
                    btnSource.Background = System.Windows.Media.Brushes.White;
                    btnSource.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF666666"));
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to switch to grid view");
            }
        }

        [SupportedOSPlatform("windows7.0")]
        private void ToggleSourceView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dgVariables = this.FindName("dgGlobalVariables") as System.Windows.Controls.DataGrid;
                var txtSource = this.FindName("txtVariablesSource") as ICSharpCode.AvalonEdit.TextEditor;
                var btnGrid = this.FindName("btnGridView") as System.Windows.Controls.Button;
                var btnSource = this.FindName("btnSourceView") as System.Windows.Controls.Button;

                if (dgVariables != null && txtSource != null && btnGrid != null && btnSource != null)
                {
                    // Load/Refresh C# source file
                    LoadVariablesSourceFile();

                    dgVariables.Visibility = Visibility.Collapsed;
                    txtSource.Visibility = Visibility.Visible;
                    
                    // Update button styles
                    btnSource.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF2E5090"));
                    btnSource.Foreground = System.Windows.Media.Brushes.White;
                    btnGrid.Background = System.Windows.Media.Brushes.White;
                    btnGrid.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF666666"));
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to switch to source view");
            }
        }
    }
}
