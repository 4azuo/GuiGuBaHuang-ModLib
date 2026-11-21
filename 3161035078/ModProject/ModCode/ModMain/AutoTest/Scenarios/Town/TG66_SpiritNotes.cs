using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-66 (SpiritNoteEvent): the bank (town storage with an account) has a "Spirit notes" button; its window changes
    /// spirit stones into notes and back, always for a fee (to the town budget); a price is paid with the notes of the
    /// bag first, the rest (or the change) in spirit stones, also a price above the int limit of the spirit stones.
    /// Money, notes, budget and account restored.
    /// </summary>
    public class TG66_SpiritNotes : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-66";

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
            return new Scenario { Id = "TG-66", Feature = "SpiritNoteEvent", Title = "Spirit notes: bank exchange and payments", Overlay = false }
                .Step("travel", c =>
                {
                    if (!c.Assert(SpiritNoteEvent.Instance != null, "SpiritNoteEvent loaded")) return;
                    if (!TravelTo(c, FindTown(t => Has(t, MapBuildSubType.TownStorage)))) return;
                    town = BagTown(c);
                    c.Bag["money"] = Player.GetUnitMoney();
                    c.Bag["notes"] = SpiritNoteEvent.Notes(Player);
                    c.Bag["budget"] = MapBuildPropertyEvent.GetBuildProperty(town);
                    c.Bag["account"] = BankAccountEvent.Instance.RegisterdTown.Contains(town.buildData.id);
                    if (!(bool)c.Bag["account"]) BankAccountEvent.Instance.RegisterdTown.Add(town.buildData.id);
                    OpenTownUI<UITownStorage>(UIType.TownStorage, ui => ui.InitData(town));
                }, 1500)
                .Step("bank button", c =>
                {
                    if (!c.Assert(g.ui.HasUI(UIType.TownStorage), "the bank (town storage) opened")) return;
                    var btn = FindButtons(UIType.TownStorage, GameTool.LS("note_button")).FirstOrDefault();
                    if (!c.Assert(btn != null, "'Spirit notes' button in the bank")) return;
                    btn.Item.onClick.Invoke();
                    c.Assert(g.ui.HasUI(UIType.TextInfoLong), "the exchange window opened");
                    c.Assert(ScreenTexts().Any(x => x == GameTool.LS("note_hint")), "the window tells the int limit of the spirit stones");
                }, 1500)
                .Step("exchange", c =>
                {
                    var fee = SpiritNoteEvent.FeeRate();
                    c.Assert(fee > 0, $"the bank always takes a fee ({fee:P2})");
                    Player.SetUnitMoney(1000000);
                    SetNotes(0);
                    var budget = MapBuildPropertyEvent.GetBuildProperty(town);
                    var cost = SpiritNoteEvent.BuyPrice(10, SpiritNoteEvent.NoteValue, fee);
                    c.Assert(SpiritNoteEvent.Buy(Player, town, 10), $"10 notes bought for {cost:#,##0}");
                    c.AssertEqual(10, SpiritNoteEvent.Notes(Player), "10 notes in the bag");
                    c.AssertEqual(1000000 - cost, (long)Player.GetUnitMoney(), "the stones paid, fee included");
                    c.AssertEqual(budget + cost - 10 * SpiritNoteEvent.NoteValue, MapBuildPropertyEvent.GetBuildProperty(town), "the fee goes to the town budget");
                    var money = Player.GetUnitMoney();
                    c.Assert(SpiritNoteEvent.Sell(Player, town, 4), "4 notes changed back");
                    c.AssertEqual(money + SpiritNoteEvent.SellPrice(4, SpiritNoteEvent.NoteValue, fee), (long)Player.GetUnitMoney(), "stones back, fee taken");
                    c.AssertEqual(6, SpiritNoteEvent.Notes(Player), "6 notes left");
                    Player.SetUnitMoney(int.MaxValue - 5000);
                    c.Assert(!SpiritNoteEvent.Sell(Player, town, 1), "no change back past the int limit of the stones");
                    c.AssertEqual(0, SpiritNoteEvent.MaxSell(Player), "nothing to change back near the limit");
                })
                .Step("payments", c =>
                {
                    // notes first, one more note and the change when the stones are short
                    SetNotes(3);
                    Player.SetUnitMoney(500);
                    c.Assert(SpiritNoteEvent.Pay(Player, 25000), "25,000 paid with 3 notes and 500 stones");
                    c.AssertEqual(0, SpiritNoteEvent.Notes(Player), "the 3 notes used");
                    c.AssertEqual(5500, Player.GetUnitMoney(), "the change back in stones");
                    c.Assert(!SpiritNoteEvent.Pay(Player, 100000), "not enough: nothing paid");
                    c.AssertEqual(5500, Player.GetUnitMoney(), "the stones untouched");
                    // a price above the int limit of the stones
                    SetNotes(300000);
                    c.Assert(SpiritNoteEvent.Pay(Player, 2500000000L), "2,500,000,000 paid with notes");
                    c.AssertEqual(50000, SpiritNoteEvent.Notes(Player), "250,000 notes used");
                })
                .Step("restore", c =>
                {
                    CloseAll(UIType.TextInfoLong, UIType.TownStorage);
                    SetNotes((int)c.Bag["notes"]);
                    Player.SetUnitMoney((int)c.Bag["money"]);
                    MapBuildPropertyEvent.AddBuildProperty(town, (long)c.Bag["budget"] - MapBuildPropertyEvent.GetBuildProperty(town));
                    if (!(bool)c.Bag["account"]) BankAccountEvent.Instance.RegisterdTown.Remove(town.buildData.id);
                    c.AssertEqual((int)c.Bag["notes"], SpiritNoteEvent.Notes(Player), "notes restored");
                });
        }
    }
}
