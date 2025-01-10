using EBattleTypeData;
using System.Linq;
using UnityEngine;

namespace ModLib.Mod
{
    public abstract partial class ModMaster : MonoBehaviour
    {
        #region ModLib - Handlers
        public virtual void _OnBattleUnitInit(ETypeData e)
        {
            CallEvents<UnitInit>("OnBattleUnitInit", e);
        }

        public virtual void _OnBattleUnitHit(ETypeData e)
        {
            CallEvents<UnitHit>("OnBattleUnitHit", e);
        }

        public virtual void _OnBattleUnitHitDynIntHandler(ETypeData e)
        {
            CallEvents<UnitHitDynIntHandler>("OnBattleUnitHitDynIntHandler", e);
        }

        public virtual void _OnBattleUnitShieldHitDynIntHandler(ETypeData e)
        {
            CallEvents<UnitShieldHitDynIntHandler>("OnBattleUnitShieldHitDynIntHandler", e);
        }

        public virtual void _OnBattleUnitUseProp(ETypeData e)
        {
            CallEvents<UnitUseProp>("OnBattleUnitUseProp", e);
        }

        public virtual void _OnBattleUnitUseSkill(ETypeData e)
        {
            CallEvents<UnitUseSkill>("OnBattleUnitUseSkill", e);
        }

        public virtual void _OnBattleUnitUseStep(ETypeData e)
        {
            CallEvents<UnitUseStep>("OnBattleUnitUseStep", e);
        }

        public virtual void _OnBattleUnitDie(ETypeData e)
        {
            CallEvents<UnitDie>("OnBattleUnitDie", e);
        }

        public virtual void _OnBattleUnitDieEnd(ETypeData e)
        {
            CallEvents<ETypeData>("OnBattleUnitDieEnd", e);
        }

        public virtual void _OnBattleUnitAddEffectStart(ETypeData e)
        {
            CallEvents<UnitAddEffectStart>("OnBattleUnitAddEffectStart", e);
        }

        public virtual void _OnBattleUnitAddEffect(ETypeData e)
        {
            CallEvents<UnitAddEffect>("OnBattleUnitAddEffect", e);
        }

        public virtual void _OnBattleUnitAddHP(ETypeData e)
        {
            CallEvents<UnitAddHP>("OnBattleUnitAddHP", e);
        }

        public virtual void _OnBattleUnitUpdateProperty(ETypeData e)
        {
            CallEvents<UnitUpdateProperty>("OnBattleUnitUpdateProperty", e);
        }

        public virtual void _OnBattleSetUnitType(ETypeData e)
        {
            CallEvents<SetUnitType>("OnBattleSetUnitType", e);
        }

        public virtual void _OnBattleStart(ETypeData e)
        {
            CallEvents<ETypeData>("OnBattleStart", e);
        }

        public virtual void _OnBattleEnd(ETypeData e)
        {
            CallEvents<BattleEnd>("OnBattleEnd", e);
        }

        public virtual void _OnBattleEndFront(ETypeData e)
        {
            CallEvents<ETypeData>("OnBattleEndFront", e);
        }

        public virtual void _OnBattleEndHandler(ETypeData e)
        {
            CallEvents<BattleEndHandler>("OnBattleEndHandler", e);
        }

        public virtual void _OnBattleEscapeFailed(ETypeData e)
        {
            CallEvents<ETypeData>("OnBattleEscapeFailed", e);
        }

        public virtual void _OnBattleExit(ETypeData e)
        {
            CallEvents<ETypeData>("OnBattleExit", e);
        }
        #endregion

        #region ModLib - Events
        public virtual void OnBattleUnitInit(UnitInit e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnBattleUnitHit(UnitHit e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnBattleUnitHitDynIntHandler(UnitHitDynIntHandler e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnBattleUnitShieldHitDynIntHandler(UnitShieldHitDynIntHandler e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnBattleUnitUseProp(UnitUseProp e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnBattleUnitUseSkill(UnitUseSkill e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnBattleUnitUseStep(UnitUseStep e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnBattleUnitDie(UnitDie e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnBattleUnitDieEnd(ETypeData e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnBattleUnitAddEffectStart(UnitAddEffectStart e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnBattleUnitAddEffect(UnitAddEffect e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnBattleUnitAddHP(UnitAddHP e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnBattleUnitUpdateProperty(UnitUpdateProperty e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnBattleSetUnitType(SetUnitType e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnBattleStart(ETypeData e)
        {
            DebugHelper.WriteLine($"Battle: SoleId:{g.world.battle.data.battleID}, Lvl:{g.world.battle.data.dungeonLevel}, SelfBattle:{g.world.battle.data.isSelfBattle}, RealBattle:{g.world.battle.data.isRealBattle}, RoomCount:{SceneType.battle.room.room.allRoom.Count}, RoomInfo:{string.Join(";", SceneType.battle.room.room.allRoom.ToArray().Select(x => $"(Id:{x.roomBaseItem.id}, Type:{x.roomBaseItem.type}, Width:{x.roomBaseItem.width}, Height:{x.roomBaseItem.height})"))}");
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnBattleEnd(BattleEnd e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnBattleEndFront(ETypeData e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnBattleEndHandler(BattleEndHandler e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnBattleEscapeFailed(ETypeData e)
        {
            EventHelper.RunMinorEvents(e);
        }

        public virtual void OnBattleExit(ETypeData e)
        {
            EventHelper.RunMinorEvents(e);
        }
        #endregion
    }
}