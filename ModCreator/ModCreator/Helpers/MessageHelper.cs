using Newtonsoft.Json;
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
        private static Dictionary<string, string> _messages;
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
                    _messages = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);
                }
                catch (Exception ex)
                {
                    DebugHelper.ShowError(ex, "Error", $"Error loading messages: {ex.Message}");
                    _messages = new Dictionary<string, string>();
                }
            }
        }

        /// <summary>
        /// Get a message by key
        /// </summary>
        /// <param name="key">The message key</param>
        /// <returns>The message string, or the key itself if not found</returns>
        public static string Get(string key)
        {
            if (_messages == null)
            {
                LoadMessages();
            }

            if (_messages.TryGetValue(key, out var value))
            {
                return value;
            }

            return $"[{key}]"; // Return key in brackets if not found
        }

        /// <summary>
        /// Get a formatted message with parameters
        /// </summary>
        /// <param name="key">The message key</param>
        /// <param name="args">Format arguments</param>
        /// <returns>The formatted message string</returns>
        public static string GetFormat(string key, params object[] args)
        {
            var message = Get(key);
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
