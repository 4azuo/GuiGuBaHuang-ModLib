using ModLib.Object;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace ModLib.Mod
{
    public abstract partial class ModMaster : MonoBehaviour
    {
        protected static HarmonyLib.Harmony harmony;
        private bool loadModFlg = true;
        private bool loadSttFlg = true;

        public static ModMaster ModObj { get; protected set; }
        public abstract string ModName { get; }
        public abstract string ModId { get; }
        public abstract string Version { get; }
        public Gamevar Gamevars { get; protected set; }

        #region caller
        private Action callTimeUpdate;
        private Action callTimeUpdate200ms;
        private Action callTimeUpdate500ms;
        private Action callTimeUpdate1s;
        private Action<ETypeData> callPlayerOpenTreeVault;
        private Action<ETypeData> callPlayerEquipCloth;
        private Action<ETypeData> callPlayerInMonstArea;
        private Action<ETypeData> callPlayerRoleEscapeInMap;
        private Action<ETypeData> callPlayerRoleUpGradeBig;
        private Action<ETypeData> callUpGradeAndCloseFateFeatureUI;
        private Action<ETypeData> callUseHobbyProps;
        //private Action<ETypeData> callFortuitousTrigger;
        //private Action<ETypeData> callTownAuctionStart;
        private Action<ETypeData> callOpenUIStart;
        private Action<ETypeData> callOpenUIEnd;
        private Action<ETypeData> callCloseUIStart;
        private Action<ETypeData> callCloseUIEnd;
        private Action<ETypeData> callInitWorld;
        private Action<ETypeData> callLoadSceneStart;
        private Action<ETypeData> callLoadScene;
        private Action<ETypeData> callIntoWorld;
        private Action<ETypeData> callSave;
        private Action<ETypeData> callOpenDrama;
        private Action<ETypeData> callOpenNPCInfoUI;
        private Action<ETypeData> callTaskAdd;
        private Action<ETypeData> callTaskComplete;
        private Action<ETypeData> callTaskFail;
        private Action<ETypeData> callTaskGive;
        private Action<ETypeData> callTaskOverl;
        private Action<ETypeData> callUnitSetGrade;
        private Action<ETypeData> callUnitSetHeartState;
        private Action<ETypeData> callBattleUnitInit;
        private Action<ETypeData> callBattleUnitHit;
        private Action<ETypeData> callBattleUnitHitDynIntHandler;
        private Action<ETypeData> callBattleUnitShieldHitDynIntHandler;
        private Action<ETypeData> callBattleUnitUseProp;
        private Action<ETypeData> callBattleUnitUseSkill;
        private Action<ETypeData> callBattleUnitUseStep;
        private Action<ETypeData> callBattleUnitDie;
        private Action<ETypeData> callBattleUnitDieEnd;
        private Action<ETypeData> callBattleUnitAddEffectStart;
        private Action<ETypeData> callBattleUnitAddEffect;
        private Action<ETypeData> callBattleUnitAddHP;
        private Action<ETypeData> callBattleUnitUpdateProperty;
        private Action<ETypeData> callBattleSetUnitType;
        private Action<ETypeData> callBattleStart;
        private Action<ETypeData> callBattleEnd;
        private Action<ETypeData> callBattleEndFront;
        private Action<ETypeData> callBattleEndHandler;
        private Action<ETypeData> callBattleEscapeFailed;
        private Action<ETypeData> callBattleExit;
        private Action callWorldRunStart;
        private Action callWorldRunEnd;
        #endregion

        #region timer
        private TimerMgr.CoroutineTime timerUpdate;
        private TimerMgr.CoroutineTime timerUpdate200ms;
        private TimerMgr.CoroutineTime timerUpdate500ms;
        private TimerMgr.CoroutineTime timerUpdate1s;
        #endregion

        #region event
        private EventsMgr.EventsData eventPlayerOpenTreeVault1;
        private EventsMgr.EventsData eventPlayerOpenTreeVault2;
        private EventsMgr.EventsData eventPlayerEquipCloth;
        private EventsMgr.EventsData eventPlayerInMonstArea;
        private EventsMgr.EventsData eventPlayerRoleEscapeInMap;
        private EventsMgr.EventsData eventPlayerRoleUpGradeBig;
        private EventsMgr.EventsData eventUpGradeAndCloseFateFeatureUI;
        private EventsMgr.EventsData eventUseHobbyProps;
        //private EventsMgr.EventsData eventFortuitousTrigger;
        //private EventsMgr.EventsData eventTownAuctionStart1;
        //private EventsMgr.EventsData eventTownAuctionStart2;
        //private EventsMgr.EventsData eventTownAuctionStart3;
        //private EventsMgr.EventsData eventTownAuctionStart4;
        //private EventsMgr.EventsData eventTownAuctionStart5;
        //private EventsMgr.EventsData eventTownAuctionStart6;
        //private EventsMgr.EventsData eventTownAuctionStart7;
        //private EventsMgr.EventsData eventTownAuctionStart8;
        //private EventsMgr.EventsData eventTownAuctionStart9;
        //private EventsMgr.EventsData eventTownAuctionStart10;
        private EventsMgr.EventsData eventOpenUIStart;
        private EventsMgr.EventsData eventOpenUIEnd;
        private EventsMgr.EventsData eventCloseUIStart;
        private EventsMgr.EventsData eventCloseUIEnd;
        private EventsMgr.EventsData eventInitWorld;
        private EventsMgr.EventsData eventLoadSceneStart;
        private EventsMgr.EventsData eventLoadScene;
        private EventsMgr.EventsData eventIntoWorld;
        private EventsMgr.EventsData eventSave;
        private EventsMgr.EventsData eventOpenDrama;
        private EventsMgr.EventsData eventOpenNPCInfoUI;
        private EventsMgr.EventsData eventTaskAdd;
        private EventsMgr.EventsData eventTaskComplete;
        private EventsMgr.EventsData eventTaskFail;
        private EventsMgr.EventsData eventTaskGive;
        private EventsMgr.EventsData eventTaskOverl;
        private EventsMgr.EventsData eventUnitSetGrade;
        private EventsMgr.EventsData eventUnitSetHeartState;
        private EventsMgr.EventsData eventBattleUnitInit;
        private EventsMgr.EventsData eventBattleUnitHit;
        private EventsMgr.EventsData eventBattleUnitHitDynIntHandler;
        private EventsMgr.EventsData eventBattleUnitShieldHitDynIntHandler;
        private EventsMgr.EventsData eventBattleUnitUseProp;
        private EventsMgr.EventsData eventBattleUnitUseSkill;
        private EventsMgr.EventsData eventBattleUnitUseStep;
        private EventsMgr.EventsData eventBattleUnitDie;
        private EventsMgr.EventsData eventBattleUnitDieEnd;
        private EventsMgr.EventsData eventBattleUnitAddEffectStart;
        private EventsMgr.EventsData eventBattleUnitAddEffect;
        private EventsMgr.EventsData eventBattleUnitAddHP;
        private EventsMgr.EventsData eventBattleUnitUpdateProperty;
        private EventsMgr.EventsData eventBattleSetUnitType;
        private EventsMgr.EventsData eventBattleStart;
        private EventsMgr.EventsData eventBattleEnd;
        private EventsMgr.EventsData eventBattleEndFront;
        private EventsMgr.EventsData eventBattleEndHandler;
        private EventsMgr.EventsData eventBattleEscapeFailed;
        private EventsMgr.EventsData eventBattleExit;
        #endregion

        #region mono
        private MonoUpdater monoUpdater;
        #endregion

        //GameEvent
        public virtual void Init()
        {
            try
            {
                DebugHelper.WriteLine("Load.");

                ModObj = this;

                //load harmony
                harmony?.UnpatchSelf();
                harmony = new HarmonyLib.Harmony(ModName);
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                //declare caller
                #region caller
                callTimeUpdate = _OnTimeUpdate;
                callTimeUpdate200ms = _OnTimeUpdate200ms;
                callTimeUpdate500ms = _OnTimeUpdate500ms;
                callTimeUpdate1s = _OnTimeUpdate1s;
                callPlayerOpenTreeVault = _OnPlayerOpenTreeVault;
                callPlayerEquipCloth = _OnPlayerEquipCloth;
                callPlayerInMonstArea = _OnPlayerInMonstArea;
                callPlayerRoleEscapeInMap = _OnPlayerRoleEscapeInMap;
                callPlayerRoleUpGradeBig = _OnPlayerRoleUpGradeBig;
                callUpGradeAndCloseFateFeatureUI = _OnUpGradeAndCloseFateFeatureUI;
                callUseHobbyProps = _OnUseHobbyProps;
                //callFortuitousTrigger = _OnFortuitousTrigger;
                //callTownAuctionStart = _OnTownAuctionStart;
                callOpenUIStart = _OnOpenUIStart;
                callOpenUIEnd = _OnOpenUIEnd;
                callCloseUIStart = _OnCloseUIStart;
                callCloseUIEnd = _OnCloseUIEnd;
                callInitWorld = _OnInitWorld;
                callLoadSceneStart = _OnLoadSceneStart;
                callLoadScene = _OnLoadScene;
                callIntoWorld = _OnIntoWorld;
                callSave = _OnSave;
                callOpenDrama = _OnOpenDrama;
                callOpenNPCInfoUI = _OnOpenNPCInfoUI;
                callTaskAdd = _OnTaskAdd;
                callTaskComplete = _OnTaskComplete;
                callTaskFail = _OnTaskFail;
                callTaskGive = _OnTaskGive;
                callTaskOverl = _OnTaskOverl;
                callUnitSetGrade = _OnUnitSetGrade;
                callUnitSetHeartState = _OnUnitSetHeartState;
                callBattleUnitInit = _OnBattleUnitInit;
                callBattleUnitHit = _OnBattleUnitHit;
                callBattleUnitHitDynIntHandler = _OnBattleUnitHitDynIntHandler;
                callBattleUnitShieldHitDynIntHandler = _OnBattleUnitShieldHitDynIntHandler;
                callBattleUnitUseProp = _OnBattleUnitUseProp;
                callBattleUnitUseSkill = _OnBattleUnitUseSkill;
                callBattleUnitUseStep = _OnBattleUnitUseStep;
                callBattleUnitDie = _OnBattleUnitDie;
                callBattleUnitDieEnd = _OnBattleUnitDieEnd;
                callBattleUnitAddEffectStart = _OnBattleUnitAddEffectStart;
                callBattleUnitAddEffect = _OnBattleUnitAddEffect;
                callBattleUnitAddHP = _OnBattleUnitAddHP;
                callBattleUnitUpdateProperty = _OnBattleUnitUpdateProperty;
                callBattleSetUnitType = _OnBattleSetUnitType;
                callBattleStart = _OnBattleStart;
                callBattleEnd = _OnBattleEnd;
                callBattleEndFront = _OnBattleEndFront;
                callBattleEndHandler = _OnBattleEndHandler;
                callBattleEscapeFailed = _OnBattleEscapeFailed;
                callBattleExit = _OnBattleExit;
                //callWorldRunStart = _OnWorldRunStart;
                //callWorldRunEnd = _OnWorldRunEnd;
                #endregion

                //register event
                #region Timer
                timerUpdate = RegTimer(callTimeUpdate, 0.111f);
                timerUpdate200ms = RegTimer(callTimeUpdate200ms, 0.211f);
                timerUpdate500ms = RegTimer(callTimeUpdate500ms, 0.511f);
                timerUpdate1s = RegTimer(callTimeUpdate1s, 1.011f);
                #endregion

                #region EMapType
                eventPlayerOpenTreeVault1 = RegEvent(EMapType.PlayerOpenTownBuild((int)MapBuildSubType.TownStorage), callPlayerOpenTreeVault);
                eventPlayerOpenTreeVault2 = RegEvent(EMapType.PlayerOpenTownBuild((int)MapBuildSubType.SchoolStorage), callPlayerOpenTreeVault);
                eventPlayerEquipCloth = RegEvent(EMapType.PlayerEquipCloth, callPlayerEquipCloth);
                eventPlayerInMonstArea = RegEvent(EMapType.PlayerInMonstArea, callPlayerInMonstArea);
                eventPlayerRoleEscapeInMap = RegEvent(EMapType.PlayerRoleEscapeInMap, callPlayerRoleEscapeInMap);
                eventPlayerRoleUpGradeBig = RegEvent(EMapType.PlayerRoleUpGradeBig, callPlayerRoleUpGradeBig);
                eventUpGradeAndCloseFateFeatureUI = RegEvent(EMapType.UpGradeAndCloseFateFeatureUI, callUpGradeAndCloseFateFeatureUI);
                eventUseHobbyProps = RegEvent(EMapType.UseHobbyProps, callUseHobbyProps);
                //eventFortuitousTrigger = RegEvent(EMapType.FortuitousTrigger, callFortuitousTrigger);
                #endregion

                #region EGameType
                //eventTownAuctionStart1 = RegEvent(EGameType.TownAuctionStart(1), callTownAuctionStart);
                //eventTownAuctionStart2 = RegEvent(EGameType.TownAuctionStart(2), callTownAuctionStart);
                //eventTownAuctionStart3 = RegEvent(EGameType.TownAuctionStart(3), callTownAuctionStart);
                //eventTownAuctionStart4 = RegEvent(EGameType.TownAuctionStart(4), callTownAuctionStart);
                //eventTownAuctionStart5 = RegEvent(EGameType.TownAuctionStart(5), callTownAuctionStart);
                //eventTownAuctionStart6 = RegEvent(EGameType.TownAuctionStart(6), callTownAuctionStart);
                //eventTownAuctionStart7 = RegEvent(EGameType.TownAuctionStart(7), callTownAuctionStart);
                //eventTownAuctionStart8 = RegEvent(EGameType.TownAuctionStart(8), callTownAuctionStart);
                //eventTownAuctionStart9 = RegEvent(EGameType.TownAuctionStart(9), callTownAuctionStart);
                //eventTownAuctionStart10 = RegEvent(EGameType.TownAuctionStart(10), callTownAuctionStart);
                eventOpenUIStart = RegEvent(EGameType.OpenUIStart, callOpenUIStart);
                eventOpenUIEnd = RegEvent(EGameType.OpenUIEnd, callOpenUIEnd);
                eventCloseUIStart = RegEvent(EGameType.CloseUIStart, callCloseUIStart);
                eventCloseUIEnd = RegEvent(EGameType.CloseUIEnd, callCloseUIEnd);
                eventInitWorld = RegEvent(EGameType.InitCreateGameWorld, callInitWorld);
                eventLoadSceneStart = RegEvent(EGameType.LoadSceneStart, callLoadSceneStart);
                eventLoadScene = RegEvent(EGameType.LoadScene, callLoadScene);
                eventIntoWorld = RegEvent(EGameType.IntoWorld, callIntoWorld);
                eventSave = RegEvent(EGameType.SaveData, callSave);
                eventOpenDrama = RegEvent(EGameType.OpenDrama, callOpenDrama);
                eventOpenNPCInfoUI = RegEvent(EGameType.OpenNPCInfoUI, callOpenNPCInfoUI);
                eventTaskAdd = RegEvent(EGameType.TaskAdd, callTaskAdd);
                eventTaskComplete = RegEvent(EGameType.TaskComplete, callTaskComplete);
                eventTaskFail = RegEvent(EGameType.TaskFail, callTaskFail);
                eventTaskGive = RegEvent(EGameType.TaskGive, callTaskGive);
                eventTaskOverl = RegEvent(EGameType.TaskOverl, callTaskOverl);
                eventUnitSetGrade = RegEvent(EGameType.UnitSetGrade, callUnitSetGrade);
                eventUnitSetHeartState = RegEvent(EGameType.UnitSetHeartState, callUnitSetHeartState);
                #endregion

                #region EBattleType
                eventBattleUnitInit = RegEvent(EBattleType.UnitInit, callBattleUnitInit);
                eventBattleUnitHit = RegEvent(EBattleType.UnitHit, callBattleUnitHit);
                eventBattleUnitHitDynIntHandler = RegEvent(EBattleType.UnitHitDynIntHandler, callBattleUnitHitDynIntHandler);
                eventBattleUnitShieldHitDynIntHandler = RegEvent(EBattleType.UnitShieldHitDynIntHandler, callBattleUnitShieldHitDynIntHandler);
                eventBattleUnitUseProp = RegEvent(EBattleType.UnitUseProp, callBattleUnitUseProp);
                eventBattleUnitUseSkill = RegEvent(EBattleType.UnitUseSkill, callBattleUnitUseSkill);
                eventBattleUnitUseStep = RegEvent(EBattleType.UnitUseStep, callBattleUnitUseStep);
                eventBattleUnitDie = RegEvent(EBattleType.UnitDie, callBattleUnitDie);
                eventBattleUnitDieEnd = RegEvent(EBattleType.UnitDieEnd, callBattleUnitDieEnd);
                eventBattleUnitAddEffectStart = RegEvent(EBattleType.UnitAddEffectStart, callBattleUnitAddEffectStart);
                eventBattleUnitAddEffect = RegEvent(EBattleType.UnitAddEffect, callBattleUnitAddEffect);
                eventBattleUnitAddHP = RegEvent(EBattleType.UnitAddHP, callBattleUnitAddHP);
                eventBattleUnitUpdateProperty = RegEvent(EBattleType.UnitUpdateProperty, callBattleUnitUpdateProperty);
                eventBattleSetUnitType = RegEvent(EBattleType.SetUnitType, callBattleSetUnitType);
                eventBattleStart = RegEvent(EBattleType.BattleStart, callBattleStart);
                eventBattleEnd = RegEvent(EBattleType.BattleEnd, callBattleEnd);
                eventBattleEndFront = RegEvent(EBattleType.BattleEndFront, callBattleEndFront);
                eventBattleEndHandler = RegEvent(EBattleType.BattleEndHandler, callBattleEndHandler);
                eventBattleEscapeFailed = RegEvent(EBattleType.BattleEscapeFailed, callBattleEscapeFailed);
                eventBattleExit = RegEvent(EBattleType.BattleExit, callBattleExit);
                #endregion

                #region MonoEvents
                ClassInjector.RegisterTypeInIl2Cpp<MonoUpdater>();
                monoUpdater = g.root.AddComponent<MonoUpdater>();
                monoUpdater.UpdateFunc = OnMonoUpdate;
                #endregion

                //#region Others
                //g.world.run.On(WorldRunOrder.Start, callWorldRunStart);
                //g.world.run.On(WorldRunOrder.End, callWorldRunEnd);
                //#endregion

                //debug
                //foreach (var e in g.timer.allTime)
                //{
                //    DebugHelper.WriteLine($"Timer({e.GetHashCode()}), loop={e.loop}, time={e.time}, stop={e.isStop}");
                //}
                //foreach (var e in g.events.allEvents)
                //{
                //    DebugHelper.WriteLine($"Id: {e.Key}");
                //    foreach (var ev in e.Value)
                //    {
                //        DebugHelper.WriteLine($"Call1({ev.call1?.GetHashCode()}), Call2({ev.call2?.GetHashCode()}), off={ev.isOff}");
                //    }
                //}
                //DebugHelper.Save();
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine(ex);
                throw ex;
            }
        }

        public void Destroy()
        {
            try
            {
                DebugHelper.WriteLine("Unload mod.");

                CallEvents("OnUnload");

                loadModFlg = true;
                loadSttFlg = true;

                //unregister event
                #region Timer
                UnregTimer(timerUpdate);
                UnregTimer(timerUpdate200ms);
                UnregTimer(timerUpdate500ms);
                UnregTimer(timerUpdate1s);
                #endregion

                #region EMapType
                UnregEvent(eventPlayerOpenTreeVault1);
                UnregEvent(eventPlayerOpenTreeVault2);
                UnregEvent(eventPlayerEquipCloth);
                UnregEvent(eventPlayerInMonstArea);
                UnregEvent(eventPlayerRoleEscapeInMap);
                UnregEvent(eventPlayerRoleUpGradeBig);
                UnregEvent(eventUpGradeAndCloseFateFeatureUI);
                UnregEvent(eventUseHobbyProps);
                //UnregEvent(eventFortuitousTrigger);
                #endregion

                #region EGameType
                //UnregEvent(eventTownAuctionStart1);
                //UnregEvent(eventTownAuctionStart2);
                //UnregEvent(eventTownAuctionStart3);
                //UnregEvent(eventTownAuctionStart4);
                //UnregEvent(eventTownAuctionStart5);
                //UnregEvent(eventTownAuctionStart6);
                //UnregEvent(eventTownAuctionStart7);
                //UnregEvent(eventTownAuctionStart8);
                //UnregEvent(eventTownAuctionStart9);
                //UnregEvent(eventTownAuctionStart10);
                UnregEvent(eventOpenUIStart);
                UnregEvent(eventOpenUIEnd);
                UnregEvent(eventCloseUIStart);
                UnregEvent(eventCloseUIEnd);
                UnregEvent(eventInitWorld);
                UnregEvent(eventLoadSceneStart);
                UnregEvent(eventLoadScene);
                UnregEvent(eventIntoWorld);
                UnregEvent(eventSave);
                UnregEvent(eventOpenDrama);
                UnregEvent(eventOpenNPCInfoUI);
                UnregEvent(eventTaskAdd);
                UnregEvent(eventTaskComplete);
                UnregEvent(eventTaskFail);
                UnregEvent(eventTaskGive);
                UnregEvent(eventTaskOverl);
                UnregEvent(eventUnitSetGrade);
                UnregEvent(eventUnitSetHeartState);
                #endregion

                #region EBattleType
                UnregEvent(eventBattleUnitInit);
                UnregEvent(eventBattleUnitHit);
                UnregEvent(eventBattleUnitHitDynIntHandler);
                UnregEvent(eventBattleUnitShieldHitDynIntHandler);
                UnregEvent(eventBattleUnitUseProp);
                UnregEvent(eventBattleUnitUseSkill);
                UnregEvent(eventBattleUnitUseStep);
                UnregEvent(eventBattleUnitDie);
                UnregEvent(eventBattleUnitDieEnd);
                UnregEvent(eventBattleUnitAddEffectStart);
                UnregEvent(eventBattleUnitAddEffect);
                UnregEvent(eventBattleUnitAddHP);
                UnregEvent(eventBattleUnitUpdateProperty);
                UnregEvent(eventBattleSetUnitType);
                UnregEvent(eventBattleStart);
                UnregEvent(eventBattleEnd);
                UnregEvent(eventBattleEndFront);
                UnregEvent(eventBattleEndHandler);
                UnregEvent(eventBattleEscapeFailed);
                UnregEvent(eventBattleExit);
                #endregion
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine(ex);
                throw ex;
            }
        }

        private TimerMgr.CoroutineTime RegTimer(Action action, float period)
        {
            g.timer.Time(action, period, true);
            return g.timer.allTime.ToArray().Last();
        }

        private EventsMgr.EventsData RegEvent(string id,  Action<ETypeData> action)
        {
            g.events.On(id, action, 0, true);
            return g.events.allEvents[id].ToArray().Last();
        }

        private void UnregTimer(TimerMgr.CoroutineTime timer)
        {
            timer.Stop();
            g.timer.allTime.Remove(timer);
        }

        private void UnregEvent(EventsMgr.EventsData eItem)
        {
            eItem.isOff = true;
            foreach (var e in g.events.allEvents)
            {
                if (e.Value.Contains(eItem))
                    e.Value.Remove(eItem);
            }
        }

        private void CallEvents(string methodName)
        {
            CallEvents<Il2CppObjectBase>(methodName, null);
        }

        private void CallEvents<T>(string methodName, Il2CppObjectBase e) where T : Il2CppObjectBase
        {
            try
            {
                var method = this.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                if (method.GetParameters().Length == 0)
                {
                    method.Invoke(this, null);
                }
                else
                {
                    method.Invoke(this, new object[] { e?.TryCast<T>()/* ?? e*/ });
                }
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"CallEvents<{typeof(T).Name}>({methodName})");
                DebugHelper.WriteLine(ex);
            }
        }

        public static void ShowException(Exception ex, string log)
        {
            if (g.ui.HasUI(UIType.TextInfoLong))
            {
                g.ui.CloseUI(UIType.TextInfoLong);
            }

            var ui = g.ui.OpenUI<UITextInfoLong>(UIType.TextInfoLong);
            ui.InitData("Exception", ex.GetAllInnnerExceptionStr(), "Open log", (Il2CppSystem.Action)(() =>
            {
                Process.Start("notepad.exe", log);
            }), true);
            ui.ptextInfo.fontSize = 14;
        }

        public static void AddGlobalCaches()
        {
            //load
            CacheHelper.LoadGlobalCaches();
            //log
            DebugHelper.Save();
        }

        public static void AddGameCaches()
        {
            //load
            CacheHelper.LoadGameCaches();
            //remove unuse cache
            CacheHelper.RemoveUnuseGlobalCaches();
            //order
            CacheHelper.Order();
            //log
            DebugHelper.Save();
        }

        public static void LoadEnumObj(Assembly ass)
        {
            if (ass == null)
                return;
            var enumTypes = ass.GetLoadableTypes().Where(x => x.IsClass && x.IsSubclassOf(typeof(EnumObject))).OrderBy(x => x.FullName).ToList();
            foreach (var t in enumTypes)
            {
                RuntimeHelpers.RunClassConstructor(t.TypeHandle);
                DebugHelper.WriteLine($"Load EObj: {t.FullName}");
            }
        }
    }
}