using ModLib.Const;
using ModLib.Object;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using UnhollowerBaseLib;
using UnityEngine;
using static DataUnitLog;

namespace ModLib.Mod
{
    public abstract partial class ModMaster : MonoBehaviour
    {
        protected bool initMod = false;
        protected static HarmonyLib.Harmony harmony;

        public abstract string ModName { get; }
        public abstract string ModId { get; }
        public virtual InGameSettings InGameSettings => InGameSettings.GetSettings<InGameSettings>();
        public static ModMaster ModObj { get; protected set; }

        //GameEvent
        public virtual void Init()
        {
            ModObj = this;

            //load harmony
            harmony?.UnpatchSelf();
            harmony = new HarmonyLib.Harmony(ModName);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            //declare funcs
            #region Timer
            var callTime100ms = new Action(_OnTimeUpdate);
            var callTime200ms = new Action(_OnTimeUpdate200ms);
            var callTime500ms = new Action(_OnTimeUpdate500ms);
            var callTime1s = new Action(_OnTimeUpdate1s);
            #endregion

            #region EMapType
            var callPlayerOpenTreeVault = (Il2CppSystem.Action<ETypeData>)_OnPlayerOpenTreeVault;
            var callPlayerEquipCloth = (Il2CppSystem.Action<ETypeData>)_OnPlayerEquipCloth;
            var callPlayerInMonstArea = (Il2CppSystem.Action<ETypeData>)_OnPlayerInMonstArea;
            var callPlayerRoleEscapeInMap = (Il2CppSystem.Action<ETypeData>)_OnPlayerRoleEscapeInMap;
            var callPlayerRoleUpGradeBig = (Il2CppSystem.Action<ETypeData>)_OnPlayerRoleUpGradeBig;
            var callUpGradeAndCloseFateFeatureUI = (Il2CppSystem.Action<ETypeData>)_OnUpGradeAndCloseFateFeatureUI;
            var callUseHobbyProps = (Il2CppSystem.Action<ETypeData>)_OnUseHobbyProps;
            //var callFortuitousTrigger = (Il2CppSystem.Action<ETypeData>)_OnFortuitousTrigger;
            #endregion

            #region EGameType
            //var callTownAuctionStart = (Il2CppSystem.Action<ETypeData>)_OnTownAuctionStart;
            var callOpenUIStart = (Il2CppSystem.Action<ETypeData>)_OnOpenUIStart;
            var callOpenUIEnd = (Il2CppSystem.Action<ETypeData>)_OnOpenUIEnd;
            var callCloseUIStart = (Il2CppSystem.Action<ETypeData>)_OnCloseUIStart;
            var callCloseUIEnd = (Il2CppSystem.Action<ETypeData>)_OnCloseUIEnd;
            var callInitWorld = (Il2CppSystem.Action<ETypeData>)_OnInitWorld;
            var callLoadScene = (Il2CppSystem.Action<ETypeData>)_OnLoadScene;
            var callIntoWorld = (Il2CppSystem.Action<ETypeData>)_OnIntoWorld;
            var callSave = (Il2CppSystem.Action<ETypeData>)_OnSave;
            var callOpenDrama = (Il2CppSystem.Action<ETypeData>)_OnOpenDrama;
            var callOpenNPCInfoUI = (Il2CppSystem.Action<ETypeData>)_OnOpenNPCInfoUI;
            var callTaskAdd = (Il2CppSystem.Action<ETypeData>)_OnTaskAdd;
            var callTaskComplete = (Il2CppSystem.Action<ETypeData>)_OnTaskComplete;
            var callTaskFail = (Il2CppSystem.Action<ETypeData>)_OnTaskFail;
            var callTaskGive = (Il2CppSystem.Action<ETypeData>)_OnTaskGive;
            var callTaskOverl = (Il2CppSystem.Action<ETypeData>)_OnTaskOverl;
            var callUnitSetGrade = (Il2CppSystem.Action<ETypeData>)_OnUnitSetGrade;
            var callUnitSetHeartState = (Il2CppSystem.Action<ETypeData>)_OnUnitSetHeartState;
            #endregion

            #region EBattleType
            var callBattleUnitInit = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitInit;
            var callBattleUnitHit = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitHit;
            var callBattleUnitHitDynIntHandler = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitHitDynIntHandler;
            var callBattleUnitShieldHitDynIntHandler = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitShieldHitDynIntHandler;
            var callBattleUnitUseProp = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitUseProp;
            var callBattleUnitUseSkill = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitUseSkill;
            var callBattleUnitUseStep = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitUseStep;
            var callBattleUnitDie = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitDie;
            var callBattleUnitDieEnd = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitDieEnd;
            var callBattleUnitAddEffectStart = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitAddEffectStart;
            var callBattleUnitAddEffect = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitAddEffect;
            var callBattleUnitAddHP = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitAddHP;
            var callBattleUnitUpdateProperty = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitUpdateProperty;
            var callBattleSetUnitType = (Il2CppSystem.Action<ETypeData>)_OnBattleSetUnitType;
            var callBattleStart = (Il2CppSystem.Action<ETypeData>)_OnBattleStart;
            var callBattleEnd = (Il2CppSystem.Action<ETypeData>)_OnBattleEnd;
            var callBattleEndFront = (Il2CppSystem.Action<ETypeData>)_OnBattleEndFront;
            var callBattleEndHandler = (Il2CppSystem.Action<ETypeData>)_OnBattleEndHandler;
            var callBattleEscapeFailed = (Il2CppSystem.Action<ETypeData>)_OnBattleEscapeFailed;
            var callBattleExit = (Il2CppSystem.Action<ETypeData>)_OnBattleExit;
            #endregion

            //declare event
            DebugHelper.WriteLine("1");
            DebugHelper.Save();
            #region Timer
            RegTimer(callTime100ms, 0.1f);
            RegTimer(callTime200ms, 0.2f);
            RegTimer(callTime500ms, 0.5f);
            RegTimer(callTime1s, 1f);
            #endregion

            DebugHelper.WriteLine("2");
            DebugHelper.Save();
            #region EMapType
            RegEvent2(EMapType.PlayerOpenTownBuild((int)MapBuildSubType.TownStorage), _OnPlayerOpenTreeVault);
            RegEvent(EMapType.PlayerOpenTownBuild((int)MapBuildSubType.SchoolStorage), callPlayerOpenTreeVault);
            RegEvent(EMapType.PlayerEquipCloth, callPlayerEquipCloth);
            RegEvent(EMapType.PlayerInMonstArea, callPlayerInMonstArea);
            RegEvent(EMapType.PlayerRoleEscapeInMap, callPlayerRoleEscapeInMap);
            RegEvent(EMapType.PlayerRoleUpGradeBig, callPlayerRoleUpGradeBig);
            RegEvent(EMapType.UpGradeAndCloseFateFeatureUI, callUpGradeAndCloseFateFeatureUI);
            RegEvent(EMapType.UseHobbyProps, callUseHobbyProps);
            //RegEvent(EMapType.FortuitousTrigger, callFortuitousTrigger);
            #endregion

            DebugHelper.WriteLine("3");
            DebugHelper.Save();
            #region EGameType
            //RegEvent(EGameType.TownAuctionStart(1), callTownAuctionStart);
            //RegEvent(EGameType.TownAuctionStart(2), callTownAuctionStart);
            //RegEvent(EGameType.TownAuctionStart(3), callTownAuctionStart);
            //RegEvent(EGameType.TownAuctionStart(4), callTownAuctionStart);
            //RegEvent(EGameType.TownAuctionStart(5), callTownAuctionStart);
            //RegEvent(EGameType.TownAuctionStart(6), callTownAuctionStart);
            //RegEvent(EGameType.TownAuctionStart(7), callTownAuctionStart);
            //RegEvent(EGameType.TownAuctionStart(8), callTownAuctionStart);
            //RegEvent(EGameType.TownAuctionStart(9), callTownAuctionStart);
            //RegEvent(EGameType.TownAuctionStart(10), callTownAuctionStart);
            RegEvent(EGameType.OpenUIStart, callOpenUIStart);
            RegEvent(EGameType.OpenUIEnd, callOpenUIEnd);
            RegEvent(EGameType.CloseUIStart, callCloseUIStart);
            RegEvent(EGameType.CloseUIEnd, callCloseUIEnd);
            RegEvent(EGameType.InitCreateGameWorld, callInitWorld);
            RegEvent(EGameType.LoadScene, callLoadScene);
            RegEvent(EGameType.IntoWorld, callIntoWorld);
            RegEvent(EGameType.SaveData, callSave);
            RegEvent(EGameType.OpenDrama, callOpenDrama);
            RegEvent(EGameType.OpenNPCInfoUI, callOpenNPCInfoUI);
            RegEvent(EGameType.TaskAdd, callTaskAdd);
            RegEvent(EGameType.TaskComplete, callTaskComplete);
            RegEvent(EGameType.TaskFail, callTaskFail);
            RegEvent(EGameType.TaskGive, callTaskGive);
            RegEvent(EGameType.TaskOverl, callTaskOverl);
            RegEvent(EGameType.UnitSetGrade, callUnitSetGrade);
            RegEvent(EGameType.UnitSetHeartState, callUnitSetHeartState);
            #endregion

            #region EBattleType
            RegEvent(EBattleType.UnitInit, callBattleUnitInit);
            RegEvent(EBattleType.UnitHit, callBattleUnitHit);
            RegEvent(EBattleType.UnitHitDynIntHandler, callBattleUnitHitDynIntHandler);
            RegEvent(EBattleType.UnitShieldHitDynIntHandler, callBattleUnitShieldHitDynIntHandler);
            RegEvent(EBattleType.UnitUseProp, callBattleUnitUseProp);
            RegEvent(EBattleType.UnitUseSkill, callBattleUnitUseSkill);
            RegEvent(EBattleType.UnitUseStep, callBattleUnitUseStep);
            RegEvent(EBattleType.UnitDie, callBattleUnitDie);
            RegEvent(EBattleType.UnitDieEnd, callBattleUnitDieEnd);
            RegEvent(EBattleType.UnitAddEffectStart, callBattleUnitAddEffectStart);
            RegEvent(EBattleType.UnitAddEffect, callBattleUnitAddEffect);
            RegEvent(EBattleType.UnitAddHP, callBattleUnitAddHP);
            RegEvent(EBattleType.UnitUpdateProperty, callBattleUnitUpdateProperty);
            RegEvent(EBattleType.SetUnitType, callBattleSetUnitType);
            RegEvent(EBattleType.BattleStart, callBattleStart);
            RegEvent(EBattleType.BattleEnd, callBattleEnd);
            RegEvent(EBattleType.BattleEndFront, callBattleEndFront);
            RegEvent(EBattleType.BattleEndHandler, callBattleEndHandler);
            RegEvent(EBattleType.BattleEscapeFailed, callBattleEscapeFailed);
            RegEvent(EBattleType.BattleExit, callBattleExit);
            #endregion

            CallEvents("OnInitMod");
        }

