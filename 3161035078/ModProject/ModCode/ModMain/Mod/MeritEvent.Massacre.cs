using EBattleTypeData;
using EGameTypeData;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Massacre of a town (đồ thành), the evil path of the merit (karma -1 or below): a real battle against the town master,
    /// the town guards and the cultivators of the town who stand to defend it (not evil, grade at least the player's - 1);
    /// the others flee. Won (the player alive, the town master fallen): the town master and the fallen defenders die, 99 %
    /// of the mortals die, the town is a ruin for 10 years (population target x0.2), half of the town budget is looted,
    /// karma -3,000, a bounty, the righteous cultivators of the town hate the player. Lost: karma -300.
    /// </summary>
    public partial class MeritEvent
    {
        public const int MASSACRE_MIN_LEVEL = -1;
        public const int MASSACRE_KARMA = -3000;
        public const int MASSACRE_FAIL_KARMA = -300;
        public const int MASSACRE_RANGE = 6;
        public const int MASSACRE_MAX_DEFENDERS = 8;
        public const float MASSACRE_SURVIVORS = 0.01f;
        public const float MASSACRE_LOOT = 0.5f;
        public const int RUIN_YEARS = 10;
        public const float RUIN_RATE = 0.2f;
        public const int MASSACRE_HATE = 500;

        public class Massacre
        {
            public string TownId { get; set; }
            public string MasterId { get; set; }
            public List<string> Defenders { get; set; } = new List<string>();
            public List<string> Fled { get; set; } = new List<string>();
            public int Guards { get; set; }
            public bool Spawned { get; set; }
        }

        /// <summary>Massacre under way (its battle).</summary>
        public Massacre CurrentMassacre { get; set; }
        /// <summary>Ruined towns: town id -> last total month of the ruin.</summary>
        public Dictionary<string, int> Ruins { get; set; } = new Dictionary<string, int>();

        // ------------------------------------------------------------------ formulas (pure)

        /// <summary>A cultivator of the town stands to defend it: not evil, strong enough, not the player's family or hired.</summary>
        public static bool Defends(int side, int grade, int playerGrade, bool family, bool hired) => !family && !hired && side >= 0 && grade >= playerGrade - 1;
        /// <summary>Town guards: 2 + 2 a palace level.</summary>
        public static int GuardCount(int palaceLevel) => 2 + 2 * Math.Max(1, palaceLevel);
        /// <summary>Mortals left after a massacre (1 %, at least 1).</summary>
        public static long Survivors(long population) => Math.Max(1, (long)Math.Round(population * (double)MASSACRE_SURVIVORS));

        // ------------------------------------------------------------------ data

        public static bool CanMassacre(WorldUnitBase wunit, MapBuildTown town) =>
            Enabled && town != null && wunit != null && Level(wunit) <= MASSACRE_MIN_LEVEL && !MapBuildPropertyEvent.IsTownMaster(town, wunit)
            && !IsRuin(town) && Instance.CurrentMassacre == null;

        public static bool IsRuin(MapBuildTown town) =>
            Enabled && town != null && Instance.Ruins.TryGetValue(town.buildData.id, out var until) && GameHelper.GetGameTotalMonth() <= until;
        /// <summary>Population target rate of a town (EstateEvent): a ruin stays small for RUIN_YEARS.</summary>
        public static float PopulationRate(MapBuildTown town) => IsRuin(town) ? RUIN_RATE : 1f;

        /// <summary>The cultivators of the town who defend it (the strongest first) and those who flee.</summary>
        public static (List<WorldUnitBase> defend, List<WorldUnitBase> flee) TownSides(MapBuildTown town, WorldUnitBase master)
        {
            var pg = Player.GetGradeLvl();
            var near = g.world.unit.GetUnitExact(town.GetOrigiPoint(), MASSACRE_RANGE, false, true).ToArray()
                .Where(x => x != null && !x.isDie && !x.IsPlayer() && (master == null || x.GetUnitId() != master.GetUnitId())).ToList();
            var defend = near.Where(x => Defends(Side(x), x.GetGradeLvl(), pg, FamilyEvent.IsFamilyOfPlayer(x), HirePeopleEvent.IsHired(x)))
                .OrderByDescending(x => x.GetGradeLvl()).Take(MASSACRE_MAX_DEFENDERS).ToList();
            var flee = near.Where(x => !defend.Contains(x)).ToList();
            return (defend, flee);
        }

        // ------------------------------------------------------------------ flow

        /// <summary>The drama of the massacre: who defends, who flees, the price; then the battle.</summary>
        public static void OfferMassacre(MapBuildTown town)
        {
            if (!CanMassacre(Player, town))
                return;
            var master = MapBuildPropertyEvent.GetTownMaster(town);
            if (master != null && master.isDie) master = null;
            var sides = TownSides(town, master);
            var guards = GuardCount(EstateEvent.Get(town)?.PalaceLevel ?? 1);
            var text = string.Format(GameTool.LS("merit_massacre_text"), town.name, master?.GetName() ?? "-", sides.defend.Count, guards, sides.flee.Count,
                EstateEvent.Get(town)?.Population ?? 0, Math.Abs(MASSACRE_KARMA));
            DramaHelper.OpenDrama2(text, new List<string> { GameTool.LS("merit_massacre_o1"), GameTool.LS("merit_massacre_o2") }, x =>
            {
                if (x.id % 10 != 0) return;
                StartMassacre(town, master, sides.defend, sides.flee, guards);
            }, "hundengshangren");
        }

        public static void StartMassacre(MapBuildTown town, WorldUnitBase master, List<WorldUnitBase> defend, List<WorldUnitBase> flee, int guards)
        {
            Instance.CurrentMassacre = new Massacre
            {
                TownId = town.buildData.id,
                MasterId = master?.GetUnitId(),
                Defenders = defend.Select(x => x.GetUnitId()).ToList(),
                Fled = flee.Select(x => x.GetUnitId()).ToList(),
                Guards = guards,
            };
            foreach (var f in flee)
                f.data.unitData.relationData.AddHate(Player.GetUnitId(), MASSACRE_HATE / 5);
            TaoistLogEvent.Log("log_merit_massacre_start", town.name, defend.Count + (master == null ? 0 : 1), flee.Count);
            if (g.ui.HasUI(UIType.TextInfoLong)) g.ui.CloseUI(UIType.TextInfoLong);
            var lead = master ?? defend.FirstOrDefault();
            if (lead == null)
            {
                // nobody to defend the town: only its guards
                g.world.battle.IntoBattle(new DataMap.MonstData { id = MapBuildBattleEvent.monstList[GuardLevel()], level = 5 * Math.Max(1, town.gridData.areaBaseID) });
                return;
            }
            g.world.battle.IntoBattle(lead, BattleHelper.NPC_FIGHT_DUNGEON_ID, null, new WorldBattleData { npcUnit = lead, isRealBattle = true });
        }

        private static int GuardLevel() => Math.Max(0, Math.Min(MapBuildBattleEvent.monstList.Length - 1, Player.GetUnitPosAreaId() - 1));

        private static bool InMassacre => Enabled && Instance.CurrentMassacre != null && GameHelper.IsInBattlle() && ModBattleEvent.SceneBattle != null;

        /// <summary>The other defenders and the guards join the battle.</summary>
        public override void OnBattleStart(ETypeData e)
        {
            base.OnBattleStart(e);
            if (!InMassacre || Instance.CurrentMassacre.Spawned)
                return;
            var m = Instance.CurrentMassacre;
            m.Spawned = true;
            var present = new HashSet<string>(ModBattleEvent.BattleUnits.ToArray().Select(x => x.GetWorldUnit()?.GetUnitId()).Where(x => x != null));
            foreach (var id in m.Defenders.Where(x => !present.Contains(x)))
            {
                var w = g.world.unit.GetUnit(id);
                if (w == null || w.isDie) continue;
                var cunit = ModBattleEvent.SceneBattle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(w.data, UnitType.Monst);
                cunit.move.SetPosition(MapBuildBattleEvent.GetTeamBPosi());
            }
            for (var i = 0; i < m.Guards; i++)
            {
                var lvl = GuardLevel();
                var cunit = ModBattleEvent.SceneBattle.unit.CreateUnitMonst(MapBuildBattleEvent.monstList[lvl], Vector2.zero, UnitType.Monst, CommonTool.Random(1, 5) + 5 * lvl);
                cunit.move.SetPosition(MapBuildBattleEvent.GetTeamBPosi());
            }
        }

        /// <summary>The end of the massacre battle: won when the player stands and the town master (or every defender) fell.</summary>
        public override void OnBattleEndHandler(BattleEndHandler e)
        {
            base.OnBattleEndHandler(e);
            if (!InMassacre)
                return;
            var m = Instance.CurrentMassacre;
            var fallen = new HashSet<string>(ModBattleEvent.BattleUnitsIncludeDie.ToArray().Where(x => x.isDie)
                .Select(x => x.GetWorldUnit()?.GetUnitId()).Where(x => x != null));
            var playerUp = ModBattleEvent.PlayerUnit != null && !ModBattleEvent.PlayerUnit.isDie;
            var won = playerUp && (m.MasterId != null ? fallen.Contains(m.MasterId) : m.Defenders.All(fallen.Contains));
            PendingMessage = EndMassacre(won, fallen);
        }

        /// <summary>Applies the end of the massacre. Returns the message for the map.</summary>
        public static string EndMassacre(bool won, ICollection<string> fallen)
        {
            var m = Instance.CurrentMassacre;
            Instance.CurrentMassacre = null;
            var town = ModMain.Store?.Towns?.FirstOrDefault(x => x.buildData.id == m?.TownId);
            if (m == null || town == null)
                return null;
            if (!won)
            {
                Add(Player, MASSACRE_FAIL_KARMA, "merit_src_massacre_fail");
                return string.Format(GameTool.LS("merit_massacre_fail"), town.name);
            }
            // the town master and the fallen defenders die
            var dead = 0;
            foreach (var id in new[] { m.MasterId }.Concat(m.Defenders).Where(x => x != null && (x == m.MasterId || fallen.Contains(x))))
            {
                var w = g.world.unit.GetUnit(id);
                if (w != null && !w.isDie) { w.Die(); if (id != m.MasterId) dead++; }
            }
            // the mortals, the ruin, the loot
            var estate = EstateEvent.Get(town);
            long before = 0, after = 0;
            if (estate != null)
            {
                before = estate.Population;
                estate.Population = after = Survivors(before);
            }
            Instance.Ruins[town.buildData.id] = GameHelper.GetGameTotalMonth() + RUIN_YEARS * 12;
            var loot = Math.Max(0, (long)(MapBuildPropertyEvent.GetBuildProperty(town) * (double)MASSACRE_LOOT));
            if (loot > 0)
            {
                MapBuildPropertyEvent.AddBuildProperty(town, -loot);
                Player.AddUnitMoney((int)Math.Min(int.MaxValue, loot));
            }
            Add(Player, MASSACRE_KARMA, "merit_src_massacre");
            // the righteous cultivators who fled will never forgive; the evil ones respect it
            foreach (var id in m.Fled)
            {
                var w = g.world.unit.GetUnit(id);
                if (w == null || w.isDie) continue;
                if (Side(w) > 0) w.data.unitData.relationData.AddHate(Player.GetUnitId(), MASSACRE_HATE);
                else if (Side(w) < 0) w.data.unitData.relationData.AddIntim(Player.GetUnitId(), MASSACRE_HATE / 5);
            }
            if (!WantedEvent.Disabled && WantedEvent.Instance != null)
                PostKarmaBounty();
            TaoistLogEvent.Log("log_merit_massacre", town.name, dead, before - after, loot);
            return string.Format(GameTool.LS("merit_massacre_ok"), town.name, dead, before - after, loot);
        }

        /// <summary>A save loaded during the massacre battle: the massacre is dropped.</summary>
        private static void DropMassacreAtLoad()
        {
            if (Enabled && Instance.CurrentMassacre != null && !GameHelper.IsInBattlle())
                Instance.CurrentMassacre = null;
        }
    }
}
