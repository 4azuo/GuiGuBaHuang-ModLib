﻿using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnhollowerBaseLib;

public static class ConfHelper
{
    private const string CONF_FOLDER = "ModConf";
    private static readonly char KEY_SPLIT_CHAR = '|';
    private static readonly char[] VALUE_SPLIT_CHARS = new char[] { '|', '_', '-', ',', ';' };

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

    [Trace]
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
                    var ns = p.Name.ToString().Split(KEY_SPLIT_CHAR);
                    if (ns.Length > 1)
                    {
                        var v = p.Value;
                        foreach (var n in ns)
                        {
                            ObjectHelper.SetValue(confItem, n, v, true);
                            //ObjectHelper.SetValue(confItem, n, v, true, (Func<Type, object>)((t) =>
                            //{
                            //    if (t.IsSubclassOf(typeof(Il2CppArrayBase<>)))
                            //    {
                            //        dynamic v2 = null;
                            //        foreach (var s in VALUE_SPLIT_CHARS)
                            //        {
                            //            v2 = v.Split(s);
                            //            if (v2.Length > 1)
                            //                return v2;
                            //        }
                            //        return v2;
                            //    }
                            //    return null;
                            //}));
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
                    var confName = Path.GetFileNameWithoutExtension(filePath).Split('_').Last();
                    LoadConf(filePath, confName);
                }
            }
        }
        catch(Exception ex)
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