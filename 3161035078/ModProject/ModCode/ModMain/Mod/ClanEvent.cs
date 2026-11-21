using MOD_nE7UL2.Helper;
using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Cultivator clans of the towns (settings: "Clans" of ModConf/estate_configs.json):
    /// - A clan: a cultivator with a house in a town (EstateEvent; the player too) and its living family (spouse, children,
    ///   grandchildren: FamilyEvent), at least MinMembers.
    /// - Two clans of a town have a relation -100..100: a base (random at first) moved by their fights, + the mean
    ///   intimacy between their members x MemberWeight, + MarriageBonus a marriage between them.
    /// - Yearly, rarely: hostile clans brawl (a descendant against another: the loser hurt, sometimes dead); bitter enemies
    ///   go to war (every member fights; the losers may die and lose their house; a clan left without members is wiped
    ///   out). When the player's family or an ally of it is in a fight, the player is asked to step in: a real battle
    ///   against the enemy's champion decides it.
    /// - "Clans" button on the town map: the clans of the town and their relations.
    /// </summary>
    [Cache(ModConst.CLAN_EVENT)]
    public class ClanEvent : ModEvent
    {
        public static ClanEvent Instance { get; set; }

        public static EstateConfig._Clans Conf => EstateEvent.Conf.Clans;

        /// <summary>Base relation of two clans of a town: "townId|headA|headB" (heads sorted).</summary>
        public Dictionary<string, int> Bases { get; set; } = new Dictionary<string, int>();
        /// <summary>A fight the player was asked to step in (resolved after the battle).</summary>
        public Fight Pending { get; set; }

        public class Fight
        {
            public string TownId { get; set; }
            public bool War { get; set; }
            /// <summary>Heads of the clans: the side the player helps, the other.</summary>
            public string Ours { get; set; }
            public string Theirs { get; set; }
            public string OurFighter { get; set; }
            public string TheirFighter { get; set; }
        }

        private static WorldUnitBase Player => g.world.playerUnit;

        // ------------------------------------------------------------------ formulas (pure, unit tests)

        /// <summary>Relation of two clans: base + mean intimacy of the members x weight + a bonus a marriage, -100..100.</summary>
        public static int Relation(int baseRel, double memberIntim, int marriages) =>
            (int)Math.Max(-100, Math.Min(100, Math.Round(baseRel + memberIntim * Conf.MemberWeight + marriages * Conf.MarriageBonus)));

        /// <summary>Chance (0-1) of side A to win: its power against the other's.</summary>
        public static double WinChance(double powerA, double powerB) => powerA <= 0 && powerB <= 0 ? 0.5 : powerA / Math.Max(1.0, powerA + powerB);

        public static string Key(string townId, string a, string b) => string.CompareOrdinal(a, b) < 0 ? $"{townId}|{a}|{b}" : $"{townId}|{b}|{a}";

        // ------------------------------------------------------------------ clans

        private static WorldUnitBase Unit(string id) => FamilyEvent.Unit(id);
        private static bool Alive(WorldUnitBase u) => u != null && !u.isDie;

        /// <summary>The heads of the clans of a town: the owners of its houses (estates already made).</summary>
        public static List<string> Heads(MapBuildTown town)
        {
            if (town == null || EstateEvent.Instance == null || !EstateEvent.Instance.Towns.TryGetValue(town.buildData.id, out var e))
                return new List<string>();
            return e.Plots.Where(x => x.OwnerId != null && x.Level > 0 && x.MainIndex == null).Select(x => x.OwnerId).Distinct()
                .Where(id => Alive(Unit(id)) && Members(Unit(id)).Count >= Conf.MinMembers).ToList();
        }

        /// <summary>The living members of a clan: the head, the spouse, the children and grandchildren.</summary>
        public static List<WorldUnitBase> Members(WorldUnitBase head)
        {
            var rs = new List<WorldUnitBase>();
            if (!Alive(head)) return rs;
            rs.Add(head);
            var f = FamilyEvent.Family(head);
            var spouse = Unit(f.Spouse);
            if (Alive(spouse)) rs.Add(spouse);
            foreach (var cid in f.Children)
            {
                var child = Unit(cid);
                if (!Alive(child)) continue;
                rs.Add(child);
                foreach (var gid in FamilyEvent.Family(child).Children)
                {
                    var gc = Unit(gid);
                    if (Alive(gc)) rs.Add(gc);
                }
            }
            return rs.GroupBy(x => x.GetUnitId()).Select(x => x.First()).ToList();
        }

        public static string ClanName(WorldUnitBase head) => string.Format(GameTool.LS("clan_name"), head?.GetName() ?? "-");

        /// <summary>Relation of two clans of a town (their base, the intimacy of their members, marriages between them).</summary>
        public static int Relation(MapBuildTown town, WorldUnitBase headA, WorldUnitBase headB)
        {
            var a = Members(headA);
            var b = Members(headB);
            var sum = 0.0;
            var n = 0;
            foreach (var x in a.Take(8))
                foreach (var y in b.Take(8))
                {
                    sum += (x.data.unitData.relationData.GetIntim(y.GetUnitId()) + y.data.unitData.relationData.GetIntim(x.GetUnitId())) / 2.0;
                    n++;
                }
            var ids = new HashSet<string>(b.Select(x => x.GetUnitId()));
            var marriages = a.Count(x => ids.Contains(FamilyEvent.Spouse(x) ?? ""));
            return Relation(Base(town.buildData.id, headA.GetUnitId(), headB.GetUnitId()), n == 0 ? 0 : sum / n, marriages);
        }

        public static int Base(string townId, string a, string b)
        {
            var key = Key(townId, a, b);
            if (!Instance.Bases.TryGetValue(key, out var v))
            {
                v = CommonTool.Random(Conf.StartRelation[0], Conf.StartRelation[1] + 1);
                Instance.Bases[key] = v;
            }
            return v;
        }

        public static void AddBase(string townId, string a, string b, int delta)
        {
            var key = Key(townId, a, b);
            Instance.Bases[key] = Math.Max(-100, Math.Min(100, Base(townId, a, b) + delta));
        }

        /// <summary>The clan of the player in a town (the player's head id) when it has a house there.</summary>
        public static bool IsPlayerClan(string head) => Player != null && head == Player.GetUnitId();

        private static double Power(IEnumerable<WorldUnitBase> units) => units.Sum(x => (double)UnitPowerCalHelper.CalWUnitBattlePower(x));

        // ------------------------------------------------------------------ yearly

        [EventCondition(IsInGame = HandleEnum.True)]
        public override void OnYearly()
        {
            base.OnYearly();
            if (!Conf.Enabled)
                return;
            for (var y = 0; y < Math.Max(1, ModMaster.YearsPassed); y++)
                foreach (var town in ModMain.Store.Towns)
                {
                    try { YearTown(town, () => CommonTool.Random(0f, 1f)); }
                    catch (Exception ex) { DebugHelper.WriteLine($"ClanEvent.YearTown {town?.name}: {ex}"); }
                }
        }

        /// <summary>A year of the clans of a town: hostile pairs may brawl, bitter enemies may go to war. Returns the fights.</summary>
        public static int YearTown(MapBuildTown town, Func<double> roll)
        {
            var heads = Heads(town);
            var fights = 0;
            for (var i = 0; i < heads.Count; i++)
                for (var j = i + 1; j < heads.Count; j++)
                {
                    var a = Unit(heads[i]); var b = Unit(heads[j]);
                    if (!Alive(a) || !Alive(b)) continue;
                    var rel = Relation(town, a, b);
                    if (rel <= Conf.WarAt && roll() < Conf.WarChance) { Start(town, a, b, true, roll); fights++; }
                    else if (rel <= Conf.HostileAt && roll() < Conf.BrawlChance) { Start(town, a, b, false, roll); fights++; }
                }
            return fights;
        }

        /// <summary>The clans of the player's side: its own, and its allies in the town.</summary>
        public static bool OnPlayersSide(MapBuildTown town, WorldUnitBase head)
        {
            if (Player == null || head == null) return false;
            if (IsPlayerClan(head.GetUnitId())) return true;
            if (!EstateEvent.HasHouse(town, Player) || Members(Player).Count < Conf.MinMembers) return false;
            return Relation(town, Player, head) >= Conf.AllyAt;
        }

        /// <summary>A fight starts: the player is asked to step in when its side is in it, else it is decided at once.</summary>
        public static void Start(MapBuildTown town, WorldUnitBase a, WorldUnitBase b, bool war, Func<double> roll)
        {
            var fa = Fighter(a, war);
            var fb = Fighter(b, war);
            if (fa == null || fb == null) return;
            var mineA = OnPlayersSide(town, a);
            var mineB = OnPlayersSide(town, b);
            if ((mineA ^ mineB) && Instance != null && CanAsk())
            {
                var ours = mineA ? a : b; var theirs = mineA ? b : a;
                var fight = new Fight
                {
                    TownId = town.buildData.id, War = war,
                    Ours = ours.GetUnitId(), Theirs = theirs.GetUnitId(),
                    OurFighter = (mineA ? fa : fb).GetUnitId(), TheirFighter = (mineA ? fb : fa).GetUnitId(),
                };
                Ask(town, fight);
                return;
            }
            Resolve(town, a, b, fa, fb, war, roll() < WinChance(SidePower(a, fa, war), SidePower(b, fb, war)), roll);
        }

        private static bool CanAsk() => !GameHelper.IsInBattlle() && !(TimeSkipEvent.Instance?.IsProcessing ?? false) && Instance.Pending == null;

        /// <summary>The one who fights for a clan: in a brawl a descendant (the youngest grown member, not the player), in a war the strongest.</summary>
        public static WorldUnitBase Fighter(WorldUnitBase head, bool war)
        {
            var m = Members(head).Where(x => !x.IsPlayer()).ToList();
            if (m.Count == 0) return head.IsPlayer() ? null : head;
            return war ? m.OrderByDescending(x => UnitPowerCalHelper.CalWUnitBattlePower(x)).First()
                       : m.Where(x => x.GetUnitId() != head.GetUnitId()).OrderBy(x => FamilyEvent.AgeYears(x)).FirstOrDefault() ?? m.First();
        }

        private static double SidePower(WorldUnitBase head, WorldUnitBase fighter, bool war) =>
            war ? Power(Members(head)) : UnitPowerCalHelper.CalWUnitBattlePower(fighter);

        /// <summary>The player is asked: step in (a real battle against the enemy's fighter) or stay out.</summary>
        public static void Ask(MapBuildTown town, Fight f)
        {
            Instance.Pending = f;
            var ours = Unit(f.Ours); var theirs = Unit(f.Theirs);
            var our = Unit(f.OurFighter); var their = Unit(f.TheirFighter);
            var text = string.Format(GameTool.LS(f.War ? "clan_war_ask" : "clan_brawl_ask"), town.name, ClanName(theirs), their.GetName(), ClanName(ours), our.GetName());
            DramaHelper.OpenDrama1(text, new List<string> { GameTool.LS("clan_step_in"), GameTool.LS("clan_stay_out") },
                x => OnAnswer(x.id % 10), their, Player);
        }

        /// <summary>0: step in (a real battle against the enemy's fighter); 1: stay out (decided at once).</summary>
        public static void OnAnswer(int opt)
        {
            var f = Instance.Pending;
            if (f == null) return;
            var town = Town(f.TownId);
            var their = Unit(f.TheirFighter);
            if (opt == 0 && town != null && Alive(their) && !GameHelper.IsInBattlle())
            {
                their.SetUnitPos(Player.GetUnitPos());
                g.world.battle.IntoBattle(their, BattleHelper.NPC_FIGHT_DUNGEON_ID, null, new WorldBattleData
                {
                    npcUnit = their,
                    isRealBattle = true,
                    npcWinNotKillPlayerUnit = true,
                    npcWinInsultPlayerUnit = false,
                });
                return;
            }
            Instance.Pending = null;
            if (town == null) return;
            Func<double> roll = () => CommonTool.Random(0f, 1f);
            var ours = Unit(f.Ours); var theirs = Unit(f.Theirs);
            var our = Unit(f.OurFighter);
            if (Alive(ours) && Alive(theirs) && Alive(our) && Alive(their))
                Resolve(town, ours, theirs, our, their, f.War, roll() < WinChance(SidePower(ours, our, f.War), SidePower(theirs, their, f.War)), roll);
        }

        /// <summary>The battle of the player ended: its side won or lost the fight.</summary>
        public override void OnBattleEndHandler(BattleEndHandler e)
        {
            base.OnBattleEndHandler(e);
            var f = Instance?.Pending;
            if (f == null) return;
            Instance.Pending = null;
            var town = Town(f.TownId);
            var ours = Unit(f.Ours); var theirs = Unit(f.Theirs);
            var our = Unit(f.OurFighter); var their = Unit(f.TheirFighter);
            if (town == null || ours == null || theirs == null) return;
            Resolve(town, ours, theirs, Alive(our) ? our : Player, their ?? theirs, f.War, e.isWin, () => CommonTool.Random(0f, 1f), true);
        }

        private static MapBuildTown Town(string id) => ModMain.Store.Towns.FirstOrDefault(x => x.buildData.id == id);

        /// <summary>
        /// The end of a fight (aWon: side A won). Brawl: the loser's fighter hurt, dead with BrawlDeath; war: each member of
        /// the losers dies with WarDeath (the winners' with a quarter of it), the losers' house goes to the mortals, a clan
        /// left without members is wiped out. The player never dies here. The base relation falls.
        /// </summary>
        public static void Resolve(MapBuildTown town, WorldUnitBase a, WorldUnitBase b, WorldUnitBase fa, WorldUnitBase fb, bool war, bool aWon, Func<double> roll, bool playerFought = false)
        {
            var winner = aWon ? a : b; var loser = aWon ? b : a;
            var wf = aWon ? fa : fb; var lf = aWon ? fb : fa;
            var tid = town.buildData.id;
            var related = OnPlayersSide(town, a) || OnPlayersSide(town, b) || Player.GetMapBuild<MapBuildTown>()?.buildData.id == tid;
            var winnerName = ClanName(winner); var loserName = ClanName(loser);
            if (!war)
            {
                Hurt(lf);
                if (wf != null && lf != null)
                {
                    FamilyEvent.ChangeIntim(lf.data.unitData.relationData, wf.GetUnitId(), -30);
                    FamilyEvent.ChangeIntim(wf.data.unitData.relationData, lf.GetUnitId(), -30);
                }
                var died = Alive(lf) && !lf.IsPlayer() && roll() < Conf.BrawlDeath;
                if (died) lf.Die();
                died = died || lf.isDie;   // or killed in the player's battle
                AddBase(tid, a.GetUnitId(), b.GetUnitId(), died ? Conf.DeathRelation : Conf.BrawlRelation);
                TaoistLogEvent.LogIf(related || died, died ? "log_clan_brawl_death" : "log_clan_brawl", town.name, wf.GetName(), winnerName, lf.GetName(), loserName);
            }
            else
            {
                var fallen = 0;
                foreach (var m in Members(loser).Where(x => !x.IsPlayer()))
                    if (roll() < Conf.WarDeath) { m.Die(); fallen++; }
                    else Hurt(m);
                foreach (var m in Members(winner).Where(x => !x.IsPlayer()))
                    if (roll() < Conf.WarDeath / 4) { m.Die(); fallen++; }
                if (EstateEvent.Instance != null && EstateEvent.Instance.Towns.TryGetValue(tid, out var e))
                    foreach (var p in e.Plots.Where(x => x.OwnerId == loser.GetUnitId() && x.Level > 0 && x.MainIndex == null).ToList())
                        EstateEvent.Release(e, p, true);
                AddBase(tid, a.GetUnitId(), b.GetUnitId(), Conf.WarRelation);
                TaoistLogEvent.Log("log_clan_war", town.name, winnerName, loserName, fallen);
                if (Members(loser).Count == 0)
                    TaoistLogEvent.Log("log_clan_extinct", town.name, loserName);
            }
            if (playerFought)
                InfoDialog.Msg(town.name, string.Format(GameTool.LS(aWon ? "clan_won" : "clan_lost"), winnerName, loserName));
        }

        private static void Hurt(WorldUnitBase u)
        {
            if (!Alive(u) || u.IsPlayer()) return;
            var max = u.GetDynProperty(UnitDynPropertyEnum.HpMax).value;
            u.SetProperty<int>(UnitPropertyEnum.Hp, Math.Max(1, Math.Min(u.GetProperty<int>(UnitPropertyEnum.Hp), max / 5)));
        }

        // ------------------------------------------------------------------ window

        /// <summary>The clans of a town: members, and the relation of every pair (to the player's clan first).</summary>
        public static void OpenClans(MapBuildTown town)
        {
            var ui = new UICustom1(string.Format(GameTool.LS("clan_title"), town.name));
            var heads = Heads(town).Select(Unit).ToList();
            var row = ui.FirstRow + 1;
            if (heads.Count == 0)
            {
                ui.AddText(ui.MidCol, row, GameTool.LS("clan_none")).Format(Color.gray, 15);
                return;
            }
            foreach (var h in heads.OrderByDescending(x => IsPlayerClan(x.GetUnitId())).Take(10))
            {
                ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("clan_line"), ClanName(h), Members(h).Count, EstateEvent.HouseName(EstateEvent.HouseOf(town, h)?.Level ?? 1)))
                    .Format(IsPlayerClan(h.GetUnitId()) ? new Color(0.55f, 0.35f, 0f) : Color.black, 15);
            }
            row++;
            var pairs = new List<(int rel, string text)>();
            for (var i = 0; i < heads.Count; i++)
                for (var j = i + 1; j < heads.Count; j++)
                {
                    var rel = Relation(town, heads[i], heads[j]);
                    pairs.Add((rel, string.Format(GameTool.LS("clan_relation"), ClanName(heads[i]), ClanName(heads[j]), rel, GameTool.LS(RelationKey(rel)))));
                }
            foreach (var p in pairs.OrderBy(x => x.rel).Take(12))
                ui.AddText(ui.MidCol, row++, p.text).Format(p.rel <= Conf.HostileAt ? new Color(0.6f, 0.1f, 0.1f) : p.rel >= Conf.AllyAt ? new Color(0.1f, 0.45f, 0.1f) : Color.black, 14);
            ui.AddText(ui.MidCol, ui.LastRow - 1, GameTool.LS("clan_hint")).Format(Color.gray, 13);
        }

        public static string RelationKey(int rel) =>
            rel <= Conf.WarAt ? "clan_rel_feud" : rel <= Conf.HostileAt ? "clan_rel_hostile" : rel >= Conf.AllyAt ? "clan_rel_allied" : "clan_rel_neutral";
    }
}
