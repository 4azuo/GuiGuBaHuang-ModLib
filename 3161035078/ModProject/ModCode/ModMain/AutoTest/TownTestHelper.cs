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
    /// open building UIs the way the game does, and drive battles.
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
            return (ModMaster.ModObj.ParameterStore?.Towns ?? new MapBuildTown[0])
                .Where(x => x != null && match(x))
                .OrderBy(x => Math.Abs(x.GetOrigiPoint().x - pos.x) + Math.Abs(x.GetOrigiPoint().y - pos.y))
                .FirstOrDefault();
        }

        /// <summary>Moves the player into a town (same as the trainer teleport) and records it in the context.</summary>
        public static bool TravelTo(AutoTestContext c, MapBuildTown town)
        {
            if (!c.Assert(town != null, "a matching town exists")) return false;
            if (CurrentTown?.buildData.id != town.buildData.id)
                Player.SetUnitPos(town.GetOrigiPoint());
            c.Bag["town"] = town;
            return c.Assert(CurrentTown?.buildData.id == town.buildData.id,
                $"player in {town.name} (area {town.gridData.areaBaseID}, {(town.IsSmallTown() ? "small town" : "city")})");
        }

        /// <summary>Town chosen by the travel step.</summary>
        public static MapBuildTown BagTown(AutoTestContext c) => c.Bag.ContainsKey("town") ? (MapBuildTown)c.Bag["town"] : CurrentTown;

        /// <summary>Opens a town building UI the way the game does (open, then InitData with the town).</summary>
        public static T OpenTownUI<T>(UIType.UITypeBase type, Action<T> init) where T : UIBase
        {
            if (g.ui.HasUI(type))
                g.ui.CloseUI(type);
            var ui = g.ui.OpenUISafe<T>(type);
            init?.Invoke(ui);
            return ui;
        }

        /// <summary>Active button of a game UI whose label starts with the text (null when none).</summary>
        public static Button FindGameButton(UIBase ui, string labelStart)
        {
            return ui?.GetComponentsInChildren<Button>()
                .FirstOrDefault(b => b.gameObject.activeInHierarchy && (b.GetComponentInChildren<Text>()?.text ?? string.Empty).StartsWith(labelStart));
        }

        /// <summary>Active texts of a game UI containing the text.</summary>
        public static List<string> FindTexts(UIBase ui, string contains)
        {
            return ui?.GetComponentsInChildren<Text>().Where(t => t.gameObject.activeInHierarchy && (t.text ?? string.Empty).Contains(contains))
                .Select(t => t.text).ToList() ?? new List<string>();
        }

        /// <summary>Closes the given UIs when open.</summary>
        public static void CloseAll(params UIType.UITypeBase[] types)
        {
            foreach (var t in types)
                if (g.ui.HasUI(t))
                    g.ui.CloseUI(t);
        }

        // ------------------------------------------------------------------ battle

        /// <summary>True when a battle scene is running with the player unit.</summary>
        public static bool InBattle => GameHelper.IsInBattlle() && ModBattleEvent.SceneBattle != null && ModBattleEvent.PlayerUnit != null;

        /// <summary>True when the player is back on the world map.</summary>
        public static bool OnMap => !GameHelper.IsInBattlle() && g.ui.HasUI(UIType.MapMain);

        /// <summary>Labels of the buttons that start a battle (pre-battle screen on the map, "War / Fight" intro in the battle).</summary>
        public static readonly string[] FIGHT = { "Fight", "战斗", "戰鬥", "开战", "開戰", "出战", "出戰" };

        /// <summary>
        /// True when the battle really started (the intro "Fight" screen was closed): units only take damage,
        /// die and join from then on.
        /// </summary>
        public static bool BattleStarted => InBattle && ModBattleEvent.SceneBattle.battleMap != null && ModBattleEvent.SceneBattle.battleMap.isStartBattle;

        /// <summary>
        /// Wait-step body: clicks "Fight" on the map pre-battle screen (MapBattlePre) and on the battle intro
        /// (BattleInfo) until the battle has started.
        /// </summary>
        /// <summary>
        /// Keeps the player alive in a battle the test must win (NotDie state): a new grade-1 character on the highest
        /// difficulty is killed in a few seconds, before the slow test steps (screenshots) hit the opponent.
        /// The hp is refilled below 50 %. Removed by the game with the battle.
        /// </summary>
        public static bool ProtectPlayer()
        {
            var p = InBattle ? ModBattleEvent.PlayerUnit : null;
            if (p == null || p.isDie) return false;
            if (!p.IsState(UnitStateType.NotDie))
                p.AddState(UnitStateType.NotDie, float.MaxValue);
            // NotDie keeps the unit at 0 hp, and the game ignores the hits of a unit at 0 hp: keep it healthy
            if (p.data.hp < p.data.maxHP.value / 2)
                p.data.hp = p.data.maxHP.value;
            return true;
        }

        public static bool StartBattle()
        {
            DismissDramas();
            if (BattleStarted) return true;
            if (g.ui.HasUI(UIType.MapBattlePre))
            {
                var pre = g.ui.GetUI<UIMapBattlePre>(UIType.MapBattlePre);
                if (ClickIn(pre, FIGHT) == null)
                    pre?.onBattleCall?.Invoke();
                return false;
            }
            if (InBattle)
            {
                var info = g.ui.GetUI<UIBattleInfo>(UIType.BattleInfo);
                if (ClickIn(info, FIGHT) == null)
                    ClickButton(FIGHT);
            }
            return false;
        }

        /// <summary>
        /// Closes the open drama dialogs (NPC joining a battle, stalkers, deals...) and an NPC info opened from them. They
        /// wait for a click and block the battle start/end (and the game timers). Returns the count.
        /// </summary>
        public static int DismissDramas()
        {
            var n = 0;
            // closed, not clicked: the first labeled button of a drama is "View" (opens the NPC info and blocks again)
            foreach (var t in new[] { UIType.DramaDialogue, UIType.DramaFortuitous, UIType.DramaHerdNPC, UIType.DramaDialogueBigTexture, UIType.DramaBigTexture, UIType.NPCInfo })
            {
                if (!g.ui.HasUI(t)) continue;
                g.ui.CloseUI(t);
                n++;
            }
            // "Target can no longer fight back... decides to beg for their life" (after defeating an NPC) and other
            // action course popups: OK once it is shown (clicked, not closed: its callback continues the flow)
            var course = g.ui.GetUI<UIActionCourseInfo>(UIType.ActionCourseInfo);
            if (course != null && course.btnOK != null && course.btnOK.gameObject.activeInHierarchy && course.btnOK.interactable)
            {
                course.btnOK.onClick.Invoke();
                n++;
            }
            return n;
        }

        /// <summary>Dungeon of a plain NPC fight on the map (DungeonBase type 40): a real, joinable battle.</summary>
        public const int NPC_FIGHT_DUNGEON_ID = 11;

        /// <summary>Starts a real duel with an NPC (the NPC does not kill or insult the player when it wins).</summary>
        public static void StartRealDuel(WorldUnitBase npc)
        {
            g.world.battle.IntoBattle(npc, NPC_FIGHT_DUNGEON_ID, null, new WorldBattleData
            {
                npcUnit = npc,
                isRealBattle = true,
                npcWinNotKillPlayerUnit = true,
                npcWinInsultPlayerUnit = false,
            });
        }

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

        /// <summary>The battle unit of a world unit (dead units included), null when not in the battle.</summary>
        public static UnitCtrlBase BattleUnitOf(WorldUnitBase w) => w == null || !InBattle ? null :
            ModBattleEvent.BattleUnitsIncludeDie.ToArray().FirstOrDefault(x => x.IsWorldUnit() && x.GetWorldUnit().GetUnitId() == w.GetUnitId());

        /// <summary>Clicks the first active button of the UI whose label is one of the labels (null when none).</summary>
        public static string ClickIn(UIBase ui, params string[] labels)
        {
            if (ui == null) return null;
            foreach (var btn in ui.GetComponentsInChildren<Button>())
            {
                if (btn == null || !btn.gameObject.activeInHierarchy || !btn.interactable) continue;
                var label = btn.GetComponentInChildren<Text>()?.text?.Trim();
                if (string.IsNullOrEmpty(label) || !labels.Any(l => string.Equals(l, label, StringComparison.OrdinalIgnoreCase))) continue;
                btn.onClick.Invoke();
                return label;
            }
            return null;
        }

        /// <summary>
        /// Kills a battle unit with the player as the killer (UnitDie with hitData.attackUnit = player). A hit goes
        /// through the whole Taoist damage pipeline (defense x basis can absorb it on buffed monsters), so a unit that
        /// survives the hit is killed with Die(). Returns "hit", "die" or null (nothing to kill).
        /// </summary>
        public static string PlayerKill(UnitCtrlBase target)
        {
            if (target == null || target.isDie) return null;
            var dmg = target.data.hp + target.data.maxHP.value + 999999;
            PlayerHit(target, dmg);
            if (target.isDie) return "hit";
            target.Die(true, new MartialTool.HitData(ModBattleEvent.PlayerUnit, null, 0, 1, dmg));
            return "die";
        }

        /// <summary>Hits a battle unit with the player (real UnitHit/UnitDie events, e.g. for kill counters).</summary>
        public static void PlayerHit(UnitCtrlBase target, int damage)
        {
            MartialTool.HitDanagePow(new MartialTool.HitData(ModBattleEvent.PlayerUnit, null, 0, 1, damage), target);
        }

        /// <summary>Ends the running battle (win or lose) like the game does.</summary>
        public static void EndBattle(bool win)
        {
            ModBattleEvent.SceneBattle.battleEnd.BattleEnd(win);
        }

        /// <summary>Wait-step body: clicks the result/confirm buttons until the map is shown again.</summary>
        public static bool LeaveBattle()
        {
            if (OnMap) return true;
            DismissDramas();
            ClickButton(CONFIRM);
            return false;
        }
    }
}
