using System;
using ModLib.Const;
using ModLib.Object;
using Newtonsoft.Json;
using System.IO;
using UnityEngine;
using ModLib.Helper;

namespace ModLib.Mod
{
    /// <summary>
    /// ModMaster partial: load sequence steps.
    /// </summary>
    public abstract partial class ModMaster : MonoBehaviour
    {
        #region ModLib - Events
        //順番１
        /// <summary>
        /// Step 1: reads ModLib mod_configs.json and loads UI sample components.
        /// </summary>
        public virtual void OnInitMod()
        {
            DebugHelper.WriteLine("Load mod.");
            //load sample component first: a missing/invalid mod_configs.json must not break every custom UI (BUG-17)
            UISampleHelper.LoadUISampples();
            //load ModMaster configs (defaults when the file is missing/invalid)
            ModConfigs configs = null;
            try
            {
                configs = JsonConvert.DeserializeObject<ModConfigs>(File.ReadAllText(ConfHelper.GetConfFilePath(ModMaster.ModObj.ModId, ModLibConst.MODLIB_CONFIGS)));
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"ModLib {ModLibConst.MODLIB_CONFIGS} missing or invalid, default configs used: {ex.Message}");
            }
            SetModMasterConfigs(configs ?? new ModConfigs());
        }

        //順番２
        /// <summary>
        /// Step 2: for each loaded mod copies images/configs to the debug folder and loads its custom configs.
        /// </summary>
        public virtual void OnInitConf()
        {
            DebugHelper.WriteLine("Load configs.");

            ConfHelper.LoadedIDs.Clear();
            foreach (var mod in g.mod.allModPaths)
            {
                if (g.mod.IsLoadMod(mod.t1))
                {
                    //copy img
                    SpriteHelper.CopyImgs(mod.t1);
                    //copy new configs to debug folder
                    ConfHelper.CopyConfs(mod.t1);
                    //load configs
                    ConfHelper.LoadCustomConf(mod.t1);
                }
            }

            //log
            DebugHelper.Save();
        }

        //順番３
        /// <summary>
        /// Step 3: initializes EnumObject types of ModLib and every loaded mod.
        /// </summary>
        public virtual void OnInitEObj()
        {
            DebugHelper.WriteLine("Load Enums.");
            LoadEnumObj(AssemblyHelper.GetModLibAssembly());
            LoadEnumObj(AssemblyHelper.GetModLibMainAssembly());

            foreach (var mod in g.mod.allModPaths)
            {
                if (g.mod.IsLoadMod(mod.t1) && mod.t1 != ModMaster.ModObj.ModId)
                {
                    LoadEnumObj(AssemblyHelper.GetModRootAssembly(mod.t1));
                }
            }

            //log
            DebugHelper.Save();
        }

        //順番４
        /// <summary>
        /// Step 4: loads Global caches.
        /// </summary>
        public virtual void OnLoadGlobals()
        {
            DebugHelper.WriteLine("Load global caches.");
            //mod data of the saves deleted in the game (only saves recorded by SaveCleanupHelper.Register)
            if (ModLibConfigs?.CleanOrphanSaves ?? true)
            {
                try
                {
                    var folders = new System.Collections.Generic.HashSet<string>();
                    foreach (var cache in g.cache.allCache)
                        if (!string.IsNullOrEmpty(cache?.data?.folder)) folders.Add(cache.data.folder);
                    var removed = SaveCleanupHelper.CleanDeletedSaves(SaveCleanupHelper.GameDataPath, folders);
                    DebugHelper.WriteLine($"Save cleanup: {folders.Count} saves, removed {removed.Count}: {string.Join(", ", removed.ConvertAll(x => Path.GetFileName(x)))}");
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine($"Save cleanup skipped: {ex.Message}");
                }
            }
            //load globals
            AddGlobalCaches();
        }

        //順番５
        /// <summary>
        /// Step 5 (save game): loads Gamevar (first month/new game flags, current month).
        /// </summary>
        public virtual void OnLoadGameVariables()
        {
            DebugHelper.WriteLine("Load ingame variables.");
            //load game variables
            Gamevars = GamevarHelper.Load();
        }

        //順番６
        /// <summary>
        /// Step 6 (save game): builds the ParameterStore and loads Local caches.
        /// </summary>
        public virtual void OnLoadGameCaches()
        {
            DebugHelper.WriteLine("Load game caches.");
            //init data-caches
            RefreshParameterStore();
            //add game-caches
            AddGameCaches();
        }

        //順番７
        /// <summary>
        /// Step 7: clears game variables and all caches.
        /// </summary>
        public virtual void OnUnload()
        {
            DebugHelper.WriteLine("Unload.");
            //remove game vars
            Gamevars = null;
            //unload globals
            CacheHelper.Clear();
            //log
            DebugHelper.Save();
        }
        #endregion
    }
}