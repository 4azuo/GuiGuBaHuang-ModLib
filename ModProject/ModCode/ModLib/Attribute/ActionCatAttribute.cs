using System;

namespace ModLib.Attributes
{
    /// <summary>
    /// Groups helper methods into a category (used by ModCreator to list available actions).
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ActionCatAttribute : Attribute
    {
        public string Category { get; set; }

        /// <summary>
        /// Creates the attribute with the category name.
        /// </summary>
        public ActionCatAttribute(string category)
        {
            Category = category;
        }
    }
}