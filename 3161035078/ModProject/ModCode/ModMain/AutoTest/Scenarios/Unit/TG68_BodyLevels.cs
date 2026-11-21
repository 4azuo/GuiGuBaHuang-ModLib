using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using Newtonsoft.Json;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Unit
{
    /// <summary>
    /// TG-68 (BodyCulEvent, gallery of the levels): the body refining window at the start and at the bottleneck of every
    /// realm and at the top level, a screenshot each: the materials of the baths and of the breakthroughs for the three
    /// doses (x1 / x2 / x3), with their icons and counts; the step info names the items. Body data restored.
    /// </summary>
    public class TG68_BodyLevels : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-68";

        private static string Names(BodyCulEvent.Need[] needs) =>
            string.Join(" + ", needs.Select(n =>
            {
                var id = BodyCulEvent.SampleId(Player, n);
                var name = id == 0 ? "-" : GameTool.LS(g.conf.itemProps.GetItem(id)?.name ?? "");
                if (n.IsHerbs) { var hc = BodyCulEvent.HerbCounts(n); return $"herbs of item level {BodyCulEvent.HerbFloor(Player.GetGradeLvl())}+: {hc.low} low / {hc.mid} middle / {hc.high} high (e.g. {name})"; }
                return $"{n.Count} x {name} or rarer (item level {n.Level}+)";
            }));

        /// <summary>Sets the cultivation grade of the player (the first phase of it).</summary>
        private static void SetGrade(int grade)
        {
            var conf = g.conf.roleGrade._allConfList.ToArray().Where(x => x.grade == grade).OrderBy(x => x.id).FirstOrDefault();
            if (conf != null) Player.SetProperty<int>(UnitPropertyEnum.GradeID, conf.id);
        }

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            var s = new Scenario { Id = "TG-68", Feature = "BodyCulEvent", Title = "Body refining levels and their materials", Overlay = false };
            s.Step("save the body", c =>
            {
                var id = Player.GetUnitId();
                c.Bag["body"] = BodyCulEvent.Instance.Bodies.TryGetValue(id, out var b) ? JsonConvert.SerializeObject(b) : null;
                c.Bag["grade"] = Player.GetProperty<int>(UnitPropertyEnum.GradeID);
                if (BodyCulEvent.Get(Player) == null) BodyCulEvent.Start(Player);
            });
            for (var r = 1; r <= BodyCulEvent.REALMS; r++)
            {
                var realm = r;
                foreach (var bottleneck in new[] { false, true })
                {
                    var level = bottleneck ? realm * BodyCulEvent.LAYERS : (realm - 1) * BodyCulEvent.LAYERS + 1;
                    var top = level >= BodyCulEvent.MAX_LEVEL;
                    var lv = level; var neck = bottleneck;
                    s.Step($"realm {realm} level {lv}{(neck ? (top ? " (top)" : " (bottleneck)") : "")}", c =>
                    {
                        CloseAll(UIType.TextInfoLong);
                        var d = BodyCulEvent.Get(Player);
                        d.Level = lv;
                        d.Exp = neck ? BodyCulEvent.NeedExp(lv) : 0;
                        d.InjuryMonths = 0;
                        d.LastBathMonth = -1;
                        // the cultivation grade of the realm (at a bottleneck: the one of the next realm): the herbs it may use
                        SetGrade(BodyCulEvent.RealmGrade(neck && !top ? lv + 1 : lv));
                        c.Info($"cultivation grade {Player.GetGradeLvl()}: herbs of item level {BodyCulEvent.HerbFloor(Player.GetGradeLvl())}+");
                        c.Info($"bath x1: {Names(BodyCulEvent.BathNeeds(lv))}");
                        if (neck && !top)
                            c.Info($"breakthrough x1: {Names(BodyCulEvent.BreakNeeds(lv))} (x2 / x3: two / three times)");
                        BodyCulEvent.OpenWindow();
                        var texts = ScreenTexts();
                        c.Assert(texts.Any(x => x.Contains(BodyCulEvent.RealmName(lv))), $"the window shows {BodyCulEvent.RealmName(lv)}");
                        string Prefix(string key) { var f = GameTool.LS(key); var i = f.IndexOf("{0}"); return i > 0 ? f.Substring(0, i) : f; }
                        c.AssertEqual(BodyCulEvent.DOSE_MAX, texts.Count(x => x.StartsWith(Prefix("body_bath_dose"))), "three bath doses");
                        if (neck && !top)
                            c.AssertEqual(BodyCulEvent.DOSE_MAX, texts.Count(x => x.StartsWith(Prefix("body_break_dose"))), "three breakthrough doses");
                    }, 1200);
                }
            }
            s.Step("grade 10, Undying Body", c =>
            {
                CloseAll(UIType.TextInfoLong);
                var d = BodyCulEvent.Get(Player);
                d.Level = BodyCulEvent.MAX_LEVEL - 2; d.Exp = 0; d.LastBathMonth = -1;
                SetGrade(10);
                c.Info($"cultivation grade {Player.GetGradeLvl()}: herbs of item level {BodyCulEvent.HerbFloor(Player.GetGradeLvl())}+; bath x1: {Names(BodyCulEvent.BathNeeds(d.Level))}");
                c.AssertEqual(5, BodyCulEvent.HerbFloor(Player.GetGradeLvl()), "grade 10: herbs of item level 5+ only");
                BodyCulEvent.OpenWindow();
            }, 1200);
            s.Step("restore", c =>
            {
                CloseAll(UIType.TextInfoLong);
                Player.SetProperty<int>(UnitPropertyEnum.GradeID, (int)c.Bag["grade"]);
                var id = Player.GetUnitId();
                BodyCulEvent.Instance.Bodies.Remove(id);
                if (c.Bag["body"] != null)
                    BodyCulEvent.Instance.Bodies[id] = JsonConvert.DeserializeObject<BodyCulEvent.BodyData>((string)c.Bag["body"]);
                UnitModifyEvent.Instance.CachedValues.Clear();
            });
            return s;
        }
    }
}
