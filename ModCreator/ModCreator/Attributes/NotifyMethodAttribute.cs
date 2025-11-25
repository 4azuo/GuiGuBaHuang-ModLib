using System;

namespace ModCreator.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class NotifyMethodAttribute : Attribute
    {
        public string[] Methods { get; set; }

        public NotifyMethodAttribute(params string[] iMethods)
        {
            Methods = iMethods;
        }
    }
}