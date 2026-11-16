using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Time
{
    /// <summary>
    /// TX-08 (FamilyEvent, SeclusionEvent): two NPCs marry, a child born 16 years ago grows up into an NPC (surname,
    /// parents, family window); the player marries and has a grown-up child, who takes over the player (continue as a
    /// descendant); an offered seclusion of 1 year passes the time and ends with the summary drama. Changes the save
    /// (restored by run-autotest.ps1).
    /// </summary>
    public class TX08_FamilySeclusion : IScenarioDefinition
    {
        private static Il2CppSystem.Collections.Generic.Dictionary<string, float> Intim(WorldUnitBase u) => u.data.unitData.relationData.intimToUnit;

        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TX-08";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            WorldUnitBase man = null, woman = null, child = null, mate = null, heir = null, friend = null, enemy = null, heirFriend = null;
            string heirName = null, heirSkillLeft = null, seatTown = null;
            var heirSkills = 0;
            long exp = 0;
            return new Scenario { Id = "TX-08", Feature = "FamilyEvent", Title = "Family, continue as a descendant, seclusion", Overlay = false }
                .Step("npc couple", c =>
                {
                    var F = FamilyEvent.Instance;
                    if (!c.Assert(F != null && SeclusionEvent.Instance != null, "FamilyEvent and SeclusionEvent loaded")) return;
                    var singles = Npcs.Where(x => string.IsNullOrEmpty(FamilyEvent.Spouse(x)) && !HirePeopleEvent.IsHired(x)).ToList();
                    man = singles.FirstOrDefault(FamilyEvent.IsMan);
                    woman = singles.FirstOrDefault(x => !FamilyEvent.IsMan(x));
                    mate = singles.LastOrDefault(x => FamilyEvent.IsMan(x) != FamilyEvent.IsMan(Player) && x != man && x != woman);
                    if (!c.Assert(man != null && woman != null && mate != null, $"singles: {man?.GetName()}, {woman?.GetName()}, mate {mate?.GetName()}")) return;
                    FamilyEvent.SetMarried(man, woman);
                    c.Assert(FamilyEvent.Spouse(man) == woman.GetUnitId() && FamilyEvent.Spouse(woman) == man.GetUnitId(), "married both ways");
                    var born = F.Birth(man, woman, GameHelper.GetGameTotalMonth() - FamilyEvent.ADULT_YEARS * 12);
                    F.OnMonthly();
                    child = FamilyEvent.Unit(born.UnitId);
                    if (!c.Assert(child != null && !child.isDie, $"the child grew up into an NPC: {child?.GetName()}")) return;
                    c.AssertEqual(FamilyEvent.ADULT_YEARS, FamilyEvent.AgeYears(child), "16 years old");
                    c.Assert(child.GetName().StartsWith(man.GetName().Split(' ')[0].Substring(0, 1)), $"surname of the father: {child.GetName()} / {man.GetName()}");
                    var f = FamilyEvent.Family(child);
                    c.Assert(f.Parents.Contains(man.GetUnitId()) && f.Parents.Contains(woman.GetUnitId()), "parents linked");
                    c.Assert(FamilyEvent.Family(man).Children.Contains(child.GetUnitId()), "child in the father's family");
                    FamilyEvent.OpenFamily(man);
                }, 1500)
                .Step("family window", c =>
                {
                    var texts = ScreenTexts();
                    c.Assert(texts.Any(x => x.Contains(child?.GetName() ?? "?")), "the family window lists the child");
                    CloseAll(UIType.TextInfoLong);
                    DismissDramas();
                })
                .Step("player family", c =>
                {
                    if (mate == null) return;
                    FamilyEvent.SetMarried(Player, mate);
                    c.Assert(FamilyEvent.Spouse(Player) == mate.GetUnitId(), $"the player married {mate.GetName()}");
                    var F = FamilyEvent.Instance;
                    var father = FamilyEvent.IsMan(Player) ? Player : mate;
                    var mother = FamilyEvent.IsMan(Player) ? mate : Player;
                    var born = F.Birth(father, mother, GameHelper.GetGameTotalMonth() - FamilyEvent.ADULT_YEARS * 12);
                    DismissDramas();
                    F.OnMonthly();
                    heir = FamilyEvent.Unit(born.UnitId);
                    c.Assert(heir != null && FamilyEvent.Heirs().Any(x => x.GetUnitId() == heir.GetUnitId()), $"heir: {heir?.GetName()}");
                }, 1500)
                .Step("legacy", c =>
                {
                    DismissDramas();
                    if (heir == null) return;
                    heirName = heir.GetName();
                    // legacy to check: a friend (half), an enemy (full), a bounty and a town seat of the old player, the heir's reputation and techniques
                    var pid = Player.GetUnitId();
                    var others = Npcs.Where(x => x != null && !x.isDie && x.GetUnitId() != heir.GetUnitId() && x != mate).ToList();
                    friend = others.First();
                    enemy = others.Skip(1).First();
                    heirFriend = others.Skip(2).First();
                    Intim(heir)[heirFriend.GetUnitId()] = 80f; Intim(heirFriend)[heir.GetUnitId()] = 80f;
                    Intim(Player)[friend.GetUnitId()] = 100f; Intim(friend)[pid] = 100f;
                    Intim(Player)[enemy.GetUnitId()] = -100f; Intim(enemy)[pid] = -100f;
                    heir.SetProperty<int>(UnitPropertyEnum.Reputation, 7);
                    heirSkills = heir.data.unitData.allActionMartial?.Count ?? 0;
                    heirSkillLeft = heir.data.unitData.skillLeft;
                    seatTown = g.world.build.GetBuilds(MapTerrainType.Town).ToArray().First().buildData.id;
                    var seats = MapBuildPropertyEvent.Instance.TownMasters;
                    if (!seats.ContainsKey(seatTown)) seats[seatTown] = new System.Collections.Generic.List<string>();
                    if (!seats[seatTown].Contains(pid)) seats[seatTown].Add(pid);
                    WantedEvent.Instance.Bounties.Add(new WantedEvent.Bounty { TownId = seatTown, TargetId = pid, PosterId = friend.GetUnitId(), Reward = 1000, PostedMonth = GameHelper.GetGameTotalMonth() });
                    c.Info($"heir {heirName}: {heirSkills} techniques; player: {Player.data.unitData.allActionMartial?.Count ?? 0}");
                })
                .Step("offer, heir, techniques", c =>
                {
                    // one step: a drama pauses the step timers and is dismissed after 5 s (the options are read from its data)
                    if (heir == null) return;
                    var cfg = SMLocalConfigsEvent.Instance.Configs;
                    var on = cfg.ContinueAsDescendant;
                    cfg.ContinueAsDescendant = true;
                    try { c.Assert(FamilyEvent.Instance.OfferInherit(null), "inheritance offered"); }
                    finally { cfg.ContinueAsDescendant = on; }
                    var labels = DramaOptions().Select(x => x.Item2).ToList();
                    var opt = labels.FirstOrDefault(x => x.Contains(heirName));
                    c.Assert(opt != null, $"options: {string.Join(" | ", labels)}");
                    c.Bag["gen"] = FamilyEvent.Instance.Generation;
                    if (opt == null) return;
                    // choosing the heir opens the technique question once the drama has closed (a later frame): opened here directly
                    DismissDramas();
                    FamilyEvent.Instance.AskSkills(heir);
                    labels = DramaOptions().Select(x => x.Item2).ToList();
                    var own = string.Format(GameTool.LS("family_inherit_skills_own"), heirName);
                    c.Assert(labels.Contains(own) && labels.Contains(GameTool.LS("family_inherit_skills_keep")), $"technique options: {string.Join(" | ", labels)}");
                    ChooseOption(own);
                }, 1500)
                .Step("inherited", c =>
                {
                    if (heir == null) return;
                    var pid = Player.GetUnitId();
                    c.AssertEqual(heirName, Player.GetName(), "the player is the heir now");
                    c.AssertEqual(FamilyEvent.ADULT_YEARS, FamilyEvent.AgeYears(Player), "16 years old");
                    c.Assert(g.world.unit.GetUnit(heir.GetUnitId()) == null || heir.isDie, "the heir NPC is gone");
                    c.AssertEqual((int)c.Bag["gen"] + 1, FamilyEvent.Instance.Generation, "next generation");
                    c.AssertEqual(7, Player.GetProperty<int>(UnitPropertyEnum.Reputation), "reputation of the heir");
                    c.AssertEqual(heirSkills, Player.data.unitData.allActionMartial?.Count ?? 0, "techniques of the heir");
                    c.AssertEqual(heirSkillLeft, Player.data.unitData.skillLeft, "equipped technique of the heir");
                    c.AssertEqual(50f, Intim(Player)[friend.GetUnitId()], "friendship halved (player side)");
                    c.AssertEqual(50f, Intim(friend)[pid], "friendship halved (friend side)");
                    c.AssertEqual(-100f, Intim(Player)[enemy.GetUnitId()], "enmity in full (player side)");
                    c.AssertEqual(-100f, Intim(enemy)[pid], "enmity in full (enemy side)");
                    c.Assert(!MapBuildPropertyEvent.Instance.TownMasters[seatTown].Contains(pid), "the town seat of the old player ended");
                    c.Assert(!WantedEvent.Instance.Bounties.Any(x => x.TargetId == pid), "bounties on the old player ended");
                    c.Assert(string.IsNullOrEmpty(FamilyEvent.Spouse(Player)) || FamilyEvent.Spouse(Player) != mate.GetUnitId(), "the old companion is not the player's");
                    // the heir's own relations moved to the player
                    var f = FamilyEvent.Family(Player);
                    c.Assert(f.Parents.Contains(mate.GetUnitId()), $"the heir's other parent is the player's parent: {string.Join(", ", f.Parents)}");
                    c.Assert(FamilyEvent.Family(mate).Children.Contains(pid) && !FamilyEvent.Family(mate).Children.Contains(heir.GetUnitId()), "the parent's child is the player now");
                    c.AssertEqual(80f, Intim(Player)[heirFriend.GetUnitId()], "the heir's friend (player side)");
                    c.AssertEqual(80f, Intim(heirFriend)[pid], "the heir's friend (friend side)");
                    DismissDramas();
                })
                .Step("npc fortune", c =>
                {
                    var npc = child ?? Npcs.First();
                    var e0 = npc.GetExp();
                    c.AssertEqual("log_npc_fortune", EncounterEvent.NpcFortune(npc, false), "NPC fortune");
                    var e1 = npc.GetExp();
                    var t = npc.GetProperty<int>(UnitPropertyEnum.Talent);
                    c.AssertEqual("log_npc_inheritance", EncounterEvent.NpcFortune(npc, true), "NPC inheritance");
                    c.Assert(e1 > e0 && npc.GetExp() > e1 && npc.GetProperty<int>(UnitPropertyEnum.Talent) > t, $"{npc.GetName()}: exp {e0} -> {e1} -> {npc.GetExp()}, talent {t} -> {npc.GetProperty<int>(UnitPropertyEnum.Talent)}");
                })
                .Step("seclusion offer", c =>
                {
                    DismissDramas();
                    exp = Player.GetExp();
                    var probe = Npcs.First(x => x != null && !x.isDie);
                    c.Bag["probe"] = probe.GetUnitId();
                    c.Bag["probeAge"] = probe.GetProperty<int>(UnitPropertyEnum.Age);
                    c.Bag["probeExp"] = probe.GetExp();
                    TimeSkipEvent.HidePlayer = true; // no NPC drama (they interrupt the seclusion): a full year
                    SeclusionEvent.Instance.Offer(SeclusionEvent.Kind.SpiritVein, 1);
                }, 1500)
                .Step("enter", c =>
                {
                    var o = DramaOptions().Select(x => x.Item2).FirstOrDefault(x => x == string.Format(GameTool.LS("secl_o1"), 1));
                    c.Assert(o != null && ChooseOption(o), "enter seclusion (1 year)");
                    c.Bag["month"] = GameHelper.GetGameTotalMonth();
                    c.Assert(SeclusionEvent.IsSecluded, "secluded");
                })
                .Step("progress", c =>
                {
                    c.Assert(g.ui.HasUI(UIType.Loading), "the loading window shows the skip");
                    c.Assert(TimeSkipEvent.Instance.Done > 0 && TimeSkipEvent.Instance.Total == 12, $"progress {TimeSkipEvent.Instance.Done} / {TimeSkipEvent.Instance.Total}");
                }, 2500)
                .Wait("a year passes", c => !SeclusionEvent.IsSecluded && !TimeSkipEvent.Instance.IsProcessing, 300000, 1000)
                .Step("summary", c =>
                {
                    TimeSkipEvent.HidePlayer = false;
                    c.Assert(!g.ui.HasUI(UIType.Loading), "loading window closed");
                    var probe = g.world.unit.GetUnit((string)c.Bag["probe"]);
                    if (probe != null)
                        c.Assert(probe.isDie || probe.GetProperty<int>(UnitPropertyEnum.Age) - (int)c.Bag["probeAge"] >= 12, $"NPC {probe.GetName()} aged with the skip: age {(int)c.Bag["probeAge"]} -> {probe.GetProperty<int>(UnitPropertyEnum.Age)} months, exp {(int)c.Bag["probeExp"]} -> {probe.GetExp()}, dead {probe.isDie}");
                    c.Assert(GameHelper.GetGameTotalMonth() - (int)c.Bag["month"] >= 12, $"12 months passed ({(int)c.Bag["month"]} -> {GameHelper.GetGameTotalMonth()})");
                    c.Assert(Player.GetExp() > exp, $"exp {exp} -> {Player.GetExp()}");
                    c.Assert(DramaText().Contains(string.Format(GameTool.LS("secl_summary").Split('\n')[0], 1).Split(':')[0]), $"summary drama: {DramaText()}");
                    var keys = TaoistLogEvent.Instance.Entries.Select(x => x.Key).ToList();
                    c.Assert(new[] { "log_family_married", "log_family_inherit", "log_secl_start", "log_secl_end" }.All(keys.Contains), "family and seclusion in the Taoist log");
                    DismissDramas();
                });
        }
    }
}
