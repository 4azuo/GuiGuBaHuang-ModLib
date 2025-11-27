using System;

namespace ModCreator.Helpers
{
    /// <summary>
    /// Main window UI texts helper for XAML bindings
    /// </summary>
    public static class MainWindowText
    {
        public static string AppTitle => MessageHelper.Get("Windows.MainWindow.AppTitle");
        public static string AppName => MessageHelper.Get("Windows.MainWindow.AppName");
        public static string AppSubtitle => MessageHelper.Get("Windows.MainWindow.AppSubtitle");
        public static string Help => MessageHelper.Get("Windows.MainWindow.Help");
        public static string About => MessageHelper.Get("Windows.MainWindow.About");
        public static string Workplace => MessageHelper.Get("Windows.MainWindow.Workplace");
        public static string Browse => MessageHelper.Get("Windows.MainWindow.Browse");
        public static string ProjectList => MessageHelper.Get("Windows.MainWindow.ProjectList");
        public static string CreateNewProject => MessageHelper.Get("Windows.MainWindow.CreateNewProject");
        public static string RefreshList => MessageHelper.Get("Windows.MainWindow.RefreshList");
        public static string SearchPlaceholder => MessageHelper.Get("Windows.MainWindow.SearchPlaceholder");
        public static string HeaderProjectName => MessageHelper.Get("Windows.MainWindow.HeaderProjectName");
        public static string HeaderDescription => MessageHelper.Get("Windows.MainWindow.HeaderDescription");
        public static string HeaderId => MessageHelper.Get("Windows.MainWindow.HeaderId");
        public static string HeaderState => MessageHelper.Get("Windows.MainWindow.HeaderState");
        public static string HeaderCreated => MessageHelper.Get("Windows.MainWindow.HeaderCreated");
        public static string HeaderModified => MessageHelper.Get("Windows.MainWindow.HeaderModified");
        public static string HeaderActions => MessageHelper.Get("Windows.MainWindow.HeaderActions");
        public static string GridOpenFolder => MessageHelper.Get("Windows.MainWindow.GridOpenFolder");
        public static string TooltipOpenFolder => MessageHelper.Get("Windows.MainWindow.TooltipOpenFolder");
        public static string GridEditInfo => MessageHelper.Get("Windows.MainWindow.GridEditInfo");
        public static string TooltipEditInfo => MessageHelper.Get("Windows.MainWindow.TooltipEditInfo");
        public static string GridDelete => MessageHelper.Get("Windows.MainWindow.GridDelete");
        public static string TooltipDelete => MessageHelper.Get("Windows.MainWindow.TooltipDelete");
        public static string ProjectDetails => MessageHelper.Get("Windows.MainWindow.ProjectDetails");
        public static string NoProjectSelected => MessageHelper.Get("Windows.MainWindow.NoProjectSelected");
        public static string ProjectDetailsName => MessageHelper.Get("Windows.MainWindow.ProjectDetailsName");
        public static string ProjectDetailsId => MessageHelper.Get("Windows.MainWindow.ProjectDetailsId");
        public static string ProjectDetailsPath => MessageHelper.Get("Windows.MainWindow.ProjectDetailsPath");
        public static string ProjectDetailsDescription => MessageHelper.Get("Windows.MainWindow.ProjectDetailsDescription");
        public static string ProjectDetailsAuthor => MessageHelper.Get("Windows.MainWindow.ProjectDetailsAuthor");
        public static string Actions => MessageHelper.Get("Windows.MainWindow.Actions");
        public static string OpenFolder => MessageHelper.Get("Windows.MainWindow.OpenFolder");
        public static string EditInfo => MessageHelper.Get("Windows.MainWindow.EditInfo");
        public static string RemoveProject => MessageHelper.Get("Windows.MainWindow.RemoveProject");
        public static string Info => MessageHelper.Get("Windows.MainWindow.Info");
        public static string TotalProjects => MessageHelper.Get("Windows.MainWindow.TotalProjects");
        public static string Template => MessageHelper.Get("Windows.MainWindow.Template");
    }

