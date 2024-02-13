using MethodBoundaryAspect.Fody.Attributes;
using System;
using System.Linq;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class TraceIgnoreAttribute : Attribute
{
}