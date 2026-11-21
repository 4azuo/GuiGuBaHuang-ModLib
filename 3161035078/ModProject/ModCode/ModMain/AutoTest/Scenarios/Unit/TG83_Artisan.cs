using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Unit
{
    /// <summary>
    /// TG-83 (ArtisanEvent): the alchemy window of the game open, a pill put in the bag, the window closed: the pill counts
    /// as crafted (craft exp); the first rank by itself, the craft value +15; a masterpiece (cultivation exp, its maker in
    /// the list); a furnace accident two levels above; the exam (outside a city: refused; an NPC passes); the window from
    /// the cultivation screen. Artisan data, craft values, exp and bag restored.
    /// </summary>
    public class TG83_Artisan : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-83";

        private static ConfItemPropsItem Pill(int level) =>
            g.conf.itemProps._allConfList.ToArray().Where(x => x.IsPill() != null && x.level == level).OrderBy(x => x.worth).FirstOrDefault();

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-83", Feature = "ArtisanEvent", Title = "Artisan Dao: crafting, ranks, masterpieces, exam", Overlay = false }
                .Step("alchemy window", c =>
                {
                    if (!c.Assert(ArtisanEvent.Enabled, "ArtisanEvent loaded")) return;
                    var id = Player.GetUnitId();
                    c.Bag["data"] = ArtisanEvent.Instance.Artisans.TryGetValue(id, out var old) ? Newtonsoft.Json.JsonConvert.SerializeObject(old) : null;
                    c.Bag["value"] = Player.GetProperty<int>(UnitPropertyEnum.RefineElixir) - (old != null && old.TryGetValue(1, out var a) ? a.Given : 0);
                    c.Bag["exp"] = Player.GetExp();
                    c.Bag["mp"] = ArtisanEvent.Instance.Masterpieces.Count;
                    if (old != null) { foreach (var x in old.Values) { x.Rank = 0; } foreach (var cr in ArtisanEvent.CRAFTS) ArtisanEvent.Sync(Player, cr); }
                    ArtisanEvent.Instance.Artisans.Remove(id);
                    g.ui.OpenUI(UIType.RefineElixir);
                }, 1500)
                .Step("a pill made", c =>
                {
                    c.Assert(g.ui.HasUI(UIType.RefineElixir), "the alchemy window of the game is open");
                    var pill = Pill(ArtisanEvent.UsualLevel(Player.GetGradeLvl()));
                    if (!c.Assert(pill != null, "a pill of the usual level")) return;
                    c.Bag["pill"] = pill.id;
                    Player.AddUnitProp(pill.id, 16);
                    g.ui.CloseUI(UIType.RefineElixir);
                }, 1000)
                .Step("craft exp", c =>
                {
                    var d = ArtisanEvent.Get(Player, ArtisanEvent.Craft.Alchemy);
                    c.Info(ArtisanEvent.LastCrafted ?? "-");
                    if (!c.Assert(d != null, "the pill counted for alchemy")) return;
                    var lvl = g.conf.itemProps.GetItem((int)c.Bag["pill"]).level;
                    var exp = ArtisanEvent.CraftExp(lvl, 16);
                    c.Assert(d.Rank == 1 || d.Exp == exp, $"craft exp {exp} (rank {d.Rank}, exp {d.Exp})");
                    // up to the first rank
                    ArtisanEvent.Crafted(Player, ArtisanEvent.Craft.Alchemy, (int)c.Bag["pill"], 100, 100f);
                    c.AssertEqual(1, d.Rank, "Apprentice of alchemy");
                    c.AssertEqual((int)c.Bag["value"] + ArtisanEvent.RANK_VALUE, Player.GetProperty<int>(UnitPropertyEnum.RefineElixir), "alchemy value +15");
                    c.Assert(TaoistLogEvent.Instance.Entries.Any(x => x.Key == "log_artisan_rank"), "Taoist log: rank");
                })
                .Step("masterpiece and accident", c =>
                {
                    var usual = ArtisanEvent.UsualLevel(Player.GetGradeLvl());
                    var high = Pill(usual + 2) ?? Pill(usual + 1);
                    if (!c.Assert(high != null, "a pill above the usual level")) return;
                    // at the start of the phase: the exp of the masterpiece is not cut at the top of the phase
                    Player.ClearExp();
                    var exp0 = Player.GetExp();
                    var hp0 = Player.GetDynProperty(UnitDynPropertyEnum.Hp).value;
                    ArtisanEvent.Crafted(Player, ArtisanEvent.Craft.Alchemy, high.id, 1, 0f);
                    var d = ArtisanEvent.Get(Player, ArtisanEvent.Craft.Alchemy);
                    c.AssertEqual(1, d.Masterpieces, $"a masterpiece (level {high.level}, usual {usual})");
                    c.Assert(Player.GetExp() > exp0, $"cultivation exp +{Player.GetExp() - exp0}");
                    c.AssertEqual(Player.GetName(), ArtisanEvent.Instance.Masterpieces.Last().Maker, "the maker's name kept");
                    if (ArtisanEvent.CanBlowUp(high.level, Player.GetGradeLvl()))
                        c.Assert(Player.GetDynProperty(UnitDynPropertyEnum.Hp).value < hp0 || hp0 <= 1, $"furnace accident: {ArtisanEvent.LastAccident} HP lost");
                })
                .Step("exam", c =>
                {
                    var d = ArtisanEvent.Get(Player, ArtisanEvent.Craft.Alchemy);
                    d.Exp = ArtisanEvent.NeedExp(d.Rank);
                    c.AssertEqual(ArtisanEvent.ExamResult.NotInCity, Player.GetMapBuild<MapBuildTown>()?.IsCity() == true ? ArtisanEvent.ExamResult.NotInCity : ArtisanEvent.Exam(Player, ArtisanEvent.Craft.Alchemy, 0f), "outside a city: no exam");
                    TravelTo(c, FindTown(t => t.IsCity()));
                    var town = Player.GetMapBuild<MapBuildTown>();
                    if (town == null || !town.IsCity())
                        c.AssertEqual(ArtisanEvent.ExamResult.NotInCity, ArtisanEvent.Exam(Player, ArtisanEvent.Craft.Alchemy, 0f), "outside a city: no exam");
                    else
                    {
                        Player.AddUnitMoney(ArtisanEvent.ExamFee(d.Rank));
                        c.AssertEqual(ArtisanEvent.ExamResult.Passed, ArtisanEvent.Exam(Player, ArtisanEvent.Craft.Alchemy, 0f), "in a city: passed");
                        c.AssertEqual(2, d.Rank, "Craftsman");
                    }
                    var npc = ModMain.Store.WUnits.FirstOrDefault(x => x != null && !x.isDie && !x.IsPlayer());
                    if (npc != null)
                    {
                        c.Bag["npc"] = npc.GetUnitId();
                        c.Bag["npcData"] = ArtisanEvent.Instance.Artisans.TryGetValue(npc.GetUnitId(), out var nd) ? Newtonsoft.Json.JsonConvert.SerializeObject(nd) : null;
                        var n = ArtisanEvent.Get(npc, ArtisanEvent.Craft.Forge, true);
                        n.Rank = 1; n.Exp = ArtisanEvent.NeedExp(1); n.LastExamYear = -1;
                        c.AssertEqual(ArtisanEvent.ExamResult.Passed, ArtisanEvent.Exam(npc, ArtisanEvent.Craft.Forge, 0f), $"{npc.GetName()} passed the forging exam");
                    }
                })
                .Step("commission and apprentice", c =>
                {
                    // a commission for the pill made: delivered from the bag
                    var cm = ArtisanEvent.NewCommission(Player);
                    if (c.Assert(cm != null, "an NPC commissions an item"))
                    {
                        cm.Craft = (int)ArtisanEvent.Craft.Alchemy;
                        cm.Level = g.conf.itemProps.GetItem((int)c.Bag["pill"]).level;
                        var money = Player.GetUnitMoney();
                        c.Assert(ArtisanEvent.CommissionItem(Player, cm) != null, "a pill made by the player fits");
                        c.Assert(ArtisanEvent.Deliver(Player, cm), "delivered");
                        c.AssertEqual(money + cm.Reward, Player.GetUnitMoney(), $"reward {cm.Reward}");
                        c.Assert(!ArtisanEvent.Instance.Commissions.Contains(cm), "the commission closed");
                    }
                    // an apprentice (rank 3: Master)
                    var d = ArtisanEvent.Get(Player, ArtisanEvent.Craft.Alchemy);
                    d.Rank = 3; ArtisanEvent.Sync(Player, ArtisanEvent.Craft.Alchemy);
                    var npc = ArtisanEvent.ApprenticeCandidate(Player) ?? ModMain.Store.WUnits.FirstOrDefault(x => x != null && !x.isDie && !x.IsPlayer());
                    c.Bag["apprenticeCount"] = ArtisanEvent.Instance.Apprentices.Count;
                    var ap = ArtisanEvent.TakeApprentice(Player, ArtisanEvent.Craft.Alchemy, npc);
                    if (c.Assert(ap != null, $"{npc?.GetName()} became the apprentice"))
                    {
                        var gift = ArtisanEvent.ApprenticeGift(Player, npc, ArtisanEvent.Craft.Alchemy);
                        c.Assert(gift != null, $"the apprentice brought {gift}");
                        c.Assert(ArtisanEvent.TakeApprentice(Player, ArtisanEvent.Craft.Alchemy, npc) == null, "a Master takes only one apprentice");
                    }
                    c.AssertEqual(6f, ArtisanEvent.SellRate(Player), "a Master sells +6 % at the market");
                })
                .Step("gather on the map", c =>
                {
                    DramaHelper.DismissDramas();
                    CloseAll(UIType.TextInfoLong, UIType.TextInfo);
                    var herb = g.conf.itemProps._allConfList.ToArray().FirstOrDefault(x => x.IsHerbItem() != null && x.level == 1);
                    if (!c.Assert(herb != null, "an herb of level 1")) return;
                    c.Assert(ArtisanEvent.GatherCraft(herb) == ArtisanEvent.Craft.Herbal, "an herb is gathered with the Herbs craft");
                    // the game's gathering lowers the herbs of the cell: the same here
                    var gd = g.data.grid.GetGridData(Player.GetUnitPos());
                    c.Bag["cellHerbs"] = gd.medicinal;
                    gd.SetMedicinalValue(System.Math.Max(10, gd.medicinal));
                    ArtisanEvent.RefreshBag();
                    c.Bag["herb"] = herb.id;
                    Player.AddUnitProp(herb.id, 4);
                    gd.SetMedicinalValue(gd.medicinal - 1);
                }, 2500)
                .Step("herbs counted", c =>
                {
                    var d = ArtisanEvent.Get(Player, ArtisanEvent.Craft.Herbal);
                    c.Info(ArtisanEvent.LastCrafted ?? "-");
                    c.Assert(d != null && (d.Exp > 0 || d.Rank > 0), $"herbs found on the map give herbs exp ({d?.Exp})");
                    // cost of the check made every second on the map: the bag read and compared
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    var bag0 = ArtisanEvent.Bag(Player);
                    for (var i = 0; i < 200; i++) ArtisanEvent.Gained(bag0, ArtisanEvent.Bag(Player));
                    sw.Stop();
                    c.Info($"gathering check: {sw.Elapsed.TotalMilliseconds / 200:0.000} ms a second ({Player.GetUnitProps().Count} stacks in the bag)");
                    // a gift of the mod is not gathered
                    var exp = d?.Exp ?? 0;
                    EncounterEvent.GiveItem(x => x.IsHerbItem() != null, 1);
                    c.Bag["herbExp"] = exp;
                }, 2500)
                .Step("a gift is not gathered", c =>
                {
                    var d = ArtisanEvent.Get(Player, ArtisanEvent.Craft.Herbal);
                    c.AssertEqual((long)c.Bag["herbExp"], d?.Exp ?? 0, "an herb given by an encounter is not counted");
                    g.data.grid.GetGridData(Player.GetUnitPos()).SetMedicinalValue((int)c.Bag["cellHerbs"]);
                })
                .Step("return to simplicity", c =>
                {
                    // two crafts return: two destinies that stack, +50 to each craft only
                    var luck0 = Player.GetProperty<int>(UnitPropertyEnum.Luck);
                    var dynLuck0 = Player.GetDynProperty(UnitDynPropertyEnum.Luck).value;
                    var alch0 = Player.GetDynProperty(UnitDynPropertyEnum.RefineElixir).value;
                    var herb0 = Player.GetDynProperty(UnitDynPropertyEnum.Herbal).value;
                    var weapon0 = Player.GetDynProperty(UnitDynPropertyEnum.RefineWeapon).value;
                    foreach (var cr in new[] { ArtisanEvent.Craft.Alchemy, ArtisanEvent.Craft.Herbal })
                    {
                        var d = ArtisanEvent.Get(Player, cr, true);
                        d.Rank = 5; d.Exp = 0;
                        d.Essence = ArtisanEvent.EssenceStep * ArtisanEvent.RETURN_STEPS - 1;
                        ArtisanEvent.Crafted(Player, cr, cr == ArtisanEvent.Craft.Herbal ? (int)c.Bag["herb"] : (int)c.Bag["pill"], 100, 100f);
                        c.Assert(ArtisanEvent.IsReturned(Player, cr), $"{cr} returned to simplicity");
                        c.Assert(Player.data.unitData.propertyData.addLuck.ToArray().Any(x => x.id == ArtisanEvent.ReturnDestiny(cr)), $"destiny {ArtisanEvent.ReturnDestiny(cr)} given");
                    }
                    c.Bag["returned"] = true;
                    c.Info($"Luck {luck0} -> {Player.GetProperty<int>(UnitPropertyEnum.Luck)}, alchemy {alch0} -> {Player.GetDynProperty(UnitDynPropertyEnum.RefineElixir).value}, herbs {herb0} -> {Player.GetDynProperty(UnitDynPropertyEnum.Herbal).value}, forging {weapon0} -> {Player.GetDynProperty(UnitDynPropertyEnum.RefineWeapon).value}");
                    c.Assert(Player.GetDynProperty(UnitDynPropertyEnum.RefineElixir).value >= alch0 + ArtisanEvent.RETURN_VALUE, "alchemy +50 (its destiny)");
                    c.Assert(Player.GetDynProperty(UnitDynPropertyEnum.Herbal).value >= herb0 + ArtisanEvent.RETURN_VALUE, "herbs +50 (its destiny)");
                    c.AssertEqual(weapon0, Player.GetDynProperty(UnitDynPropertyEnum.RefineWeapon).value, "forging unchanged (no return)");
                    c.AssertEqual(2, ArtisanEvent.ReturnedCrafts(Player).Count, "two returns stack");
                    c.AssertEqual(dynLuck0 + 2 * ArtisanEvent.RETURN_LUCK, Player.GetDynProperty(UnitDynPropertyEnum.Luck).value, "Luck +3 a destiny (dynamic value)");
                    c.Assert(TaoistLogEvent.Instance.Entries.Count(x => x.Key == "log_artisan_return") >= 2, "Taoist log: two returns");
                }, 2500)
                .Step("cultivation screen", c =>
                {
                    DramaHelper.DismissDramas();
                    var ui = g.ui.GetUI<UIMapMain>(UIType.MapMain);
                    (ui.playerInfo.btnFateFeature ?? ui.playerInfo.btnFateFeature_En).onClick.Invoke();
                }, 1500)
                .Step("artisan window", c =>
                {
                    var btn = FindButtons(UIType.FateFeature, GameTool.LS("artisan_button")).FirstOrDefault();
                    if (c.Assert(btn != null, "'Artisan Dao' button on the cultivation screen")) btn.Item.onClick.Invoke();
                    c.Assert(g.ui.HasUI(UIType.TextInfoLong), "artisan window opened");
                }, 1500)
                .Step("restore", c =>
                {
                    CloseAll(UIType.TextInfoLong, UIType.FateFeature);
                    if (c.Bag.ContainsKey("returned"))
                        foreach (var cr in ArtisanEvent.CRAFTS)
                            if (ArtisanEvent.IsReturned(Player, cr)) Player.DelLuck(ArtisanEvent.ReturnDestiny(cr));
                    if (c.Bag.ContainsKey("apprenticeCount"))
                    {
                        var n0 = (int)c.Bag["apprenticeCount"];
                        if (ArtisanEvent.Instance.Apprentices.Count > n0) ArtisanEvent.Instance.Apprentices.RemoveRange(n0, ArtisanEvent.Instance.Apprentices.Count - n0);
                    }
                    var id = Player.GetUnitId();
                    foreach (var cr in ArtisanEvent.CRAFTS) { var d = ArtisanEvent.Get(Player, cr); if (d != null) { d.Rank = 0; d.Essence = 0; } ArtisanEvent.Sync(Player, cr); }
                    ArtisanEvent.Instance.Artisans.Remove(id);
                    if (c.Bag["data"] != null)
                    {
                        var old = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<int, ArtisanEvent.CraftData>>((string)c.Bag["data"]);
                        foreach (var x in old.Values) x.Given = 0;
                        ArtisanEvent.Instance.Artisans[id] = old;
                        foreach (var cr in ArtisanEvent.CRAFTS) ArtisanEvent.Sync(Player, cr);
                    }
                    if (c.Bag.ContainsKey("npc"))
                    {
                        var npc = g.world.unit.GetUnit((string)c.Bag["npc"]);
                        var n = ArtisanEvent.Get(npc, ArtisanEvent.Craft.Forge);
                        if (n != null) { n.Rank = 0; ArtisanEvent.Sync(npc, ArtisanEvent.Craft.Forge); }
                        ArtisanEvent.Instance.Artisans.Remove((string)c.Bag["npc"]);
                        if (c.Bag["npcData"] != null)
                            ArtisanEvent.Instance.Artisans[(string)c.Bag["npc"]] = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<int, ArtisanEvent.CraftData>>((string)c.Bag["npcData"]);
                    }
                    var mp = (int)c.Bag["mp"];
                    if (ArtisanEvent.Instance.Masterpieces.Count > mp) ArtisanEvent.Instance.Masterpieces.RemoveRange(mp, ArtisanEvent.Instance.Masterpieces.Count - mp);
                    Player.ClearExp();
                    Player.AddExp((int)c.Bag["exp"]);
                    c.AssertEqual((int)c.Bag["value"] + (ArtisanEvent.Get(Player, ArtisanEvent.Craft.Alchemy)?.Given ?? 0), Player.GetProperty<int>(UnitPropertyEnum.RefineElixir), "alchemy value restored");
                });
        }
    }
}
