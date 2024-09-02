using ModLib.Object;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using UnhollowerBaseLib;
using UnityEngine;

namespace ModLib.Mod
{
    public abstract partial class ModMaster : MonoBehaviour
    {
        private bool initMod = false;
        protected TimerCoroutine corTimeUpdate;
        protected TimerCoroutine corFrameUpdate;
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
            if (harmony != null)
                harmony.UnpatchSelf();
            harmony = new HarmonyLib.Harmony(ModName);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            //declare event
            #region Timer
            corTimeUpdate = g.timer.Time(new Action(_OnTimeUpdate), 0.1f, true);
            corFrameUpdate = g.timer.Frame(new Action(_OnFrameUpdate), 2, true);
            #endregion

            #region EMapType
            var callPlayerOpenTreeVault = (Il2CppSystem.Action<ETypeData>)_OnPlayerOpenTreeVault;
            g.events.On(EMapType.PlayerOpenTownBuild((int)MapBuildSubType.TownStorage), callPlayerOpenTreeVault);
            g.events.On(EMapType.PlayerOpenTownBuild((int)MapBuildSubType.SchoolStorage), callPlayerOpenTreeVault);

            var callPlayerEquipCloth = (Il2CppSystem.Action<ETypeData>)_OnPlayerEquipCloth;
            g.events.On(EMapType.PlayerEquipCloth, callPlayerEquipCloth);

            var callPlayerInMonstArea = (Il2CppSystem.Action<ETypeData>)_OnPlayerInMonstArea;
            g.events.On(EMapType.PlayerInMonstArea, callPlayerInMonstArea);

            var callPlayerRoleEscapeInMap = (Il2CppSystem.Action<ETypeData>)_OnPlayerRoleEscapeInMap;
            g.events.On(EMapType.PlayerRoleEscapeInMap, callPlayerRoleEscapeInMap);

            var callPlayerRoleUpGradeBig = (Il2CppSystem.Action<ETypeData>)_OnPlayerRoleUpGradeBig;
            g.events.On(EMapType.PlayerRoleUpGradeBig, callPlayerRoleUpGradeBig);

            var callUpGradeAndCloseFateFeatureUI = (Il2CppSystem.Action<ETypeData>)_OnUpGradeAndCloseFateFeatureUI;
            g.events.On(EMapType.UpGradeAndCloseFateFeatureUI, callUpGradeAndCloseFateFeatureUI);

            var callUseHobbyProps = (Il2CppSystem.Action<ETypeData>)_OnUseHobbyProps;
            g.events.On(EMapType.UseHobbyProps, callUseHobbyProps);

            //var callFortuitousTrigger = (Il2CppSystem.Action<ETypeData>)_OnFortuitousTrigger;
            //g.events.On(EMapType.FortuitousTrigger, callFortuitousTrigger);
            #endregion

            #region EGameType
            //var callTownAuctionStart = (Il2CppSystem.Action<ETypeData>)_OnTownAuctionStart;
            //g.events.On(EGameType.TownAuctionStart(1), callTownAuctionStart);
            //g.events.On(EGameType.TownAuctionStart(2), callTownAuctionStart);
            //g.events.On(EGameType.TownAuctionStart(3), callTownAuctionStart);
            //g.events.On(EGameType.TownAuctionStart(4), callTownAuctionStart);
            //g.events.On(EGameType.TownAuctionStart(5), callTownAuctionStart);
            //g.events.On(EGameType.TownAuctionStart(6), callTownAuctionStart);
            //g.events.On(EGameType.TownAuctionStart(7), callTownAuctionStart);
            //g.events.On(EGameType.TownAuctionStart(8), callTownAuctionStart);
            //g.events.On(EGameType.TownAuctionStart(9), callTownAuctionStart);
            //g.events.On(EGameType.TownAuctionStart(10), callTownAuctionStart);

            var callOpenUIStart = (Il2CppSystem.Action<ETypeData>)_OnOpenUIStart;
            g.events.On(EGameType.OpenUIStart, callOpenUIStart);

            var callOpenUIEnd = (Il2CppSystem.Action<ETypeData>)_OnOpenUIEnd;
            g.events.On(EGameType.OpenUIEnd, callOpenUIEnd);

            var callCloseUIStart = (Il2CppSystem.Action<ETypeData>)_OnCloseUIStart;
            g.events.On(EGameType.CloseUIStart, callCloseUIStart);

            var callCloseUIEnd = (Il2CppSystem.Action<ETypeData>)_OnCloseUIEnd;
            g.events.On(EGameType.CloseUIEnd, callCloseUIEnd);

            var callInitWorld = (Il2CppSystem.Action<ETypeData>)_OnInitWorld;
            g.events.On(EGameType.InitCreateGameWorld, callInitWorld);

            var callLoadScene = (Il2CppSystem.Action<ETypeData>)_OnLoadScene;
            g.events.On(EGameType.LoadScene, callLoadScene);

            var callIntoWorld = (Il2CppSystem.Action<ETypeData>)_OnIntoWorld;
            g.events.On(EGameType.IntoWorld, callIntoWorld);

            var callSave = (Il2CppSystem.Action<ETypeData>)_OnSave;
            g.events.On(EGameType.SaveData, callSave);

