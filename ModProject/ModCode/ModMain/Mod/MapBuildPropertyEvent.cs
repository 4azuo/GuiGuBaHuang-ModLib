using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using UnityEngine;
using static DataBuildTown;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.MAP_BUILD_PROPERTY_EVENT)]
    public class MapBuildPropertyEvent : ModEvent
    {
        public IDictionary<string, long> Budget { get; set; } = new Dictionary<string, long>();

        public override void OnLoadGame()
        {
            var market = EventHelper.GetEvent<RealMarketEvent>(ModConst.REAL_MARKET_EVENT);

            foreach (var town in g.world.build.GetBuilds<MapBuildTown>())
            {
                if (!Budget.ContainsKey(town.buildData.id))
                {
                    #region old
                    if (market.MarketST.ContainsKey(town.buildData.id))
                    {
                        Budget.Add(town.buildData.id, market.MarketST[town.buildData.id]);
                    }
                    #endregion
                    else
                    {
                        Budget.Add(town.buildData.id, Math.Pow(3, town.gridData.areaBaseID).Parse<long>() * 200);
                    }
                }
            }
        }

        public override void OnMonthly()
        {
            foreach (var wunit in g.world.unit.GetUnits())
            {
                var town = g.world.build.GetBuild<MapBuildTown>(wunit.data.unitData.GetPoint());
                if (town != null)
                {
                    Budget[town.buildData.id] += (Math.Pow(2, g.conf.roleGrade.GetItem(wunit.GetProperty<int>(UnitPropertyEnum.GradeID)).grade) * 6).Parse<long>();
                }
                var school = g.world.build.GetBuild<MapBuildSchool>(wunit.data.unitData.GetPoint());
                if (school != null)
                {
                    school.buildData.money += (Math.Pow(2, g.conf.roleGrade.GetItem(wunit.GetProperty<int>(UnitPropertyEnum.GradeID)).grade) * 6).Parse<long>();
                }
            }
        }

        public override void OnYearly()
        {
            foreach (var town in g.world.build.GetBuilds<MapBuildTown>())
            {
                Budget[town.buildData.id] += Math.Pow(2, town.gridData.areaBaseID).Parse<long>() * 200;
                
                var auction = town.GetBuildSub<MapBuildTownAuction>();
                if (auction != null)
                {
                    Budget[town.buildData.id] += Math.Pow(2, town.gridData.areaBaseID).Parse<long>() * 150;
                }
            }
            foreach (var school in g.world.build.GetBuilds<MapBuildSchool>())
            {
                school.buildData.money += Math.Pow(2, school.gridData.areaBaseID).Parse<long>() * 300;
            }
        }

        public static long GetBuildProperty(MapBuildBase build)
        {
            var town = build.TryCast<MapBuildTown>();
            if (town != null)
            {
                var x = EventHelper.GetEvent<MapBuildPropertyEvent>(ModConst.MAP_BUILD_PROPERTY_EVENT);
                return x.Budget[town.buildData.id];
            }

            var school = build.TryCast<MapBuildSchool>();
            if (school != null)
            {
                return school.buildData.money;
            }

            return 0;
        }

        public static void AddBuildProperty(MapBuildBase build, long add)
        {
            var town = build.TryCast<MapBuildTown>();
            if (town != null)
            {
                var x = EventHelper.GetEvent<MapBuildPropertyEvent>(ModConst.MAP_BUILD_PROPERTY_EVENT);
                x.Budget[town.buildData.id] += add;
            }

            var school = build.TryCast<MapBuildSchool>();
            if (school != null)
            {
                school.buildData.money += add;
            }
        }
    }
}
