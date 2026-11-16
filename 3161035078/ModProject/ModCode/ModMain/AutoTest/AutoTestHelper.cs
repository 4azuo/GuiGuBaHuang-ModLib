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

        /// <summary>Labels of the buttons that confirm a dialog or leave a result screen.</summary>
        public static readonly string[] CONFIRM = { "OK", "Yes", "Confirm", "Continue", "Leave", "Exit", "Back", "Close", "确定", "確定", "是", "离开", "離開", "退出", "返回" };

        /// <summary>Result of the last watched battle (set by AutoTestEvent).</summary>
        public static bool LastBattleWin { get; set; }

        /// <summary>The player world unit.</summary>
        public static WorldUnitBase Player => g.world.playerUnit;

        /// <summary>Living NPCs of the current ParameterStore.</summary>
        public static IEnumerable<WorldUnitBase> Npcs =>
            (ModMaster.ModObj.ParameterStore?.WUnits ?? new WorldUnitBase[0]).Where(x => x != null && !x.isDie && !x.IsPlayer());

        /// <summary>Town at the player position, or the first town of the world.</summary>
        public static MapBuildTown AnyTown =>
            Player.GetMapBuild<MapBuildTown>() ?? ModMaster.ModObj.ParameterStore?.Towns?.FirstOrDefault();

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
