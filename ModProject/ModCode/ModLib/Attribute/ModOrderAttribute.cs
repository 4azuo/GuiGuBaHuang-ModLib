using System;

namespace ModLib.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class ModOrderAttribute : Attribute
    {
        public string OrderFile { get; private set; }

        public ModOrderAttribute(string orderFile)
        {
            OrderFile = orderFile;
        }
    }
}