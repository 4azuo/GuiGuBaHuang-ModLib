using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using MOD_nE7UL2.Mod;
using System.Collections.Generic;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Unit
{
    /// <summary>
    /// TG-82 (MeritEvent, massacre), two real battles: with karma -2 the merit window offers the massacre of a small town;
    /// the defenders (town master, cultivators of the town who stand, guards) fill the battle, the others flee. Battle 1
    /// ends with the town master alive: a failed massacre (karma -300). Battle 2: the player kills the town master and
    /// every defender, then wins: the battle end handler of the mod kills the town master and the fallen defenders, leaves
    /// 1 % of the mortals, makes the town a ruin (population target x0.2), loots half of the budget, karma Damned, a bounty,
    /// the result on the map. The save is restored after the run.
    /// </summary>
    public class TG82_Massacre : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-82";

        private static MapBuildTown Town => Player.GetMapBuild<MapBuildTown>();
        private static MapBuildTown BagTownOf(AutoTestContext c) => ModMain.Store.Towns.First(x => x.buildData.id == (string)c.Bag["townId"]);

        /// <summary>Starts the massacre of the town of the run (as the merit window's button does after the drama).</summary>
        private static void Start(AutoTestContext c)
        {
            var town = BagTownOf(c);
            if (Town?.buildData.id != town.buildData.id) Player.SetUnitPos(town.GetOrigiPoint());
            var master = MapBuildPropertyEvent.GetTownMaster(town);
            var sides = MeritEvent.TownSides(town, master);
            MeritEvent.StartMassacre(town, master, sides.defend, sides.flee, MeritEvent.GuardCount(EstateEvent.Get(town)?.PalaceLevel ?? 1));
            c.Assert(MeritEvent.Instance.CurrentMassacre != null, "massacre under way");
        }

        private static List<UnitCtrlBase> Foes() =>
            ModBattleEvent.BattleUnits.ToArray().Where(x => !x.isDie && !x.IsPlayer() && !x.IsPotmon() && !x.IsSummoned()).ToList();

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-82", Feature = "MeritEvent", Title = "Merit: the massacre of a town (the evil way)", Overlay = false }
                .Step("karma in a town", c =>
                {
                    if (!c.Assert(MeritEvent.Enabled, "MeritEvent loaded")) return;
                    if (!TravelTo(c, FindTown(t => t.IsSmallTown() && MapBuildPropertyEvent.GetTownMaster(t) != null) ?? FindTown(t => t.IsSmallTown()))) return;
                    c.Bag["townId"] = Town.buildData.id;
                    var d = MeritEvent.Get(Player, true);
                    MeritEvent.Add(Player, -600 - d.Merit);
                    c.AssertEqual(-2, MeritEvent.Level(Player), "karma -2");
                    c.Assert(MeritEvent.CanMassacre(Player, Town), $"the massacre of {Town.name} is possible");
                    MeritEvent.OpenWindow();
                    c.Assert(FindButtons(UIType.TextInfoLong, string.Format(GameTool.LS("merit_massacre_button"), Town.name)).Any(), "'Massacre' button in the merit window");
                }, 1500)
                .Step("the massacre drama", c =>
                {
                    var btn = FindButtons(UIType.TextInfoLong, string.Format(GameTool.LS("merit_massacre_button"), Town.name)).FirstOrDefault();
                    if (!c.Assert(btn != null, "the button")) return;
                    btn.Item.onClick.Invoke();
                    var master = MapBuildPropertyEvent.GetTownMaster(Town);
                    var sides = MeritEvent.TownSides(Town, master);
                    c.Bag["master"] = master?.GetUnitId();
                    c.Info($"town master {master?.GetName() ?? "-"}, {sides.defend.Count} defenders ({string.Join(", ", sides.defend.Select(x => $"{x.GetName()} g{x.GetGradeLvl()}"))}), {sides.flee.Count} flee");
                    c.Assert(sides.defend.All(x => MeritEvent.Side(x) >= 0 && x.GetGradeLvl() >= Player.GetGradeLvl() - 1 && !FamilyEvent.IsFamilyOfPlayer(x)), "every defender: not evil, strong enough, not family");
                    c.Assert(sides.flee.All(x => !sides.defend.Contains(x)), "the others flee");
                }, 3000)
                .Step("battle 1: into the battle", c =>
                {
                    DramaHelper.DismissDramas();
                    CloseAll(UIType.TextInfoLong);
                    Start(c);
                })
                .Wait("battle 1 loaded", c => { StartBattle(); ProtectPlayer(); return InBattle; }, 90000, 1000)
                .Wait("battle 1 started", c => { ProtectPlayer(); return StartBattle(); }, 60000, 1000)
                .Step("defenders and guards", c =>
                {
                    if (!InBattle) return;
                    var m = MeritEvent.Instance.CurrentMassacre;
                    var foes = Foes();
                    var expected = m.Defenders.Count + m.Guards + (m.MasterId != null ? 1 : 0);
                    c.AssertEqual(expected, foes.Count, $"foes in the battle: defenders {m.Defenders.Count}, guards {m.Guards}, master {(m.MasterId != null ? 1 : 0)}");
                    if (m.MasterId != null)
                        c.Assert(foes.Any(x => x.GetWorldUnit()?.GetUnitId() == m.MasterId), "the town master fights");
                }, 3000)
                .Step("battle 1: the master stands", c => { ProtectPlayer(); if (InBattle) EndBattle(true); }, 1500)
                .Wait("battle 1: back to the map", c => LeaveBattle(), 90000, 2000)
                .Step("a failed massacre", c =>
                {
                    c.Assert(MeritEvent.Instance.CurrentMassacre == null, "the massacre ended");
                    if (c.Bag["master"] != null)
                    {
                        c.AssertEqual(-900L, MeritEvent.Get(Player).Merit, "karma -300 more");
                        c.Assert(!g.world.unit.GetUnit((string)c.Bag["master"]).isDie, "the town master lives");
                        c.Assert(!MeritEvent.IsRuin(BagTownOf(c)), "the town stands");
                    }
                    var town = BagTownOf(c);
                    c.Bag["pop"] = EstateEvent.Get(town)?.Population ?? 0;
                    c.Bag["budget"] = MapBuildPropertyEvent.GetBuildProperty(town);
                    c.Bag["money"] = Player.GetUnitMoney();
                }, 2000)
                .Step("battle 2: into the battle", c =>
                {
                    DramaHelper.DismissDramas();
                    CloseAll(UIType.TextInfo);
                    Start(c);
                })
                .Wait("battle 2 loaded", c => { StartBattle(); ProtectPlayer(); return InBattle; }, 90000, 1000)
                .Wait("battle 2 started", c => { ProtectPlayer(); return StartBattle(); }, 60000, 1000)
                // the foes killed until none is left (a death takes a moment; a defender may come in late)
                .Wait("battle 2: every defender slain", c =>
                {
                    if (!InBattle) return true;
                    ProtectPlayer();
                    var killed = Foes().Select(x => PlayerKill(x)).Count(x => x != null);
                    c.Bag["killed"] = (c.Bag.TryGetValue("killed", out var k) ? (int)k : 0) + killed;
                    return Foes().Count == 0;
                }, 30000, 1000, 2000)
                .Step("battle 2: no foe left", c =>
                {
                    c.Info($"{(c.Bag.TryGetValue("killed", out var k) ? (int)k : 0)} foes killed by the player");
                    if (InBattle) c.AssertEqual(0, Foes().Count, "no foe left");
                }, 500)
                .Step("battle 2: won", c => { ProtectPlayer(); if (InBattle) EndBattle(true); }, 1500)
                .Wait("battle 2: back to the map", c => LeaveBattle(), 90000, 2000)
                .Wait("result on the map", c => MeritEvent.PendingMessage == null, 15000, 500)
                .Step("the massacre won", c =>
                {
                    var town = BagTownOf(c);
                    var master = c.Bag["master"] == null ? null : g.world.unit.GetUnit((string)c.Bag["master"]);
                    c.Info(MeritEvent.LastMessage ?? "-");
                    c.Assert(MeritEvent.LastMessage != null && MeritEvent.LastMessage.Contains(town.name), "the result shown on the map");
                    if (master != null) c.Assert(master.isDie, $"the town master {master.GetName()} died");
                    var pop = (long)c.Bag["pop"];
                    if (EstateEvent.Get(town) != null && pop > 0)
                        c.AssertEqual(MeritEvent.Survivors(pop), EstateEvent.Get(town).Population, $"1 % of {pop} mortals left");
                    c.Assert(MeritEvent.IsRuin(town), "the town is a ruin");
                    c.AssertEqual(0.2f, MeritEvent.PopulationRate(town), "population target x0.2");
                    var loot = (long)((long)c.Bag["budget"] * 0.5);
                    c.Assert(Player.GetUnitMoney() - (int)c.Bag["money"] >= loot, $"half of the budget looted ({loot})");
                    c.AssertEqual(-3, MeritEvent.Level(Player), "Damned (karma -3)");
                    c.Assert(!MeritEvent.CanMassacre(Player, town), "a ruin cannot be massacred again");
                    c.Assert(TaoistLogEvent.Instance.Entries.Any(x => x.Key == "log_merit_massacre"), "Taoist log: massacre");
                    if (!WantedEvent.Disabled) c.Assert(WantedEvent.On(Player).Any(), "a bounty on the player");
                })
                .Step("restore", c =>
                {
                    CloseAll(UIType.TextInfo);
                    var d = MeritEvent.Get(Player);
                    if (d != null) { d.Merit = 0; d.Essence = 0; MeritEvent.Sync(Player); }
                    MeritEvent.Instance.CurrentMassacre = null;
                    if (c.Bag.ContainsKey("townId")) MeritEvent.Instance.Ruins.Remove((string)c.Bag["townId"]);
                    c.Info("the save is restored after the run (the deaths cannot be undone in the game)");
                });
        }
    }
}
