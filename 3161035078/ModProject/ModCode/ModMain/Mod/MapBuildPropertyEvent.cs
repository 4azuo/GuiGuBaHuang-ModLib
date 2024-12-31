using EBattleTypeData;
using EGameTypeData;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using ModLib.Enum;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DataBuildTown;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.MAP_BUILD_PROPERTY_EVENT)]
    public class MapBuildPropertyEvent : ModEvent
    {
        public const float FIXING_RATE = 6.00f;
        public const int TOWN_MASTER_LUCK_ID = 420041111;
        public const int TOWN_GUARDIAN_LUCK_ID = 420041112;
        public const string TOWN_COUCIL_LUCK_DESC = "townmaster420041110desc";
        public const int MAX_GUARDIANS = 10;

        public Dictionary<string, long> Budget { get; set; } = new Dictionary<string, long>();
        public Dictionary<string, List<string>> TownMasters { get; set; } = new Dictionary<string, List<string>>();

        public override void OnLoadGame()
        {
            base.OnLoadGame();

            foreach (var town in g.world.build.GetBuilds<MapBuildTown>())
            {
                //default budget
                if (!Budget.ContainsKey(town.buildData.id))
                {
                    Budget.Add(town.buildData.id, Math.Pow(2, town.gridData.areaBaseID).Parse<long>() * 300);
                }
                //town master & guardians
                if (!TownMasters.ContainsKey(town.buildData.id))
                {
                    TownMasters.Add(town.buildData.id, new List<string>());

                    var orgPoint = town.GetOrigiPoint();
                    var aroundWUnits = UnitHelper.GetUnitsAround(orgPoint.x, orgPoint.y, 4, false, false).ToArray()
                        .Where(x => x.GetLuck(TOWN_MASTER_LUCK_ID) == null && x.GetLuck(TOWN_GUARDIAN_LUCK_ID) == null);
                    var master = aroundWUnits.GetStrongestWUnit();
                    var masterId = master.GetUnitId();
                    var guardians = aroundWUnits.Where(x => x != master).Take(MAX_GUARDIANS);
                    var guardianIds = guardians.Select(x => x.GetUnitId());
                    TownMasters[town.buildData.id].Add(masterId);
                    TownMasters[town.buildData.id].AddRange(guardianIds);

                    master.AddLuck(TOWN_MASTER_LUCK_ID);
                    foreach (var wunit in guardians)
                    {
                        wunit.AddLuck(TOWN_GUARDIAN_LUCK_ID);
                    }
                }
            }
        }

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.SkyTip.uiName)
            {
                var ui = g.ui.GetUI<UISkyTip>(UIType.SkyTip);
                if (ui.ptextTip.text == TOWN_COUCIL_LUCK_DESC)
                {
                    var a = g.ui.GetUI<UINPCInfo>(UIType.NPCInfo);
                    var b = g.ui.GetUI<UIPlayerInfo>(UIType.PlayerInfo);
                    var wunitId = (a.unit ?? b.unit).GetUnitId();
                    var townId = TownMasters.First(x => x.Value.Contains(wunitId)).Key;
                    var town = g.world.build.GetBuild<MapBuildTown>(townId);
                    ui.ptextTip.text = town.name;
                }
            }
        }

        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);

            //use inn
            if (!wunit.IsPlayer())
            {
                wunit.AddProperty<int>(UnitPropertyEnum.Hp, wunit.GetDynProperty(UnitDynPropertyEnum.HpMax).value / 5);
                wunit.AddProperty<int>(UnitPropertyEnum.Mp, wunit.GetDynProperty(UnitDynPropertyEnum.MpMax).value / 5);
                wunit.AddProperty<int>(UnitPropertyEnum.Sp, wunit.GetDynProperty(UnitDynPropertyEnum.SpMax).value / 5);
            }

            //town tax
            int tax = GetTax(wunit);
            var location = wunit.data.unitData.GetPoint();
            var town = g.world.build.GetBuild<MapBuildTown>(location);
            if (town != null)
            {
                Budget[town.buildData.id] += tax;
            }

            //school tax
            var school = g.world.build.GetBuild<MapBuildSchool>(location);
            if (school != null)
            {
                school.buildData.money += tax;
            }

            //
            wunit.AddUnitMoney(-tax);
        }

        public override void OnYearly()
        {
            base.OnYearly();

            foreach (var town in g.world.build.GetBuilds<MapBuildTown>())
            {
                //budget inc yearly
                Budget[town.buildData.id] += Math.Pow(2, town.gridData.areaBaseID).Parse<long>() * 200;
                
                //budget inc from auction
                var auction = town.GetBuildSub<MapBuildTownAuction>();
                if (auction != null)
                {
                    Budget[town.buildData.id] += Math.Pow(2, town.gridData.areaBaseID).Parse<long>() * 150;
                }

                //master & guardians 's profit
                foreach (var wunitId in TownMasters[town.buildData.id].ToArray())
                {
                    var wunit = g.world.unit.GetUnit(wunitId);
                    if (wunit != null && !wunit.isDie)
                    {
                        var profit = Budget[town.buildData.id] / (wunit.GetLuck(TOWN_MASTER_LUCK_ID) == null ? 20 : 10);
                        Budget[town.buildData.id] -= profit;
                        wunit.AddUnitMoney(profit.FixValue(0, int.MaxValue).Parse<int>());
                    }
                    else
                    {
                        TownMasters[town.buildData.id].Remove(wunitId);
                    }
                }

                //renew master & guardians
                var orgPoint = town.GetOrigiPoint();
                var aroundWUnits = UnitHelper.GetUnitsAround(orgPoint.x, orgPoint.y, 4, false, false).ToArray()
                        .Where(x => x.GetLuck(TOWN_MASTER_LUCK_ID) == null && x.GetLuck(TOWN_GUARDIAN_LUCK_ID) == null);
                var townCouncilWUnits = TownMasters[town.buildData.id].Select(x => g.world.unit.GetUnit(x));
                var outCouncilWUnits = aroundWUnits.Except(townCouncilWUnits);
                var master = townCouncilWUnits.FirstOrDefault(x => x.GetLuck(TOWN_MASTER_LUCK_ID) != null);
                TownMasters[town.buildData.id].Clear();
                if (master == null)
                {
                    master = aroundWUnits.GetStrongestWUnit();
                    var masterId = master.GetUnitId();
                    TownMasters[town.buildData.id].Add(masterId);
                }
                var guardians = townCouncilWUnits.Where(x => x != master).ToList();
                if (guardians.Count < MAX_GUARDIANS)
                {
                    guardians.AddRange(outCouncilWUnits.Take(MAX_GUARDIANS - guardians.Count));
                    var guardianIds = guardians.Select(x => x.GetUnitId());
                    TownMasters[town.buildData.id].AddRange(guardianIds);
                }
            }

            foreach (var school in g.world.build.GetBuilds<MapBuildSchool>())
            {
                //school budget inc yearly
                school.buildData.money += Math.Pow(2, school.gridData.areaBaseID).Parse<long>() * 300;
            }
        }

        public override void OnBattleUnitDie(UnitDie e)
        {
            base.OnBattleUnitDie(e);

            if (e?.hitData?.attackUnit != null && e.hitData.attackUnit.IsWorldUnit())
            {
                var killer = e.hitData.attackUnit.GetWorldUnit();
                var killerId = killer.GetUnitId();

                var location = killer.data.unitData.GetPoint();
                var town = g.world.build.GetBuild<MapBuildTown>(location);
                if (town != null)
                {
                    foreach (var councilWUnit in TownMasters[town.buildData.id].Select(x => g.world.unit.GetUnit(x)))
                    {
                        councilWUnit.data.unitData.relationData.AddHate(killerId, 50f);
                    }
                }

                if (e?.unit != null && e.unit.IsWorldUnit())
                {
                    var dieUnit = e.unit.GetWorldUnit();
                    var dieUnitId = dieUnit.GetUnitId();
                    if (TownMasters.Any(x => x.Value.Contains(dieUnitId)))
                    {
                        var dieUnitCouncil = TownMasters.First(x => x.Value.Contains(dieUnitId));
                        dieUnitCouncil.Value.Remove(dieUnitId);

                        foreach (var councilWUnit in dieUnitCouncil.Value.Select(x => g.world.unit.GetUnit(x)))
                        {
                            councilWUnit.data.unitData.relationData.AddHate(killerId, 400f);
                        }

                        if (dieUnit.GetLuck(TOWN_MASTER_LUCK_ID) != null)
                        {
                            killer.AddLuck(TOWN_MASTER_LUCK_ID);
                            dieUnitCouncil.Value.Add(killerId);
                        }
                    }
                }
            }
        }

        public static int GetTax(WorldUnitBase wunit)
        {
            var location = wunit.data.unitData.GetPoint();
            var areaId = wunit.data.unitData.pointGridData.areaBaseID;
            var smConfigs = EventHelper.GetEvent<SMLocalConfigsEvent>(ModConst.SM_LOCAL_CONFIGS_EVENT);
            var tax = smConfigs.Calculate(Convert.ToInt32(InflationaryEvent.CalculateInflationary((
                        Math.Pow(2, areaId) * FIXING_RATE *
                        (1.00f + UnitTypeLuckEnum.Merchant.CustomEffects[ModConst.UTYPE_LUCK_EFX_SELL_VALUE].Value0.Parse<float>() + MerchantLuckEnum.Merchant.GetCurLevel(wunit) * MerchantLuckEnum.Merchant.IncSellValueEachLvl)
                    ).Parse<int>())), smConfigs.Configs.AddTaxRate).Parse<int>();
            var school = g.world.build.GetBuild<MapBuildSchool>(location);
            if (school != null)
                tax *= school.schoolData.allEffects.Count;
            return tax;
        }

        public static long GetBuildProperty(MapBuildBase build)
        {
            var town = build.TryCast<MapBuildTown>();
            if (town != null)
            {
                var x = EventHelper.GetEvent<MapBuildPropertyEvent>(ModConst.MAP_BUILD_PROPERTY_EVENT);
                if (!x.Budget.ContainsKey(town.buildData.id))
                    x.Budget.Add(town.buildData.id, 0);
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
                if (!x.Budget.ContainsKey(town.buildData.id))
                    x.Budget.Add(town.buildData.id, 0);
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
