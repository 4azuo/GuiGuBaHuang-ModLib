using EBattleTypeData;
using EGameTypeData;
using ModLib.Attributes;
using ModLib.Object;

namespace ModLib.Mod
{
    /// <summary>
    /// Base class for mod event handlers.
    /// Provides virtual methods that are called at various game lifecycle points.
    /// Override these methods in derived classes to handle specific game events.
    /// </summary>
    // [TraceIgnore]
    public abstract class ModEvent : CachableObject
    {
        /// <summary>
        /// Gets or sets the dynamic event parameter passed to event handlers.
        /// </summary>
        public dynamic EventParameter { get; set; }

        #region Timer
        /// <summary>Called every 10 milliseconds. Use for very frequent updates.</summary>
        [EventCat("Timer")]
        public virtual void OnTimeUpdate10ms() { }
        /// <summary>Called every 100 milliseconds (0.1 seconds). Use for frequent updates.</summary>
        [EventCat("Timer")]
        public virtual void OnTimeUpdate100ms() { }
        /// <summary>Called every 200 milliseconds (0.2 seconds). Use for regular updates.</summary>
        [EventCat("Timer")]
        public virtual void OnTimeUpdate200ms() { }
        /// <summary>Called every 500 milliseconds (0.5 seconds). Use for moderate updates.</summary>
        [EventCat("Timer")]
        public virtual void OnTimeUpdate500ms() { }
        /// <summary>Called every 1000 milliseconds (1 second). Use for slow updates.</summary>
        [EventCat("Timer")]
        public virtual void OnTimeUpdate1000ms() { }
        #endregion

        #region EMapType
        /// <summary>Called when player opens a tree vault (treasure storage).</summary>
        [EventCat("Map")]
        public virtual void OnPlayerOpenTreeVault(ETypeData e) { EventParameter = e; }
        /// <summary>Called when player equips clothing/armor.</summary>
        [EventCat("Map")]
        public virtual void OnPlayerEquipCloth(ETypeData e) { EventParameter = e; }
        /// <summary>Called when player enters a monster area.</summary>
        [EventCat("Map")]
        public virtual void OnPlayerInMonstArea(ETypeData e) { EventParameter = e; }
        /// <summary>Called when player character escapes from the map.</summary>
        [EventCat("Map")]
        public virtual void OnPlayerRoleEscapeInMap(ETypeData e) { EventParameter = e; }
        /// <summary>Called when player makes a major cultivation level breakthrough.</summary>
        [EventCat("Map")]
        public virtual void OnPlayerRoleUpGradeBig(ETypeData e) { EventParameter = e; }
        /// <summary>Called when upgrading and closing the fate feature UI.</summary>
        [EventCat("Map")]
        public virtual void OnUpGradeAndCloseFateFeatureUI(ETypeData e) { EventParameter = e; }
        /// <summary>Called when player uses hobby-related items.</summary>
        [EventCat("Map")]
        public virtual void OnUseHobbyProps(ETypeData e) { EventParameter = e; }
        //[EventCat("Map")]
        //public virtual void OnFortuitousTrigger(ETypeData e) { EventParameter = e; }
        #endregion

