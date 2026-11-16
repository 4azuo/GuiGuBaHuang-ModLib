using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOD_nE7UL2;
using MOD_nE7UL2.AutoTest;
using MOD_nE7UL2.Object;
using ModLib.Attributes;
using ModLib.Helper;
using ModLib.Mod;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace UnitTests.Taoist
{
    /// <summary>
    /// Static checks of Taoist configs and code/config consistency (catch broken JSON, unknown event ids,
    /// missing LocalText keys and scenario id mismatches before starting the game).
    /// </summary>
    [TestClass]
    public class ConfigTests
    {
        private static string ModConf => TestSetup.Repo("3161035078", "ModProject", "ModConf");
        private static string ModCode => TestSetup.Repo("3161035078", "ModProject", "ModCode", "ModMain");
        private static Assembly Taoist => typeof(ModMain).Assembly;

        private static List<(Type Type, CacheAttribute Attr)> CacheTypes() =>
            Taoist.GetLoadableTypes().Where(t => t.IsClass && typeof(ModEvent).IsAssignableFrom(t))
                .Select(t => (t, t.GetCustomAttribute<CacheAttribute>())).Where(x => x.Item2 != null).ToList();

        [TestMethod, Description("Every luck (*roleCreateFeature.json) effect id exists in a *roleEffect.json (Taoist) or the game RoleEffect (a missing effect throws NullReferenceException in CreateAction when the luck is added)")]
        public void LuckEffects_Exist()
        {
            IEnumerable<string> Ids(string file) => JArray.Parse(File.ReadAllText(file)).Select(x => (string)x["id"]);
            var sample = TestSetup.Repo("3385996759", "SampleConfs");
            var effects = new HashSet<string>(Directory.GetFiles(ModConf, "*roleEffect.json").SelectMany(Ids)
                .Concat(new[] { "RoleEffect.json", "DLCRoleEffect.json" }.Select(f => Path.Combine(sample, f)).Where(File.Exists).SelectMany(Ids)));
            var missing = new List<string>();
            foreach (var f in Directory.GetFiles(ModConf, "*roleCreateFeature.json"))
                foreach (var luck in JArray.Parse(File.ReadAllText(f)))
                    foreach (var eff in ((string)luck["effect"] ?? "").Split('|', ',', '_').Select(x => x.Trim()).Where(x => x.Length > 0 && x != "0"))
                        if (!effects.Contains(eff))
                            missing.Add($"{Path.GetFileName(f)}: luck {luck["id"]} effect {eff}");
            Check.Info($"{effects.Count} effect ids known");
            Check.None(missing, "luck effects without definition");
        }

        [TestMethod, Description("Every JSON file under 3161035078/ModProject/ModConf parses")]
        public void AllModConfJson_AreValid()
        {
            var errors = new List<string>();
            foreach (var f in Directory.GetFiles(ModConf, "*.json", SearchOption.AllDirectories))
            {
                try { JToken.Parse(File.ReadAllText(f)); }
                catch (Exception ex) { errors.Add($"{f.Substring(ModConf.Length + 1)}: {ex.Message}"); }
            }
            Check.None(errors, "invalid JSON files", Directory.GetFiles(ModConf, "*.json", SearchOption.AllDirectories).Length);
        }

        [TestMethod, Description("mod_configs.json sections/keys = ModStts properties (no typo, nothing missing); bank fees valid")]
        public void ModConfigs_SectionsMatchModStts()
        {
            // ModStts cannot be deserialized outside the game: it uses Il2Cpp enums (MartialType, MonstType...)
            // whose proxy types fail to load ("Method in an Enum"). Check the JSON structure against the class instead.
            var json = JObject.Parse(File.ReadAllText(Path.Combine(ModConf, "mod_configs.json")));
            var props = typeof(ModStts).GetProperties().ToDictionary(p => p.Name, p => p);
            var unknown = json.Properties().Select(p => p.Name).Where(n => !n.StartsWith("__") && !props.ContainsKey(n)).ToList();   // "__xxx" = editor metadata
            var missing = props.Keys.Where(n => json[n] == null).ToList();
            Check.Info($"{json.Properties().Count()} sections in mod_configs.json, {props.Count} properties in ModStts");
            Check.None(unknown, "sections not in ModStts");
            Check.None(missing, "ModStts sections missing in mod_configs.json");
            var errors = new List<string>();
            foreach (var section in json.Properties().Where(p => p.Value is JObject && props.ContainsKey(p.Name)))
            {
                var names = props[section.Name].PropertyType.GetProperties().Select(p => p.Name).ToList();
                errors.AddRange(((JObject)section.Value).Properties().Where(p => !p.Name.StartsWith("__") && !names.Contains(p.Name)).Select(p => $"{section.Name}.{p.Name}"));
            }
            Check.None(errors, "keys ignored by ModStts (typo?)");
            var fees = (JObject)json["BankAccountConfigs"]["OpenFee"];
            Check.True(fees.Count >= 10, "bank fee for every area (>= 10)", fees.Count);
            Check.True(fees.Properties().All(p => (int)p.Value >= 100), "bank fees >= 100 (BankAccountEvent.Cost divides by OpenFee / 100)", fees.Properties().Select(p => (int)p.Value).ToList());
        }

        [TestMethod, Description("Every [Cache] id of the Taoist events is unique")]
        public void CacheIds_AreUnique()
        {
            var dups = CacheTypes().GroupBy(x => x.Attr.CacheId).Where(g => g.Count() > 1).Select(g => $"{g.Key}: {string.Join(", ", g.Select(x => x.Type.Name))}").ToList();
            Check.None(dups, "duplicated cache ids", CacheTypes().Count);
        }

        [TestMethod, Description("_EventOrderIndex.json: every id is an event, every event has an order (except BUG-09), orders > 0")]
        public void EventOrderIndex_MatchesEvents()
        {
            var order = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(Path.Combine(ModConf, "_EventOrderIndex.json")));
            var ids = CacheTypes().Where(x => !typeof(ModChild).IsAssignableFrom(x.Type)).Select(x => x.Attr.CacheId).ToList();
            var disabled = new[] { "DebugEvent" };   // DebugEvent.cs is commented out
            var knownNotOrdered = KnownIssues.EventsWithoutOrder;   // BUG-09, reproduced by KnownIssues
            var unknown = order.Keys.Where(k => !ids.Contains(k) && !disabled.Contains(k)).ToList();
            Check.Info($"{order.Count} ids in _EventOrderIndex.json, {ids.Count} events; tolerated (BUG-09): {string.Join(", ", knownNotOrdered)}");
            Check.None(unknown, "ids in _EventOrderIndex.json without event");
            var notOrdered = ids.Where(i => !order.ContainsKey(i) && !knownNotOrdered.Contains(i)).ToList();
            Check.None(notOrdered, "events without order (get 9000+)");
            Check.True(order.Values.All(v => v > 0), "order index > 0", $"min {order.Values.Min()}, max {order.Values.Max()}");
        }

        [TestMethod, Description("Every GameTool.LS(\"key\") literal in the Taoist code exists in *_localText.json (except BUG-08)")]
        public void LocalTextKeys_UsedInCode_Exist()
        {
            var keys = new HashSet<string>();
            // Taoist texts + ModLib texts (ModLib is always loaded with Taoist; e.g. TT-04 uses ModLib's libtxt keys)
            var libConf = TestSetup.Repo("3385996759", "ModProject", "ModConf");
            foreach (var f in Directory.GetFiles(ModConf, "*localText.json", SearchOption.TopDirectoryOnly)
                         .Concat(Directory.Exists(libConf) ? Directory.GetFiles(libConf, "*localText.json", SearchOption.TopDirectoryOnly) : new string[0]))
                foreach (var t in JArray.Parse(File.ReadAllText(f)))
                    keys.Add((string)t["key"]);
            var used = Directory.GetFiles(ModCode, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
                .SelectMany(f => File.ReadAllLines(f).Where(l => !l.TrimStart().StartsWith("//"))
                    .SelectMany(l => Regex.Matches(l, @"GameTool\.LS\(""([^""{}]+)""\)").Cast<Match>().Select(m => (File: Path.GetFileName(f), Key: m.Groups[1].Value))))
                .ToList();
            Check.True(used.Count > 50, "LS calls found (> 50)", used.Count);
            Check.Info($"{keys.Count} LocalText keys, {used.Select(x => x.Key).Distinct().Count()} distinct keys used in code");
            var missing = used.Where(x => !keys.Contains(x.Key)).Select(x => $"{x.File}: {x.Key}").Distinct()
                .Where(x => !KnownIssues.MissingLocalTextKeys.Contains(x)).ToList();   // known: BUG-08
            Check.None(missing, "keys missing in *_localText.json");
        }

        [TestMethod, Description("LocalText ids and keys are unique across all *_localText.json files")]
        public void LocalTextIds_AreUnique()
        {
            var all = Directory.GetFiles(ModConf, "*localText.json", SearchOption.TopDirectoryOnly)
                .SelectMany(f => JArray.Parse(File.ReadAllText(f)).Select(t => (File: Path.GetFileName(f), Id: (string)t["id"], Key: (string)t["key"])))
                .ToList();
            var dupIds = all.GroupBy(x => x.Id).Where(g => g.Count() > 1).Select(g => $"id {g.Key}: {string.Join(", ", g.Select(x => $"{x.File}/{x.Key}"))}").ToList();
            var dupKeys = all.GroupBy(x => x.Key).Where(g => g.Count() > 1).Select(g => $"key {g.Key}: {string.Join(", ", g.Select(x => x.File))}").ToList();
            Check.None(dupIds.Concat(dupKeys).ToList(), "duplicated LocalText ids/keys", all.Count);
        }

        [TestMethod, Description("tests/scenarios.json auto/watch ids = scenario classes (AutoTest/Scenarios); kinds and triggers match")]
        public void Scenarios_JsonMatchesScenarioClasses()
        {
            var defs = JObject.Parse(File.ReadAllText(TestSetup.Repo("3161035078", "tests", "scenarios.json")))["scenarios"]
                .Select(x => (Id: (string)x["id"], Kind: (string)x["kind"])).ToList();
            Check.Eq(defs.Count, defs.Select(x => x.Id).Distinct().Count(), "scenario ids are unique (distinct count = count)");
            // Create() of every definition must not touch game (Il2Cpp) types: it runs before the steps
            var failed = new List<string>();
            foreach (var t in typeof(AutoTestScenarios).Assembly.GetLoadableTypes().Where(t => t.IsClass && !t.IsAbstract && typeof(IScenarioDefinition).IsAssignableFrom(t)))
            {
                try { ((IScenarioDefinition)Activator.CreateInstance(t)).Create(); }
                catch (Exception ex) { failed.Add($"{t.Name}.Create(): {ex.GetType().Name}: {ex.Message}"); }
            }
            Check.None(failed, "scenario definitions whose Create() fails outside the game");
            var scenarios = AutoTestScenarios.Create();   // also checks class Id == scenario Id and no duplicates
            var codeIds = scenarios.Select(x => x.Id).ToList();
            var jsonAuto = defs.Where(x => x.Kind != "manual").Select(x => x.Id).ToList();
            Check.Info($"scenarios.json: {defs.Count} ids ({jsonAuto.Count} auto/watch, {defs.Count - jsonAuto.Count} manual); classes: {codeIds.Count}; run order: {string.Join(" ", codeIds)}");
            Check.SetEq(jsonAuto, codeIds, "auto/watch ids in scenarios.json = scenario classes");
            foreach (var s in scenarios)
            {
                Assert.IsTrue(s.Steps.Count > 0, $"{s.Id} has steps");
                var kind = defs.First(x => x.Id == s.Id).Kind;
                Assert.AreEqual(kind == "watch", s.Kind == ScenarioKind.Watch, $"{s.Id} kind");
                Assert.AreEqual(kind == "watch", s.Trigger != null, $"{s.Id} trigger");
            }
        }
    }
}
