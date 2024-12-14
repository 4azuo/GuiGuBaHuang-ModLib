using EBattleTypeData;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModLib.Mod
{
    public abstract partial class ModMaster : MonoBehaviour
    {
        private static readonly IList<string> battleCheckList = new List<string>();
        private static bool isBattleEnd = true;

        #region ModLib - Handlers
        public virtual void _OnBattleUnitInit(ETypeData e)
        {
            CallEvents<UnitInit>("OnBattleUnitInit", e, true, false);
        }

        public virtual void _OnBattleUnitHit(ETypeData e)
        {
            CallEvents<UnitHit>("OnBattleUnitHit", e, true, false);
        }

        public virtual void _OnBattleUnitHitDynIntHandler(ETypeData e)
        {
            CallEvents<UnitHitDynIntHandler>("OnBattleUnitHitDynIntHandler", e, true, false);
        }

        public virtual void _OnBattleUnitShieldHitDynIntHandler(ETypeData e)
        {
            CallEvents<UnitShieldHitDynIntHandler>("OnBattleUnitShieldHitDynIntHandler", e, true, false);
        }

        public virtual void _OnBattleUnitUseProp(ETypeData e)
        {
            CallEvents<UnitUseProp>("OnBattleUnitUseProp", e, true, false);
        }

        public virtual void _OnBattleUnitUseSkill(ETypeData e)
        {
            CallEvents<UnitUseSkill>("OnBattleUnitUseSkill", e, true, false);
        }

        public virtual void _OnBattleUnitUseStep(ETypeData e)
        {
            CallEvents<UnitUseStep>("OnBattleUnitUseStep", e, true, false);
        }

        public virtual void _OnBattleUnitDie(ETypeData e)
        {
            CallEvents<UnitDie>("OnBattleUnitDie", e, true, false);
        }

        public virtual void _OnBattleUnitDieEnd(ETypeData e)
        {
            CallEvents<ETypeData>("OnBattleUnitDieEnd", e, true, false);
        }

        public virtual void _OnBattleUnitAddEffectStart(ETypeData e)
        {
            CallEvents<UnitAddEffectStart>("OnBattleUnitAddEffectStart", e, true, false);
        }

        public virtual void _OnBattleUnitAddEffect(ETypeData e)
        {
            CallEvents<UnitAddEffect>("OnBattleUnitAddEffect", e, true, false);
        }

        public virtual void _OnBattleUnitAddHP(ETypeData e)
        {
            CallEvents<UnitAddHP>("OnBattleUnitAddHP", e, true, false);
        }

        public virtual void _OnBattleUnitUpdateProperty(ETypeData e)
        {
            CallEvents<UnitUpdateProperty>("OnBattleUnitUpdateProperty", e, true, false);
        }

        public virtual void _OnBattleSetUnitType(ETypeData e)
        {
            var x = e.TryCast<SetUnitType>();
            if (x != null && !battleCheckList.Contains(x.unit.data.createUnitSoleID))
            {
                battleCheckList.Add(x.unit.data.createUnitSoleID);
                CallEvents<UnitCtrlBase>("OnBattleUnitInto", x.unit, true, false);
            }
            CallEvents<SetUnitType>("OnBattleSetUnitType", e, true, false);
        }

        public virtual void _OnBattleStart(ETypeData e)
        {
            isBattleEnd = false;
            CallEvents<ETypeData>("OnBattleStart", e, true, false);
        }

        public virtual void _OnBattleEnd(ETypeData e)
        {
            if (!isBattleEnd)
            {
                CallEvents<BattleEnd>("OnBattleEndOnce", e, true, false);
                isBattleEnd = true;
            }
            CallEvents<BattleEnd>("OnBattleEnd", e, true, false);
            battleCheckList.Clear();
        }

        public virtual void _OnBattleEndFront(ETypeData e)
        {
            CallEvents<ETypeData>("OnBattleEndFront", e, true, false);
        }

        public virtual void _OnBattleEndHandler(ETypeData e)
        {
            CallEvents<BattleEndHandler>("OnBattleEndHandler", e, true, false);
        }

        public virtual void _OnBattleEscapeFailed(ETypeData e)
        {
            CallEvents<ETypeData>("OnBattleEscapeFailed", e, true, false);
        }

        public virtual void _OnBattleExit(ETypeData e)
        {
            CallEvents<ETypeData>("OnBattleExit", e, true, false);
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

        public virtual void OnBattleUnitInto(UnitCtrlBase e)
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

        public virtual void OnBattleEndOnce(BattleEnd e)
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