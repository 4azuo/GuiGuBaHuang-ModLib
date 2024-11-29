using EGameTypeData;
using ModLib.Object;
using System;
using UnityEngine;

namespace ModLib.Mod
{
    public abstract partial class ModMaster : MonoBehaviour
    {
        private bool initMod = false;

        #region ModLib - Handlers
        //public virtual void _OnTownAuctionStart(ETypeData e)
        //{
        //    CallEvents("OnTownAuctionStart", true, false);
        //}

        public virtual void _OnOpenUIStart(ETypeData e)
        {
            try
            {
                //debug
                //DebugHelper.WriteLine(e.TryCast<EGameTypeData.OpenUIStart>()?.uiType?.uiName);
                //DebugHelper.Save();

                if (!initMod)
                {
                    DebugHelper.WriteLine("Load configs.");
                    CallEvents("OnInitConf");
                    CallEvents("OnInitEObj");
                    DebugHelper.Save();
                    initMod = true;
                }

                if (GameHelper.IsInGame())
                {
                    if (InGameSettings.LoadGameBefore)
                    {
                        CallEvents("OnLoadGameBefore");
                        InGameSettings.LoadGameBefore = false;
                    }

                    if (InGameSettings.LoadGame)
                    {
                        CallEvents("OnLoadGame");
                        InGameSettings.LoadGame = false;
                    }

                    if (InGameSettings.LoadGameAfter)
                    {
                        CallEvents("OnLoadGameAfter");
                        InGameSettings.LoadGameAfter = false;
                    }

                    if (InGameSettings.LoadGameFirst)
                    {
                        CallEvents("OnLoadGameFirst");
                        InGameSettings.LoadGameFirst = false;
                    }
                }
                else
                {
                    CacheHelper.ClearGameCache();
                }

                CallEvents<OpenUIStart>("OnOpenUIStart", e, false, false);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine(ex);
                DebugHelper.Save();
            }
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
                    DebugHelper.WriteLine($"↑↑↑↑↑↑↑↑↑{GameHelper.GetGameYear()}年{GameHelper.GetGameMonth()}月{GameHelper.GetGameDay()}日↑↑↑↑↑↑↑↑↑");

                    //monthly event
                    if (InGameSettings.CurMonth != g.game.world.run.roundMonth)
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

                    //next month
                    InGameSettings.CurMonth = g.game.world.run.roundMonth;

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
            CallEvents("OnWorldRunStart", true, false);
        }

        public virtual void _OnWorldRunEnd()
        {
            CallEvents("OnWorldRunEnd", true, false);
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