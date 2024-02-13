using EBattleTypeData;
using EGameTypeData;
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
            var callUnitInit = (Il2CppSystem.Action<ETypeData>)_OnUnitInit;
            g.events.On(EBattleType.UnitInit, callUnitInit);

            var callUnitHit = (Il2CppSystem.Action<ETypeData>)_OnUnitHit;
            g.events.On(EBattleType.UnitHit, callUnitHit);

            var callUnitUseProp = (Il2CppSystem.Action<ETypeData>)_OnUnitUseProp;
            g.events.On(EBattleType.UnitUseProp, callUnitUseProp);

            var callUnitUseSkill = (Il2CppSystem.Action<ETypeData>)_OnUnitUseSkill;
            g.events.On(EBattleType.UnitUseSkill, callUnitUseSkill);

            var callUnitUseStep = (Il2CppSystem.Action<ETypeData>)_OnUnitUseStep;
            g.events.On(EBattleType.UnitUseStep, callUnitUseStep);

            var callUnitDie = (Il2CppSystem.Action<ETypeData>)_OnUnitDie;
            g.events.On(EBattleType.UnitDie, callUnitDie);

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

        #region EGameType
        protected virtual void _OnOpenUIStart(ETypeData edata)
        {
            var e = new OpenUIStart(edata.Pointer);

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
                var e = new OpenUIEnd(edata.Pointer);

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
                var e = new CloseUIStart(edata.Pointer);

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
                var e = new CloseUIEnd(edata.Pointer);

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
                    var e = new LoadScene(edata.Pointer);

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
        protected virtual void _OnUnitInit(ETypeData edata)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    var e = new UnitInit(edata.Pointer);

                    OnUnitInit(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnUnitHit(ETypeData edata)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    var e = new UnitHit(edata.Pointer);

                    OnUnitHit(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnUnitUseProp(ETypeData edata)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    var e = new UnitUseProp(edata.Pointer);

                    OnUnitUseProp(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnUnitUseSkill(ETypeData edata)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    var e = new UnitUseSkill(edata.Pointer);

                    OnUnitUseSkill(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnUnitUseStep(ETypeData edata)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    var e = new UnitUseStep(edata.Pointer);

                    OnUnitUseStep(e);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }
        }

        protected virtual void _OnUnitDie(ETypeData edata)
        {
            if (GameHelper.IsInGame())
            {
                try
                {
                    var e = new UnitDie(edata.Pointer);

                    OnUnitDie(e);
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
                    var e = new BattleEnd(edata.Pointer);

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
        protected virtual void OnUnitInit(UnitInit e)
        {
            foreach (var ev in EventHelper.GetEvents("OnUnitInit"))
            {
                ev.OnUnitInit(e);
            }
        }

        protected virtual void OnUnitHit(UnitHit e)
        {
            foreach (var ev in EventHelper.GetEvents("OnUnitHit"))
            {
                ev.OnUnitHit(e);
            }
        }

        protected virtual void OnUnitUseProp(UnitUseProp e)
        {
            foreach (var ev in EventHelper.GetEvents("OnUnitUseProp"))
            {
                ev.OnUnitUseProp(e);
            }
        }

        protected virtual void OnUnitUseSkill(UnitUseSkill e)
        {
            foreach (var ev in EventHelper.GetEvents("OnUnitUseSkill"))
            {
                ev.OnUnitUseSkill(e);
            }
        }

        protected virtual void OnUnitUseStep(UnitUseStep e)
        {
            foreach (var ev in EventHelper.GetEvents("OnUnitUseStep"))
            {
                ev.OnUnitUseStep(e);
            }
        }

        protected virtual void OnUnitDie(UnitDie e)
        {
            foreach (var ev in EventHelper.GetEvents("OnUnitDie"))
            {
                ev.OnUnitDie(e);
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