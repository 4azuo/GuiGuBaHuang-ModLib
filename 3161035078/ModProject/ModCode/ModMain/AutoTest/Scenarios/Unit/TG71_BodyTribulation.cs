using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using MOD_nE7UL2.Enum;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Unit
{
    /// <summary>
    /// TG-71 (BodyCulEvent, extensions): a year at the waterfall (an encounter of the world map, seclusion reward of a tempering place) gives the body
    /// exp of the place month by month; the body tribulation (trial of lightning) of a Copper Skin body at its bottleneck:
    /// the materials of a x1 breakthrough are used, the MP stays under 20 %, surviving breaks into Iron Bone with the
    /// tempering of a x2 dose and no trial properties. Grade and body data restored.
    /// </summary>
    public class TG71_BodyTribulation : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-71";

        private static void SetGrade(int grade)
        {
            var conf = g.conf.roleGrade._allConfList.ToArray().Where(x => x.grade == grade).OrderBy(x => x.id).FirstOrDefault();
            if (conf != null) Player.SetProperty<int>(UnitPropertyEnum.GradeID, conf.id);
        }

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-71", Feature = "BodyCulEvent", Title = "Body refining: tempering place and body tribulation", Overlay = false }
                .Step("a year at the waterfall", c =>
                {
                    if (!c.Assert(BodyCulEvent.Enabled, "BodyCulEvent loaded")) return;
                    var id = Player.GetUnitId();
                    c.Bag["grade"] = Player.GetProperty<int>(UnitPropertyEnum.GradeID);
                    c.Bag["body"] = BodyCulEvent.Instance.Bodies.TryGetValue(id, out var old) ? Newtonsoft.Json.JsonConvert.SerializeObject(old) : null;
                    BodyCulEvent.Instance.Bodies.Remove(id);
                    var d = BodyCulEvent.Start(Player);
                    var expect = BodyCulEvent.PlaceTotalExp(BodyCulEvent.Place.Waterfall, 1, 12);
                    var text = SeclusionEvent.Reward(new SeclusionEvent.Running { Kind = SeclusionEvent.Kind.Waterfall, Months = 12 });
                    c.Info(text);
                    c.AssertEqual(expect, d.Exp, $"a year at the waterfall: {expect} body exp (level 1)");
                    c.Assert(!string.IsNullOrEmpty(text) && !text.Contains("secl_res_"), "result text of the waterfall");
                    c.AssertEqual(SeclusionEvent.Kind.Lava, SeclusionEvent.KindOf(BodyCulEvent.Place.Lava), "lava pool seclusion kind");
                    // the places are encounters of the world map: Copper Skin stands the waterfall only
                    var places = BodyCulEvent.PlacesFor(Player);
                    c.Assert(places.Count == 1 && places[0] == BodyCulEvent.Place.Waterfall, $"Copper Skin: waterfall encounter only ({string.Join(", ", places)})");
                    c.AssertEqual(EncounterEvent.Encounter.Thunder, EncounterEvent.EncounterOf(BodyCulEvent.Place.Thunder), "thunder pool encounter");
                    if (Player.GetMapBuild<MapBuildTown>() == null)
                        c.Assert(EncounterEvent.Possible().Contains(EncounterEvent.Encounter.Waterfall), "outside towns the waterfall is a possible encounter");
                })
                .Step("enter the tribulation", c =>
                {
                    var d = BodyCulEvent.Get(Player);
                    if (d == null) return;
                    d.Level = 3;
                    d.Exp = BodyCulEvent.NeedExp(3);
                    d.InjuryMonths = 0;
                    d.Temper = 0;
                    SetGrade(BodyCulEvent.RealmGrade(4));
                    c.Assert(BodyCulEvent.IsBottleneck(d), "Copper Skin - Late at its bottleneck");
                    foreach (var n in BodyCulEvent.BreakNeeds(3))
                    {
                        var item = BodyCulEvent.SampleId(Player, n);
                        if (item != 0) Player.AddUnitProp(item, n.Count);
                    }
                    c.Bag["trial"] = UnitTypeEnum.Trial.PropIncRatio.Select(p => p.Values[0] as UnitPropertyEnum).Where(p => p != null)
                        .ToDictionary(p => p, p => Player.GetProperty<int>(p));
                    var rs = BodyCulEvent.StartTribulation(Player);
                    c.AssertEqual(BodyCulEvent.BreakResult.Success, rs, "the tribulation starts (materials used)");
                    c.AssertEqual(3, BodyCulEvent.Instance.TribulationLevel, "tribulation from level 3 under way");
                })
                .Wait("battle loaded", c => { StartBattle(); return InBattle; }, 90000, 1000)
                .Wait("battle started", c => StartBattle(), 60000, 1000)
                .Step("MP held down", c =>
                {
                    if (!InBattle) return;
                    c.Assert(BodyCulEvent.InTribulation, $"battle is the body tribulation (dungeon {g.world.battle.data.dungeonBaseItem.id})");
                    var h = ModBattleEvent.PlayerUnit?.data?.TryCast<UnitDataHuman>();
                    if (h != null)
                        c.Assert(h.mp <= (int)(h.maxMP.value * BodyCulEvent.TRIBULATION_MP), $"MP {h.mp} <= 20 % of {h.maxMP.value}");
                }, 2000)
                .Step("survive", c => { if (InBattle) EndBattle(true); }, 1500)
                .Wait("back to the map", c => LeaveBattle(), 90000, 2000)
                .Wait("result shown", c => BodyCulEvent.PendingMessage == null, 15000, 500)
                .Step("into Iron Bone", c =>
                {
                    var d = BodyCulEvent.Get(Player);
                    if (!c.Assert(d != null, "still a body refiner")) return;
                    c.AssertEqual(4, d.Level, "Iron Bone - Early");
                    c.AssertEqual(0.2, System.Math.Round(d.Temper, 2), "tempering of a x2 dose");
                    c.AssertEqual(0, BodyCulEvent.Instance.TribulationLevel, "no tribulation under way");
                    c.Assert(BodyCulEvent.LastMessage != null && BodyCulEvent.LastMessage.Contains(BodyCulEvent.RealmName(4)), $"the result shown on the map: {BodyCulEvent.LastMessage}");
                    c.Assert(TaoistLogEvent.Instance.Entries.Any(x => x.Key == "log_body_trib_start"), "Taoist log: tribulation");
                    var before = (System.Collections.Generic.Dictionary<UnitPropertyEnum, int>)c.Bag["trial"];
                    c.Assert(before.All(p => Player.GetProperty<int>(p.Key) == p.Value), "no trial of lightning properties");
                }, 3000)
                .Step("restore", c =>
                {
                    if (g.ui.HasUI(UIType.TextInfo)) g.ui.CloseUI(UIType.TextInfo);
                    Player.SetProperty<int>(UnitPropertyEnum.GradeID, (int)c.Bag["grade"]);
                    var id = Player.GetUnitId();
                    BodyCulEvent.Instance.Bodies.Remove(id);
                    if (c.Bag["body"] != null)
                        BodyCulEvent.Instance.Bodies[id] = Newtonsoft.Json.JsonConvert.DeserializeObject<BodyCulEvent.BodyData>((string)c.Bag["body"]);
                    UnitModifyEvent.Instance.CachedValues.Clear();
                });
        }
    }
}
