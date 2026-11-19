using MOD_nE7UL2.Mod;
using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;

namespace MOD_nE7UL2.AutoTest
{
    /// <summary>
    /// Helpers for the town and battle scenarios: travel to a matching town (like the trainer teleport),
    /// open building UIs the way the game does, and drive battles. The generic ones are ModLib's (UIClickHelper,
    /// BattleHelper, DramaHelper): the names here are kept for the scenarios.
    /// </summary>
    public static class TownTestHelper
    {
        /// <summary>Town at the player position (null when the player is outside a town).</summary>
        public static MapBuildTown CurrentTown => Player.GetMapBuild<MapBuildTown>();

        /// <summary>True when the town has the sub-building.</summary>
        public static bool Has(MapBuildTown town, MapBuildSubType sub) => town != null && town.allBuildSub.ContainsKey(sub);

        /// <summary>The current town when it matches, otherwise the matching town closest to the player.</summary>
        public static MapBuildTown FindTown(Func<MapBuildTown, bool> match)
        {
            var cur = CurrentTown;
            if (cur != null && match(cur))
                return cur;
            var pos = Player.GetUnitPos();
            return (ModMain.Store?.Towns ?? new MapBuildTown[0])
                .Where(x => x != null && match(x))
                .OrderBy(x => Math.Abs(x.GetOrigiPoint().x - pos.x) + Math.Abs(x.GetOrigiPoint().y - pos.y))
                .FirstOrDefault();
        }

        /// <summary>Moves the player into a town (same as the trainer teleport) and records it in the context.</summary>
        public static bool TravelTo(AutoTestContext c, MapBuildTown town)
        {
            if (!c.Assert(town != null, "a matching town exists"))
            {
                var towns = (ModMain.Store?.Towns ?? new MapBuildTown[0]).Where(x => x != null).ToList();
                c.Info($"towns in the store: {towns.Count}, areas {string.Join(",", towns.Select(x => x.gridData.areaBaseID).Distinct().OrderBy(x => x))}, main towns: {towns.Count(x => x.buildTownData.isMainTown)}");
                return false;
            }
            if (CurrentTown?.buildData.id != town.buildData.id)
                Player.SetUnitPos(town.GetOrigiPoint());
            c.Bag["town"] = town;
            return c.Assert(CurrentTown?.buildData.id == town.buildData.id,
                $"player in {town.name} (area {town.gridData.areaBaseID}, {(town.IsSmallTown() ? "small town" : "city")})");
        }

        /// <summary>Town chosen by the travel step.</summary>
        public static MapBuildTown BagTown(AutoTestContext c) => c.Bag.ContainsKey("town") ? (MapBuildTown)c.Bag["town"] : CurrentTown;

        /// <summary>Opens a town building UI the way the game does (open, then InitData with the town) (UIClickHelper.OpenFresh).</summary>
        public static T OpenTownUI<T>(UIType.UITypeBase type, Action<T> init) where T : UIBase => UIClickHelper.OpenFresh(type, init);

        /// <summary>Active button of a game UI whose label starts with the text (UIClickHelper.FindButton).</summary>
        public static Button FindGameButton(UIBase ui, string labelStart) => UIClickHelper.FindButton(ui, labelStart);

        /// <summary>Active texts of a game UI containing the text (UIClickHelper.FindTexts).</summary>
        public static List<string> FindTexts(UIBase ui, string contains) => UIClickHelper.FindTexts(ui, contains);

        /// <summary>Closes the given UIs when open (UIClickHelper.CloseAll).</summary>
        public static void CloseAll(params UIType.UITypeBase[] types) => UIClickHelper.CloseAll(types);

        // ------------------------------------------------------------------ battle

        /// <summary>True when a battle scene is running with the player unit (BattleHelper.InBattle).</summary>
        public static bool InBattle => BattleHelper.InBattle;

        /// <summary>True when the player is back on the world map (BattleHelper.OnMap).</summary>
        public static bool OnMap => BattleHelper.OnMap;

        /// <summary>Labels of the buttons that start a battle (BattleHelper.FIGHT_LABELS).</summary>
        public static readonly string[] FIGHT = BattleHelper.FIGHT_LABELS;

        /// <summary>True when the battle really started (BattleHelper.BattleStarted).</summary>
        public static bool BattleStarted => BattleHelper.BattleStarted;

        /// <summary>
        /// Keeps the player alive in a battle the test must win (BattleHelper.KeepAlive): a new grade-1 character on the
        /// highest difficulty is killed in a few seconds, before the slow test steps (screenshots) hit the opponent.
        /// </summary>
        public static bool ProtectPlayer() => BattleHelper.KeepAlive();

        /// <summary>Wait-step body: clicks "Fight" until the battle has started (BattleHelper.ClickFight).</summary>
        public static bool StartBattle() => BattleHelper.ClickFight();

        /// <summary>Closes the open drama dialogs blocking the flow (DramaHelper.DismissDramas). Returns the count.</summary>
        public static int DismissDramas() => DramaHelper.DismissDramas();

        /// <summary>Dungeon of a plain NPC fight on the map (BattleHelper.NPC_FIGHT_DUNGEON_ID).</summary>
        public const int NPC_FIGHT_DUNGEON_ID = BattleHelper.NPC_FIGHT_DUNGEON_ID;

