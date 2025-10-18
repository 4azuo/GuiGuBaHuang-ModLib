using EGameTypeData;
using UnityEngine;

namespace ModLib.Mod
{
    public abstract partial class ModMaster : MonoBehaviour
    {
        #region ModLib - Handlers
        //public virtual void _OnTownAuctionStart(ETypeData e)
        //{
        //    CallEvents("OnTownAuctionStart");
        //}

        public virtual void _OnOpenUIStart(ETypeData e)
        {
            CallEvents<OpenUIStart>("OnOpenUIStart", e);
        }

        public virtual void _OnOpenUIEnd(ETypeData e)
        {
            CallEvents<OpenUIEnd>("OnOpenUIEnd", e);
        }

        public virtual void _OnCloseUIStart(ETypeData e)
        {
            CallEvents<CloseUIStart>("OnCloseUIStart", e);
        }

        public virtual void _OnCloseUIEnd(ETypeData e)
        {
            CallEvents<CloseUIEnd>("OnCloseUIEnd", e);
        }

        public virtual void _OnInitWorld(ETypeData e)
        {
            CallEvents<ETypeData>("OnInitWorld", e);
        }

        public virtual void _OnLoadSceneStart(ETypeData e)
        {
            CallEvents<LoadSceneStart>("OnLoadSceneStart", e);
        }

        public virtual void _OnLoadScene(ETypeData e)
        {
            CallEvents<LoadScene>("OnLoadScene", e);
        }

        public virtual void _OnIntoWorld(ETypeData e)
        {
            CallEvents<ETypeData>("OnIntoWorld", e);
        }

        public virtual void _OnSave(ETypeData e)
        {
            CallEvents<ETypeData>("OnSave", e);
        }

        public virtual void _OnOpenDrama(ETypeData e)
        {
            CallEvents<OpenDrama>("OnOpenDrama", e);
        }

        public virtual void _OnOpenNPCInfoUI(ETypeData e)
        {
            CallEvents<OpenNPCInfoUI>("OnOpenNPCInfoUI", e);
        }

        public virtual void _OnTaskAdd(ETypeData e)
        {
            CallEvents<TaskAdd>("OnTaskAdd", e);
        }

        public virtual void _OnTaskComplete(ETypeData e)
        {
            CallEvents<TaskComplete>("OnTaskComplete", e);
        }

        public virtual void _OnTaskFail(ETypeData e)
        {
            CallEvents<TaskFail>("OnTaskFail", e);
        }

        public virtual void _OnTaskGive(ETypeData e)
        {
            CallEvents<TaskGive>("OnTaskGive", e);
        }

        public virtual void _OnTaskOverl(ETypeData e)
        {
            CallEvents<TaskOverl>("OnTaskOverl", e);
        }

        public virtual void _OnUnitSetGrade(ETypeData e)
        {
            CallEvents<ETypeData>("OnUnitSetGrade", e);
        }

        public virtual void _OnUnitSetHeartState(ETypeData e)
        {
            CallEvents<ETypeData>("OnUnitSetHeartState", e);
        }

        public virtual void _OnWorldRunStart(ETypeData e)
        {
            CallEvents<WorldRunStart>("OnWorldRunStart", e);
        }

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
        public virtual void OnOpenUIStart(OpenUIStart e)
        {
            DebugHelper.WriteLine($"Open: {e.uiType.uiName}");
            EventHelper.RunMinorEvents("OnOpenUIStart", e);
        }

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

        public virtual void OnCloseUIStart(CloseUIStart e)
        {
            EventHelper.RunMinorEvents("OnCloseUIStart", e);
        }

        public virtual void OnCloseUIEnd(CloseUIEnd e)
        {
            EventHelper.RunMinorEvents("OnCloseUIEnd", e);
            DebugHelper.WriteLine($"Close: {e.uiType.uiName}");
        }

