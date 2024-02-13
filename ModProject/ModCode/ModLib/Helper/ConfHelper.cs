using ModLib.Mod;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

public class ConfHelper
{
    private const string CONF_FOLDER = "ModConf";
    public static string GetConfFilePath(string fileName)
    {
        if (!Directory.Exists(GetConfFolderPath()))
        {
            var orgFile = $"{g.mod.GetModPathRoot(ModMaster.ModObj.ModId)}\\..\\..\\ModProject\\ModConf\\";
            FileHelper.CopyFilesRecursively(orgFile, GetConfFolderPath());
        }
        return Path.Combine(GetConfFolderPath(), fileName);
    }

    public static string GetConfFolderPath()
    {
        return Path.Combine(g.mod.GetModPathRoot(ModMaster.ModObj.ModId), CONF_FOLDER);
    }

    public static string ReadConfData(string fileName)
    {
        return File.ReadAllText(GetConfFilePath(fileName));
    }

    [Trace]
    private static void LoadConf(string filePath, string confName)
    {
        var confProp = g.conf.GetType().GetProperty(confName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        var confObj = confProp.GetValue(g.conf) as ConfBase;
        dynamic confList = confObj.GetType().GetProperty("allConfList", BindingFlags.Public | BindingFlags.Instance).GetValue(confObj);
        var confListItemType = confList.GetType().GetGenericArguments()[0] as Type;
        foreach (dynamic item in JsonConvert.DeserializeObject(ReadConfData(filePath)) as dynamic)
        {
            dynamic confItem;
            var id = ParseHelper.Parse<int>(item.id.ToString());
            var i = (int)confObj.GetItemIndex(id);
            if (i >= 0 && confList[i].id == id)
            {
                confItem = confList[i];
            }
            else
            {
                confItem = ParseHelper.ParseJson(item, confListItemType);
                confItem.isModExtend = true;
                confObj.AddItem(confItem);
                if (g.conf.localText.GetType() == confObj.GetType())
                {
                    g.conf.localText.allText.Add(confItem.key, confItem);
                }
            }
            foreach (var p in item)
            {
                foreach (var n in p.Name.ToString().Split('|'))
                {
                    ObjectHelper.SetValue(confItem, n, p.Value, true);
                }
            }
        }
    }

    public static void LoadCustomConf()
    {
        try
        {
            var assetFolder = GetConfFolderPath();
            foreach (var filePath in Directory.GetFiles(assetFolder, "*.json"))
            {
                var confName = Path.GetFileNameWithoutExtension(filePath).Split('_').Last();
                LoadConf(filePath, confName);
            }
        }
        catch(Exception ex)
        {
            DebugHelper.WriteLine(ex);
        }
    }
}