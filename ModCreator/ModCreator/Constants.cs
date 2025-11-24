namespace ModCreator
{
    /// <summary>
    /// Application constants
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Root directory of the project (GUIGUBAHUANG-MODLIB/ModCreator)
        /// </summary>
        public static readonly string RootDir = @"C:\git\GuiGuBaHuang-ModLib\";

        /// <summary>
        /// Documentation directory (.github/docs)
        /// </summary>
        public static readonly string DocsDir = System.IO.Path.Combine(RootDir, "..", ".github", "docs");

        /// <summary>
        /// Resources directory (bin/*/Resources)
        /// </summary>
        public static readonly string ResourcesDir = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Resources");

        /// <summary>
        /// Messages file path (bin/*/Resources/messages.json)
        /// </summary>
        public static readonly string MessagesFilePath = System.IO.Path.Combine(ResourcesDir, "messages.json");

        /// <summary>
        /// Project replacements configuration file path
        /// </summary>
        public static readonly string ProjectReplacementsFilePath = System.IO.Path.Combine(ResourcesDir, "new-project-replacements.json");

        /// <summary>
        /// Log directory (Logs in root)
        /// </summary>
        public static readonly string LogsDir = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "logs");

        /// <summary>
        /// Default workplace directory name
        /// </summary>
        public const string DEFAULT_WORKPLACE_DIR = "GuiGuBaHuang-ModProjects";
    }
}
