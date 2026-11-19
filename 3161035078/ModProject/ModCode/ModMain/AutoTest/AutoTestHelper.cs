using ModLib.Helper;
using ModLib.Mod;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MOD_nE7UL2.AutoTest
{
    /// <summary>
    /// Shared shortcuts for scenario files (imported with "using static MOD_nE7UL2.AutoTest.AutoTestHelper"). The
    /// generic ones are ModLib's (UIClickHelper, DramaHelper, EventHelper, AssemblyHelper): the names here are kept for
    /// the scenarios.
    /// </summary>
    public static class AutoTestHelper
    {
        /// <summary>Popups seen on the title screen before the scenarios (closed by the runner).</summary>
        public static List<string> TitlePopups { get; } = new List<string>();

        /// <summary>Clicks the first button with one of the labels, outside the title / map screens (UIClickHelper.ClickButton).</summary>
        public static string ClickButton(params string[] labels) => UIClickHelper.ClickButton(labels);

        /// <summary>Active texts of every open UI (UIClickHelper.ScreenTexts).</summary>
        public static List<string> ScreenTexts() => UIClickHelper.ScreenTexts();

        /// <summary>Clicks the first button one of whose texts is the label (UIClickHelper.ClickOption).</summary>
        public static string ClickOption(string label) => UIClickHelper.ClickOption(label);

        /// <summary>The open drama UI, null when none (DramaHelper.GetOpenDrama).</summary>
        public static UIDramaBase DramaUI() => DramaHelper.GetOpenDrama();

        /// <summary>Text of the open drama (DramaHelper.GetDramaText).</summary>
        public static string DramaText() => DramaHelper.GetDramaText();

        /// <summary>Options shown by the open drama: (option, label) (DramaHelper.GetDramaOptions).</summary>
        public static List<Tuple<ConfDramaOptionsItem, string>> DramaOptions() => DramaHelper.GetDramaOptions();

        /// <summary>Chooses the drama option with this label (DramaHelper.ChooseDramaOption).</summary>
        public static bool ChooseOption(string label) => DramaHelper.ChooseDramaOption(label);

        /// <summary>Goes to the next line of the open drama (DramaHelper.NextDramaLine).</summary>
        public static bool DramaNext() => DramaHelper.NextDramaLine();

        /// <summary>Labels of the buttons that confirm a dialog or leave a result screen (UIClickHelper.CONFIRM_LABELS).</summary>
        public static readonly string[] CONFIRM = UIClickHelper.CONFIRM_LABELS;

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

        /// <summary>True when the event with this CacheId is loaded (EventHelper.IsLoaded; not "Instance == null", BUG-16).</summary>
        public static bool IsLoaded(string cacheId) => EventHelper.IsLoaded(cacheId);

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

        /// <summary>True when the numeric parts of cur are >= req (AssemblyHelper.VersionGreaterOrEqual).</summary>
        public static bool VersionGreaterOrEqual(string cur, string req) => AssemblyHelper.VersionGreaterOrEqual(cur, req);
    }
}
