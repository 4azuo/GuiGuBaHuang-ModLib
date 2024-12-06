﻿using System.Reflection;
using UnityEngine;

namespace ModLib.Mod
{
    public abstract partial class ModMaster : MonoBehaviour
    {
        #region ModLib - Events
        //順番１
        [Trace]
        public virtual void OnInitMod()
        {
            AddGlobalCaches();
        }

        //順番２
        [Trace]
        public virtual void OnInitConf()
        {
            //load configs
            ConfHelper.LoadCustomConf();
        }

        //順番３
        [Trace]
        public virtual void OnInitEObj()
        {
            GameHelper.LoadEnumObj(GameHelper.GetModMasterAssembly());
            GameHelper.LoadEnumObj(GameHelper.GetModMainAssembly());
        }

        //順番４
        [Trace]
        public virtual void OnLoadGameSettings()
        {
        }

        //順番５
        [Trace]
        public virtual void OnLoadGameCaches()
        {
            //add game-cache
            AddGameCaches();
            //remove unuse global-cache
            RemoveUnuseGlobalCaches();
        }
        #endregion
    }
}