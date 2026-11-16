using ModLib.Helper;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Mod;
using System;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Title
{
    /// <summary>
    /// TP-01 (pre-test): creates a new character for the run, with the S&amp;M trainer on and the CrueltyShura
    /// difficulty, in a free save slot (never over an existing save). Every other scenario then runs on it, from
    /// the same starting state on every run. The created save is removed by the restore of run-autotest.ps1.
    /// Skipped when the run uses an existing save (-Save / -ExistingSave).
    /// </summary>
    public class TP01_NewCharacter : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TP-01";

        /// <summary>True when the run asked for a new character (set by AutoTestEvent from the request).</summary>
        public static bool Requested { get; set; }

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            var slot = -1;
            DateTime lastClick = DateTime.MinValue;
            var sawIntro = "";
            return new Scenario { Id = "TP-01", Feature = "Pre-test (new character)", Title = "Create a new character (trainer on, CrueltyShura, 3 artifacts, intro skipped)", Scope = ScenarioScope.Title, Overlay = false }
                .Step("S&M for the test", c =>
                {
                    if (!Requested) { c.Skip("the run uses an existing save"); return; }
                    var sm = SMGlobalConfigsEvent.Instance;
                    sm.EnableTrainer = true;
                    CacheHelper.SaveGlobalCache(sm);
                    c.Assert(sm.EnableTrainer, "S&M trainer on (saved in the global configs copied by the new save)");
                })
                .Step("open the save list", c =>
                {
                    if (!Requested) return;
                    var ui = g.ui.GetUI<UILogin>(UIType.Login);
                    if (!c.Assert(ui != null, "title screen")) return;
                    ui.btnInto.onClick.Invoke();
                }, 1000)
                .Step("free slot", c =>
                {
                    if (!Requested) return;
                    var items = g.ui.GetUI<UILogin>(UIType.Login)?.allCacheItem?.ToArray() ?? new UILoginCacheItem[0];
                    slot = Array.FindIndex(items, x => x != null && x.cacheData == null);
                    c.Info($"save slots: {items.Length}, used: {items.Count(x => x?.cacheData != null)}");
                    c.Assert(slot >= 0, slot >= 0 ? $"free save slot {slot}" : "no free save slot (no existing save is overwritten)");
                }, 2500)
                // the game's own path (difficulty screen -> character creation): opening UICreatePlayer directly left
                // the world data incomplete (NullReferenceException in UICreatePlayer.CreateGameInfo, endless loading)
                .Step("difficulty screen", c =>
                {
                    if (!Requested || slot < 0) return;
                    var diff = g.ui.OpenUISafe<UIDifficulty>(UIType.Difficulty);
                    diff.InitData(slot);
                    c.Info($"difficulty screen for slot {slot}");
                }, 1000)
                .Step("select CrueltyShura", c =>
                {
                    if (!Requested || slot < 0) return;
                    var diff = g.ui.GetUI<UIDifficulty>(UIType.Difficulty);
                    if (!c.Assert(diff != null, "difficulty screen open")) return;
                    var items = diff.itemList.ToArray();
                    var item = items.FirstOrDefault(x => x.gameLevel == GameLevelType.CrueltyShura);
                    if (!c.Assert(item != null, $"CrueltyShura listed ({string.Join(", ", items.Select(x => $"{x.gameLevel}{(x.isUnlock ? "" : " locked")}"))})")) return;
                    c.Assert(item.isUnlock, "CrueltyShura unlocked");
                    // click the item like a player (SelectItem alone only highlighted it: "Select an unlocked difficulty!")
                    var roots = diff.GetComponentsInChildren<UnityEngine.UI.Button>().Where(b => b.gameObject.activeInHierarchy && b.name == "G:btnRoot").ToList();
                    // Array.IndexOf does not match Il2Cpp wrappers: find the item by its level
                    var index = -1;
                    for (var i = 0; i < items.Length; i++) if (items[i].gameLevel == GameLevelType.CrueltyShura) { index = i; break; }
                    if (roots.Count == items.Length && index >= 0) roots[index].onClick.Invoke();
                    else diff.SelectItem(item.confDifficultyItem);
                    // the OK button refuses ("Select an unlocked difficulty!") while selLevelItem is empty
                    if (diff.selLevelItem == null) diff.selLevelItem = item.confDifficultyItem;
                    c.Info($"difficulty item {index} clicked ({roots.Count} item buttons), selected: {diff.selLevelItem?.id}");
                    // amount of NPCs: Sparse (first of Sparse / Normal / Plentiful): the world is created faster
                    var npcNums = diff.npcNums;
                    var before = diff.npcCountId;
                    if (npcNums != null && npcNums.Length > 0)
                    {
                        npcNums[0].isOn = true;
                        npcNums[0].onValueChanged.Invoke(true);
                    }
                    c.Assert(npcNums != null && npcNums.Length > 0 && npcNums[0].isOn, $"amount of NPCs: Sparse (npc count id {before} -> {diff.npcCountId})");
                    c.Info($"selected, npc count id {diff.npcCountId}; buttons: {string.Join(", ", diff.GetComponentsInChildren<UnityEngine.UI.Button>().Where(b => b.gameObject.activeInHierarchy).Select(b => $"{b.name}[{b.GetComponentInChildren<UnityEngine.UI.Text>()?.text?.Trim()}]"))}");
                }, 1500)
                .Wait("creation screen", c =>
                {
                    if (!Requested || slot < 0) return true;
                    if (g.ui.HasUI(UIType.CreatePlayer)) return true;
                    // "Chaos mode is extremely difficult... Choose Chaos level?" Yes/No popup
                    if (g.ui.HasUI(UIType.CheckPopup))
                    {
                        ClickIn(g.ui.GetUI<UICheckPopup>(UIType.CheckPopup), "Yes", "OK", "是", "确定", "確定");
                        return false;
                    }
                    var diff = g.ui.GetUI<UIDifficulty>(UIType.Difficulty);
                    if (diff != null) ClickIn(diff, "OK", "Confirm", "Start", "Next", "Enter", "确定", "開始", "开始", "确认");
                    ClickButton(CONFIRM);
                    return false;
                }, 40000, 1500)
                .Step("stats tab", c =>
                {
                    if (!Requested || slot < 0) return;
                    var ui = g.ui.GetUI<UICreatePlayer>(UIType.CreatePlayer);
                    // the visible tab toggle (tglProperty_En in English, tglProperty otherwise): the hidden one of the other
                    // language mixed the Appear and Stats contents; isOn only, the game's toggle group switches the tab
                    var statsTab = new[] { ui?.tglProperty_En, ui?.tglProperty }.FirstOrDefault(t => t != null && t.gameObject.activeInHierarchy);
                    if (!c.Assert(statsTab != null, "Stats tab of the creation screen")) return;
                    statsTab.isOn = true;
                    c.Info($"tab: {statsTab.name}");
                }, 1500)
                .Step("confirm", c =>
                {
                    if (!Requested || slot < 0) return;
                    var ui = g.ui.GetUI<UICreatePlayer>(UIType.CreatePlayer);
                    if (!c.Assert(ui != null, "creation screen open")) return;
                    // the game wants 3 born destinies (nature): turn the destiny toggles on until creation is allowed
                    var picked = new System.Collections.Generic.List<string>();
                    // the born destinies are on the Stats tab (switched in the previous step): only the toggles of the destiny
                    // list (G:goBornLuck / G:goBornLuck_En); the game keeps the chosen ones in uiProperty.selectLuck and refuses
                    // to start without 3 (ChechIsCanCreate does not check them). Appearance/trait/emotion toggles are left alone.
                    var all = ui.GetComponentsInChildren<UnityEngine.UI.Toggle>().Where(t => t.gameObject.activeInHierarchy && (t.transform.parent?.name ?? "").StartsWith("G:goBornLuck")).ToList();
                    c.Info($"destiny toggles: {string.Join(", ", all.Select(t => $"{t.name}[{t.isOn}]"))}");
                    foreach (var t in all)
                    {
                        if (ui.uiProperty.selectLuck.Count >= 3) break;
                        if (t.isOn || !t.interactable) continue;
                        t.isOn = true;
                        picked.Add($"{t.transform.parent?.name}/{t.name}");
                    }
                    c.Info($"destinies selected: {ui.uiProperty.selectLuck.Count} ({string.Join(", ", picked)})");
                    if (!c.Assert(ui.uiProperty.selectLuck.Count >= 3 && ui.ChechIsCanCreate(), "3 destinies selected, creation allowed")) return;
                    // NewGameEvent: its checkboxes are on the screen; all 3 divine artifacts + skip the intro and the first battle
                    // ticked like a player (the checkboxes save the choice through their change action)
                    var cover = UIHelper.UIs.FirstOrDefault(x => x.UITypeName == UIType.CreatePlayer.uiName);
                    var boxes = cover?.Items.OfType<ModLib.Object.UIItemToggle>().ToList() ?? new System.Collections.Generic.List<ModLib.Object.UIItemToggle>();
                    if (!c.AssertEqual(4, boxes.Count, "NewGameEvent checkboxes on the creation screen")) return;
                    foreach (var b in boxes) b.Item.isOn = true;
                    var ng = NewGameEvent.Instance;
                    if (!c.Assert(ng.PiscesPendant && ng.GodEye && ng.DevilDemon && ng.SkipIntro, "3 artifacts + skip intro checked"))
                    {
                        ng.PiscesPendant = ng.GodEye = ng.DevilDemon = ng.SkipIntro = true;
                        CacheHelper.SaveGlobalCache(ng);
                    }
                    ui.OnOkClick();
                }, 3000)
                .Wait("world created", c =>
                {
                    if (!Requested || slot < 0) return true;
                    if (GameHelper.IsInGame() && g.ui.HasUI(UIType.MapMain) && !GameHelper.IsLoadingScreen() && !GameHelper.IsInBattlle()) return true;
                    if (GameHelper.IsInBattlle() && !sawIntro.Contains("battle")) sawIntro += " battle";
                    if (g.ui.HasUI(UIType.IntoGameDrama) && !sawIntro.Contains("drama")) sawIntro += " drama";
                    if ((DateTime.Now - lastClick).TotalSeconds >= 3)
                    {
                        lastClick = DateTime.Now;
                        // confirmation popups, intro dramas and an intro battle on the way to the map
                        DismissDramas();
                        ClickButton(CONFIRM);
                        if (GameHelper.IsInBattlle())
                        {
                            if (StartBattle() && InBattle) EndBattle(true);
                            LeaveBattle();
                        }
                    }
                    return false;
                }, 300000, 1000)
                .Step("character", c =>
                {
                    if (!Requested || slot < 0) return;
                    c.Assert(GameHelper.IsInGame(), $"in the new save: {Player?.GetName()} ({Player?.GetUnitId()})");
                    c.AssertEqual(1, Player.GetGradeLvl(), "grade 1");
                    c.AssertEqual(GameLevelType.CrueltyShura.ToString(), GameHelper.GetGameLevel().ToString(), "difficulty");
                    c.Assert(SMLocalConfigsEvent.Instance.Configs.EnableTrainer, "trainer on in the save configs");
                    c.Assert(IsLoaded(ModConst.TRAINER_EVENT), "TrainerEvent loaded");
                    // NewGameEvent: the 3 artifacts, no intro screen / first battle left
                    c.Assert(GodArtifactHelper.IsEnabledPiscesPendant() && GodArtifactHelper.IsEnabledGodEye() && GodArtifactHelper.IsEnabledDevilDemon(),
                        $"divine artifacts: {string.Join(", ", GodArtifactHelper.GetAnimaWeapon().ToArray())}");
                    c.Assert(!g.ui.HasUI(UIType.IntoGameInfo) && !g.ui.HasUI(UIType.IntoGameDrama) && !g.ui.HasUI(UIType.GodEye) && !GameHelper.IsInBattlle(), "intro skipped (no intro screen, artifact window or battle)");
                    c.Assert(!sawIntro.Contains("battle"), sawIntro == "" ? "no intro drama/battle seen on the way" : $"seen on the way:{sawIntro}");
                    c.Info($"year {GameHelper.GetGameYear()}/{GameHelper.GetGameMonth()}, town {AnyTown?.name}, NPCs {Npcs.Count()}");
                }, 5000);
        }
    }
}
