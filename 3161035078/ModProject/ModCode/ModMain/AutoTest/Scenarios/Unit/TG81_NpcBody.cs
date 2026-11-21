using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Unit
{
    /// <summary>
    /// TG-81 (BodyCulEvent, NPCs): the body refining sects are chosen by their style (the members who fight the most with
    /// bare hands); an NPC of grade 3+ at the Copper Skin bottleneck survives a body tribulation (Iron Bone, tempering of a
    /// x2 dose), goes to the lava pool for a year and comes back with the exp of the place. The NPC's body data restored.
    /// </summary>
    public class TG81_NpcBody : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-81";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-81", Feature = "BodyCulEvent", Title = "Body refining of the NPCs: sects by style, tribulation, tempering place", Overlay = false }
                .Step("sects by style", c =>
                {
                    if (!c.Assert(BodyCulEvent.Enabled, "BodyCulEvent loaded")) return;
                    BodyCulEvent.RefreshBodySects();
                    var sects = BodyCulEvent.Instance.BodySects;
                    c.Assert(sects.Count > 0, $"{sects.Count} body refining sects");
                    var units = ModMain.Store.WUnits.Where(x => x != null && !x.isDie && !string.IsNullOrEmpty(x.data?.school?.schoolNameID)).ToList();
                    var shares = units.GroupBy(x => x.data.school.schoolNameID).Where(x => x.Count() >= BodyCulEvent.SECT_MIN_MEMBERS)
                        .ToDictionary(x => x.Key, x => x.Average(y => BodyCulEvent.HandShare(y)));
                    if (sects.Count > 0 && shares.Count > sects.Count)
                    {
                        var minBody = sects.Min(x => shares[x]);
                        var maxOther = shares.Where(x => !sects.Contains(x.Key)).Max(x => x.Value);
                        c.Assert(minBody >= maxOther, $"the body sects fight the most with bare hands ({minBody:0.000} >= {maxOther:0.000})");
                        c.Info(string.Join(", ", sects.Select(x => $"{units.First(u => u.data.school.schoolNameID == x).data.school.name} {shares[x]:0.000}")));
                    }
                    var member = units.FirstOrDefault(x => BodyCulEvent.IsBodySect(x.data.school.schoolNameID));
                    if (member != null)
                        c.Assert(BodyCulEvent.InBodySect(member), $"{member.GetName()} is in a body refining sect");
                })
                .Step("NPC body tribulation", c =>
                {
                    var npc = ModMain.Store.WUnits.Where(x => x != null && !x.isDie && !x.IsPlayer() && x.GetGradeLvl() >= BodyCulEvent.RealmGrade(4))
                        .OrderBy(x => x.GetGradeLvl()).FirstOrDefault();
                    if (!c.Assert(npc != null, "an NPC of grade 3 or more")) return;
                    c.Bag["npc"] = npc.GetUnitId();
                    c.Bag["body"] = BodyCulEvent.Instance.Bodies.TryGetValue(npc.GetUnitId(), out var old) ? Newtonsoft.Json.JsonConvert.SerializeObject(old) : null;
                    BodyCulEvent.Instance.Bodies.Remove(npc.GetUnitId());
                    var d = BodyCulEvent.Start(npc);
                    d.Level = 3;
                    d.Exp = BodyCulEvent.NeedExp(3);
                    var rs = BodyCulEvent.NpcBreak(npc, d, true, 0f);
                    c.AssertEqual(BodyCulEvent.BreakResult.Success, rs, $"{npc.GetName()} survived the tribulation");
                    c.AssertEqual(4, d.Level, "Iron Bone - Early");
                    c.AssertEqual(0.2, System.Math.Round(d.Temper, 2), "tempering of a x2 dose");
                    d.Level = 3; d.Exp = BodyCulEvent.NeedExp(3); d.Temper = 0;
                    rs = BodyCulEvent.NpcBreak(npc, d, true, 99.9f);
                    c.AssertEqual(BodyCulEvent.BreakResult.Failure, rs, "a bad roll: the tribulation fails");
                    c.AssertEqual(3, d.InjuryMonths, "injured 3 months");
                })
                .Step("NPC tempering place", c =>
                {
                    var npc = c.Bag.ContainsKey("npc") ? g.world.unit.GetUnit((string)c.Bag["npc"]) : null;
                    var d = BodyCulEvent.Get(npc);
                    if (d == null) return;
                    d.Level = 4; d.Exp = 0; d.InjuryMonths = 0;
                    var p = BodyCulEvent.NpcGoToPlace(npc, d, 0);
                    c.AssertEqual(BodyCulEvent.Place.Lava, p, "Iron Bone: the lava pool (the best it can stand)");
                    c.AssertEqual(12, d.PlaceMonths, "one year");
                    var expect = BodyCulEvent.PlaceTotalExp(BodyCulEvent.Place.Lava, 4, 12);
                    c.Assert(BodyCulEvent.NpcAtPlace(npc, d, 12), "the NPC stays at the place");
                    c.Assert(d.Level > 4 || d.Exp == expect, $"the exp of a year of lava ({expect}): level {d.Level}, exp {d.Exp}");
                    c.AssertEqual(0, d.PlaceMonths, "back from the place");
                    c.AssertEqual(0, d.PlaceKind, "no place");
                })
                .Step("restore", c =>
                {
                    if (!c.Bag.ContainsKey("npc")) return;
                    var id = (string)c.Bag["npc"];
                    BodyCulEvent.Instance.Bodies.Remove(id);
                    if (c.Bag["body"] != null)
                        BodyCulEvent.Instance.Bodies[id] = Newtonsoft.Json.JsonConvert.DeserializeObject<BodyCulEvent.BodyData>((string)c.Bag["body"]);
                });
        }
    }
}
