using ModCreator.Commons;
using ModCreator.Models;
using ModCreator.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModCreator.WindowData
{
    /// <summary>
    /// Helper window data layer - displays documentation
    /// </summary>
    public class HelperWindowData : CWindowData
    {
        #region Properties

        /// <summary>
        /// List of documentation items
        /// </summary>
        public List<DocItem> DocItems { get; set; } = new List<DocItem>();

        /// <summary>
        /// Selected documentation item
        /// </summary>
        [NotifyMethod(nameof(LoadDocContent))]
        public DocItem SelectedDoc { get; set; }

        /// <summary>
        /// Documentation content
        /// </summary>
        public string DocContent { get; set; }

        #endregion

        #region Lifecycle Methods

        public override void OnLoad()
        {
            base.OnLoad();
            LoadDocumentList();
        }

        #endregion

        #region Business Logic Methods

        /// <summary>
        /// Load list of documentation files
        /// </summary>
        public void LoadDocumentList()
        {
            try
            {
                var docsPath = ModCreator.Constants.DocsDir;

                if (Directory.Exists(docsPath))
                {
                    var mdFiles = Directory.GetFiles(docsPath, "*.md", SearchOption.AllDirectories);
                    DocItems = mdFiles.Select(f => new DocItem
                    {
                        Title = Path.GetFileNameWithoutExtension(f),
                        FilePath = f
                    }).OrderBy(d => d.Title).ToList();

                    if (DocItems.Count > 0)
                    {
                        SelectedDoc = DocItems[0];
                    }
                }
                else
                {
                    DocContent = "Documentation folder not found.\n\nExpected location: " + docsPath;
                }
            }
            catch (Exception ex)
            {
                DocContent = $"Error loading documentation:\n\n{ex.Message}";
            }
        }

        /// <summary>
        /// Load content of selected documentation file
        /// </summary>
        public void LoadDocContent(string propName = null)
        {
            if (SelectedDoc == null)
            {
                DocContent = string.Empty;
                return;
            }

            try
            {
                DocContent = File.ReadAllText(SelectedDoc.FilePath);
            }
            catch (Exception ex)
            {
                DocContent = $"Error loading file:\n\n{ex.Message}";
            }
        }

        #endregion
    }
}