        private void RegTimer(Il2CppSystem.Action action, float period)
        {
            var e = g.timer.allTime.ToArray().FirstOrDefault(x =>
            {
                return x.call.Method.Name == action.Method.Name && x.call.Method.DeclaringType.FullName == action.Method.DeclaringType.FullName;
            });
            if (e != null)
                g.timer.allTime.Remove(e);
            g.timer.Time(action, period, true);
            DebugHelper.WriteLine($"MethodName: {action.Method.Name}");
            DebugHelper.WriteLine($"ClassName: {action.Method.DeclaringType.FullName}");
            DebugHelper.Save();
        }

        private void RegEvent(string id, Il2CppSystem.Action<ETypeData> action)
        {
            var e = g.events.allEvents[id].ToArray().FirstOrDefault(x =>
            {
                return  (x.call1?.Method.Name == action.Method.Name && x.call1?.Method.DeclaringType.FullName == action.Method.DeclaringType.FullName) ||
                        (x.call2?.Method.Name == action.Method.Name && x.call2?.Method.DeclaringType.FullName == action.Method.DeclaringType.FullName);
            });
            if (e != null)
                g.events.allEvents[id].Remove(e);
            g.events.On(id, action);
            DebugHelper.WriteLine($"MethodName: {action.Method.Name}");
            DebugHelper.WriteLine($"ClassName: {action.Method.DeclaringType.FullName}");
            DebugHelper.Save();
        }

