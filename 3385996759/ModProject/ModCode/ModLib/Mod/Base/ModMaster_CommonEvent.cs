using UnityEngine;

namespace ModLib.Mod
{
    public abstract partial class ModMaster : MonoBehaviour
    {
        #region ModLib - Events
        //順番１
        public virtual void OnInitMod()
        {
            DebugHelper.WriteLine("Load mod.");
            UISampleHelper.LoadUISampples();
        }

        //順番２
        public virtual void OnInitConf()
        {
            DebugHelper.WriteLine("Load configs.");

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
            //init data-caches
            RefreshParameterStore();
            //add game-caches
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