using MethodBoundaryAspect.Fody.Attributes;
using System;
using System.Linq;

[AttributeUsage(AttributeTargets.Class)]
public class InGameCustomSettingsAttribute : Attribute
{
    public string ConfCustomConfigFile { get; private set; }
    public string ConfCustomConfigVersion { get; private set; }

    public InGameCustomSettingsAttribute(string fileName, string version)
    {
        ConfCustomConfigFile = fileName;
        ConfCustomConfigVersion = version;
    }
}