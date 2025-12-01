using EBattleTypeData;
using EGameTypeData;
using ModLib.Attributes;
using ModLib.Object;

namespace ModLib.Mod
{
    // [TraceIgnore]
    public abstract class ModEvent : CachableObject
    {
        public dynamic EventParameter { get; set; }

        #region Timer
        [EventCat("Timer")]
        public virtual void OnTimeUpdate10ms() { }
        [EventCat("Timer")]
        public virtual void OnTimeUpdate100ms() { }
        [EventCat("Timer")]
        public virtual void OnTimeUpdate200ms() { }
        [EventCat("Timer")]
        public virtual void OnTimeUpdate500ms() { }
        [EventCat("Timer")]
        public virtual void OnTimeUpdate1000ms() { }
        #endregion

        #region EMapType
        [EventCat("Map")]
        public virtual void OnPlayerOpenTreeVault(ETypeData e) { EventParameter = e; }
        [EventCat("Map")]
        public virtual void OnPlayerEquipCloth(ETypeData e) { EventParameter = e; }
        [EventCat("Map")]
        public virtual void OnPlayerInMonstArea(ETypeData e) { EventParameter = e; }
        [EventCat("Map")]
        public virtual void OnPlayerRoleEscapeInMap(ETypeData e) { EventParameter = e; }
        [EventCat("Map")]
        public virtual void OnPlayerRoleUpGradeBig(ETypeData e) { EventParameter = e; }
        [EventCat("Map")]
        public virtual void OnUpGradeAndCloseFateFeatureUI(ETypeData e) { EventParameter = e; }
        [EventCat("Map")]
        public virtual void OnUseHobbyProps(ETypeData e) { EventParameter = e; }
        //[EventCat("Map")]
        //public virtual void OnFortuitousTrigger(ETypeData e) { EventParameter = e; }
        #endregion

        #region EGameType
        //[EventCat("Game")]
        //public virtual void OnTownAuctionStart() { }
        [EventCat("Game")]
        public virtual void OnOpenUIStart(OpenUIStart e) { EventParameter = e; }
        [EventCat("Game")]
        public virtual void OnOpenUIEnd(OpenUIEnd e) { EventParameter = e; }
        [EventCat("Game")]
        public virtual void OnCloseUIStart(CloseUIStart e) { EventParameter = e; }
        [EventCat("Game")]
        public virtual void OnCloseUIEnd(CloseUIEnd e) { EventParameter = e; }
        [EventCat("Game")]
        public virtual void OnLoadNewGame() { }
        [EventCat("Game")]
        public virtual void OnLoadGameBefore() { }
        [EventCat("Game")]
        public virtual void OnLoadGame() { }
        [EventCat("Game")]
        public virtual void OnLoadGameAfter() { }
        //[EventCat("Game")]
        //public virtual void OnLoadMapNewGame() { }
        //[EventCat("Game")]
        //public virtual void OnLoadMapFirst() { }
        [EventCat("Game")]
        public virtual void OnInitWorld(ETypeData e) { EventParameter = e; }
        [EventCat("Game")]
        public virtual void OnLoadSceneStart(LoadSceneStart e) { EventParameter = e; }
        [EventCat("Game")]
        public virtual void OnLoadScene(LoadScene e) { EventParameter = e; }
        [EventCat("Game")]
        public virtual void OnIntoWorld(ETypeData e) { EventParameter = e; }
        [EventCat("Game")]
        public virtual void OnFirstMonth() { }
        [EventCat("Game")]
        public virtual void OnYearly() { }
        [EventCat("Game")]
        public virtual void OnMonthly() { }
        [EventCat("Game")]
        public virtual void OnRefreshParameterStoreOnMonthly() { }
        [EventCat("Game")]
        public virtual void OnMonthlyForEachWUnit(WorldUnitBase wunit) { EventParameter = wunit; }
        [EventCat("Game")]
        public virtual void OnSave(ETypeData e) { EventParameter = e; }
        [EventCat("Game")]
        public virtual void OnOpenDrama(OpenDrama e) { EventParameter = e; }
        [EventCat("Game")]
        public virtual void OnOpenNPCInfoUI(OpenNPCInfoUI e) { EventParameter = e; }
        [EventCat("Game")]
        public virtual void OnTaskAdd(TaskAdd e) { EventParameter = e; }
        [EventCat("Game")]
        public virtual void OnTaskComplete(TaskComplete e) { EventParameter = e; }
        [EventCat("Game")]
        public virtual void OnTaskFail(TaskFail e) { EventParameter = e; }
        [EventCat("Game")]
        public virtual void OnTaskGive(TaskGive e) { EventParameter = e; }
        [EventCat("Game")]
        public virtual void OnTaskOverl(TaskOverl e) { EventParameter = e; }
        [EventCat("Game")]
        public virtual void OnUnitSetGrade(ETypeData e) { EventParameter = e; }
        [EventCat("Game")]
        public virtual void OnUnitSetHeartState(ETypeData e) { EventParameter = e; }
        [EventCat("Game")]
        public virtual void OnWorldRunStart(WorldRunStart e) { EventParameter = e; }
        [EventCat("Game")]
        public virtual void OnWorldRunEnd(WorldRunEnd e) { EventParameter = e; }
        [EventCat("Game")]
        public virtual void OnMonoUpdate() { }
        #endregion

