using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using ModLib.Attributes;
using ModLib.Mod;
using System.Collections.Generic;
using UnityEngine;
using ModLib.Helper;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Towns/sects (re)build sub-buildings over time with their budget; options to start towns empty or keep portals only in cities/sects.
    /// </summary>
    [Cache(ModConst.BUILDING_ARRANGE_EVENT)]
    public class BuildingArrangeEvent : ModEvent
    {
        public static BuildingArrangeEvent Instance { get; set; }

        public List<string> DefaultBuilding { get; set; } = new List<string>();

        /// <summary>
        /// Key "&lt;buildId&gt;_&lt;subBuilding&gt;".
        /// </summary>
        private string GetDefaultBuildingKey(MapBuildBase build, BuildingCostEnum e)
        {
            return $"{build?.buildData?.id}_{e?.Name}";
        }

        /// <summary>
        /// Remembers the original sub-buildings and removes them when "build up over time" (or small-town portals) is enabled.
        /// </summary>
        public override void OnLoadNewGame()
        {
            base.OnLoadNewGame();

            //add default
            foreach (var build in GetModChildParameterStore().Buildings)
            {
                foreach (var e in BuildingCostEnum.GetAllEnums<BuildingCostEnum>())
                {
                    if (build.GetBuildSub(e.BuildType) != null)
                        DefaultBuilding.Add(GetDefaultBuildingKey(build, e));
                }
            }

            if (SMLocalConfigsEvent.Instance.Configs.AllowTownBuildupOverTime)
            {
                foreach (var build in GetModChildParameterStore().Buildings)
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
                foreach (var build in GetModChildParameterStore().Buildings)
                {
                    if (build.IsSmallTown())
                    {
                        Destroy(build, BuildingCostEnum.TownPortalBuildCost);
                    }
                }
            }
        }

        /// <summary>
        /// Tries to build every missing original sub-building (skipped for towns the player governs).
        /// </summary>
        public override void OnMonthly()
        {
            base.OnMonthly();

            foreach (var build in GetModChildParameterStore().Buildings)
            {
                if (build.IsTown() && MapBuildPropertyEvent.IsTownMaster(build.TryCast<MapBuildTown>(), g.world.playerUnit))
                    continue; //if player is town-master, then skip auto build
                foreach (var e in BuildingCostEnum.GetAllEnums<BuildingCostEnum>())
                {
                    AddBuildSub(build, e);
                }
            }
        }

        /// <summary>
        /// Destroys a sub-building when allowed.
        /// </summary>
        public void RemoveBuildSub(MapBuildBase build, BuildingCostEnum e)
        {
            if (build == null || e == null)
                return;
            if (IsDestroyable(build, e))
            {
                var buildSub = build.GetBuildSub(e.BuildType);
                if (buildSub != null)
                    build.DelBuildSub(buildSub);
            }
        }

        /// <summary>
        /// Builds a sub-building when affordable (random build rate).
        /// </summary>
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

        /// <summary>
        /// True when the sub-building is not managed.
        /// </summary>
        public static bool IsIgnored(BuildingCostEnum e)
        {
            if (e == null)
                return true;
            return e.BuildRate == -1f || e.BuildCosts == null;
        }

        /// <summary>
        /// True when it exists.
        /// </summary>
        public static bool IsBuilt(MapBuildBase build, BuildingCostEnum e)
        {
            if (build == null || e == null)
                return false;
            return build.GetBuildSub(e.BuildType) != null;
        }

        /// <summary>
        /// True when it can be built here.
        /// </summary>
        public static bool IsBuildable(MapBuildBase build, BuildingCostEnum e)
        {
            if (build == null || e == null)
                return false;
            return e.IsMatchBuildConds(build) && !IsIgnored(e) && !IsBuilt(build, e) && Instance.DefaultBuilding.Contains(Instance.GetDefaultBuildingKey(build, e));
        }

        /// <summary>
        /// True when it can be destroyed here.
        /// </summary>
        public static bool IsDestroyable(MapBuildBase build, BuildingCostEnum e)
        {
            if (build == null || e == null)
                return false;
            return e.IsMatchBuildConds(build) && !IsIgnored(e) && IsBuilt(build, e) && Instance.DefaultBuilding.Contains(Instance.GetDefaultBuildingKey(build, e));
        }

        /// <summary>
        /// Builds and pays the cost from the budget.
        /// </summary>
        public static void Build(MapBuildBase build, BuildingCostEnum e)
        {
            if (build == null || e == null)
                return;
            if (e.Build(build))
            {
                MapBuildPropertyEvent.AddBuildProperty(build, -GetBuildingCost(build, e));
            }
        }

        /// <summary>
        /// Destroys if present.
        /// </summary>
        public static void Destroy(MapBuildBase build, BuildingCostEnum e)
        {
            if (build == null || e == null || build.GetBuildSub(e.BuildType) == null) //just destroy when the build already has subbuild
                return;
            Instance.RemoveBuildSub(build, e);
        }

        /// <summary>
        /// Cost by area (inflation and S&amp;M rate applied).
        /// </summary>
        public static long GetBuildingCost(MapBuildBase build, BuildingCostEnum e)
        {
            if (build == null || e == null)
                return 0;
            var cost = InflationaryEvent.CalculateInflationary(e.BuildCosts[build.gridData.areaBaseID - 1]);
            return SMLocalConfigsEvent.Instance.Calculate(cost, SMLocalConfigsEvent.Instance.Configs.AddBuildingCostRate).Parse<long>();
        }
    }
}
