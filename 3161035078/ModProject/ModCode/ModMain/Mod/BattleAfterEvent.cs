using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using System;
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
                foreach (var wunit in player.GetUnitsAround(playerGrade, true, false).ToArray().Where(x => IsNoticed(x) && robRate > x.GetGradeLvl() * 10))
                {

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
