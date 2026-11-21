using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
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
    /// TG-74 (ClanEvent in depth): the relation follows the intimacy of the members and a marriage between the clans;
    /// a year of the town: no fight between neutral clans, a brawl (not a war) between hostile ones, a war in a feud; the
    /// player's clan (a house and a spouse): the player is asked, steps in, the real battle against the enemy's fighter is
    /// won, the fight is decided for the player's side; the player never dies in a lost war. Changes the save (marriages,
    /// deaths: restored by run-autotest.ps1); estate, relations, money and places restored.
    /// </summary>
    public class TG74_ClanDepth : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-74";

        private static Func<double> Rolls(params double[] v)
        {
            var i = 0;
            return () => i < v.Length ? v[i++] : 0.99;
        }

        private static string Key(MapBuildTown t, WorldUnitBase a, WorldUnitBase b) => ClanEvent.Key(t.buildData.id, a.GetUnitId(), b.GetUnitId());

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            MapBuildTown town = null;
            WorldUnitBase a = null, b = null, foe = null;
            return new Scenario { Id = "TG-74", Feature = "ClanEvent", Title = "Clans in depth: relations, yearly fights, the player steps in", Overlay = false }
                .Step("clans", c =>
                {
                    if (!TravelTo(c, FindTown(t => t.IsSmallTown()) ?? FindTown(t => true))) return;
                    town = BagTown(c);
                    c.Bag["estate"] = JsonConvert.SerializeObject(EstateEvent.Get(town));
                    c.Bag["budget"] = MapBuildPropertyEvent.GetBuildProperty(town);
                    c.Bag["bases"] = JsonConvert.SerializeObject(ClanEvent.Instance.Bases);
                    var heads = Npcs.Where(x => !HirePeopleEvent.IsHired(x) && !FamilyEvent.IsFamilyOfPlayer(x) && !EstateEvent.HousesOf(x).Any())
                        .Where(x => ClanEvent.Members(x).Count >= 2 && ClanEvent.Members(x).All(m => !m.IsPlayer())).ToList();
                    var picked = new List<WorldUnitBase>();
                    foreach (var h in heads)
                    {
                        if (picked.Count == 3) break;
                        var ids = ClanEvent.Members(h).Select(x => x.GetUnitId()).ToList();
                        if (picked.Any(p => ClanEvent.Members(p).Any(m => ids.Contains(m.GetUnitId())))) continue;
                        picked.Add(h);
                    }
                    if (!c.Assert(picked.Count == 3, $"three NPC families ({picked.Count})")) return;
                    a = picked[0]; b = picked[1]; foe = picked[2];
                    var saved = new Dictionary<string, object[]>();
                    foreach (var h in picked)
                    {
                        saved[h.GetUnitId()] = new object[] { h.GetUnitPos(), h.GetUnitMoney() };
                        h.SetUnitPos(town.GetOrigiPoint());
                        h.SetUnitMoney(int.MaxValue / 2);
                        c.AssertEqual("buy", EstateEvent.NpcHouse(h, 1, () => 0, town), $"{ClanEvent.ClanName(h)} settles ({ClanEvent.Members(h).Count} members)");
                    }
                    c.Bag["units"] = saved;
                })
                .Step("relation and members", c =>
                {
                    if (a == null) return;
                    ClanEvent.Instance.Bases[Key(town, a, b)] = 0;
                    var r0 = ClanEvent.Relation(town, a, b);
                    // every member of A and B: intimacy +60 both ways -> the relation + about 30
                    foreach (var x in ClanEvent.Members(a))
                        foreach (var y in ClanEvent.Members(b))
                        {
                            FamilyEvent.ChangeIntim(x.data.unitData.relationData, y.GetUnitId(), 60);
                            FamilyEvent.ChangeIntim(y.data.unitData.relationData, x.GetUnitId(), 60);
                        }
                    var r1 = ClanEvent.Relation(town, a, b);
                    c.Assert(r1 - r0 >= 20, $"friendship between the members: relation {r0} -> {r1}");
                    // a marriage between the clans: + MarriageBonus
                    var single = ClanEvent.Members(a).Concat(ClanEvent.Members(b)).Where(x => string.IsNullOrEmpty(FamilyEvent.Spouse(x))).ToList();
                    var x1 = single.FirstOrDefault(x => ClanEvent.Members(a).Contains(x));
                    var y1 = single.FirstOrDefault(x => ClanEvent.Members(b).Contains(x) && x1 != null && FamilyEvent.IsMan(x) != FamilyEvent.IsMan(x1));
                    if (x1 != null && y1 != null)
                    {
                        var before = ClanEvent.Relation(town, a, b);
                        FamilyEvent.SetMarried(x1, y1);
                        var after = ClanEvent.Relation(town, a, b);
                        c.Assert(after >= Math.Min(100, before + ClanEvent.Conf.MarriageBonus - 5), $"{x1.GetName()} marries {y1.GetName()}: relation {before} -> {after}");
                    }
                    else c.Info("no single members to marry");
                    c.Info($"relation A-B {ClanEvent.Relation(town, a, b)} ({GameTool.LS(ClanEvent.RelationKey(ClanEvent.Relation(town, a, b)))})");
                })
                .Step("a year of the town", c =>
                {
                    if (a == null) return;
                    // A-B allied, A-foe neutral, B-foe hostile: a sure roll -> only B and the foe brawl
                    SetRelation(town, a, b, 100);
                    SetRelation(town, a, foe, 0);
                    SetRelation(town, b, foe, ClanEvent.Conf.HostileAt - 10);
                    c.Info($"relations: A-B {ClanEvent.Relation(town, a, b)}, A-foe {ClanEvent.Relation(town, a, foe)}, B-foe {ClanEvent.Relation(town, b, foe)}");
                    var baseAB = ClanEvent.Base(town.buildData.id, a.GetUnitId(), b.GetUnitId());
                    var baseAF = ClanEvent.Base(town.buildData.id, a.GetUnitId(), foe.GetUnitId());
                    var baseBF = ClanEvent.Base(town.buildData.id, b.GetUnitId(), foe.GetUnitId());
                    var logs = TaoistLogEvent.Instance.Entries.Count;
                    var fights = ClanEvent.YearTown(town, () => 0);
                    c.AssertEqual(baseAB, ClanEvent.Base(town.buildData.id, a.GetUnitId(), b.GetUnitId()), "allied clans: no fight");
                    c.AssertEqual(baseAF, ClanEvent.Base(town.buildData.id, a.GetUnitId(), foe.GetUnitId()), "neutral clans: no fight");
                    c.Assert(ClanEvent.Base(town.buildData.id, b.GetUnitId(), foe.GetUnitId()) < baseBF, "hostile clans brawled");
                    c.AssertEqual(1, fights, "one fight in the year");
                    c.Assert(EstateEvent.HouseOf(town, b) != null && EstateEvent.HouseOf(town, foe) != null, "a brawl takes no house");
                    // no chance hit: nothing
                    c.AssertEqual(0, ClanEvent.YearTown(town, () => 0.99), "no fight without the yearly chance");
                    c.Info($"log: {string.Join(", ", TaoistLogEvent.Instance.Entries.Skip(logs).Select(x => x.Key))}");
                })
                .Step("the player's clan", c =>
                {
                    if (a == null) return;
                    // the player: a house in the town and a spouse (a clan); the foe hates it
                    if (string.IsNullOrEmpty(FamilyEvent.Spouse(Player)))
                    {
                        var mate = Npcs.FirstOrDefault(x => string.IsNullOrEmpty(FamilyEvent.Spouse(x)) && !HirePeopleEvent.IsHired(x) && FamilyEvent.IsMan(x) != FamilyEvent.IsMan(Player)
                            && !ClanEvent.Members(foe).Contains(x) && !ClanEvent.Members(a).Contains(x) && !ClanEvent.Members(b).Contains(x));
                        if (mate != null) FamilyEvent.SetMarried(Player, mate);
                    }
                    var e = EstateEvent.Get(town);
                    if (EstateEvent.HouseOf(town, Player) == null)
                    {
                        var plot = e.Plots.First(x => x.OwnerId == null && x.Level == 0);
                        plot.OwnerId = Player.GetUnitId(); plot.Level = 2;
                    }
                    c.Assert(ClanEvent.Heads(town).Contains(Player.GetUnitId()), $"the player's family is a clan ({ClanEvent.Members(Player).Count} members)");
                    c.Assert(ClanEvent.OnPlayersSide(town, Player), "its own side");
                    ClanEvent.Instance.Bases[Key(town, Player, a)] = 100;
                    c.Assert(ClanEvent.OnPlayersSide(town, a), $"an ally: {ClanEvent.ClanName(a)} ({ClanEvent.Relation(town, Player, a)})");
                    ClanEvent.Instance.Bases[Key(town, Player, foe)] = -100;
                    // the foe picks a fight with the ally: the player is asked
                    ClanEvent.Instance.Pending = null;
                    ClanEvent.Start(town, foe, a, false, () => 0);
                    c.Assert(ClanEvent.Instance.Pending != null && ClanEvent.Instance.Pending.Ours == a.GetUnitId(), "the player is asked to help its ally");
                }, 1500)
                .Step("steps in", c =>
                {
                    if (ClanEvent.Instance.Pending == null) return;
                    c.Assert(DramaUI() != null, "the drama");
                    DismissDramas();
                    c.Bag["base"] = ClanEvent.Base(town.buildData.id, a.GetUnitId(), foe.GetUnitId());
                    c.Bag["their"] = ClanEvent.Instance.Pending.TheirFighter;
                    ClanEvent.OnAnswer(0);
                })
                .Wait("battle loaded", c => { StartBattle(); ProtectPlayer(); return InBattle && ModBattleEvent.BattleUnits.ToArray().Any(x => x.IsWorldUnit() && !x.IsPlayer()); }, 90000, 1000)
                .Wait("battle started", c => { ProtectPlayer(); return StartBattle(); }, 60000, 1000)
                .Step("defeat the fighter", c =>
                {
                    if (!InBattle) return;
                    ProtectPlayer();
                    var enemy = ModBattleEvent.BattleUnits.ToArray().FirstOrDefault(x => x.IsWorldUnit() && !x.IsPlayer());
                    c.Assert(enemy?.GetWorldUnit()?.GetUnitId() == (string)c.Bag["their"], $"the enemy's fighter in the battle: {enemy?.GetWorldUnit()?.GetName()}");
                    for (var i = 0; i < 10 && enemy != null && enemy.data.hp > 1 && !enemy.isDie; i++)
                        PlayerHit(enemy, (enemy.data.hp + enemy.data.maxHP.value) * 3);
                }, 2000)
                .Step("win", c => { if (InBattle) EndBattle(true); }, 1500)
                .Wait("back to the map", c => LeaveBattle(), 90000, 2000)
                .Step("decided", c =>
                {
                    if (!c.Bag.ContainsKey("base")) return;
                    c.Assert(OnMap, "back on the map");
                    c.Assert(ClanEvent.Instance.Pending == null, "the fight is decided after the battle");
                    c.Assert(ClanEvent.Base(town.buildData.id, a.GetUnitId(), foe.GetUnitId()) < (int)c.Bag["base"], "the brawl counted (the base relation falls)");
                    var their = FamilyEvent.Unit((string)c.Bag["their"]);
                    c.Assert(their == null || their.isDie || their.GetProperty<int>(UnitPropertyEnum.Hp) <= their.GetDynProperty(UnitDynPropertyEnum.HpMax).value / 5 + 1, "the foe's fighter lost (hurt or dead)");
                }, 1500)
                .Step("the player never dies", c =>
                {
                    if (a == null) return;
                    CloseAll(UIType.CheckPopup);
                    // the player's clan loses a war in which every loser dies: not the player
                    var members = ClanEvent.Members(Player);
                    ClanEvent.Resolve(town, foe, Player, ClanEvent.Fighter(foe, true), ClanEvent.Fighter(Player, true) ?? Player, true, true, Rolls(members.Where(x => !x.IsPlayer()).Select(_ => 0.0).ToArray()));
                    c.Assert(!Player.isDie, "the player stands");
                    c.Assert(members.Where(x => !x.IsPlayer()).All(x => x.isDie), "the other members fell");
                    c.Assert(EstateEvent.HouseOf(town, Player) == null, "the player's house is lost with the war");
                })
                .Step("restore", c =>
                {
                    DismissDramas();
                    CloseAll(UIType.TextInfoLong, UIType.CheckPopup);
                    ClanEvent.Instance.Pending = null;
                    if (town == null) return;
                    EstateEvent.Instance.Towns[town.buildData.id] = JsonConvert.DeserializeObject<EstateEvent.TownEstate>((string)c.Bag["estate"]);
                    MapBuildPropertyEvent.AddBuildProperty(town, (long)c.Bag["budget"] - MapBuildPropertyEvent.GetBuildProperty(town));
                    ClanEvent.Instance.Bases = JsonConvert.DeserializeObject<Dictionary<string, int>>((string)c.Bag["bases"]);
                    if (c.Bag.ContainsKey("units"))
                        foreach (var kv in (Dictionary<string, object[]>)c.Bag["units"])
                        {
                            var u = FamilyEvent.Unit(kv.Key);
                            if (u == null || u.isDie) continue;
                            u.SetUnitPos((UnityEngine.Vector2Int)kv.Value[0]);
                            u.SetUnitMoney((int)kv.Value[1]);
                        }
                    c.AssertEqual((long)c.Bag["budget"], MapBuildPropertyEvent.GetBuildProperty(town), "budget restored");
                });
        }

        /// <summary>Sets the base of two clans so that their relation is the target (the members' part kept).</summary>
        private static void SetRelation(MapBuildTown town, WorldUnitBase x, WorldUnitBase y, int target)
        {
            var key = ClanEvent.Key(town.buildData.id, x.GetUnitId(), y.GetUnitId());
            var cur = ClanEvent.Relation(town, x, y);
            ClanEvent.Instance.Bases[key] = Math.Max(-300, Math.Min(300, ClanEvent.Base(town.buildData.id, x.GetUnitId(), y.GetUnitId()) + target - cur));
        }
    }
}
