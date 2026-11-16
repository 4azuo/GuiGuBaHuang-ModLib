using ModLib.Helper;
using ModLib.Object;
using MOD_nE7UL2.Mod;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Title
{
    /// <summary>
    /// TT-05 (SMGlobalConfigsEvent): opening the S&amp;M configs and pressing OK without changing anything must keep
    /// every value (each control saves into the field it was built from) and writes the global config file.
    /// </summary>
    public class TT05_SMConfigsOk : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TT-05";

        private static UICustomBase Window => UIHelper.GetUICustomBase(UIType.TextInfoLong).LastOrDefault();

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            var start = DateTime.Now;
            return new Scenario { Id = "TT-05", Feature = "SMGlobalConfigsEvent", Title = "S&M configs: OK without change keeps every value", Scope = ScenarioScope.Title }
                .Step("open window", c =>
                {
                    start = DateTime.Now;
                    c.Bag["before"] = JsonConvert.SerializeObject(SMGlobalConfigsEvent.Instance);
                    var btn = FindButtons(UIType.Login, SMGlobalConfigsEvent.TITLE).FirstOrDefault();
                    if (!c.Assert(btn != null, "S&M button on the title screen")) return;
                    btn.Item.onClick.Invoke();
                })
                .Step("OK", c =>
                {
                    var ui = g.ui.GetUI<UITextInfoLong>(UIType.TextInfoLong);
                    if (!c.Assert(ui != null && Window != null, "S&M window open")) return;
                    ui.btnOK.onClick.Invoke();
                }, 1000)
                .Step("values kept", c =>
                {
                    c.Assert(!g.ui.HasUI(UIType.TextInfoLong), "window closed by OK");
                    var before = Newtonsoft.Json.Linq.JObject.Parse((string)c.Bag["before"]);
                    var after = Newtonsoft.Json.Linq.JObject.Parse(JsonConvert.SerializeObject(SMGlobalConfigsEvent.Instance));
                    var changed = before.Properties().Where(p => !Newtonsoft.Json.Linq.JToken.DeepEquals(p.Value, after[p.Name]))
                        .Select(p => $"{p.Name}: {p.Value} -> {after[p.Name]}").ToList();
                    c.Assert(changed.Count == 0, changed.Count == 0 ? $"all {before.Properties().Count()} values kept" : $"values changed by OK: {string.Join("; ", changed)}");
                    var file = CacheHelper.GetGlobalCacheFilePath(SMGlobalConfigsEvent.Instance.ModId, SMGlobalConfigsEvent.Instance.CacheId);
                    c.Assert(File.Exists(file) && File.GetLastWriteTime(file) >= start.AddSeconds(-1), $"config file written: {Path.GetFileName(file)}");
                }, 1000);
        }
    }
}
