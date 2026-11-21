using ModLib.Helper;
using MOD_nE7UL2.Mod;
using ModLib.Object;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Title
{
    /// <summary>
    /// TT-02 (SMGlobalConfigsEvent): S&amp;M configs window - the 3 pages (page 3: toggles of the additional features, no score), difficulty presets change the score,
    /// Cancel keeps the saved configs.
    /// </summary>
    public class TT02_SMGlobalConfigsEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TT-02";

        private static UICustomBase Window => UIHelper.GetUICustomBase(UIType.TextInfoLong).LastOrDefault();

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TT-02", Feature = "SMGlobalConfigsEvent", Title = "S&M configs window: pages, presets and cancel", Scope = ScenarioScope.Title }
                .Step("values", c =>
                {
                    var cfg = SMGlobalConfigsEvent.Instance;
                    if (!c.Assert(cfg != null, "SMGlobalConfigsEvent loaded")) return;
                    c.Assert(cfg.GrowUpSpeed >= 1, $"GrowUpSpeed >= 1 ({cfg.GrowUpSpeed})");
                    c.Assert(cfg.NPCAmount > 0, $"NPCAmount > 0 ({cfg.NPCAmount})");
                    c.Assert(cfg.StartupPlayerRole >= 0 && cfg.StartupPlayerRole < cfg.StartupPlayerRoles.Length, $"StartupPlayerRole in range ({cfg.StartupPlayerRole})");
                    c.Bag["saved"] = Newtonsoft.Json.JsonConvert.SerializeObject(new { cfg.AddAtkRate, cfg.AddTaxRate, cfg.GrowUpSpeed, cfg.Onelife, cfg.EnableTrainer, cfg.NoRebirth });
                })
                .Step("open window", c =>
                {
                    var btn = FindButtons(UIType.Login, SMGlobalConfigsEvent.TITLE).FirstOrDefault();
                    if (!c.Assert(btn != null, "S&M button exists on the title screen")) return;
                    btn.Item.onClick.Invoke();
                })
                .Step("page 1", c =>
                {
                    c.Assert(g.ui.HasUI(UIType.TextInfoLong) && Window != null, "S&M window is open");
                    c.Assert(Window?.Pages.Count == 4, $"4 pages ({Window?.Pages.Count}): the storylines on their own page");
                    c.AssertEqual(0, Window?.CurrentPageIndex ?? -1, "page 1 shown first");
                    c.Bag["score"] = SMGlobalConfigsEvent.CalSMTotalScore();
                    c.Info($"total score: {c.Bag["score"]}");
                })
                .Step("page 2", c =>
                {
                    Window.NextPage();
                    c.AssertEqual(1, Window.CurrentPageIndex, "next page shows page 2 (economy, battle, misc options)");
                })
                .Step("page 3", c =>
                {
                    Window.NextPage();
                    c.AssertEqual(2, Window.CurrentPageIndex, "page 3: additional features");
                    var score = SMGlobalConfigsEvent.CalSMTotalScore();
                    var labels = new[] { "smcfgs151", "smcfgs152", "smcfgs153", "smcfgs154", "smcfgs156", "smcfgs157", "smcfgs158", "smcfgs159", "smcfgs164", "smcfgs167" }.Select(k => GameTool.LS(k)).ToList();
                    var toggles = Window.Items.OfType<ModLib.Object.UIItemText>().Where(x => labels.Contains(x.GetFormat() ?? string.Empty))
                        .Select(x => x.Parent as ModLib.Object.UIItemComposite).Where(x => x != null).ToList();
                    c.AssertEqual(10, toggles.Count, "toggles: encounters, bank loans, buy orders, bounties, seclusion, forced seclusion, family, same-sex, descendant, storylines");
                    var others = new[] { "smcfgs160", "smcfgs165", "smcfgs166" }.Select(k => GameTool.LS(k)).ToList();
                    c.AssertEqual(3, Window.Items.OfType<ModLib.Object.UIItemText>().Count(x => others.Contains(x.GetFormat() ?? string.Empty)), "child surname select, encounter and seclusion rate sliders");
                    // each toggle has its points (2026-11-18): No family +2000 alone, then Continue as a descendant -10000
                    // only with the family system on; No bounties -2000 (easier)
                    var family = (ModLib.Object.UIItemComposite)Window.Items.OfType<ModLib.Object.UIItemText>().First(x => x.GetFormat() == GameTool.LS("smcfgs158")).Parent;
                    var bounty = (ModLib.Object.UIItemComposite)Window.Items.OfType<ModLib.Object.UIItemText>().First(x => x.GetFormat() == GameTool.LS("smcfgs154")).Parent;
                    foreach (var t in toggles) t.MainComponent.Set(false);
                    var off = SMGlobalConfigsEvent.CalSMTotalScore();
                    family.MainComponent.Set(true);
                    c.AssertEqual(off + 2000, SMGlobalConfigsEvent.CalSMTotalScore(), "No family system: +2000 P");
                    family.MainComponent.Set(false);
                    bounty.MainComponent.Set(true);
                    c.AssertEqual(off - 2000, SMGlobalConfigsEvent.CalSMTotalScore(), "No bounties: -2000 P (easier)");
                    bounty.MainComponent.Set(false);
                    c.AssertEqual(off, SMGlobalConfigsEvent.CalSMTotalScore(), "all off again: the score of before");
                    c.Info($"score {score} with the saved toggles, {off} with all off");
                    // the cultivation paths and the estates (6.0): on / off, gains, prices
                    var paths = new[] { "smcfgs192", "smcfgs193", "smcfgs194", "smcfgs195", "smcfgs196", "smcfgs199", "smcfgs200", "smcfgs201", "smcfgs202", "smcfgs203",
                        "smcfgs204", "smcfgs205", "smcfgs206", "smcfgs207" }.Select(k => GameTool.LS(k)).ToList();
                    var items = Window.Items.OfType<ModLib.Object.UIItemText>().Where(x => paths.Contains(x.GetFormat() ?? string.Empty)).ToList();
                    c.AssertEqual(14, items.Count, "cultivation paths (5 toggles, 5 gain sliders) and estates (2 toggles, price and tax sliders)");
                    var body = (ModLib.Object.UIItemComposite)items.First(x => x.GetFormat() == GameTool.LS("smcfgs199")).Parent;
                    var before = SMGlobalConfigsEvent.CalSMTotalScore();
                    body.MainComponent.Set(1f);
                    c.AssertEqual(before - 300, SMGlobalConfigsEvent.CalSMTotalScore(), "body exp +100 %: -300 P (easier)");
                    body.MainComponent.Set(0f);
                    var price = (ModLib.Object.UIItemComposite)items.First(x => x.GetFormat() == GameTool.LS("smcfgs206")).Parent;
                    price.MainComponent.Set(1f);
                    c.AssertEqual(before + 500, SMGlobalConfigsEvent.CalSMTotalScore(), "house prices +100 %: +500 P (harder)");
                    price.MainComponent.Set(0f);
                })
                .Step("page 3 shown", c => c.Info($"page {Window.CurrentPageIndex + 1}"), 1000) // screenshot of page 3
                .Step("page 4", c =>
                {
                    Window.NextPage();
                    c.AssertEqual(3, Window.CurrentPageIndex, "page 4: NPC storylines");
                    var lines = new[] { "smcfgs169", "smcfgs191", "smcfgs208", "smcfgs211" }.Select(k => GameTool.LS(k)).ToList();
                    c.Assert(Window.Items.OfType<ModLib.Object.UIItemText>().Count(x => lines.Any(l => (x.GetFormat() ?? string.Empty).Contains(l.Substring(0, System.Math.Min(20, l.Length))))) >= 4,
                        "the lines A..P described");
                }, 1000)
                .Step("back to page 3", c => Window.PrevPage())
                .Step("back to page 2", c => Window.PrevPage())
                .Step("preset level 3", c =>
                {
                    var btn = FindButtons(UIType.TextInfoLong, $"{GameTool.LS("smcfgs045")} 3").FirstOrDefault(x => x.GetFormat() == $"{GameTool.LS("smcfgs045")} 3");
                    if (!c.Assert(btn != null, "button 'Level 3' exists")) return;
                    btn.Item.onClick.Invoke();
                    var score = SMGlobalConfigsEvent.CalSMTotalScore();
                    c.Assert(score != (int)c.Bag["score"], $"score changed by the preset: {c.Bag["score"]} -> {score}");
                })
                .Step("preset default", c =>
                {
                    var btn = FindButtons(UIType.TextInfoLong, GameTool.LS("smcfgs044")).FirstOrDefault(x => x.GetFormat() == GameTool.LS("smcfgs044"));
                    if (!c.Assert(btn != null, "button 'Default' exists")) return;
                    btn.Item.onClick.Invoke();
                    c.Info($"score after Default: {SMGlobalConfigsEvent.CalSMTotalScore()}");
                    Window.PrevPage();
                    c.AssertEqual(0, Window.CurrentPageIndex, "previous page shows page 1");
                })
                .Step("cancel", c =>
                {
                    if (g.ui.HasUI(UIType.TextInfoLong)) g.ui.CloseUI(UIType.TextInfoLong);
                    c.Assert(!g.ui.HasUI(UIType.TextInfoLong), "S&M window closed without OK");
                    var cfg = SMGlobalConfigsEvent.Instance;
                    var now = Newtonsoft.Json.JsonConvert.SerializeObject(new { cfg.AddAtkRate, cfg.AddTaxRate, cfg.GrowUpSpeed, cfg.Onelife, cfg.EnableTrainer, cfg.NoRebirth });
                    c.AssertEqual((string)c.Bag["saved"], now, "saved configs unchanged after Cancel");
                });
        }
    }
}
