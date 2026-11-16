using System;

namespace ModLib.Attributes
{
    /// <summary>
    /// No exception dialog for an event method (noisy UI/timer handlers): the error is still logged, once per distinct error.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ErrorIgnoreAttribute : Attribute
    {
    }
}