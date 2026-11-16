using ModLib.Const;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-48 (CustomRefineEvent): full custom refine at a small town forge. Refine button -> item selector ->
    /// spirit stone selector -> OK: refine exp/level of the item increase, the stones are paid to the town budget,
    /// a result message is shown, and the item has refine adjustments. Replaces the manual TM-13.
    /// </summary>
    public class TG48_CustomRefine : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-48";

        private const int STONES = 5000;

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            DataProps.PropsData item = null;
            return new Scenario { Id = "TG-48", Feature = "CustomRefineEvent", Title = "Custom refine at a small town forge", Overlay = false }
                .Step("travel", c => TravelTo(c, FindTown(t => Has(t, MapBuildSubType.TownFactory) && !t.buildTownData.isMainTown)))
                .Step("refinable item", c =>
                {
                    var conf = g.conf.itemProps._allConfList.ToArray().FirstOrDefault(x => x.IsRing() != null || x.IsOutfit() != null);
                    if (!c.Assert(conf != null, "a ring/outfit config exists")) return;
                    Player.AddUnitProp(conf.id, 1);
                    item = Player.GetUnitProps(conf.id).LastOrDefault();
                    c.Assert(item != null && CustomRefineEvent.IsRefinableItem(item), $"refinable item {item?.propsInfoBase?.name} in the bag");
                    if (Player.GetUnitMoney() < STONES * 2) Player.AddUnitMoney(STONES * 2);
                })
                .Step("open forge", c => OpenTownUI<UITownFactoryShapeOption>(UIType.TownFactoryShapeOption, ui => ui.InitData(BagTown(c))))
                .Step("refine button", c =>
                {
                    var ui = g.ui.GetUI<UITownFactoryShapeOption>(UIType.TownFactoryShapeOption);
                    if (!c.Assert(ui != null, "forge opened")) return;
                    var label = ui.btnUpgrade.GetComponentInChildren<UnityEngine.UI.Text>(true)?.text;
                    var custom = ui.GetComponentsInChildren<UnityEngine.UI.Button>()
                        .FirstOrDefault(b => b.gameObject.activeInHierarchy && b.name.StartsWith("CompHelper:") && b.GetComponentInChildren<UnityEngine.UI.Text>()?.text == label);
                    if (!c.Assert(custom != null, "custom refine button")) return;
                    custom.onClick.Invoke();
                    c.AssertEqual(GameTool.LS("other500020001"), g.ui.GetUI<UIPropSelect>(UIType.PropSelect)?.textTitle1?.text, "item selector opened");
                }, 1000)
                .Step("select item", c =>
                {
                    var sel = g.ui.GetUI<UIPropSelect>(UIType.PropSelect);
                    if (sel == null || item == null) { c.Skip("no selector/item"); return; }
                    c.Assert(sel.allItems.allProps.ToArray().Any(x => x.soleID == item.soleID), "the item is listed");
                    sel.onCustomSelectCall.Invoke(item);
                    sel.btnOK.onClick.Invoke();
                }, 1000)
                .Step("select stones", c =>
                {
                    var sel = g.ui.GetUI<UIPropSelect>(UIType.PropSelect);
                    if (!c.Assert(sel != null && sel.textTitle1.text == GameTool.LS("other500020002"), "spirit stone selector opened")) return;
                    var stones = Player.GetUnitProps(ModLibConst.MONEY_PROP_ID).FirstOrDefault();
                    if (!c.Assert(stones != null, "spirit stones in the bag")) return;
                    sel.AddSelectProps(stones);
                    var selected = UIPropSelect.allSlectDataProps.allProps.ToArray().FirstOrDefault();
                    if (selected != null && selected.propsCount > STONES) selected.propsCount = STONES;
                    var value = UIPropSelect.allSlectDataProps.allProps.ToArray().Sum(x => x.propsInfoBase.worth * x.propsCount);
                    c.Bag["value"] = value;
                    c.Bag["exp"] = CustomRefineEvent.GetRefineExp(item);
                    c.Bag["lvl"] = CustomRefineEvent.GetRefineLvl(item);
                    c.Bag["money"] = Player.GetUnitMoney();
                    c.Bag["budget"] = MapBuildPropertyEvent.GetBuildProperty(BagTown(c));
                    c.Info($"selected stones value {value}");
                    sel.btnOK.onClick.Invoke();
                }, 1000)
                .Step("refined", c =>
                {
                    if (!c.Bag.ContainsKey("value")) { c.Skip("not refined"); return; }
                    var value = (int)c.Bag["value"];
                    var exp = CustomRefineEvent.GetRefineExp(item);
                    c.Assert(exp > (long)c.Bag["exp"], $"refine exp {(long)c.Bag["exp"]} -> {exp}");
                    c.Info($"refine level {(int)c.Bag["lvl"]} -> {CustomRefineEvent.GetRefineLvl(item)}");
                    c.AssertEqual((int)c.Bag["money"] - value, Player.GetUnitMoney(), $"{value} spirit stones paid");
                    c.AssertEqual((long)c.Bag["budget"] + value, MapBuildPropertyEvent.GetBuildProperty(BagTown(c)), "paid to the town budget");
                    c.Assert(g.ui.HasUI(UIType.CheckPopup), "result message shown");
                    var adj = CustomRefineEvent.GetCustomAdjType(item, 1);
                    c.Assert(adj != null, $"refine adjustment #1 of the item: {adj?.AdjType}");
                }, 1000)
                .Step("close", c =>
                {
                    ClickButton(CONFIRM);
                    CloseAll(UIType.PropSelect, UIType.TownFactoryShapeOption);
                    if (item != null) Player.RemoveUnitProp(item.soleID);
                    c.Assert(!g.ui.HasUI(UIType.TownFactoryShapeOption), "forge closed");
                });
        }
    }
}
