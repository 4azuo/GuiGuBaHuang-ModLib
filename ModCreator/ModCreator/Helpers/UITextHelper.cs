using System;

namespace ModCreator.Helpers
{
    /// <summary>
    /// Helper class for accessing UI text messages in XAML bindings.
    /// Use this class for data binding in XAML files.
    /// </summary>
    public static class UITextHelper
    {
        // Direct property accessors for XAML binding
        public static string AppTitle => MessageHelper.Get("AppTitle");
        public static string Browse => MessageHelper.Get("Browse");
        public static string Cancel => MessageHelper.Get("Cancel");
        public static string Create => MessageHelper.Get("Create");
        public static string CreateNew => MessageHelper.Get("CreateNew");
        public static string EditInfo => MessageHelper.Get("EditInfo");
        public static string Error => MessageHelper.Get("Error");
        public static string ErrorOpeningFolder => MessageHelper.Get("ErrorOpeningFolder");
        public static string ErrorOpeningHelpWindow => MessageHelper.Get("ErrorOpeningHelpWindow");
        public static string ErrorOpeningAboutWindow => MessageHelper.Get("ErrorOpeningAboutWindow");
        public static string LoadedProjects => MessageHelper.Get("LoadedProjects");
        public static string NewProjectTitle => MessageHelper.Get("NewProjectTitle");
        public static string NoProjectSelected => MessageHelper.Get("NoProjectSelected");
        public static string OpenFolder => MessageHelper.Get("OpenFolder");
        public static string ProjectDeleted => MessageHelper.Get("ProjectDeleted");
        public static string ProjectDeleteMessage => MessageHelper.Get("ProjectDeleteMessage");
        public static string ProjectDeleteTitle => MessageHelper.Get("ProjectDeleteTitle");
        public static string ProjectDetails => MessageHelper.Get("ProjectDetails");
        public static string ProjectDetailsDescription => MessageHelper.Get("ProjectDetailsDescription");
        public static string ProjectDetailsId => MessageHelper.Get("ProjectDetailsId");
        public static string ProjectDetailsModId => MessageHelper.Get("ProjectDetailsModId");
        public static string ProjectDetailsName => MessageHelper.Get("ProjectDetailsName");
        public static string ProjectDetailsPath => MessageHelper.Get("ProjectDetailsPath");
        public static string ProjectFieldDescription => MessageHelper.Get("ProjectFieldDescription");
        public static string ProjectFieldName => MessageHelper.Get("ProjectFieldName");
        public static string ProjectList => MessageHelper.Get("ProjectList");
        public static string Ready => MessageHelper.Get("Ready");
        public static string Refresh => MessageHelper.Get("Refresh");
        public static string RemoveProject => MessageHelper.Get("RemoveProject");
        public static string Save => MessageHelper.Get("Save");
        public static string SearchPlaceholder => MessageHelper.Get("SearchPlaceholder");
        public static string Success => MessageHelper.Get("Success");
        public static string Workplace => MessageHelper.Get("Workplace");
        public static string WorkplacePath => MessageHelper.Get("WorkplacePath");
        public static string SelectWorkplace => MessageHelper.Get("SelectWorkplace");
        public static string About => MessageHelper.Get("About");
        public static string Help => MessageHelper.Get("Help");
        public static string PleaseSetWorkplacePath => MessageHelper.Get("PleaseSetWorkplacePath");
        public static string CreateNewProject => MessageHelper.Get("CreateNewProject");
        public static string RefreshList => MessageHelper.Get("RefreshList");
        public static string Actions => MessageHelper.Get("Actions");
        public static string Info => MessageHelper.Get("Info");
        public static string TotalProjects => MessageHelper.Get("TotalProjects");
        public static string Template => MessageHelper.Get("Template");
        public static string HeaderProjectName => MessageHelper.Get("HeaderProjectName");
        public static string HeaderDescription => MessageHelper.Get("HeaderDescription");
        public static string HeaderId => MessageHelper.Get("HeaderId");
        public static string HeaderState => MessageHelper.Get("HeaderState");
        public static string HeaderCreated => MessageHelper.Get("HeaderCreated");
        public static string HeaderModified => MessageHelper.Get("HeaderModified");
        public static string HeaderActions => MessageHelper.Get("HeaderActions");
        public static string GridOpenFolder => MessageHelper.Get("GridOpenFolder");
        public static string GridEditInfo => MessageHelper.Get("GridEditInfo");
        public static string GridDelete => MessageHelper.Get("GridDelete");
        public static string TooltipOpenFolder => MessageHelper.Get("TooltipOpenFolder");
        public static string TooltipEditInfo => MessageHelper.Get("TooltipEditInfo");
        public static string TooltipDelete => MessageHelper.Get("TooltipDelete");
        public static string Version => MessageHelper.Get("Version");
        public static string Author => MessageHelper.Get("Author");
        public static string Repository => MessageHelper.Get("Repository");
        public static string License => MessageHelper.Get("License");
        public static string Features => MessageHelper.Get("Features");
        public static string FeaturesList => MessageHelper.Get("FeaturesList");
        public static string Close => MessageHelper.Get("Close");
        public static string HelpTitle => MessageHelper.Get("HelpTitle");
        public static string HelpSubtitle => MessageHelper.Get("HelpSubtitle");
        public static string Topics => MessageHelper.Get("Topics");
        public static string AppName => MessageHelper.Get("AppName");
        public static string AppSubtitle => MessageHelper.Get("AppSubtitle");
        public static string ProjectIdLabel => MessageHelper.Get("ProjectIdLabel");
        public static string ProjectEditorTitle => MessageHelper.Get("ProjectEditorTitle");
        public static string EditModeLabel => MessageHelper.Get("EditModeLabel");
        public static string UIModeLabel => MessageHelper.Get("UIModeLabel");
        public static string CodeModeLabel => MessageHelper.Get("CodeModeLabel");
        public static string CodeModeNote => MessageHelper.Get("CodeModeNote");
        public static string ModEventsHeader => MessageHelper.Get("ModEventsHeader");
        public static string AddEvent => MessageHelper.Get("AddEvent");
        public static string EditEvent => MessageHelper.Get("EditEvent");
        public static string RemoveEvent => MessageHelper.Get("RemoveEvent");
        public static string EventEditorHeader => MessageHelper.Get("EventEditorHeader");
        public static string UIModeEditorTitle => MessageHelper.Get("UIModeEditorTitle");
        public static string UIModeEditorSubtitle => MessageHelper.Get("UIModeEditorSubtitle");
        public static string CodeModeEditorTitle => MessageHelper.Get("CodeModeEditorTitle");
        public static string CodeModeEditorSubtitle => MessageHelper.Get("CodeModeEditorSubtitle");
        public static string FeatureComingSoon => MessageHelper.Get("FeatureComingSoon");
        public static string CodeEditorPlaceholder => MessageHelper.Get("CodeEditorPlaceholder");
        public static string ErrorFillRequiredFields => MessageHelper.Get("ErrorFillRequiredFields");
        public static string ErrorWorkplaceNotSet => MessageHelper.Get("ErrorWorkplaceNotSet");
        public static string ErrorInitializingWindow => MessageHelper.Get("ErrorInitializingWindow");
        public static string InitializationError => MessageHelper.Get("InitializationError");
    }
}
