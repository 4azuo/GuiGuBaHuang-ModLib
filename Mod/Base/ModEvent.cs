using EBattleTypeData;
using EGameTypeData;
using ModLib.Object;

namespace ModLib.Mod
{
    // [TraceIgnore]
    public abstract class ModEvent : CachableObject
    {
        public dynamic EventParameter { get; set; }

        #region Timer
        public virtual void OnTimeUpdate10ms() { }
        public virtual void OnTimeUpdate100ms() { }
        public virtual void OnTimeUpdate200ms() { }
        public virtual void OnTimeUpdate500ms() { }
        public virtual void OnTimeUpdate1000ms() { }
        #endregion

        #region EMapType
        public virtual void OnPlayerOpenTreeVault(ETypeData e) { EventParameter = e; }
        public virtual void OnPlayerEquipCloth(ETypeData e) { EventParameter = e; }
        public virtual void OnPlayerInMonstArea(ETypeData e) { EventParameter = e; }
        public virtual void OnPlayerRoleEscapeInMap(ETypeData e) { EventParameter = e; }
        public virtual void OnPlayerRoleUpGradeBig(ETypeData e) { EventParameter = e; }
        public virtual void OnUpGradeAndCloseFateFeatureUI(ETypeData e) { EventParameter = e; }
        public virtual void OnUseHobbyProps(ETypeData e) { EventParameter = e; }
        //public virtual void OnFortuitousTrigger(ETypeData e) { EventParameter = e; }
        #endregion

        #region EGameType
        //public virtual void OnTownAuctionStart() { }
        public virtual void OnOpenUIStart(OpenUIStart e) { EventParameter = e; }
        public virtual void OnOpenUIEnd(OpenUIEnd e) { EventParameter = e; }
        public virtual void OnCloseUIStart(CloseUIStart e) { EventParameter = e; }
        public virtual void OnCloseUIEnd(CloseUIEnd e) { EventParameter = e; }
        public virtual void OnLoadNewGame() { }
        public virtual void OnLoadGameBefore() { }
        public virtual void OnLoadGame() { }
        public virtual void OnLoadGameAfter() { }
        //public virtual void OnLoadMapNewGame() { }
        //public virtual void OnLoadMapFirst() { }
        public virtual void OnInitWorld(ETypeData e) { EventParameter = e; }
        public virtual void OnLoadSceneStart(LoadSceneStart e) { EventParameter = e; }
        public virtual void OnLoadScene(LoadScene e) { EventParameter = e; }
        public virtual void OnIntoWorld(ETypeData e) { EventParameter = e; }
        public virtual void OnFirstMonth() { }
        public virtual void OnYearly() { }
        public virtual void OnMonthly() { }
        public virtual void OnRefreshParameterStoreOnMonthly() { }
        public virtual void OnMonthlyForEachWUnit(WorldUnitBase wunit) { EventParameter = wunit; }
        public virtual void OnSave(ETypeData e) { EventParameter = e; }
        public virtual void OnOpenDrama(OpenDrama e) { EventParameter = e; }
        public virtual void OnOpenNPCInfoUI(OpenNPCInfoUI e) { EventParameter = e; }
        public virtual void OnTaskAdd(TaskAdd e) { EventParameter = e; }
        public virtual void OnTaskComplete(TaskComplete e) { EventParameter = e; }
        public virtual void OnTaskFail(TaskFail e) { EventParameter = e; }
        public virtual void OnTaskGive(TaskGive e) { EventParameter = e; }
        public virtual void OnTaskOverl(TaskOverl e) { EventParameter = e; }
        public virtual void OnUnitSetGrade(ETypeData e) { EventParameter = e; }
        public virtual void OnUnitSetHeartState(ETypeData e) { EventParameter = e; }
        public virtual void OnWorldRunStart(WorldRunStart e) { EventParameter = e; }
        public virtual void OnWorldRunEnd(WorldRunEnd e) { EventParameter = e; }
        public virtual void OnMonoUpdate() { }
        #endregion

        #region EBattleType
        public virtual void OnBattleUnitInit(UnitInit e) { EventParameter = e; }
        public virtual void OnBattleUnitHit(UnitHit e) { EventParameter = e; }
        public virtual void OnBattleUnitHitDynIntHandler(UnitHitDynIntHandler e) { EventParameter = e; }
        public virtual void OnBattleUnitShieldHitDynIntHandler(UnitShieldHitDynIntHandler e) { EventParameter = e; }
        public virtual void OnBattleUnitUseProp(UnitUseProp e) { EventParameter = e; }
        public virtual void OnBattleUnitUseSkill(UnitUseSkill e) { EventParameter = e; }
        public virtual void OnBattleUnitUseStep(UnitUseStep e) { EventParameter = e; }
        public virtual void OnBattleUnitDie(UnitDie e) { EventParameter = e; }
        public virtual void OnBattleUnitDieEnd(ETypeData e) { EventParameter = e; }
        public virtual void OnBattleUnitAddEffectStart(UnitAddEffectStart e) { EventParameter = e; }
        public virtual void OnBattleUnitAddEffect(UnitAddEffect e) { EventParameter = e; }
        public virtual void OnBattleUnitAddHP(UnitAddHP e) { EventParameter = e; }
        public virtual void OnBattleUnitUpdateProperty(UnitUpdateProperty e) { EventParameter = e; }
        public virtual void OnBattleSetUnitType(SetUnitType e) { EventParameter = e; }
        public virtual void OnBattleUnitInto(UnitCtrlBase e) { EventParameter = e; }
        public virtual void OnBattleStart(ETypeData e) { EventParameter = e; }
        public virtual void OnBattleEnd(BattleEnd e) { EventParameter = e; }
        public virtual void OnBattleEndFront(ETypeData e) { EventParameter = e; }
        public virtual void OnBattleEndHandler(BattleEndHandler e) { EventParameter = e; }
        public virtual void OnBattleEscapeFailed(ETypeData e) { EventParameter = e; }
        public virtual void OnBattleExit(ETypeData e) { EventParameter = e; }
        #endregion

        public void SetModChildParameterStore(ParameterStore parameterStore)
        {
            Parent.SetParameterStore(parameterStore);
        }

        public ParameterStore GetModChildParameterStore()
        {
            return Parent.ParameterStore ?? ModMaster.ModObj.ParameterStore;
        }
    }
}