        private void RegEvent2(string id, Action<ETypeData> action)
        {
            DebugHelper.WriteLine($"MethodName: {action.Method.Name}");
            DebugHelper.WriteLine($"ClassName: {action.Method.DeclaringType.FullName}");
            DebugHelper.Save();
            foreach (var x in g.events.allEvents[id])
            {
                DebugHelper.WriteLine($"MethodName1: {x.call1?.Method.Name}");
                DebugHelper.WriteLine($"ClassName1: {x.call1?.Method.DeclaringType.FullName}");
                DebugHelper.WriteLine($"MethodName2: {x.call2?.Method.Name}");
                DebugHelper.WriteLine($"ClassName2: {x.call2?.Method.DeclaringType.FullName}");
                DebugHelper.Save();
            }
            var e = g.events.allEvents[id].ToArray().FirstOrDefault(x =>
            {
                return  (x.call1?.Method.Name == action.Method.Name && x.call1?.Method.DeclaringType.FullName == action.Method.DeclaringType.FullName) ||
                        (x.call2?.Method.Name == action.Method.Name && x.call2?.Method.DeclaringType.FullName == action.Method.DeclaringType.FullName);
            });
            if (e != null)
            {
                g.events.allEvents[id].Remove(e);
            }
            g.events.On(id, action);
        }

