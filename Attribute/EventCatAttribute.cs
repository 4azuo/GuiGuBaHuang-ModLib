using System;

namespace ModLib.Attributes
{
    /// <summary>
    /// Groups ModEvent methods into a category (Timer, Map, Game, Battle) for ModCreator.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class EventCatAttribute : Attribute
    {
        public string Category { get; set; }

        /// <summary>
        /// Creates the attribute with the category name.
        /// </summary>
        public EventCatAttribute(string category)
        {
            Category = category;
        }
    }
}