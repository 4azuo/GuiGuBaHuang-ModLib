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
        private System.Windows.Controls.Image titleImage;

        [SupportedOSPlatform("windows6.1")]
        private void SetupTitleImage()
        {
            try
            {
                LoadTitleImage();
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to setup title image");
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void LoadTitleImage()
        {
            var container = this.FindName("TitleImageContainer") as Border;
            if (container == null || WindowData?.Project == null) return;

            // Remove old image if exists
            if (titleImage != null)
            {
                container.Child = null;
                titleImage = null;
            }

            // Create new image if TitleImg exists
            if (!string.IsNullOrEmpty(WindowData.Project.TitleImg))
            {
                titleImage = new System.Windows.Controls.Image
                {
                    Stretch = System.Windows.Media.Stretch.Uniform
                };

                try
                {
                    var bitmap = BitmapHelper.LoadFromFile(WindowData.Project.TitleImg);
                    if (bitmap != null)
                    {
                        titleImage.Source = bitmap;
                        container.Child = titleImage;
                    }
                }
                catch
                {
                    // Ignore image load errors
                }
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void TitleImage_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "Select Title Image",
                    Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif"
                };

                if (dialog.ShowDialog() == true)
                {
                    WindowData.Project.TitleImg = dialog.FileName;
                    LoadTitleImage();
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to load title image");
            }
        }
        
        private void OpenProjectFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (WindowData?.Project != null && Directory.Exists(WindowData.Project.ProjectPath))
                {
                    var projectPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject");
                    if (Directory.Exists(projectPath))
                    {
                        System.Diagnostics.Process.Start("explorer.exe", projectPath);
                    }
                    else
                    {
                        System.Diagnostics.Process.Start("explorer.exe", WindowData.Project.ProjectPath);
                    }
                }
            }
            catch (Exception ex)
            {
                DebugHelper.ShowError(ex, MessageHelper.Get("Messages.Error.Title"), "Failed to open folder");
            }
        }
    }
}
