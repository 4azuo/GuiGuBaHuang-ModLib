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

namespace ModLib.Mod
{
    public abstract partial class ModMaster : MonoBehaviour
    {
        protected static bool initMod = false;
        protected static HarmonyLib.Harmony harmony;

        public abstract string ModName { get; }
        public abstract string ModId { get; }
        public virtual InGameSettings InGameSettings => InGameSettings.GetSettings<InGameSettings>();
        public static ModMaster ModObj { get; protected set; }

        //GameEvent
        public virtual void Init()
        {
            try
            {
                ModObj = this;

                //load harmony
                harmony?.UnpatchSelf();
                harmony = new HarmonyLib.Harmony(ModName);
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                //declare event
                #region Timer
                RegTimer(_OnTimeUpdate, 0.1f);
                RegTimer(_OnTimeUpdate200ms, 0.2f);
                RegTimer(_OnTimeUpdate500ms, 0.5f);
                RegTimer(_OnTimeUpdate1s, 1f);
                #endregion

                #region EMapType
                RegEvent(EMapType.PlayerOpenTownBuild((int)MapBuildSubType.TownStorage), _OnPlayerOpenTreeVault);
                RegEvent(EMapType.PlayerOpenTownBuild((int)MapBuildSubType.SchoolStorage), _OnPlayerOpenTreeVault);
                RegEvent(EMapType.PlayerEquipCloth, _OnPlayerEquipCloth);
                RegEvent(EMapType.PlayerInMonstArea, _OnPlayerInMonstArea);
                RegEvent(EMapType.PlayerRoleEscapeInMap, _OnPlayerRoleEscapeInMap);
                RegEvent(EMapType.PlayerRoleUpGradeBig, _OnPlayerRoleUpGradeBig);
                RegEvent(EMapType.UpGradeAndCloseFateFeatureUI, _OnUpGradeAndCloseFateFeatureUI);
                RegEvent(EMapType.UseHobbyProps, _OnUseHobbyProps);
                //RegEvent(EMapType.FortuitousTrigger, _OnFortuitousTrigger);
                #endregion

                #region EGameType
                //RegEvent(EGameType.TownAuctionStart(1), _OnTownAuctionStart);
                //RegEvent(EGameType.TownAuctionStart(2), _OnTownAuctionStart);
                //RegEvent(EGameType.TownAuctionStart(3), _OnTownAuctionStart);
                //RegEvent(EGameType.TownAuctionStart(4), _OnTownAuctionStart);
                //RegEvent(EGameType.TownAuctionStart(5), _OnTownAuctionStart);
                //RegEvent(EGameType.TownAuctionStart(6), _OnTownAuctionStart);
                //RegEvent(EGameType.TownAuctionStart(7), _OnTownAuctionStart);
                //RegEvent(EGameType.TownAuctionStart(8), _OnTownAuctionStart);
                //RegEvent(EGameType.TownAuctionStart(9), _OnTownAuctionStart);
                //RegEvent(EGameType.TownAuctionStart(10), _OnTownAuctionStart);
                RegEvent(EGameType.OpenUIStart, _OnOpenUIStart);
                RegEvent(EGameType.OpenUIEnd, _OnOpenUIEnd);
                RegEvent(EGameType.CloseUIStart, _OnCloseUIStart);
                RegEvent(EGameType.CloseUIEnd, _OnCloseUIEnd);
                RegEvent(EGameType.InitCreateGameWorld, _OnInitWorld);
                RegEvent(EGameType.LoadScene, _OnLoadScene);
                RegEvent(EGameType.IntoWorld, _OnIntoWorld);
                RegEvent(EGameType.SaveData, _OnSave);
                RegEvent(EGameType.OpenDrama, _OnOpenDrama);
                RegEvent(EGameType.OpenNPCInfoUI, _OnOpenNPCInfoUI);
                RegEvent(EGameType.TaskAdd, _OnTaskAdd);
                RegEvent(EGameType.TaskComplete, _OnTaskComplete);
                RegEvent(EGameType.TaskFail, _OnTaskFail);
                RegEvent(EGameType.TaskGive, _OnTaskGive);
                RegEvent(EGameType.TaskOverl, _OnTaskOverl);
                RegEvent(EGameType.UnitSetGrade, _OnUnitSetGrade);
                RegEvent(EGameType.UnitSetHeartState, _OnUnitSetHeartState);
                #endregion

                #region EBattleType
                RegEvent(EBattleType.UnitInit, _OnBattleUnitInit);
                RegEvent(EBattleType.UnitHit, _OnBattleUnitHit);
                RegEvent(EBattleType.UnitHitDynIntHandler, _OnBattleUnitHitDynIntHandler);
                RegEvent(EBattleType.UnitShieldHitDynIntHandler, _OnBattleUnitShieldHitDynIntHandler);
                RegEvent(EBattleType.UnitUseProp, _OnBattleUnitUseProp);
                RegEvent(EBattleType.UnitUseSkill, _OnBattleUnitUseSkill);
                RegEvent(EBattleType.UnitUseStep, _OnBattleUnitUseStep);
                RegEvent(EBattleType.UnitDie, _OnBattleUnitDie);
                RegEvent(EBattleType.UnitDieEnd, _OnBattleUnitDieEnd);
                RegEvent(EBattleType.UnitAddEffectStart, _OnBattleUnitAddEffectStart);
                RegEvent(EBattleType.UnitAddEffect, _OnBattleUnitAddEffect);
                RegEvent(EBattleType.UnitAddHP, _OnBattleUnitAddHP);
                RegEvent(EBattleType.UnitUpdateProperty, _OnBattleUnitUpdateProperty);
                RegEvent(EBattleType.SetUnitType, _OnBattleSetUnitType);
                RegEvent(EBattleType.BattleStart, _OnBattleStart);
                RegEvent(EBattleType.BattleEnd, _OnBattleEnd);
                RegEvent(EBattleType.BattleEndFront, _OnBattleEndFront);
                RegEvent(EBattleType.BattleEndHandler, _OnBattleEndHandler);
                RegEvent(EBattleType.BattleEscapeFailed, _OnBattleEscapeFailed);
                RegEvent(EBattleType.BattleExit, _OnBattleExit);
                #endregion

                //foreach (var e in g.events.allEvents)
                //{
                //    DebugHelper.WriteLine($"Id: {e.Key}");
                //    foreach (var ev in e.Value)
                //    {
                //        DebugHelper.WriteLine($"Name1: {ev.call1?.GetHashCode()}");
                //        DebugHelper.WriteLine($"Name2: {ev.call2?.GetHashCode()}");
                //    }
                //}
                //DebugHelper.Save();

                CallEvents("OnInitMod");
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine(ex);
                throw ex;
            }
        }

        private void RegTimer(Action action, float period)
        {
            var e = g.timer.allTime.ToArray().FirstOrDefault(x =>
            {
                return x.call.Method.Name == action.Method.Name && x.call.Method.DeclaringType.FullName == action.Method.DeclaringType.FullName;
            });
            if (e != null)
                g.timer.allTime.Remove(e);
            g.timer.Time(action, period, true);
        }

        private void RegEvent(string id,  Action<ETypeData> action)
        {
            //if (g.events.allEvents.ContainsKey(id))
            //{
            //    Il2CppSystem.Action<ETypeData> tmp = action;
            //    var e = g.events.allEvents[id].Find((Il2CppSystem.Predicate<EventsMgr.EventsData>)(x => x.call1 == tmp || x.call2 == tmp));
            //    if (e != null)
            //        g.events.allEvents[id].Remove(e);
            //}
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