    /// <summary>
    /// About window UI texts helper for XAML bindings
    /// </summary>
    public static class AboutWindowText
    {
        public static string About => MessageHelper.Get("Windows.AboutWindow.About");
        public static string Version => MessageHelper.Get("Windows.AboutWindow.Version");
        public static string Author => MessageHelper.Get("Windows.AboutWindow.Author");
        public static string Repository => MessageHelper.Get("Windows.AboutWindow.Repository");
        public static string License => MessageHelper.Get("Windows.AboutWindow.License");
        public static string Features => MessageHelper.Get("Windows.AboutWindow.Features");
        public static string FeaturesList => MessageHelper.Get("Windows.AboutWindow.FeaturesList");
        public static string Close => MessageHelper.Get("Windows.AboutWindow.Close");
    }

    /// <summary>
    /// Help window UI texts helper for XAML bindings
    /// </summary>
    public static class HelpWindowText
    {
        public static string Help => MessageHelper.Get("Windows.HelpWindow.Help");
        public static string HelpTitle => MessageHelper.Get("Windows.HelpWindow.HelpTitle");
        public static string HelpSubtitle => MessageHelper.Get("Windows.HelpWindow.HelpSubtitle");
        public static string Topics => MessageHelper.Get("Windows.HelpWindow.Topics");
    }

    /// <summary>
    /// New project window UI texts helper for XAML bindings
    /// </summary>
    public static class NewProjectWindowText
    {
        public static string NewProjectTitle => MessageHelper.Get("Windows.NewProjectWindow.NewProjectTitle");
        public static string ProjectFieldName => MessageHelper.Get("Windows.NewProjectWindow.ProjectFieldName");
        public static string ProjectFieldDescription => MessageHelper.Get("Windows.NewProjectWindow.ProjectFieldDescription");
        public static string ProjectFieldAuthor => MessageHelper.Get("Windows.NewProjectWindow.ProjectFieldAuthor");
        public static string Create => MessageHelper.Get("Windows.NewProjectWindow.Create");
        public static string Cancel => MessageHelper.Get("Windows.NewProjectWindow.Cancel");
    }

