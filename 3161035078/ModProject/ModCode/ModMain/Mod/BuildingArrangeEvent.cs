﻿using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using ModLib.Mod;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.BUILDING_ARRANGE_EVENT)]
    public class BuildingArrangeEvent : ModEvent
    {
        public static BuildingArrangeEvent Instance { get; set; }

        public List<string> ArrDic { get; set; } = new List<string>();

        private string GetArrDicKey(MapBuildBase build, BuildingCostEnum e)
        {
            return $"{build.buildData.id}_{e.Name}";
        }

        public override void OnLoadGame()
        {
            base.OnLoadGame();

            foreach (var build in g.world.build.GetBuilds())
            {
                foreach (var e in BuildingCostEnum.GetAllEnums<BuildingCostEnum>())
                {
                    RemoveBuildSub(build, e);
                    AddBuildSub(build, e, 100.00f);
                }
            }
        }

        public override void OnMonthly()
        {
            base.OnMonthly();

            foreach (var build in g.world.build.GetBuilds())
            {
                if (build.IsTown() && MapBuildPropertyEvent.GetTownGuardians(build.TryCast<MapBuildTown>()).Any(x => x.GetUnitId() == g.world.playerUnit.GetUnitId()))
                    continue; //if player is town-master, then skip auto build
                foreach (var e in BuildingCostEnum.GetAllEnums<BuildingCostEnum>())
                {
                    AddBuildSub(build, e);
                }
            }
        }

        private void RemoveBuildSub(MapBuildBase build, BuildingCostEnum e)
        {
            if (e.IsMatchBuildConds(build))
            {
                var k = GetArrDicKey(build, e);
                if (!ArrDic.Contains(k))
                {
                    var buildSub = build.GetBuildSub(e.BuildType);
                    if (buildSub != null)
                        build.DelBuildSub(buildSub);
                }
            }
        }

        private void AddBuildSub(MapBuildBase build, BuildingCostEnum e, float rate = -1f)
        {
            if (SMLocalConfigsEvent.Instance.Configs.OnlyPortalAtCityAndSect && e == BuildingCostEnum.TownPortalBuildCost && build.IsSmallTown())
                return;
            if (e.IsMatchBuildConds(build))
            {
                if (IsIgnored(build, e))
                    return;

                var r = CommonTool.Random(0.00f, 100.00f);
                if (MapBuildPropertyEvent.GetBuildProperty(build) > GetBuildingCost(build, e) &&
                    ValueHelper.IsBetween(r, 0.00f, rate == -1f ? e.BuildRate : rate))
                {
                    Build(build, e);
                }
            }
        }

        public static bool IsIgnored(MapBuildBase build, BuildingCostEnum e)
        {
            return e.BuildRate == -1f || 
                e.BuildCosts == null || 
                Instance.ArrDic.Contains(Instance.GetArrDicKey(build, e)) ||
                build.GetBuildSub(e.BuildType) !=  null;
        }

        public static void Build(MapBuildBase build, BuildingCostEnum e)
        {
            Instance.ArrDic.Add(Instance.GetArrDicKey(build, e));
            e.Build(build);
            MapBuildPropertyEvent.AddBuildProperty(build, GetBuildingCost(build, e));
        }

        public static long GetBuildingCost(MapBuildBase build, BuildingCostEnum e)
        {
            var cost = InflationaryEvent.CalculateInflationary(e.BuildCosts[build.gridData.areaBaseID - 1]);
            return SMLocalConfigsEvent.Instance.Calculate(cost, SMLocalConfigsEvent.Instance.Configs.AddBuildingCostRate).Parse<long>();
        }
    }
}
