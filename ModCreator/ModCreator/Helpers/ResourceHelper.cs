using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace ModCreator.Helpers
{
    /// <summary>
    /// Helper class for loading master data from embedded resources
    /// </summary>
    public static class ResourceHelper
    {
        /// <summary>
        /// Read embedded resource as string
        /// </summary>
        /// <param name="resourceName">Full resource name (e.g., "ModCreator.Resources.file.json")</param>
        /// <returns>Resource content as string</returns>
        public static string ReadEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Read embedded resource and deserialize to specified type
        /// </summary>
        /// <typeparam name="T">Type to deserialize to</typeparam>
        /// <param name="resourceName">Full resource name (e.g., "ModCreator.Resources.file.json")</param>
        /// <returns>Deserialized object of type T</returns>
        public static T ReadEmbeddedResource<T>(string resourceName)
        {
            var json = ReadEmbeddedResource(resourceName);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