    /// <summary>
    /// Project editor window UI texts helper for XAML bindings
    /// </summary>
    public static class ProjectEditorWindowText
    {
        public static string ProjectEditorTitle => MessageHelper.Get("Windows.ProjectEditorWindow.ProjectEditorTitle");
        public static string ProjectIdLabel => MessageHelper.Get("Windows.ProjectEditorWindow.ProjectIdLabel");
        public static string Help => MessageHelper.Get("Windows.ProjectEditorWindow.Help");
        public static string Save => MessageHelper.Get("Windows.ProjectEditorWindow.Save");
        public static string Cancel => MessageHelper.Get("Windows.ProjectEditorWindow.Cancel");
        public static string TabProjectInfo => MessageHelper.Get("Windows.ProjectEditorWindow.TabProjectInfo");
        public static string ProjectDetailsId => MessageHelper.Get("Windows.ProjectEditorWindow.ProjectDetailsId");
        public static string ProjectDetailsPath => MessageHelper.Get("Windows.ProjectEditorWindow.ProjectDetailsPath");
        public static string ProjectDetailsDescription => MessageHelper.Get("Windows.ProjectEditorWindow.ProjectDetailsDescription");
        public static string ProjectDetailsAuthor => MessageHelper.Get("Windows.ProjectEditorWindow.ProjectDetailsAuthor");
        public static string ProjectDetailsTitleImg => MessageHelper.Get("Windows.ProjectEditorWindow.ProjectDetailsTitleImg");
        public static string HeaderCreated => MessageHelper.Get("Windows.ProjectEditorWindow.HeaderCreated");
        public static string HeaderModified => MessageHelper.Get("Windows.ProjectEditorWindow.HeaderModified");
        public static string HeaderState => MessageHelper.Get("Windows.ProjectEditorWindow.HeaderState");
        public static string TabModConf => MessageHelper.Get("Windows.ProjectEditorWindow.TabModConf");
        public static string ConfFiles => MessageHelper.Get("Windows.ProjectEditorWindow.ConfFiles");
        public static string CreateFolder => MessageHelper.Get("Windows.ProjectEditorWindow.CreateFolder");
        public static string DeleteFolder => MessageHelper.Get("Windows.ProjectEditorWindow.DeleteFolder");
        public static string AddConf => MessageHelper.Get("Windows.ProjectEditorWindow.AddConf");
        public static string CloneConf => MessageHelper.Get("Windows.ProjectEditorWindow.CloneConf");
        public static string RenameConf => MessageHelper.Get("Windows.ProjectEditorWindow.RenameConf");
        public static string RemoveConf => MessageHelper.Get("Windows.ProjectEditorWindow.RemoveConf");
        public static string JsonEditor => MessageHelper.Get("Windows.ProjectEditorWindow.JsonEditor");
        public static string TabModImg => MessageHelper.Get("Windows.ProjectEditorWindow.TabModImg");
        public static string ImageFiles => MessageHelper.Get("Windows.ProjectEditorWindow.ImageFiles");
        public static string ImportImage => MessageHelper.Get("Windows.ProjectEditorWindow.ImportImage");
        public static string ExportImage => MessageHelper.Get("Windows.ProjectEditorWindow.ExportImage");
        public static string RemoveImage => MessageHelper.Get("Windows.ProjectEditorWindow.RemoveImage");
        public static string ImagePreview => MessageHelper.Get("Windows.ProjectEditorWindow.ImagePreview");
        public static string TabGlobalVariables => MessageHelper.Get("Windows.ProjectEditorWindow.TabGlobalVariables");
        public static string AddVariable => MessageHelper.Get("Windows.ProjectEditorWindow.AddVariable");
        public static string VariableName => MessageHelper.Get("Windows.ProjectEditorWindow.VariableName");
        public static string VariableType => MessageHelper.Get("Windows.ProjectEditorWindow.VariableType");
        public static string VariableTypeTooltip => MessageHelper.Get("Windows.ProjectEditorWindow.VariableTypeTooltip");
        public static string VariableValue => MessageHelper.Get("Windows.ProjectEditorWindow.VariableValue");
        public static string VariableDescription => MessageHelper.Get("Windows.ProjectEditorWindow.VariableDescription");
        public static string HeaderActions => MessageHelper.Get("Windows.ProjectEditorWindow.HeaderActions");
        public static string GenerateCode => MessageHelper.Get("Windows.ProjectEditorWindow.GenerateCode");
        public static string GenerateCodeNote => MessageHelper.Get("Windows.ProjectEditorWindow.GenerateCodeNote");
        public static string TabModEvent => MessageHelper.Get("Windows.ProjectEditorWindow.TabModEvent");
        public static string FeatureComingSoon => MessageHelper.Get("Windows.ProjectEditorWindow.FeatureComingSoon");
        public static string OpenInExplorer => MessageHelper.Get("Windows.ProjectEditorWindow.OpenInExplorer");
        public static string Refresh => MessageHelper.Get("Windows.ProjectEditorWindow.Refresh");
    }

    /// <summary>
    /// Add configuration window UI texts helper for XAML bindings
    /// </summary>
    public static class AddConfWindowText
    {
        public static string AddConfTitle => MessageHelper.Get("Windows.AddConfWindow.AddConfTitle");
        public static string SelectConf => MessageHelper.Get("Windows.AddConfWindow.SelectConf");
        public static string Prefix => MessageHelper.Get("Windows.AddConfWindow.Prefix");
        public static string Description => MessageHelper.Get("Windows.AddConfWindow.Description");
        public static string Add => MessageHelper.Get("Windows.AddConfWindow.Add");
        public static string Cancel => MessageHelper.Get("Windows.AddConfWindow.Cancel");
        public static string SearchPlaceholder => MessageHelper.Get("Windows.AddConfWindow.SearchPlaceholder");
    }
}
