using EBattleTypeData;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using ModLib.Enum;
using ModLib.Mod;
using System.Collections.Generic;
using System.Linq;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.BATTLE_AFTER_EVENT)]
    public class BattleAfterEvent : ModEvent
    {
        public const float STALKE_RATE = 30f;

        public static BattleAfterEvent Instance { get; set; }

        public static readonly int[] enemyInTraits = new int[]
        {
            UnitTraitEnum.Wicked.Parse<int>(),
            UnitTraitEnum.Selfish.Parse<int>(),
            UnitTraitEnum.Evil.Parse<int>(),
            UnitTraitEnum.Glory_Hound.Parse<int>(),
            UnitTraitEnum.Power_hungry.Parse<int>()
        };

        public Dictionary<string, StalkReasonEnum> Stalkers { get; set; } = new Dictionary<string, StalkReasonEnum>();

        public override void OnMonthly()
        {
            base.OnMonthly();

            var playerPos = g.world.playerUnit.GetUnitPos();
            foreach (var stalker in Stalkers)
            {
                var wunit = g.world.unit.GetUnit(stalker.Key);
                if (wunit != null)
                {
                    foreach (var member in HirePeopleEvent.GetTeamDetailData(wunit).Item2)
                    {
                        member.SetUnitPos(new UnityEngine.Vector2Int(CommonTool.Random(playerPos.x - 4, playerPos.x + 4), CommonTool.Random(playerPos.y - 4, playerPos.y + 4)));
                    }
                }
            }
        }

        public override void OnBattleEndOnce(BattleEnd e)
        {
            base.OnBattleEndOnce(e);

            if (!SMLocalConfigsEvent.Instance.Configs.NoStalker)
            {
                var dropItems = ModBattleEvent.SceneBattle.battleData.allDropRewardItem.ToArray();
                var bestItem = dropItems.OrderByDescending(x => x.GetItemValue()).FirstOrDefault();
                if (bestItem != null && bestItem.propsInfoBase?.level > 2)
                {
                    var player = g.world.playerUnit;
                    foreach (var wunit in player.GetUnitsAround(player.GetGradeLvl(), true, false).ToArray()
                        .Where(x => IsStalker(x) && x.CheckItemCouldBeRobbed(bestItem)))
                    {
                        if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, STALKE_RATE))
                        {
                            foreach (var item in HirePeopleEvent.GetTeamDetailData(wunit).Item2)
                            {
                                Stalkers.Add(item.GetUnitId(), StalkReasonEnum.KillBoss);
                            }
                        }
                        else
                        {
                            wunit.data.unitData.relationData.AddHate(player.GetUnitId(), 100); //hate player
                        }
                    }
                }
            }
        }

        public static bool IsStalker(WorldUnitBase wunit)
        {
            return 
                //check stalker hp/mp/sp
                wunit.GetDynProperty(UnitDynPropertyEnum.Hp).value > (wunit.GetDynProperty(UnitDynPropertyEnum.HpMax).value * 0.7f) &&
                wunit.GetDynProperty(UnitDynPropertyEnum.Mp).value > (wunit.GetDynProperty(UnitDynPropertyEnum.MpMax).value * 0.5f) &&
                wunit.GetDynProperty(UnitDynPropertyEnum.Sp).value > (wunit.GetDynProperty(UnitDynPropertyEnum.SpMax).value * 0.3f) &&
                //check stalker power  >= player power
                UnitPowerCalHelper.CalWUnitBattlePower(g.world.playerUnit) <= UnitPowerCalHelper.CalWUnitBattlePower(wunit) &&
                //check stalker traits and intim
                (
                    enemyInTraits.Contains(wunit.data.unitData.propertyData.inTrait) ||
                    wunit.data.unitData.relationData.GetIntim(g.world.playerUnit) <= -100
                );
        }

        public static void AddStalker(StalkReasonEnum reason, params WorldUnitBase[] wunits)
        {
            foreach (var wunit in wunits)
            {
                foreach (var unit in HirePeopleEvent.GetTeamDetailData(wunit).Item2)
                {
                    var unitId = unit.GetUnitId();
                    if (!Instance.Stalkers.ContainsKey(unitId))
                    {
                        Instance.Stalkers.Add(unitId, reason);
                        DebugHelper.WriteLine($"Get Stalked by {unit.GetName()} ({unitId})");
                    }
                }
            }
        }
    }
}
