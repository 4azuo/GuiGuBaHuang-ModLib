using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.WindowData;
using System.Collections.Generic;
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
            if (WindowData?.Project == null) return;
            
            var editor = this.FindName("txtVariablesSource") as ICSharpCode.AvalonEdit.TextEditor;
            if (editor == null) return;

            var filePath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModCode", "ModMain", "Const", "ModCreatorChildVars.cs");
            
            editor.Text = File.Exists(filePath)
                ? File.ReadAllText(filePath)
                : "// File not found. Generate code first using 'Generate Code' button.";
        }

        private void AddVariable_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.GlobalVariables.Any(v => string.IsNullOrWhiteSpace(v.Name)))
            {
                MessageBox.Show("Please complete the existing variable with empty name before adding a new one!", MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            WindowData.GlobalVariables.Add(new GlobalVariable
            {
                Name = "",
                Type = "dynamic",
                Value = "",
                Description = ""
            });
        }

        private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit) return;

            var variable = e.Row.Item as GlobalVariable;
            if (variable == null || !string.IsNullOrWhiteSpace(variable.Name)) return;

            e.Cancel = true;
            MessageBox.Show("Variable name cannot be empty!", MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
            
            if (string.IsNullOrWhiteSpace(variable.Type) && string.IsNullOrWhiteSpace(variable.Value) && string.IsNullOrWhiteSpace(variable.Description))
                WindowData.GlobalVariables.Remove(variable);
        }

        private void GenerateVariablesCode_Click(object sender, RoutedEventArgs e)
        {
            WindowData.SaveGlobalVariables();

            if (WindowData.GlobalVariables == null || WindowData.GlobalVariables.Count == 0)
            {
                MessageBox.Show("No variables to generate. Please add variables first.", MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (WindowData.GlobalVariables.Any(v => string.IsNullOrWhiteSpace(v.Name)))
            {
                MessageBox.Show("All variables must have names. Please complete all variables before generating code.", MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var varTemplatePath = Path.Combine(WindowData.Project.ProjectPath, "VarTemplate.tmp");
            var varTemplateContentPath = Path.Combine(WindowData.Project.ProjectPath, "VarTemplateContent.tmp");

            if (!File.Exists(varTemplatePath))
            {
                MessageBox.Show($"Template file not found: {varTemplatePath}\n\nPlease ensure VarTemplate.tmp exists in the project root.", MessageHelper.Get("Messages.Error.Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!File.Exists(varTemplateContentPath))
            {
                MessageBox.Show($"Template file not found: {varTemplateContentPath}\n\nPlease ensure VarTemplateContent.tmp exists in the project root.", MessageHelper.Get("Messages.Error.Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var varTemplate = File.ReadAllText(varTemplatePath);
            var varTemplateContent = File.ReadAllText(varTemplateContentPath);

            var variableProperties = new System.Text.StringBuilder();
            foreach (var variable in WindowData.GlobalVariables)
            {
                if (string.IsNullOrWhiteSpace(variable.Name)) continue;

                var propertyCode = varTemplateContent
                    .Replace("#VARTYPE#", variable.Type ?? "string")
                    .Replace("#VARNAME#", variable.Name)
                    .Replace("#VARVALUE#", FormatVariableValue(variable));

                if (!string.IsNullOrWhiteSpace(variable.Description))
                    variableProperties.AppendLine($"        // {variable.Description}");

                variableProperties.AppendLine($"        {propertyCode.Trim()}");
            }

            var generatedCode = varTemplate.Replace("#VARIABLES#", variableProperties.ToString());
            var outputPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModCode", "ModMain", "Const", "ModCreatorChildVars.cs");
            
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            File.WriteAllText(outputPath, generatedCode);

            MessageBox.Show($"Variables code generated successfully!\n\nOutput file: {outputPath}", MessageHelper.Get("Messages.Info.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
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
            var variable = (sender as Button)?.Tag as GlobalVariable;
            if (variable == null) return;

            var clonedVar = new GlobalVariable
            {
                Name = variable.Name + "_copy",
                Type = variable.Type,
                Value = variable.Value,
                Description = variable.Description
            };

            var index = WindowData.GlobalVariables.IndexOf(variable);
            if (index >= 0)
                WindowData.GlobalVariables.Insert(index + 1, clonedVar);
            else
                WindowData.GlobalVariables.Add(clonedVar);
        }

        private void RemoveVariable_Click(object sender, RoutedEventArgs e)
        {
            var variable = (sender as Button)?.Tag as GlobalVariable;
            if (variable == null) return;

            var result = MessageBox.Show($"Are you sure you want to remove variable '{variable.Name}'?", "Confirm Remove", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
                WindowData.GlobalVariables.Remove(variable);
        }

        [SupportedOSPlatform("windows7.0")]
        private void ToggleGridView_Click(object sender, RoutedEventArgs e)
        {
            var dgVariables = this.FindName("dgGlobalVariables") as DataGrid;
            var txtSource = this.FindName("txtVariablesSource") as ICSharpCode.AvalonEdit.TextEditor;
            var btnGrid = this.FindName("btnGridView") as Button;
            var btnSource = this.FindName("btnSourceView") as Button;

            if (dgVariables == null || txtSource == null || btnGrid == null || btnSource == null) return;

            dgVariables.Visibility = Visibility.Visible;
            txtSource.Visibility = Visibility.Collapsed;
            
            btnGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2E5090"));
            btnGrid.Foreground = Brushes.White;
            btnSource.Background = Brushes.White;
            btnSource.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF666666"));
        }

        [SupportedOSPlatform("windows7.0")]
        private void ToggleSourceView_Click(object sender, RoutedEventArgs e)
        {
            var dgVariables = this.FindName("dgGlobalVariables") as DataGrid;
            var txtSource = this.FindName("txtVariablesSource") as ICSharpCode.AvalonEdit.TextEditor;
            var btnGrid = this.FindName("btnGridView") as Button;
            var btnSource = this.FindName("btnSourceView") as Button;

            if (dgVariables == null || txtSource == null || btnGrid == null || btnSource == null) return;

            LoadVariablesSourceFile();

            dgVariables.Visibility = Visibility.Collapsed;
            txtSource.Visibility = Visibility.Visible;
            
            btnSource.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2E5090"));
            btnSource.Foreground = Brushes.White;
            btnGrid.Background = Brushes.White;
            btnGrid.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF666666"));
        }
    }
}