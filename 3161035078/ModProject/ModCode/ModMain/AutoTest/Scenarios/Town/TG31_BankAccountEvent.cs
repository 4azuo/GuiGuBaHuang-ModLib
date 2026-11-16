using MOD_nE7UL2.Mod;
using ModLib.Helper;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-31 (BankAccountEvent): without an account the town storage offers "Open account (cost)"; opening it moves
    /// the fee from the player to the town budget and registers the account.
    /// </summary>
    public class TG31_BankAccountEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-31";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-31", Feature = "BankAccountEvent", Title = "Town storage: open a bank account", Overlay = false }
                .Step("travel", c => TravelTo(c, FindTown(t => Has(t, MapBuildSubType.TownStorage) && !BankAccountEvent.Instance.RegisterdTown.Contains(t.buildData.id))))
                .Step("open storage", c =>
                {
                    var town = CurrentTown;
                    if (town == null) { c.Skip("player is not in a town"); return; }
                    c.Bag["registered"] = BankAccountEvent.Instance.RegisterdTown.Contains(town.buildData.id);
                    c.Info($"account in {town.name}: {c.Bag["registered"]}, fee {BankAccountEvent.Cost(town.gridData.areaBaseID)}");
                    OpenTownUI<UITownStorage>(UIType.TownStorage, ui => ui.InitData(town));
                })
                .Step("account button", c =>
                {
                    var town = CurrentTown;
                    if (town == null || !g.ui.HasUI(UIType.TownStorage)) { c.Skip("storage not opened"); return; }
                    var ui = g.ui.GetUI<UITownStorage>(UIType.TownStorage);
                    var btn = FindGameButton(ui, "Open account");
                    if ((bool)c.Bag["registered"] || MapBuildPropertyEvent.IsTownGuardian(town, Player))
                    {
                        c.Assert(btn == null, "account exists (or guardian): no 'Open account' button");
                        return;
                    }
                    if (!c.Assert(btn != null, "'Open account (fee)' button replaces the storage button")) return;
                    var cost = BankAccountEvent.Cost(town.gridData.areaBaseID);
                    if (Player.GetUnitMoney() < cost)
                        Player.AddUnitMoney(cost);   // test data: enough money to open the account
                    c.Bag["money"] = Player.GetUnitMoney();
                    c.Bag["budget"] = MapBuildPropertyEvent.GetBuildProperty(town);
                    c.Bag["cost"] = cost;
                    btn.onClick.Invoke();
                })
                .Step("account opened", c =>
                {
                    var town = CurrentTown;
                    if (town == null || !c.Bag.ContainsKey("cost")) { c.Skip("no account to open"); return; }
                    var cost = (int)c.Bag["cost"];
                    c.Assert(BankAccountEvent.Instance.RegisterdTown.Contains(town.buildData.id), "account registered");
                    c.AssertEqual((int)c.Bag["money"] - cost, Player.GetUnitMoney(), "player paid the fee");
                    c.AssertEqual((long)c.Bag["budget"] + cost, MapBuildPropertyEvent.GetBuildProperty(town), "town budget received the fee");
                    c.Assert(!g.ui.HasUI(UIType.TownStorage), "storage closed after opening the account");
                })
                .Step("storage with account", c =>
                {
                    var town = CurrentTown;
                    if (town == null) { c.Skip("player is not in a town"); return; }
                    var ui = OpenTownUI<UITownStorage>(UIType.TownStorage, x => x.InitData(town));
                    c.Assert(FindGameButton(ui, "Open account") == null, "no 'Open account' button anymore");
                })
                .Step("close", c => CloseAll(UIType.TownStorageProps, UIType.TownStorage));
        }
    }
}