        #region EBattleType
        [EventCat("Battle")]
        public virtual void OnBattleUnitInit(UnitInit e) { EventParameter = e; }
        [EventCat("Battle")]
        public virtual void OnBattleUnitHit(UnitHit e) { EventParameter = e; }
        [EventCat("Battle")]
        public virtual void OnBattleUnitHitDynIntHandler(UnitHitDynIntHandler e) { EventParameter = e; }
        [EventCat("Battle")]
        public virtual void OnBattleUnitShieldHitDynIntHandler(UnitShieldHitDynIntHandler e) { EventParameter = e; }
        [EventCat("Battle")]
        public virtual void OnBattleUnitUseProp(UnitUseProp e) { EventParameter = e; }
        [EventCat("Battle")]
        public virtual void OnBattleUnitUseSkill(UnitUseSkill e) { EventParameter = e; }
        [EventCat("Battle")]
        public virtual void OnBattleUnitUseStep(UnitUseStep e) { EventParameter = e; }
        [EventCat("Battle")]
        public virtual void OnBattleUnitDie(UnitDie e) { EventParameter = e; }
        [EventCat("Battle")]
        public virtual void OnBattleUnitDieEnd(ETypeData e) { EventParameter = e; }
        [EventCat("Battle")]
        public virtual void OnBattleUnitAddEffectStart(UnitAddEffectStart e) { EventParameter = e; }
        [EventCat("Battle")]
        public virtual void OnBattleUnitAddEffect(UnitAddEffect e) { EventParameter = e; }
        [EventCat("Battle")]
        public virtual void OnBattleUnitAddHP(UnitAddHP e) { EventParameter = e; }
        [EventCat("Battle")]
        public virtual void OnBattleUnitUpdateProperty(UnitUpdateProperty e) { EventParameter = e; }
        [EventCat("Battle")]
        public virtual void OnBattleSetUnitType(SetUnitType e) { EventParameter = e; }
        [EventCat("Battle")]
        public virtual void OnBattleUnitInto(UnitCtrlBase e) { EventParameter = e; }
        [EventCat("Battle")]
        public virtual void OnBattleStart(ETypeData e) { EventParameter = e; }
        [EventCat("Battle")]
        public virtual void OnBattleEnd(BattleEnd e) { EventParameter = e; }
        [EventCat("Battle")]
        public virtual void OnBattleEndFront(ETypeData e) { EventParameter = e; }
        [EventCat("Battle")]
        public virtual void OnBattleEndHandler(BattleEndHandler e) { EventParameter = e; }
        [EventCat("Battle")]
        public virtual void OnBattleEscapeFailed(ETypeData e) { EventParameter = e; }
        [EventCat("Battle")]
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
