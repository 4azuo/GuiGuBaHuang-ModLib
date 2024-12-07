using EGameTypeData;
using ModLib.Object;
using UnityEngine;
using System.Reflection;
using System.Linq;
using static CacheAttribute;
using System;

namespace ModLib.Mod
{
    public abstract partial class ModMaster : MonoBehaviour
    {
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
            if ((InGameSettings?.LoadGameAfter).Is(true) == 1)
            {
                if (InGameSettings.CurMonth != g.game.world.run.roundMonth)
                {
                    InGameSettings.CurMonth = g.game.world.run.roundMonth;

                    //first month
                    if (InGameSettings.LoadFirstMonth)
                    {
                        CallEvents("OnFirstMonth", true, false);
                        InGameSettings.LoadFirstMonth = false;
                    }
                    //monthly
                    CallEvents("OnMonthly", true, false);
                    CallEvents("OnMonthlyForEachWUnit", true, false);
                    //yearly
                    if (g.world.run.roundMonth % 12 == 0)
                    {
                        CallEvents("OnYearly", true, false);
                    }
                }

                //save
                CallEvents<ETypeData>("OnSave", e, true, false);
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
            CallEvents("OnWorldRunEnd", true, false);
        }
        #endregion

        #region ModLib - Events
        public virtual void OnOpenUIStart(OpenUIStart e)
        {
            DebugHelper.WriteLine(e.uiType.uiName);

            if (loadModFlg)
            {
                CallEvents("OnInitConf");
                CallEvents("OnInitEObj");
                loadModFlg = false;
            }

            if (e.uiType.uiName == UIType.MapMain.uiName)
            {
                if (loadSttFlg)
                {
                    CallEvents("OnLoadGameSettings");
                    CallEvents("OnLoadGameCaches");
                    loadSttFlg = false;
                }

                if (InGameSettings.LoadNewGame)
                {
                    CallEvents("OnLoadNewGame");
                    InGameSettings.LoadNewGame = false;
                }

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
            }
            else
            if (e.uiType.uiName == UIType.Town.uiName)
            {
                if (InGameSettings.LoadMapNewGame)
                {
                    CallEvents("OnLoadMapNewGame");
                    InGameSettings.LoadMapNewGame = false;
                }

                if (InGameSettings.LoadMapFirst)
                {
                    CallEvents("OnLoadMapFirst");
                    InGameSettings.LoadMapFirst = false;
                }
            }
            else
            if (e.uiType.uiName == UIType.Login.uiName)
            {
                AddGlobalCaches();
                CallEvents("OnLoadGlobal");
                CacheHelper.SaveGlobalCaches();
            }

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

        public virtual void OnLoadMapNewGame()
        {
            EventHelper.RunMinorEvents();
        }

        public virtual void OnLoadMapFirst()
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

        public virtual void OnMonthlyForEachWUnit()
        {
            foreach (var wunit in g.world.unit.GetUnits())
            {
                EventHelper.RunMinorEvents(wunit);
            }
        }

        public virtual void OnSave(ETypeData e)
        {
            //save log
            DebugHelper.WriteLine($"Save: {GameHelper.GetGameYear()}年{GameHelper.GetGameMonth()}月{GameHelper.GetGameDay()}日");
            EventHelper.RunMinorEvents(e);
            InGameSettings.Save();
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