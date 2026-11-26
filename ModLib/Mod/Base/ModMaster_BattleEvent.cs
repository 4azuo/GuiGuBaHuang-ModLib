using EBattleTypeData;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ModLib.Helper;

namespace ModLib.Mod
{
    /// <summary>
    /// ModMaster partial: battle (EBattleType) events.
    /// </summary>
    public abstract partial class ModMaster : MonoBehaviour
    {
        private static readonly List<string> battleCheckList = new List<string>();

        #region ModLib - Handlers
        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnBattleUnitInit through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnBattleUnitInit(ETypeData e)
        {
            CallEvents<UnitInit>("OnBattleUnitInit", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnBattleUnitHit through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnBattleUnitHit(ETypeData e)
        {
            CallEvents<UnitHit>("OnBattleUnitHit", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnBattleUnitHitDynIntHandler through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnBattleUnitHitDynIntHandler(ETypeData e)
        {
            CallEvents<UnitHitDynIntHandler>("OnBattleUnitHitDynIntHandler", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnBattleUnitShieldHitDynIntHandler through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnBattleUnitShieldHitDynIntHandler(ETypeData e)
        {
            CallEvents<UnitShieldHitDynIntHandler>("OnBattleUnitShieldHitDynIntHandler", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnBattleUnitUseProp through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnBattleUnitUseProp(ETypeData e)
        {
            CallEvents<UnitUseProp>("OnBattleUnitUseProp", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnBattleUnitUseSkill through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnBattleUnitUseSkill(ETypeData e)
        {
            CallEvents<UnitUseSkill>("OnBattleUnitUseSkill", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnBattleUnitUseStep through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnBattleUnitUseStep(ETypeData e)
        {
            CallEvents<UnitUseStep>("OnBattleUnitUseStep", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnBattleUnitDie through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnBattleUnitDie(ETypeData e)
        {
            CallEvents<UnitDie>("OnBattleUnitDie", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnBattleUnitDieEnd through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnBattleUnitDieEnd(ETypeData e)
        {
            CallEvents<ETypeData>("OnBattleUnitDieEnd", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnBattleUnitAddEffectStart through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnBattleUnitAddEffectStart(ETypeData e)
        {
            CallEvents<UnitAddEffectStart>("OnBattleUnitAddEffectStart", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnBattleUnitAddEffect through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnBattleUnitAddEffect(ETypeData e)
        {
            CallEvents<UnitAddEffect>("OnBattleUnitAddEffect", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnBattleUnitAddHP through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnBattleUnitAddHP(ETypeData e)
        {
            CallEvents<UnitAddHP>("OnBattleUnitAddHP", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnBattleUnitUpdateProperty through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnBattleUnitUpdateProperty(ETypeData e)
        {
            CallEvents<UnitUpdateProperty>("OnBattleUnitUpdateProperty", e);
        }

        /// <summary>
        /// SetUnitType callback: the first time a unit gets its type in a battle, raises OnBattleUnitInto (unit entered battle).
        /// </summary>
        public virtual void _OnBattleSetUnitType(ETypeData e)
        {
            var x = e.TryCast<SetUnitType>();
            if (x != null && !battleCheckList.Contains(x.unit.data.createUnitSoleID))
            {
                battleCheckList.Add(x.unit.data.createUnitSoleID);
                CallEvents<UnitCtrlBase>("OnBattleUnitInto", x.unit);
            }
            CallEvents<SetUnitType>("OnBattleSetUnitType", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnBattleStart through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnBattleStart(ETypeData e)
        {
            CallEvents<ETypeData>("OnBattleStart", e);
        }

        /// <summary>
        /// Battle end callback: dispatches OnBattleEnd and clears the "unit entered" list.
        /// </summary>
        public virtual void _OnBattleEnd(ETypeData e)
        {
            CallEvents<BattleEnd>("OnBattleEnd", e);
            battleCheckList.Clear();
            BalanceStatsHelper.FlushAll();
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnBattleEndFront through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnBattleEndFront(ETypeData e)
        {
            CallEvents<ETypeData>("OnBattleEndFront", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnBattleEndHandler through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnBattleEndHandler(ETypeData e)
        {
            CallEvents<BattleEndHandler>("OnBattleEndHandler", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnBattleEscapeFailed through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnBattleEscapeFailed(ETypeData e)
        {
            CallEvents<ETypeData>("OnBattleEscapeFailed", e);
        }

        /// <summary>
        /// Native game callback (registered in Init()). Forwards the event to OnBattleExit through CallEvents, which logs timing and catches exceptions.
        /// </summary>
        public virtual void _OnBattleExit(ETypeData e)
        {
            CallEvents<ETypeData>("OnBattleExit", e);
        }
        #endregion

        #region ModLib - Events
        /// <summary>
        /// Dispatches OnBattleUnitInit to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnBattleUnitInit(UnitInit e)
        {
            EventHelper.RunMinorEvents("OnBattleUnitInit", e);
        }

        /// <summary>
        /// Dispatches OnBattleUnitHit to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnBattleUnitHit(UnitHit e)
        {
            EventHelper.RunMinorEvents("OnBattleUnitHit", e);
        }

        /// <summary>
        /// Dispatches OnBattleUnitHitDynIntHandler to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnBattleUnitHitDynIntHandler(UnitHitDynIntHandler e)
        {
            EventHelper.RunMinorEvents("OnBattleUnitHitDynIntHandler", e);
        }

        /// <summary>
        /// Dispatches OnBattleUnitShieldHitDynIntHandler to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnBattleUnitShieldHitDynIntHandler(UnitShieldHitDynIntHandler e)
        {
            EventHelper.RunMinorEvents("OnBattleUnitShieldHitDynIntHandler", e);
        }

        /// <summary>
        /// Dispatches OnBattleUnitUseProp to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnBattleUnitUseProp(UnitUseProp e)
        {
            EventHelper.RunMinorEvents("OnBattleUnitUseProp", e);
        }

        /// <summary>
        /// Dispatches OnBattleUnitUseSkill to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnBattleUnitUseSkill(UnitUseSkill e)
        {
            EventHelper.RunMinorEvents("OnBattleUnitUseSkill", e);
        }

        /// <summary>
        /// Dispatches OnBattleUnitUseStep to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnBattleUnitUseStep(UnitUseStep e)
        {
            EventHelper.RunMinorEvents("OnBattleUnitUseStep", e);
        }

        /// <summary>
        /// Dispatches OnBattleUnitDie to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnBattleUnitDie(UnitDie e)
        {
            EventHelper.RunMinorEvents("OnBattleUnitDie", e);
        }

        /// <summary>
        /// Dispatches OnBattleUnitDieEnd to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnBattleUnitDieEnd(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnBattleUnitDieEnd", e);
        }

        /// <summary>
        /// Dispatches OnBattleUnitAddEffectStart to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnBattleUnitAddEffectStart(UnitAddEffectStart e)
        {
            EventHelper.RunMinorEvents("OnBattleUnitAddEffectStart", e);
        }

        /// <summary>
        /// Dispatches OnBattleUnitAddEffect to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnBattleUnitAddEffect(UnitAddEffect e)
        {
            EventHelper.RunMinorEvents("OnBattleUnitAddEffect", e);
        }

        /// <summary>
        /// Dispatches OnBattleUnitAddHP to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnBattleUnitAddHP(UnitAddHP e)
        {
            EventHelper.RunMinorEvents("OnBattleUnitAddHP", e);
        }

        /// <summary>
        /// Dispatches OnBattleUnitUpdateProperty to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnBattleUnitUpdateProperty(UnitUpdateProperty e)
        {
            EventHelper.RunMinorEvents("OnBattleUnitUpdateProperty", e);
        }

        /// <summary>
        /// Dispatches OnBattleSetUnitType to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnBattleSetUnitType(SetUnitType e)
        {
            EventHelper.RunMinorEvents("OnBattleSetUnitType", e);
        }

        /// <summary>
        /// Logs the unit details and dispatches OnBattleUnitInto (unit spawned in battle).
        /// </summary>
        public virtual void OnBattleUnitInto(UnitCtrlBase e)
        {
            var monst = e.TryCast<UnitCtrlMonst>();
            //Ignore because can not be log here: "IsFairy:{ e.IsFairy()}, IsSummoned: { e.IsSummoned()}, IsSectGuardian: { e.IsSectGuardian()}, IsHerd: { e.IsHerd()}, IsPotmon: { e.IsPotmon()}"
            DebugHelper.WriteLine($"Unit: SoleId:{e.data.createUnitSoleID}, IsEnemy:{e.IsEnemy(UnitType.Player)}, IsWorldUnit:{e.IsWorldUnit()}, IsPlayer:{e.IsPlayer()}, IsNPC:{e.IsNPC()}, IsHuman:{e.IsHuman()}, IsMonster:{e.IsMonster()}, IsMonsterHuman:{e.IsMonsterHuman()}, UnitType:{e.data.unitType}, MonstType:{monst?.data.monstType}, MonstId:{monst?.data.unitAttrItem.id}, MonstName:{monst?.data.unitAttrItem.name}");
            EventHelper.RunMinorEvents("OnBattleUnitInto", e);
        }

        /// <summary>
        /// Logs the battle/dungeon details and dispatches OnBattleStart.
        /// </summary>
        public virtual void OnBattleStart(ETypeData e)
        {
            DebugHelper.WriteLine($"Battle: SoleId:{g.world.battle.data.battleID}, DungeonId:{g.world.battle.data.dungeonBaseItem.id}, DungeonName:{g.world.battle.data.dungeonBaseItem.name}, SceneBaseID:{g.world.battle.data.dungeonBaseItem.sceneBaseID}, ScaleBaseID:{g.world.battle.data.dungeonBaseItem.scaleBaseID}, Lvl:{g.world.battle.data.dungeonLevel}, SelfBattle:{g.world.battle.data.isSelfBattle}, RealBattle:{g.world.battle.data.isRealBattle}, RoomCount:{SceneType.battle.room.room.allRoom.Count}, RoomInfo:{string.Join(";", SceneType.battle.room.room.allRoom.ToArray().Select(x => $"(Id:{x.roomBaseItem.id}, Type:{x.roomBaseItem.type}, Width:{x.roomBaseItem.width}, Height:{x.roomBaseItem.height})"))}");
            EventHelper.RunMinorEvents("OnBattleStart", e);
        }

        /// <summary>
        /// Dispatches OnBattleEnd to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnBattleEnd(BattleEnd e)
        {
            EventHelper.RunMinorEvents("OnBattleEnd", e);
        }

        /// <summary>
        /// Dispatches OnBattleEndFront to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnBattleEndFront(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnBattleEndFront", e);
        }

        /// <summary>
        /// Dispatches OnBattleEndHandler to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnBattleEndHandler(BattleEndHandler e)
        {
            EventHelper.RunMinorEvents("OnBattleEndHandler", e);
        }

        /// <summary>
        /// Dispatches OnBattleEscapeFailed to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnBattleEscapeFailed(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnBattleEscapeFailed", e);
        }

        /// <summary>
        /// Dispatches OnBattleExit to every loaded ModEvent that overrides it (EventHelper.RunMinorEvents honours EventCondition and delays).
        /// </summary>
        public virtual void OnBattleExit(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnBattleExit", e);
        }
        #endregion
    }
}