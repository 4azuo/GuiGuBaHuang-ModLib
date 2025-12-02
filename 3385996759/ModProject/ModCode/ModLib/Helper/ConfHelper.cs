using ModLib.Attributes;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnhollowerBaseLib;

namespace ModLib.Helper
{
    [ActionCat("Conf")]
    public static class ConfHelper
    {
        public const string CONF_FOLDER = "ModConf";
        public const char KEY_SPLIT_CHAR = '|';
        public const string ARRAY_EMPTY = "0";
        public const string KEY_ARRAY = "@";

        public static string GetConfFilePath(string modId, string fileName, string subFolder = "")
        {
            return Path.Combine(GetConfFolderPath(modId, subFolder), fileName);
        }

        public static string GetConfFolderPath(string modId, string subFolder = "")
        {
            return Path.Combine(AssemblyHelper.GetModPathRoot(modId), CONF_FOLDER, subFolder);
        }

        public static string ReadConfData(string modId, string fileName, string subFolder = "")
        {
            return File.ReadAllText(GetConfFilePath(modId, fileName, subFolder));
        }

        // [Trace]
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