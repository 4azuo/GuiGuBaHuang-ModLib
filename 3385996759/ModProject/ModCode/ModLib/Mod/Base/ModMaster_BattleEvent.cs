using EBattleTypeData;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModLib.Mod
{
    public abstract partial class ModMaster : MonoBehaviour
    {
        private static readonly List<string> battleCheckList = new List<string>();
        private static bool isBattleEnd = true;

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
            var x = e.TryCast<SetUnitType>();
            if (x != null && !battleCheckList.Contains(x.unit.data.createUnitSoleID))
            {
                battleCheckList.Add(x.unit.data.createUnitSoleID);
                CallEvents<UnitCtrlBase>("OnBattleUnitInto", x.unit);
            }
            CallEvents<SetUnitType>("OnBattleSetUnitType", e);
        }

        public virtual void _OnBattleStart(ETypeData e)
        {
            isBattleEnd = false;
            CallEvents<ETypeData>("OnBattleStart", e);
        }

        public virtual void _OnBattleEnd(ETypeData e)
        {
            if (!isBattleEnd)
            {
                CallEvents<BattleEnd>("OnBattleEndOnce", e);
                isBattleEnd = true;
            }
            CallEvents<BattleEnd>("OnBattleEnd", e);
            battleCheckList.Clear();
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
            EventHelper.RunMinorEvents("OnBattleUnitInit", e);
        }

        public virtual void OnBattleUnitHit(UnitHit e)
        {
            EventHelper.RunMinorEvents("OnBattleUnitHit", e);
        }

        public virtual void OnBattleUnitHitDynIntHandler(UnitHitDynIntHandler e)
        {
            EventHelper.RunMinorEvents("OnBattleUnitHitDynIntHandler", e);
        }

        public virtual void OnBattleUnitShieldHitDynIntHandler(UnitShieldHitDynIntHandler e)
        {
            EventHelper.RunMinorEvents("OnBattleUnitShieldHitDynIntHandler", e);
        }

        public virtual void OnBattleUnitUseProp(UnitUseProp e)
        {
            EventHelper.RunMinorEvents("OnBattleUnitUseProp", e);
        }

        public virtual void OnBattleUnitUseSkill(UnitUseSkill e)
        {
            EventHelper.RunMinorEvents("OnBattleUnitUseSkill", e);
        }

        public virtual void OnBattleUnitUseStep(UnitUseStep e)
        {
            EventHelper.RunMinorEvents("OnBattleUnitUseStep", e);
        }

        public virtual void OnBattleUnitDie(UnitDie e)
        {
            EventHelper.RunMinorEvents("OnBattleUnitDie", e);
        }

        public virtual void OnBattleUnitDieEnd(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnBattleUnitDieEnd", e);
        }

        public virtual void OnBattleUnitAddEffectStart(UnitAddEffectStart e)
        {
            EventHelper.RunMinorEvents("OnBattleUnitAddEffectStart", e);
        }

        public virtual void OnBattleUnitAddEffect(UnitAddEffect e)
        {
            EventHelper.RunMinorEvents("OnBattleUnitAddEffect", e);
        }

        public virtual void OnBattleUnitAddHP(UnitAddHP e)
        {
            EventHelper.RunMinorEvents("OnBattleUnitAddHP", e);
        }

        public virtual void OnBattleUnitUpdateProperty(UnitUpdateProperty e)
        {
            EventHelper.RunMinorEvents("OnBattleUnitUpdateProperty", e);
        }

        public virtual void OnBattleSetUnitType(SetUnitType e)
        {
            EventHelper.RunMinorEvents("OnBattleSetUnitType", e);
        }

        public virtual void OnBattleUnitInto(UnitCtrlBase e)
        {
            var monst = e.TryCast<UnitCtrlMonst>();
            DebugHelper.WriteLine($"Unit: SoleId:{e.data.createUnitSoleID}, IsEnemy:{e.IsEnemy(UnitType.Player)}, IsWorldUnit:{e.IsWorldUnit()}, IsPlayer:{e.IsPlayer()}, IsNPC:{e.IsNPC()}, IsHuman:{e.IsHuman()}, IsMonster:{e.IsMonster()}, IsMonsterHuman:{e.IsMonsterHuman()}, IsFairy:{e.IsFairy()}, IsHerd:{e.IsHerd()}, IsPotmon:{e.IsPotmon()}, UnitType:{e.data.unitType}, MonstType:{monst?.data.monstType}, MonstId:{monst?.data.unitAttrItem.id}, MonstName:{monst?.data.unitAttrItem.name}");
            EventHelper.RunMinorEvents("OnBattleUnitInto", e);
        }

        public virtual void OnBattleStart(ETypeData e)
        {
            DebugHelper.WriteLine($"Battle: SoleId:{g.world.battle.data.battleID}, DungeonId:{g.world.battle.data.dungeonBaseItem.id}, DungeonName:{g.world.battle.data.dungeonBaseItem.name}, Lvl:{g.world.battle.data.dungeonLevel}, SelfBattle:{g.world.battle.data.isSelfBattle}, RealBattle:{g.world.battle.data.isRealBattle}, RoomCount:{SceneType.battle.room.room.allRoom.Count}, RoomInfo:{string.Join(";", SceneType.battle.room.room.allRoom.ToArray().Select(x => $"(Id:{x.roomBaseItem.id}, Type:{x.roomBaseItem.type}, Width:{x.roomBaseItem.width}, Height:{x.roomBaseItem.height})"))}");
            EventHelper.RunMinorEvents("OnBattleStart", e);
        }

        public virtual void OnBattleEnd(BattleEnd e)
        {
            EventHelper.RunMinorEvents("OnBattleEnd", e);
        }

        public virtual void OnBattleEndOnce(BattleEnd e)
        {
            EventHelper.RunMinorEvents("OnBattleEndOnce", e);
            DebugHelper.Save();
        }

        public virtual void OnBattleEndFront(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnBattleEndFront", e);
        }

        public virtual void OnBattleEndHandler(BattleEndHandler e)
        {
            EventHelper.RunMinorEvents("OnBattleEndHandler", e);
        }

        public virtual void OnBattleEscapeFailed(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnBattleEscapeFailed", e);
        }

        public virtual void OnBattleExit(ETypeData e)
        {
            EventHelper.RunMinorEvents("OnBattleExit", e);
        }
        #endregion
    }
}