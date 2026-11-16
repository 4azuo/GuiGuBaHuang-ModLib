using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;
using E = MOD_nE7UL2.Mod.EncounterEvent.Encounter;

namespace MOD_nE7UL2.AutoTest.Scenarios.Story
{
    /// <summary>
    /// TG-55 (EncounterEvent): random encounters. An encounter drama opens with its options (one is clicked), the
    /// monthly handler opens one at 100 %, and every result is checked with fixed rolls: wounded cultivator (medicine:
    /// friendship + herb; rob: his money), chess (win: exp + stake; lose: stake), peddler (item / empty box / haggle),
    /// cave (materials / spirit stones / injury), spirit beast cub (herb -> luck; no herb), town festival (mood, cost to
    /// the town budget). Each result goes to the Taoist log.
    /// </summary>
    public class TG55_Encounters : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-55";

        private static int HerbId => g.conf.itemProps._allConfList.ToArray().First(x => x.IsHerbItem() != null && x.sale > 0).id;
        private static int Bag => Player.GetUnitProps().Sum(x => x.propsCount) - Player.GetUnitMoney();
        private static void Rich() { if (Player.GetUnitMoney() < EncounterEvent.Cost(20)) Player.AddUnitMoney(EncounterEvent.Cost(20)); }

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-55", Feature = "EncounterEvent", Title = "Random encounters: dramas, choices and rewards", Overlay = false }
                .Step("encounter drama", c =>
                {
                    if (!c.Assert(EncounterEvent.Instance != null, "EncounterEvent loaded")) return;
                    Rich();
                    EncounterEvent.Open(E.Chess);
                }, 1500)
                .Step("drama options", c =>
                {
                    var o1 = string.Format(GameTool.LS("enc_chess_o1"), EncounterEvent.ChessChance());
                    c.AssertEqual(string.Format(GameTool.LS("enc_chess"), EncounterEvent.Cost(3)), DramaText(), "the chess drama is shown");
                    var labels = DramaOptions().Select(x => x.Item2).ToList();
                    c.Assert(labels.Count == 2 && labels.Contains(o1) && labels.Contains(GameTool.LS("enc_chess_o2")), $"its 2 options: {string.Join(" | ", labels)}");
                    c.Assert(ChooseOption(GameTool.LS("enc_chess_o2")), "decline chosen");
                }, 1500)
                .Step("monthly", c =>
                {
                    DismissDramas();
                    var ev = EncounterEvent.Instance;
                    var cfg = SMLocalConfigsEvent.Instance.Configs; var rate = cfg.EncounterRate;
                    var count = ev.Count;
                    cfg.EncounterRate = 100f;
                    try { ev.OnMonthly(); } finally { cfg.EncounterRate = rate; }
                    c.AssertEqual(count + 1, ev.Count, $"monthly: an encounter at 100 % ({string.Join(", ", EncounterEvent.Possible())} possible here)");
                }, 1500)
                .Step("chess", c =>
                {
                    DismissDramas();
                    Rich();
                    var stake = EncounterEvent.Cost(3);
                    var m = Player.GetUnitMoney(); var exp = Player.GetExp();
                    c.AssertEqual("enc_chess_win", EncounterEvent.Resolve(E.Chess, 0, roll: 0f), "win at roll 0");
                    c.AssertEqual(m + stake, Player.GetUnitMoney(), $"win: +{stake} spirit stones");
                    c.Assert(Player.GetExp() > exp, $"win: exp {exp} -> {Player.GetExp()}");
                    m = Player.GetUnitMoney();
                    c.AssertEqual("enc_chess_lose", EncounterEvent.Resolve(E.Chess, 0, roll: 99f), "lose at roll 99");
                    c.AssertEqual(m - stake, Player.GetUnitMoney(), $"lose: -{stake} spirit stones");
                    c.AssertEqual(null, EncounterEvent.Resolve(E.Chess, 1), "declining does nothing");
                }, 1000)
                .Step("peddler", c =>
                {
                    DismissDramas();
                    Rich();
                    Rich();
                    var price = EncounterEvent.Cost(5);
                    var m = Player.GetUnitMoney(); var items = Bag;
                    c.AssertEqual("enc_peddler_item", EncounterEvent.Resolve(E.Peddler, 0, roll: 50f), "box with an item");
                    c.Assert(Player.GetUnitMoney() == m - price && Bag == items + 1, $"paid {price}, one more item");
                    m = Player.GetUnitMoney();
                    c.AssertEqual("enc_peddler_empty", EncounterEvent.Resolve(E.Peddler, 0, roll: 5f), "empty box");
                    c.AssertEqual(m - price, Player.GetUnitMoney(), "the empty box is paid too");
                    m = Player.GetUnitMoney();
                    c.AssertEqual("enc_peddler_leave", EncounterEvent.Resolve(E.Peddler, 1, roll: 70f), "haggling makes him leave");
                    c.AssertEqual(m, Player.GetUnitMoney(), "nothing paid");
                    Rich();
                    m = Player.GetUnitMoney();
                    items = Bag;
                    c.AssertEqual("enc_peddler_deal", EncounterEvent.Resolve(E.Peddler, 1, roll: 10f), "haggling gets a discount");
                    c.Assert(Player.GetUnitMoney() == m - price * 7 / 10 && Bag == items + 1, $"paid {price * 7 / 10} (30 % off), one more item");
                }, 1000)
                .Step("cave", c =>
                {
                    DismissDramas();
                    var items = Bag;
                    c.AssertEqual("enc_cave_items", EncounterEvent.Resolve(E.Cave, 0, roll: 5f), "materials");
                    c.AssertEqual(items + 2, Bag, "2 materials found (roll under 10)");
                    var m = Player.GetUnitMoney();
                    c.AssertEqual("enc_cave_stones", EncounterEvent.Resolve(E.Cave, 0, roll: 50f), "spirit stones");
                    c.AssertEqual(m + EncounterEvent.Cost(2), Player.GetUnitMoney(), "spirit stones found");
                    Player.SetProperty<int>(UnitPropertyEnum.Hp, Player.GetDynProperty(UnitDynPropertyEnum.HpMax).value);
                    var hp = Player.GetProperty<int>(UnitPropertyEnum.Hp);
                    c.AssertEqual("enc_cave_hurt", EncounterEvent.Resolve(E.Cave, 0, roll: 90f), "injury");
                    c.Assert(Player.GetProperty<int>(UnitPropertyEnum.Hp) < hp && Player.GetProperty<int>(UnitPropertyEnum.Hp) > 0, $"vitality {hp} -> {Player.GetProperty<int>(UnitPropertyEnum.Hp)}");
                }, 1000)
                .Step("cub", c =>
                {
                    DismissDramas();
                    foreach (var h in Player.GetUnitProps().Where(x => x.propsItem?.IsHerbItem() != null).ToArray()) Player.RemoveUnitProp(h.soleID);
                    c.AssertEqual("enc_cub_noherb", EncounterEvent.Resolve(E.Cub, 0, roll: 50f), "no herb to give");
                    Player.AddUnitProp(HerbId, 1);
                    var luck = Player.GetProperty<int>(UnitPropertyEnum.Luck);
                    c.AssertEqual("enc_cub_r", EncounterEvent.Resolve(E.Cub, 0, roll: 50f), "the cub is fed");
                    c.AssertEqual(luck + 1, Player.GetProperty<int>(UnitPropertyEnum.Luck), "+1 luck");
                    c.AssertEqual(0, Player.GetUnitPropCount(HerbId), "the herb is eaten");
                }, 1000)
                .Step("wounded", c =>
                {
                    DismissDramas();
                    Rich();
                    var npc = Npcs.FirstOrDefault(x => !HirePeopleEvent.IsHired(x));
                    if (!c.Assert(npc != null, "an NPC")) return;
                    var intim = npc.data.unitData.relationData.GetIntim(Player);
                    var items = Bag;
                    c.AssertEqual("enc_wounded_r1", EncounterEvent.Resolve(E.Wounded, 0, npc), "medicine");
                    c.Assert(npc.data.unitData.relationData.GetIntim(Player) > intim && Bag == items + 1, "friendship and a herb");
                    if (npc.GetUnitMoney() < 100) npc.AddUnitMoney(1000);
                    var nm = npc.GetUnitMoney(); var m = Player.GetUnitMoney();
                    c.AssertEqual("enc_wounded_r2", EncounterEvent.Resolve(E.Wounded, 1, npc), "robbed");
                    c.AssertEqual(m + nm * 15 / 100, Player.GetUnitMoney(), "15 % of his money taken");
                }, 1000)
                .Step("festival", c =>
                {
                    DismissDramas();
                    if (!TravelTo(c, FindTown(t => true))) return;
                    Rich();
                    Player.SetProperty<int>(UnitPropertyEnum.Mood, 10);
                    var budget = MapBuildPropertyEvent.GetBuildProperty(BagTown(c));
                    c.AssertEqual("enc_festival_r", EncounterEvent.Resolve(E.Festival, 0), "festival joined");
                    c.Assert(Player.GetProperty<int>(UnitPropertyEnum.Mood) > 10, $"mood 10 -> {Player.GetProperty<int>(UnitPropertyEnum.Mood)}");
                    c.AssertEqual(budget + EncounterEvent.Cost(), MapBuildPropertyEvent.GetBuildProperty(BagTown(c)), "the cost goes to the town budget");
                }, 1000)
                .Step("result drama and log", c =>
                {
                    c.Assert(g.ui.HasUI(UIType.DramaDialogue) || g.ui.HasUI(UIType.DramaDialogueBigTexture) || g.ui.HasUI(UIType.DramaBigTexture) || g.ui.HasUI(UIType.DramaFortuitous), "the result drama is shown");
                    var keys = TaoistLogEvent.Instance.Entries.Select(x => x.Key).ToList();
                    c.Assert(new[] { "enc_chess_win", "enc_peddler_item", "enc_cave_items", "enc_cub_r", "enc_wounded_r1", "enc_festival_r" }.All(keys.Contains), "every result is in the Taoist log");
                    DismissDramas();
                });
        }
    }
}
