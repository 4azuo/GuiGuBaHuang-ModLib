using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

public static class ConfHelper
{
    private const string CONF_FOLDER = "ModConf";

    public static string GetConfFilePath(string modId, string fileName)
    {
        return Path.Combine(GetConfFolderPath(modId), fileName);
    }

    public static string GetConfFolderPath(string modId)
    {
        return Path.Combine(AssemblyHelper.GetModChildPathRoot(modId), CONF_FOLDER);
    }

    public static string ReadConfData(string modId, string fileName)
    {
        return File.ReadAllText(GetConfFilePath(modId, fileName));
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
                    foreach (var n in p.Name.ToString().Split('|'))
                    {
                        ObjectHelper.SetValue(confItem, n, p.Value, true);
                    }
                }
            }
        }
    }

    public static void LoadCustomConf(string modId)
    {
        try
        {
            var assetFolder = GetConfFolderPath(modId);
            if (Directory.Exists(assetFolder))
            {
                foreach (var filePath in Directory.GetFiles(assetFolder, "*.json"))
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
}