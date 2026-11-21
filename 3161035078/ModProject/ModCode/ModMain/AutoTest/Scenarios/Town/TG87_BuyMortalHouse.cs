using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-87 (EstateEvent, mortal houses): a click on a mortal house of a town where the player has no house opens a text
    /// window (UITextInfo) that tells which plot it is (number, place), what it is, the price to buy it back, with the
    /// button "Buy this house"; the player buys it with banknotes: the house is the player's, of the level of the mortal
    /// house, the town budget gets the price. A shop of the market cannot be bought. The house released after.
    /// </summary>
    public class TG87_BuyMortalHouse : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-87";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-87", Feature = "EstateEvent", Title = "Estate: buy back a mortal house (text window)", Overlay = false }
                .Step("a town without a house", c =>
                {
                    if (!TravelTo(c, FindTown(t => !EstateEvent.HasHouse(t, Player) && EstateEvent.Get(t) != null && EstateEvent.Get(t).Plots.Any(EstateEvent.IsMortalHome)))) return;
                    var town = BagTown(c);
                    var p = EstateEvent.Get(town).Plots.Where(EstateEvent.IsMortalHome).OrderBy(x => x.Level).ThenBy(x => EstateEvent.MortalPrice(town, x)).First();
                    c.Bag["plot"] = p;
                    c.Bag["budget"] = MapBuildPropertyEvent.GetBuildProperty(town);
                    c.Bag["level"] = p.Level;
                    var price = EstateEvent.MortalPrice(town, p);
                    c.Bag["price"] = price;
                    c.Info($"plot {p.Index + 1} ({p.X + 1}, {p.Y + 1}), {EstateEvent.HouseName(p.Level)}, price {price}");
                    var notes = SpiritNoteEvent.NotesFor(price) - SpiritNoteEvent.Notes(Player);
                    if (notes > 0) Player.AddUnitProp(SpiritNoteEvent.NOTE_ID, (int)notes);
                    c.Bag["notes"] = SpiritNoteEvent.Notes(Player);
                    EstateEvent.ClickPlot(town, p);
                }, 1500)
                .Step("the window of the plot", c =>
                {
                    var p = (EstateEvent.Plot)c.Bag["plot"];
                    c.Assert(g.ui.HasUI(UIType.TextInfo), "a text window (UITextInfo), not a message box");
                    c.Assert(InfoDialog.LastText != null && InfoDialog.LastText.Contains(EstateEvent.PlotWhere(p)), $"it tells which plot: {EstateEvent.PlotWhere(p)}");
                    var label = string.Format(GameTool.LS("estate_mortal_buy"), SpiritNoteEvent.NotesFor((long)c.Bag["price"]));
                    c.Bag["btn"] = FindButtons(UIType.TextInfo, label).FirstOrDefault();
                    c.Assert(c.Bag["btn"] != null, $"the button '{label}'");
                }, 1500)
                .Step("buy this house", c => (c.Bag["btn"] as ModLib.Object.UIItemButton)?.Item.onClick.Invoke(), 1500)
                .Step("bought", c =>
                {
                    var town = BagTown(c);
                    var p = (EstateEvent.Plot)c.Bag["plot"];
                    c.AssertEqual(Player.GetUnitId(), p.OwnerId, "the house is the player's");
                    c.AssertEqual((int)c.Bag["level"], p.Level, "of the level of the mortal house");
                    c.Assert(MapBuildPropertyEvent.GetBuildProperty(town) > (long)c.Bag["budget"], "the town budget got the price");
                    c.Assert(SpiritNoteEvent.Notes(Player) < (int)c.Bag["notes"], "paid with banknotes");
                    c.Assert(TaoistLogEvent.Instance.Entries.Any(x => x.Key == "log_estate_buy_mortal"), "Taoist log");
                    var shop = EstateEvent.Get(town).Plots.FirstOrDefault(x => x.OwnerId == null && x.Level > 0 && EstateEvent.DistrictOf(x.District).Code == 'M');
                    if (shop != null) c.AssertEqual(EstateEvent.BuildResult.NotFree, EstateEvent.BuyMortal(town, shop, Player), "a shop of the market cannot be bought");
                }, 1500)
                .Step("restore", c =>
                {
                    CloseAll(UIType.TextInfo, UIType.TextInfoLong);
                    if (c.Bag.ContainsKey("plot"))
                    {
                        var p = (EstateEvent.Plot)c.Bag["plot"];
                        if (p.OwnerId == Player.GetUnitId()) { p.OwnerId = null; p.Level = (int)c.Bag["level"]; }
                    }
                });
        }
    }
}
