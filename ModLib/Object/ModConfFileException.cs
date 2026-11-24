using System;

namespace ModLib.Object
{
    /// <summary>
    /// Error while reading a config file of a mod: carries the file path, so that a JSON error inside (broken file) is
    /// shown to the player with the file name (KnownErrorHelper). Make one with KnownErrorHelper.InConfFile.
    /// </summary>
    public class ModConfFileException : Exception
    {
        /// <summary>The config file.</summary>
        public string FilePath { get; }

        /// <param name="filePath">The config file</param>
        /// <param name="inner">The error while reading it</param>
        public ModConfFileException(string filePath, Exception inner) : base($"Error in the config file {filePath}: {inner?.Message}", inner)
        {
            FilePath = filePath;
        }
    }
}
