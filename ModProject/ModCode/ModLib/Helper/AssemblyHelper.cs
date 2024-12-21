using System;
using System.Linq;
using ModLib.Mod;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public static class AssemblyHelper
{
    public static List<Assembly> GetAssembliesInChildren()
    {
        var rs = g.mod.allModPaths.ToArray().Select(x => GetModChildAssembly(x.t1)).ToList();
        rs.RemoveAll(item => item == null);
        return rs;
    }

    public static Assembly GetModLibAssembly()
    {
        return Assembly.GetAssembly(typeof(ModMaster));
    }

    public static Assembly GetModLibMainAssembly()
    {
        return Assembly.GetAssembly(ModMaster.ModObj.GetType());
    }

    public static Assembly GetModChildAssembly(string modId)
    {
        var assPath = $"{GetModChildPathRoot(modId)}\\ModCode\\dll\\Mod_{modId}.dll";
        if (!File.Exists(assPath))
            return null;
        return Assembly.LoadFrom(assPath);
    }

    public static string GetModChildPathRoot(string modId)
    {
        return g.mod.GetModPathRoot(modId);
    }

    public static string GetModChildPathSource(string modId)
    {
        return $"{GetModChildPathRoot(modId)}\\..\\..\\ModProject\\";
    }

    public static List<Type> GetLoadableTypes(this Assembly assembly)
    {
        if (assembly == null) 
            return new List<Type>();
        try
        {
            return assembly.GetTypes().ToList();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(t => t != null).ToList();
        }
    }
}