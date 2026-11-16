using System;

namespace ModLib.Attributes
{
    /// <summary>
    /// Hides a helper method/class from the ModCreator action list.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ActionCatIgnAttribute : Attribute
    {
    }
}