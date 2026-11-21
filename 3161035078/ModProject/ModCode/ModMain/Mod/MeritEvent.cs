using MOD_nE7UL2.Helper;
using EBattleTypeData;
using EGameTypeData;
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
    /// Merit (công đức): a path of cultivation by good deeds, for the player and the NPCs.
    /// - Merit points go up by killing beasts, killing evil cultivators, helping people (encounters: wounded, flood, plague,
    ///   beast raid, festival, cub; donations to a town) and down by killing righteous ones, robbing, walking away.
    /// - 6 levels (100 / 500 / 2,000 / 6,000 / 15,000 / 40,000 points); below zero, 3 levels of karma (-100 / -500 / -2,000).
    ///   Above the top level the points become the essence of merit (a step every 10,000), without a limit.
    /// - Bonuses: attack, defense, Luck by level (karma: more attack, less defense, less Luck); the lightning of the trials
    ///   and of the body tribulations is weaker for merit, stronger for karma.
    /// - World: the NPCs around the player like or hate them more every month by their side (righteous / evil); NPCs gather
    ///   merit or karma every month by their side; donations go to the town budget; heavy karma brings a bounty on the player.
    /// - Merit moves the world: grateful people bring gifts (merit 2+), the kin of the victims take revenge (karma -2 or
    ///   below), the town wars count (defending a town: merit, attacking one: karma), a town where the player has a house
    ///   grows faster with merit; the massacre of a town is the evil way (MeritEvent.Massacre.cs).
    /// Off with S&amp;M "No merit".
    /// </summary>
    [Cache(ModConst.MERIT_EVENT)]
    public partial class MeritEvent : ModEvent
    {
        public static MeritEvent Instance { get; set; }

        /// <summary>Merit points needed for the levels 1-6.</summary>
        public static readonly long[] LEVELS = { 100, 500, 2000, 6000, 15000, 40000 };
        /// <summary>Karma: merit points at or below which the levels -1, -2, -3 begin.</summary>
        public static readonly long[] KARMA = { -100, -500, -2000 };
        public static int MAX_LEVEL => LEVELS.Length;
        public static int MIN_LEVEL => -KARMA.Length;
        public static long MaxMerit => LEVELS[LEVELS.Length - 1];
        public static long MinMerit => KARMA[KARMA.Length - 1] * 5;
        /// <summary>Essence of merit: a step every ESSENCE_STEP points above the top.</summary>
        public const long ESSENCE_STEP = 10000;

        // bonuses (share of the base value)
        public const float ATK_PER_LEVEL = 0.01f;
        public const float DEF_PER_LEVEL = 0.015f;
        public const float KARMA_ATK_PER_LEVEL = 0.02f;
        public const float KARMA_DEF_PER_LEVEL = 0.02f;
        public const float ESSENCE_ATK = 0.005f;
        public const float ESSENCE_DEF = 0.005f;
        public const int ESSENCE_LUCK_STEPS = 5;
        /// <summary>Lightning damage: -5 % a merit level, +10 % a karma level.</summary>
        public const float LIGHTNING_MERIT = 0.05f;
        public const float LIGHTNING_KARMA = 0.10f;

        // sources
        public const int MONSTER_MERIT = 2;        // x the beast grade
        public const int EVIL_KILL_MERIT = 5;      // x the grade of an evil cultivator killed
        public const int GOOD_KILL_MERIT = -20;    // x the grade of a righteous cultivator killed
        public const int NEUTRAL_KILL_MERIT = -10; // x the grade
        public const int DONATE_MERIT = 10;        // x the player's grade, a donation of DONATE_COST x the encounter cost
        public const int DONATE_COST = 5;
        /// <summary>NPC merit a month: righteous 0..3, evil 0..-3.</summary>
        public const int NPC_MONTHLY = 3;
        /// <summary>Intimacy a month of the NPCs near the player, a level.</summary>
        public const int INTIM_SAME = 2;
        public const int INTIM_OTHER = 1;
        public const int INTIM_RANGE = 8;

        public class MeritData
        {
            /// <summary>Merit points (karma below zero), from MinMerit to MaxMerit.</summary>
            public long Merit { get; set; }
            /// <summary>Points above the top level.</summary>
            public long Essence { get; set; }
            /// <summary>Luck given by the merit (kept in sync with the level).</summary>
            public int LuckGiven { get; set; }
            public int LastDonateMonth { get; set; } = -1;
        }

        public Dictionary<string, MeritData> Merits { get; set; } = new Dictionary<string, MeritData>();
        public int LastBountyYear { get; set; } = -1;

        public override bool OnCacheHandler()
        {
            return !(SMLocalConfigsEvent.Instance?.Configs?.NoMerit ?? false);
        }

        public static bool Enabled => Instance != null && !(SMLocalConfigsEvent.Instance?.Configs?.NoMerit ?? false);

        private static WorldUnitBase Player => g.world.playerUnit;

        // ------------------------------------------------------------------ formulas (pure, unit tests)

        /// <summary>Level of merit points: 0-6, karma -1 to -3.</summary>
        public static int LevelOf(long merit)
        {
            if (merit >= 0)
                return LEVELS.Count(x => merit >= x);
            return -KARMA.Count(x => merit <= x);
        }
        /// <summary>Points of the next level (the top: MaxMerit).</summary>
        public static long NextOf(long merit)
        {
            var l = LevelOf(merit);
            if (l >= MAX_LEVEL) return MaxMerit;
            if (l >= 0) return LEVELS[l];
            return l == -1 ? 0 : KARMA[-l - 2];
        }
        public static double EssenceSteps(long essence) => Math.Max(0, essence) / (double)ESSENCE_STEP;
        /// <summary>Attack share: +1 % a level and +0.5 % an essence step; karma +2 % a level.</summary>
        public static float AtkRate(int level, double steps) => level >= 0 ? ATK_PER_LEVEL * level + ESSENCE_ATK * (float)steps : KARMA_ATK_PER_LEVEL * -level;
        /// <summary>Defense share: +1.5 % a level and +0.5 % an essence step; karma -2 % a level.</summary>
        public static float DefRate(int level, double steps) => level >= 0 ? DEF_PER_LEVEL * level + ESSENCE_DEF * (float)steps : -KARMA_DEF_PER_LEVEL * -level;
        /// <summary>Luck: +1 every 2 levels, +1 every 5 essence steps; karma -1 a level.</summary>
        public static int LuckOf(int level, double steps) => level >= 0 ? level / 2 + (int)(steps / ESSENCE_LUCK_STEPS) : level;
        /// <summary>Lightning damage rate (trials, body tribulations).</summary>
        public static float LightningRateOf(int level) => level >= 0 ? Math.Max(0.5f, 1f - LIGHTNING_MERIT * level) : 1f + LIGHTNING_KARMA * -level;
        /// <summary>Intimacy a month of an NPC near a unit of this level: same side (merit / righteous, karma / evil) up, other side down.</summary>
        public static int IntimOf(int level, bool righteous)
        {
            if (level == 0) return 0;
            var good = level > 0;
            var n = Math.Abs(level);
            return good == righteous ? INTIM_SAME * n : -INTIM_OTHER * n;
        }
        /// <summary>Merit of a kill: a beast by its grade, a cultivator by its side (1 righteous: karma, -1 evil: merit) and grade.</summary>
        public static int KillMerit(bool monster, int grade, int side)
        {
            grade = Math.Max(1, grade);
            if (monster) return MONSTER_MERIT * grade;
            return (side > 0 ? GOOD_KILL_MERIT : side < 0 ? EVIL_KILL_MERIT : NEUTRAL_KILL_MERIT) * grade;
        }

        public const int DEFEND_MERIT = 3;         // x the grade of a foe killed defending a town
        public const int ATTACK_MERIT = -3;        // x the grade of a defender killed attacking a town
        public static int TownWarMerit(bool defending, int grade) => (defending ? DEFEND_MERIT : ATTACK_MERIT) * Math.Max(1, grade);
        /// <summary>Chance (%) a month of a gift of the grateful (merit 2+) or a revenge of the kin (karma -2 or below), by level.</summary>
        public const float GIFT_RATE = 1f, REVENGE_RATE = 1.5f;
        public static float GiftChance(int level) => level >= 2 ? GIFT_RATE * (level - 1) : 0f;
        public static float RevengeChance(int level) => level <= -2 ? REVENGE_RATE * (-level - 1) : 0f;
        public const float REVENGE_HP = 0.15f;
        public const int REVENGE_MOOD = 10;
        /// <summary>Population target of a town where the player has a house: +3 % a merit level (EstateEvent).</summary>
        public const float TOWN_POP_PER_LEVEL = 0.03f;
        public static float TownPopRate(int level, bool hasHouse) => hasHouse && level > 0 ? 1f + TOWN_POP_PER_LEVEL * level : 1f;

        // ------------------------------------------------------------------ data

        public static MeritData Get(WorldUnitBase wunit, bool create = false)
        {
            if (wunit == null || !Enabled)
                return null;
            var id = wunit.GetUnitId();
            if (!Instance.Merits.TryGetValue(id, out var d) && create)
                Instance.Merits[id] = d = new MeritData();
            return d;
        }

        public static long MeritOf(WorldUnitBase wunit) => Get(wunit)?.Merit ?? 0;
        public static int Level(WorldUnitBase wunit) => LevelOf(MeritOf(wunit));
        public static double Steps(WorldUnitBase wunit) => EssenceSteps(Get(wunit)?.Essence ?? 0);
        public static string LevelName(int level) => GameTool.LS(level < 0 ? $"merit_lvl_m{-level}" : $"merit_lvl_{level}");
        /// <summary>1 = righteous (StandUp above StandDown), -1 = evil, 0 = between.</summary>
        public static int Side(WorldUnitBase wunit)
        {
            var up = wunit.GetProperty<int>(UnitPropertyEnum.StandUp);
            var down = wunit.GetProperty<int>(UnitPropertyEnum.StandDown);
            return up > down ? 1 : up < down ? -1 : 0;
        }

        /// <summary>Adds merit (or karma, below zero); above the top level the points go into the essence. Returns the levels moved.</summary>
        public static int Add(WorldUnitBase wunit, long points, string reasonKey = null)
        {
            var d = Get(wunit, true);
            if (d == null || points == 0)
                return 0;
            if (points > 0) points = (long)Math.Round(points * SMGlobalConfigsEvent.Rate(c => c.MeritRate));
            var before = LevelOf(d.Merit);
            var m = d.Merit + points;
            if (m > MaxMerit)
            {
                d.Essence += m - MaxMerit;
                m = MaxMerit;
            }
            d.Merit = Math.Max(MinMerit, m);
            var after = LevelOf(d.Merit);
            Sync(wunit);
            if (wunit.IsPlayer())
            {
                if (reasonKey != null)
                    TaoistLogEvent.Log(points > 0 ? "log_merit_gain" : "log_merit_loss", Math.Abs(points), GameTool.LS(reasonKey));
                if (after != before)
                    TaoistLogEvent.Log("log_merit_level", LevelName(after));
            }
            return after - before;
        }

        /// <summary>Keeps the Luck given by the merit in step with the level.</summary>
        public static void Sync(WorldUnitBase wunit)
        {
            var d = Get(wunit);
            if (d == null)
                return;
            var luck = LuckOf(LevelOf(d.Merit), EssenceSteps(d.Essence));
            if (luck != d.LuckGiven)
            {
                wunit.AddProperty<int>(UnitPropertyEnum.Luck, luck - d.LuckGiven);
                d.LuckGiven = luck;
            }
        }

        // ------------------------------------------------------------------ bonuses (UnitModifyEvent, RealTrialEvent, BodyCulEvent)

        /// <summary>Flat part of the bonuses (a low grade has small base values): attack +1 a level (karma +2), defense +1 a level (karma -1).</summary>
        public const int FLAT_ATK = 1, FLAT_DEF = 1, KARMA_FLAT_ATK = 2, KARMA_FLAT_DEF = 1;
        public static int FlatAtk(int level) => level >= 0 ? FLAT_ATK * level : KARMA_FLAT_ATK * -level;
        public static int FlatDef(int level) => level >= 0 ? FLAT_DEF * level : -KARMA_FLAT_DEF * -level;
        /// <summary>Attack bonus of a base attack: the share of the level and the essence + the flat part.</summary>
        public static int AtkBonus(int baseAtk, int level, double steps) => (int)Math.Round(baseAtk * (double)AtkRate(level, steps)) + FlatAtk(level);
        public static int DefBonus(int baseDef, int level, double steps) => (int)Math.Round(baseDef * (double)DefRate(level, steps)) + FlatDef(level);
        public static int AdjAtk(WorldUnitBase wunit, int baseAtk) => Get(wunit) == null ? 0 : AtkBonus(baseAtk, Level(wunit), Steps(wunit));
        public static int AdjDef(WorldUnitBase wunit, int baseDef) => Get(wunit) == null ? 0 : DefBonus(baseDef, Level(wunit), Steps(wunit));
        public static float LightningRate(WorldUnitBase wunit) => Get(wunit) == null ? 1f : LightningRateOf(Level(wunit));

        // ------------------------------------------------------------------ sources

        /// <summary>A beast or a cultivator killed by the player (not in a spar, the arena, a trial or a town war).</summary>
        public override void OnBattleUnitDie(UnitDie e)
        {
            base.OnBattleUnitDie(e);
            var dead = e?.unit;
            var killer = e?.hitData?.attackUnit?.GetWorldUnit();
            if (dead == null || killer == null || !killer.IsPlayer() || dead.IsPlayer())
                return;
            if (DungeonHelper.IsSpar() || DungeonHelper.IsArena() || DungeonHelper.IsTrailOfLightning())
                return;
            if (MapBuildBattleEvent.IsBattleTownWar())
            {
                // a town war: every foe killed counts for the side the player took (defending a town or attacking it)
                var defending = MapBuildBattleEvent.PlayerSide == MapBuildBattleEvent.TeamSideEnum.TeamA;
                var grade = dead.IsWorldUnit() ? dead.GetWorldUnit().GetGradeLvl() : dead.data.grade.value;
                Add(Player, TownWarMerit(defending, grade), defending ? "merit_src_defend" : "merit_src_attack");
                return;
            }
            if (dead.IsMonster())
                Add(Player, KillMerit(true, dead.data.grade.value, 0), "merit_src_beast");
            else if (dead.IsWorldUnit())
            {
                var w = dead.GetWorldUnit();
                var side = Side(w);
                Add(Player, KillMerit(false, w.GetGradeLvl(), side), side > 0 ? "merit_src_kill_good" : side < 0 ? "merit_src_kill_evil" : "merit_src_kill");
            }
        }

        /// <summary>NPCs gather merit or karma by their side; the NPCs near the player like or hate them by their merit.</summary>
        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);
            if (wunit == null || wunit.isDie)
                return;
            var months = Math.Max(1, ModMaster.MonthsPassed);
            if (wunit.IsPlayer())
            {
                var level = Level(wunit);
                if (level == 0)
                    return;
                MonthlyDeeds(level, months);
                foreach (var npc in wunit.GetUnitsAround(INTIM_RANGE, false, false).ToArray().Where(x => x != null && !x.isDie && !x.IsPlayer()))
                {
                    var n = IntimOf(level, Side(npc) >= 0) * months;
                    if (n > 0) npc.data.unitData.relationData.AddIntim(wunit.GetUnitId(), n);
                    else if (n < 0) npc.data.unitData.relationData.AddHate(wunit.GetUnitId(), -n);
                }
                return;
            }
            var side = Side(wunit);
            if (side != 0)
                Add(wunit, side * CommonTool.Random(0, NPC_MONTHLY + 1) * months);
        }

        /// <summary>The world answers the merit of the player: a gift of the grateful, a revenge of the kin of the victims.</summary>
        public static void MonthlyDeeds(int level, int months)
        {
            if (GameHelper.IsInBattlle() || (TimeSkipEvent.Instance?.IsProcessing ?? false))
                return;
            var roll = CommonTool.Random(0f, 100f);
            if (roll < GiftChance(level) * months)
            {
                var item = EncounterEvent.GiveItem(x => x.IsHerbItem() != null || x.IsPill() != null, Math.Max(1, Player.GetGradeLvl()));
                TaoistLogEvent.Log("log_merit_gift", item);
                DramaHelper.OpenDrama2(string.Format(GameTool.LS("merit_gift"), item), new List<string> { GameTool.LS("enc_leave") }, null, "huapaonanziganxie");
            }
            else if (roll < RevengeChance(level) * months)
            {
                var hp = Player.GetDynProperty(UnitDynPropertyEnum.Hp).value;
                var lost = Math.Max(0, Math.Min(hp - 1, (int)(hp * REVENGE_HP)));
                Player.AddProperty<int>(UnitPropertyEnum.Hp, -lost);
                Player.AddProperty<int>(UnitPropertyEnum.Mood, -Math.Min(REVENGE_MOOD, Player.GetProperty<int>(UnitPropertyEnum.Mood)));
                TaoistLogEvent.Log("log_merit_revenge", lost);
                DramaHelper.OpenDrama2(string.Format(GameTool.LS("merit_revenge"), lost), new List<string> { GameTool.LS("enc_leave") }, null, "hundengshangren");
            }
        }

        /// <summary>A message for the map after a battle (massacre).</summary>
        public static string PendingMessage { get; set; }

        [ErrorIgnore]
        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
        public override void OnTimeUpdate1000ms()
        {
            base.OnTimeUpdate1000ms();
            if (PendingMessage != null && BattleHelper.OnMap)
            {
                var msg = PendingMessage;
                PendingMessage = null;
                LastMessage = msg;
                InfoDialog.Msg(GameTool.LS("merit_title"), msg);
            }
        }
        public static string LastMessage { get; private set; }

        [EventCondition(IsInGame = HandleEnum.True)]
        public override void OnLoadGame()
        {
            base.OnLoadGame();
            PendingMessage = null;
            DropMassacreAtLoad();
        }

        /// <summary>Heavy karma (level -2 or below): once a year a righteous cultivator puts a bounty on the player.</summary>
        [EventCondition(IsInGame = HandleEnum.True)]
        public override void OnYearly()
        {
            base.OnYearly();
            if (Level(Player) > -2 || WantedEvent.Disabled || WantedEvent.Instance == null || LastBountyYear == GameHelper.GetGameYear())
                return;
            LastBountyYear = GameHelper.GetGameYear();
            PostKarmaBounty();
        }

        /// <summary>A righteous NPC of a town puts a bounty on the player for his karma. Returns the error key (null = posted).</summary>
        public static string PostKarmaBounty()
        {
            var town = Player.GetMapBuild<MapBuildTown>() ?? WantedEvent.PlayerTowns().FirstOrDefault() ?? ModMain.Store?.Towns?.FirstOrDefault();
            var poster = (ModMain.Store?.WUnits ?? new WorldUnitBase[0])
                .Where(x => x != null && !x.isDie && !x.IsPlayer() && Side(x) > 0)
                .OrderByDescending(x => x.GetGradeLvl()).FirstOrDefault();
            if (town == null || poster == null)
                return "wanted_err_target";
            var reward = WantedEvent.MinReward(Player) * Math.Max(1, -Level(Player));
            var err = WantedEvent.Instance.Post(town, poster, Player, reward);
            if (err == null)
                TaoistLogEvent.Log("log_merit_bounty", poster.GetName(), town.name, reward);
            return err;
        }

        /// <summary>A donation to the town the player stands in (once a month): spirit stones to the budget, merit.</summary>
        public static bool Donate(WorldUnitBase wunit)
        {
            var d = Get(wunit, true);
            var town = wunit.GetMapBuild<MapBuildTown>();
            if (d == null || town == null || d.LastDonateMonth == GameHelper.GetGameTotalMonth())
                return false;
            var cost = EncounterEvent.Cost(DONATE_COST);
            if (wunit.GetUnitMoney() < cost)
                return false;
            wunit.AddUnitMoney(-cost);
            MapBuildPropertyEvent.AddBuildProperty(town, cost);
            d.LastDonateMonth = GameHelper.GetGameTotalMonth();
            Add(wunit, DONATE_MERIT * Math.Max(1, wunit.GetGradeLvl()), "merit_src_donate");
            return true;
        }

        // ------------------------------------------------------------------ UI

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.FateFeature.uiName)
            {
                var ui = new UICover<UIFateFeature>(e.ui);
                ui.AddButton(ui.MidCol - 8, ui.MidRow - 10, OpenWindow, GameTool.LS("merit_button")).Size(160f, 30f);
                ui.AddText(ui.MidCol - 8, ui.MidRow - 9, "{0}").Format(Color.black, 13).SetWork(new UIItemWork
                {
                    Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>(x => new object[] { LevelName(Level(Player)) }),
                });
                ui.IsAutoUpdate = true;
                ui.UpdateUI();
            }
            else if (e.uiType.uiName == UIType.NPCInfo.uiName)
            {
                var uiBase = g.ui.GetUI<UINPCInfo>(e.ui.uiType);
                var unit = uiBase?.unit;
                if (unit != null && !unit.IsPlayer() && Level(unit) != 0 && !HirePeopleEvent.isShowHirePeopleUI)
                {
                    var ui = new UICover<UINPCInfo>(uiBase);
                    ui.AddText(ui.MidCol - 6, ui.MidRow - 6, string.Format(GameTool.LS("merit_npc"), LevelName(Level(unit))))
                        .Align().Format(Level(unit) > 0 ? new Color(1f, 0.85f, 0.4f) : new Color(1f, 0.45f, 0.45f), 16)
                        .SetParentTransform(ui.UI.uiHeart.goGroupRoot);
                    ui.UpdateUI();
                }
            }
        }

        /// <summary>Merit window of the player.</summary>
        public static void OpenWindow()
        {
            var ui = new UICustom1(GameTool.LS("merit_title"));
            var row = ui.FirstRow + 1;
            var d = Get(Player, true);
            var level = LevelOf(d.Merit);
            var steps = EssenceSteps(d.Essence);
            ui.AddText(ui.MidCol, row++, LevelName(level)).Format(level >= 0 ? new Color(0.55f, 0.4f, 0f) : new Color(0.6f, 0.1f, 0.1f), 20);
            ui.AddText(ui.MidCol, row++, level >= MAX_LEVEL
                ? string.Format(GameTool.LS("merit_points_top"), d.Merit)
                : string.Format(GameTool.LS("merit_points"), d.Merit, NextOf(d.Merit))).Format(null, 15);
            if (d.Essence > 0 || level >= MAX_LEVEL)
                ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("merit_essence"), d.Essence, (int)steps,
                    Math.Round(ESSENCE_ATK * steps * 100, 1), Math.Round(ESSENCE_DEF * steps * 100, 1))).Format(new Color(0.45f, 0.2f, 0.55f), 15);
            row++;
            ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("merit_bonus"),
                Math.Round(AtkRate(level, steps) * 100, 1), Math.Round(DefRate(level, steps) * 100, 1), LuckOf(level, steps))).Format(null, 15);
            ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("merit_flat"), FlatAtk(level), FlatDef(level))).Format(null, 14);
            ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("merit_lightning"), (int)Math.Round((LightningRateOf(level) - 1) * 100))).Format(null, 15);
            ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("merit_intim"), IntimOf(level, true), IntimOf(level, false))).Format(null, 15);
            if (level <= -2)
                ui.AddText(ui.MidCol, row++, GameTool.LS("merit_bounty_warn")).Format(Color.red, 14);
            row++;
            // levels
            for (var l = MIN_LEVEL; l <= MAX_LEVEL; l++)
            {
                if (l == 0) continue;
                var at = l > 0 ? LEVELS[l - 1] : KARMA[-l - 1];
                ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("merit_level_line"), LevelName(l), at,
                    Math.Round(AtkRate(l, 0) * 100, 1), Math.Round(DefRate(l, 0) * 100, 1), LuckOf(l, 0)))
                    .Format(l == level ? new Color(0.1f, 0.4f, 0.1f) : new Color(0.35f, 0.35f, 0.35f), 13);
            }
            row++;
            var town = Player.GetMapBuild<MapBuildTown>();
            if (town != null)
            {
                ui.AddButton(ui.MidCol, row, () =>
                {
                    if (!Donate(Player))
                        InfoDialog.Msg(GameTool.LS("merit_title"), GameTool.LS(Get(Player)?.LastDonateMonth == GameHelper.GetGameTotalMonth() ? "merit_donate_done" : "enc_nomoney"));
                    else
                        OpenWindow();
                }, string.Format(GameTool.LS("merit_donate"), town.name, EncounterEvent.Cost(DONATE_COST), DONATE_MERIT * Math.Max(1, Player.GetGradeLvl()))).Format(Color.black, 13).Size(760, 36);
                row += 2;
                // the evil way: the massacre of the town
                if (CanMassacre(Player, town))
                {
                    ui.AddButton(ui.MidCol, row, () => OfferMassacre(town), string.Format(GameTool.LS("merit_massacre_button"), town.name)).Format(Color.black, 13).Size(760, 36);
                    row += 2;
                }
                else if (IsRuin(town))
                    ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("merit_ruin"), town.name)).Format(new Color(0.6f, 0.1f, 0.1f), 14);
            }
            ui.AddText(ui.MidCol, ui.LastRow - 1, GameTool.LS("merit_help")).Format(null, 13);
        }
    }
}
