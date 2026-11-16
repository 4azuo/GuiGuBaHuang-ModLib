using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using MOD_nE7UL2.Enum;
using MOD_nE7UL2.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Battle
{
    /// <summary>
    /// TB-07 (BattleEvent/BattleAfterEvent/ChallengeQuestEvent, started by code): a real NPC duel with, around the
    /// player, a friend, an enemy, a stalker and a hired team member; the join rates are set to 100 % for the test.
    /// The friend and the team member join on the player side, the enemy and the stalker on the other side (the
    /// stalker leaves the stalker list); killing an opponent at least one grade above the player counts for the
    /// challenge quest; the monthly stalker handler moves stalkers next to the player. Replaces the manual TM-21,
    /// TM-22 and TM-30.
    /// </summary>
    public class TB07_NpcJoinBattle : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TB-07";

        /// <summary>Dungeon of a plain NPC fight on the map (DungeonBase type 40).</summary>
        public const int NPC_FIGHT_DUNGEON_ID = 11;

        private static UnitCtrlBase InBattleUnit(WorldUnitBase w) => w == null ? null :
            ModBattleEvent.BattleUnitsIncludeDie.ToArray().FirstOrDefault(x => x.IsWorldUnit() && x.GetWorldUnit().GetUnitId() == w.GetUnitId());

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            WorldUnitBase opponent = null, friend = null, enemy = null, stalker = null, member = null;
            var saved = new Dictionary<string, float>();
            var sides = new Dictionary<string, string>(); // unit type names (no Il2Cpp enum in Create(): unit tests load it outside the game)
            return new Scenario { Id = "TB-07", Feature = "BattleEvent/BattleAfterEvent/ChallengeQuestEvent", Title = "NPCs join battles, stalkers, challenge quest", Overlay = false }
                .Step("prepare NPCs", c =>
                {
                    var cfg = BattleEvent.Configs;
                    saved["random"] = cfg.RandomNpcJoinRate; saved["stalker"] = cfg.StalkerJoinRate;
                    saved["team"] = cfg.TeamMemberJoinRate; saved["betray"] = cfg.TeamMemberBetrayRate;
                    cfg.RandomNpcJoinRate = 100f; cfg.StalkerJoinRate = 100f; cfg.TeamMemberJoinRate = 100f; cfg.TeamMemberBetrayRate = 0f;
                    var pid = Player.GetUnitId();
                    var pg = Player.GetGradeLvl();
                    opponent = Npcs.Where(x => !HirePeopleEvent.IsHired(x) && x.GetGradeLvl() > pg).OrderBy(x => x.GetGradeLvl()).FirstOrDefault()
                        ?? Npcs.Where(x => !HirePeopleEvent.IsHired(x)).OrderByDescending(x => x.GetGradeLvl()).FirstOrDefault();
                    var pool = Npcs.Where(x => x != opponent && !HirePeopleEvent.IsHired(x) && !HirePeopleEvent.IsTeamMaster(x) && x.GetGradeLvl() >= pg - 1)
                        .OrderBy(x => x.GetGradeLvl()).Take(8).ToList();
                    // CondJoinBattle wants HP > 70 %, MP > 50 %, SP > 30 %: the chosen NPCs are rested for the test
                    foreach (var w in pool)
                    {
                        w.SetProperty<int>(UnitPropertyEnum.Hp, int.MaxValue);
                        w.SetProperty<int>(UnitPropertyEnum.Mp, int.MaxValue);
                        w.SetProperty<int>(UnitPropertyEnum.Sp, int.MaxValue);
                    }
                    if (!c.Assert(opponent != null && pool.Count >= 4 && pool.Take(4).All(BattleEvent.CondJoinBattle), $"opponent and 4 NPCs able to join ({pool.Count(BattleEvent.CondJoinBattle)}/{pool.Count} pass CondJoinBattle)"))
                    {
                        opponent = null; // no battle: the following steps skip
                        return;
                    }
                    // the friend must not also count as an enemy (traits, good vs evil): enemies are picked first
                    friend = pool.FirstOrDefault(x => BattleEvent.IsEnemyUnit(Player, x) < 0) ?? pool[0];
                    pool.Remove(friend);
                    enemy = pool[0]; stalker = pool[1]; member = pool[2];
                    var pos = Player.GetUnitPos();
                    foreach (var w in new[] { friend, enemy, stalker }) w.SetUnitPos(pos);
                    friend.data.unitData.relationData.SetIntim(pid, cfg.FriendlyIntim + 50);
                    enemy.data.unitData.relationData.SetIntim(pid, cfg.EnemyIntim - 50);
                    BattleAfterEvent.AddStalker(StalkReasonEnum.KillBoss, stalker);
                    HirePeopleEvent.Hire(Player, member);
                    c.Assert(BattleEvent.IsFriendlyUnit(Player, friend) >= 0, $"friend {friend.GetName()}");
                    c.Assert(BattleEvent.IsEnemyUnit(Player, enemy) >= 0, $"enemy {enemy.GetName()}");
                    c.Assert(BattleAfterEvent.Instance.Stalkers.ContainsKey(stalker.GetUnitId()), $"stalker {stalker.GetName()}");
                    c.Assert(HirePeopleEvent.IsTeam(Player, member), $"team member {member.GetName()}");
                    c.Info($"opponent {opponent.GetName()} grade {opponent.GetGradeLvl()} (player grade {pg})");
                })
                .Step("start duel", c =>
                {
                    if (opponent == null) return;
                    c.Bag["task"] = ChallengeQuestEvent.Instance.TaskLevel;
                    c.Bag["wins"] = ChallengeQuestEvent.Instance.CurrentWins;
                    g.world.battle.IntoBattle(opponent, NPC_FIGHT_DUNGEON_ID, null, new WorldBattleData { npcUnit = opponent, isRealBattle = true });
                })
                .Wait("battle loaded", c => { StartBattle(); return opponent == null || InBattle; }, 90000, 1000)
                .Wait("battle started", c => opponent == null || StartBattle(), 60000, 1000)
                .Step("joinable battle", c =>
                {
                    if (!InBattle) return;
                    c.Info($"dungeon {g.world.battle.data.dungeonBaseItem.id}, real battle {DungeonHelper.IsRealBattle()}, joinable {BattleEvent.IsJoinableBattle()}");
                    if (!DungeonHelper.IsRealBattle())
                    {
                        g.world.battle.data.isRealBattle = true;
                        BattleEvent.Instance.OnBattleStart(null);
                        c.Info("battle marked as real battle for the test");
                    }
                    c.Assert(BattleEvent.IsJoinableBattle(), "NPCs can join this battle");
                    // keep the duel running while the NPCs join (the joiners can kill the opponent or the player)
                    InBattleUnit(opponent)?.AddState(UnitStateType.NotDie, float.MaxValue);
                    ModBattleEvent.PlayerUnit?.AddState(UnitStateType.NotDie, float.MaxValue);
                    c.Info($"around units: {string.Join(", ", BattleEvent.Instance.AroundUnits.Select(x => x.GetName()))}; team: {BattleEvent.Instance.TeamMembers.Count}");
                }, 1000)
                .Wait("NPCs joined", c =>
                {
                    DismissDramas();
                    ClickButton(CONFIRM);
                    if (!InBattle) return true;
                    // side of each NPC when it joins (a unit can die or change side later); the join loop takes the
                    // first friend/enemy around each second, so any friendly/hostile NPC counts, not only ours
                    foreach (var u in ModBattleEvent.BattleUnitsIncludeDie.ToArray().Where(x => x.IsWorldUnit() && !x.IsPlayer()))
                    {
                        var w = u.GetWorldUnit();
                        if (w == null || w.GetUnitId() == opponent.GetUnitId() || sides.ContainsKey(w.GetUnitId())) continue;
                        sides[w.GetUnitId()] = u.data.unitType.ToString();
                        if (u.data.unitType == UnitType.PlayerNPC && BattleEvent.IsFriendlyUnit(Player, w) >= 0) c.Bag["friendJoined"] = w.GetName();
                        if (u.data.unitType == UnitType.Monst && BattleEvent.IsEnemyUnit(Player, w) >= 0 && w != stalker) c.Bag["enemyJoined"] = w.GetName();
                    }
                    return c.Bag.ContainsKey("friendJoined") && c.Bag.ContainsKey("enemyJoined") && sides.ContainsKey(stalker.GetUnitId()) && sides.ContainsKey(member.GetUnitId());
                }, 30000, 1000)
                .Step("sides", c =>
                {
                    if (opponent == null) return;
                    string Side(WorldUnitBase w) => w != null && sides.ContainsKey(w.GetUnitId()) ? sides[w.GetUnitId()] : "None";
                    c.Assert(c.Bag.ContainsKey("friendJoined"), $"a friendly NPC joined on the player side: {(c.Bag.ContainsKey("friendJoined") ? c.Bag["friendJoined"] : "none")}");
                    c.Assert(c.Bag.ContainsKey("enemyJoined"), $"a hostile NPC joined against the player: {(c.Bag.ContainsKey("enemyJoined") ? c.Bag["enemyJoined"] : "none")}");
                    c.Info($"joined: {sides.Count} NPCs ({sides.Count(x => x.Value == "PlayerNPC")} with the player, {sides.Count(x => x.Value == "Monst")} against)");
                    c.AssertEqual("Monst", Side(stalker), "stalker joined against the player");
                    c.Assert(stalker != null && !BattleAfterEvent.Instance.Stalkers.ContainsKey(stalker.GetUnitId()), "stalker removed from the stalker list");
                    c.AssertEqual("PlayerNPC", Side(member), "team member joined on the player side");
                    for (var i = 0; i < 3; i++) { DismissDramas(); ClickButton(CONFIRM); }
                }, 1000)
                .Step("stalker pay off", c =>
                {
                    // the stalker drama offers to pay it off: its team leaves the battle, the money goes to the stalker
                    if (opponent == null || !InBattle) return;
                    var su = InBattleUnit(stalker);
                    if (su == null || su.isDie) { c.Skip("stalker not in the battle any more"); return; }
                    var fee = BattleEvent.StalkerFee(stalker);
                    c.Assert(fee >= InflationaryEvent.CalculateInflationary(500), $"stalker fee {fee} (500 x grade, at least 10 % of the money)");
                    if (Player.GetUnitMoney() < fee) Player.AddUnitMoney(fee * 2);
                    var pm = Player.GetUnitMoney(); var sm = stalker.GetUnitMoney();
                    BattleEvent.PayOffStalker(stalker, new List<UnitCtrlBase> { su }, fee);
                    c.AssertEqual(pm - fee, Player.GetUnitMoney(), $"player paid {fee}");
                    c.AssertEqual(sm + fee, stalker.GetUnitMoney(), "the stalker got the money");
                    var still = ModBattleEvent.BattleUnits.ToArray().Any(x => x != null && !x.isDie && x.IsWorldUnit() && x.GetWorldUnit()?.GetUnitId() == stalker.GetUnitId());
                    c.Assert(!still, "the stalker left the battle");
                    c.Assert(TaoistLogEvent.Instance.Entries.Any(x => x.Key == "log_stalker_paid"), "pay-off in the Taoist log");
                    var mu = InBattleUnit(member);
                    c.Bag["intim"] = member.data.unitData.relationData.GetIntim(Player);
                    c.Bag["memberAlive"] = mu != null && !mu.isDie;
                }, 1500)
                .Step("challenge kill", c =>
                {
                    if (opponent == null) return;
                    if (!c.Assert(InBattle, "still in the duel")) return;
                    var o = InBattleUnit(opponent);
                    if (!c.Assert(o != null, "opponent in the battle")) return;
                    var q = ChallengeQuestEvent.Instance;
                    var info = ChallengeQuestEvent.TaskInfo.FirstOrDefault(x => x.TaskLevel == q.TaskLevel + 1);
                    var diff = opponent.GetGradeLvl() - Player.GetGradeLvl();
                    o.DelState(UnitStateType.NotDie);
                    PlayerKill(o);
                    c.Assert(o.isDie, "opponent killed by the player");
                    if (info == null) { c.Skip("challenge quest finished"); return; }
                    if (diff < info.MinLevelDifference) { c.Skip($"opponent only {diff} grade(s) above the player (quest needs {info.MinLevelDifference})"); return; }
                    var progressed = q.TaskLevel > (int)c.Bag["task"] || q.CurrentWins > (int)c.Bag["wins"];
                    c.Assert(progressed, $"challenge quest: level {(int)c.Bag["task"]} -> {q.TaskLevel}, wins {(int)c.Bag["wins"]} -> {q.CurrentWins}");
                    if (q.TaskLevel > (int)c.Bag["task"])
                    {
                        // a luck cannot be added in a battle scene: it is pending until the map
                        c.Assert(q.PendingLuck, "challenge luck pending until the map");
                        c.Bag["luckLevel"] = q.TaskLevel;
                    }
                }, 1500)
                .Step("win", c => { if (InBattle) EndBattle(true); }, 1500)
                .Wait("back to the map", c => LeaveBattle(), 90000, 2000)
                // the pending luck is given 2 s after the map opened (ChallengeQuestEvent.OnOpenUIEnd)
                .Wait("challenge luck", c => !c.Bag.ContainsKey("luckLevel") || !ChallengeQuestEvent.Instance.PendingLuck, 15000, 500)
                .Step("stalker follows", c =>
                {
                    c.Assert(OnMap, "player back on the world map");
                    if (c.Bag.ContainsKey("luckLevel"))
                    {
                        c.Assert(!ChallengeQuestEvent.Instance.PendingLuck, "pending challenge luck given on the map");
                        c.Assert(Player.GetLuck(ChallengeQuestEvent.LUCK_BASE_ID + (int)c.Bag["luckLevel"]) != null, "challenge luck added");
                    }
                    if (c.Bag.ContainsKey("intim") && (bool)c.Bag["memberAlive"])
                    {
                        var now = member.data.unitData.relationData.GetIntim(Player);
                        c.Assert(now >= Convert.ToSingle(c.Bag["intim"]) + BattleEvent.TEAM_BOND_WIN - 0.01f, $"team member fought beside the player: intimacy {Convert.ToSingle(c.Bag["intim"])} -> {now}");
                    }
                    if (stalker == null) return;
                    var far = Player.GetUnitPos() + new UnityEngine.Vector2Int(30, 30);
                    stalker.SetUnitPos(far);
                    BattleAfterEvent.AddStalker(StalkReasonEnum.KillBoss, stalker);
                    BattleAfterEvent.Instance.OnMonthly();
                    var p = Player.GetUnitPos(); var s = stalker.GetUnitPos();
                    c.Assert(Math.Abs(p.x - s.x) <= 4 && Math.Abs(p.y - s.y) <= 4, $"monthly: stalker moved next to the player ({s.x},{s.y} / player {p.x},{p.y})");
                    BattleAfterEvent.Instance.Stalkers.Remove(stalker.GetUnitId());
                }, 2000)
                .Step("restore", c =>
                {
                    var cfg = BattleEvent.Configs;
                    cfg.RandomNpcJoinRate = saved["random"]; cfg.StalkerJoinRate = saved["stalker"];
                    cfg.TeamMemberJoinRate = saved["team"]; cfg.TeamMemberBetrayRate = saved["betray"];
                    if (member != null && HirePeopleEvent.IsTeam(Player, member)) HirePeopleEvent.Dismiss(Player, member);
                    c.Assert(member == null || !HirePeopleEvent.IsTeam(Player, member), "team member dismissed");
                });
        }
    }
}
