﻿using EGameTypeData;
using ModLib.Object;
using System;
using System.Linq;
using UnityEngine;

namespace ModLib.Mod
{
    public abstract partial class ModMaster : MonoBehaviour
    {
        private bool initMod = false;
        private bool loadGlobal = false;

        #region ModLib - Handlers
        //public virtual void _OnTownAuctionStart(ETypeData e)
        //{
        //    CallEvents("OnTownAuctionStart", true, false);
        //}

        public virtual void _OnOpenUIStart(ETypeData e)
        {
            CallEvents<OpenUIStart>("OnOpenUIStart", e, false, false);
        }

        public virtual void _OnOpenUIEnd(ETypeData e)
        {
            CallEvents<OpenUIEnd>("OnOpenUIEnd", e, false, false);
        }

        public virtual void _OnCloseUIStart(ETypeData e)
        {
            CallEvents<CloseUIStart>("OnCloseUIStart", e, false, false);
        }

        public virtual void _OnCloseUIEnd(ETypeData e)
        {
            CallEvents<CloseUIEnd>("OnCloseUIEnd", e, false, false);
        }

        public virtual void _OnInitWorld(ETypeData e)
        {
            CallEvents<ETypeData>("OnInitWorld", e, true, false);
        }

        public virtual void _OnLoadSceneStart(ETypeData e)
        {
            if (!initMod)
            {
                DebugHelper.WriteLine("Load configs.");
                CallEvents("OnInitConf");
                CallEvents("OnInitEObj");
                DebugHelper.Save();
                initMod = true;
            }

            if (!loadGlobal)
            {
                DebugHelper.WriteLine("Load globals.");
                CallEvents("OnLoadGlobal");
                loadGlobal = true;
            }

            if (GameHelper.IsInGame())
            {
                if (InGameSettings.LoadNewGame)
                {
                    DebugHelper.WriteLine("OnLoadNewGame");
                    CallEvents("OnLoadNewGame");
                    InGameSettings.LoadNewGame = false;
                }

                if (InGameSettings.LoadGameBefore)
                {
                    DebugHelper.WriteLine("OnLoadGameBefore");
                    CallEvents("OnLoadGameBefore");
                    InGameSettings.LoadGameBefore = false;
                }

                if (InGameSettings.LoadGame)
                {
                    DebugHelper.WriteLine("OnLoadGame");
                    CallEvents("OnLoadGame");
                    InGameSettings.LoadGame = false;
                }

                if (InGameSettings.LoadGameAfter)
                {
                    DebugHelper.WriteLine("OnLoadGameAfter");
                    CallEvents("OnLoadGameAfter");
                    InGameSettings.LoadGameAfter = false;
                }
            }
            else
            {
                CacheHelper.ClearGameCache();
            }

            CallEvents<LoadSceneStart>("OnLoadSceneStart", e, true, false);
        }

        public virtual void _OnLoadScene(ETypeData e)
        {
            CallEvents<LoadScene>("OnLoadScene", e, true, false);
        }

        public virtual void _OnIntoWorld(ETypeData e)
        {
            CallEvents<ETypeData>("OnIntoWorld", e, true, false);
        }

        public virtual void _OnSave(ETypeData e)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    if (InGameSettings.CurMonth != g.game.world.run.roundMonth)
                    {
                        InGameSettings.CurMonth = g.game.world.run.roundMonth;

                        //first month
                        if (InGameSettings.LoadFirstMonth)
                        {
                            DebugHelper.WriteLine("OnFirstMonth");
                            OnFirstMonth();
                            InGameSettings.LoadFirstMonth = false;
                        }
                        //monthly
                        OnMonthly();
                        //yearly
                        if (g.world.run.roundMonth % 12 == 0)
                        {
                            OnYearly();
                        }
                    }

                    //save log
                    DebugHelper.WriteLine($"↑↑↑↑↑↑↑↑↑{GameHelper.GetGameYear()}年{GameHelper.GetGameMonth()}月{GameHelper.GetGameDay()}日↑↑↑↑↑↑↑↑↑");

