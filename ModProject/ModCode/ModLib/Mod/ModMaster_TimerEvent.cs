using System;
using UnityEngine;

namespace ModLib.Mod
{
    public abstract partial class ModMaster : MonoBehaviour
    {
        #region ModLib - Handlers
        public virtual void _OnTimeUpdate()
        {
            if (GameHelper.IsInGame() && CacheHelper.IsGameCacheLoaded())
            {
                try
                {
                    var stt = ModSettings.GetSettings<ModSettings>();

                    if (!stt.LoadGameBefore &&
                        !stt.LoadGame &&
                        !stt.LoadGameAfter &&
                        !stt.LoadGameFirst)
                    {
                        OnTimeUpdate();
                    }
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        public virtual void _OnFrameUpdate()
        {
            if (GameHelper.IsInGame() && CacheHelper.IsGameCacheLoaded())
            {
                try
                {
                    var stt = ModSettings.GetSettings<ModSettings>();

                    if (!stt.LoadGameBefore &&
                        !stt.LoadGame &&
                        !stt.LoadGameAfter &&
                        !stt.LoadGameFirst)
                    {
                        OnFrameUpdate();
                    }
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }
        #endregion

        #region ModLib - Events
        public virtual void OnTimeUpdate()
        {
            EventHelper.RunMinorEvents();
        }

        public virtual void OnFrameUpdate()
        {
            EventHelper.RunMinorEvents();
        }
        #endregion
    }
}