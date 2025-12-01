using System;

namespace ModLib.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ErrorIgnoreAttribute : Attribute
    {
    }
}