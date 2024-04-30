﻿using EGameTypeData;
using System;
using UnityEngine;

namespace ModLib.Mod
{
    public abstract partial class ModMaster : MonoBehaviour
    {
        #region ModLib - Handlers
        public virtual void _OnTownAuctionStart(ETypeData e)
        {
            CallEvents<ETypeData>("OnTownAuctionStart", e, true, false);
        }

        public virtual void _OnOpenUIStart(ETypeData e)
        {
            if (!initMod)
            {
                CallEvents("OnInitConf");
                CallEvents("OnInitEObj");
                DebugHelper.Save();
                initMod = true;
            }

            if (GameHelper.IsInGame())
            {
                var stt = ModSettings.GetSettings<ModSettings>();

                if (stt.LoadGameBefore)
                {
                    CallEvents("OnLoadGameBefore");
                    stt.LoadGameBefore = false;
                }

                if (stt.LoadGame)
                {
                    CallEvents("OnLoadGame");
                    stt.LoadGame = false;
                }

                if (stt.LoadGameAfter)
                {
                    CallEvents("OnLoadGameAfter");
                    stt.LoadGameAfter = false;
                }

                if (stt.LoadGameFirst)
                {
                    CallEvents("OnLoadGameFirst");
                    stt.LoadGameFirst = false;
                }
            }
            else
            {
                CacheHelper.ClearGameCache();
            }

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
                    //monthly event
                    var stt = ModSettings.GetSettings<ModSettings>();
                    if (stt.CurMonth != g.game.world.run.roundMonth)
                    {
                        //first month
                        if (g.game.world.run.roundMonth <= 0)
                        {
                            OnFirstMonth();
                        }
                        OnMonthly();
                        if (g.world.run.roundMonth % 12 == 0)
                        {
                            OnYearly();
                        }
                    }

                    //save
                    OnSave(e);

                    //next month
                    stt.CurMonth = g.game.world.run.roundMonth;
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
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
        #endregion

        #region ModLib - Events
        public virtual void OnOpenUIStart(OpenUIStart e)
        {
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

        public virtual void OnLoadGameBefore()
        {
        }

        public virtual void OnLoadGame()
        {
            EventHelper.RunMinorEvents();
        }

        public virtual void OnLoadGameAfter()
        {
        }

        public virtual void OnLoadGameFirst()
        {
            EventHelper.RunMinorEvents();
        }

        public virtual void OnInitWorld(ETypeData e)
        {
            ModSettings.CreateIfNotExists<ModSettings>();
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnLoadScene(LoadScene e)
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
        #endregion
    }
}