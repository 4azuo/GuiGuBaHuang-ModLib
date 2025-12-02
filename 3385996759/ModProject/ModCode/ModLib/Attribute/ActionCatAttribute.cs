using System;

namespace ModLib.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ActionCatAttribute : Attribute
    {
        public string Category { get; set; }

        public ActionCatAttribute(string category)
        {
            Category = category;
        }
    }
}