        #region EGameType
        //[EventCat("Game")]
        //public virtual void OnTownAuctionStart() { }
        /// <summary>Called when a UI starts opening.</summary>
        [EventCat("Game")]
        public virtual void OnOpenUIStart(OpenUIStart e) { EventParameter = e; }
        /// <summary>Called when a UI finishes opening.</summary>
        [EventCat("Game")]
        public virtual void OnOpenUIEnd(OpenUIEnd e) { EventParameter = e; }
        /// <summary>Called when a UI starts closing.</summary>
        [EventCat("Game")]
        public virtual void OnCloseUIStart(CloseUIStart e) { EventParameter = e; }
        /// <summary>Called when a UI finishes closing.</summary>
        [EventCat("Game")]
        public virtual void OnCloseUIEnd(CloseUIEnd e) { EventParameter = e; }
        /// <summary>Called when loading a new game.</summary>
        [EventCat("Game")]
        public virtual void OnLoadNewGame() { }
        /// <summary>Called before loading a saved game.</summary>
        [EventCat("Game")]
        public virtual void OnLoadGameBefore() { }
        /// <summary>Called when loading a saved game.</summary>
        [EventCat("Game")]
        public virtual void OnLoadGame() { }
        /// <summary>Called after loading a saved game.</summary>
        [EventCat("Game")]
        public virtual void OnLoadGameAfter() { }
        //[EventCat("Game")]
        //public virtual void OnLoadMapNewGame() { }
        //[EventCat("Game")]
        //public virtual void OnLoadMapFirst() { }
        /// <summary>Called when initializing the game world.</summary>
        [EventCat("Game")]
        public virtual void OnInitWorld(ETypeData e) { EventParameter = e; }
        /// <summary>Called when a scene starts loading.</summary>
        [EventCat("Game")]
        public virtual void OnLoadSceneStart(LoadSceneStart e) { EventParameter = e; }
        /// <summary>Called when a scene finishes loading.</summary>
        [EventCat("Game")]
        public virtual void OnLoadScene(LoadScene e) { EventParameter = e; }
        /// <summary>Called when entering the game world.</summary>
        [EventCat("Game")]
        public virtual void OnIntoWorld(ETypeData e) { EventParameter = e; }
        /// <summary>Called on the first month of the game.</summary>
        [EventCat("Game")]
        public virtual void OnFirstMonth() { }
        /// <summary>Called every in-game year.</summary>
        [EventCat("Game")]
        public virtual void OnYearly() { }
        /// <summary>Called every in-game month.</summary>
        [EventCat("Game")]
        public virtual void OnMonthly() { }
        /// <summary>Called to refresh parameter store every month.</summary>
        [EventCat("Game")]
        public virtual void OnRefreshParameterStoreOnMonthly() { }
        /// <summary>Called monthly for each world unit.</summary>
        [EventCat("Game")]
        public virtual void OnMonthlyForEachWUnit(WorldUnitBase wunit) { EventParameter = wunit; }
        /// <summary>Called when saving the game.</summary>
        [EventCat("Game")]
        public virtual void OnSave(ETypeData e) { EventParameter = e; }
        /// <summary>Called when opening a drama/story dialog.</summary>
        [EventCat("Game")]
        public virtual void OnOpenDrama(OpenDrama e) { EventParameter = e; }
        /// <summary>Called when opening NPC information UI.</summary>
        [EventCat("Game")]
        public virtual void OnOpenNPCInfoUI(OpenNPCInfoUI e) { EventParameter = e; }
        /// <summary>Called when a task is added.</summary>
        [EventCat("Game")]
        public virtual void OnTaskAdd(TaskAdd e) { EventParameter = e; }
        /// <summary>Called when a task is completed.</summary>
        [EventCat("Game")]
        public virtual void OnTaskComplete(TaskComplete e) { EventParameter = e; }
        /// <summary>Called when a task fails.</summary>
        [EventCat("Game")]
        public virtual void OnTaskFail(TaskFail e) { EventParameter = e; }
        /// <summary>Called when a task is given/assigned.</summary>
        [EventCat("Game")]
        public virtual void OnTaskGive(TaskGive e) { EventParameter = e; }
        /// <summary>Called when a task overlaps/conflicts.</summary>
        [EventCat("Game")]
        public virtual void OnTaskOverl(TaskOverl e) { EventParameter = e; }
        /// <summary>Called when unit's cultivation grade is set.</summary>
        [EventCat("Game")]
        public virtual void OnUnitSetGrade(ETypeData e) { EventParameter = e; }
        /// <summary>Called when unit's heart state (affection/relationship) is set.</summary>
        [EventCat("Game")]
        public virtual void OnUnitSetHeartState(ETypeData e) { EventParameter = e; }
        /// <summary>Called when world simulation starts running.</summary>
        [EventCat("Game")]
        public virtual void OnWorldRunStart(WorldRunStart e) { EventParameter = e; }
        /// <summary>Called when world simulation stops running.</summary>
        [EventCat("Game")]
        public virtual void OnWorldRunEnd(WorldRunEnd e) { EventParameter = e; }
        /// <summary>Called on Unity MonoBehaviour Update cycle.</summary>
        [EventCat("Game")]
        public virtual void OnMonoUpdate() { }
        #endregion

