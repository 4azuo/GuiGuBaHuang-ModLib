using System;
using System.Linq;
using ModLib.Mod;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ModLib.Const;

public static class AssemblyHelper
{
    public static Assembly GetModLibAssembly()
    {
        return Assembly.GetAssembly(typeof(ModMaster));
    }

    public static Assembly GetModLibMainAssembly()
    {
        return Assembly.GetAssembly(ModMaster.ModObj.GetType());
    }

    public static Assembly GetModRootAssembly(string modId)
    {
        var assPath = GetModPathRootAssembly(modId);
        if (!File.Exists(assPath))
            return null;
        return Assembly.LoadFrom(assPath);
    }

    public static Assembly GetModLibChildAssembly(string modId)
    {
        var assPath = GetModLibPathChildAssembly(modId);
        if (!File.Exists(assPath))
            return null;
        return Assembly.LoadFrom(assPath);
    }

    public static string GetModPathRootAssembly(string modId)
    {
        return $"{GetModPathRootFolderAssembly(modId)}\\MOD_{modId}.dll";
    }

    public static string GetModLibPathChildAssembly(string modId)
    {
        return $"{GetModPathRootFolderAssembly(ModMaster.ModObj.ModId)}\\MOD_{modId}.dll";
    }

    public static string GetModPathRootFolderAssembly(string modId)
    {
        return $"{GetModPathRoot(modId)}\\ModCode\\dll\\";
    }

    public static string GetModPathRoot(string modId)
    {
        return g.mod.GetModPathRoot(modId);
    }

    public static string GetModPathSource(string modId)
    {
        return $"{GetModPathRoot(modId)}\\..\\..\\ModProject\\";
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

    public static void CopyAssemblies()
    {
        foreach (var mod in g.mod.allModPaths)
        {
            if (g.mod.IsLoadMod(mod.t1) && mod.t1 != ModMaster.ModObj.ModId)
            {
                foreach (var f in Directory.GetFiles(GetModPathRootFolderAssembly(mod.t1), "*.dll"))
                {
                    var fileName = Path.GetFileName(f);
                    if (fileName == ModLibConst.MODLIB_DLL)
                        continue;
                    File.Copy(f, $"{GetModPathRootFolderAssembly(ModMaster.ModObj.ModId)}\\{fileName}", true);
                }
            }
        }
    }
}