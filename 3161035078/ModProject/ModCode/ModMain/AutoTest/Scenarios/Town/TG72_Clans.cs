using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-72 (ClanEvent): two married NPCs settle in a town (a house each): two clans; the town map has a "Clans" button,
    /// the window lists them and their relation (a blood feud set for the test); a brawl (the loser's fighter hurt, the
    /// relation falls), the player asked to step in (drama, stays out), a clan war (the losers die, lose their house, the
    /// clan is wiped out, Taoist log). Changes the save (NPCs die; restored by run-autotest.ps1); estate, relations, money
    /// and places restored.
    /// </summary>
    public class TG72_Clans : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-72";

        /// <summary>Rolls of a test: the values in order, then 0.99 (nothing happens).</summary>
        private static Func<double> Rolls(params double[] v)
        {
            var i = 0;
            return () => i < v.Length ? v[i++] : 0.99;
        }

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            MapBuildTown town = null;
            WorldUnitBase a = null, b = null;
            return new Scenario { Id = "TG-72", Feature = "ClanEvent", Title = "Cultivator clans: relations, brawls, clan war", Overlay = false }
                .Step("two clans", c =>
                {
                    if (!TravelTo(c, FindTown(t => t.IsSmallTown()) ?? FindTown(t => true))) return;
                    town = BagTown(c);
                    var e = EstateEvent.Get(town);
                    c.Bag["estate"] = JsonConvert.SerializeObject(e);
                    c.Bag["budget"] = MapBuildPropertyEvent.GetBuildProperty(town);
                    c.Bag["bases"] = JsonConvert.SerializeObject(ClanEvent.Instance.Bases);
                    // two married NPCs (a clan needs a family), not of the player's family, without a house
                    var heads = Npcs.Where(x => !HirePeopleEvent.IsHired(x) && !FamilyEvent.IsFamilyOfPlayer(x) && !EstateEvent.HousesOf(x).Any())
                        .Where(x => ClanEvent.Members(x).Count >= 2 && ClanEvent.Members(x).All(m => !m.IsPlayer())).ToList();
                    a = heads.FirstOrDefault();
                    b = heads.FirstOrDefault(x => a != null && ClanEvent.Members(a).All(m => m.GetUnitId() != x.GetUnitId()) && ClanEvent.Members(x).All(m => m.GetUnitId() != a.GetUnitId()));
                    if (!c.Assert(a != null && b != null, $"two NPCs with a family ({heads.Count} found)")) return;
                    var saved = new Dictionary<string, object>();
                    foreach (var h in new[] { a, b })
                    {
                        saved[h.GetUnitId()] = new object[] { h.GetUnitPos(), h.GetUnitMoney() };
                        h.SetUnitPos(town.GetOrigiPoint());
                        h.SetUnitMoney(int.MaxValue / 2);
                        c.AssertEqual("buy", EstateEvent.NpcHouse(h, 1, () => 0, town), $"{h.GetName()} settles in {town.name} ({ClanEvent.Members(h).Count} members)");
                    }
                    c.Bag["heads"] = saved;
                    var list = ClanEvent.Heads(town);
                    c.Assert(list.Contains(a.GetUnitId()) && list.Contains(b.GetUnitId()), $"two clans in the town ({list.Count})");
                    // a blood feud for the test (the base; the members' intimacy and marriages add to it)
                    ClanEvent.Instance.Bases[ClanEvent.Key(town.buildData.id, a.GetUnitId(), b.GetUnitId())] = -100;
                    var rel = ClanEvent.Relation(town, a, b);
                    c.Assert(rel <= ClanEvent.Conf.WarAt, $"relation {rel}: {GameTool.LS(ClanEvent.RelationKey(rel))}");
                    EstateEvent.OpenPlots(town);
                    c.Assert(ScreenTexts().Any(x => x == string.Format(GameTool.LS("clan_button"), list.Count)), "a Clans button on the town map");
                }, 1500)
                .Step("clans window", c =>
                {
                    if (a == null || b == null) return;
                    CloseAll(UIType.TextInfoLong);
                    ClanEvent.OpenClans(town);
                    var texts = ScreenTexts();
                    c.Assert(texts.Any(x => x.StartsWith(ClanEvent.ClanName(a))) && texts.Any(x => x.StartsWith(ClanEvent.ClanName(b))), "both clans listed");
                    c.Assert(texts.Any(x => x.Contains(GameTool.LS("clan_rel_feud"))), "their blood feud shown");
                }, 1500)
                .Step("a brawl", c =>
                {
                    if (a == null || b == null) return;
                    CloseAll(UIType.TextInfoLong);
                    var fa = ClanEvent.Fighter(a, false); var fb = ClanEvent.Fighter(b, false);
                    c.Info($"fighters: {fa.GetName()} ({ClanEvent.ClanName(a)}) - {fb.GetName()} ({ClanEvent.ClanName(b)})");
                    var base0 = ClanEvent.Base(town.buildData.id, a.GetUnitId(), b.GetUnitId());
                    ClanEvent.Instance.Bases[ClanEvent.Key(town.buildData.id, a.GetUnitId(), b.GetUnitId())] = -50;
                    var intim = fb.data.unitData.relationData.GetIntim(fa.GetUnitId());
                    // A wins (roll 0), nobody dies (0.99)
                    ClanEvent.Resolve(town, a, b, fa, fb, false, true, Rolls(0.99));
                    c.Assert(!fb.isDie && fb.GetProperty<int>(UnitPropertyEnum.Hp) <= fb.GetDynProperty(UnitDynPropertyEnum.HpMax).value / 5, $"{fb.GetName()} beaten, hurt");
                    c.AssertEqual(-50 + ClanEvent.Conf.BrawlRelation, ClanEvent.Base(town.buildData.id, a.GetUnitId(), b.GetUnitId()), "the base relation falls");
                    c.Assert(fb.data.unitData.relationData.GetIntim(fa.GetUnitId()) < intim, "the two fighters hate each other more");
                    c.Info($"base before the test {base0}");
                })
                .Step("the player is asked", c =>
                {
                    if (a == null || b == null) return;
                    // as if A were on the player's side: the drama asks to step in
                    ClanEvent.Ask(town, new ClanEvent.Fight
                    {
                        TownId = town.buildData.id, War = true, Ours = a.GetUnitId(), Theirs = b.GetUnitId(),
                        OurFighter = ClanEvent.Fighter(a, true).GetUnitId(), TheirFighter = ClanEvent.Fighter(b, true).GetUnitId(),
                    });
                }, 1500)
                .Step("stays out", c =>
                {
                    if (a == null || b == null) return;
                    c.Assert(DramaUI() != null && ClanEvent.Instance.Pending != null, "the drama asks the player");
                    c.Assert(DramaOptions().Any(x => x.Item2 == GameTool.LS("clan_step_in")) && DramaOptions().Any(x => x.Item2 == GameTool.LS("clan_stay_out")), "step in / stay out");
                    DismissDramas();
                    ClanEvent.Instance.Pending = null;   // the war of the test is decided below
                })
                .Step("clan war", c =>
                {
                    if (a == null || b == null) return;
                    var e = EstateEvent.Get(town);
                    var losers = ClanEvent.Members(b);
                    var logs = TaoistLogEvent.Instance.Entries.Count;
                    // A wins; every loser dies (0), no winner dies (0.99)
                    ClanEvent.Resolve(town, a, b, ClanEvent.Fighter(a, true), ClanEvent.Fighter(b, true), true, true,
                        Rolls(losers.Select(_ => 0.0).ToArray()));
                    c.Assert(losers.All(x => x.isDie), $"{losers.Count} member(s) of the losers fell");
                    c.Assert(ClanEvent.Members(a).Count > 0, "the winners stand");
                    c.Assert(!e.Plots.Any(x => x.OwnerId == b.GetUnitId()), "their house went to the mortals");
                    c.AssertEqual(0, ClanEvent.Members(b).Count, "the clan is wiped out");
                    var keys = TaoistLogEvent.Instance.Entries.Skip(logs).Select(x => x.Key).ToList();
                    c.Assert(keys.Contains("log_clan_war") && keys.Contains("log_clan_extinct"), $"Taoist log: {string.Join(", ", keys)}");
                    c.Assert(!ClanEvent.Heads(town).Contains(b.GetUnitId()), "one clan left in the town");
                    TaoistLogEvent.Instance.OpenLog();
                }, 1500)
                .Step("restore", c =>
                {
                    DismissDramas();
                    CloseAll(UIType.TextInfoLong, UIType.CheckPopup);
                    if (town == null) return;
                    EstateEvent.Instance.Towns[town.buildData.id] = JsonConvert.DeserializeObject<EstateEvent.TownEstate>((string)c.Bag["estate"]);
                    MapBuildPropertyEvent.AddBuildProperty(town, (long)c.Bag["budget"] - MapBuildPropertyEvent.GetBuildProperty(town));
                    ClanEvent.Instance.Bases = JsonConvert.DeserializeObject<Dictionary<string, int>>((string)c.Bag["bases"]);
                    if (c.Bag.ContainsKey("heads"))
                        foreach (var kv in (Dictionary<string, object>)c.Bag["heads"])
                        {
                            var u = FamilyEvent.Unit(kv.Key);
                            var v = (object[])kv.Value;
                            if (u == null || u.isDie) continue;
                            u.SetUnitPos((UnityEngine.Vector2Int)v[0]);
                            u.SetUnitMoney((int)v[1]);
                        }
                    c.AssertEqual((long)c.Bag["budget"], MapBuildPropertyEvent.GetBuildProperty(town), "budget restored");
                });
        }
    }
}
