using System;

namespace ModLib.Attributes
{
    /// <summary>
    /// Hides an event method from the ModCreator event list.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class EventCatIgnAttribute : Attribute
    {
    }
}