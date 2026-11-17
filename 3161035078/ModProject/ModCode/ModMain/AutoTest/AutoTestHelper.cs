using ModLib.Helper;
using ModLib.Mod;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MOD_nE7UL2.AutoTest
{
    /// <summary>
    /// Shared shortcuts for scenario files (imported with "using static MOD_nE7UL2.AutoTest.AutoTestHelper").
    /// </summary>
    public static class AutoTestHelper
    {
        /// <summary>Popups seen on the title screen before the scenarios (closed by the runner).</summary>
        public static List<string> TitlePopups { get; } = new List<string>();

        /// <summary>
        /// Clicks the first active, interactable button whose label is one of the given labels (case-insensitive),
        /// outside the title/map screens. Returns the clicked label or null.
        /// </summary>
        public static string ClickButton(params string[] labels)
        {
            if (g.ui.uiRoot == null) return null;
            foreach (var btn in g.ui.uiRoot.GetComponentsInChildren<UnityEngine.UI.Button>())
            {
                if (btn == null || !btn.gameObject.activeInHierarchy || !btn.interactable) continue;
                var label = btn.GetComponentInChildren<UnityEngine.UI.Text>()?.text?.Trim();
                if (string.IsNullOrEmpty(label) || !labels.Any(l => string.Equals(l, label, StringComparison.OrdinalIgnoreCase))) continue;
                var owner = btn.GetComponentInParent<UIBase>();
                var name = owner?.uiType?.uiName;
                if (name == UIType.Login.uiName || name == UIType.MapMain.uiName) continue;
                btn.onClick.Invoke();
                return label;
            }
            return null;
        }

        /// <summary>Active texts of every open UI.</summary>
        public static List<string> ScreenTexts()
        {
            if (g.ui.uiRoot == null) return new List<string>();
            return g.ui.uiRoot.GetComponentsInChildren<UnityEngine.UI.Text>().Where(t => t != null && t.gameObject.activeInHierarchy && !string.IsNullOrEmpty(t.text))
                .Select(t => t.text.Trim()).ToList();
        }

        private static string Clean(string s) => (s ?? string.Empty).Trim().TrimStart('•', '·', ' ').Trim();

        /// <summary>
        /// Clicks the first active button one of whose texts is the label (a drama option may start with a bullet).
        /// Returns the clicked text or null.
        /// </summary>
        public static string ClickOption(string label)
        {
            if (g.ui.uiRoot == null) return null;
            foreach (var btn in g.ui.uiRoot.GetComponentsInChildren<UnityEngine.UI.Button>())
            {
                if (btn == null || !btn.gameObject.activeInHierarchy || !btn.interactable) continue;
                var text = btn.GetComponentsInChildren<UnityEngine.UI.Text>().Select(t => Clean(t.text)).FirstOrDefault(t => t == Clean(label));
                if (text == null) continue;
                btn.onClick.Invoke();
                return text;
            }
            return null;
        }

        /// <summary>The open drama UI (any drama type: dialogue, fortuitous, NPCs around, heresy...), null when none.</summary>
        public static UIDramaBase DramaUI() =>
            g.ui.uiRoot?.GetComponentsInChildren<UIDramaBase>().FirstOrDefault(x => x != null && x.gameObject.activeInHierarchy);

        /// <summary>Text of the open drama (its longest TextMeshPro text), empty when none.</summary>
        public static string DramaText()
        {
            var ui = DramaUI();
            if (ui == null) return string.Empty;
            return ui.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Where(x => x.gameObject.activeInHierarchy && !string.IsNullOrEmpty(x.text))
                .Select(x => x.text).OrderByDescending(x => x.Length).FirstOrDefault() ?? string.Empty;
        }

        /// <summary>Options shown by the open drama: (option, label).</summary>
        public static List<Tuple<ConfDramaOptionsItem, string>> DramaOptions()
        {
            var ui = DramaUI();
            var rs = new List<Tuple<ConfDramaOptionsItem, string>>();
            if (ui == null) return rs;
            // options given in code (ModLib DramaHelper): ids and texts of DramaData.dialogueOptionsText / dialogueOptions
            var given = ui.dramaData?.dialogueOptionsText != null && ui.dramaData.dialogueOptionsText.Count > 0 ? ui.dramaData.dialogueOptionsText : ui.dramaData?.dialogueOptions;
            if ((ui.activeOptionsData == null || ui.activeOptionsData.Count == 0) && given != null)
            {
                foreach (var kv in given)
                {
                    var conf = g.conf.dramaOptions.GetItem(kv.Key);
                    if (conf != null) rs.Add(Tuple.Create(conf, kv.Value));
                }
                return rs;
            }
            if (ui.activeOptionsData == null) return rs;
            var texts = ui.dramaData?.dialogueOptionsText;
            var custom = ui.dramaData?.dialogueOptions;
            foreach (var o in ui.activeOptionsData.ToArray())
            {
                // the game keeps a list per layout (CN/EN): one entry per option id
                if (rs.Any(x => x.Item1.id == o.id)) continue;
                var label = texts != null && texts.ContainsKey(o.id) ? texts[o.id] : custom != null && custom.ContainsKey(o.id) ? custom[o.id] : GameTool.LS(o.text);
                rs.Add(Tuple.Create(o, label));
            }
            return rs;
        }

        /// <summary>Chooses the drama option with this label (as a click does). Returns false when not shown.</summary>
        public static bool ChooseOption(string label)
        {
            var ui = DramaUI();
            var o = DramaOptions().FirstOrDefault(x => x.Item2 == label);
            if (ui == null || o == null) return false;
            ui.ClickOption(o.Item1);
            return true;
        }

        /// <summary>Goes to the next line of the open drama (click on the text). False without a drama.</summary>
        public static bool DramaNext()
        {
            var d = g.ui.HasUI(UIType.DramaDialogue) ? g.ui.GetUI<UIDramaDialogue>(UIType.DramaDialogue) : null;
            if (d != null) { d.OnNextClick(); return true; }
            var f = g.ui.HasUI(UIType.DramaFortuitous) ? g.ui.GetUI<UIDramaFortuitous>(UIType.DramaFortuitous) : null;
            if (f != null) { f.OnNextClick(); return true; }
            var btn = DramaUI()?.GetComponentsInChildren<UnityEngine.UI.Button>().FirstOrDefault(b => b.name.Contains("btnNext"));
            if (btn != null) { btn.onClick.Invoke(); return true; }
            return false;
        }

        /// <summary>Labels of the buttons that confirm a dialog or leave a result screen.</summary>
        public static readonly string[] CONFIRM = { "OK", "Yes", "Confirm", "Continue", "Leave", "Exit", "Back", "Close", "确定", "確定", "是", "离开", "離開", "退出", "返回" };

        /// <summary>Result of the last watched battle (set by AutoTestEvent).</summary>
        public static bool LastBattleWin { get; set; }

        /// <summary>The player world unit.</summary>
        public static WorldUnitBase Player => g.world.playerUnit;

        /// <summary>Living NPCs of the current ParameterStore.</summary>
        public static IEnumerable<WorldUnitBase> Npcs =>
            (ModMain.Store?.WUnits ?? new WorldUnitBase[0]).Where(x => x != null && !x.isDie && !x.IsPlayer());

        /// <summary>Town at the player position, or the first town of the world.</summary>
        public static MapBuildTown AnyTown =>
            Player.GetMapBuild<MapBuildTown>() ?? ModMain.Store?.Towns?.FirstOrDefault();

        /// <summary>
        /// True when the event with this CacheId is loaded. Do not test "XxxEvent.Instance == null": ModLib sets
        /// Instance in OnLoadClass before OnCacheHandler can reject the object (BUG-16).
        /// </summary>
        public static bool IsLoaded(string cacheId) => EventHelper.GetEvents().Any(x => x.CacheId == cacheId);

        /// <summary>
        /// The loaded event with this CacheId, or a temporary instance (child of ModMain) when the feature is switched off
        /// in the S&amp;M configs, so its code can still be tested. Use RaiseOpenUIEnd for its UI hooks.
        /// </summary>
        public static T FeatureEvent<T>(string cacheId) where T : ModEvent, new()
        {
            return IsLoaded(cacheId) ? EventHelper.GetEvent<T>(cacheId) : new T { Parent = ModMain.ModObj };
        }

        /// <summary>
        /// Runs OnOpenUIEnd of a temporary feature event for an opened UI (a loaded event already received it from ModLib).
        /// </summary>
        public static void RaiseOpenUIEnd(ModEvent ev, string cacheId, UIBase ui, UIType.UITypeBase t)
        {
            if (ev != null && ui != null && !IsLoaded(cacheId))
                ev.OnOpenUIEnd(new EGameTypeData.OpenUIEnd(ui, t));
        }

        /// <summary>Custom UIs (UICover/UICustom) currently registered for a game UI type.</summary>
        public static int CoverCount(UIType.UITypeBase t) => UIHelper.GetUICustomBase(t).Count;

        /// <summary>Custom buttons of a game UI whose text contains the given text.</summary>
        public static List<UIItemButton> FindButtons(UIType.UITypeBase t, string text) =>
            UIHelper.GetUICustomBase(t).SelectMany(x => x.Items).OfType<UIItemButton>()
                .Where(x => (x.GetFormat() ?? string.Empty).Contains(text)).ToList();

        /// <summary>True when the numeric parts of cur are >= req.</summary>
        public static bool VersionGreaterOrEqual(string cur, string req)
        {
            var c = Regex.Split(cur ?? "0", @"\D").Select(x => x.Parse<int>()).ToArray();
            var r = Regex.Split(req ?? "0", @"\D").Select(x => x.Parse<int>()).ToArray();
            for (var i = 0; i < Math.Min(c.Length, r.Length); i++)
            {
                if (c[i] != r[i])
                    return c[i] > r[i];
            }
            return true;
        }
    }
}
