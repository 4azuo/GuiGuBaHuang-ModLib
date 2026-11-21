using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using Newtonsoft.Json;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-63 (EstateEvent): the mortals and plots of a town; the population moves with the tax rate and pays the mortal
    /// tax; the player buys a plot, builds and upgrades a house (grade gate), pays the monthly house tax, rests in the town
    /// and enters it without the entry tax; the plots window draws the houses; unpaid taxes take the house back.
    /// Estate of the town, budget, tax rate and money restored.
    /// </summary>
    public class TG63_Estate : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-63";

        private static void SetNotes(int n)
        {
            var have = SpiritNoteEvent.Notes(Player);
            if (have > n) Player.RemoveUnitProp(SpiritNoteEvent.NOTE_ID, have - n);
            else if (have < n) Player.AddUnitProp(SpiritNoteEvent.NOTE_ID, n - have);
        }

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            MapBuildTown town = null;
            EstateEvent.Plot plot = null;
            return new Scenario { Id = "TG-63", Feature = "EstateEvent", Title = "Town mortals, plots and the player's house", Overlay = false }
                .Step("travel", c =>
                {
                    if (!c.Assert(EstateEvent.Instance != null, "EstateEvent loaded")) return;
                    if (!TravelTo(c, FindTown(t => t.IsSmallTown()) ?? FindTown(t => true))) return;
                    town = BagTown(c);
                    var e = EstateEvent.Get(town);
                    c.Bag["estate"] = JsonConvert.SerializeObject(e);
                    c.Bag["budget"] = MapBuildPropertyEvent.GetBuildProperty(town);
                    c.Bag["rate"] = EstateEvent.TaxRateOf(town);
                    c.Bag["money"] = Player.GetUnitMoney();
                    c.Bag["notes"] = SpiritNoteEvent.Notes(Player);
                    // a town where the player already has a house: start without it
                    foreach (var p in e.Plots.Where(x => x.OwnerId == Player.GetUnitId() && x.Level > 0).ToList()) EstateEvent.Release(e, p);
                })
                .Step("mortals and plots", c =>
                {
                    var e = EstateEvent.Get(town);
                    var basePop = EstateEvent.BasePopulation(town);
                    c.Assert(EstateEvent.IsCurrent(e, town.IsCity()), $"map of the town: {EstateEvent.StyleName(e)}, {e.Plots.Count} plots");
                    c.Assert(e.Population >= basePop * 0.3 && e.Population <= basePop * 2.4 + 6 * EstateEvent.PopPerHouseLevel, $"population {e.Population:#,##0} (base {basePop:#,##0})");
                    c.Assert(e.Plots.Any(x => x.Level > 0 && x.OwnerId == null), $"mortal houses: {e.Plots.Count(x => x.Level > 0)}");
                    c.Assert(e.Plots.Max(x => x.Prosperity) > e.Plots.Min(x => x.Prosperity), "busy and quiet plots");
                    // a high tax rate drives the mortals away; the mortal tax goes to the budget
                    MapBuildPropertyEvent.Instance.TaxRate[town.buildData.id] = 10f;
                    var pop = e.Population;
                    var target = EstateEvent.Target(town, e);
                    var budget = MapBuildPropertyEvent.GetBuildProperty(town);
                    EstateEvent.MonthTown(town, 1);
                    c.Assert(e.Population < pop || pop <= target, $"tax rate 10: population {pop:#,##0} -> {e.Population:#,##0} (target {target:#,##0})");
                    c.AssertEqual(budget + e.LastMortalTax, MapBuildPropertyEvent.GetBuildProperty(town), $"mortal tax {e.LastMortalTax:#,##0} to the budget");
                    MapBuildPropertyEvent.Instance.TaxRate[town.buildData.id] = (float)c.Bag["rate"];
                })
                .Step("buy a plot", c =>
                {
                    var e = EstateEvent.Get(town);
                    plot = e.Plots.Where(x => x.Level == 0 && x.OwnerId == null).OrderByDescending(x => x.Prosperity).FirstOrDefault();
                    if (plot == null)
                    {
                        // every plot built: a mortal house leaves
                        plot = e.Plots.Where(x => x.OwnerId == null).OrderBy(x => x.Prosperity).First();
                        plot.Level = 0;
                    }
                    var cost = EstateEvent.LandPrice(town, plot) + EstateEvent.BuildCost(town, 1);
                    // a market plot costs more than an outskirts one (prices by district)
                    var m = e.Plots.First(x => x.District == "M"); var o = e.Plots.First(x => x.District == "O");
                    c.Assert(EstateEvent.LandPrice(town, m) > EstateEvent.LandPrice(town, o) * 3, $"market land {EstateEvent.LandPrice(town, m):#,##0} > 3 x outskirts {EstateEvent.LandPrice(town, o):#,##0}");
                    // houses and land are paid in spirit banknotes only (whole banknotes)
                    var notes = (int)SpiritNoteEvent.NotesFor(EstateEvent.Payable(cost));
                    Player.SetUnitMoney(int.MaxValue / 2);
                    SetNotes(notes - 1);
                    c.AssertEqual(EstateEvent.BuildResult.NoMoney, EstateEvent.Buy(town, plot, Player), "not enough banknotes (spirit stones do not count)");
                    SetNotes(notes + 2);
                    var budget = MapBuildPropertyEvent.GetBuildProperty(town);
                    c.AssertEqual(EstateEvent.BuildResult.Ok, EstateEvent.Buy(town, plot, Player), $"bought plot #{plot.Index} (prosperity {plot.Prosperity}) for {notes} banknotes ({cost:#,##0} stones)");
                    c.AssertEqual(2, SpiritNoteEvent.Notes(Player), $"{notes} banknotes paid");
                    c.AssertEqual(int.MaxValue / 2, Player.GetUnitMoney(), "no spirit stone paid");
                    c.AssertEqual(budget + notes * SpiritNoteEvent.NoteValue, MapBuildPropertyEvent.GetBuildProperty(town), "to the town budget");
                    c.Assert(EstateEvent.HasHouse(town, Player), "the player has a house in the town");
                    var other = e.Plots.FirstOrDefault(x => x.Level == 0 && x.OwnerId == null);
                    if (other != null)
                        c.AssertEqual(EstateEvent.BuildResult.HasHouse, EstateEvent.Buy(town, other, Player), "one house per town");
                })
                .Step("upgrade", c =>
                {
                    SetNotes((int)SpiritNoteEvent.NotesFor(EstateEvent.UpgradeCost(town, plot).total));
                    c.AssertEqual(EstateEvent.BuildResult.Ok, EstateEvent.Upgrade(town, plot, Player), "tiled house (banknotes)");
                    c.AssertEqual(0, SpiritNoteEvent.Notes(Player), "the banknotes of the upgrade paid");
                    SetNotes(100000);
                    c.AssertEqual(2, plot.Level, "level 2");
                    Player.SetUnitMoney(int.MaxValue / 2);
                    if (Player.GetGradeLvl() < EstateEvent.RequiredGrade(3))
                        c.AssertEqual(EstateEvent.BuildResult.GradeTooLow, EstateEvent.Upgrade(town, plot, Player), "a courtyard house needs grade 2");
                    else
                        c.Info($"grade {Player.GetGradeLvl()}: no grade gate to test");
                    var keys = TaoistLogEvent.Instance.Entries.Select(x => x.Key).ToList();
                    c.Assert(keys.Contains("log_estate_buy") && keys.Contains("log_estate_upgrade"), "Taoist log entries");
                })
                .Step("house tax", c =>
                {
                    SetNotes(0); // the monthly tax: banknotes first, then spirit stones
                    var tax = EstateEvent.HouseTax(town, plot);
                    c.Assert(tax > 0, $"house tax {tax:#,##0}/month");
                    Player.SetUnitMoney(0);
                    EstateEvent.MonthTown(town, 1);
                    c.AssertEqual(1, plot.Unpaid, "unpaid month counted");
                    Player.SetUnitMoney((int)tax + 50);
                    EstateEvent.MonthTown(town, 1);
                    c.AssertEqual(0, plot.Unpaid, "paid");
                    c.AssertEqual(50, Player.GetUnitMoney(), "the house tax paid from the money");
                })
                .Step("rest and enter the town", c =>
                {
                    Player.SetProperty<int>(UnitPropertyEnum.Hp, 1);
                    EstateEvent.Instance.OnMonthlyForEachWUnit(Player);
                    c.AssertEqual(Player.GetDynProperty(UnitDynPropertyEnum.HpMax).value, Player.GetDynProperty(UnitDynPropertyEnum.Hp).value, "a full rest in the town with a house");
                    MapBuildPropertyEvent.Instance.PayTaxTown.Remove(town.buildData.id);
                    Player.SetUnitMoney(100000);
                    c.Bag["before"] = Player.GetUnitMoney();
                    town.OpenBuild();
                })
                .Wait("town opened", c => g.ui.HasUI(UIType.Town), 10000)
                .Step("no entry tax", c =>
                {
                    c.AssertEqual((int)c.Bag["before"], Player.GetUnitMoney(), "no entry tax with a house");
                    c.Assert(!MapBuildPropertyEvent.Instance.PayTaxTown.Contains(town.buildData.id), "the town did not charge the entry tax");
                    if (!MapBuildPropertyEvent.IsTownGuardian(town, Player))
                        c.Assert(FindButtons(UIType.Town, GameTool.LS("estate_tax_free")).Any(), "the tax button reads 'free'");
                }, 2000)
                .Step("town map", c =>
                {
                    var btn = FindButtons(UIType.Town, GameTool.LS("estate_button")).FirstOrDefault();
                    if (!c.Assert(btn != null, "'Plots' button on the town window")) return;
                    btn.Item.onClick.Invoke();
                    c.Assert(g.ui.HasUI(UIType.TextInfoLong), "town map opened");
                    var texts = ScreenTexts();
                    c.Assert("MNRCO".All(d => texts.Contains(EstateEvent.DistrictName(d.ToString()))), "every district is named on the map");
                    c.Assert(texts.Any(x => x == string.Format(GameTool.LS("estate_palace_label"), EstateEvent.Get(town).PalaceLevel)), "the town master's palace is drawn");
                    c.Assert(texts.Any(x => x == EstateEvent.LegendText()), $"the price legend from the config: {EstateEvent.LegendText()}");
                    c.Assert(EstateEvent.GameSprite("TownCommon", "Build2005") != null, "game picture of the tiled house");
                    c.Assert(EstateEvent.GameSprite("CommonMapGrid", "cunzhuangmaowu1") != null, "game picture of the thatched hut");
                    // every picture of the config is found in the game
                    var sprites = EstateEvent.Conf.Houses.Select(x => x.Sprite).Concat(EstateEvent.Conf.Palace).Concat(EstateEvent.Conf.Districts.Values.SelectMany(x => x.Sprites)).Concat(EstateEvent.Conf.Map.Trees).Concat(EstateEvent.Conf.Map.People).Distinct().ToList();
                    var missing = sprites.Where(s => { var a = EstateConfig.SplitSprite(s); return EstateEvent.GameSprite(a[0], a[1]) == null; }).ToList();
                    c.Assert(missing.Count == 0, $"the {sprites.Count} pictures of estate_configs.json are found" + (missing.Count == 0 ? "" : ": missing " + string.Join(", ", missing)));
                }, 1500)
                .Step("taken back", c =>
                {
                    CloseAll(UIType.TextInfoLong);
                    Player.SetUnitMoney(0);
                    plot.Unpaid = EstateEvent.MaxUnpaid - 1;
                    EstateEvent.MonthTown(town, 1);
                    c.Assert(plot.OwnerId == null && !EstateEvent.HasHouse(town, Player), "the house is taken back after 6 unpaid months");
                    c.Assert(TaoistLogEvent.Instance.Entries.Any(x => x.Key == "log_estate_seized"), "Taoist log: seized");
                })
                .Step("restore", c =>
                {
                    CloseAll(UIType.TextInfoLong, UIType.Town);
                    EstateEvent.Instance.Towns[town.buildData.id] = JsonConvert.DeserializeObject<EstateEvent.TownEstate>((string)c.Bag["estate"]);
                    MapBuildPropertyEvent.AddBuildProperty(town, (long)c.Bag["budget"] - MapBuildPropertyEvent.GetBuildProperty(town));
                    MapBuildPropertyEvent.Instance.TaxRate[town.buildData.id] = (float)c.Bag["rate"];
                    Player.SetUnitMoney((int)c.Bag["money"]);
                    SetNotes((int)c.Bag["notes"]);
                    c.AssertEqual((long)c.Bag["budget"], MapBuildPropertyEvent.GetBuildProperty(town), "budget restored");
                });
        }
    }
}
