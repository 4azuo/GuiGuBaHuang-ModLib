using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.WindowData;
using ModCreator.Windows;
using System;
using System.Windows;

namespace ModCreator.Businesses
{
    /// <summary>
    /// Business logic for Global Variables operations
    /// </summary>
    public class GlobalVariablesBusiness
    {
        private readonly ProjectEditorWindowData _windowData;
        private readonly Window _owner;

        public GlobalVariablesBusiness(ProjectEditorWindowData windowData, Window owner)
        {
            _windowData = windowData;
            _owner = owner;
        }

        public void AddVariable()
        {
            try
            {
                var inputWindow = new InputWindow
                {
                    Owner = _owner
                };
                inputWindow.WindowData.WindowTitle = "Add Global Variable";
                inputWindow.WindowData.Label = "Variable Name:";
                inputWindow.WindowData.InputValue = "newVariable";

                if (inputWindow.ShowDialog() == true)
                {
                    var varName = inputWindow.WindowData.InputValue;
                    if (string.IsNullOrWhiteSpace(varName))
                    {
                        MessageBox.Show(
                            "Variable name cannot be empty!",
                            MessageHelper.Get("Messages.Warning.Title"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    var newVar = new GlobalVariable
                    {
                        Name = varName,
                        Type = "string",
                        Value = "\"\""
                    };

                    _windowData.GlobalVariables.Add(newVar);
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to add global variable");
            }
        }

        public void RemoveVariable(GlobalVariable variable)
        {
            if (variable == null)
                return;

            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete variable '{variable.Name}'?",
                    "Delete Variable",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _windowData.GlobalVariables.Remove(variable);
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to delete global variable");
            }
        }

        public void MoveVariableUp(GlobalVariable variable)
        {
            if (variable == null)
                return;

            try
            {
                var index = _windowData.GlobalVariables.IndexOf(variable);
                if (index > 0)
                {
                    _windowData.GlobalVariables.Move(index, index - 1);
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to move variable up");
            }
        }

        public void MoveVariableDown(GlobalVariable variable)
        {
            if (variable == null)
                return;

            try
            {
                var index = _windowData.GlobalVariables.IndexOf(variable);
                if (index < _windowData.GlobalVariables.Count - 1)
                {
                    _windowData.GlobalVariables.Move(index, index + 1);
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to move variable down");
            }
        }
    }
}
