using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System;
using System.IO;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Tool
{
    /// <summary>
    /// TI-01 (investigation, a tool: run only with -Scenario TI-01): can a run start again from a saved state without
    /// closing the game? The save files are put back while the game runs, then the save is opened again:
    /// A. "Reload save" of the game menu (UIGameMemu.ReloadCache);
    /// B. "Return to main menu" (UIGameMemu.btnReturnMain), then the save opened from the save list.
    /// For each: the player's money (the game's save) and S&amp;M AddLevelupExpRate (a ModLib local cache, the mod's save)
    /// must be the snapshot's again, and the save files must not be written on the way (would undo the restore).
    /// </summary>
    public class TI01_SaveReloadProbe : IScenarioDefinition
    {
        public string Id => "TI-01";

        private const int MONEY_DELTA = 12345;
        private const double RATE_DELTA = 0.123;

        private static int _money0;
        private static double _rate0;
        private static string _folder, _playerId, _snap;
        private static DateTime _mark;

        private static string GameSaveDir => Path.Combine(SaveCleanupHelper.GameDataPath, "Steam", "CacheData", _folder);

        private static double Rate
        {
            get => SMLocalConfigsEvent.Instance?.Configs?.AddLevelupExpRate ?? double.NaN;
            set { if (SMLocalConfigsEvent.Instance?.Configs != null) SMLocalConfigsEvent.Instance.Configs.AddLevelupExpRate = (float)value; }
        }

        private static bool InGame => GameHelper.IsInGame() && g.ui.HasUI(UIType.MapMain) && !GameHelper.IsLoadingScreen();

        /// <summary>The newest write time of the files of the character's saves (game + mods).</summary>
        private static DateTime LastWrite()
        {
            var files = Directory.GetFiles(GameSaveDir, "*", SearchOption.AllDirectories).ToList();
            foreach (var mod in Directory.GetDirectories(Path.Combine(SaveCleanupHelper.GameDataPath, "mod")))
            {
                var d = Path.Combine(mod, "saves", _playerId);
                if (Directory.Exists(d)) files.AddRange(Directory.GetFiles(d, "*", SearchOption.AllDirectories));
            }
            return files.Count == 0 ? DateTime.MinValue : files.Max(File.GetLastWriteTime);
        }

        private static void CopyDir(string src, string dst)
        {
            if (Directory.Exists(dst)) Directory.Delete(dst, true);
            Directory.CreateDirectory(dst);
            foreach (var f in Directory.GetFiles(src, "*", SearchOption.AllDirectories))
            {
                var to = Path.Combine(dst, f.Substring(src.Length).TrimStart('\\', '/'));
                Directory.CreateDirectory(Path.GetDirectoryName(to));
                File.Copy(f, to, true);
            }
        }

        private static void Snapshot(bool restore)
        {
            var pairs = new[] { Tuple.Create(GameSaveDir, Path.Combine(_snap, "game")) }.ToList();
            foreach (var mod in Directory.GetDirectories(Path.Combine(SaveCleanupHelper.GameDataPath, "mod")))
            {
                var d = Path.Combine(mod, "saves", _playerId);
                if (Directory.Exists(d) || restore) pairs.Add(Tuple.Create(d, Path.Combine(_snap, "mod", Path.GetFileName(mod))));
            }
            foreach (var p in pairs)
            {
                if (restore) { if (Directory.Exists(p.Item2)) CopyDir(p.Item2, p.Item1); }
                else CopyDir(p.Item1, p.Item2);
            }
        }

        private static void Change(AutoTestContext c, string label)
        {
            Player.AddUnitMoney(MONEY_DELTA);
            Rate = _rate0 + RATE_DELTA;
            GameHelper.SaveGame();
            c.Info($"{label}: money {Player.GetUnitMoney():#,##0}, rate {Rate}, game saved");
        }

        private static void Check(AutoTestContext c, string label)
        {
            c.Info($"{label}: money {Player.GetUnitMoney():#,##0} (snapshot {_money0:#,##0}), rate {Rate} (snapshot {_rate0}), save files written since the restore: {LastWrite() > _mark}");
            c.AssertEqual(_money0, Player.GetUnitMoney(), $"{label}: money of the snapshot (game save read again)");
            c.Assert(Math.Abs(Rate - _rate0) < 1e-4, $"{label}: S&M rate of the snapshot (mod save read again)");
        }

        public Scenario Create()
        {
            var startA = DateTime.MinValue;
            var startB = DateTime.MinValue;
            return new Scenario { Id = Id, Feature = "Autotest", Title = "Investigation: start again from a save without closing the game", Kind = ScenarioKind.Tool, Overlay = false }
                .Step("baseline", c =>
                {
                    _folder = g.cache.curCache?.data?.folder;
                    _playerId = Player.GetUnitId();
                    _snap = Path.Combine(AutoTestEvent.OutDir ?? Path.GetTempPath(), "reload-probe");
                    _money0 = Player.GetUnitMoney();
                    _rate0 = Rate;
                    GameHelper.SaveGame();
                    c.Info($"save {_folder}, player {_playerId}: money {_money0:#,##0}, rate {_rate0}");
                }, 6000)
                .Step("snapshot", c =>
                {
                    Snapshot(false);
                    c.Info($"snapshot: {Directory.GetFiles(_snap, "*", SearchOption.AllDirectories).Length} file(s)");
                })
                // ---- A: "Reload save" of the game menu
                .Step("A change", c => Change(c, "A"), 6000)
                .Step("A restore files", c =>
                {
                    Snapshot(true);
                    _mark = DateTime.Now;
                    c.Info("save files put back (game running)");
                }, 1000)
                .Step("A reload save", c =>
                {
                    var menu = g.ui.OpenUISafe<UIGameMemu>(UIType.GameMemu);
                    if (!c.Assert(menu != null, "game menu open")) return;
                    startA = DateTime.Now;
                    menu.ReloadCache(g.cache.curCache);
                }, 3000)
                .Wait("A in game again", c => { DramaHelper.DismissDramas(); return InGame; }, 180000, 1000)
                .Step("A check", c =>
                {
                    c.Info($"A: reload took {(DateTime.Now - startA).TotalSeconds:0.0} s");
                    Check(c, "A");
                }, 2000)
                // ---- B: return to the main menu, then the save opened again
                .Step("B change", c => Change(c, "B"), 6000)
                .Step("B return to main menu", c =>
                {
                    var menu = g.ui.OpenUISafe<UIGameMemu>(UIType.GameMemu);
                    if (!c.Assert(menu != null, "game menu open")) return;
                    startB = DateTime.Now;
                    menu.btnReturnMain.onClick.Invoke();
                }, 2000)
                .Wait("B title screen", c => { ClickButton(CONFIRM); return GameHelper.IsLoginScreen() && g.ui.HasUI(UIType.Login); }, 180000, 1000)
                .Step("B restore files", c =>
                {
                    c.Info($"B: title after {(DateTime.Now - startB).TotalSeconds:0.0} s; save files written on the way: {LastWrite() > DateTime.Now.AddSeconds(-30)}");
                    Snapshot(true);
                    _mark = DateTime.Now;
                }, 1000)
                .Step("B open the save list", c => g.ui.GetUI<UILogin>(UIType.Login)?.btnInto.onClick.Invoke(), 2500)
                .Step("B load the save", c =>
                {
                    var item = g.ui.GetUI<UILogin>(UIType.Login)?.allCacheItem?.ToArray().FirstOrDefault(x => x?.cacheData?.data?.folder == _folder);
                    if (!c.Assert(item != null, $"save {_folder} in the list")) return;
                    item.OnItemClick();
                }, 3000)
                .Wait("B in game again", c => { DramaHelper.DismissDramas(); return InGame; }, 180000, 1000)
                .Step("B check", c =>
                {
                    c.Info($"B: return + load took {(DateTime.Now - startB).TotalSeconds:0.0} s");
                    Check(c, "B");
                }, 2000)
                .Step("clean", c => { Snapshot(true); c.Info("save files of the snapshot put back"); });
        }
    }
}
