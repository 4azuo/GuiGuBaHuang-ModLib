using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Helper;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Several divine artifacts at once (NewGameEvent, trainer...): the game draws their buttons at the same place and
    /// has one artifact key only.
    /// - Map (UIMapMain) and battle (UIBattleInfo): the active artifact buttons are laid out side by side, each button
    ///   shows its own hotkey.
    /// - Each artifact has its own hotkey: on the map it opens the artifact window (like a click on its button), in a
    ///   battle it uses the artifact skill. The game's artifact key keeps working.
    /// - The hotkeys are bound in Taoist Settings (rows copied from the key binding rows of the game settings, same look
    ///   and use: click, then press a key; Esc cancels, Backspace/Delete clears; keys of the game are refused).
    /// The hotkeys are global (all saves).
    /// </summary>
    [Cache(ModConst.ANIMA_WEAPON_EVENT, CacheType = CacheAttribute.CType.Global, WorkOn = CacheAttribute.WType.All)]
    public class AnimaWeaponEvent : ModEvent
    {
        public static AnimaWeaponEvent Instance { get; set; }

        // no field of a game/Unity enum type: the class must load outside the game (unit tests)
        public static GameAnimaWeapon[] WEAPONS => new[] { GameAnimaWeapon.PiscesPendant, GameAnimaWeapon.HootinEye, GameAnimaWeapon.DevilDemon };
        public const string ROW_PREFIX = "Taoist:Key:";

        /// <summary>Hotkey (KeyCode value) of the Bagua Jade (Pisces Pendant).</summary>
        public int PiscesPendantKeyCode { get; set; }
        /// <summary>Hotkey (KeyCode value) of the God Eye.</summary>
        public int GodEyeKeyCode { get; set; }
        /// <summary>Hotkey (KeyCode value) of the Devil Demon pot.</summary>
        public int DevilDemonKeyCode { get; set; }

        [Newtonsoft.Json.JsonIgnore] public KeyCode PiscesPendantKey { get => (KeyCode)PiscesPendantKeyCode; set => PiscesPendantKeyCode = (int)value; }
        [Newtonsoft.Json.JsonIgnore] public KeyCode GodEyeKey { get => (KeyCode)GodEyeKeyCode; set => GodEyeKeyCode = (int)value; }
        [Newtonsoft.Json.JsonIgnore] public KeyCode DevilDemonKey { get => (KeyCode)DevilDemonKeyCode; set => DevilDemonKeyCode = (int)value; }

        private static int capturing;
        /// <summary>Artifact waiting for a key press (binding row clicked), None otherwise.</summary>
        public static GameAnimaWeapon Capturing { get => (GameAnimaWeapon)capturing; private set => capturing = (int)value; }
        /// <summary>Frames seen during a battle (autotest: the hotkeys are read in battles too).</summary>
        public static int MonoTicksInBattle { get; private set; }
        /// <summary>Last artifact used by a hotkey and where ("map"/"battle"), for the log and the autotest.</summary>
        public static string LastUse { get; private set; }

        // keys that can be bound (mouse buttons 0-2 are the game's attack/skill/click)
        private static int[] capturable;
        private static IEnumerable<KeyCode> CAPTURABLE => (capturable ?? (capturable = ((KeyCode[])System.Enum.GetValues(typeof(KeyCode)))
            .Where(k => k != KeyCode.None && k != KeyCode.Mouse0 && k != KeyCode.Mouse1 && k != KeyCode.Mouse2 && (int)k < (int)KeyCode.JoystickButton0)
            .Select(k => (int)k).Distinct().ToArray())).Select(k => (KeyCode)k);

        // original positions of the buttons (all at the same place), per UI instance
        private static readonly Dictionary<int, Vector2> basePositions = new Dictionary<int, Vector2>();
        private static bool loggedRow;

        public KeyCode GetKey(GameAnimaWeapon w)
        {
            if (w == GameAnimaWeapon.PiscesPendant) return PiscesPendantKey;
            if (w == GameAnimaWeapon.HootinEye) return GodEyeKey;
            if (w == GameAnimaWeapon.DevilDemon) return DevilDemonKey;
            return KeyCode.None;
        }

        public void SetKey(GameAnimaWeapon w, KeyCode key)
        {
            if (w == GameAnimaWeapon.PiscesPendant) PiscesPendantKey = key;
            else if (w == GameAnimaWeapon.HootinEye) GodEyeKey = key;
            else if (w == GameAnimaWeapon.DevilDemon) DevilDemonKey = key;
        }

        public static string WeaponName(GameAnimaWeapon w)
        {
            if (w == GameAnimaWeapon.PiscesPendant) return GameTool.LS("newgame001");
            if (w == GameAnimaWeapon.HootinEye) return GameTool.LS("newgame002");
            return GameTool.LS("newgame003");
        }

        public static string KeyName(KeyCode key)
        {
            if (key == KeyCode.None) return "-";
            var s = key.ToString();
            if (s.StartsWith("Alpha")) return s.Substring(5);
            if (s.StartsWith("Keypad")) return "Num" + s.Substring(6);
            if (s == "Mouse3") return "MB4";
            if (s == "Mouse4") return "MB5";
            return s;
        }

        /// <summary>Game keys (map and battle) already bound to the key, empty when free.</summary>
        public static string GameKeyUsing(KeyCode key)
        {
            var k = g.data?.globle?.key;
            if (k == null || key == KeyCode.None) return null;
            var used = new Dictionary<string, KeyCode>
            {
                ["battleMoveUp"] = k.battleMoveUp, ["battleMoveDown"] = k.battleMoveDown, ["battleMoveLeft"] = k.battleMoveLeft, ["battleMoveRight"] = k.battleMoveRight,
                ["battleSkill1"] = k.battleSkill1, ["battleSkill2"] = k.battleSkill2, ["battleStep"] = k.battleStep, ["battleUltimate"] = k.battleUltimate,
                ["battleProp1"] = k.battleProp1, ["battleProp2"] = k.battleProp2, ["battleProp3"] = k.battleProp3, ["battleProp4"] = k.battleProp4, ["battleProp5"] = k.battleProp5,
                ["battleDialogue"] = k.battleDialogue, ["battleRule"] = k.battleRule, ["battleGodEye"] = k.battleGodEye, ["battleImmortal"] = k.battleImmortal,
                ["mapMoveUp"] = k.mapMoveUp, ["mapMoveDown"] = k.mapMoveDown, ["mapMoveLeft"] = k.mapMoveLeft, ["mapMoveRight"] = k.mapMoveRight,
                ["mapPlayerInfoUI"] = k.mapPlayerInfoUI, ["mapPlayerSkillUI"] = k.mapPlayerSkillUI, ["mapPlayerPackUI"] = k.mapPlayerPackUI, ["mapPlayerTaskUI"] = k.mapPlayerTaskUI,
                ["mapPlayerEmailUI"] = k.mapPlayerEmailUI, ["mapPlayerMonthTipUI"] = k.mapPlayerMonthTipUI, ["mapMinMapUI"] = k.mapMinMapUI, ["mapRefineElixirUI"] = k.mapRefineElixirUI,
                ["mapNextMonthUI"] = k.mapNextMonthUI, ["mapGradeUI"] = k.mapGradeUI, ["mapGodEyeUI"] = k.mapGodEyeUI, ["mapImmortalSkillUI"] = k.mapImmortalSkillUI,
            };
            return used.FirstOrDefault(x => x.Value == key).Key;
        }

        // ------------------------------------------------------------------ events

        /// <summary>Map / battle UI: lays the artifact buttons out (after the game set them up).</summary>
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.MapMain.uiName || e.uiType.uiName == UIType.BattleInfo.uiName)
                ModDelayEvent.Instance.DelayEvent(this, Arrange, 200);
        }

        /// <summary>Settings window closed while waiting for a key: cancelled.</summary>
        public override void OnCloseUIEnd(CloseUIEnd e)
        {
            base.OnCloseUIEnd(e);
            if (e.uiType.uiName == UIType.GameSetting.uiName || e.uiType.uiName == UIType.TextInfoLong.uiName)
                Capturing = GameAnimaWeapon.None;
        }

        /// <summary>The game shows/hides the buttons by itself (artifact changed, battle started...): laid out again.</summary>
        [ErrorIgnore]
        public override void OnTimeUpdate1000ms()
        {
            base.OnTimeUpdate1000ms();
            Arrange();
        }

        /// <summary>Key capture for a binding row, then the hotkeys (map and battle).</summary>
        [ErrorIgnore]
        public override void OnMonoUpdate()
        {
            base.OnMonoUpdate();
            if (Capturing != GameAnimaWeapon.None)
            {
                foreach (var k in CAPTURABLE)
                {
                    if (Input.GetKeyDown(k))
                    {
                        ApplyCapture(k);
                        break;
                    }
                }
                return;
            }
            if (!GameHelper.IsInGame())
                return;
            var inBattle = GameHelper.IsInBattlle();
            if (inBattle) MonoTicksInBattle++;
            if (PiscesPendantKey == KeyCode.None && GodEyeKey == KeyCode.None && DevilDemonKey == KeyCode.None)
                return;
            if (IsTyping())
                return;
            foreach (var w in WEAPONS)
            {
                var key = GetKey(w);
                if (key != KeyCode.None && Input.GetKeyDown(key))
                    Trigger(w);
            }
        }

        // ------------------------------------------------------------------ hotkeys

        private static bool IsTyping()
        {
            var go = UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject;
            return go != null && go.GetComponent<InputField>() != null;
        }

        /// <summary>
        /// Uses an artifact like its hotkey: battle -> its skill (the game checks cooldown/charges), map -> its window
        /// (a click on its button). False when it cannot be used now.
        /// </summary>
        public bool Trigger(GameAnimaWeapon w)
        {
            if (!GameHelper.IsInGame() || !GodArtifactHelper.HasAnimaWeapon(w))
                return false;
            if (GameHelper.IsInBattlle())
            {
                if (ModBattleEvent.SceneBattle == null || ModBattleEvent.SceneBattle.battleMap == null || !ModBattleEvent.SceneBattle.battleMap.isStartBattle)
                    return false;
                var player = ModBattleEvent.PlayerUnit;
                if (player == null || player.isDie || player.inputCtrl == null)
                    return false;
                var go = BattleButton(w);
                if (go == null || !go.activeInHierarchy) // no battle skill (yet) for this artifact
                    return false;
                if (w == GameAnimaWeapon.PiscesPendant) player.inputCtrl.UsePiscesPendant();
                else if (w == GameAnimaWeapon.HootinEye) player.inputCtrl.UseGodEye();
                else player.inputCtrl.UseDevilDemon();
                LastUse = $"battle:{w}";
            }
            else
            {
                if (GameHelper.IsLoadingScreen() || GameHelper.IsMaskScreen())
                    return false;
                var btn = MapButton(w);
                if (btn == null || !btn.gameObject.activeInHierarchy || !btn.interactable)
                    return false;
                btn.onClick.Invoke();
                LastUse = $"map:{w}";
            }
            DebugHelper.WriteLine($"AnimaWeaponEvent: hotkey {LastUse}");
            return true;
        }

        public static GameObject BattleButton(GameAnimaWeapon w)
        {
            var info = g.ui.GetUI<UIBattleInfo>(UIType.BattleInfo)?.uiInfo;
            if (info == null) return null;
            if (w == GameAnimaWeapon.PiscesPendant) return info.goPiscesPendant;
            if (w == GameAnimaWeapon.HootinEye) return info.goGodEye;
            return info.goDevilDemon;
        }

        private static Button MapButton(GameAnimaWeapon w)
        {
            var info = g.ui.GetUI<UIMapMain>(UIType.MapMain)?.uiPlayerInfo;
            if (info == null) return null;
            if (w == GameAnimaWeapon.PiscesPendant) return info.btnPiscesPendant;
            if (w == GameAnimaWeapon.HootinEye) return info.btnGodEye;
            return info.btnDevilDemon;
        }

        // ------------------------------------------------------------------ binding rows

        /// <summary>Starts waiting for the key of an artifact (its binding row was clicked).</summary>
        public void StartCapture(GameAnimaWeapon w)
        {
            Capturing = w;
            RefreshRows();
        }

        /// <summary>
        /// Key pressed while waiting: Esc cancels, Backspace/Delete clears, a key used by the game is refused (tip),
        /// a key of another artifact is moved to this one. Saved at once.
        /// </summary>
        public void ApplyCapture(KeyCode key)
        {
            var w = Capturing;
            Capturing = GameAnimaWeapon.None;
            if (w == GameAnimaWeapon.None || key == KeyCode.Escape)
            {
                RefreshUI();
                return;
            }
            if (key == KeyCode.Backspace || key == KeyCode.Delete)
                key = KeyCode.None;
            var usedBy = GameKeyUsing(key);
            if (usedBy != null)
            {
                g.ui.MsgBox(GameTool.LS("setting005"), string.Format(GameTool.LS("setting008"), KeyName(key)));
                RefreshUI();
                return;
            }
            foreach (var other in WEAPONS)
                if (other != w && key != KeyCode.None && GetKey(other) == key)
                    SetKey(other, KeyCode.None);
            SetKey(w, key);
            CacheHelper.SaveGlobalCache(this);
            DebugHelper.WriteLine($"AnimaWeaponEvent: {w} -> {KeyName(key)}");
            RefreshUI();
        }

        private void RefreshUI()
        {
            RefreshRows();
            if (GameHelper.IsInGame()) Arrange();
        }

        // binding rows shown now (Taoist Settings window): row -> artifact
        private static readonly List<Tuple<GameObject, int>> rows = new List<Tuple<GameObject, int>>();

        /// <summary>The binding rows of the open Taoist Settings window.</summary>
        public static List<GameObject> GetRows()
        {
            rows.RemoveAll(x => !IsAlive(x.Item1));
            return rows.Select(x => x.Item1).ToList();
        }

        private static bool IsAlive(GameObject go)
        {
            try { return go != null && go.activeInHierarchy; }
            catch { return false; }
        }

        /// <summary>Updates the key texts of the rows ("..." while waiting for a key).</summary>
        public void RefreshRows()
        {
            foreach (var r in rows.ToArray())
            {
                if (!IsAlive(r.Item1)) { rows.Remove(r); continue; }
                var w = (GameAnimaWeapon)r.Item2;
                var texts = RowTexts(r.Item1);
                if (texts.Item1 != null && texts.Item1.text != WeaponName(w)) texts.Item1.text = WeaponName(w);
                var keyText = Capturing == w ? "..." : KeyName(GetKey(w));
                if (texts.Item2 != null && texts.Item2.text != keyText) texts.Item2.text = keyText;
                foreach (var t in r.Item1.GetComponentsInChildren<Toggle>(true))
                    if (t.isOn != (Capturing == w)) t.isOn = Capturing == w;
            }
        }

        /// <summary>
        /// Adds a binding row per artifact to a Taoist window, from row "row" down: a copy of a key binding row of the
        /// game settings (same look and use: click, then press a key). Without the game settings open: plain buttons.
        /// Returns the next free row.
        /// </summary>
        public int AddBindingRows(ModLib.Object.UICustomBase ui, int row)
        {
            rows.Clear();
            var template = BindingTemplate();
            foreach (var w in WEAPONS)
            {
                var weapon = w;
                if (template != null)
                {
                    var go = UnityEngine.Object.Instantiate(template, ui.UIBase.transform);
                    go.name = ROW_PREFIX + weapon;
                    var rt = go.GetComponent<RectTransform>();
                    rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(ui.Columns[ui.MidCol], ui.Rows[row]);
                    SetupRow(go, weapon);
                    rows.Add(Tuple.Create(go, (int)weapon));
                }
                else
                {
                    ui.AddButton(ui.MidCol, row, () => StartCapture(weapon), "{0}").Size(300, 32).SetWork(new ModLib.Object.UIItemWork
                    {
                        Formatter = ActionHelper.WTracedFunc<ModLib.Object.UIItemBase, object[]>(x => new object[] { $"{WeaponName(weapon)}:  {(Capturing == weapon ? "..." : KeyName(GetKey(weapon)))}" })
                    });
                    ui.IsAutoUpdate = true;
                }
                row += 2;
            }
            RefreshRows();
            return row;
        }

        // a key row of the game settings (combat list) with its name and key texts
        private static GameObject BindingTemplate()
        {
            var root = g.ui.GetUI<UIGameSetting>(UIType.GameSetting)?.goKeyBattleRoot;
            if (root == null)
                return null;
            for (var i = 0; i < root.transform.childCount; i++)
            {
                var child = root.transform.GetChild(i).gameObject;
                if (child.activeSelf && child.GetComponentsInChildren<Text>(true).Length >= 2 && child.GetComponentsInChildren<Selectable>(true).Any(x => x.interactable))
                    return child;
            }
            return null;
        }
        /// <summary>Name text (left) and key text (right) of a row.</summary>
        private static Tuple<Text, Text> RowTexts(GameObject row)
        {
            var texts = row.GetComponentsInChildren<Text>(true).OrderBy(x => x.transform.position.x).ToArray();
            return Tuple.Create(texts.FirstOrDefault(), texts.Length > 1 ? texts.Last() : null);
        }

        private void SetupRow(GameObject row, GameAnimaWeapon w)
        {
            if (!loggedRow)
            {
                loggedRow = true;
                DebugHelper.WriteLine($"AnimaWeaponEvent: binding row from '{row.transform.parent.name}': " +
                    string.Join(", ", row.GetComponentsInChildren<Component>(true).Select(x => $"{x.gameObject.name}:{x.GetIl2CppType().Name}")));
            }
            // the game rows are toggles wired in code: the copy keeps no listener. Turned on (clicked) = waiting for a
            // key, with the game's own "press a new key" look; RefreshRows turns it off again
            var toggles = row.GetComponentsInChildren<Toggle>(true).ToArray();
            foreach (var t in toggles)
            {
                t.onValueChanged.RemoveAllListeners();
                t.isOn = false;
                t.interactable = true;
                t.onValueChanged.AddListener(ActionHelper.TracedUnityAction<bool>(v => { if (v && Capturing != w) StartCapture(w); }));
            }
            if (toggles.Length == 0)
            {
                Button[] buttons = row.GetComponentsInChildren<Button>(true).ToArray();
                if (buttons.Length == 0)
                    buttons = new[] { row.AddComponent<Button>() };
                foreach (var b in buttons)
                {
                    b.interactable = true;
                    b.onClick.RemoveAllListeners();
                    b.onClick.AddListener(ActionHelper.TracedUnityAction(() => StartCapture(w)));
                }
            }
            foreach (var g2 in row.GetComponentsInChildren<Graphic>(true))
                g2.color = new Color(g2.color.r, g2.color.g, g2.color.b, Math.Max(g2.color.a, 0.9f));
        }

        // ------------------------------------------------------------------ layout

        /// <summary>Lays the active artifact buttons of the map and battle UIs out side by side, with their hotkeys.</summary>
        public void Arrange()
        {
            if (!GameHelper.IsInGame())
                return;
            var map = g.ui.GetUI<UIMapMain>(UIType.MapMain);
            if (map != null && map.uiPlayerInfo != null)
            {
                var info = map.uiPlayerInfo;
                Layout(info.btnPiscesPendant?.gameObject, info.btnGodEye?.gameObject, info.btnDevilDemon?.gameObject);
                SetKeyText(info.textPiscesKey, PiscesPendantKey);
                SetKeyText(info.textGodEyeKey, GodEyeKey);
                SetKeyText(info.textDevilDemonKey, DevilDemonKey);
            }
            var battle = g.ui.GetUI<UIBattleInfo>(UIType.BattleInfo);
            if (battle != null && battle.uiInfo != null)
            {
                var info = battle.uiInfo;
                Layout(info.goPiscesPendant, info.goGodEye, info.goDevilDemon);
                SetKeyText(info.textKeyPiscesSkill, PiscesPendantKey);
                SetKeyText(info.textKeyGodEyeSkill, GodEyeKey);
                SetKeyText(info.textKeyDevilDemonSkill, DevilDemonKey);
            }
        }

        // the button shows its own hotkey (none set: the game's key label is kept)
        private static void SetKeyText(Text text, KeyCode key)
        {
            if (text == null || key == KeyCode.None)
                return;
            var s = KeyName(key);
            if (text.text != s)
                text.text = s;
        }

        /// <summary>
        /// Active buttons from the original place to the right, one button width apart (one active button: not moved).
        /// </summary>
        private static void Layout(params GameObject[] items)
        {
            var actives = new List<RectTransform>();
            foreach (var go in items)
            {
                if (go == null)
                    continue;
                var rt = go.GetComponent<RectTransform>();
                if (rt == null)
                    continue;
                var id = go.GetInstanceID();
                if (!basePositions.ContainsKey(id))
                    basePositions[id] = rt.anchoredPosition;
                if (go.activeSelf)
                    actives.Add(rt);
                else
                    rt.anchoredPosition = basePositions[id];
            }
            var step = 0f;
            foreach (var rt in actives)
                step = Mathf.Max(step, rt.rect.width);
            if (step <= 1f) step = 60f;
            step *= 1.05f;
            for (var i = 0; i < actives.Count; i++)
            {
                var pos = basePositions[actives[i].gameObject.GetInstanceID()] + new Vector2(step * i, 0f);
                if (actives[i].anchoredPosition != pos)
                    actives[i].anchoredPosition = pos;
            }
        }
    }
}
