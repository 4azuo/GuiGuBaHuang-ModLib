using ModLib.Converter;
using ModLib.Mod;
using ModLib.Object;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public static class SttHelper
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
        return $"{g.world.playerUnit.GetUnitId()}_settings.json";
    }

    public static string GetSettingsFolderName()
    {
        return $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}Low\\guigugame\\guigubahuang\\mod\\{ModMaster.ModObj.ModName}\\";
    }

    public static string GetSettingsFilePath()
    {
        return Path.Combine(GetSettingsFolderName(), GetSettingsFileName());
    }

    public static T Load<T>() where T : InGameSettings
    {
        if (!File.Exists(GetSettingsFilePath()))
        {
            var customSettings = ModMaster.ModObj.GetType().GetCustomAttribute<InGameCustomSettingsAttribute>();
            if (string.IsNullOrEmpty(customSettings?.ConfCustomConfigFile))
                return null;
            var stt = JsonConvert.DeserializeObject<T>(ConfHelper.ReadConfData(customSettings.ConfCustomConfigFile), JSON_SETTINGS);
            stt.CustomConfigFile = customSettings.ConfCustomConfigFile;
            stt.CustomConfigVersion = customSettings.ConfCustomConfigVersion;
            return stt;
        }
        return JsonConvert.DeserializeObject<T>(File.ReadAllText(GetSettingsFilePath()), JSON_SETTINGS);
    }

    public static void DeleteOldStt()
    {
        File.Delete(GetSettingsFilePath());
    }

    public static void Save(this InGameSettings stt)
    {
        File.WriteAllText(GetSettingsFilePath(), JsonConvert.SerializeObject(stt, JSON_SETTINGS));
    }
}