using ModLib.Const;
using ModLib.Helper;
using ModLib.Mod;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-45 (RealMarketEvent2): buying from a shop. The quantity dialog shows the town buy rate and its unit
    /// price is base x rate; confirming adds the paid price to the town budget; a town master pays less.
    /// Replaces the manual TM-06.
    /// </summary>
    public class TG45_ShopBuy : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-45";

        private const int BASE_PRICE = 1000;

        private static UIPropSelectCount Dialog => g.ui.GetUI<UIPropSelectCount>(UIType.PropSelectCount);

        private static void OpenDialog(AutoTestContext c)
        {
            CloseAll(UIType.PropSelectCount);
            var dlg = g.ui.OpenUISafe<UIPropSelectCount>(UIType.PropSelectCount);
            dlg.InitData(BASE_PRICE, 5, ActionHelper.TracedIl2Action<int, int>((n, x) => c.Bag["bought"] = n), ModLibConst.MONEY_PROP_ID);
        }

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-45", Feature = "RealMarketEvent2", Title = "Buy from a shop: price rate, town budget, master discount", Overlay = false }
                .Step("travel", c => TravelTo(c, FindTown(t => Has(t, MapBuildSubType.TownMarketPill) && !MapBuildPropertyEvent.IsTownGuardian(t, Player))))
                .Step("open shop", c => OpenTownUI<UITownMarketBuy>(UIType.TownMarketBuy, ui => ui.InitData(BagTown(c))))
                .Step("quantity dialog", c =>
                {
                    if (!c.Assert(g.ui.HasUI(UIType.TownMarketBuy), "shop opened")) return;
                    c.Bag["rate"] = RealMarketEvent2.GetBuyRate(BagTown(c), Player);
                    OpenDialog(c);
                })
                .Step("unit price", c =>
                {
                    if (!c.Assert(Dialog != null, "quantity dialog opened")) return;
                    var rate = (float)c.Bag["rate"];
                    c.AssertEqual((BASE_PRICE * rate / 100f).Parse<int>(), Dialog.oneCost, $"unit price = {BASE_PRICE} x buy rate {rate:0.00}%");
                    c.Assert(FindTexts(Dialog, "Price rate:").Any(), $"rate text in the dialog: {FindTexts(Dialog, "Price rate:").FirstOrDefault()}");
                }, 1500)
                .Step("buy 2", c =>
                {
                    if (Dialog == null) { c.Skip("no dialog"); return; }
                    var town = BagTown(c);
                    var budget = MapBuildPropertyEvent.GetBuildProperty(town);
                    var paid = Dialog.oneCost * 2;
                    if (Player.GetUnitMoney() < paid * 2) Player.AddUnitMoney(paid * 2);
                    Dialog.iptNum.text = "2";
                    Dialog.btnOK.onClick.Invoke();
                    c.AssertEqual(budget + paid, MapBuildPropertyEvent.GetBuildProperty(town), $"town budget + paid price {paid}");
                    c.Info($"game buy callback: {(c.Bag.ContainsKey("bought") ? c.Bag["bought"] : "not called")}");
                }, 1500)
                .Step("become master", c =>
                {
                    CloseAll(UIType.PropSelectCount);
                    var town = BagTown(c);
                    new TrainerEvent { Parent = ModMain.ModObj }.BecomeTownMaster();
                    if (!c.Assert(MapBuildPropertyEvent.IsTownMaster(town, Player), $"player is master of {town.name}")) return;
                    var rate = RealMarketEvent2.GetBuyRate(town, Player);
                    c.AssertEqual((float)c.Bag["rate"] + RealMarketEvent2.TownMReduceBuyRate, rate, $"master buy rate {(float)c.Bag["rate"]:0.00}% -> {rate:0.00}%");
                    c.Bag["rate"] = rate;
                    OpenDialog(c);
                })
                .Step("discounted price", c =>
                {
                    if (!c.Assert(Dialog != null, "quantity dialog opened")) return;
                    c.AssertEqual((BASE_PRICE * (float)c.Bag["rate"] / 100f).Parse<int>(), Dialog.oneCost, "unit price with the master discount");
                }, 1500)
                .Step("close", c =>
                {
                    CloseAll(UIType.PropSelectCount, UIType.TownMarketBuy);
                    ClickButton(CONFIRM);
                    c.Assert(!g.ui.HasUI(UIType.TownMarketBuy), "shop closed");
                });
        }
    }
}