        /// <summary>Starts a real duel with an NPC (BattleHelper.StartRealDuel).</summary>
        public static void StartRealDuel(WorldUnitBase npc) => BattleHelper.StartRealDuel(npc);

        // ------------------------------------------------------------------ lost battle (TB-10..TB-15, TZ-01)

        /// <summary>
        /// Lose-penalty setup: picks the opponent (c.Bag["npc"]), switches every lose option off except the given ones
        /// (originals kept for RestoreAfterLoss) and records the player state (exp, money, Life, grade, HP).
        /// </summary>
        public static WorldUnitBase SetupLoss(AutoTestContext c, bool lostItem = false, bool lostMoney = false, bool lostLifespan = false, bool autoSave = false, bool oneLife = false)
        {
            var cfg = SMLocalConfigsEvent.Instance.Configs;
            c.Bag["cfg"] = new[] { cfg.LostItemWhenDie, cfg.LostMoneyWhenDie, cfg.LostLifespanWhenDie, cfg.AutoSaveAfterLostInBattle, cfg.Onelife };
            cfg.LostItemWhenDie = lostItem; cfg.LostMoneyWhenDie = lostMoney; cfg.LostLifespanWhenDie = lostLifespan;
            cfg.AutoSaveAfterLostInBattle = autoSave; cfg.Onelife = oneLife;
            c.Info($"lose options: item {lostItem}, money {lostMoney}, lifespan {lostLifespan}, auto save {autoSave}, one-life {oneLife}");
            var npc = Npcs.Where(x => !HirePeopleEvent.IsHired(x)).OrderBy(x => x.GetGradeLvl()).FirstOrDefault();
            if (c.Assert(npc != null, $"an opponent: {npc?.GetName()} grade {npc?.GetGradeLvl()}")) c.Bag["npc"] = npc;
            c.Bag["p.exp"] = Player.GetExp();
            c.Bag["p.money"] = Player.GetUnitMoney();
            c.Bag["p.life"] = Player.GetProperty<int>(UnitPropertyEnum.Life);
            c.Bag["p.grade"] = Player.GetProperty<int>(UnitPropertyEnum.GradeID);
            return npc;
        }

        /// <summary>Opponent chosen by SetupLoss (null when none).</summary>
        public static WorldUnitBase LossNpc(AutoTestContext c) => c.Bag.ContainsKey("npc") ? (WorldUnitBase)c.Bag["npc"] : null;

        /// <summary>Restores the lose options and the player state recorded by SetupLoss.</summary>
        public static void RestoreAfterLoss(AutoTestContext c)
        {
            if (c.Bag.ContainsKey("cfg"))
            {
                var v = (bool[])c.Bag["cfg"];
                var cfg = SMLocalConfigsEvent.Instance.Configs;
                cfg.LostItemWhenDie = v[0]; cfg.LostMoneyWhenDie = v[1]; cfg.LostLifespanWhenDie = v[2]; cfg.AutoSaveAfterLostInBattle = v[3]; cfg.Onelife = v[4];
            }
            if (!c.Bag.ContainsKey("p.exp")) return;
            Player.SetProperty<int>(UnitPropertyEnum.GradeID, (int)c.Bag["p.grade"]);
            Player.ClearExp();
            Player.AddExp((int)c.Bag["p.exp"]);
            Player.AddUnitMoney((int)Math.Max(int.MinValue, Math.Min(int.MaxValue, (long)(int)c.Bag["p.money"] - Player.GetUnitMoney())));
            Player.SetProperty<int>(UnitPropertyEnum.Life, (int)c.Bag["p.life"]);
            Player.SetProperty<int>(UnitPropertyEnum.Hp, int.MaxValue);
            c.Assert(Player.GetExp() == (int)c.Bag["p.exp"] && Player.GetUnitMoney() == (int)c.Bag["p.money"], "player exp, money, Life, grade restored; options restored");
        }

        /// <summary>The battle unit of a world unit, dead ones included (BattleHelper.GetBattleUnit).</summary>
        public static UnitCtrlBase BattleUnitOf(WorldUnitBase w) => BattleHelper.GetBattleUnit(w);

        /// <summary>Clicks the first button of the UI with one of the labels (UIClickHelper.ClickIn).</summary>
        public static string ClickIn(UIBase ui, params string[] labels) => UIClickHelper.ClickIn(ui, labels);

        /// <summary>Kills a battle unit with the player as the killer (BattleHelper.Kill): "hit", "die" or null.</summary>
        public static string PlayerKill(UnitCtrlBase target) => BattleHelper.Kill(target);

        /// <summary>Hits a battle unit with the player (BattleHelper.Hit: real UnitHit/UnitDie events).</summary>
        public static void PlayerHit(UnitCtrlBase target, int damage) => BattleHelper.Hit(target, damage);

        /// <summary>Ends the running battle (win or lose) like the game does (BattleHelper.EndBattle).</summary>
        public static void EndBattle(bool win) => BattleHelper.EndBattle(win);

        /// <summary>Wait-step body: clicks the result/confirm buttons until the map is shown again (BattleHelper.LeaveBattle).</summary>
        public static bool LeaveBattle() => BattleHelper.LeaveBattle();
    }
}
