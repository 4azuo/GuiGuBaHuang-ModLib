using EBattleTypeData;
using EGameTypeData;
using EMapTypeData;
using Steamworks;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ModLib.Mod
{
    public abstract class ModMaster : MonoBehaviour
    {
        private bool initMod = false;
        protected TimerCoroutine corTimeUpdate;
        protected TimerCoroutine corFrameUpdate;
        protected static HarmonyLib.Harmony harmony;

        public abstract string ModName { get; }
        public abstract string ModId { get; }
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
            corFrameUpdate = g.timer.Frame(new Action(_OnFrameUpdate), 1, true);
            #endregion

            #region EMapType
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
            #endregion

            #region EGameType
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
            #endregion

            #region EBattleType
            var callBattleUnitInit = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitInit;
            g.events.On(EBattleType.UnitInit, callBattleUnitInit);

            var callBattleUnitHit = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitHit;
            g.events.On(EBattleType.UnitHit, callBattleUnitHit);

            var callBattleUnitHitDynIntHandler = (Il2CppSystem.Action<ETypeData>)_OnBattleUnitHitDynIntHandler;
            g.events.On(EBattleType.UnitHitDynIntHandler, callBattleUnitHitDynIntHandler);

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

            var callBattleSetUnitType = (Il2CppSystem.Action<ETypeData>)_OnBattleSetUnitType;
            g.events.On(EBattleType.SetUnitType, callBattleSetUnitType);

            var callBattleStart = (Il2CppSystem.Action<ETypeData>)_OnBattleStart;
            g.events.On(EBattleType.BattleStart, callBattleStart);

            var callBattleEnd = (Il2CppSystem.Action<ETypeData>)_OnBattleEnd;
            g.events.On(EBattleType.BattleEnd, callBattleEnd);

            var callBattleExit = (Il2CppSystem.Action<ETypeData>)_OnBattleExit;
            g.events.On(EBattleType.BattleExit, callBattleExit);
            #endregion
        }

        //GameEvent
        public virtual void Destroy()
        {
            if (corTimeUpdate != null)
                g.timer.Stop(corTimeUpdate);
        }

        #region ModLib - Handlers
        #region Timer
        protected virtual void _OnTimeUpdate()
        {
            if (GameHelper.IsInGame() && CacheHelper.IsGameCacheLoaded())
            {
                try
                {
                    var stt = ModSettings.GetSettings<ModSettings>();

                    if (!stt.LoadGameBefore &&
                        !stt.LoadGame &&
                        !stt.LoadGameAfter &&
                        !stt.LoadGameFirst)
                    {
                        OnTimeUpdate();
                    }
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnFrameUpdate()
        {
            if (GameHelper.IsInGame() && CacheHelper.IsGameCacheLoaded())
            {
                try
                {
                    var stt = ModSettings.GetSettings<ModSettings>();

                    if (!stt.LoadGameBefore &&
                        !stt.LoadGame &&
                        !stt.LoadGameAfter &&
                        !stt.LoadGameFirst)
                    {
                        OnFrameUpdate();
                    }
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }
        #endregion

        #region EMapType
        protected virtual void _OnPlayerEquipCloth(ETypeData edata)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    var e = edata.Cast<PlayerEquipCloth>();

                    OnPlayerEquipCloth(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnPlayerInMonstArea(ETypeData edata)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    var e = edata.Cast<PlayerInMonstArea>();

                    OnPlayerInMonstArea(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnPlayerRoleEscapeInMap(ETypeData edata)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    var e = edata.Cast<PlayerRoleEscapeInMap>();

                    OnPlayerRoleEscapeInMap(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnPlayerRoleUpGradeBig(ETypeData edata)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    var e = edata.Cast<PlayerRoleUpGradeBig>();

                    OnPlayerRoleUpGradeBig(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnUpGradeAndCloseFateFeatureUI(ETypeData edata)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    var e = edata.Cast<UpGradeAndCloseFateFeatureUI>();

                    OnUpGradeAndCloseFateFeatureUI(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnUseHobbyProps(ETypeData edata)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    var e = edata.Cast<UseHobbyProps>();

                    OnUseHobbyProps(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }
        #endregion

        #region EGameType
        protected virtual void _OnOpenUIStart(ETypeData edata)
        {
            var e = edata.Cast<OpenUIStart>();

            if (!initMod)
            {
                try
                {
                    OnInitConf();
                    OnInitEObj();
                    DebugHelper.Save();
                    initMod = true;
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }

            if (GameHelper.IsInGame())
            {
                try
                {
                    var stt = ModSettings.GetSettings<ModSettings>();

                    if (stt.LoadGameBefore)
                    {
                        OnLoadGameBefore();

                        stt.LoadGameBefore = false;
                    }

                    if (stt.LoadGame)
                    {
                        OnLoadGame();

                        stt.LoadGame = false;
                    }

                    if (stt.LoadGameAfter)
                    {
                        OnLoadGameAfter();

                        stt.LoadGameAfter = false;
                    }

                    if (stt.LoadGameFirst)
                    {
                        OnLoadGameFirst();

                        stt.LoadGameFirst = false;
                    }
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
            else
            {
                CacheHelper.ClearGameCache();
            }

            OnOpenUIStart(e);
        }

        protected virtual void _OnOpenUIEnd(ETypeData edata)
        {
            try
            {
                var e = edata.Cast<OpenUIEnd>();

                OnOpenUIEnd(e);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine(ex);
            }
        }

        protected virtual void _OnCloseUIStart(ETypeData edata)
        {
            try
            {
                var e = edata.Cast<CloseUIStart>();

                OnCloseUIStart(e);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine(ex);
            }
        }

        protected virtual void _OnCloseUIEnd(ETypeData edata)
        {
            try
            {
                var e = edata.Cast<CloseUIEnd>();

                OnCloseUIEnd(e);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine(ex);
            }
        }

        protected virtual void _OnInitWorld(ETypeData e)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    OnInitWorld(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnLoadScene(ETypeData edata)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    var e = edata.Cast<LoadScene>();

                    OnLoadScene(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnIntoWorld(ETypeData e)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    OnIntoWorld(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnSave(ETypeData e)
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
        #endregion

        #region EBattleType
        protected virtual void _OnBattleUnitInit(ETypeData edata)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    var e = edata.Cast<UnitInit>();

                    OnBattleUnitInit(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnBattleUnitHit(ETypeData edata)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    var e = edata.Cast<UnitHit>();

                    OnBattleUnitHit(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnBattleUnitHitDynIntHandler(ETypeData edata)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    var e = edata.Cast<UnitHitDynIntHandler>();

                    OnBattleUnitHitDynIntHandler(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnBattleUnitUseProp(ETypeData edata)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    var e = edata.Cast<UnitUseProp>();

                    OnBattleUnitUseProp(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnBattleUnitUseSkill(ETypeData edata)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    var e = edata.Cast<UnitUseSkill>();

                    OnBattleUnitUseSkill(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnBattleUnitUseStep(ETypeData edata)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    var e = edata.Cast<UnitUseStep>();

                    OnBattleUnitUseStep(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnBattleUnitDie(ETypeData edata)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    var e = edata.Cast<UnitDie>();

                    OnBattleUnitDie(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnBattleUnitDieEnd(ETypeData e)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    OnBattleUnitDieEnd(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnBattleUnitAddEffectStart(ETypeData edata)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    var e = edata.Cast<UnitAddEffectStart>();

                    OnBattleUnitAddEffectStart(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnBattleUnitAddEffect(ETypeData edata)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    var e = edata.Cast<UnitAddEffect>();

                    OnBattleUnitAddEffect(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnBattleSetUnitType(ETypeData edata)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    var e = edata.Cast<SetUnitType>();

                    OnBattleSetUnitType(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnBattleStart(ETypeData e)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    OnBattleStart(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnBattleEnd(ETypeData edata)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    var e = edata.Cast<BattleEnd>();

                    OnBattleEnd(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnBattleExit(ETypeData e)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    OnBattleExit(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }
        #endregion
        #endregion

        #region ModLib - Events
        #region Common
        protected virtual void OnInitConf()
        {
            var orgFolder = $"{ConfHelper.GetConfFolderPath()}\\..\\..\\..\\ModProject\\ModConf\\";
            Directory.CreateDirectory(ConfHelper.GetConfFolderPath());
            foreach (var orgFile in Directory.GetFiles(orgFolder))
            {
                File.Copy(orgFile, ConfHelper.GetConfFilePath(Path.GetFileName(orgFile)), true);
            }
            ConfHelper.LoadCustomConf();
        }

        protected virtual void OnInitEObj()
        {
            GameHelper.LoadEnumObj(Assembly.GetAssembly(typeof(ModMaster)));
            GameHelper.LoadEnumObj(Assembly.GetAssembly(ModObj.GetType()));
        }
        #endregion

        #region Timer
        protected virtual void OnTimeUpdate()
        {
            foreach (var ev in EventHelper.GetEvents("OnTimeUpdate"))
            {
                ev.OnTimeUpdate();
            }
        }

        protected virtual void OnFrameUpdate()
        {
            foreach (var ev in EventHelper.GetEvents("OnFrameUpdate"))
            {
                ev.OnFrameUpdate();
            }
        }
        #endregion

        #region EMapType
        protected virtual void OnPlayerEquipCloth(PlayerEquipCloth e)
        {
            foreach (var ev in EventHelper.GetEvents("OnPlayerEquipCloth"))
            {
                ev.OnPlayerEquipCloth(e);
            }
        }

        protected virtual void OnPlayerInMonstArea(PlayerInMonstArea e)
        {
            foreach (var ev in EventHelper.GetEvents("OnPlayerInMonstArea"))
            {
                ev.OnPlayerInMonstArea(e);
            }
        }

        protected virtual void OnPlayerRoleEscapeInMap(PlayerRoleEscapeInMap e)
        {
            foreach (var ev in EventHelper.GetEvents("OnPlayerRoleEscapeInMap"))
            {
                ev.OnPlayerRoleEscapeInMap(e);
            }
        }

        protected virtual void OnPlayerRoleUpGradeBig(PlayerRoleUpGradeBig e)
        {
            foreach (var ev in EventHelper.GetEvents("OnPlayerRoleUpGradeBig"))
            {
                ev.OnPlayerRoleUpGradeBig(e);
            }
        }

        protected virtual void OnUpGradeAndCloseFateFeatureUI(UpGradeAndCloseFateFeatureUI e)
        {
            foreach (var ev in EventHelper.GetEvents("OnUpGradeAndCloseFateFeatureUI"))
            {
                ev.OnUpGradeAndCloseFateFeatureUI(e);
            }
        }

        protected virtual void OnUseHobbyProps(UseHobbyProps e)
        {
            foreach (var ev in EventHelper.GetEvents("OnUseHobbyProps"))
            {
                ev.OnUseHobbyProps(e);
            }
        }
        #endregion

        #region EGameType
        protected virtual void OnOpenUIStart(OpenUIStart e)
        {
            foreach (var ev in EventHelper.GetEvents("OnOpenUIStart"))
            {
                ev.OnOpenUIStart(e);
            }
        }

        protected virtual void OnOpenUIEnd(OpenUIEnd e)
        {
            foreach (var ev in EventHelper.GetEvents("OnOpenUIEnd"))
            {
                ev.OnOpenUIEnd(e);
            }
        }

        protected virtual void OnCloseUIStart(CloseUIStart e)
        {
            foreach (var ev in EventHelper.GetEvents("OnCloseUIStart"))
            {
                ev.OnCloseUIStart(e);
            }
        }

        protected virtual void OnCloseUIEnd(CloseUIEnd e)
        {
            foreach (var ev in EventHelper.GetEvents("OnCloseUIEnd"))
            {
                ev.OnCloseUIEnd(e);
            }
        }

        protected virtual void OnLoadGameBefore()
        {
        }

        protected virtual void OnLoadGame()
        {
            foreach (var ev in EventHelper.GetEvents("OnLoadGame"))
            {
                ev.OnLoadGame();
            }
        }

        protected virtual void OnLoadGameAfter()
        {
        }

        protected virtual void OnLoadGameFirst()
        {
            foreach (var ev in EventHelper.GetEvents("OnLoadGameFirst"))
            {
                ev.OnLoadGameFirst();
            }
        }

        protected virtual void OnInitWorld(ETypeData e)
        {
            ModSettings.CreateIfNotExists<ModSettings>();
            foreach (var ev in EventHelper.GetEvents("OnInitWorld"))
            {
                ev.OnInitWorld(e);
            }
        }

        protected virtual void OnLoadScene(LoadScene e)
        {
            foreach (var ev in EventHelper.GetEvents("OnLoadScene"))
            {
                ev.OnLoadScene(e);
            }
        }

        protected virtual void OnIntoWorld(ETypeData e)
        {
            foreach (var ev in EventHelper.GetEvents("OnIntoWorld"))
            {
                ev.OnIntoWorld(e);
            }
        }

        protected virtual void OnFirstMonth()
        {
            foreach (var ev in EventHelper.GetEvents("OnFirstMonth"))
            {
                ev.OnFirstMonth();
            }
        }

        protected virtual void OnMonthly()
        {
            foreach (var ev in EventHelper.GetEvents("OnMonthly"))
            {
                ev.OnMonthly();
            }
        }

        protected virtual void OnSave(ETypeData e)
        {
            foreach (var ev in EventHelper.GetEvents("OnSave"))
            {
                ev.OnSave(e);
            }
            CacheHelper.Save();
            DebugHelper.Save();
        }
        #endregion

        #region EBattleType
        protected virtual void OnBattleUnitInit(UnitInit e)
        {
            foreach (var ev in EventHelper.GetEvents("OnBattleUnitInit"))
            {
                ev.OnBattleUnitInit(e);
            }
        }

        protected virtual void OnBattleUnitHit(UnitHit e)
        {
            foreach (var ev in EventHelper.GetEvents("OnBattleUnitHit"))
            {
                ev.OnBattleUnitHit(e);
            }
        }

        protected virtual void OnBattleUnitHitDynIntHandler(UnitHitDynIntHandler e)
        {
            foreach (var ev in EventHelper.GetEvents("OnBattleUnitHitDynIntHandler"))
            {
                ev.OnBattleUnitHitDynIntHandler(e);
            }
        }

        protected virtual void OnBattleUnitUseProp(UnitUseProp e)
        {
            foreach (var ev in EventHelper.GetEvents("OnBattleUnitUseProp"))
            {
                ev.OnBattleUnitUseProp(e);
            }
        }

        protected virtual void OnBattleUnitUseSkill(UnitUseSkill e)
        {
            foreach (var ev in EventHelper.GetEvents("OnBattleUnitUseSkill"))
            {
                ev.OnBattleUnitUseSkill(e);
            }
        }

        protected virtual void OnBattleUnitUseStep(UnitUseStep e)
        {
            foreach (var ev in EventHelper.GetEvents("OnBattleUnitUseStep"))
            {
                ev.OnBattleUnitUseStep(e);
            }
        }

        protected virtual void OnBattleUnitDie(UnitDie e)
        {
            foreach (var ev in EventHelper.GetEvents("OnBattleUnitDie"))
            {
                ev.OnBattleUnitDie(e);
            }
        }

        protected virtual void OnBattleUnitDieEnd(ETypeData e)
        {
            foreach (var ev in EventHelper.GetEvents("OnBattleUnitDieEnd"))
            {
                ev.OnBattleUnitDieEnd(e);
            }
        }

        protected virtual void OnBattleUnitAddEffectStart(UnitAddEffectStart e)
        {
            foreach (var ev in EventHelper.GetEvents("OnBattleUnitAddEffectStart"))
            {
                ev.OnBattleUnitAddEffectStart(e);
            }
        }

        protected virtual void OnBattleUnitAddEffect(UnitAddEffect e)
        {
            foreach (var ev in EventHelper.GetEvents("OnBattleUnitAddEffect"))
            {
                ev.OnBattleUnitAddEffect(e);
            }
        }

        protected virtual void OnBattleSetUnitType(SetUnitType e)
        {
            foreach (var ev in EventHelper.GetEvents("OnBattleSetUnitType"))
            {
                ev.OnBattleSetUnitType(e);
            }
        }

        protected virtual void OnBattleStart(ETypeData e)
        {
            foreach (var ev in EventHelper.GetEvents("OnBattleStart"))
            {
                ev.OnBattleStart(e);
            }
        }

        protected virtual void OnBattleEnd(BattleEnd e)
        {
            foreach (var ev in EventHelper.GetEvents("OnBattleEnd"))
            {
                ev.OnBattleEnd(e);
            }
        }

        protected virtual void OnBattleExit(ETypeData e)
        {
            foreach (var ev in EventHelper.GetEvents("OnBattleExit"))
            {
                ev.OnBattleExit(e);
            }
        }
        #endregion
        #endregion
    }

    public abstract class ModMaster<T> : ModMaster where T : ModSettings
    {
        protected override void OnInitWorld(ETypeData e)
        {
            ModSettings.CreateIfNotExists<T>();
        }
    }
}