        #region EBattleType
        /// <summary>Called when a battle unit is initialized.</summary>
        [EventCat("Battle")]
        public virtual void OnBattleUnitInit(UnitInit e) { EventParameter = e; }
        /// <summary>Called when a battle unit is hit by an attack.</summary>
        [EventCat("Battle")]
        public virtual void OnBattleUnitHit(UnitHit e) { EventParameter = e; }
        /// <summary>Called to handle dynamic integer values when unit is hit.</summary>
        [EventCat("Battle")]
        public virtual void OnBattleUnitHitDynIntHandler(UnitHitDynIntHandler e) { EventParameter = e; }
        /// <summary>Called to handle dynamic integer values when unit's shield is hit.</summary>
        [EventCat("Battle")]
        public virtual void OnBattleUnitShieldHitDynIntHandler(UnitShieldHitDynIntHandler e) { EventParameter = e; }
        /// <summary>Called when a battle unit uses an item/prop.</summary>
        [EventCat("Battle")]
        public virtual void OnBattleUnitUseProp(UnitUseProp e) { EventParameter = e; }
        /// <summary>Called when a battle unit uses a skill.</summary>
        [EventCat("Battle")]
        public virtual void OnBattleUnitUseSkill(UnitUseSkill e) { EventParameter = e; }
        /// <summary>Called when a battle unit uses a movement step.</summary>
        [EventCat("Battle")]
        public virtual void OnBattleUnitUseStep(UnitUseStep e) { EventParameter = e; }
        /// <summary>Called when a battle unit dies.</summary>
        [EventCat("Battle")]
        public virtual void OnBattleUnitDie(UnitDie e) { EventParameter = e; }
        /// <summary>Called after a battle unit's death is processed.</summary>
        [EventCat("Battle")]
        public virtual void OnBattleUnitDieEnd(ETypeData e) { EventParameter = e; }
        /// <summary>Called before an effect is added to a battle unit.</summary>
        [EventCat("Battle")]
        public virtual void OnBattleUnitAddEffectStart(UnitAddEffectStart e) { EventParameter = e; }
        /// <summary>Called when an effect is added to a battle unit.</summary>
        [EventCat("Battle")]
        public virtual void OnBattleUnitAddEffect(UnitAddEffect e) { EventParameter = e; }
        /// <summary>Called when HP is added/removed from a battle unit.</summary>
        [EventCat("Battle")]
        public virtual void OnBattleUnitAddHP(UnitAddHP e) { EventParameter = e; }
        /// <summary>Called when a battle unit's properties are updated.</summary>
        [EventCat("Battle")]
        public virtual void OnBattleUnitUpdateProperty(UnitUpdateProperty e) { EventParameter = e; }
        /// <summary>Called when a battle unit's type is set.</summary>
        [EventCat("Battle")]
        public virtual void OnBattleSetUnitType(SetUnitType e) { EventParameter = e; }
        /// <summary>Called when a unit enters the battle.</summary>
        [EventCat("Battle")]
        public virtual void OnBattleUnitInto(UnitCtrlBase e) { EventParameter = e; }
        /// <summary>Called when a battle starts.</summary>
        [EventCat("Battle")]
        public virtual void OnBattleStart(ETypeData e) { EventParameter = e; }
        /// <summary>Called when a battle ends.</summary>
        [EventCat("Battle")]
        public virtual void OnBattleEnd(BattleEnd e) { EventParameter = e; }
        /// <summary>Called before battle end processing.</summary>
        [EventCat("Battle")]
        public virtual void OnBattleEndFront(ETypeData e) { EventParameter = e; }
        /// <summary>Called to handle battle end logic.</summary>
        [EventCat("Battle")]
        public virtual void OnBattleEndHandler(BattleEndHandler e) { EventParameter = e; }
        /// <summary>Called when battle escape attempt fails.</summary>
        [EventCat("Battle")]
        public virtual void OnBattleEscapeFailed(ETypeData e) { EventParameter = e; }
        /// <summary>Called when exiting a battle.</summary>
        [EventCat("Battle")]
        public virtual void OnBattleExit(ETypeData e) { EventParameter = e; }
        #endregion

        /// <summary>
        /// Sets the parameter store for the parent ModChild.
        /// </summary>
        /// <param name="parameterStore">The parameter store to set</param>
        public void SetModChildParameterStore(ParameterStore parameterStore)
        {
            Parent.SetParameterStore(parameterStore);
        }

        /// <summary>
        /// Gets the parameter store from parent ModChild or falls back to ModMaster.
        /// </summary>
        /// <returns>The parameter store</returns>
        public ParameterStore GetModChildParameterStore()
        {
            return Parent.ParameterStore ?? ModMaster.ModObj.ParameterStore;
        }
    }
}
