using ModLib.Const;
using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Enum;
using MOD_nE7UL2.Mod;
using System;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-48 (CustomRefineEvent): custom refine of an artifact (weapon) at a small town forge, twice, like a player.
    /// Refine button -> item selector -> spirit stone selector -> OK.
    /// 1st refine (a few stones): refine exp increases, the stones go to the town budget, the result message shows
    ///   "level old -> new (+exp)".
    /// 2nd refine (enough stones for refine level 100): the level reaches 100, the 1st adjustment slot is unlocked
    ///   (its quality color is shown, its value is above 0), the artifact Atk/Def bonuses grow with the level, and the
    ///   artifact info window shows "Refine (level)".
    /// Replaces the manual TM-13.
    /// </summary>
    public class TG48_CustomRefine : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-48";

        private const int STONES = 5000;
        private const int TARGET_LVL = 100;

        /// <summary>Spirit stones needed to bring the item from its current exp to the refine level.</summary>
        private static long StonesFor(DataProps.PropsData item, int level)
        {
            var lvl1 = SMLocalConfigsEvent.Instance.Calculate(item.propsInfoBase.grade * 100 + item.propsInfoBase.level * 20, SMLocalConfigsEvent.Instance.Configs.AddRefineCost);
            // level = sqrt(exp / lvl1) (CustomRefineEvent.GetRefineLvl): exp = lvl1 x level^2, with a margin
            var needExp = (long)Math.Ceiling(lvl1 * level * level * 1.05) - CustomRefineEvent.GetRefineExp(item);
            var refine = Math.Max(1, Player.GetDynProperty(UnitDynPropertyEnum.RefineWeapon).value);
            return Math.Max(1, (long)Math.Ceiling(needExp * (double)CustomRefineEvent.REFINE_EXP_RATE / refine));
        }

        /// <summary>Forge open: clicks the custom refine button (replaces the upgrade button), the item selector opens.</summary>
        private static void ClickCustomRefine(AutoTestContext c)
        {
            var ui = g.ui.GetUI<UITownFactoryShapeOption>(UIType.TownFactoryShapeOption);
            if (!c.Assert(ui != null, "forge opened")) return;
            var label = ui.btnUpgrade.GetComponentInChildren<UnityEngine.UI.Text>(true)?.text;
            var custom = ui.GetComponentsInChildren<UnityEngine.UI.Button>()
                .FirstOrDefault(b => b.gameObject.activeInHierarchy && b.name.StartsWith("CompHelper:") && b.GetComponentInChildren<UnityEngine.UI.Text>()?.text == label);
            if (!c.Assert(custom != null, "custom refine button")) return;
            custom.onClick.Invoke();
            c.AssertEqual(GameTool.LS("other500020001"), g.ui.GetUI<UIPropSelect>(UIType.PropSelect)?.textTitle1?.text, "item selector opened");
        }

        private static Scenario Refine(Scenario s, string name, Func<DataProps.PropsData> item, Func<DataProps.PropsData, long> stones)
        {
            return s
                .Step($"{name}: open forge", c => OpenTownUI<UITownFactoryShapeOption>(UIType.TownFactoryShapeOption, ui => ui.InitData(BagTown(c))))
                .Step($"{name}: refine button", c => ClickCustomRefine(c), 1000)
                .Step($"{name}: select item", c =>
                {
                    var sel = g.ui.GetUI<UIPropSelect>(UIType.PropSelect);
                    var it = item();
                    if (sel == null || it == null) { c.Skip("no selector/item"); return; }
                    var listed = sel.allItems.allProps.ToArray();
                    c.Assert(listed.Any(x => x.soleID == it.soleID), "the artifact is listed");
                    c.Assert(listed.All(CustomRefineEvent.IsRefinableItem), "only refinable items (ring/outfit/artifact/mount) are listed");
                    sel.onCustomSelectCall.Invoke(it);
                    sel.btnOK.onClick.Invoke();
                }, 1000)
                .Step($"{name}: select stones", c =>
                {
                    var sel = g.ui.GetUI<UIPropSelect>(UIType.PropSelect);
                    var it = item();
                    if (!c.Assert(sel != null && sel.textTitle1.text == GameTool.LS("other500020002"), "spirit stone selector opened")) return;
                    c.Assert(sel.allItems.allProps.ToArray().All(CustomRefineEvent.IsRefinableMaterial), "only spirit stones are listed");
                    var count = (int)Math.Min(int.MaxValue / 4, stones(it));
                    if (Player.GetUnitMoney() < count * 2L) Player.AddUnitMoney(count * 2);
                    var money = Player.GetUnitProps(ModLibConst.MONEY_PROP_ID).FirstOrDefault();
                    if (!c.Assert(money != null, "spirit stones in the bag")) return;
                    sel.AddSelectProps(money);
                    var selected = UIPropSelect.allSlectDataProps.allProps.ToArray().FirstOrDefault();
                    if (selected != null && selected.propsCount > count) selected.propsCount = count;
                    var value = UIPropSelect.allSlectDataProps.allProps.ToArray().Sum(x => x.propsInfoBase.worth * x.propsCount);
                    c.Bag["value"] = value;
                    c.Bag["exp"] = CustomRefineEvent.GetRefineExp(it);
                    c.Bag["lvl"] = CustomRefineEvent.GetRefineLvl(it);
                    c.Bag["money"] = Player.GetUnitMoney();
                    c.Bag["budget"] = MapBuildPropertyEvent.GetBuildProperty(BagTown(c));
                    c.Info($"selected stones value {value:#,##0} (refine weapon {Player.GetDynProperty(UnitDynPropertyEnum.RefineWeapon).value})");
                    sel.btnOK.onClick.Invoke();
                }, 1000)
                .Step($"{name}: refined", c =>
                {
                    var it = item();
                    if (!c.Bag.ContainsKey("value") || it == null) { c.Skip("not refined"); return; }
                    var value = (int)c.Bag["value"];
                    var oldExp = (long)c.Bag["exp"];
                    var oldLvl = (int)c.Bag["lvl"];
                    var exp = CustomRefineEvent.GetRefineExp(it);
                    var lvl = CustomRefineEvent.GetRefineLvl(it);
                    var gained = exp - oldExp;
                    var expected = (long)value * Player.GetDynProperty(UnitDynPropertyEnum.RefineWeapon).value / CustomRefineEvent.REFINE_EXP_RATE;
                    c.AssertEqual(expected, gained, $"refine exp {oldExp:#,##0} -> {exp:#,##0} (stones x refine weapon / {CustomRefineEvent.REFINE_EXP_RATE})");
                    c.Assert(lvl >= oldLvl, $"refine level {oldLvl} -> {lvl}");
                    c.AssertEqual((int)c.Bag["money"] - value, Player.GetUnitMoney(), $"{value:#,##0} spirit stones paid");
                    c.AssertEqual((long)c.Bag["budget"] + value, MapBuildPropertyEvent.GetBuildProperty(BagTown(c)), "paid to the town budget");
                    var msg = string.Format(GameTool.LS("other500020051"), oldLvl, lvl, gained);
                    c.Assert(g.ui.HasUI(UIType.CheckPopup) && FindTexts(g.ui.GetUI<UIBase>(UIType.CheckPopup), msg).Any(), $"result message: {msg}");
                    c.Bag.Remove("value");
                }, 1000)
                .Step($"{name}: close", c =>
                {
                    ClickButton(CONFIRM);
                    CloseAll(UIType.PropSelect, UIType.TownFactoryShapeOption);
                    c.Assert(!g.ui.HasUI(UIType.TownFactoryShapeOption), "forge closed");
                });
        }

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            DataProps.PropsData item = null;
            var s = new Scenario { Id = "TG-48", Feature = "CustomRefineEvent", Title = "Custom refine of an artifact: exp, level, adjustment quality, bonuses, progress, reroll", Overlay = false }
                .Step("travel", c => TravelTo(c, FindTown(t => Has(t, MapBuildSubType.TownFactory) && !t.buildTownData.isMainTown)))
                .Step("artifact", c =>
                {
                    var conf = g.conf.itemProps._allConfList.ToArray().FirstOrDefault(x => x.IsArtifact() != null);
                    if (!c.Assert(conf != null, "an artifact config exists")) return;
                    Player.AddUnitProp(conf.id, 1);
                    item = Player.GetUnitProps(conf.id).LastOrDefault();
                    if (!c.Assert(item != null && CustomRefineEvent.IsRefinableItem(item), $"refinable artifact {item?.propsInfoBase?.name} (grade {item?.propsInfoBase?.grade}, level {item?.propsInfoBase?.level}) in the bag")) return;
                    c.AssertEqual(0L, CustomRefineEvent.GetRefineExp(item), "a new artifact has no refine exp");
                    c.Bag["atk0"] = UnitModifyHelper.GetRefineArtifactAdjAtk(item, 0);
                    c.Bag["def0"] = UnitModifyHelper.GetRefineArtifactAdjDef(item, 0);
                    c.Assert(Player.GetDynProperty(UnitDynPropertyEnum.RefineWeapon).value > 0, $"player refine weapon skill {Player.GetDynProperty(UnitDynPropertyEnum.RefineWeapon).value} > 0 (else stones give no exp)");
                });
            s = Refine(s, "small", () => item, it => STONES);
            s = Refine(s, $"to level {TARGET_LVL}", () => item, it => StonesFor(it, TARGET_LVL));
            return s
                .Step("quality", c =>
                {
                    if (item == null) { c.Skip("no artifact"); return; }
                    var lvl = CustomRefineEvent.GetRefineLvl(item);
                    c.Assert(lvl >= TARGET_LVL, $"refine level {lvl} >= {TARGET_LVL}");
                    var adj1 = CustomRefineEvent.GetCustomAdjType(item, 1);
                    var adj2 = CustomRefineEvent.GetCustomAdjType(item, 2);
                    if (!c.Assert(adj1 != null && adj2 != null, "adjustment slots exist")) return;
                    c.Assert(CustomRefine_ArtifactTypes().Contains(adj1.AdjType), $"slot 1 type {adj1.AdjType} is an artifact adjustment");
                    c.Assert(adj1.IsEnable(lvl) && !adj2.IsEnable(lvl), $"slot 1 unlocked at level {lvl}, slot 2 still locked (needs 200)");
                    c.Assert(adj1.GetColor(lvl) != AdjLevelEnum.None.Color, $"slot 1 shows its quality {adj1.AdjLevel} (x{adj1.AdjLevel.Multiplier})");
                    c.Assert(adj1.GetRefineCustommAdjValue(Player, item, lvl) > 0, $"slot 1 value: {adj1.GetText(Player, item, lvl)}");
                    c.Assert(adj2.GetText(Player, item, lvl).Contains("(Req 200)"), "slot 2 text shows its requirement");
                    var atk = UnitModifyHelper.GetRefineArtifactAdjAtk(item, lvl);
                    var def = UnitModifyHelper.GetRefineArtifactAdjDef(item, lvl);
                    c.Assert(atk > (int)c.Bag["atk0"] && def >= (int)c.Bag["def0"], $"artifact bonuses grow: +Atk {(int)c.Bag["atk0"]} -> {atk}, +Def {(int)c.Bag["def0"]} -> {def}");
                })
                .Step("progress", c =>
                {
                    if (item == null) { c.Skip("no artifact"); return; }
                    var lvl = CustomRefineEvent.GetRefineLvl(item);
                    var exp = CustomRefineEvent.GetRefineExp(item);
                    c.Assert(CustomRefineEvent.ExpForLevel(item, lvl) <= exp && exp < CustomRefineEvent.ExpForLevel(item, lvl + 1), $"exp {exp:#,##0} between level {lvl} and {lvl + 1} ({CustomRefineEvent.ExpForLevel(item, lvl + 1):#,##0})");
                    var text = CustomRefineEvent.ProgressText(item);
                    c.Assert(text.Contains(lvl.ToString()) && text.Contains("200"), $"progress: {text}");
                })
                .Step("reroll", c =>
                {
                    if (item == null) { c.Skip("no artifact"); return; }
                    var cost = CustomRefineEvent.RerollCost(item, 1);
                    if (Player.GetUnitMoney() < cost * 2) Player.AddUnitMoney(cost * 2);
                    var m = Player.GetUnitMoney();
                    var before = CustomRefineEvent.GetCustomAdjType(item, 1);
                    c.Assert(CustomRefineEvent.Reroll(item, 1), $"slot 1 rerolled ({cost:#,##0})");
                    c.AssertEqual(m - cost, Player.GetUnitMoney(), "reroll paid");
                    c.Assert(!ReferenceEquals(before, CustomRefineEvent.GetCustomAdjType(item, 1)), $"new slot 1: {CustomRefineEvent.GetCustomAdjType(item, 1).GetText(Player, item, CustomRefineEvent.GetRefineLvl(item))}");
                    c.Assert(!CustomRefineEvent.Reroll(item, 2), "a locked slot cannot be rerolled");
                    CustomRefineEvent.Instance.OpenReroll(item);
                }, 1500)
                .Step("reroll window", c =>
                {
                    c.Assert(g.ui.HasUI(UIType.TextInfoLong), "reroll window");
                    c.Assert(FindTexts(g.ui.GetUI<UIBase>(UIType.TextInfoLong), "(Req 200)").Any(), "locked slots show their requirement");
                    CloseAll(UIType.TextInfoLong);
                })
                // the refine info is added to the artifact info opened from a bag/selector (here: the forge item selector)
                .Step("info: forge", c => OpenTownUI<UITownFactoryShapeOption>(UIType.TownFactoryShapeOption, ui => ui.InitData(BagTown(c))))
                .Step("info: item selector", c => ClickCustomRefine(c), 1000)
                .Step("artifact info", c =>
                {
                    if (item == null || !g.ui.HasUI(UIType.PropSelect)) { c.Skip("no artifact/selector"); return; }
                    var ui = g.ui.OpenUISafe<UIArtifactInfo>(UIType.ArtifactInfo);
                    ui.InitData(Player, item, UnityEngine.Vector2.zero, false);
                    c.Assert(ui != null, "artifact info opened");
                }, 1500)
                .Step("artifact info text", c =>
                {
                    var ui = g.ui.GetUI<UIArtifactInfo>(UIType.ArtifactInfo);
                    if (ui == null || item == null) { c.Skip("no artifact info"); return; }
                    var lvl = CustomRefineEvent.GetRefineLvl(item);
                    c.Assert(FindTexts(ui, $"Refine ({lvl})").Any(), $"artifact info shows 'Refine ({lvl})'");
                    c.Assert(FindTexts(ui, $"+Atk: {UnitModifyHelper.GetRefineArtifactAdjAtk(item, lvl)}").Any(), "artifact info shows the refine Atk bonus");
                })
                .Step("cleanup", c =>
                {
                    CloseAll(UIType.ArtifactInfo, UIType.PropSelect, UIType.TownFactoryShapeOption);
                    if (item != null) Player.RemoveUnitProp(item.soleID);
                    c.Assert(!g.ui.HasUI(UIType.ArtifactInfo), "closed, artifact removed");
                });
        }

        private static AdjTypeEnum[] CustomRefine_ArtifactTypes() => MOD_nE7UL2.Object.CustomRefine.ArtifactAdjTypes;
    }
}