        public virtual void OnLoadNewGame()
        {
            EventHelper.RunMinorEvents("OnLoadNewGame");
        }

        public virtual void OnLoadGameBefore()
        {
            EventHelper.RunMinorEvents("OnLoadGameBefore");
        }

        public virtual void OnLoadGame()
        {
            EventHelper.RunMinorEvents("OnLoadGame");
        }

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

        public virtual void OnInitWorld(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnInitWorld", e);
        }

        public virtual void OnLoadScene(LoadScene e)
        {
            EventHelper.RunMinorEvents("OnLoadScene", e);
        }

        public virtual void OnLoadSceneStart(LoadSceneStart e)
        {
            EventHelper.RunMinorEvents("OnLoadSceneStart", e);
        }

        public virtual void OnIntoWorld(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnIntoWorld", e);
        }

        public virtual void OnFirstMonth()
        {
            EventHelper.RunMinorEvents("OnFirstMonth");
        }

        public virtual void OnYearly()
        {
            EventHelper.RunMinorEvents("OnYearly");
        }

        public virtual void OnMonthly()
        {
            EventHelper.RunMinorEvents("OnMonthly");
        }

        public virtual void OnRefreshParameterStoreOnMonthly()
        {
            EventHelper.RunMinorEvents("OnRefreshParameterStoreOnMonthly");
        }

        public virtual void OnMonthlyForEachWUnit()
        {
            foreach (var wunit in ParameterStore.WUnits)
            {
                EventHelper.RunMinorEvents("OnMonthlyForEachWUnit", wunit);
            }
        }

        public virtual void OnSave(ETypeData e)
        {
            DebugHelper.WriteLine($"Save: {GameHelper.GetGameYear()}年{GameHelper.GetGameMonth()}月{GameHelper.GetGameDay()}日");
            EventHelper.RunMinorEvents("OnSave", e);
            Gamevars.Save();
            CacheHelper.Save();
            DebugHelper.Save();
        }

        public virtual void OnOpenDrama(OpenDrama e)
        {
            EventHelper.RunMinorEvents("OnOpenDrama", e);
        }

        public virtual void OnOpenNPCInfoUI(OpenNPCInfoUI e)
        {
            EventHelper.RunMinorEvents("OnOpenNPCInfoUI", e);
        }

        public virtual void OnTaskAdd(TaskAdd e)
        {
            EventHelper.RunMinorEvents("OnTaskAdd", e);
        }

        public virtual void OnTaskComplete(TaskComplete e)
        {
            EventHelper.RunMinorEvents("OnTaskComplete", e);
        }

        public virtual void OnTaskFail(TaskFail e)
        {
            EventHelper.RunMinorEvents("OnTaskFail", e);
        }

        public virtual void OnTaskGive(TaskGive e)
        {
            EventHelper.RunMinorEvents("OnTaskGive", e);
        }

        public virtual void OnTaskOverl(TaskOverl e)
        {
            EventHelper.RunMinorEvents("OnTaskOverl", e);
        }

        public virtual void OnUnitSetGrade(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnUnitSetGrade", e);
        }

        public virtual void OnUnitSetHeartState(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnUnitSetHeartState", e);
        }

        public virtual void OnWorldRunStart(WorldRunStart e)
        {
            EventHelper.RunMinorEvents("OnWorldRunStart", e);
        }

        public virtual void OnWorldRunEnd(WorldRunEnd e)
        {
            DebugHelper.WriteLine($"WorldRunEnd: {GameHelper.GetGameYear()}年{GameHelper.GetGameMonth()}月{GameHelper.GetGameDay()}日");
            EventHelper.RunMinorEvents("OnWorldRunEnd", e);
            EventHelper.CallGameEvent<SaveData>(EGameType.SaveData);
        }

        public virtual void OnMonoUpdate()
        {
            EventHelper.RunMinorEvents("OnMonoUpdate");
        }
        #endregion
    }
}