using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Reflection;
using System.Runtime.Versioning;
using System.Xml;

namespace ModCreator.Helpers
{
    /// <summary>
    /// Helper class for AvalonEdit operations
    /// </summary>
    public static class AvalonHelper
    {
        /// <summary>
        /// Load custom syntax highlighting from embedded resource
        /// </summary>
        /// <param name="editor">TextEditor instance</param>
        /// <param name="resourceName">Resource name (e.g., "ModCreator.Styles.Json.xshd")</param>
        [SupportedOSPlatform("windows6.1")]
        public static void LoadSyntaxHighlighting(TextEditor editor, string resourceName)
        {
            if (editor == null)
                throw new ArgumentNullException(nameof(editor));

            if (string.IsNullOrEmpty(resourceName))
                throw new ArgumentNullException(nameof(resourceName));

            try
            {
                var assembly = Assembly.GetExecutingAssembly();

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (var reader = new XmlTextReader(stream))
                        {
                            editor.SyntaxHighlighting = HighlightingLoader.Load(reader,
                                HighlightingManager.Instance);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Resource not found: {resourceName}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load syntax highlighting from {resourceName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Load JSON syntax highlighting
        /// </summary>
        /// <param name="editor">TextEditor instance</param>
        [SupportedOSPlatform("windows6.1")]
        public static void LoadJsonSyntaxHighlighting(TextEditor editor)
        {
            LoadSyntaxHighlighting(editor, "ModCreator.Styles.Json.xshd");
        }

        /// <summary>
        /// Load C# syntax highlighting
        /// </summary>
        /// <param name="editor">TextEditor instance</param>
        [SupportedOSPlatform("windows6.1")]
        public static void LoadCSharpSyntaxHighlighting(TextEditor editor)
        {
            if (editor == null)
                throw new ArgumentNullException(nameof(editor));

            try
            {
                // Use built-in C# highlighting
                editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load C# syntax highlighting: {ex.Message}");
            }
        }

        /// <summary>
        /// Clear syntax highlighting
        /// </summary>
        /// <param name="editor">TextEditor instance</param>
        [SupportedOSPlatform("windows6.1")]
        public static void ClearSyntaxHighlighting(TextEditor editor)
        {
            if (editor == null)
                throw new ArgumentNullException(nameof(editor));

            editor.SyntaxHighlighting = null;
        }
    }
}
