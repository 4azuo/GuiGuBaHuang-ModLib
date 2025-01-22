using EBattleTypeData;
using EGameTypeData;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.MAP_BUILD_PROPERTY_EVENT)]
    public class MapBuildPropertyEvent : ModEvent
    {
        public static MapBuildPropertyEvent Instance { get; set; }

        public const float FIXING_RATE = 5.00f;
        public const int TOWN_MASTER_LUCK_ID = 420041111;
        public const int TOWN_GUARDIAN_LUCK_ID = 420041112;
        public const string TOWN_COUCIL_LUCK_DESC = "townmaster420041110desc";
        public const int BECOME_TOWN_MASTER_DRAMA = 420041113;

        public Dictionary<string, float> TaxRate { get; set; } = new Dictionary<string, float>();
        public Dictionary<string, long> Budget { get; set; } = new Dictionary<string, long>();
        public Dictionary<string, List<string>> TownMasters { get; set; } = new Dictionary<string, List<string>>();
        public List<string> PayTaxTown { get; } = new List<string>();

        public static UINPCInfo UINPCInfo;
        public static bool isShowHirePeopleUI = false;
        public static bool isShowManageTeamUI1 = false;
        public static bool isShowManageTeamUI2 = false;

        public override void OnLoadGame()
        {
            base.OnLoadGame();

            foreach (var town in g.world.build.GetBuilds<MapBuildTown>())
            {
                //default tax rate
                if (!TaxRate.ContainsKey(town.buildData.id))
                {
                    TaxRate.Add(town.buildData.id, 1f);
                }
                //default budget
                if (!Budget.ContainsKey(town.buildData.id))
                {
                    Budget.Add(town.buildData.id, Math.Pow(2, town.gridData.areaBaseID).Parse<long>() * 300);
                }
                //town master & guardians
                if (!TownMasters.ContainsKey(town.buildData.id))
                {
                    TownMasters.Add(town.buildData.id, new List<string>());

                    var aroundWUnits = UnitHelper.GetUnitsAround(town.GetOrigiPoint(), 4, false, true).ToArray().Where(x => IsMatchCondWUnit(x)).ToList();
                    if (aroundWUnits.Count > 0)
                    {
                        var master = aroundWUnits.GetFamousWUnit();
                        Hire(town, master, TOWN_MASTER_LUCK_ID);
                    }
                }
            }
        }

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);

            if (e.uiType.uiName == UIType.Town.uiName)
            {
                var town = GetGuardTown(g.world.playerUnit);
                if (town != null && !PayTaxTown.Contains(town.buildData.id) && IsTownGuardian(town, g.world.playerUnit))
                {
                    var tax = GetTax(g.world.playerUnit, g.world.playerUnit.GetUnitPosAreaId());
                    if (g.world.playerUnit.GetUnitMoney() > tax)
                    {
                        Budget[town.buildData.id] += tax;
                        g.world.playerUnit.AddUnitMoney(-tax);
                        PayTaxTown.Add(town.buildData.id);
                    }
                    else
                    {
                        g.ui.CloseUI(e.ui);
                        g.ui.MsgBox("Town", $"You dont have enough {tax:#,##0} Spirit Stones to pay!");
                    }
                }
            }
            else
            if (e.uiType.uiName == UIType.SkyTip.uiName)
            {
                var ui = g.ui.GetUI<UISkyTip>(UIType.SkyTip);
                if (ui.ptextTip.text == TOWN_COUCIL_LUCK_DESC)
                {
                    var a = g.ui.GetUI<UINPCInfo>(UIType.NPCInfo);
                    var b = g.ui.GetUI<UIPlayerInfo>(UIType.PlayerInfo);
                    ui.ptextTip.text = GetGuardTown(a?.unit ?? b?.unit)?.name;
                }
            }
            else
            if (e.uiType.uiName == UIType.NPCSearch.uiName)
            {
                if (isShowManageTeamUI1)
                {
                    var town = GetGuardTown(g.world.playerUnit);

                    var i = -6;
                    var uiCover = new UICover<UINPCSearch>(e.ui);
                    {
                        uiCover.AddText(uiCover.MidCol - 5, uiCover.MidRow + i++, $"Town Budget: {GetBuildProperty(town):#,##0}").Format().Align();
                        uiCover.AddText(uiCover.MidCol - 5, uiCover.MidRow + i++, $"Payment: {GetTotalMonthlyPayment(town):#,##0}/year").Format().Align();
                        uiCover.AddCompositeSlider(uiCover.MidCol - 5, uiCover.MidRow + i++, $"Base Tax:", 0.50f, 10.00f, 1.00f, "{0}/month").SetWork(new UIItemWork
                        {
                            Formatter = (ibase) => new object[] { GetBaseTax(g.world.playerUnit.GetUnitPosAreaId()) }
                        });

                        var c = 0;
                        foreach (var em in BuildingCostEnum.GetAllEnums<BuildingCostEnum>())
                        {
                            if (BuildingArrangeEvent.IsBuildable(town, em))
                            {
                                var cost = BuildingArrangeEvent.GetBuildingCost(town, em);
                                uiCover.AddText(uiCover.MidCol - 5 + (c % 2 * 10), uiCover.MidRow + i, $"{em.BuildingName}　／　Cost: {cost:#,##0}").Format().Align();
                                uiCover.AddButton(uiCover.MidCol - 8 + (c % 2 * 10), uiCover.MidRow + i, () =>
                                {
                                    if (GetBuildProperty(town) > cost)
                                    {
                                        BuildingArrangeEvent.Build(town, em);
                                    }
                                    else
                                    {
                                        g.ui.MsgBox("Info", $"You cant build this building with current budget!{Environment.NewLine}{GetBuildProperty(town):#,##0}");
                                    }
                                }, "Build").Format().Align(TextAnchor.MiddleCenter);
                                i += 2;
                                c++;
                            }
                        }

                        if (IsTownMaster(town, g.world.playerUnit) && !town.buildTownData.isMainTown)
                            uiCover.AddButton(uiCover.LastCol - 8, uiCover.LastRow - 20, () => Upgrade2City(town), "Upgrade to City").Format(Color.black, 17).Align(TextAnchor.MiddleCenter).Size(300, 60);
                        uiCover.AddButton(uiCover.LastCol - 8, uiCover.LastRow - 16, Deposit, "Deposit").Format(Color.black, 17).Align(TextAnchor.MiddleCenter).Size(300, 30);
                        uiCover.AddButton(uiCover.LastCol - 8, uiCover.LastRow - 14, Withdraw, "Withdraw").Format(Color.black, 17).Align(TextAnchor.MiddleCenter).Size(300, 30);
                        uiCover.AddButton(uiCover.LastCol - 8, uiCover.LastRow - 12, OpenUIHirePeople, "Hire People").Format(Color.black, 17).Align(TextAnchor.MiddleCenter).Size(300, 60);
                        uiCover.AddButton(uiCover.LastCol - 8, uiCover.LastRow - 8, () => Leave(g.world.playerUnit), "Dismiss").Format(Color.black, 17).Align(TextAnchor.MiddleCenter).Size(300, 60);
                    }
                    uiCover.UpdateUI();
                }
                else
                if (isShowManageTeamUI2)
                {
                    var uiCover = new UICover<UINPCSearch>(e.ui);
                    {
                        if (IsTownMaster(g.world.playerUnit))
                            uiCover.AddButton(uiCover.LastCol - 8, uiCover.LastRow - 8, () => BattleEvent.TownWar(), "Declare War").Format(Color.black, 17).Align(TextAnchor.MiddleCenter).Size(300, 60);
                    }
                    uiCover.UpdateUI();
                }
            }
            else
            if (e.uiType.uiName == UIType.NPCInfo.uiName)
            {
                var ui = new UICover<UINPCInfo>(e.ui);
                if (ui.UI.unit.GetUnitId() == g.world.playerUnit.GetUnitId())
                {
                    ui.Dispose();
                }
                else
                {
                    var town = GetGuardTown(ui.UI.unit);
                    if (IsTownGuardian(ui.UI.unit))
                    {
                        if (IsTownGuardian(town, ui.UI.unit) && ui.UI.unit.data.unitData.relationData.GetIntim(g.world.playerUnit) < 200)
                            ui.AddText(0, 0, $"{GetRequiredSpiritStones(town, ui.UI.unit):#,##0} Spirit Stones/year").Align().Format(Color.white).Pos(ui.UI.uiProperty.textInTrait1.transform, 0f, 0.5f).SetParentTransform(ui.UI.uiProperty.textInTrait1.transform);
                        ui.AddText(0, 0, $"Town: {GetGuardTownInfoStr(ui.UI.unit)}").Align().Format(Color.white).Pos(ui.UI.uiProperty.textInTrait1.transform, 0f, 0.25f).SetParentTransform(ui.UI.uiProperty.textInTrait1.transform);
                    }
                    else
                    if (isShowHirePeopleUI)
                    {
                        ui.AddText(0, 0, $"{GetRequiredSpiritStones(town, ui.UI.unit):#,##0} Spirit Stones/year").Align().Format(Color.white).Pos(ui.UI.uiProperty.textInTrait1.transform, 0f, 0.5f).SetParentTransform(ui.UI.uiProperty.textInTrait1.transform);
                        ui.AddText(0, 0, $"{GetRequiredReputations(town, ui.UI.unit):#,##0} Reputations").Align().Format(Color.white).Pos(ui.UI.uiProperty.textInTrait1.transform, 0f, 0.25f).SetParentTransform(ui.UI.uiProperty.textInTrait1.transform);
                        ui.AddButton(0, 0, () =>
                        {
                            PreHire(ui.UI.unit);
                        }, "Hire").Format(Color.black).Size(100, 40).Pos(ui.UI.uiProperty.textInTrait1.transform, -1.1f, 0.4f).SetParentTransform(ui.UI.uiProperty.textInTrait1.transform);
                    }
                    else
                    if (isShowManageTeamUI1)
                    {
                        ui.AddText(0, 0, $"{GetRequiredSpiritStones(town, ui.UI.unit):#,##0} Spirit Stones/year").Align().Format(Color.white).Pos(ui.UI.uiProperty.textInTrait1.transform, 0f, 0.5f).SetParentTransform(ui.UI.uiProperty.textInTrait1.transform);
                        ui.AddText(0, 0, $"{GetRequiredReputations(town, ui.UI.unit):#,##0} Reputations").Align().Format(Color.white).Pos(ui.UI.uiProperty.textInTrait1.transform, 0f, 0.25f).SetParentTransform(ui.UI.uiProperty.textInTrait1.transform);
                        ui.AddButton(0, 0, () =>
                        {
                            PreDismiss(ui.UI.unit);
                        }, "Dismiss").Format(Color.black).Size(100, 40).Pos(ui.UI.uiProperty.textInTrait1.transform, -1.1f, 0.4f).SetParentTransform(ui.UI.uiProperty.textInTrait1.transform);
                    }
                    ui.UpdateUI();
                    UINPCInfo = ui.UI;
                }
            }
        }

        public override void OnMonthly()
        {
            base.OnMonthly();
            //clear lst pay tax town
            PayTaxTown.Clear();
            //fix bug every month
            foreach (var t in TownMasters)
            {
                foreach (var wunitId in t.Value.ToArray())
                {
                    var wunit = g.world.unit.GetUnit(wunitId);
                    if (wunit == null || wunit.isDie || !IsTownGuardian(wunit) || IsErrorLuckId(wunit))
                    {
                        RemoveFromTownGuardians(wunitId, wunit);
                    }
                }
            }
            //town data
            foreach (var town in g.world.build.GetBuilds<MapBuildTown>())
            {
                //random tax rate
                if (!IsTownMaster(town, g.world.playerUnit))
                    TaxRate[town.buildData.id] = CommonTool.Random(0.90f, 1.20f);
                //hire more people
                if (!TownMasters.ContainsKey(town.buildData.id))
                {
                    var aroundWUnits = UnitHelper.GetUnitsAround(town.GetOrigiPoint(), 4, false, true).ToArray().Where(x => IsMatchCondWUnit(x)).ToList();
                    if (aroundWUnits.Count > 0)
                    {
                        var guard = aroundWUnits.GetFamousWUnit();
                        Hire(town, guard, TOWN_GUARDIAN_LUCK_ID);
                    }
                }
            }
        }

        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);

            var wunitId = wunit.GetUnitId();

            //fix bug every month
            if (IsTownGuardian(wunit) && TownMasters.All(x => !x.Value.Contains(wunitId)))
            {
                RemoveFromTownGuardians(wunitId, wunit);
            }

            //town tax
            var town = wunit.GetMapBuild<MapBuildTown>();
            if (town != null && !TownMasters[town.buildData.id].Contains(wunit.GetUnitId()))
            {
                //pay town tax
                var tax = GetTax(wunit, wunit.GetUnitPosAreaId());
                if (wunit.GetUnitMoney() > tax)
                {
                    wunit.AddUnitMoney(-tax);
                    Budget[town.buildData.id] += tax;

                    //use inn
                    if (!wunit.IsPlayer())
                    {
                        wunit.AddProperty<int>(UnitPropertyEnum.Hp, wunit.GetDynProperty(UnitDynPropertyEnum.HpMax).value);
                        wunit.AddProperty<int>(UnitPropertyEnum.Mp, wunit.GetDynProperty(UnitDynPropertyEnum.MpMax).value);
                        wunit.AddProperty<int>(UnitPropertyEnum.Sp, wunit.GetDynProperty(UnitDynPropertyEnum.SpMax).value);
                    }
                }
                else
                {
                    //get out
                    wunit.data.unitData.relationData.AddHate(GetTownMaster(town).GetUnitId(), 5);
                }
            }

            //school tax
            var school = wunit.GetMapBuild<MapBuildSchool>();
            if (school != null && !IsSchoolMember(school, wunit))
            {
                //pay school tax
                var tax = GetTax(wunit, wunit.GetUnitPosAreaId());
                wunit.AddUnitMoney(-tax);
                school.buildData.money += tax;
            }
        }

        public override void OnYearly()
        {
            base.OnYearly();

            foreach (var town in g.world.build.GetBuilds<MapBuildTown>())
            {
                if (!TownMasters.ContainsKey(town.buildData.id))
                    TownMasters.Add(town.buildData.id, new List<string>());
                var townMasterData = TownMasters[town.buildData.id];

                //budget inc yearly
                Budget[town.buildData.id] += Math.Pow(2, town.gridData.areaBaseID).Parse<long>() * 200;
                
                //budget inc from auction
                var auction = town.GetBuildSub<MapBuildTownAuction>();
                if (auction != null)
                {
                    Budget[town.buildData.id] += Math.Pow(2, town.gridData.areaBaseID).Parse<long>() * 150;
                }

                //master & guardians 's profit
                foreach (var wunitId in townMasterData.ToArray())
                {
                    var wunit = g.world.unit.GetUnit(wunitId);
                    if (wunit != null && !wunit.isDie)
                    {
                        if (IsTownMaster(wunit))
                            continue;
                        var profit = GetRequiredSpiritStones(town, wunit);
                        if (Budget[town.buildData.id] > profit)
                        {
                            Budget[town.buildData.id] -= profit;
                            wunit.AddUnitMoney(profit);
                        }
                        else
                        {
                            Leave(wunit);
                            wunit.data.unitData.relationData.AddHate(GetTownMaster(town).GetUnitId(), 100);
                        }

                        if (wunit.IsPlayer())
                            continue;
                        wunit.SetUnitPos(town.GetOrigiPoint());
                    }
                    else
                    {
                        RemoveFromTownGuardians(wunitId, wunit);
                    }
                }

                //choose new town master
                var townCouncilWUnits = GetTownGuardians(town);
                var master = GetTownMaster(town);
                while (master == null)
                {
                    master = townCouncilWUnits.GetFamousWUnit();
                    if (master != null && !master.isDie)
                    {
                        var masterId = master.GetUnitId();
                        master.DelLuck(TOWN_GUARDIAN_LUCK_ID);
                        master.AddLuck(TOWN_MASTER_LUCK_ID);
                        townMasterData.Add(masterId);

                        if (master.IsPlayer())
                            DramaTool.OpenDrama(BECOME_TOWN_MASTER_DRAMA);
                    }
                    else
                    {
                        master = null;
                    }
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

            if (g.world.battle.data.isRealBattle &&
                e?.hitData?.attackUnit != null && e.hitData.attackUnit.IsWorldUnit() &&
                e?.unit != null && e.unit.IsWorldUnit())
            {
                var killer = e.hitData.attackUnit.GetWorldUnit();
                var killerId = killer.GetUnitId();

                var town = killer.GetMapBuild<MapBuildTown>();
                if (town != null && TownMasters.ContainsKey(town.buildData.id))
                {
                    if (TownMasters[town.buildData.id].Contains(killerId))
                    {
                        //blamed, power-hara
                    }
                    else
                    {
                        foreach (var councilWUnit in TownMasters[town.buildData.id].Select(x => g.world.unit.GetUnit(x)))
                        {
                            if (councilWUnit != null)
                                councilWUnit.data.unitData.relationData.AddHate(killerId, 50f);
                        }
                    }
                }

                var dieUnit = e.unit.GetWorldUnit();
                var dieUnitId = dieUnit.GetUnitId();
                if (TownMasters.Any(x => x.Value.Contains(dieUnitId)))
                {
                    var dieUnitCouncil = TownMasters.First(x => x.Value.Contains(dieUnitId));
                    RemoveFromTownGuardians(dieUnitId, dieUnit);
                    foreach (var councilWUnit in dieUnitCouncil.Value.Select(x => g.world.unit.GetUnit(x)))
                    {
                        if (councilWUnit != null && killerId != councilWUnit.GetUnitId())
                            councilWUnit.data.unitData.relationData.AddHate(killerId, 200f);
                    }
                }
            }
        }

        public static void PreHire(WorldUnitBase wunit)
        {
            var player = g.world.playerUnit;
            var town = player.GetMapBuild<MapBuildTown>();
            var playerId = player.GetUnitId();

            if (town == null)
                return;

            var requiredSpiritStones = GetRequiredSpiritStones(town, wunit);
            if (player.GetUnitMoney() < requiredSpiritStones)
            {
                g.ui.MsgBox("Town", $"Require {requiredSpiritStones:#,##0} Spirit Stones");
                return;
            }

            var requiredReputations = GetRequiredReputations(town, wunit);
            if (player.GetDynProperty(UnitDynPropertyEnum.Reputation).value < requiredReputations)
            {
                g.ui.MsgBox("Town", $"Require {requiredReputations:#,##0} Reputations");
                return;
            }

            g.ui.MsgBox("Town", "Are you sure about adding this person?", MsgBoxButtonEnum.YesNo, () =>
            {
                Hire(town, wunit, TOWN_GUARDIAN_LUCK_ID);
                Instance.Budget[town.buildData.id] -= requiredSpiritStones;
                g.ui.CloseUI(UINPCInfo);
            });
        }

        public static void Hire(MapBuildTown town, WorldUnitBase member, int luckId)
        {
            Instance.TownMasters[town.buildData.id].Add(member.GetUnitId());
            member.AddLuck(luckId);
        }

        public static void PreDismiss(WorldUnitBase wunit)
        {
            var player = g.world.playerUnit;
            var town = player.GetMapBuild<MapBuildTown>();

            g.ui.MsgBox("Town", "Are you sure about dismissing this person?", MsgBoxButtonEnum.YesNo, () =>
            {
                Dismiss(town, wunit);
                g.ui.CloseUI(UINPCInfo);
            });
        }

        public static void Dismiss(MapBuildTown town, WorldUnitBase member)
        {
            Leave(member);
        }

        public static int GetBaseTax(int areaId)
        {
            //base on area
            return SMLocalConfigsEvent.Instance.Calculate(Convert.ToInt32(InflationaryEvent.CalculateInflationary((
                Math.Pow(2, areaId) * FIXING_RATE
            ).Parse<int>())), SMLocalConfigsEvent.Instance.Configs.AddTaxRate).Parse<int>();
        }

        public static int GetTax(WorldUnitBase wunit, int areaId)
        {
            if (wunit == null || wunit.isDie)
                return 0;
            var tax = GetBaseTax(areaId);
            var school = wunit.GetMapBuild<MapBuildSchool>();
            if (school != null)
            {
                //if school -> *effect-count
                tax *= school.schoolData.allEffects.Count;
            }
            var town = wunit.GetMapBuild<MapBuildTown>();
            if (town != null)
            {
                //if town -> double
                if (town.buildTownData.isMainTown)
                    tax *= 2;
                //add alternative tax rate
                tax = (tax * Instance.TaxRate[town.buildData.id]).Parse<int>();
            }
            //if merchant, add more tax
            if (UnitTypeEvent.GetUnitTypeEnum(wunit) == UnitTypeEnum.Merchant)
            {
                tax *= (1.00f + UnitTypeLuckEnum.Merchant.CustomEffects[ModConst.UTYPE_LUCK_EFX_SELL_VALUE].Value0.Parse<float>()).Parse<int>();
            }
            tax *= (1.00f + MerchantLuckEnum.Merchant.GetCurLevel(wunit) * MerchantLuckEnum.Merchant.IncSellValueEachLvl).Parse<int>();
            return tax;
        }

        public static long GetBuildProperty(MapBuildBase build)
        {
            var town = build.TryCast<MapBuildTown>();
            if (town != null)
            {
                if (!Instance.Budget.ContainsKey(town.buildData.id))
                    Instance.Budget.Add(town.buildData.id, 0);
                return Instance.Budget[town.buildData.id];
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
                if (!Instance.Budget.ContainsKey(town.buildData.id))
                    Instance.Budget.Add(town.buildData.id, 0);
                Instance.Budget[town.buildData.id] += add;
                return;
            }

            var school = build.TryCast<MapBuildSchool>();
            if (school != null)
            {
                school.buildData.money += add;
                return;
            }
        }

        public static bool IsErrorLuckId(WorldUnitBase wunit)
        {
            return wunit.GetLuck(TOWN_GUARDIAN_LUCK_ID) != null && wunit.GetLuck(TOWN_MASTER_LUCK_ID) != null;
        }

        public static bool IsTownGuardian(WorldUnitBase wunit)
        {
            return wunit != null && (wunit.GetLuck(TOWN_GUARDIAN_LUCK_ID) != null || wunit.GetLuck(TOWN_MASTER_LUCK_ID) != null);
        }

        public static bool IsTownMaster(WorldUnitBase wunit)
        {
            return wunit != null && wunit.GetLuck(TOWN_MASTER_LUCK_ID) != null;
        }

        public static bool IsTownGuardian(MapBuildTown town, WorldUnitBase wunit)
        {
            if (town == null || wunit == null)
                return false;
            var wunitId = wunit.GetUnitId();
            return Instance.TownMasters[town.buildData.id].Contains(wunitId);
        }

        public static bool IsTownMaster(MapBuildTown town, WorldUnitBase wunit)
        {
            return IsTownGuardian(town, wunit) && IsTownMaster(wunit);
        }

        public static bool IsSchoolMember(WorldUnitBase wunit)
        {
            return wunit.data.school?.schoolNameID != null;
        }

        public static bool IsSchoolMember(MapBuildSchool school, WorldUnitBase wunit)
        {
            if (school == null)
                return false;
            return school.schoolNameID == wunit.data.school?.schoolNameID;
        }

        public static void Upgrade2City(MapBuildTown town)
        {
            town.buildTownData.isMainTown = true;
        }

        public static void Deposit()
        {
        }

        public static void Withdraw()
        {
        }

        public static void OpenUIHirePeople()
        {
            var ui = g.ui.OpenUI<UINPCSearch>(UIType.NPCSearch);
            ui.InitData(new Vector2Int(0, 0));
            ui.units = GetHirablePeople().ToIl2CppList();
            ui.UpdateUI();
            isShowHirePeopleUI = true;
        }

        public static void OpenUITownManage(MapBuildTown town)
        {
            var ui = g.ui.OpenUI<UINPCSearch>(UIType.NPCSearch);
            ui.InitData(new Vector2Int(0, 0));
            ui.units = GetTownGuardians(town).ToIl2CppList();
            ui.UpdateUI();
            isShowManageTeamUI1 = true;
        }

        public static void OpenUITownGuardians(MapBuildTown town)
        {
            var ui = g.ui.OpenUI<UINPCSearch>(UIType.NPCSearch);
            ui.InitData(new Vector2Int(0, 0));
            ui.units = GetTownGuardians(town).ToIl2CppList();
            ui.UpdateUI();
            isShowManageTeamUI2 = true;
        }

        public static bool IsMatchCondWUnit(WorldUnitBase wunit)
        {
            return !IsTownGuardian(wunit) && !HirePeopleEvent.IsHired(wunit);
        }

        public static List<WorldUnitBase> GetHirablePeople()
        {
            var rs = new List<WorldUnitBase>();
            foreach (var wunit in g.world.playerUnit.GetUnitsAround(4, false, false))
            {
                if (IsMatchCondWUnit(wunit) && wunit.GetGradeLvl() < g.world.playerUnit.GetGradeLvl() + 2 &&
                    !rs.Any((x) => x.GetUnitId() == wunit.GetUnitId()))
                {
                    rs.Add(wunit);
                }
            }
            return rs;
        }

        public static List<WorldUnitBase> GetTownGuardians(MapBuildTown town)
        {
            var rs = new List<WorldUnitBase>();
            if (town == null || !Instance.TownMasters.ContainsKey(town.buildData.id))
                return rs;
            foreach (var wunitId in Instance.TownMasters[town.buildData.id])
            {
                var wunit = g.world.unit.GetUnit(wunitId);
                if (wunit == null)
                    continue;
                rs.Add(wunit);
            }
            return rs;
        }

        public static WorldUnitBase GetTownMaster(MapBuildTown town)
        {
            if (town == null || !Instance.TownMasters.ContainsKey(town.buildData.id))
                return null;
            foreach (var wunitId in Instance.TownMasters[town.buildData.id])
            {
                var wunit = g.world.unit.GetUnit(wunitId);
                if (wunit == null)
                    continue;
                if (IsTownMaster(wunit))
                    return wunit;
            }
            return null;
        }

        public static MapBuildTown GetGuardTown(WorldUnitBase wunit)
        {
            var wunitId = wunit.GetUnitId();
            if (Instance.TownMasters.Any(x => x.Value.Contains(wunitId)))
            {
                var townId = Instance.TownMasters.First(x => x.Value.Contains(wunitId)).Key;
                return g.world.build.GetBuild<MapBuildTown>(townId);
            }
            return null;
        }

        public static string GetGuardTownInfoStr(WorldUnitBase wunit)
        {
            return GetGuardTown(wunit)?.name;
        }

        public static void RemoveFromTownGuardians(string wunitId, WorldUnitBase wunit)
        {
            if (!string.IsNullOrEmpty(wunitId))
            {
                foreach (var data in Instance.TownMasters)
                {
                    if (data.Value.Contains(wunitId))
                    {
                        data.Value.RemoveAll(x => x == wunitId);
                        break;
                    }
                }
            }
            wunit?.DelLuck(TOWN_MASTER_LUCK_ID);
            wunit?.DelLuck(TOWN_GUARDIAN_LUCK_ID);
        }

        public static void Leave(WorldUnitBase wunit)
        {
            if (wunit == null)
                return;
            RemoveFromTownGuardians(wunit.GetUnitId(), wunit);
        }

        public static int GetTotalMonthlyPayment(MapBuildTown town)
        {
            var rs = 0;
            foreach (var guardian in GetTownGuardians(town))
            {
                if (!IsTownMaster(guardian))
                    rs += GetRequiredSpiritStones(town, guardian);
            }
            return rs;
        }

        public static int GetRequiredSpiritStones(MapBuildTown town, WorldUnitBase wunit)
        {
            var k = (Instance.TownMasters.ContainsKey(town.buildData.id) ? Instance.TownMasters[town.buildData.id].Count : 1) + 1;
            return (Math.Pow(3, wunit.GetGradeLvl()) * 1000 * k).Parse<int>();
        }

        public static int GetRequiredReputations(MapBuildTown town, WorldUnitBase wunit)
        {
            var k = (Instance.TownMasters.ContainsKey(town.buildData.id) ? Instance.TownMasters[town.buildData.id].Count : 1) + 1;
            return (Math.Pow(2, k) * 100 + Math.Pow(3, wunit.GetGradeLvl()) * 10).Parse<int>();
        }
    }
}
