using ModLib.Attributes;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnhollowerBaseLib;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for loading and managing custom configuration files.
    /// Handles loading, merging, and applying mod configurations from JSON files.
    /// </summary>
    [ActionCat("Conf")]
    public static class ConfHelper
    {
        /// <summary>
        /// Name of the configuration folder in mod directory.
        /// </summary>
        public const string CONF_FOLDER = "ModConf";
        
        /// <summary>
        /// Character used to split multiple property names in config keys.
        /// </summary>
        public const char KEY_SPLIT_CHAR = '|';
        
        /// <summary>
        /// String value representing an empty array in configs.
        /// </summary>
        public const string ARRAY_EMPTY = "0";
        
        /// <summary>
        /// Prefix character for array properties in config files.
        /// </summary>
        public const string KEY_ARRAY = "@";

        /// <summary>
        /// Gets the full path to a configuration file.
        /// </summary>
        /// <param name="modId">The mod identifier</param>
        /// <param name="fileName">The configuration filename</param>
        /// <param name="subFolder">Optional subfolder within ModConf</param>
        /// <returns>Full path to the configuration file</returns>
        public static string GetConfFilePath(string modId, string fileName, string subFolder = "")
        {
            return Path.Combine(GetConfFolderPath(modId, subFolder), fileName);
        }

        /// <summary>
        /// Gets the path to the configuration folder.
        /// </summary>
        /// <param name="modId">The mod identifier</param>
        /// <param name="subFolder">Optional subfolder within ModConf</param>
        /// <returns>Path to the configuration directory</returns>
        public static string GetConfFolderPath(string modId, string subFolder = "")
        {
            return Path.Combine(AssemblyHelper.GetModPathRoot(modId), CONF_FOLDER, subFolder);
        }

        /// <summary>
        /// Reads the content of a configuration file as text.
        /// </summary>
        /// <param name="modId">The mod identifier</param>
        /// <param name="fileName">The configuration filename</param>
        /// <param name="subFolder">Optional subfolder within ModConf</param>
        /// <returns>The file content as a string</returns>
        public static string ReadConfData(string modId, string fileName, string subFolder = "")
        {
            return File.ReadAllText(GetConfFilePath(modId, fileName, subFolder));
        }

        // [Trace]
        /// <summary>
        /// Loads a configuration file and merges it with the game's configuration.
        /// Supports adding, updating, and deleting configuration items.
        /// </summary>
        /// <param name="filePath">Path to the configuration file</param>
        /// <param name="confName">Name of the configuration class to modify</param>
        private static void LoadConf(string filePath, string confName)
        {
            //DebugHelper.WriteLine($"Edit Conf: {confName}");
            var confProp = g.conf.GetType().GetProperty(confName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (confProp == null)
                return;
            var confObj = confProp.GetValue(g.conf) as ConfBase;
            dynamic confList = confObj.GetType().GetProperty("allConfList", BindingFlags.Public | BindingFlags.Instance).GetValue(confObj);
            var confListItemType = confList.GetType().GetGenericArguments()[0] as Type;
            foreach (dynamic item in JsonConvert.DeserializeObject(File.ReadAllText(filePath)) as dynamic)
            {
                dynamic confItem;
                var id = ParseHelper.Parse<int>(item.id.ToString());
                var i = (int)confObj.GetItemIndex(id);
                if (i >= 0 && confList[i].id == id)
                {
                    //delete
                    if (item.DELETE == "1")
                    {
                        //DebugHelper.WriteLine($" - Delete({id}): {JsonConvert.SerializeObject(item)}");
                        confItem = null;
                        confList.Remove(confList[i]);
                        if (g.conf.localText.GetType() == confObj.GetType())
                            g.conf.localText.allText.Remove(confItem.key);
                    }
                    //update
                    else
                    {
                        //DebugHelper.WriteLine($" - Update({id}): {JsonConvert.SerializeObject(item)}");
                        confItem = confList[i];
                        //if (g.conf.localText.GetType() == confObj.GetType())
                        //    g.conf.localText.allText[confItem.key] = confItem;
                    }
                }
                //add new
                else
                {
                    //DebugHelper.WriteLine($" - Add({id}): {JsonConvert.SerializeObject(item)}");
                    confItem = ParseHelper.ParseJson(item, confListItemType);
                    confItem.isModExtend = true;
                    confObj.AddItem(confItem);
                    if (g.conf.localText.GetType() == confObj.GetType())
                        g.conf.localText.allText.Add(confItem.key, confItem);
                }
                if (confItem != null)
                {
                    foreach (var p in item)
                    {
                        string n = p.Name;
                        string v = p.Value;
                        if (n.StartsWith(KEY_ARRAY))
                        {
                            char splitChar = n[1];
                            string propName = n.Substring(2);
                            Type propType = confItem.GetType().GetProperty(propName).PropertyType;
                            Type eleType = propType.GetGenericArguments()[0];
                            if (typeof(int).IsAssignableFrom(eleType))
                            {
                                if (v == ARRAY_EMPTY)
                                {
                                    ObjectHelper.SetValue(confItem, propName, new Il2CppStructArray<int>(0), true);
                                }
                                else
                                {
                                    Il2CppStructArray<int> t = v.Split(new char[] { splitChar }).Select(x => x.Parse<int>()).ToArray();
                                    ObjectHelper.SetValue(confItem, propName, t, true);
                                }
                            }
                            else
                            if (typeof(string).IsAssignableFrom(eleType))
                            {
                                if (v == ARRAY_EMPTY)
                                {
                                    ObjectHelper.SetValue(confItem, propName, new Il2CppStringArray(0), true);
                                }
                                else
                                {
                                    Il2CppStringArray t = v.Split(new char[] { splitChar }).Select(x => x.Parse<string>()).ToArray();
                                    ObjectHelper.SetValue(confItem, propName, t, true);
                                }
                            }
                            else
                            if (confObj is ConfBattleUnitAttr && propName == "unitDatas")
                            {
                                ObjectHelper.SetValue(confItem, propName, new Il2CppReferenceArray<Il2CppStringArray>(0), true);
                            }
                        }
                        else
                        {
                            var ns = n.ToString().Split(KEY_SPLIT_CHAR);
                            if (ns.Length >= 1)
                            {
                                foreach (var ni in ns)
                                {
                                    ObjectHelper.SetValue(confItem, ni, v, true);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads all custom configuration files from a mod's config folder.
        /// Files are matched by pattern and processed automatically.
        /// </summary>
        /// <param name="modId">The mod identifier</param>
        /// <param name="subFolder">Optional subfolder within ModConf</param>
        /// <param name="searchPattern">File search pattern (default: *.json)</param>
        public static void LoadCustomConf(string modId, string subFolder = "", string searchPattern = "*.json")
        {
            try
            {
                //load conf
                var assetFolder = GetConfFolderPath(modId, subFolder);
                if (Directory.Exists(assetFolder))
                {
                    foreach (var filePath in Directory.GetFiles(assetFolder, searchPattern))
                    {
                        try
                        {
                            var confName = Path.GetFileNameWithoutExtension(filePath).Split('_').Last();
                            LoadConf(filePath, confName);
                        }
                        catch (Exception ex)
                        {
                            DebugHelper.WriteLine($"Failed to load conf file: {filePath}");
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine(ex);
            }
        }

        /// <summary>
        /// Copies configuration files from the mod's source project to the deployed location.
        /// Used during mod build/deployment process.
        /// </summary>
        /// <param name="modId">The mod identifier</param>
        public static void CopyConfs(string modId)
        {
            var srcFolder = Path.Combine(AssemblyHelper.GetModPathSource(modId), CONF_FOLDER);
            if (Directory.Exists(srcFolder))
            {
                var dstFolder = ConfHelper.GetConfFolderPath(modId);
                foreach (var srcFile in Directory.GetFiles(srcFolder, "*", SearchOption.AllDirectories))
                {
                    var dstFile = srcFile.Replace(srcFolder, dstFolder);
                    Directory.CreateDirectory(Path.GetDirectoryName(dstFile));
                    File.Copy(srcFile, dstFile, true);
                }
            }
        }
    }
}