            var callOpenDrama = (Il2CppSystem.Action<ETypeData>)_OnOpenDrama;
            g.events.On(EGameType.OpenDrama, callOpenDrama);

            var callOpenNPCInfoUI = (Il2CppSystem.Action<ETypeData>)_OnOpenNPCInfoUI;
            g.events.On(EGameType.OpenNPCInfoUI, callOpenNPCInfoUI);

            var callTaskAdd = (Il2CppSystem.Action<ETypeData>)_OnTaskAdd;
            g.events.On(EGameType.TaskAdd, callTaskAdd);

            var callTaskComplete = (Il2CppSystem.Action<ETypeData>)_OnTaskComplete;
            g.events.On(EGameType.TaskComplete, callTaskComplete);

            var callTaskFail = (Il2CppSystem.Action<ETypeData>)_OnTaskFail;
            g.events.On(EGameType.TaskFail, callTaskFail);

            var callTaskGive = (Il2CppSystem.Action<ETypeData>)_OnTaskGive;
            g.events.On(EGameType.TaskGive, callTaskGive);

            var callTaskOverl = (Il2CppSystem.Action<ETypeData>)_OnTaskOverl;
            g.events.On(EGameType.TaskOverl, callTaskOverl);

            var callUnitSetGrade = (Il2CppSystem.Action<ETypeData>)_OnUnitSetGrade;
            g.events.On(EGameType.UnitSetGrade, callUnitSetGrade);

            var callUnitSetHeartState = (Il2CppSystem.Action<ETypeData>)_OnUnitSetHeartState;
            g.events.On(EGameType.UnitSetHeartState, callUnitSetHeartState);
            #endregion

            #region EBattleType
            var callBattleUnitInit = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitInit;
            g.events.On(EBattleType.UnitInit, callBattleUnitInit);

            var callBattleUnitHit = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitHit;
            g.events.On(EBattleType.UnitHit, callBattleUnitHit);

            var callBattleUnitHitDynIntHandler = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitHitDynIntHandler;
            g.events.On(EBattleType.UnitHitDynIntHandler, callBattleUnitHitDynIntHandler);

            var callBattleUnitShieldHitDynIntHandler = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitShieldHitDynIntHandler;
            g.events.On(EBattleType.UnitShieldHitDynIntHandler, callBattleUnitShieldHitDynIntHandler);

            var callBattleUnitUseProp = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitUseProp;
            g.events.On(EBattleType.UnitUseProp, callBattleUnitUseProp);

            var callBattleUnitUseSkill = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitUseSkill;
            g.events.On(EBattleType.UnitUseSkill, callBattleUnitUseSkill);

            var callBattleUnitUseStep = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitUseStep;
            g.events.On(EBattleType.UnitUseStep, callBattleUnitUseStep);

            var callBattleUnitDie = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitDie;
            g.events.On(EBattleType.UnitDie, callBattleUnitDie);

            var callBattleUnitDieEnd = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitDieEnd;
            g.events.On(EBattleType.UnitDieEnd, callBattleUnitDieEnd);

            var callBattleUnitAddEffectStart = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitAddEffectStart;
            g.events.On(EBattleType.UnitAddEffectStart, callBattleUnitAddEffectStart);

            var callBattleUnitAddEffect = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitAddEffect;
            g.events.On(EBattleType.UnitAddEffect, callBattleUnitAddEffect);

            var callBattleUnitAddHP = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitAddHP;
            g.events.On(EBattleType.UnitAddHP, callBattleUnitAddHP);

            var callBattleUnitUpdateProperty = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitUpdateProperty;
            g.events.On(EBattleType.UnitUpdateProperty, callBattleUnitUpdateProperty);

            var callBattleSetUnitType = (Il2CppSystem.Action<ETypeData>)_OnBattleSetUnitType;
            g.events.On(EBattleType.SetUnitType, callBattleSetUnitType);

            var callBattleStart = (Il2CppSystem.Action<ETypeData>)_OnBattleStart;
            g.events.On(EBattleType.BattleStart, callBattleStart);

            var callBattleEnd = (Il2CppSystem.Action<ETypeData>)_OnBattleEnd;
            g.events.On(EBattleType.BattleEnd, callBattleEnd);

            var callBattleEndFront = (Il2CppSystem.Action<ETypeData>)_OnBattleEndFront;
            g.events.On(EBattleType.BattleEndFront, callBattleEndFront);

            var callBattleEndHandler = (Il2CppSystem.Action<ETypeData>)_OnBattleEndHandler;
            g.events.On(EBattleType.BattleEndHandler, callBattleEndHandler);

            var callBattleEscapeFailed = (Il2CppSystem.Action<ETypeData>)_OnBattleEscapeFailed;
            g.events.On(EBattleType.BattleEscapeFailed, callBattleEscapeFailed);

            var callBattleExit = (Il2CppSystem.Action<ETypeData>)_OnBattleExit;
            g.events.On(EBattleType.BattleExit, callBattleExit);
            #endregion

            CallEvents("OnInitMod");
        }

        //GameEvent
        public virtual void Destroy()
        {
            if (corTimeUpdate != null)
                g.timer.Stop(corTimeUpdate);
            if (corFrameUpdate != null)
                g.timer.Stop(corFrameUpdate);
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
                if (callback != null)
                {
                    callback.Invoke();
                }
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
    }
}