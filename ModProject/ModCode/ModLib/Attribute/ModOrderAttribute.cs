using System;

namespace ModLib.Attributes
{
    /// <summary>
    /// Placed on a ModChild: names a ModConf JSON file mapping CacheId -&gt; OrderIndex for all events of the mod.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class ModOrderAttribute : Attribute
    {
        public string OrderFile { get; private set; }

        /// <summary>
        /// Creates the attribute with the order file name.
        /// </summary>
        public ModOrderAttribute(string orderFile)
        {
            OrderFile = orderFile;
        }
    }
}