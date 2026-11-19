using ModLib.Helper;
using ModLib.Mod;
using MOD_nE7UL2.Mod;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Battle
{
    /// <summary>
    /// TB-16 (AnimaWeaponEvent): divine artifact hotkeys with the 3 artifacts of the new character.
    /// Settings > Bindings has a row per artifact in the combat and map lists (copied game rows); a click waits for a
    /// key, a game key is refused, a key of another artifact moves, Esc cancels. On the map the buttons are side by side
    /// and show their keys, a hotkey opens the artifact window. In a battle the buttons show their keys, the hotkeys are
    /// read every frame and use the artifact. The keys are restored at the end.
    /// </summary>
    public class TB16_ArtifactHotkeys : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TB-16";

        private static AnimaWeaponEvent Ev => AnimaWeaponEvent.Instance;
        // built when used (in the game): Create() also runs in the unit tests, outside the game
        private static KeyCode[] TEST_KEYS => new[] { KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9 };

        private static Text KeyText(GameObject row) =>
            row.GetComponentsInChildren<Text>(true).OrderBy(x => x.transform.position.x).LastOrDefault();

        private static string CdText(UIBattleInfoBase.Info info, GameAnimaWeapon w)
        {
            if (info == null) return "?";
            if (w == GameAnimaWeapon.PiscesPendant) return $"{info.imgPiscesSkillCD.fillAmount:0.00}/{info.textPiscesSkillCD.text}";
            if (w == GameAnimaWeapon.HootinEye) return $"{info.imgGodEyeSkillCD.fillAmount:0.00}/{info.textGodEyeSkillCD.text}";
            return $"{info.imgDevilDemonSkillCD.fillAmount:0.00}/{info.textDevilDemonSkillCD.text}";
        }

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            var saved = new Dictionary<int, int>(); // weapon -> key (ints: no Il2Cpp type in Create())
            WorldUnitBase npc = null;
            return new Scenario { Id = "TB-16", Feature = "AnimaWeaponEvent", Title = "Divine artifact hotkeys: bindings, map, battle", Overlay = false }
                .Step("artifacts", c =>
                {
                    if (!c.Assert(Ev != null, "AnimaWeaponEvent loaded")) return;
                    foreach (var w in AnimaWeaponEvent.WEAPONS) saved[(int)w] = (int)Ev.GetKey(w);
                    GodArtifactHelper.SetAnimaWeapon(AnimaWeaponEvent.WEAPONS);
                    GodArtifactHelper.UpdateUI();
                    foreach (var w in AnimaWeaponEvent.WEAPONS) Ev.SetKey(w, KeyCode.None);
                    c.Assert(AnimaWeaponEvent.WEAPONS.All(GodArtifactHelper.HasAnimaWeapon), "the 3 divine artifacts are active");
                })
                .Step("open settings", c => g.ui.OpenUISafe<UIGameSetting>(UIType.GameSetting), 1000)
                .Step("open Taoist settings", c =>
                {
                    var btn = FindButtons(UIType.GameSetting, GameTool.LS("setting000")).LastOrDefault();
                    if (!c.Assert(btn != null, "Taoist settings button")) return;
                    btn.Item.onClick.Invoke();
                }, 1500)
                .Step("binding rows", c =>
                {
                    c.Assert(g.ui.HasUI(UIType.TextInfoLong), "Taoist settings opened");
                    var all = AnimaWeaponEvent.GetRows();
                    var rows = all.Where(r => AnimaWeaponEvent.WEAPONS.Any(w => r.name == AnimaWeaponEvent.ROW_PREFIX + w)).ToList();
                    c.AssertEqual(3, rows.Count, "a binding row per artifact (copies of the game key rows)");
                    // the damage calculation hotkey: its own section ("Other hotkeys") under the artifacts
                    var dmg = all.FirstOrDefault(r => r.name == AnimaWeaponEvent.ROW_PREFIX + AnimaWeaponEvent.DamageInfoRow);
                    c.Assert(dmg != null && dmg.GetComponent<RectTransform>().anchoredPosition.y < rows.Min(r => r.GetComponent<RectTransform>().anchoredPosition.y),
                        "damage calculation row under the artifact rows");
                    var title = UIHelper.GetUICustomBase(UIType.TextInfoLong).SelectMany(x => x.Items).OfType<ModLib.Object.UIItemText>()
                        .FirstOrDefault(x => x.Item.text == GameTool.LS("setting015"));
                    c.Assert(title != null && dmg != null && title.Item.rectTransform.anchoredPosition.y > dmg.GetComponent<RectTransform>().anchoredPosition.y,
                        $"'{GameTool.LS("setting015")}' title above it (not in the divine artifact section)");
                    var ys = rows.Select(r => r.GetComponent<RectTransform>().anchoredPosition.y).ToList();
                    c.Assert(ys.Distinct().Count() == 3, $"rows one below the other (y: {string.Join(", ", ys.Select(y => y.ToString("0")))})");
                    foreach (var w in AnimaWeaponEvent.WEAPONS)
                    {
                        var row = rows.FirstOrDefault(x => x.name == AnimaWeaponEvent.ROW_PREFIX + w);
                        var texts = row?.GetComponentsInChildren<Text>(true).Select(x => x.text).ToList() ?? new List<string>();
                        c.Assert(texts.Contains(AnimaWeaponEvent.WeaponName(w)) && texts.Contains("-"), $"row {w}: {string.Join(" | ", texts)}");
                    }
                })
                .Step("bind keys", c =>
                {
                    var rows = AnimaWeaponEvent.GetRows();
                    if (!c.Assert(rows.Count > 0, "binding rows")) return;
                    for (var i = 0; i < 3; i++)
                    {
                        var w = AnimaWeaponEvent.WEAPONS[i];
                        var row = rows.First(x => x.name == AnimaWeaponEvent.ROW_PREFIX + w);
                        var tg = row.GetComponentsInChildren<Toggle>(true).FirstOrDefault(); // the game row is a toggle: clicked = on
                        if (tg != null) tg.isOn = true; else row.GetComponentsInChildren<Button>(true).First().onClick.Invoke();
                        c.Assert(AnimaWeaponEvent.Capturing == w && KeyText(row)?.text == "...", $"{w}: click waits for a key ('{KeyText(row)?.text}')");
                        Ev.ApplyCapture(TEST_KEYS[i]); // what pressing the key does
                        c.AssertEqual(TEST_KEYS[i].ToString(), Ev.GetKey(w).ToString(), $"{w} bound");
                        c.AssertEqual(AnimaWeaponEvent.KeyName(TEST_KEYS[i]), KeyText(row)?.text, $"{w} row shows its key");
                        c.Assert(tg == null || !tg.isOn, "the row is released after the key");
                    }
                })
                .Step("game key refused", c =>
                {
                    var gameKey = g.data.globle.key.battleGodEye;
                    Ev.StartCapture(GameAnimaWeapon.HootinEye);
                    Ev.ApplyCapture(gameKey);
                    c.AssertEqual(TEST_KEYS[1].ToString(), Ev.GetKey(GameAnimaWeapon.HootinEye).ToString(), $"the game's artifact key {gameKey} is refused");
                    c.Assert(AnimaWeaponEvent.GameKeyUsing(gameKey) != null, $"{gameKey} is a game key ({AnimaWeaponEvent.GameKeyUsing(gameKey)})");
                    ClickButton(CONFIRM);
                }, 1000)
                .Step("move and cancel", c =>
                {
                    Ev.StartCapture(GameAnimaWeapon.HootinEye);
                    Ev.ApplyCapture(TEST_KEYS[0]);
                    c.Assert(Ev.GetKey(GameAnimaWeapon.HootinEye) == TEST_KEYS[0] && Ev.GetKey(GameAnimaWeapon.PiscesPendant) == KeyCode.None, "a key of another artifact moves to the new one");
                    Ev.StartCapture(GameAnimaWeapon.HootinEye);
                    Ev.ApplyCapture(TEST_KEYS[1]);
                    Ev.StartCapture(GameAnimaWeapon.PiscesPendant);
                    Ev.ApplyCapture(TEST_KEYS[0]);
                    Ev.StartCapture(GameAnimaWeapon.DevilDemon);
                    Ev.ApplyCapture(KeyCode.Escape);
                    c.Assert(Ev.GetKey(GameAnimaWeapon.DevilDemon) == TEST_KEYS[2] && AnimaWeaponEvent.Capturing == GameAnimaWeapon.None, "Esc cancels");
                    Ev.StartCapture(GameAnimaWeapon.DevilDemon);
                    Ev.ApplyCapture(KeyCode.Backspace);
                    c.Assert(Ev.GetKey(GameAnimaWeapon.DevilDemon) == KeyCode.None, "Backspace clears");
                    Ev.StartCapture(GameAnimaWeapon.DevilDemon);
                    Ev.ApplyCapture(TEST_KEYS[2]);
                })
                .Step("close settings", c =>
                {
                    CloseAll(UIType.TextInfoLong, UIType.GameSetting);
                    c.Assert(!g.ui.HasUI(UIType.GameSetting), "settings closed");
                }, 1000)
                .Step("map buttons", c =>
                {
                    Ev.Arrange();
                    var info = g.ui.GetUI<UIMapMain>(UIType.MapMain)?.uiPlayerInfo;
                    if (!c.Assert(info != null, "map UI")) return;
                    var xs = new[] { info.btnPiscesPendant, info.btnGodEye, info.btnDevilDemon }.Select(b => b.GetComponent<RectTransform>().anchoredPosition.x).ToList();
                    c.Assert(xs.Distinct().Count() == 3, $"the 3 buttons are side by side (x: {string.Join(", ", xs.Select(x => x.ToString("0")))})");
                    c.Assert(info.textPiscesKey.text == "7" && info.textGodEyeKey.text == "8" && info.textDevilDemonKey.text == "9",
                        $"map buttons show their keys ({info.textPiscesKey.text}, {info.textGodEyeKey.text}, {info.textDevilDemonKey.text})");
                })
                .Step("map hotkey", c =>
                {
                    c.Assert(Ev.Trigger(GameAnimaWeapon.HootinEye), "God Eye hotkey on the map");
                    c.AssertEqual("map:HootinEye", AnimaWeaponEvent.LastUse, "used on the map");
                }, 1500)
                .Step("artifact window", c =>
                {
                    c.Assert(g.ui.HasUI(UIType.GodEye), "the God Eye window opened");
                    CloseAll(UIType.GodEye, UIType.PiscesPendant, UIType.DevilDemon);
                }, 1000)
                .Step("start duel", c =>
                {
                    npc = Npcs.Where(x => !HirePeopleEvent.IsHired(x) && !x.isDie).OrderBy(x => x.GetGradeLvl()).FirstOrDefault();
                    if (!c.Assert(npc != null, "an NPC to fight")) return;
                    c.Bag["ticks"] = AnimaWeaponEvent.MonoTicksInBattle;
                    g.world.battle.IntoBattle(npc, MapBuildPropertyEvent.CATCH_SNEAKY_DUNGEON_ID, battleData: new WorldBattleData { npcUnit = npc });
                })
                .Wait("battle loaded", c => { StartBattle(); ProtectPlayer(); return npc == null || InBattle; }, 90000, 1000)
                .Wait("battle started", c => { ProtectPlayer(); return npc == null || StartBattle(); }, 60000, 1000)
                .Step("battle buttons", c =>
                {
                    if (!InBattle) return;
                    Ev.Arrange();
                    var info = g.ui.GetUI<UIBattleInfo>(UIType.BattleInfo)?.uiInfo;
                    if (!c.Assert(info != null, "battle UI")) return;
                    c.Assert(info.textKeyPiscesSkill.text == "7" && info.textKeyGodEyeSkill.text == "8" && info.textKeyDevilDemonSkill.text == "9",
                        $"battle buttons show their keys ({info.textKeyPiscesSkill.text}, {info.textKeyGodEyeSkill.text}, {info.textKeyDevilDemonSkill.text})");
                    var xs = new[] { info.goPiscesPendant, info.goGodEye, info.goDevilDemon }.Where(x => x.activeSelf).Select(x => x.GetComponent<RectTransform>().anchoredPosition.x).ToList();
                    c.Assert(xs.Distinct().Count() == xs.Count, $"active battle buttons side by side ({xs.Count}: {string.Join(", ", xs.Select(x => x.ToString("0")))})");
                    c.Assert(AnimaWeaponEvent.MonoTicksInBattle > (int)c.Bag["ticks"], $"hotkeys are read during the battle ({AnimaWeaponEvent.MonoTicksInBattle - (int)c.Bag["ticks"]} frames)");
                    // only the artifacts with a battle skill have a battle button
                    var active = AnimaWeaponEvent.WEAPONS.Where(w => AnimaWeaponEvent.BattleButton(w)?.activeInHierarchy ?? false).ToList();
                    c.Info($"artifacts with a battle skill: {string.Join(", ", active)}");
                    if (!c.Assert(active.Count > 0, "at least one artifact usable in the battle")) return;
                    c.Bag["w"] = (int)active[0];
                    c.Bag["cd"] = CdText(info, active[0]);
                    var inactive = AnimaWeaponEvent.WEAPONS.Except(active).FirstOrDefault();
                    if (inactive != GameAnimaWeapon.None)
                        c.Assert(!Ev.Trigger(inactive), $"{inactive} has no battle skill: its hotkey does nothing");
                }, 2000)
                .Step("battle hotkey", c =>
                {
                    if (!InBattle || !c.Bag.ContainsKey("w")) return;
                    ProtectPlayer();
                    var w = (GameAnimaWeapon)(int)c.Bag["w"];
                    c.Assert(Ev.Trigger(w), $"{w} hotkey in the battle");
                    c.AssertEqual($"battle:{w}", AnimaWeaponEvent.LastUse, "used in the battle");
                }, 2000)
                .Step("artifact used", c =>
                {
                    if (!InBattle || !c.Bag.ContainsKey("w")) return;
                    var w = (GameAnimaWeapon)(int)c.Bag["w"];
                    var cd = CdText(g.ui.GetUI<UIBattleInfo>(UIType.BattleInfo)?.uiInfo, w);
                    // the effect depends on the battle (e.g. the Devil Demon pot needs a weakened monster): shown, not checked
                    c.Info($"{w} skill state after the hotkey: cooldown {(string)c.Bag["cd"]} -> {cd}");
                }, 2000)
                .Step("win", c => { if (InBattle) EndBattle(true); }, 1500)
                .Wait("back to the map", c => LeaveBattle(), 90000, 2000)
                .Step("restore", c =>
                {
                    foreach (var kv in saved) Ev.SetKey((GameAnimaWeapon)kv.Key, (KeyCode)kv.Value);
                    CacheHelper.SaveGlobalCache(Ev);
                    c.Assert(OnMap, "back on the map, keys restored");
                });
        }
    }
}
