using ModLib.Helper;
using MOD_nE7UL2.Mod;
using ModLib.Object;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Title
{
    /// <summary>
    /// TT-02 (SMGlobalConfigsEvent): S&amp;M configs window - both pages, difficulty presets change the score,
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
                    c.Assert(Window?.Pages.Count == 2, $"2 pages ({Window?.Pages.Count})");
                    c.AssertEqual(0, Window?.CurrentPageIndex ?? -1, "page 1 shown first");
                    c.Bag["score"] = SMGlobalConfigsEvent.CalSMTotalScore();
                    c.Info($"total score: {c.Bag["score"]}");
                })
                .Step("page 2", c =>
                {
                    Window.NextPage();
                    c.AssertEqual(1, Window.CurrentPageIndex, "next page shows page 2 (economy, battle, misc options)");
                })
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
