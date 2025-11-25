using System;

namespace ModCreator.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NotifyDirectAttribute : Attribute
    {
        public bool WayDown { get; set; } = false;
    }
}