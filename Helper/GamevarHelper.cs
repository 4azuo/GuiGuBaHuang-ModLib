using ModLib.Attributes;
using ModLib.Converter;
using ModLib.Mod;
using ModLib.Object;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for managing global game variables.
    /// Handles serialization and persistence of mod-specific game variables.
    /// </summary>
    [ActionCatIgn]
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

        /// <summary>
        /// Gets the settings filename for current player unit.
        /// </summary>
        /// <returns>Settings filename</returns>
        public static string GetSettingsFileName()
        {
            return $"{g.world.playerUnit.GetUnitId()}_configs.json";
        }

        /// <summary>
        /// Gets the settings folder path for this mod (creates if needed).
        /// </summary>
        /// <returns>Folder path</returns>
        public static string GetSettingsFolderName()
        {
            var p = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}Low\\guigugame\\guigubahuang\\mod\\{ModMaster.ModObj.ModId}\\";
            if (!Directory.Exists(p))
                Directory.CreateDirectory(p);
            return p;
        }

        /// <summary>
        /// Gets the full settings file path for current player.
        /// </summary>
        /// <returns>File path</returns>
        public static string GetSettingsFilePath()
        {
            return Path.Combine(GetSettingsFolderName(), GetSettingsFileName());
        }

        /// <summary>
        /// Loads game variables from file or creates new if not exists.
        /// </summary>
        /// <returns>Gamevar object</returns>
        public static Gamevar Load()
        {
            if (!File.Exists(GetSettingsFilePath()))
                return new Gamevar();
            return JsonConvert.DeserializeObject<Gamevar>(File.ReadAllText(GetSettingsFilePath()), JSON_SETTINGS);
        }

        /// <summary>
        /// Saves game variables to file.
        /// </summary>
        /// <param name="stt">Gamevar to save</param>
        public static void Save(this Gamevar stt)
        {
            File.WriteAllText(GetSettingsFilePath(), JsonConvert.SerializeObject(stt, JSON_SETTINGS));
        }
    }
}