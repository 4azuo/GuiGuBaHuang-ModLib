using EBattleTypeData;
using EGameTypeData;
using ModLib.Object;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ModLib.Mod
{
    [TraceIgnore]
    public abstract class ModEvent : CachableObject
    {
        [JsonIgnore]
        public virtual int OrderIndex
        {
            get;
        } = -1;
        [JsonIgnore]
        public static ModEvent LastestObject { get; private set; }

        public ModEvent()
        {
            LastestObject = this;
        }

        #region Private methods
        public bool IsLoading()
        {
            return 
                ModMaster.ModObj.InGameSettings.LoadGameBefore ||
                ModMaster.ModObj.InGameSettings.LoadGame ||
                ModMaster.ModObj.InGameSettings.LoadGameAfter ||
                ModMaster.ModObj.InGameSettings.LoadGameFirst;
        }
        #endregion

        #region Timer
        public virtual void OnTimeUpdate() { }
        public virtual void OnTimeUpdate200ms() { }
        public virtual void OnTimeUpdate500ms() { }
        public virtual void OnTimeUpdate1s() { }
        #endregion

        #region EMapType
        public virtual void OnPlayerOpenTreeVault(ETypeData e) { }
        public virtual void OnPlayerEquipCloth(ETypeData e) { }
        public virtual void OnPlayerInMonstArea(ETypeData e) { }
        public virtual void OnPlayerRoleEscapeInMap(ETypeData e) { }
        public virtual void OnPlayerRoleUpGradeBig(ETypeData e) { }
        public virtual void OnUpGradeAndCloseFateFeatureUI(ETypeData e) { }
        public virtual void OnUseHobbyProps(ETypeData e) { }
        //public virtual void OnFortuitousTrigger(ETypeData e) { }
        #endregion

        #region EGameType
        //public virtual void OnTownAuctionStart() { }
        public virtual void OnOpenUIStart(OpenUIStart e) { }
        public virtual void OnOpenUIEnd(OpenUIEnd e) { }
        public virtual void OnCloseUIStart(CloseUIStart e) { }
        public virtual void OnCloseUIEnd(CloseUIEnd e) { }
        public virtual void OnLoadGlobal() { }
        public virtual void OnLoadGameBefore() { }
        public virtual void OnLoadGame() { }
        public virtual void OnLoadGameAfter() { }
        public virtual void OnLoadGameFirst() { }
        public virtual void OnInitWorld(ETypeData e) { }
        public virtual void OnLoadMapNewGame(LoadScene e) { }
        public virtual void OnLoadMapFirst(LoadScene e) { }
        public virtual void OnLoadSceneStart(LoadScene e) { }
        public virtual void OnLoadScene(LoadScene e) { }
        public virtual void OnIntoWorld(ETypeData e) { }
        public virtual void OnFirstMonth() { }
        public virtual void OnYearly() { }
        public virtual void OnMonthly() { }
        public virtual void OnSave(ETypeData e) { }
        public virtual void OnOpenDrama(OpenDrama e) { }
        public virtual void OnOpenNPCInfoUI(OpenNPCInfoUI e) { }
        public virtual void OnTaskAdd(TaskAdd e) { }
        public virtual void OnTaskComplete(TaskComplete e) { }
        public virtual void OnTaskFail(TaskFail e) { }
        public virtual void OnTaskGive(TaskGive e) { }
        public virtual void OnTaskOverl(TaskOverl e) { }
        public virtual void OnUnitSetGrade(ETypeData e) { }
        public virtual void OnUnitSetHeartState(ETypeData e) { }
        public virtual void OnWorldRunStart() { }
        public virtual void OnWorldRunEnd() { }
        #endregion

        #region EBattleType
        public virtual void OnBattleUnitInit(UnitInit e) { }
        public virtual void OnBattleUnitHit(UnitHit e) { }
        public virtual void OnBattleUnitHitDynIntHandler(UnitHitDynIntHandler e) { }
        public virtual void OnBattleUnitShieldHitDynIntHandler(UnitShieldHitDynIntHandler e) { }
        public virtual void OnBattleUnitUseProp(UnitUseProp e) { }
        public virtual void OnBattleUnitUseSkill(UnitUseSkill e) { }
        public virtual void OnBattleUnitUseStep(UnitUseStep e) { }
        public virtual void OnBattleUnitDie(UnitDie e) { }
        public virtual void OnBattleUnitDieEnd(ETypeData e) { }
        public virtual void OnBattleUnitAddEffectStart(UnitAddEffectStart e) { }
        public virtual void OnBattleUnitAddEffect(UnitAddEffect e) { }
        public virtual void OnBattleUnitAddHP(UnitAddHP e) { }
        public virtual void OnBattleUnitUpdateProperty(UnitUpdateProperty e) { }
        public virtual void OnBattleSetUnitType(SetUnitType e) { }
        public virtual void OnBattleUnitInto(UnitCtrlBase e) { }
        public virtual void OnBattleStart(ETypeData e) { }
        public virtual void OnBattleEnd(BattleEnd e) { }
        public virtual void OnBattleEndOnce(BattleEnd e) { }
        public virtual void OnBattleEndFront(ETypeData e) { }
        public virtual void OnBattleEndHandler(BattleEndHandler e) { }
        public virtual void OnBattleEscapeFailed(ETypeData e) { }
        public virtual void OnBattleExit(ETypeData e) { }
        #endregion
    }
}
