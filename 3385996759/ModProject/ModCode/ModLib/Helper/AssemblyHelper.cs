using ModLib.Attributes;
using ModLib.Const;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for managing assemblies and mod file paths.
    /// Provides utilities for loading mod assemblies and accessing mod directories.
    /// </summary>
    [ActionCatIgn]
    public static class AssemblyHelper
    {
        /// <summary>
        /// Gets the ModLib framework assembly.
        /// </summary>
        /// <returns>The assembly containing ModMaster type</returns>
        public static Assembly GetModLibAssembly()
        {
            return Assembly.GetAssembly(typeof(ModMaster));
        }

        /// <summary>
        /// Gets the main assembly of the currently executing mod.
        /// </summary>
        /// <returns>The assembly of the current mod object</returns>
        public static Assembly GetModLibMainAssembly()
        {
            return Assembly.GetAssembly(ModMaster.ModObj.GetType());
        }

        /// <summary>
        /// Gets the root assembly for a specific mod by its ID.
        /// </summary>
        /// <param name="modId">The mod identifier</param>
        /// <returns>The loaded assembly, or null if not found</returns>
        public static Assembly GetModRootAssembly(string modId)
        {
            var assPath = GetModPathRootAssembly(modId);
            if (!File.Exists(assPath))
                return null;
            return Assembly.LoadFrom(assPath);
        }

        /// <summary>
        /// Gets a child assembly of ModLib for a specific mod.
        /// </summary>
        /// <param name="modId">The mod identifier</param>
        /// <returns>The loaded child assembly, or null if not found</returns>
        public static Assembly GetModLibChildAssembly(string modId)
        {
            var assPath = GetModLibPathChildAssembly(modId);
            if (!File.Exists(assPath))
                return null;
            return Assembly.LoadFrom(assPath);
        }

        /// <summary>
        /// Gets the full path to a mod's root assembly DLL file.
        /// </summary>
        /// <param name="modId">The mod identifier</param>
        /// <returns>Full path to MOD_{modId}.dll</returns>
        public static string GetModPathRootAssembly(string modId)
        {
            return $"{GetModPathRootFolderAssembly(modId)}\\MOD_{modId}.dll";
        }

        /// <summary>
        /// Gets the path to a ModLib child assembly.
        /// </summary>
        /// <param name="modId">The mod identifier</param>
        /// <returns>Path to the child assembly in the current mod's directory</returns>
        public static string GetModLibPathChildAssembly(string modId)
        {
            return $"{GetModPathRootFolderAssembly(ModMaster.ModObj.ModId)}\\MOD_{modId}.dll";
        }

        /// <summary>
        /// Gets the path to the mod's assembly folder (ModCode/dll/).
        /// </summary>
        /// <param name="modId">The mod identifier</param>
        /// <returns>Path to the assembly directory</returns>
        public static string GetModPathRootFolderAssembly(string modId)
        {
            return $"{GetModPathRoot(modId)}\\ModCode\\dll\\";
        }

        /// <summary>
        /// Gets the root path of a mod.
        /// </summary>
        /// <param name="modId">The mod identifier</param>
        /// <returns>Root directory path of the mod</returns>
        public static string GetModPathRoot(string modId)
        {
            return g.mod.GetModPathRoot(modId);
        }

        /// <summary>
        /// Gets the source project path of a mod (ModProject folder).
        /// </summary>
        /// <param name="modId">The mod identifier</param>
        /// <returns>Path to the ModProject directory</returns>
        public static string GetModPathSource(string modId)
        {
            return $"{GetModPathRoot(modId)}\\..\\..\\ModProject\\";
        }

        /// <summary>
        /// Safely gets all loadable types from an assembly, handling exceptions gracefully.
        /// </summary>
        /// <param name="assembly">The assembly to extract types from</param>
        /// <returns>List of types that could be loaded successfully</returns>
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

        /// <summary>
        /// Copies DLL assemblies from all loaded mods to the current mod's assembly folder.
        /// Excludes the ModLib core DLL itself.
        /// </summary>
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
}