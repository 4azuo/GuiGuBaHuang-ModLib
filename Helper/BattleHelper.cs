using ModLib.Attributes;
using ModLib.Mod;
using System.Linq;

namespace ModLib.Helper
{
    /// <summary>
    /// Drives a battle from code: start it (the "Fight" buttons), keep a unit alive, hit or kill a unit as another one,
    /// end it, leave the result screens. See <see cref="BattleSceneHelper"/> for the scene (camera, what is shown).
    /// </summary>
    [ActionCat("Battle")]
    public static class BattleHelper
    {
        /// <summary>Labels of the buttons that start a battle (pre-battle screen on the map, intro in the battle).</summary>
        public static readonly string[] FIGHT_LABELS = { "Fight", "战斗", "戰鬥", "开战", "開戰", "出战", "出戰" };

        /// <summary>Dungeon of a plain NPC fight on the map (DungeonBase type 40): a real battle others can join.</summary>
        public const int NPC_FIGHT_DUNGEON_ID = 11;

        /// <summary>True when a battle scene runs with the player's unit.</summary>
        public static bool InBattle => GameHelper.IsInBattlle() && ModBattleEvent.SceneBattle != null && ModBattleEvent.PlayerUnit != null;

        /// <summary>True when the player is on the world map (not in a battle).</summary>
        public static bool OnMap => !GameHelper.IsInBattlle() && g.ui.HasUI(UIType.MapMain);

        /// <summary>True when the battle really started (its intro closed): units take damage, die and join from then on.</summary>
        public static bool BattleStarted => InBattle && ModBattleEvent.SceneBattle.battleMap != null && ModBattleEvent.SceneBattle.battleMap.isStartBattle;

        /// <summary>
        /// One step to start a battle (call it until it returns true): closes the dramas in the way, clicks "Fight" on
        /// the map's pre-battle screen (MapBattlePre) and on the battle intro (BattleInfo).
        /// </summary>
        public static bool ClickFight()
        {
            DramaHelper.DismissDramas();
            if (BattleStarted) return true;
            if (g.ui.HasUI(UIType.MapBattlePre))
            {
                var pre = g.ui.GetUI<UIMapBattlePre>(UIType.MapBattlePre);
                if (UIClickHelper.ClickIn(pre, FIGHT_LABELS) == null)
                    pre?.onBattleCall?.Invoke();
                return false;
            }
            if (InBattle)
            {
                var info = g.ui.GetUI<UIBattleInfo>(UIType.BattleInfo);
                if (UIClickHelper.ClickIn(info, FIGHT_LABELS) == null)
                    UIClickHelper.ClickButton(FIGHT_LABELS);
            }
            return false;
        }

        /// <summary>
        /// One step to leave a finished battle (call it until it returns true): closes the dramas, clicks the result and
        /// confirm buttons until the world map shows.
        /// </summary>
        public static bool LeaveBattle()
        {
            if (OnMap) return true;
            DramaHelper.DismissDramas();
            UIClickHelper.ClickButton(UIClickHelper.CONFIRM_LABELS);
            return false;
        }

        /// <summary>
        /// The battle's pause menu (UIBattleExit: items obtained, escape, Resume / Settings / Quit Game) closed with its
        /// Resume button when it is open - it opens on Esc or when the game loses the focus, and stops the battle.
        /// True when it was open.
        /// </summary>
        public static bool ResumeIfPaused()
        {
            if (!g.ui.HasUI(UIType.BattleExit)) return false;
            var ui = g.ui.GetUI<UIBattleExit>(UIType.BattleExit);
            if (ui?.btnGameReturn != null && ui.btnGameReturn.gameObject.activeInHierarchy) ui.btnGameReturn.onClick.Invoke();
            else g.ui.CloseUI(UIType.BattleExit);
            return true;
        }

        /// <summary>Ends the running battle (won or lost) like the game does.</summary>
        public static void EndBattle(bool win)
        {
            ModBattleEvent.SceneBattle?.battleEnd?.BattleEnd(win);
        }

        /// <summary>Starts a real duel with an NPC on the map (the NPC neither kills nor insults the player when it wins).</summary>
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

        /// <summary>
        /// Keeps a unit alive (the NotDie state until the battle ends; call it again while needed): its hp refilled below
        /// half - NotDie keeps a unit at 0 hp, and the game ignores the hits of a unit at 0 hp. Default: the player.
        /// </summary>
        public static bool KeepAlive(UnitCtrlBase unit = null)
        {
            unit = unit ?? (InBattle ? ModBattleEvent.PlayerUnit : null);
            if (unit == null || unit.isDie) return false;
            if (!unit.IsState(UnitStateType.NotDie))
                unit.AddState(UnitStateType.NotDie, float.MaxValue);
            if (unit.data.hp < unit.data.maxHP.value / 2)
                unit.data.hp = unit.data.maxHP.value;
            return true;
        }

        /// <summary>A hit on <paramref name="target"/> by <paramref name="attacker"/> (default: the player), with the real UnitHit / UnitDie events.</summary>
        public static void Hit(UnitCtrlBase target, int damage, UnitCtrlBase attacker = null)
        {
            MartialTool.HitDanagePow(new MartialTool.HitData(attacker ?? ModBattleEvent.PlayerUnit, null, 0, 1, damage), target);
        }

        /// <summary>
        /// Kills <paramref name="target"/> with <paramref name="attacker"/> (default: the player) as the killer: a hit
        /// bigger than its hp (it goes through the damage pipeline of the mods, which may absorb it), then Die() when it
        /// survived. Returns "hit", "die", or null when there is nothing to kill.
        /// </summary>
        public static string Kill(UnitCtrlBase target, UnitCtrlBase attacker = null)
        {
            if (target == null || target.isDie) return null;
            attacker = attacker ?? ModBattleEvent.PlayerUnit;
            var dmg = target.data.hp + target.data.maxHP.value + 999999;
            Hit(target, dmg, attacker);
            if (target.isDie) return "hit";
            target.Die(true, new MartialTool.HitData(attacker, null, 0, 1, dmg));
            return "die";
        }

        /// <summary>The battle unit of a world unit (dead ones included), null when it is not in the battle.</summary>
        public static UnitCtrlBase GetBattleUnit(this WorldUnitBase w) => w == null || !InBattle ? null :
            ModBattleEvent.BattleUnitsIncludeDie.ToArray().FirstOrDefault(x => x.IsWorldUnit() && x.GetWorldUnit().GetUnitId() == w.GetUnitId());
    }
}
