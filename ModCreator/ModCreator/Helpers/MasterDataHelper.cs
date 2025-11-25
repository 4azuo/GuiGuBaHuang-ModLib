using ModCreator.Models;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace ModCreator.Helpers
{
    /// <summary>
    /// Helper class for loading master data from embedded resources
    /// </summary>
    public static class MasterDataHelper
    {
        /// <summary>
        /// Load supported image extensions from image-extensions.json
        /// </summary>
        /// <returns>List of ImageExtension objects</returns>
        public static List<ImageExtension> LoadImageExtensions()
        {
            var extensions = new List<ImageExtension>();
            var assembly = Assembly.GetExecutingAssembly();
            const string resourceName = "ModCreator.Resources.image-extensions.json";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var json = reader.ReadToEnd();
                        var loadedExtensions = JsonSerializer.Deserialize<List<ImageExtension>>(json);
                        if (loadedExtensions != null)
                        {
                            extensions.AddRange(loadedExtensions);
                        }
                    }
                }
            }

            return extensions;
        }

        /// <summary>
        /// Load variable types from var-types.json
        /// </summary>
        /// <returns>List of VarType objects</returns>
        public static List<VarType> LoadVarTypes()
        {
            var types = new List<VarType>();
            var assembly = Assembly.GetExecutingAssembly();
            const string resourceName = "ModCreator.Resources.var-types.json";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var json = reader.ReadToEnd();
                        var loadedTypes = JsonSerializer.Deserialize<List<VarType>>(json);
                        if (loadedTypes != null)
                        {
                            types.AddRange(loadedTypes);
                        }
                    }
                }
            }

            return types;
        }
    }
}
