using MethodBoundaryAspect.Fody.Attributes;
using System;
using System.Linq;

[AttributeUsage(AttributeTargets.Class)]
public class InGameCustomSettingsAttribute : Attribute
{
    public string ConfCustomConfigFile { get; private set; }
    public int ConfCustomConfigVersion { get; private set; }

    public InGameCustomSettingsAttribute(string fileName, int version)
    {
        ConfCustomConfigFile = fileName;
        ConfCustomConfigVersion = version;
    }
}