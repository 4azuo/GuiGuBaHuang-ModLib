using ModLib.Converter;
using ModLib.Mod;
using ModLib.Object;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public static class GamevarHelper
{
    public static readonly JsonSerializerSettings JSON_SETTINGS = new JsonSerializerSettings
    {
        Formatting = Formatting.Indented,
        TypeNameHandling = TypeNameHandling.All,
        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        Converters = new List<JsonConverter>() { new EnumObjectConverter() },
        Error = (sender, args) =>
        {
            args.ErrorContext.Handled = true;
        },
    };

    public static string GetSettingsFileName()
    {
        return $"{g.world.playerUnit.GetUnitId()}_configs.json";
    }

    public static string GetSettingsFolderName()
    {
        return $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}Low\\guigugame\\guigubahuang\\mod\\{ModMaster.ModObj.ModName}\\";
    }

    public static string GetSettingsFilePath()
    {
        return Path.Combine(GetSettingsFolderName(), GetSettingsFileName());
    }

    public static Gamevar Load()
    {
        if (!File.Exists(GetSettingsFilePath()))
            return new Gamevar();
        return JsonConvert.DeserializeObject<Gamevar>(File.ReadAllText(GetSettingsFilePath()), JSON_SETTINGS);
    }

    public static void Save(this Gamevar stt)
    {
        File.WriteAllText(GetSettingsFilePath(), JsonConvert.SerializeObject(stt, JSON_SETTINGS));
    }
}