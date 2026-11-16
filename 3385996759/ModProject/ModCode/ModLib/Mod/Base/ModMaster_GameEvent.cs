using EGameTypeData;
using UnityEngine;
using ModLib.Helper;

namespace ModLib.Mod
{
    /// <summary>
    /// ModMaster partial: game (EGameType) events, load sequence and monthly/yearly ticks.
    /// </summary>
    public abstract partial class ModMaster : MonoBehaviour
    {
        #region ModLib - Handlers
        //public virtual void _OnTownAuctionStart(ETypeData e)
        //{
        //    CallEvents("OnTownAuctionStart");
        //}

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnOpenUIStart through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnOpenUIStart(ETypeData e)
        {
            CallEvents<OpenUIStart>("OnOpenUIStart", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnOpenUIEnd through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnOpenUIEnd(ETypeData e)
        {
            CallEvents<OpenUIEnd>("OnOpenUIEnd", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnCloseUIStart through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnCloseUIStart(ETypeData e)
        {
            CallEvents<CloseUIStart>("OnCloseUIStart", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnCloseUIEnd through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnCloseUIEnd(ETypeData e)
        {
            CallEvents<CloseUIEnd>("OnCloseUIEnd", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnInitWorld through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnInitWorld(ETypeData e)
        {
            CallEvents<ETypeData>("OnInitWorld", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnLoadSceneStart through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnLoadSceneStart(ETypeData e)
        {
            CallEvents<LoadSceneStart>("OnLoadSceneStart", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnLoadScene through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnLoadScene(ETypeData e)
        {
            CallEvents<LoadScene>("OnLoadScene", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnIntoWorld through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnIntoWorld(ETypeData e)
        {
            CallEvents<ETypeData>("OnIntoWorld", e);
        }

        /// <summary>
        /// Save callback: runs OnSave only when game variables exist and a save game is open.
        /// </summary>
        public virtual void _OnSave(ETypeData e)
        {
            if (Gamevars == null)
                return;

            if (GameHelper.IsInGame())
            {
                CallEvents<ETypeData>("OnSave", e);
            }
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnOpenDrama through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnOpenDrama(ETypeData e)
        {
            CallEvents<OpenDrama>("OnOpenDrama", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnOpenNPCInfoUI through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnOpenNPCInfoUI(ETypeData e)
        {
            CallEvents<OpenNPCInfoUI>("OnOpenNPCInfoUI", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnTaskAdd through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnTaskAdd(ETypeData e)
        {
            CallEvents<TaskAdd>("OnTaskAdd", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnTaskComplete through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnTaskComplete(ETypeData e)
        {
            CallEvents<TaskComplete>("OnTaskComplete", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnTaskFail through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnTaskFail(ETypeData e)
        {
            CallEvents<TaskFail>("OnTaskFail", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnTaskGive through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnTaskGive(ETypeData e)
        {
            CallEvents<TaskGive>("OnTaskGive", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnTaskOverl through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnTaskOverl(ETypeData e)
        {
            CallEvents<TaskOverl>("OnTaskOverl", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnUnitSetGrade through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnUnitSetGrade(ETypeData e)
        {
            CallEvents<ETypeData>("OnUnitSetGrade", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnUnitSetHeartState through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnUnitSetHeartState(ETypeData e)
        {
            CallEvents<ETypeData>("OnUnitSetHeartState", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnWorldRunStart through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnWorldRunStart(ETypeData e)
        {
            CallEvents<WorldRunStart>("OnWorldRunStart", e);
        }

        /// <summary>
        /// World-run end callback. When the month changed: refreshes the ParameterStore and raises
        /// OnRefreshParameterStoreOnMonthly, OnFirstMonth (once), OnMonthly, OnMonthlyForEachWUnit and OnYearly (every 12 months), then OnWorldRunEnd.
        /// </summary>
        public virtual void _OnWorldRunEnd(ETypeData e)
        {
            if (Gamevars == null)
                return;

            if (GameHelper.IsInGame())
            {
                if (Gamevars.CurMonth != g.game.world.run.roundMonth)
                {
                    Gamevars.CurMonth = g.game.world.run.roundMonth;

                    RefreshParameterStore();
                    CallEvents("OnRefreshParameterStoreOnMonthly");

                    //first month
                    if (Gamevars.LoadFirstMonth)
                    {
                        CallEvents("OnFirstMonth");
                        Gamevars.LoadFirstMonth = false;
                    }
                    //monthly
                    CallEvents("OnMonthly");
                    CallEvents("OnMonthlyForEachWUnit");
                    //yearly
                    if (g.world.run.roundMonth % 12 == 0)
                    {
                        CallEvents("OnYearly");
                    }
                }

                //end run
                CallEvents<WorldRunEnd>("OnWorldRunEnd", e);
            }
        }
        #endregion

        #region ModLib - Events
        /// <summary>
        /// Logs the opened UI and dispatches OnOpenUIStart.
        /// </summary>
        public virtual void OnOpenUIStart(OpenUIStart e)
        {
            DebugHelper.WriteLine($"Open: {e.uiType.uiName}");
            EventHelper.RunMinorEvents("OnOpenUIStart", e);
        }

        /// <summary>
        /// First UI ever: runs OnInitMod, OnInitConf, OnInitEObj, OnLoadGlobals.
        /// First MapMain of a save: OnLoadGameVariables, OnLoadGameCaches, then OnLoadNewGame (new save only), OnLoadGameBefore, OnLoadGame, OnLoadGameAfter.
        /// Finally dispatches OnOpenUIEnd.
        /// </summary>
        public virtual void OnOpenUIEnd(OpenUIEnd e)
        {
            if (loadModFlg)
            {
                CallEvents("OnInitMod");
                CallEvents("OnInitConf");
                CallEvents("OnInitEObj");
                CallEvents("OnLoadGlobals");
                loadModFlg = false;
            }

            if (GameHelper.IsInGame())
            {
                if (e.uiType.uiName == UIType.MapMain.uiName)
                {
                    if (loadSttFlg)
                    {
                        CallEvents("OnLoadGameVariables");
                        CallEvents("OnLoadGameCaches");
                        loadSttFlg = false;
                    }

                    if (Gamevars.LoadNewGame)
                    {
                        CallEvents("OnLoadNewGame");
                        Gamevars.LoadNewGame = false;
                    }

                    if (Gamevars.LoadGameBefore)
                    {
                        CallEvents("OnLoadGameBefore");
                        Gamevars.LoadGameBefore = false;
                    }

                    if (Gamevars.LoadGame)
                    {
                        CallEvents("OnLoadGame");
                        Gamevars.LoadGame = false;
                    }

                    if (Gamevars.LoadGameAfter)
                    {
                        CallEvents("OnLoadGameAfter");
                        Gamevars.LoadGameAfter = false;
                    }

                    //if (e.uiType.uiName == UIType.Town.uiName) <- Have a problem with the condition
                    //{
                    //    if (InGameSettings.LoadMapNewGame)
                    //    {
                    //        CallEvents("OnLoadMapNewGame");
                    //        InGameSettings.LoadMapNewGame = false;
                    //    }

                    //    if (InGameSettings.LoadMapFirst)
                    //    {
                    //        CallEvents("OnLoadMapFirst");
                    //        InGameSettings.LoadMapFirst = false;
                    //    }
                    //}
                }
            }

            EventHelper.RunMinorEvents("OnOpenUIEnd", e);
        }

        /// <summary>
        /// Dispatches OnCloseUIStart to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnCloseUIStart(CloseUIStart e)
        {
            EventHelper.RunMinorEvents("OnCloseUIStart", e);
        }

        /// <summary>
        /// Dispatches OnCloseUIEnd and logs the closed UI.
        /// </summary>
        public virtual void OnCloseUIEnd(CloseUIEnd e)
        {
            EventHelper.RunMinorEvents("OnCloseUIEnd", e);
            DebugHelper.WriteLine($"Close: {e.uiType.uiName}");
        }

        /// <summary>
        /// Dispatches OnLoadNewGame to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnLoadNewGame()
        {
            EventHelper.RunMinorEvents("OnLoadNewGame");
        }

        /// <summary>
        /// Dispatches OnLoadGameBefore to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnLoadGameBefore()
        {
            EventHelper.RunMinorEvents("OnLoadGameBefore");
        }

        /// <summary>
        /// Dispatches OnLoadGame to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnLoadGame()
        {
            EventHelper.RunMinorEvents("OnLoadGame");
        }

        /// <summary>
        /// Dispatches OnLoadGameAfter to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnLoadGameAfter()
        {
            EventHelper.RunMinorEvents("OnLoadGameAfter");
        }

        //public virtual void OnLoadMapNewGame()
        //{
        //    EventHelper.RunMinorEvents("OnLoadMapNewGame");
        //}

        //public virtual void OnLoadMapFirst()
        //{
        //    EventHelper.RunMinorEvents("OnLoadMapFirst");
        //}

        /// <summary>
        /// Dispatches OnInitWorld to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnInitWorld(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnInitWorld", e);
        }

        /// <summary>
        /// Dispatches OnLoadScene to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnLoadScene(LoadScene e)
        {
            EventHelper.RunMinorEvents("OnLoadScene", e);
        }

        /// <summary>
        /// Dispatches OnLoadSceneStart to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnLoadSceneStart(LoadSceneStart e)
        {
            EventHelper.RunMinorEvents("OnLoadSceneStart", e);
        }

        /// <summary>
        /// Dispatches OnIntoWorld to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnIntoWorld(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnIntoWorld", e);
        }

        /// <summary>
        /// Dispatches OnFirstMonth to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnFirstMonth()
        {
            EventHelper.RunMinorEvents("OnFirstMonth");
        }

        /// <summary>
        /// Dispatches OnYearly to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnYearly()
        {
            EventHelper.RunMinorEvents("OnYearly");
        }

        /// <summary>
        /// Dispatches OnMonthly to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnMonthly()
        {
            EventHelper.RunMinorEvents("OnMonthly");
        }

        /// <summary>
        /// Dispatches OnRefreshParameterStoreOnMonthly to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnRefreshParameterStoreOnMonthly()
        {
            EventHelper.RunMinorEvents("OnRefreshParameterStoreOnMonthly");
        }

        /// <summary>
        /// Dispatches OnMonthlyForEachWUnit once for every unit of the ParameterStore.
        /// </summary>
        public virtual void OnMonthlyForEachWUnit()
        {
            foreach (var wunit in ParameterStore.WUnits)
            {
                EventHelper.RunMinorEvents("OnMonthlyForEachWUnit", wunit);
            }
        }

        /// <summary>
        /// Dispatches OnSave, then saves game variables, all caches and the log.
        /// </summary>
        public virtual void OnSave(ETypeData e)
        {
            DebugHelper.WriteLine($"Save: {GameHelper.GetGameYear()}年{GameHelper.GetGameMonth()}月{GameHelper.GetGameDay()}日");
            EventHelper.RunMinorEvents("OnSave", e);
            Gamevars.Save();
            CacheHelper.Save();
            DebugHelper.Save();
        }

        /// <summary>
        /// Dispatches OnOpenDrama to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnOpenDrama(OpenDrama e)
        {
            EventHelper.RunMinorEvents("OnOpenDrama", e);
        }

        /// <summary>
        /// Dispatches OnOpenNPCInfoUI to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnOpenNPCInfoUI(OpenNPCInfoUI e)
        {
            EventHelper.RunMinorEvents("OnOpenNPCInfoUI", e);
        }

        /// <summary>
        /// Dispatches OnTaskAdd to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnTaskAdd(TaskAdd e)
        {
            EventHelper.RunMinorEvents("OnTaskAdd", e);
        }

        /// <summary>
        /// Dispatches OnTaskComplete to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnTaskComplete(TaskComplete e)
        {
            EventHelper.RunMinorEvents("OnTaskComplete", e);
        }

        /// <summary>
        /// Dispatches OnTaskFail to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnTaskFail(TaskFail e)
        {
            EventHelper.RunMinorEvents("OnTaskFail", e);
        }

        /// <summary>
        /// Dispatches OnTaskGive to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnTaskGive(TaskGive e)
        {
            EventHelper.RunMinorEvents("OnTaskGive", e);
        }

        /// <summary>
        /// Dispatches OnTaskOverl to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnTaskOverl(TaskOverl e)
        {
            EventHelper.RunMinorEvents("OnTaskOverl", e);
        }

        /// <summary>
        /// Dispatches OnUnitSetGrade to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnUnitSetGrade(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnUnitSetGrade", e);
        }

        /// <summary>
        /// Dispatches OnUnitSetHeartState to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnUnitSetHeartState(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnUnitSetHeartState", e);
        }

        /// <summary>
        /// Dispatches OnWorldRunStart to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnWorldRunStart(WorldRunStart e)
        {
            EventHelper.RunMinorEvents("OnWorldRunStart", e);
        }

        /// <summary>
        /// Dispatches OnWorldRunEnd, then saves the game.
        /// </summary>
        public virtual void OnWorldRunEnd(WorldRunEnd e)
        {
            DebugHelper.WriteLine($"WorldRunEnd: {GameHelper.GetGameYear()}年{GameHelper.GetGameMonth()}月{GameHelper.GetGameDay()}日");
            EventHelper.RunMinorEvents("OnWorldRunEnd", e);
            GameHelper.SaveGame();
        }

        /// <summary>
        /// Dispatches OnMonoUpdate to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnMonoUpdate()
        {
            EventHelper.RunMinorEvents("OnMonoUpdate");
        }
        #endregion
    }
}