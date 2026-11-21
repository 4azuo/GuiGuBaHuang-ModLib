using MOD_nE7UL2.Helper;
using EBattleTypeData;
using EGameTypeData;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ModLib.Helper;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Town/sect economy and government: budgets, taxes, town masters and guardians, deposits, upgrade to city.
    /// </summary>
    [Cache(ModConst.MAP_BUILD_PROPERTY_EVENT)]
    public class MapBuildPropertyEvent : ModEvent
    {
        public static MapBuildPropertyEvent Instance { get; set; }

        public const int INIT_BUDGET = 1000;
        public const int TOWN_YEARLY_BUDGET = 300;
        public const int AUCTION_YEARLY_BUDGET = 150;
        public const int SCHOOL_YEARLY_BUDGET = 500;

        public const float FIXING_RATE = 5.00f;
        public const int TOWN_MASTER_LUCK_ID = 420041111;
        public const int TOWN_GUARDIAN_LUCK_ID = 420041112;
        public const string TOWN_COUCIL_LUCK_DESC = "townmaster420041110desc";

        public const int BECOME_TOWN_MASTER_DRAMA = 420041113;
        public const int BECOME_TOWN_GUARDIAN_DRAMA = 420041114;
        public const int BECOME_TOWN_GUARDIAN_DRAMA_OPT1 = 420041115;
        public const int BECOME_TOWN_GUARDIAN_DRAMA_OPT2 = 420041116;

        public const int TAXPAY_NOT_ENOUGH_MONEY_DRAMA = 501020027;
        public const int TAXPAY_NOT_ENOUGH_MONEY_DRAMA_OPT1 = 501120027;
        public const int TAXPAY_NOT_ENOUGH_MONEY_DRAMA_OPT2 = 501220027;

        public const int CATCH_SNEAKY_DUNGEON_ID = 480110993;

        public Dictionary<string, float> TaxRate { get; set; } = new Dictionary<string, float>();
        /// <summary>Town tax statistics (this month and the previous one), shown to the town master.</summary>
        public Dictionary<string, TaxStat> TaxStats { get; set; } = new Dictionary<string, TaxStat>();

        public class TaxStat
        {
            public int Month { get; set; }
            public long Income { get; set; }
            public int Payers { get; set; }
            public int Leavers { get; set; }
            public long LastIncome { get; set; }
            public int LastPayers { get; set; }
            public int LastLeavers { get; set; }
        }

        /// <summary>Counts a tax payment (paid) or a resident who left (not paid) for the month.</summary>
        public static void RecordTax(MapBuildTown town, int tax, bool paid)
        {
            var id = town.buildData.id;
            var month = GameHelper.GetGameTotalMonth();
            if (!Instance.TaxStats.ContainsKey(id))
                Instance.TaxStats[id] = new TaxStat { Month = month };
            var s = Instance.TaxStats[id];
            if (s.Month != month)
            {
                s.LastIncome = s.Month == month - 1 ? s.Income : 0;
                s.LastPayers = s.Month == month - 1 ? s.Payers : 0;
                s.LastLeavers = s.Month == month - 1 ? s.Leavers : 0;
                s.Income = 0; s.Payers = 0; s.Leavers = 0; s.Month = month;
            }
            if (paid) { s.Income += tax; s.Payers++; }
            else s.Leavers++;
        }

        /// <summary>"Last month: income from n residents, m left" of a town.</summary>
        public static string TaxStatText(MapBuildTown town)
        {
            TaxStat s;
            if (!Instance.TaxStats.TryGetValue(town.buildData.id, out s))
                return GameTool.LS("town_taxstat_none");
            var last = s.Month == GameHelper.GetGameTotalMonth();
            return string.Format(GameTool.LS("town_taxstat"), last ? s.LastIncome : s.Income, last ? s.LastPayers : s.Payers, last ? s.LastLeavers : s.Leavers);
        }
        public Dictionary<string, long> Budget { get; set; } = new Dictionary<string, long>();
        public Dictionary<string, List<string>> TownMasters { get; set; } = new Dictionary<string, List<string>>();
        public List<string> PayTaxTown { get; } = new List<string>();

        public static bool isShowHirePeopleUI = false;
        public static bool isShowManageTeamUI1 = false;
        public static bool isShowManageTeamUI2 = false;

        /// <summary>
        /// Initializes tax rates, initial budgets and a town master for each town.
        /// </summary>
        public override void OnLoadGame()
        {
            base.OnLoadGame();

            foreach (var town in ModMain.Store.Towns)
            {
                //default tax rate
                if (!TaxRate.ContainsKey(town.buildData.id))
                {
                    TaxRate.Add(town.buildData.id, 1f);
                }
                //default budget
                if (!Budget.ContainsKey(town.buildData.id))
                {
                    AddBuildProperty(town, GetBaseTax(town) * INIT_BUDGET);
                }
                //town master & guardians
                if (!TownMasters.ContainsKey(town.buildData.id))
                {
                    TownMasters.Add(town.buildData.id, new List<string>());

                    var aroundWUnits = WUnitHelper.GetUnitsAround(town.GetOrigiPoint(), 4, false, true).ToArray().Where(x => IsMatchCondWUnit(x)).ToList();
                    if (aroundWUnits.Count > 0)
                    {
                        var master = aroundWUnits.GetFamousWUnit();
                        Hire(town, master, TOWN_MASTER_LUCK_ID);
                    }
                }
            }
            foreach (var school in ModMain.Store.Schools)
            {
                //default tax rate
                if (!TaxRate.ContainsKey(school.buildData.id))
                {
                    TaxRate.Add(school.buildData.id, 1f);
                }
            }
        }

        /// <summary>
        /// Adds town management/guardian/deposit buttons to town UIs and hire buttons on NPCInfo.
        /// </summary>
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            var player = g.world.playerUnit;

            if (e.uiType.uiName == UIType.Town.uiName)
            {
                //pay town tax (a house owner pays the fixed tax of the house instead, EstateEvent)
                var town = player.GetMapBuild<MapBuildTown>();
                if (town != null && !PayTaxTown.Contains(town.buildData.id) && !IsTownGuardian(town, player) && !EstateEvent.HasHouse(town, player))
                {
                    var guard = GetTownGuardians(town).Random();
                    CheckIn(guard, town, e.ui);
                }
            }
            else
            if (e.uiType.uiName == UIType.School.uiName)
            {
                //pay school tax
                var school = player.GetMapBuild<MapBuildSchool>();
                if (school != null && !PayTaxTown.Contains(school.buildData.id) && !MapBuildPropertyEvent.IsSchoolMember(school, player))
                {
                    var guard = g.world.unit.GetUnitExact(g.world.playerUnit.GetUnitPos(), 4).ToArray().Where(x => x.data.school?.schoolNameID == school.schoolNameID).ToArray().Random();
                    CheckIn(guard, school, e.ui);
                }
            }
            else
            if (e.uiType.uiName == UIType.SkyTip.uiName)
            {
                var ui = g.ui.GetUI<UISkyTip>(UIType.SkyTip);
                if (ui?.ptextTip.text == TOWN_COUCIL_LUCK_DESC)
                {
                    var a = g.ui.GetUI<UINPCInfo>(UIType.NPCInfo);
                    var b = g.ui.GetUI<UIPlayerInfo>(UIType.PlayerInfo);
                    ui.ptextTip.text = GetGuardTown(a?.unit ?? b?.unit)?.name;
                }
            }
            else
            if (e.uiType.uiName == UIType.NPCInfo.uiName)
            {
                var ui = g.ui.GetUI<UINPCInfo>(e.uiType);
                if (ui.unit.GetUnitId() != player.GetUnitId())
                {
                    var uiCover = new UICover<UINPCInfo>(e.ui);
                    if (IsTownGuardian(uiCover.UI.unit))
                    {
                        var town = GetGuardTown(uiCover.UI.unit);
                        if (IsTownMaster(town, g.world.playerUnit))
                            uiCover.AddText(300f, 100f, string.Format(GameTool.LS("other500020042"), GetRequiredSpiritStones(town, uiCover.UI.unit))).Align().Format(Color.white)
                                    .SetParentTransform(uiCover.UI.uiProperty.textAddLuckTitle);
                        uiCover.AddText(300f, 80f, ModTextHelper.LSF("ui_town_info", GetGuardTownInfoStr(uiCover.UI.unit))).Align().Format(Color.white)
                                    .SetParentTransform(uiCover.UI.uiProperty.textAddLuckTitle);
                        if (isShowManageTeamUI1)
                        {
                            uiCover.AddButton(200f, 90f, () =>
                            {
                                InfoDialog.Msg(GameTool.LS("other500020011"), GameTool.LS("other500020007"), MsgBoxButtonEnum.YesNo, () =>
                                {
                                    Leave(uiCover.UI.unit);
                                    g.ui.CloseUI(e.ui);
                                });
                            }, GameTool.LS("other500020012")).Format(Color.black).Size(100, 40)
                                    .SetParentTransform(uiCover.UI.uiProperty.textAddLuckTitle);
                        }
                    }
                    else
                    if (isShowHirePeopleUI)
                    {
                        var town = uiCover.UI.unit.GetMapBuild<MapBuildTown>();
                        uiCover.AddText(300f, 100f, string.Format(GameTool.LS("other500020042"), GetRequiredSpiritStones(town, uiCover.UI.unit))).Align().Format(Color.white)
                                    .SetParentTransform(uiCover.UI.uiProperty.textAddLuckTitle);
                        uiCover.AddText(300f, 80f, ModTextHelper.LSF("ui_reputations_n", GetRequiredReputations(town, uiCover.UI.unit))).Align().Format(Color.white)
                                    .SetParentTransform(uiCover.UI.uiProperty.textAddLuckTitle);
                        uiCover.AddButton(200f, 90f, () =>
                        {
                            var requiredSpiritStones = GetRequiredSpiritStones(town, uiCover.UI.unit);
                            if (player.GetUnitMoney() < requiredSpiritStones)
                            {
                                InfoDialog.Msg(GameTool.LS("other500020011"), ModTextHelper.LSF("ui_require_stones", requiredSpiritStones));
                                return;
                            }

                            var requiredReputations = GetRequiredReputations(town, uiCover.UI.unit);
                            if (player.GetDynProperty(UnitDynPropertyEnum.Reputation).value < requiredReputations)
                            {
                                InfoDialog.Msg(GameTool.LS("other500020011"), ModTextHelper.LSF("ui_require_reputations", requiredReputations));
                                return;
                            }

                            InfoDialog.Msg(GameTool.LS("other500020011"), GameTool.LS("other500020008"), MsgBoxButtonEnum.YesNo, () =>
                            {
                                Hire(town, uiCover.UI.unit, TOWN_GUARDIAN_LUCK_ID);
                                AddBuildProperty(town, -requiredSpiritStones);
                                g.ui.CloseUI(e.ui);
                            });
                        }, GameTool.LS("other500020013")).Format(Color.black).Size(100, 40)
                                    .SetParentTransform(uiCover.UI.uiProperty.textAddLuckTitle);
                    }
                    uiCover.UpdateUI();
                }
            }
        }

        /// <summary>
        /// Resets UI flags.
        /// </summary>
        public override void OnCloseUIStart(CloseUIStart e)
        {
            base.OnCloseUIStart(e);
            if (e.uiType.uiName == UIType.NPCSearch.uiName)
            {
                if (isShowHirePeopleUI)
                    isShowHirePeopleUI = false;
                else if (isShowManageTeamUI1)
                    isShowManageTeamUI1 = false;
                else if (isShowManageTeamUI2)
                    isShowManageTeamUI2 = false;
            }
        }

        /// <summary>
        /// Clears the list of towns paid this month.
        /// </summary>
        public override void OnMonthly()
        {
            base.OnMonthly();
            //clear lst pay tax town
            PayTaxTown.Clear();
        }

        /// <summary>
        /// Fixes guardian lucks, collects town/sect taxes from NPCs (inn rest included), expels NPCs that cannot pay.
        /// </summary>
        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);

            var wunitId = wunit.GetUnitId();

            //fix bug every month
            if (IsTownGuardian(wunit) && TownMasters.All(x => !x.Value.Contains(wunitId)))
            {
                RemoveFromTownGuardians(wunitId, wunit);
            }
            else
            if (!IsTownGuardian(wunit) && TownMasters.Any(x => x.Value.Contains(wunitId)))
            {
                RemoveFromTownGuardians(wunitId, wunit);
            }

            //town tax
            var town = wunit.GetMapBuild<MapBuildTown>();
            if (town != null)
            {
                //pay town tax
                if (TownMasters[town.buildData.id].Contains(wunit.GetUnitId()))
                {
                    wunit.AddProperty<int>(UnitPropertyEnum.Hp, wunit.GetDynProperty(UnitDynPropertyEnum.HpMax).value);
                    wunit.AddProperty<int>(UnitPropertyEnum.Mp, wunit.GetDynProperty(UnitDynPropertyEnum.MpMax).value);
                    wunit.AddProperty<int>(UnitPropertyEnum.Sp, wunit.GetDynProperty(UnitDynPropertyEnum.SpMax).value);
                }
                else if (!wunit.IsPlayer()) //npc only
                {
                    var tax = GetTax(town, wunit) * ModLib.Mod.ModMaster.MonthsPassed;
                    var townMaster = GetTownMaster(town);
                    if (wunit.GetUnitMoney() > tax)
                    {
                        if (townMaster != null && tax > wunit.GetUnitMoney() * 0.01)
                            wunit.data.unitData.relationData.AddHate(townMaster.GetUnitId(), 1);
                        if (townMaster != null && tax < wunit.GetUnitMoney() * 0.001)
                            wunit.data.unitData.relationData.AddIntim(townMaster.GetUnitId(), 1);

                        wunit.AddUnitMoney(-tax);
                        AddBuildProperty(town, tax);
                        RecordTax(town, tax, true);

                        //use inn
                        wunit.AddProperty<int>(UnitPropertyEnum.Hp, wunit.GetDynProperty(UnitDynPropertyEnum.HpMax).value);
                        wunit.AddProperty<int>(UnitPropertyEnum.Mp, wunit.GetDynProperty(UnitDynPropertyEnum.MpMax).value);
                        wunit.AddProperty<int>(UnitPropertyEnum.Sp, wunit.GetDynProperty(UnitDynPropertyEnum.SpMax).value);
                    }
                    else
                    {
                        //get out
                        RecordTax(town, tax, false);
                        if (townMaster != null)
                            wunit.data.unitData.relationData.AddHate(townMaster.GetUnitId(), 5);
                        // a random cell near may still be in the town (a town has several cells): further until out
                        var from = wunit.GetUnitPos();
                        for (var r = 3; r <= 12 && wunit.GetMapBuild<MapBuildTown>() != null; r++)
                            wunit.SetUnitRandomPos(from, r);
                    }
                }
            }

            //school tax
            var school = wunit.GetMapBuild<MapBuildSchool>();
            if (school != null && !IsSchoolMember(school, wunit))
            {
                //pay school tax
                var tax = GetTax(school, wunit) * ModLib.Mod.ModMaster.MonthsPassed;
                wunit.AddUnitMoney(-tax);
                school.buildData.money += tax;
            }
        }

        /// <summary>
        /// Yearly budgets, elects a new master when needed, pays guardians.
        /// </summary>
        public override void OnYearly()
        {
            base.OnYearly();
            // one dispatch may stand for several years (time skip): budgets and payments of every year
            var years = Math.Max(1, ModLib.Mod.ModMaster.YearsPassed);

            foreach (var town in ModMain.Store.Towns)
            {
                if (!TownMasters.ContainsKey(town.buildData.id))
                    TownMasters.Add(town.buildData.id, new List<string>());
                var townMasterData = TownMasters[town.buildData.id];

                //choose new town master
                var townCouncilWUnits = GetTownGuardians(town);
                var master = GetTownMaster(town);
                if (master == null || master.isDie)
                {
                    master = townCouncilWUnits.GetFamousWUnit();
                    //promote to town-master
                    if (master != null)
                    {
                        var masterId = master.GetUnitId();
                        master.DelLuck(TOWN_GUARDIAN_LUCK_ID);
                        master.AddLuck(TOWN_MASTER_LUCK_ID);

                        if (master.IsPlayer())
                            DramaTool.OpenDrama(BECOME_TOWN_MASTER_DRAMA);
                    }
                }

                if (master != null && !master.IsPlayer())
                {
                    //random tax rate
                    //(range: estate_configs.json Population.NpcTaxRate; a high rate drives the mortals away, EstateEvent.TaxFactor)
                    var npcRate = EstateEvent.Conf.Population.NpcTaxRate;
                    TaxRate[town.buildData.id] = CommonTool.Random(npcRate[0], npcRate[1]);

                    //hire more people
                    if (GetBuildProperty(town) > GetRequiredSpiritStones(town, master.GetGradeLvl()) * 1.5)
                    {
                        var aroundWUnits = WUnitHelper.GetUnitsAround(town.GetOrigiPoint(), 4, false, true).ToArray().Where(x => IsMatchCondWUnit(x)).ToList();
                        if (aroundWUnits.Count > 0)
                        {
                            var newGuard = aroundWUnits.GetFamousWUnit();
                            //hire player
                            if (newGuard.IsPlayer())
                            {
                                DramaTool.OpenDrama(BECOME_TOWN_GUARDIAN_DRAMA, new DramaData
                                {
                                    dialogueText = { [BECOME_TOWN_GUARDIAN_DRAMA] = string.Format(GameTool.LS("townmaster420041114drama"), town.name) },
                                    onOptionsClickCall = ActionHelper.TracedIl2Action<ConfDramaOptionsItem>((x) =>
                                    {
                                        switch (x.id)
                                        {
                                            case BECOME_TOWN_GUARDIAN_DRAMA_OPT1:
                                                Hire(town, newGuard, TOWN_GUARDIAN_LUCK_ID);
                                                break;
                                            case BECOME_TOWN_GUARDIAN_DRAMA_OPT2:
                                                break;
                                        }
                                    })
                                });
                            }
                            //hire npc
                            else
                            {
                                Hire(town, newGuard, TOWN_GUARDIAN_LUCK_ID);
                            }
                        }
                    }
                }

                //budget inc yearly
                //(the town master's palace raises it, EstateEvent)
                AddBuildProperty(town, (long)Math.Round((double)GetBaseTax(town) * TOWN_YEARLY_BUDGET * years * EstateEvent.PalaceBudgetRate(town)));
                
                //budget inc by auction
                var auction = town.GetBuildSub<MapBuildTownAuction>();
                if (auction != null)
                {
                    AddBuildProperty(town, GetBaseTax(town) * AUCTION_YEARLY_BUDGET * years);
                }

                //guardians
                foreach (var wunit in townCouncilWUnits)
                {
                    if (wunit != null && !wunit.isDie)
                    {
                        if (IsTownMaster(wunit))
                            continue;

                        //payment
                        var profit = GetRequiredSpiritStones(town, wunit) * years;
                        if (Budget[town.buildData.id] > profit)
                        {
                            AddBuildProperty(town, -profit);
                            wunit.AddUnitMoney(profit);
                        }
                        else
                        {
                            Leave(wunit);
                            wunit.data.unitData.relationData.AddHate(GetTownMaster(town).GetUnitId(), 100);
                        }

                        if (wunit.IsPlayer())
                            continue;
                        //come back the town
                        wunit.SetUnitPos(town.GetOrigiPoint());
                    }
                    else
                    {
                        RemoveFromTownGuardians(wunit?.GetUnitId(), wunit);
                    }
                }

                //master npc's payment
                if (master != null && !master.IsPlayer())
                {
                    var profit = (GetRequiredSpiritStones(town, master) + (GetBuildProperty(town) / 100).FixValue(0, int.MaxValue).Parse<int>()) * years;
                    if (Budget[town.buildData.id] > profit)
                    {
                        AddBuildProperty(town, -profit);
                        master.AddUnitMoney(profit);
                    }
                    //he'll come back to his town yearly
                    master.SetUnitPos(town.GetOrigiPoint());
                }
            }

            foreach (var school in ModMain.Store.Schools)
            {
                //school budget inc yearly
                school.buildData.money += GetBaseTax(school) * SCHOOL_YEARLY_BUDGET * years;
            }
        }

        /// <summary>
        /// Handles the death of masters/guardians.
        /// </summary>
        public override void OnBattleUnitDie(UnitDie e)
        {
            base.OnBattleUnitDie(e);

            if (DungeonHelper.IsRealBattle() &&
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

        /// <summary>
        /// Makes a unit master/guardian of a town.
        /// </summary>
        public static void Hire(MapBuildTown town, WorldUnitBase member, int luckId)
        {
            Instance.TownMasters[town.buildData.id].Add(member.GetUnitId());
            member.AddLuck(luckId);
        }

        /// <summary>
        /// Base tax of a building.
        /// </summary>
        public static int GetBaseTax(MapBuildBase buildbase)
        {
            //default tax rate
            if (!Instance.TaxRate.ContainsKey(buildbase.buildData.id))
                Instance.TaxRate[buildbase.buildData.id] = 1f;
            //base on area
            var baseTax = SMLocalConfigsEvent.Instance.Calculate(Convert.ToInt32(InflationaryEvent.CalculateInflationary((
                Math.Pow(2, buildbase.gridData.areaBaseID) * FIXING_RATE * Instance.TaxRate[buildbase.buildData.id]
            ).Parse<int>())), SMLocalConfigsEvent.Instance.Configs.AddTaxRate).Parse<int>();
            //school tax
            var school = buildbase.TryCast<MapBuildSchool>();
            if (school != null)
            {
                //if school -> *effect-count
                baseTax *= school.schoolData.allEffects.Count;
            }
            //town tax
            if (buildbase.IsCity())
            {
                baseTax *= 2;
            }
            return baseTax;
        }

        /// <summary>
        /// Tax for a unit (S&amp;M tax rate, guardians exempt).
        /// </summary>
        public static int GetTax(MapBuildBase buildbase, WorldUnitBase wunit)
        {
            if (wunit == null || wunit.isDie)
                return 0;
            var tax = GetBaseTax(buildbase);
            //if merchant, add more tax
            // (multiplied as floats: the rates are like 1.2, an int cast of the rate would drop the increase)
            if (UnitTypeEvent.GetUnitTypeEnum(wunit) == UnitTypeEnum.Merchant)
            {
                tax = (tax * (1.00f + UnitTypeLuckEnum.Merchant.CustomEffects[ModConst.UTYPE_LUCK_EFX_SELL_VALUE].Value0.Parse<float>())).Parse<int>();
            }
            tax = (tax * (1.00f + MerchantLuckEnum.Merchant.GetCurLevel(wunit) * MerchantLuckEnum.Merchant.IncSellValueEachLvl)).Parse<int>();
            //wanted by the bank of this town (unpaid loan, BankLoanEvent)
            if (wunit.IsPlayer() && BankLoanEvent.IsWanted(buildbase))
                tax *= BankLoanEvent.WANTED_TAX_RATE;
            return tax;
        }

        /// <summary>
        /// Budget (spirit stones) of a town/sect.
        /// </summary>
        public static long GetBuildProperty(MapBuildBase build)
        {
            if (build == null)
                return 0;

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

        /// <summary>
        /// Adds to (or removes from) a budget.
        /// </summary>
        public static void AddBuildProperty(MapBuildBase build, long add)
        {
            if (build == null)
                return;

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

        /// <summary>
        /// True when the unit has a guardian luck without being registered.
        /// </summary>
        public static bool IsErrorLuckId(WorldUnitBase wunit)
        {
            return wunit.GetLuck(TOWN_GUARDIAN_LUCK_ID) != null && wunit.GetLuck(TOWN_MASTER_LUCK_ID) != null;
        }

        /// <summary>
        /// True when the unit guards a (given) town.
        /// </summary>
        public static bool IsTownGuardian(WorldUnitBase wunit)
        {
            return wunit != null && (wunit.GetLuck(TOWN_GUARDIAN_LUCK_ID) != null || wunit.GetLuck(TOWN_MASTER_LUCK_ID) != null);
        }

        /// <summary>
        /// True when the unit is master of a (given) town.
        /// </summary>
        public static bool IsTownMaster(WorldUnitBase wunit)
        {
            return wunit != null && wunit.GetLuck(TOWN_MASTER_LUCK_ID) != null;
        }

        /// <summary>
        /// True when the unit guards a (given) town.
        /// </summary>
        public static bool IsTownGuardian(MapBuildTown town, WorldUnitBase wunit)
        {
            if (town == null || wunit == null)
                return false;
            return Instance.TownMasters[town.buildData.id].Contains(wunit.GetUnitId());
        }

        /// <summary>
        /// True when the unit is master of a (given) town.
        /// </summary>
        public static bool IsTownMaster(MapBuildTown town, WorldUnitBase wunit)
        {
            return IsTownGuardian(town, wunit) && IsTownMaster(wunit);
        }

        /// <summary>
        /// True when the unit belongs to a (given) sect.
        /// </summary>
        public static bool IsSchoolMember(WorldUnitBase wunit)
        {
            return wunit.data.school?.schoolNameID != null;
        }

        /// <summary>
        /// True when the unit belongs to a (given) sect.
        /// </summary>
        public static bool IsSchoolMember(MapBuildSchool school, WorldUnitBase wunit)
        {
            if (school == null)
                return false;
            return school.schoolNameID == wunit.data.school?.schoolNameID;
        }

        /// <summary>
        /// Cost to upgrade a town to a city.
        /// </summary>
        public static long GetUpgrade2CityCost(MapBuildTown town)
        {
            return (Math.Pow(3, town.gridData.areaBaseID) * 1000000).Parse<long>();
        }

        /// <summary>
        /// Upgrades a town to a city.
        /// </summary>
        public static void Upgrade2City(MapBuildTown town)
        {
            var budget = Instance.Budget[town.buildData.id];
            var cost = GetUpgrade2CityCost(town);
            if (budget > cost)
            {
                town.buildTownData.isMainTown = true;
                Instance.Budget[town.buildData.id] -= cost;
            }
            else
            {
                InfoDialog.Msg(GameTool.LS("other500020011"), GameTool.LS("other500020025"));
            }
        }

        /// <summary>
        /// Moves player money to the town budget.
        /// </summary>
        public static void Deposit()
        {
            var uiPropSelectCount = g.ui.OpenUISafe<UIPropSelectCount>(UIType.PropSelectCount);
            uiPropSelectCount.minCount = 0;
            uiPropSelectCount.maxCount = g.world.playerUnit.GetUnitMoney();
            uiPropSelectCount.oneCost = 1;
            uiPropSelectCount.btnOK.onClick.RemoveAllListeners();
            uiPropSelectCount.btnOK.onClick.AddListener(ActionHelper.TracedUnityAction(() =>
            {
                var town = g.world.playerUnit.GetMapBuild<MapBuildTown>();
                AddBuildProperty(town, uiPropSelectCount.curSelectCount);
                g.world.playerUnit.AddUnitMoney(-uiPropSelectCount.curSelectCount);
                g.ui.CloseUI(uiPropSelectCount);
                UIHelper.UpdateAllUI();
            }));
            uiPropSelectCount.textGrade.Set(string.Empty);
            uiPropSelectCount.textName.Set(string.Empty);
            uiPropSelectCount.ptextInfo.text = string.Empty;
            uiPropSelectCount.textTitle.Set(GameTool.LS("other500020015"));
        }

        /// <summary>
        /// Moves town budget to the player (town master).
        /// </summary>
        public static void Withdraw()
        {
            var town = g.world.playerUnit.GetMapBuild<MapBuildTown>();
            var uiPropSelectCount = g.ui.OpenUISafe<UIPropSelectCount>(UIType.PropSelectCount);
            uiPropSelectCount.minCount = 0;
            uiPropSelectCount.maxCount = Instance.Budget[town.buildData.id].FixValue(0, int.MaxValue).Parse<int>();
            uiPropSelectCount.oneCost = 1;
            uiPropSelectCount.btnOK.onClick.RemoveAllListeners();
            uiPropSelectCount.btnOK.onClick.AddListener(ActionHelper.TracedUnityAction(() =>
            {
                AddBuildProperty(town, -uiPropSelectCount.curSelectCount);
                g.world.playerUnit.AddUnitMoney(uiPropSelectCount.curSelectCount);
                g.ui.CloseUI(uiPropSelectCount);
                UIHelper.UpdateAllUI();
            }));
            uiPropSelectCount.textGrade.Set(string.Empty);
            uiPropSelectCount.textName.Set(string.Empty);
            uiPropSelectCount.ptextInfo.text = string.Empty;
            uiPropSelectCount.textTitle.Set(GameTool.LS("other500020016"));
        }

        /// <summary>
        /// Guardian hiring UI.
        /// </summary>
        public static void OpenUIHirePeople()
        {
            var ui = g.ui.OpenUISafe<UINPCSearch>(UIType.NPCSearch);
            ui.InitData(new Vector2Int(0, 0));
            ui.units = GetHirablePeople().ToIl2CppList();
            ui.UpdateUI();
            isShowHirePeopleUI = true;

            var town = GetGuardTown(g.world.playerUnit);
            UIHelper.GetUICustomBase(UIType.NPCSearch).Dispose();

            //var i = -1;
            //var uiCover = new UICover<UINPCSearch>(ui);
            //{
            //    var col = uiCover.MidCol - 11;
            //    var row = uiCover.MidRow;
            //    uiCover.AddText(col, row + i++, string.Format(GameTool.LS("townmaster420041117"), town.name, GetTownMaster(town).GetName())).Format().Align();
            //    uiCover.AddText(col, row + i++, GameTool.LS("townmaster420041118")).Format().Align().SetWork(new UIItemWork
            //    {
            //        Formatter = (ibase) => new object[] { GetBuildProperty(town) },
            //    });
            //    uiCover.AddText(col, row + i++, GameTool.LS("townmaster420041119")).Format().Align().SetWork(new UIItemWork
            //    {
            //        Formatter = (ibase) => new object[] { GetTotalMonthlyPayment(town) },
            //    });
            //}
            //uiCover.UpdateUI();
        }

        /// <summary>
        /// Town management window.
        /// </summary>
        public static void OpenUITownManage(MapBuildTown town)
        {
            var ui = g.ui.OpenUISafe<UINPCSearch>(UIType.NPCSearch);
            ui.InitData(new Vector2Int(0, 0));
            ui.units = GetTownGuardians(town).ToIl2CppList();
            ui.UpdateUI();
            isShowManageTeamUI1 = true;

            var i = -1;
            var uiCover = new UICover<UINPCSearch>(ui);
            {
                var col = uiCover.MidCol - 11;
                var row = uiCover.MidRow;
                uiCover.AddText(col, row + i++, string.Format(GameTool.LS("townmaster420041117"), town.name, GetTownMaster(town).GetName())).Format().Align();
                uiCover.AddText(col, row + i++, GameTool.LS("townmaster420041118")).Format().Align().SetWork(new UIItemWork
                {
                    Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>((ibase) => new object[] { GetBuildProperty(town) }),
                });
                uiCover.AddText(col, row + i++, GameTool.LS("townmaster420041119")).Format().Align().SetWork(new UIItemWork
                {
                    Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>((ibase) => new object[] { GetTotalMonthlyPayment(town) }),
                });
                uiCover.AddCompositeSlider(col, row + i++, GameTool.LS("ui_base_tax"), 0.40f, 20.00f, Instance.TaxRate[town.buildData.id], GameTool.LS("ui_per_month")).SetWork(new UIItemWork
                {
                    Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>((ibase) => new object[] { GetBaseTax(town) }),
                    ChangeAct = ActionHelper.WTracedAction<UIItemBase, object>((ibase, value) => Instance.TaxRate[town.buildData.id] = value.Parse<float>()),
                });
                uiCover.AddToolTipButton(col - 2, row + i, GameTool.LS("town_tax_help"));
                uiCover.AddText(col, row + i++, "{0}").Format().Align().SetWork(new UIItemWork
                {
                    Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>((ibase) => new object[] { TaxStatText(town) }),
                });
                //mortals of the town (EstateEvent)
                uiCover.AddText(col, row + i++, "{0}").Format().Align().SetWork(new UIItemWork
                {
                    Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>((ibase) =>
                    {
                        var e = EstateEvent.Get(town);
                        return new object[] { e == null ? string.Empty : string.Format(GameTool.LS("estate_manage_line"), e.Population, EstateEvent.Target(town, e), e.LastMortalTax) };
                    }),
                });

                i++;
                // the buildings of the town: picture, name and cost, then built / a "build" button / not in this town
                var c = 0;
                foreach (var em in BuildingCostEnum.GetAllEnums<BuildingCostEnum>().Where(x => !x.IsSchool && x.IsMatchBuildConds(town) && !BuildingArrangeEvent.IsIgnored(x)))
                {
                    // a cell: the picture, the state (or the "build" button), the name, the cost from the budget (own column)
                    var ex = uiCover.Columns[col - 4] + (c % 2) * 480f;
                    var ey = uiCover.Rows[uiCover.MidRow + i + (c / 2 * 3)];
                    var cost = BuildingArrangeEvent.GetBuildingCost(town, em);
                    EstateEvent.Picture(uiCover, BuildingArrangeEvent.Sprite(em.BuildType), ex, ey, 60f, 48f);
                    var label = uiCover.AddText(ex + 125f, ey, GameTool.LS(em.BuildingName)).Format().Align(TextAnchor.MiddleLeft);
                    label.Pos(ex + 125f + label.Item.rectTransform.rect.width / 2f, ey);
                    var price = uiCover.AddText(ex + 390f, ey, TextIconHelper.ToSprites($"{cost:#,##0} [icon:zm_lingshi]")).Format().Align(TextAnchor.MiddleRight);
                    price.Pos(ex + 390f - price.Item.rectTransform.rect.width / 2f, ey);
                    if (BuildingArrangeEvent.IsBuilt(town, em))
                        uiCover.AddText(ex + 75f, ey, GameTool.LS("estate_building_built")).Format(new Color(0.1f, 0.45f, 0.1f), 15).Align(TextAnchor.MiddleCenter);
                    else if (!BuildingArrangeEvent.IsBuildable(town, em))
                        uiCover.AddText(ex + 75f, ey, GameTool.LS("estate_building_none")).Format(Color.gray, 12).Align(TextAnchor.MiddleCenter);
                    else
                    {
                        uiCover.AddButton(ex + 75f, ey, () =>
                        {
                            if (GetBuildProperty(town) > cost)
                            {
                                BuildingArrangeEvent.Build(town, em);
                                g.ui.CloseUI(ui);
                                InfoDialog.Msg(GameTool.LS("other500020011"), ModTextHelper.LSF("ui_building_built", GameTool.LS(em.BuildingName)));
                            }
                            else
                            {
                                InfoDialog.Msg(GameTool.LS("other500020011"), ModTextHelper.LSF("ui_building_no_budget", GetBuildProperty(town)));
                            }
                        }, GameTool.LS("other500020017")).Format().Align(TextAnchor.MiddleCenter);
                    }
                    c++;
                }

                // one column of buttons, same size, evenly spaced from the "Plots" row to the "Dismiss" row
                var buttons = new List<(Action act, string text)>();
                if (EstateEvent.Enabled) buttons.Add((() => EstateEvent.OpenPlots(town), GameTool.LS("estate_button")));
                if (IsTownMaster(town, g.world.playerUnit) && !town.buildTownData.isMainTown)
                    buttons.Add((() => Upgrade2City(town), TextIconHelper.ToSprites(string.Format(GameTool.LS("estate_city_btn"), GetUpgrade2CityCost(town)) + " [icon:zm_lingshi]")));
                buttons.Add((Deposit, GameTool.LS("other500020015")));
                buttons.Add((Withdraw, GameTool.LS("other500020016")));
                buttons.Add((OpenUIHirePeople, GameTool.LS("other500020054")));
                buttons.Add((() =>
                {
                    InfoDialog.Msg(GameTool.LS("other500020011"), GameTool.LS("other500020009"), MsgBoxButtonEnum.YesNo, () =>
                    {
                        Leave(g.world.playerUnit);
                        g.ui.CloseUI(UIType.NPCSearch);
                        g.ui.CloseUI(UIType.Town);
                    });
                }, GameTool.LS("other500020012")));
                var bx0 = uiCover.Columns[uiCover.LastCol - 7];
                var top = uiCover.Rows[uiCover.LastRow - 23];
                var step = (uiCover.Rows[uiCover.LastRow - 8] - top) / (buttons.Count - 1);
                for (var k = 0; k < buttons.Count; k++)
                {
                    var (act, text) = buttons[k];
                    uiCover.AddButton(bx0, top + k * step, act, text).Format(Color.black, 15).Align(TextAnchor.MiddleCenter).Size(300, 48);
                }
            }
            uiCover.UpdateUI();
        }

        /// <summary>
        /// Guardians list.
        /// </summary>
        public static void OpenUITownGuardians(MapBuildTown town)
        {
            var ui = g.ui.OpenUISafe<UINPCSearch>(UIType.NPCSearch);
            ui.InitData(new Vector2Int(0, 0));
            ui.units = GetTownGuardians(town).ToIl2CppList();
            ui.UpdateUI();
            isShowManageTeamUI2 = true;

            if (IsTownMaster(g.world.playerUnit) && !IsTownGuardian(town, g.world.playerUnit))
            {
                var uiCover = new UICover<UINPCSearch>(ui);
                {
                    uiCover.AddButton(uiCover.LastCol - 10, uiCover.LastRow - 8, () =>
                    {
                        InfoDialog.Msg(GameTool.LS("other500020011"), GameTool.LS("other500020010"), MsgBoxButtonEnum.YesNo, () =>
                        {
                            MapBuildBattleEvent.TownWar(town, GetGuardTown(g.world.playerUnit), true);
                        });
                    }, GameTool.LS("other500020014")).Format(Color.black, 17).Align(TextAnchor.MiddleCenter).Size(300, 64);
                }
                uiCover.UpdateUI();
            }
        }

        /// <summary>
        /// True when the unit can become a guardian.
        /// </summary>
        public static bool IsMatchCondWUnit(WorldUnitBase wunit)
        {
            return wunit != null && !wunit.isDie && ((!IsTownGuardian(wunit) && !HirePeopleEvent.IsHired(wunit)) || wunit.IsPlayer());
        }

        /// <summary>
        /// Units that can be hired as guardians.
        /// </summary>
        public static List<WorldUnitBase> GetHirablePeople()
        {
            var rs = new List<WorldUnitBase>();
            var town = g.world.playerUnit.GetMapBuild<MapBuildTown>();
            foreach (var wunit in g.world.playerUnit.GetUnitsAround(4, false, false))
            {
                if (IsMatchCondWUnit(wunit) && wunit.GetGradeLvl() < g.world.playerUnit.GetGradeLvl() + 2 &&
                    wunit.GetMapBuild<MapBuildTown>()?.buildData.id == town.buildData.id &&
                    !rs.Any((x) => x.GetUnitId() == wunit.GetUnitId()))
                {
                    rs.Add(wunit);
                }
            }
            return rs;
        }

        /// <summary>
        /// Guardians of a town.
        /// </summary>
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

        /// <summary>
        /// Master of a town.
        /// </summary>
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

        /// <summary>
        /// Town guarded by a unit.
        /// </summary>
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

        /// <summary>
        /// Guarded town description.
        /// </summary>
        public static string GetGuardTownInfoStr(WorldUnitBase wunit)
        {
            return GetGuardTown(wunit)?.name;
        }

        /// <summary>
        /// Removes a unit from all towns and its lucks.
        /// </summary>
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

        /// <summary>
        /// A guardian leaves.
        /// </summary>
        public static void Leave(WorldUnitBase wunit)
        {
            if (wunit == null)
                return;
            RemoveFromTownGuardians(wunit.GetUnitId(), wunit);
        }

        /// <summary>
        /// Monthly salary of all guardians.
        /// </summary>
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

        /// <summary>
        /// Guardian salary.
        /// </summary>
        public static int GetRequiredSpiritStones(MapBuildTown town, int gradeLvl)
        {
            var k = (Instance.TownMasters.ContainsKey(town.buildData.id) ? Instance.TownMasters[town.buildData.id].Count : 1) + 1;
            return InflationaryEvent.CalculateInflationary((Math.Pow(3, gradeLvl) * 500 * k).Parse<int>());
        }

        /// <summary>
        /// Guardian salary.
        /// </summary>
        public static int GetRequiredSpiritStones(MapBuildTown town, WorldUnitBase wunit)
        {
            return GetRequiredSpiritStones(town, wunit.GetGradeLvl());
        }

        /// <summary>
        /// Reputation required to hire a guardian.
        /// </summary>
        public static int GetRequiredReputations(MapBuildTown town, WorldUnitBase wunit)
        {
            var k = (Instance.TownMasters.ContainsKey(town.buildData.id) ? Instance.TownMasters[town.buildData.id].Count : 1) + 1;
            return (Math.Pow(2, k) * 100 + Math.Pow(3, wunit.GetGradeLvl()) * 10).Parse<int>();
        }

        /// <summary>
        /// Guardian checks a sneaky unit (may start the catch battle).
        /// </summary>
        public static void CheckIn(WorldUnitBase guard, MapBuildBase buildBase, UIBase ui)
        {
            if (SMLocalConfigsEvent.Instance.Configs.NoTaxing)
                return;

            var player = g.world.playerUnit;
            var tax = GetTax(buildBase, player);
            if (player.GetUnitMoney() > tax)
            {
                AddBuildProperty(buildBase, tax);
                player.AddUnitMoney(-tax);
                Instance.PayTaxTown.Add(buildBase.buildData.id);
                TaoistLogEvent.Log("log_tax_paid", tax, buildBase.name);
            }
            else
            {
                DramaTool.OpenDrama(TAXPAY_NOT_ENOUGH_MONEY_DRAMA, new DramaData
                {
                    dialogueText = { [TAXPAY_NOT_ENOUGH_MONEY_DRAMA] = string.Format(GameTool.LS("other500020027"), tax) },
                    onOptionsClickCall = ActionHelper.TracedIl2Action<ConfDramaOptionsItem>((x) =>
                    {
                        switch (x.id)
                        {
                            case TAXPAY_NOT_ENOUGH_MONEY_DRAMA_OPT1:
                                if (guard != null && !CommonTool.Random(0f, 100f).IsBetween(0f, (Math.Max(player.GetGradeLvl() - buildBase.gridData.areaBaseID, 0) + 1) * 40f))
                                {
                                    g.world.battle.IntoBattle(guard, CATCH_SNEAKY_DUNGEON_ID, battleData: new WorldBattleData
                                    {
                                        npcUnit = guard
                                    });
                                }
                                break;
                            case TAXPAY_NOT_ENOUGH_MONEY_DRAMA_OPT2:
                                g.ui.CloseUI(ui);
                                break;
                        }
                    })
                });
            }
        }

        /// <summary>
        /// Periodic town UI refresh.
        /// </summary>
        [ErrorIgnore]
        [EventCondition(IsInGame = HandleEnum.Ignore, IsInBattle = HandleEnum.True)]
        public override void OnTimeUpdate1000ms()
        {
            base.OnTimeUpdate1000ms();
            if (g.world.battle.data.dungeonBaseItem.id == CATCH_SNEAKY_DUNGEON_ID && 
                (ModBattleEvent.PlayerUnit.isDie || ModBattleEvent.BattleUnits.ToArray().Where(x => x.IsEnemy(ModBattleEvent.PlayerUnit)).All(x => x.isDie)))
            {
                ModBattleEvent.SceneBattle.battleEnd.BattleEnd(!ModBattleEvent.PlayerUnit.isDie);
            }
        }
    }
}
