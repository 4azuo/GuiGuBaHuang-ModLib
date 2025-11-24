using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace ModCreator.Helpers
{
    /// <summary>
    /// Helper class for loading and accessing localized messages from messages.json.
    /// Use this class in C# code with Get() and GetFormat() methods.
    /// For XAML bindings, use UITextHelper instead.
    /// </summary>
    public static class MessageHelper
    {
        private static JObject _messagesRoot;
        private static readonly object _lock = new object();

        /// <summary>
        /// Load messages from the JSON file
        /// </summary>
        static MessageHelper()
        {
            LoadMessages();
        }

        /// <summary>
        /// Load or reload messages from messages.json
        /// </summary>
        public static void LoadMessages()
        {
            lock (_lock)
            {
                try
                {
                    var jsonPath = ModCreator.Constants.MessagesFilePath;
                    
                    if (!File.Exists(jsonPath))
                    {
                        throw new FileNotFoundException($"Message file not found: {jsonPath}");
                    }

                    var jsonContent = File.ReadAllText(jsonPath);
                    _messagesRoot = JsonConvert.DeserializeObject<JObject>(jsonContent);
                }
                catch (Exception ex)
                {
                    DebugHelper.ShowError(ex, "Error", $"Error loading messages: {ex.Message}");
                    _messagesRoot = new JObject();
                }
            }
        }

        /// <summary>
        /// Get a message by path (e.g., "Windows.MainWindow.AppTitle" or "Messages.Success.CreatedProject")
        /// </summary>
        /// <param name="path">The message path using dot notation</param>
        /// <returns>The message string, or the path itself if not found</returns>
        public static string Get(string path)
        {
            if (_messagesRoot == null)
            {
                LoadMessages();
            }

            try
            {
                var token = _messagesRoot.SelectToken(path.Replace(".", "."));
                if (token != null && token.Type == JTokenType.String)
                {
                    return token.Value<string>();
                }
            }
            catch
            {
                // Ignore exceptions and return default
            }

            return $"[{path}]"; // Return path in brackets if not found
        }

        /// <summary>
        /// Get a formatted message with parameters
        /// </summary>
        /// <param name="path">The message path using dot notation</param>
        /// <param name="args">Format arguments</param>
        /// <returns>The formatted message string</returns>
        public static string GetFormat(string path, params object[] args)
        {
            var message = Get(path);
            try
            {
                return string.Format(message, args);
            }
            catch (FormatException)
            {
                return message; // Return unformatted if formatting fails
            }
        }
    }
}
