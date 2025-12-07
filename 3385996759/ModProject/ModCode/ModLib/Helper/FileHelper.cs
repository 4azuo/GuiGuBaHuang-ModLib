using ModLib.Attributes;
using ModLib.Object;
using Newtonsoft.Json;
using System.IO;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for file operations and validation.
    /// Provides utilities for checking file readability and working with cachable objects.
    /// </summary>
    [ActionCat("File")]
    public static class FileHelper
    {
        /// <summary>
        /// Checks if a file can be read and deserialized as the specified cachable type.
        /// </summary>
        /// <typeparam name="T">CachableObject type</typeparam>
        /// <param name="filePath">File path</param>
        /// <returns>True if readable</returns>
        public static bool IsReadable<T>(string filePath) where T : CachableObject
        {
            try
            {
                JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}