        private void CallEvents(string methodName, bool isInGame = false, bool isInBattle = false, Func<bool> predicate = null, Action callback = null)
        {
            CallEvents<Il2CppObjectBase>(methodName, null, isInGame, isInBattle, predicate, callback);
        }

        private void CallEvents<T>(string methodName, Il2CppObjectBase e, bool isInGame = false, bool isInBattle = false, Func<bool> predicate = null, Action callback = null) where T : Il2CppObjectBase
        {
            try
            {
                if (isInGame && !GameHelper.IsInGame())
                    return;
                if (isInBattle && !GameHelper.IsInBattlle())
                    return;
                if (!(predicate?.Invoke() ?? true))
                    return;
                
                var method = this.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                if (method == null)
                {
                    throw new NullReferenceException();
                }
                if (method.GetParameters().Length == 0)
                {
                    method.Invoke(this, null);
                }
                else
                {
                    method.Invoke(this, new object[] { e?.TryCast<T>() ?? e });
                }

                callback?.Invoke();
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"CallEvents<{typeof(T).Name}>({methodName}, -, {isInGame}, {isInBattle})");
                DebugHelper.WriteLine(ex);
            }
        }
    }

    public abstract class ModMaster<T> : ModMaster where T : InGameSettings
    {
        public override InGameSettings InGameSettings => ModLib.Object.InGameSettings.GetSettings<T>();
        public T InGameCustomSettings => (T)InGameSettings;

        public override void OnLoadGameBefore()
        {
            base.OnLoadGameBefore();
            var customSettings = this.GetType().GetCustomAttribute<InGameCustomSettingsAttribute>();
            if (customSettings != null)
            {
                if (InGameCustomSettings.CustomConfigVersion != customSettings.ConfCustomConfigVersion)
                {
                    if (!File.Exists(ConfHelper.GetConfFilePath(customSettings.ConfCustomConfigFile)))
                    {
                        //throw new Exception($"CustomConfigFile ({customSettings.ConfCustomConfigFile}) was not found!");
                        return;
                    }
                    var cusStt = JsonConvert.DeserializeObject<T>(ConfHelper.ReadConfData(customSettings.ConfCustomConfigFile));
                    cusStt.IsOldVersion = cusStt.IsOldVersion ||
                        (GameHelper.GetGameMonth() > 1 && (
                            !InGameCustomSettings.CustomConfigVersion.HasValue ||
                            InGameCustomSettings.CustomConfigVersion < ModLibConst.OLD_VERSION_NEED_UPDATE
                        ));
                    cusStt.CustomConfigFile = customSettings.ConfCustomConfigFile;
                    cusStt.CustomConfigVersion = customSettings.ConfCustomConfigVersion;
                    foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.GetCustomAttribute<InheritanceAttribute>() != null))
                    {
                        prop.SetValue(cusStt, prop?.GetValue(InGameCustomSettings), null);
                    }
                    ModLib.Object.InGameSettings.SetSettings(cusStt);
                }
            }
        }

        public override void OnSave(ETypeData e)
        {
            base.OnSave(e);
            if (InGameCustomSettings.IsOldVersion)
            {
                DramaTool.OpenDrama(ModLibConst.OLD_VERSION_DIALOGUE);
                return;
            }
        }
    }
}