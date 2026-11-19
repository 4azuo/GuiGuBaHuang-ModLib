using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using UnityEngine;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.UI
{
    /// <summary>
    /// TG-61 (DamageInfo, TaoistLogEvent, RealMarketEvent3): the damage calculation window before a battle and the
    /// polished windows. The player info has the "Damage Calc" button on the top edge (left of the close button) and the
    /// S&amp;M score at the top left; the window has 2 pages: the stats with icons, then the recent monsters and cultivators as cards
    /// with their picture, their numbers of that time and the damage both ways. The Taoist log writes "Year 1, Month 2"
    /// with an icon per entry; the buy orders list shows the icon of each item. Recent foes and orders restored.
    /// </summary>
    public class TG61_DamageInfoWindow : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-61";

        public Scenario Create()
        {
            System.Collections.Generic.List<DamageInfo.FoeRecord> savedFoes = null;
            WorldUnitBase npc = null;
            MapBuildTown town = null;
            var orders = 0;
            return new Scenario { Id = "TG-61", Feature = "DamageInfo", Title = "Damage calculation window, Taoist log dates, buy order icons" }
                .Step("player info", c =>
                {
                    if (!c.Assert(DamageInfoEvent.Instance != null, "DamageInfoEvent loaded")) return;
                    savedFoes = DamageInfoEvent.Instance.Foes.ToList();
                    g.ui.OpenUISafe<UIPlayerInfo>(UIType.PlayerInfo);
                }, 1500)
                .Step("button", c =>
                {
                    var btn = FindButtons(UIType.PlayerInfo, GameTool.LS("dmginfo_button")).FirstOrDefault();
                    if (!c.Assert(btn != null, $"'{GameTool.LS("dmginfo_button")}' button in the player info")) return;
                    var ui = g.ui.GetUI<UIPlayerInfo>(UIType.PlayerInfo);
                    // on the top edge of the scroll, left of the close button (every tab), the S&M score line at the top left
                    var bp = btn.Item.transform.position;
                    var cp = ui.btnClose.transform.position;
                    c.Assert(System.Math.Abs(bp.y - cp.y) < 0.2f && bp.x < cp.x, $"next to the close button (button {bp.x:0.00},{bp.y:0.00}, close {cp.x:0.00},{cp.y:0.00})");
                    var score = UIHelper.GetUICustomBase(UIType.PlayerInfo).SelectMany(x => x.Items).OfType<ModLib.Object.UIItemText>()
                        .FirstOrDefault(x => TextIconHelper.GetShownText(x.Item).Contains(" P "));
                    c.Assert(score != null && score.Item.transform.position.x < bp.x && System.Math.Abs(score.Item.transform.position.y - cp.y) < 0.2f, "S&M score line at the top left");
                    c.Snap("button");
                })
                .Step("recent foes", c =>
                {
                    // 3 cultivators of the world recorded as foes, and a monster (base numbers of a battle config)
                    DamageInfoEvent.Instance.Foes.Clear();
                    npc = Npcs.OrderBy(x => x.GetGradeLvl()).FirstOrDefault();
                    foreach (var n in Npcs.OrderBy(x => x.GetGradeLvl()).Take(4).Reverse())
                        DamageInfo.AddRecord(DamageInfo.Snapshot(DamageInfo.FromWorld(n)));
                    c.AssertEqual(DamageInfo.RECENT_FOES, DamageInfo.RecentFoes(true).Count, "4 recorded: the 3 newest kept");
                    c.Assert(DamageInfo.RecentFoes(true)[0].UnitId == npc?.GetUnitId(), "newest first");
                    DamageInfo.AddRecord(DamageInfo.Snapshot(DamageInfo.FromWorld(npc)));
                    c.AssertEqual(DamageInfo.RECENT_FOES, DamageInfo.RecentFoes(true).Count, "the same foe again: replaced, not added");
                    var b = g.conf.battleUnitBase._allConfList.ToArray().FirstOrDefault(x => x.atk > 0 && x.hp > 0 && !string.IsNullOrEmpty(x.model));
                    if (b != null)
                    {
                        var side = new DamageInfo.Side { Name = "TG-61 monster", Grade = 2, Atk = b.atk, Def = b.def, Hp = b.hp, HpMax = b.hp };
                        foreach (var k in DamageInfo.BASIS) side.Basis[k] = 0;
                        DamageInfo.AddRecord(DamageInfo.Snapshot(side, b.id));
                    }
                    c.AssertEqual(b == null ? 0 : 1, DamageInfo.RecentFoes(false).Count, "a monster recorded");
                })
                .Step("open window", c =>
                {
                    FindButtons(UIType.PlayerInfo, GameTool.LS("dmginfo_button")).FirstOrDefault()?.Item.onClick.Invoke();
                }, 1500)
                .Step("stats page", c =>
                {
                    if (!c.Assert(DamageInfo.IsOpen, "the button opens the window")) return;
                    c.AssertEqual(2, DamageInfo.Window.Pages.Count, "2 pages: stats, recent foes");
                    var shown = DamageInfo.Window.Items.OfType<ModLib.Object.UIItemText>().Where(x => x.IsActive()).Select(x => TextIconHelper.GetShownText(x.Item)).ToList();
                    c.Assert(shown.Any(x => x.Contains(GameTool.LS("dmginfo_attack"))) && shown.Any(x => x.Contains(GameTool.LS("dmginfo_defense"))), "attack and defense columns");
                    c.Assert(shown.Count(x => x.Contains("<sprite")) >= 15, $"lines with icons ({shown.Count(x => x.Contains("<sprite"))})");
                    c.Assert(UnityEngine.Time.timeScale > 0f, "not paused outside a battle");
                    c.Snap("stats");
                })
                .Step("foes page", c =>
                {
                    if (!DamageInfo.IsOpen) return;
                    DamageInfo.Window.NextPage();
                    var shown = DamageInfo.Window.Items.OfType<ModLib.Object.UIItemText>().Where(x => x.IsActive()).Select(x => TextIconHelper.GetShownText(x.Item)).ToList();
                    foreach (var r in DamageInfo.RecentFoes(true).Concat(DamageInfo.RecentFoes(false)))
                        c.Assert(shown.Any(x => x.Contains(r.Name)), $"card of {r.Name}");
                    var pictures = DamageInfo.Window.UIBase.GetComponentsInChildren<UnityEngine.UI.RawImage>().Length;
                    c.Assert(pictures >= DamageInfo.RecentFoes(true).Count, $"pictures of the foes ({pictures} shown)");
                    c.Assert(shown.Any(x => x.Contains(string.Format(GameTool.LS("dmginfo_foe_dealt"), 0, 0, 0).Split(' ')[0])), "damage both ways on the cards");
                    c.Snap("foes");
                }, 1500)
                .Step("close", c =>
                {
                    DamageInfo.Close();
                    CloseAll(UIType.PlayerInfo);
                    c.Assert(!DamageInfo.IsOpen, "closed");
                }, 1000)
                .Step("Taoist log", c =>
                {
                    TaoistLogEvent.Log("log_tax_paid", 1, "TG-61");
                    var line = TaoistLogEvent.Instance.Entries.Last().Render();
                    c.Assert(line.Contains(string.Format(GameTool.LS("log_line"), GameHelper.GetGameYear(), GameHelper.GetGameMonth(), "").Trim().TrimEnd(':').Trim()),
                        $"full year and month: {line}");
                    c.Assert(line.StartsWith("<sprite") || line.Contains("<sprite"), "an icon per entry");
                    TaoistLogEvent.Instance.Entries.RemoveAt(TaoistLogEvent.Instance.Entries.Count - 1);
                })
                .Step("buy orders", c =>
                {
                    town = AnyTown;
                    if (RealMarketEvent3.Instance == null || town == null) { c.Info("no market: skipped"); return; }
                    orders = RealMarketEvent3.Instance.BuyOrders.Count;
                    var conf = g.conf.itemProps._allConfList.ToArray().FirstOrDefault(x => x.sale > 0 && x.IsHerbItem() != null);
                    if (conf != null) RealMarketEvent3.AddBuyOrder(town, conf.id, 1, System.Math.Max(RealMarketEvent3.MIN_PRICE, conf.sale));
                    RealMarketEvent3.Instance.OpenBuyOrders(town);
                }, 1500)
                .Step("buy order icons", c =>
                {
                    if (RealMarketEvent3.Instance == null || town == null) return;
                    var ui = UIHelper.GetUICustomBase(UIType.TextInfoLong).LastOrDefault();
                    var icons = ui?.UIBase.GetComponentsInChildren<UIIconItemBase>().Length ?? 0;
                    c.Assert(icons >= RealMarketEvent3.Instance.BuyOrders.Count, $"an item icon per order ({icons} icons, {RealMarketEvent3.Instance.BuyOrders.Count} orders)");
                    c.Snap("buy-orders");
                    CloseAll(UIType.TextInfoLong);
                    while (RealMarketEvent3.Instance.BuyOrders.Count > orders)
                        RealMarketEvent3.CancelBuyOrder(RealMarketEvent3.Instance.BuyOrders.Last());
                }, 1000)
                .Step("restore", c =>
                {
                    if (savedFoes != null) { DamageInfoEvent.Instance.Foes.Clear(); DamageInfoEvent.Instance.Foes.AddRange(savedFoes); }
                    c.Assert(true, "recent foes and buy orders restored");
                });
        }
    }
}
