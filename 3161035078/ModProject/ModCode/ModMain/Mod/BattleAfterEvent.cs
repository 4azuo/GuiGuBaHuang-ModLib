using EBattleTypeData;
using Harmony;
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

        public static List<WorldUnitBase> Stalkers { get; } = new List<WorldUnitBase>();

        public const float STALKE_RATE = 30f;

        public override void OnMonthly()
        {
            base.OnMonthly();

            var playerPos = g.world.playerUnit.GetUnitPos();
            foreach (var wunit in Stalkers)
            {
                foreach (var member in HirePeopleEvent.GetTeamDetailData(wunit).Item2)
                {
                    wunit.SetUnitPos(new UnityEngine.Vector2Int(CommonTool.Random(playerPos.x - 4, playerPos.x + 4), CommonTool.Random(playerPos.y - 4, playerPos.y + 4)));
                }
            }
        }

        public override void OnBattleEndOnce(BattleEnd e)
        {
            base.OnBattleEndOnce(e);

            var itemMaxLevel = ModBattleEvent.SceneBattle.battleData.allDropRewardItem.ToArray().Max(x => x.propsInfoBase.level);
            if (itemMaxLevel > 2)
            {
                var itemMaxGrade = ModBattleEvent.SceneBattle.battleData.allDropRewardItem.ToArray().Max(x => x.propsInfoBase.grade);
                var robRate = itemMaxGrade * 10 + Math.Pow(1.7, itemMaxLevel);

                var player = g.world.playerUnit;
                var playerGrade = player.GetGradeLvl();
                var aroundUnits = player.GetUnitsAround(playerGrade, true, false).ToArray().Where(x => IsNoticed(x) && robRate > x.GetGradeLvl() * 10).ToList();
                foreach (var wunit in Stalkers.ToArray())
                {
                    if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, STALKE_RATE))
                    {
                        Stalkers.AddRange(HirePeopleEvent.GetTeamDetailData(wunit).Item2); //stalke
                    }
                    else
                    {
                        wunit.data.unitData.relationData.AddHate(player.GetUnitId(), 100); //hate player
                    }
                }
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
    }
}
