using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.MAP_BUILD_PROPERTY_EVENT)]
    public class MapBuildPropertyEvent : ModEvent
    {
        public const float FIXING_RATE = 6.00f;

        public IDictionary<string, long> Budget { get; set; } = new Dictionary<string, long>();

        public override void OnLoadGame()
        {
            base.OnLoadGame();

            foreach (var town in g.world.build.GetBuilds<MapBuildTown>())
            {
                if (!Budget.ContainsKey(town.buildData.id))
                {
                    Budget.Add(town.buildData.id, Math.Pow(2, town.gridData.areaBaseID).Parse<long>() * 300);
                }
            }
        }

        public override void OnMonthly()
        {
            base.OnMonthly();

            foreach (var wunit in g.world.unit.GetUnits())
            {
                var income = Convert.ToInt32(InflationaryEvent.CalculateInflationary((
                        Math.Pow(2, wunit.GetGradeLvl()) * FIXING_RATE *
                        (1.00f + UnitTypeLuckEnum.Merchant.CustomEffects[ModConst.UTYPE_LUCK_EFX_SELL_VALUE].Value0.Parse<float>() + MerchantLuckEnum.Merchant.GetCurLevel(wunit) * MerchantLuckEnum.Merchant.IncSellValueEachLvl)
                    ).Parse<long>(), GameHelper.GetGameYear()));
                var town = g.world.build.GetBuild<MapBuildTown>(wunit.data.unitData.GetPoint());
                if (town != null)
                {
                    Budget[town.buildData.id] += income;
                }
                var school = g.world.build.GetBuild<MapBuildSchool>(wunit.data.unitData.GetPoint());
                if (school != null)
                {
                    school.buildData.money += income;
                }
                wunit.AddUnitMoney(-income);
            }
        }

        public override void OnYearly()
        {
            base.OnYearly();

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
