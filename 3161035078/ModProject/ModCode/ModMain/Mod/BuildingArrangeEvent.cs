using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using ModLib.Mod;
using System.Collections.Generic;
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
            return $"{build?.buildData?.id}_{e?.Name}";
        }

        public override void OnLoadNewGame()
        {
            base.OnLoadNewGame();

            foreach (var build in g.world.build.GetBuilds())
            {
                foreach (var e in BuildingCostEnum.GetAllEnums<BuildingCostEnum>())
                {
                    if (IsBuilt(build, e))
                        ArrDic.Add(GetArrDicKey(build, e));
                }
            }

            if (SMLocalConfigsEvent.Instance.Configs.AllowTownBuildupOverTime)
            {
                foreach (var build in g.world.build.GetBuilds())
                {
                    if (SMLocalConfigsEvent.Instance.Configs.AllowTownBuildupOverTime_IncludeFirstTown || build.gridData.areaBaseID > 1)
                    {
                        foreach (var e in BuildingCostEnum.GetAllEnums<BuildingCostEnum>())
                        {
                            Destroy(build, e);
                        }
                    }
                }
            }
            else
            if (SMLocalConfigsEvent.Instance.Configs.OnlyPortalAtCityAndSect)
            {
                foreach (var build in g.world.build.GetBuilds())
                {
                    if (build.IsSmallTown())
                    {
                        Destroy(build, BuildingCostEnum.TownPortalBuildCost);
                    }
                }
            }
        }

        public override void OnMonthly()
        {
            base.OnMonthly();

            foreach (var build in g.world.build.GetBuilds())
            {
                if (build.IsTown() && MapBuildPropertyEvent.IsTownMaster(build.TryCast<MapBuildTown>(), g.world.playerUnit))
                    continue; //if player is town-master, then skip auto build
                foreach (var e in BuildingCostEnum.GetAllEnums<BuildingCostEnum>())
                {
                    AddBuildSub(build, e);
                }
            }
        }

        public void RemoveBuildSub(MapBuildBase build, BuildingCostEnum e)
        {
            if (build == null || e == null)
                return;
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

        public void AddBuildSub(MapBuildBase build, BuildingCostEnum e)
        {
            if (build == null || e == null)
                return;
            if (SMLocalConfigsEvent.Instance.Configs.OnlyPortalAtCityAndSect && e == BuildingCostEnum.TownPortalBuildCost && build.IsSmallTown())
                return; //OnlyPortalAtCityAndSect then remove small town portal
            if (IsBuildable(build, e))
            {
                if (MapBuildPropertyEvent.GetBuildProperty(build) > GetBuildingCost(build, e) &&
                    ValueHelper.IsBetween(CommonTool.Random(0.00f, 100.00f), 0.00f, e.BuildRate))
                {
                    Build(build, e);
                }
            }
        }

        public static bool IsIgnored(BuildingCostEnum e)
        {
            if (e == null)
                return true;
            return e.BuildRate == -1f || e.BuildCosts == null;
        }

        public static bool IsBuilt(MapBuildBase build, BuildingCostEnum e)
        {
            if (build == null || e == null)
                return false;
            return Instance.ArrDic.Contains(Instance.GetArrDicKey(build, e));
        }

        public static bool IsBuildable(MapBuildBase build, BuildingCostEnum e)
        {
            if (build == null || e == null)
                return false;
            return e.IsMatchBuildConds(build) && !IsBuilt(build, e) && !IsIgnored(e);
        }

        public static void Build(MapBuildBase build, BuildingCostEnum e)
        {
            if (build == null || e == null)
                return;
            if (e.Build(build))
            {
                Instance.ArrDic.Add(Instance.GetArrDicKey(build, e));
                MapBuildPropertyEvent.AddBuildProperty(build, -GetBuildingCost(build, e));
            }
        }

        public static void Destroy(MapBuildBase build, BuildingCostEnum e)
        {
            if (build == null || e == null || build.GetBuildSub(e.BuildType) == null) //just destroy when the build already has subbuild
                return;
            Instance.ArrDic.Remove(Instance.GetArrDicKey(build, e));
            Instance.RemoveBuildSub(build, e);
        }

        public static long GetBuildingCost(MapBuildBase build, BuildingCostEnum e)
        {
            if (build == null || e == null)
                return 0;
            var cost = InflationaryEvent.CalculateInflationary(e.BuildCosts[build.gridData.areaBaseID - 1]);
            return SMLocalConfigsEvent.Instance.Calculate(cost, SMLocalConfigsEvent.Instance.Configs.AddBuildingCostRate).Parse<long>();
        }
    }
}
