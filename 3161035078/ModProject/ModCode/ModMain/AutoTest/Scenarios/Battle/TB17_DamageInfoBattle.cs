using ModLib.Helper;
using ModLib.Mod;
using MOD_nE7UL2.Mod;
using System.Linq;
using UnityEngine;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Battle
{
    /// <summary>
    /// TB-17 (DamageInfo): the damage calculation window in a battle. The battle screen has the "Damage Calc" button
    /// under the damage multiplier (top centre); a hit dealt and a hit taken are recorded step by step with the foe
    /// (its numbers of that time, kept in the save for page 2); the hotkey row opens the window with the foe as
    /// the target and pauses the battle, the hotkey again closes it and the battle runs again.
    /// </summary>
    public class TB17_DamageInfoBattle : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TB-17";

        public Scenario Create()
        {
            WorldUnitBase npc = null;
            return new Scenario { Id = "TB-17", Feature = "DamageInfo", Title = "Damage calculation in a battle: button, hotkey, pause, last hits, recent foes", Overlay = false }
                .Step("start duel", c =>
                {
                    c.Assert(DamageInfoEvent.Instance != null, "DamageInfoEvent loaded (recent foes kept in the save)");
                    npc = Npcs.Where(x => !HirePeopleEvent.IsHired(x) && !x.isDie).OrderBy(x => x.GetGradeLvl()).FirstOrDefault();
                    if (!c.Assert(npc != null, "an NPC to fight")) return;
                    c.Info($"foe: {npc.GetName()} grade {npc.GetGradeLvl()}");
                    g.world.battle.IntoBattle(npc, MapBuildPropertyEvent.CATCH_SNEAKY_DUNGEON_ID, battleData: new WorldBattleData { npcUnit = npc });
                })
                .Wait("battle loaded", c => { StartBattle(); ProtectPlayer(); return npc == null || InBattle; }, 90000, 1000)
                .Wait("battle started", c => { ProtectPlayer(); return npc == null || StartBattle(); }, 60000, 1000)
                .Step("button", c =>
                {
                    if (!InBattle) return;
                    var btn = FindButtons(UIType.BattleInfo, GameTool.LS("dmginfo_button")).FirstOrDefault();
                    if (!c.Assert(btn != null, $"'{GameTool.LS("dmginfo_button")}' button on the battle screen")) return;
                    var y = btn.Item.GetComponent<RectTransform>().anchoredPosition.y;
                    c.Assert(y > 0, $"in the top half of the screen (y {y:0}), away from the skill bar");
                    c.Snap("button");
                }, 1000)
                .Step("hits", c =>
                {
                    if (!InBattle) return;
                    var foe = npc.GetBattleUnit();
                    if (!c.Assert(foe != null, "the foe is in the battle")) return;
                    ProtectPlayer();
                    BattleHelper.KeepAlive(foe);
                    BattleHelper.Hit(foe, 50);
                    BattleHelper.Hit(ModBattleEvent.PlayerUnit, 30, foe);
                    c.Assert(DamageInfo.LastDealt != null && DamageInfo.LastDealt.Steps.Count > 0, $"hit dealt recorded: {DamageInfo.LastDealt?.Attacker} -> {DamageInfo.LastDealt?.Target}: {(DamageInfo.LastDealt == null ? "-" : DamageInfo.Steps(DamageInfo.LastDealt.Steps))}");
                    c.Assert(DamageInfo.LastTaken != null && DamageInfo.LastTaken.Steps.Count > 0, $"hit taken recorded: {(DamageInfo.LastTaken == null ? "-" : DamageInfo.Steps(DamageInfo.LastTaken.Steps))}");
                    c.Assert(DamageInfo.LastFoe != null && DamageInfo.LastFoe.Pointer == foe.Pointer, "the foe is the target of the window");
                    var rec = DamageInfo.RecentFoes(true).FirstOrDefault();
                    c.Assert(rec != null && rec.UnitId == npc.GetUnitId(), $"the foe is the newest recent cultivator ({rec?.Name})");
                    if (rec != null)
                        c.Assert(rec.Atk > 0 && rec.HpMax > 0 && rec.Grade > 0, $"its numbers of that time: ATK {rec.Atk}, DEF {rec.Def}, HP {rec.HpMax}, grade {rec.Grade}, year {rec.Year} month {rec.Month}");
                }, 1000)
                .Step("hotkey opens", c =>
                {
                    if (!InBattle) return;
                    ProtectPlayer();
                    AnimaWeaponEvent.Instance.Trigger(AnimaWeaponEvent.DamageInfoRow);
                    c.Assert(DamageInfo.IsOpen, "the hotkey opens the window");
                    c.Assert(UnityEngine.Time.timeScale == 0f && DamageInfo.PausedByWindow, $"the battle is paused (time scale {UnityEngine.Time.timeScale})");
                    var texts = DamageInfo.Window?.Items.OfType<ModLib.Object.UIItemText>().Select(x => TextIconHelper.GetShownText(x.Item)).ToList();
                    c.Assert(texts != null && texts.Any(x => x.Contains(npc.GetName())), "the foe is the target");
                    c.Assert(texts != null && texts.Any(x => x.Contains("<sprite")), "lines with icons");
                    c.Snap("window");
                    // the runner works with the game time: the game runs again for the rest of the test (the window
                    // stays open; it restores the speed when closed)
                    UnityEngine.Time.timeScale = 1f;
                }, 1500)
                .Step("recent foes page", c =>
                {
                    if (!DamageInfo.IsOpen) return;
                    DamageInfo.Window.NextPage();
                    var shown = DamageInfo.Window.Items.OfType<ModLib.Object.UIItemText>().Where(x => x.IsActive()).Select(x => TextIconHelper.GetShownText(x.Item)).ToList();
                    c.Assert(shown.Any(x => x.Contains(npc.GetName())), "page 2 shows the foe's card");
                    c.Snap("recent-foes");
                }, 1500)
                .Step("hotkey closes", c =>
                {
                    if (!InBattle) return;
                    AnimaWeaponEvent.Instance.Trigger(AnimaWeaponEvent.DamageInfoRow);
                    c.Assert(!DamageInfo.IsOpen, "the hotkey again closes it");
                    c.Assert(!DamageInfo.PausedByWindow && UnityEngine.Time.timeScale > 0f, $"the battle runs again (time scale {UnityEngine.Time.timeScale})");
                }, 1000)
                .Step("win", c => { if (InBattle) EndBattle(true); }, 1500)
                .Wait("back to the map", c => LeaveBattle(), 90000, 2000)
                .Step("kept in the save", c =>
                {
                    c.Assert(OnMap, "back on the map");
                    c.Assert(DamageInfo.RecentFoes(true).Any(x => x.UnitId == npc?.GetUnitId()), "the foe is still in the recent foes after the battle");
                });
        }
    }
}
