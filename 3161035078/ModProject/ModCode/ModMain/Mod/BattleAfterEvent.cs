using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.BATTLE_AFTER_EVENT)]
    public class BattleAfterEvent : ModEvent
    {
        public static BattleAfterEvent Instance { get; set; }

        public static readonly int[] enemyInTraits = new int[]
        {
            UnitTraitEnum.Wicked.Parse<int>(),
            UnitTraitEnum.Selfish.Parse<int>(),
            UnitTraitEnum.Evil.Parse<int>(),
            UnitTraitEnum.Glory_Hound.Parse<int>(),
            UnitTraitEnum.Power_hungry.Parse<int>()
        };

        public static List<DataProps.PropsData> dropItems;
        public static List<UnitCtrlBase> teamMember;
        public static List<WorldUnitBase> aroundUnits;

        public const int DUNGEON_BASE_ID = 480110993;
        public const float JOIN_DISPUTE_RATE = 30f;

        public override void OnBattleEndOnce(BattleEnd e)
        {
            base.OnBattleEndOnce(e);

            dropItems = ModBattleEvent.SceneBattle.battleData.allDropRewardItem.ToList();
            var itemMaxLevel = dropItems.Max(x => x.propsInfoBase.level);
            if (itemMaxLevel > 2)
            {
                var itemMaxGrade = ModBattleEvent.SceneBattle.battleData.allDropRewardItem.ToArray().Max(x => x.propsInfoBase.grade);
                var robRate = itemMaxGrade * 10 + Math.Pow(1.7, itemMaxLevel);

                var player = g.world.playerUnit;
                var playerGrade = player.GetGradeLvl();
                aroundUnits = player.GetUnitsAround(playerGrade, true, false).ToArray().Where(x => IsNoticed(x) && robRate > x.GetGradeLvl() * 10).ToList();
                foreach (var wunit in aroundUnits.ToArray())
                {
                    if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, JOIN_DISPUTE_RATE))
                    {
                        wunit.data.unitData.relationData.AddHate(player.GetUnitId(), 100); //hate player
                        aroundUnits.Remove(wunit); //wont attack
                    }
                }
                if (aroundUnits.Count > 0)
                {
                    g.world.battle.IntoBattle(new DataMap.MonstData() { id = DUNGEON_BASE_ID, level = player.GetUnitPosAreaId() * 5 });
                }
            }
        }

        public override void OnBattleStart(ETypeData e)
        {
            base.OnBattleStart(e);

            aroundUnits = null;
            teamMember = null;
            if (g.world.battle.data.isRealBattle && IsDispute())
            {
                //team member join
                teamMember = NPCJoin(UnitType.PlayerNPC, HirePeopleEvent.GetTeamDetailData(g.world.playerUnit).Item2.Where(x => !x.isDie && !x.IsPlayer()).ToArray());
            }
        }

        private bool IsNoticed(WorldUnitBase wunit)
        {
            return UnitPowerCalHelper.CalWUnitBattlePower(g.world.playerUnit) < UnitPowerCalHelper.CalWUnitBattlePower(wunit) &&
                wunit.GetDynProperty(UnitDynPropertyEnum.Hp).value > (wunit.GetDynProperty(UnitDynPropertyEnum.HpMax).value * 0.7f) &&
                wunit.GetDynProperty(UnitDynPropertyEnum.Mp).value > (wunit.GetDynProperty(UnitDynPropertyEnum.MpMax).value * 0.5f) &&
                wunit.GetDynProperty(UnitDynPropertyEnum.Sp).value > (wunit.GetDynProperty(UnitDynPropertyEnum.SpMax).value * 0.3f) &&
                (
                    enemyInTraits.Contains(wunit.data.unitData.propertyData.inTrait) ||
                    wunit.data.unitData.relationData.GetIntim(g.world.playerUnit) <= -200
                );
        }

        private List<UnitCtrlBase> NPCJoin(UnitType ut, params WorldUnitBase[] wunits)
        {
            var rs = new List<UnitCtrlBase>();
            var wpos = g.world.playerUnit.GetUnitPos();
            foreach (var wunit in wunits.Where(x => !x.isDie))
            {
                //set wpos
                wunit.SetUnitPos(wpos);

                //create cunit
                var cunit = ModBattleEvent.SceneBattle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(wunit.data, ut);

                //set cpos
                cunit.move.SetPosition(new UnityEngine.Vector2(
                    CommonTool.Random(ModBattleEvent.SceneBattle.battleMap.roomCenterPosi.x - 16, ModBattleEvent.SceneBattle.battleMap.roomCenterPosi.x + 16),
                    CommonTool.Random(ModBattleEvent.SceneBattle.battleMap.roomCenterPosi.y - 16, ModBattleEvent.SceneBattle.battleMap.roomCenterPosi.y + 16)));

                rs.Add(cunit);
            }
            return rs;
        }

        public bool IsDispute()
        {
            return g.world.battle.data.dungeonBaseItem.id == DUNGEON_BASE_ID;
        }
    }
}
