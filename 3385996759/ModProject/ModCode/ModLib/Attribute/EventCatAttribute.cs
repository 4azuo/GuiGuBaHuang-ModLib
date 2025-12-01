using System;

namespace ModLib.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class EventCatAttribute : Attribute
    {
        public string Category { get; set; }

        public EventCatAttribute(string category)
        {
            Category = category;
        }
    }
}