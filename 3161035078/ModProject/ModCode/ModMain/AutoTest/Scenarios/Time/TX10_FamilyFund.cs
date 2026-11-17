using ModLib.Const;
using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Time
{
    /// <summary>
    /// TX-10 (FamilyFundEvent): the player marries and has a grown-up child and a young one; the monthly upkeep is paid
    /// (money, then the bank storage stones, then unpaid: the family cools down); a member brings spirit stones and an item
    /// to the bank storage, then takes the item back; the accounts window lists the members and the entries.
    /// Changes the save (restored by run-autotest.ps1).
    /// </summary>
    public class TX10_FamilyFund : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TX-10";

        private static int Stored(int propsId) => GameHelper.GetStorage().data.propData.allProps.ToArray().Where(x => x.propsID == propsId).Sum(x => x.propsCount);

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            WorldUnitBase mate = null, child = null;
            return new Scenario { Id = "TX-10", Feature = "FamilyFundEvent", Title = "Household upkeep, gifts and accounts", Overlay = false }
                .Step("household", c =>
                {
                    var F = FamilyEvent.Instance;
                    if (!c.Assert(F != null && FamilyFundEvent.Instance != null, "FamilyEvent and FamilyFundEvent loaded")) return;
                    mate = FamilyEvent.Unit(FamilyEvent.Spouse(Player));
                    // a living companion (one married in an earlier scenario may have died since)
                    if (mate == null || mate.isDie)
                    {
                        mate = Npcs.FirstOrDefault(x => !x.isDie && string.IsNullOrEmpty(FamilyEvent.Spouse(x)) && FamilyEvent.IsMan(x) != FamilyEvent.IsMan(Player) && !HirePeopleEvent.IsHired(x));
                        if (!c.Assert(mate != null, "a single NPC of the other sex")) return;
                        FamilyEvent.SetMarried(Player, mate);
                    }
                    var father = FamilyEvent.IsMan(Player) ? Player : mate;
                    var mother = FamilyEvent.IsMan(Player) ? mate : Player;
                    var grown = F.Birth(father, mother, GameHelper.GetGameTotalMonth() - FamilyEvent.ADULT_YEARS * 12);
                    F.Birth(father, mother, GameHelper.GetGameTotalMonth() - 24);
                    DismissDramas();
                    F.OnMonthly();
                    child = FamilyEvent.Unit(grown.UnitId);
                    var members = FamilyFundEvent.Members();
                    c.Assert(members.Any(x => x.GetUnitId() == mate.GetUnitId()) && child != null && members.Any(x => x.GetUnitId() == child.GetUnitId()), $"members: {string.Join(", ", members.Select(x => x.GetName()))}");
                    c.Assert(FamilyFundEvent.Minors().Count >= 1, $"children not grown up: {FamilyFundEvent.Minors().Count}");
                    c.Info($"monthly upkeep {FamilyFundEvent.MonthlyUpkeep()}");
                }, 1000)
                .Step("upkeep paid", c =>
                {
                    DismissDramas();
                    var I = FamilyFundEvent.Instance;
                    var upkeep = FamilyFundEvent.MonthlyUpkeep();
                    c.Assert(upkeep > 0, $"upkeep {upkeep}");
                    Player.SetUnitMoney((int)upkeep + 100);
                    I.PayUpkeep();
                    c.AssertEqual(100, Player.GetUnitMoney(), "paid from the money");
                    c.AssertEqual(upkeep, I.Ledger.Last().Amount, "ledger: upkeep");
                })
                .Step("upkeep from the storage, then unpaid", c =>
                {
                    var I = FamilyFundEvent.Instance;
                    var upkeep = FamilyFundEvent.MonthlyUpkeep();
                    var storage = GameHelper.GetStorage();
                    var stones = Stored(ModLibConst.MONEY_PROP_ID);
                    if (stones > 0) storage.data.propData.DelProps(ModLibConst.MONEY_PROP_ID, stones);
                    storage.data.propData.AddProps(ModLibConst.MONEY_PROP_ID, (int)(upkeep / 2));
                    Player.SetUnitMoney(0);
                    var intim = mate.data.unitData.relationData.GetIntim(Player.GetUnitId());
                    I.PayUpkeep();
                    c.AssertEqual(0, Stored(ModLibConst.MONEY_PROP_ID), "the storage stones paid half");
                    c.AssertEqual(FamilyFundEvent.Kind.Unpaid, I.Ledger.Last().Kind, "ledger: unpaid");
                    c.AssertEqual(upkeep - upkeep / 2, I.Ledger.Last().Amount, "unpaid amount");
                    var member = FamilyFundEvent.Members().Any(x => x.GetUnitId() == mate.GetUnitId());
                    c.Assert(mate.data.unitData.relationData.GetIntim(Player.GetUnitId()) < intim, $"the companion is upset: {intim} -> {mate.data.unitData.relationData.GetIntim(Player.GetUnitId())} (member {member}, spouse {FamilyEvent.Spouse(Player)} / {mate.GetUnitId()}, dead {mate.isDie}, player side {Player.data.unitData.relationData.GetIntim(mate.GetUnitId())})");
                    Player.SetUnitMoney(100000);
                })
                .Step("gifts and use", c =>
                {
                    var I = FamilyFundEvent.Instance;
                    var stones = Stored(ModLibConst.MONEY_PROP_ID);
                    c.AssertEqual("log_family_gift_stones", I.Gift(child, false), "gift: spirit stones");
                    c.Assert(Stored(ModLibConst.MONEY_PROP_ID) > stones, $"stones in the storage: {stones} -> {Stored(ModLibConst.MONEY_PROP_ID)}");
                    c.AssertEqual("log_family_gift_item", I.Gift(child, true), "gift: item");
                    var item = I.Ledger.Last().ItemId;
                    var count = Stored(item);
                    c.Assert(count > 0 && I.GiftedItems.ContainsKey(item), $"item {item} in the storage ({count})");
                    I.UseGifts(true);
                    c.AssertEqual(FamilyFundEvent.Kind.Used, I.Ledger.Last().Kind, "ledger: used");
                    c.AssertEqual(count - 1, Stored(item), "the item left the storage");
                    var keys = TaoistLogEvent.Instance.Entries.Select(x => x.Key).ToList();
                    c.Assert(new[] { "log_family_unpaid", "log_family_gift_stones", "log_family_gift_item", "log_family_used" }.All(keys.Contains), "Taoist log entries");
                })
                .Step("map button", c =>
                {
                    CloseAll(UIType.NPCSearch);
                    c.Assert(ScreenTexts().Any(x => x == GameTool.LS("fund_button")), "the Family button is on the map");
                    FamilyFundEvent.OpenAccounts();
                }, 1500)
                .Step("accounts window", c =>
                {
                    var ui = g.ui.GetUI<UINPCSearch>(UIType.NPCSearch);
                    c.Assert(ui != null && ui.units.Count == FamilyFundEvent.Members().Count, $"members listed: {ui?.units.Count}");
                    var texts = ScreenTexts();
                    c.Assert(texts.Any(x => x.Contains(child?.GetName() ?? "?")), "an entry names the child");
                    c.Assert(texts.Any(x => x.Contains(FamilyFundEvent.MonthlyUpkeep().ToString("#,##0"))), "the monthly upkeep is shown");
                }, 1000)
                .Step("close", c => CloseAll(UIType.NPCSearch));
        }
    }
}
