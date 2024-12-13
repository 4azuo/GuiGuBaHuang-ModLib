﻿using UnityEngine;

namespace ModLib.Mod
{
    public abstract partial class ModMaster : MonoBehaviour
    {
        #region ModLib - Events
        //順番１
        public virtual void OnInitMod()
        {
            DebugHelper.WriteLine("Load mod.");
        }

        //順番２
        public virtual void OnInitConf()
        {
            DebugHelper.WriteLine("Load configs.");

            foreach (var mod in g.mod.allModPaths)
            {
                //copy new configs to debug folder
                CopyConf(mod.t1);
                //load configs
                ConfHelper.LoadCustomConf(mod.t1);
            }
        }

        //順番３
        public virtual void OnInitEObj()
        {
            DebugHelper.WriteLine("Load Enums.");
            LoadEnumObj(GameHelper.GetModLibAssembly());
            LoadEnumObj(GameHelper.GetModLibMainAssembly());
            foreach (var ass in GameHelper.GetAssembliesInChildren())
            {
                LoadEnumObj(ass);
            }
        }

        //順番４
        public virtual void OnLoadGlobals()
        {
            DebugHelper.WriteLine("Load global caches.");
            //load globals
            AddGlobalCaches();
        }

        //順番５
        public virtual void OnLoadGameVariables()
        {
            DebugHelper.WriteLine("Load ingame variables.");
            Gamevars = GamevarHelper.Load();
        }

        //順番６
        public virtual void OnLoadGameCaches()
        {
            DebugHelper.WriteLine("Load game caches.");
            //add game-cache
            AddGameCaches();
        }

        //順番７
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