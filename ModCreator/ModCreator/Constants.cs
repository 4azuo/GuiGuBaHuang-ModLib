using ModCreator.Helpers;
using System;
using System.IO;

namespace ModCreator
{
    /// <summary>
    /// Application constants
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Root directory of the project
        /// </summary>
        public static string RootDir => SettingHelper.Get("rootDir");

        /// <summary>
        /// Documentation directory (.github/docs)
        /// </summary>
        public static readonly string DocsDir = Path.GetFullPath(Path.Combine(RootDir, ".github", "docs"));

        /// <summary>
        /// Resources directory (bin/*/Resources)
        /// </summary>
        public static readonly string ResourcesDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources"));

        /// <summary>
        /// Messages file path (bin/*/Resources/messages.json)
        /// </summary>
        public static readonly string MessagesFilePath = Path.GetFullPath(Path.Combine(ResourcesDir, "messages.json"));

        /// <summary>
        /// Log directory (Logs in root)
        /// </summary>
        public static readonly string LogsDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs"));

        /// <summary>
        /// Default workplace directory name
        /// </summary>
        public const string DEFAULT_WORKPLACE_DIR = "GuiGuBaHuang-ModProjects";
    }
}