                    //save
                    OnSave(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                    DebugHelper.Save();
                }
            }
        }

        public virtual void _OnOpenDrama(ETypeData e)
        {
            CallEvents<OpenDrama>("OnOpenDrama", e, true, false);
        }

        public virtual void _OnOpenNPCInfoUI(ETypeData e)
        {
            CallEvents<OpenNPCInfoUI>("OnOpenNPCInfoUI", e, true, false);
        }

        public virtual void _OnTaskAdd(ETypeData e)
        {
            CallEvents<TaskAdd>("OnTaskAdd", e, true, false);
        }

        public virtual void _OnTaskComplete(ETypeData e)
        {
            CallEvents<TaskComplete>("OnTaskComplete", e, true, false);
        }

        public virtual void _OnTaskFail(ETypeData e)
        {
            CallEvents<TaskFail>("OnTaskFail", e, true, false);
        }

        public virtual void _OnTaskGive(ETypeData e)
        {
            CallEvents<TaskGive>("OnTaskGive", e, true, false);
        }

        public virtual void _OnTaskOverl(ETypeData e)
        {
            CallEvents<TaskOverl>("OnTaskOverl", e, true, false);
        }

        public virtual void _OnUnitSetGrade(ETypeData e)
        {
            CallEvents<ETypeData>("OnUnitSetGrade", e, true, false);
        }

        public virtual void _OnUnitSetHeartState(ETypeData e)
        {
            CallEvents<ETypeData>("OnUnitSetHeartState", e, true, false);
        }

        public virtual void _OnWorldRunStart()
        {
            //start run
            CallEvents("OnWorldRunStart", true, false);
        }

        public virtual void _OnWorldRunEnd()
        {
            //end run
            DebugHelper.WriteLine($"↓↓↓↓↓↓↓↓↓{GameHelper.GetGameYear()}年{GameHelper.GetGameMonth()}月{GameHelper.GetGameDay()}日↓↓↓↓↓↓↓↓↓");
            CallEvents("OnWorldRunEnd", true, false);
        }
        #endregion

        #region ModLib - Events
        public virtual void OnOpenUIStart(OpenUIStart e)
        {
            DebugHelper.WriteLine(e.uiType.uiName);
            DebugHelper.WriteLine(string.Join(", ", EventHelper.GetEvents().Select(x => x.CacheId)));
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnOpenUIEnd(OpenUIEnd e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnCloseUIStart(CloseUIStart e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnCloseUIEnd(CloseUIEnd e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnLoadGlobal()
        {
            EventHelper.RunMinorEvents();
        }

        public virtual void OnLoadNewGame()
        {
            EventHelper.RunMinorEvents();
        }

        public virtual void OnLoadGameBefore()
        {
            EventHelper.RunMinorEvents();
        }

        public virtual void OnLoadGame()
        {
            EventHelper.RunMinorEvents();
        }

        public virtual void OnLoadGameAfter()
        {
            EventHelper.RunMinorEvents();
        }

        public virtual void OnInitWorld(ETypeData e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnLoadScene(LoadScene e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnLoadSceneStart(LoadSceneStart e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnIntoWorld(ETypeData e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnFirstMonth()
        {
            EventHelper.RunMinorEvents();
        }

        public virtual void OnYearly()
        {
            EventHelper.RunMinorEvents();
        }

        public virtual void OnMonthly()
        {
            EventHelper.RunMinorEvents();
        }

        public virtual void OnSave(ETypeData e)
        {
            EventHelper.RunMinorEvents(e);
            CacheHelper.Save();
            DebugHelper.Save();
        }

        public virtual void OnOpenDrama(OpenDrama e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnOpenNPCInfoUI(OpenNPCInfoUI e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnTaskAdd(TaskAdd e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnTaskComplete(TaskComplete e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnTaskFail(TaskFail e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnTaskGive(TaskGive e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnTaskOverl(TaskOverl e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnUnitSetGrade(ETypeData e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnUnitSetHeartState(ETypeData e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnWorldRunStart()
        {
            EventHelper.RunMinorEvents();
        }

        public virtual void OnWorldRunEnd()
        {
            EventHelper.RunMinorEvents();
        }
        #endregion
    }
}