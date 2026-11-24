using ModLib.Attributes;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModLib.Helper
{
    /// <summary>
    /// Wallpapers of the mods on the title screen. A mod declares each wallpaper in its own file "*_wallpaper.json" of
    /// its ModConf folder (WallpaperConf: the image of ModImg and its settings, see docs/guide/08-Wallpapers.md; the game
    /// does not load these files: no config of the game has this name). They are added to the game's "Change Theme"
    /// window (UIWallPaper) and shown on the title screen (UILogin) over the game's wallpaper when chosen. The choice is
    /// saved in the global cache of ModTitleEvent (Wallpaper: "modId/image file"; null = the game's wallpaper). A
    /// wallpaper can also be shown at random when the game starts (StartupRate, this game session only, not saved).
    /// </summary>
    [ActionCat("Wallpaper")]
    public static class WallpaperHelper
    {
        /// <summary>End of the file name of a wallpaper file in ModConf ("taoist_1_wallpaper.json", "wallpaper.json").</summary>
        public const string CONF_SUFFIX = "wallpaper.json";
        /// <summary>Name of the image shown over the game's wallpaper on the title screen.</summary>
        public const string OVERLAY_NAME = "ModLib:Wallpaper";
        /// <summary>Name prefix of the items added to the Change Theme window.</summary>
        public const string ITEM_PREFIX = "ModLib:WallpaperItem:";

        private static readonly Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();
        private static GameObject _hiddenEffect;

        /// <summary>
        /// Wallpaper shown on the title screen for this game session only (not saved; "modId/file"): the startup draw
        /// (DrawStartupWallpaper), or set by a mod before the title screen opens. Cleared when the player chooses a
        /// wallpaper in the Change Theme window.
        /// </summary>
        public static string StartupWallpaper { get; set; }

        /// <summary>The startup draw was made (once a game session: the first title screen).</summary>
        public static bool StartupDrawn { get; set; }

        /// <summary>
        /// A file "*_wallpaper.json" of ModConf: one wallpaper (property names are not case-sensitive):
        /// { "image": "taoist_1_wallpaper.png", "name": null, "order": 0, "hideGameTitle": true, "startupRate": 0.25,
        ///   "startupWhenChosen": false }
        /// </summary>
        public class WallpaperConf
        {
            /// <summary>Image file in ModImg (png, jpg, jpeg).</summary>
            public string Image { get; set; }
            /// <summary>Name shown on the item when the item of the Change Theme window has a text (else "Mod wallpaper n").</summary>
            public string Name { get; set; }
            /// <summary>Place among the wallpapers of the mod in the Change Theme window (then the file name).</summary>
            public int Order { get; set; } = 0;
            /// <summary>The game's title (logo) hidden over it (the picture has its own), unless the player chose otherwise.</summary>
            public bool HideGameTitle { get; set; } = true;
            /// <summary>Chance (0..1) that the game starts on this wallpaper for the session (0: never).</summary>
            public double StartupRate { get; set; } = 0;
            /// <summary>Startup draw even when the player chose a wallpaper of the same mod (default: no draw then).</summary>
            public bool StartupWhenChosen { get; set; } = false;
        }

        /// <summary>A mod wallpaper: Key = "modId/image file".</summary>
        public class Wallpaper
        {
            public string ModId { get; set; }
            /// <summary>The image file of ModImg.</summary>
            public string FileName { get; set; }
            /// <summary>The wallpaper file of ModConf.</summary>
            public string ConfFile { get; set; }
            public WallpaperConf Conf { get; set; } = new WallpaperConf();
            public string Name => Conf.Name;
            public bool HideGameTitle => Conf.HideGameTitle;
            public double StartupRate => Math.Max(0, Conf.StartupRate);
            public string Key => MakeKey(ModId, FileName);
        }

        public static string MakeKey(string modId, string fileName) => $"{modId}/{fileName}";

        private static readonly string[] EXTENSIONS = { ".png", ".jpg", ".jpeg" };

        /// <summary>True for a file name "*wallpaper.json" (case ignored).</summary>
        public static bool IsWallpaperConfFile(string fileName)
        {
            return !string.IsNullOrEmpty(fileName) && fileName.EndsWith(CONF_SUFFIX, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Reads a wallpaper file (JSON text). Null (with a log line) when the JSON cannot be read or the image is missing
        /// (imageExists) or not png/jpg/jpeg.
        /// </summary>
        public static Wallpaper ReadWallpaper(string modId, string confFile, string json, Func<string, bool> imageExists)
        {
            WallpaperConf conf;
            try
            {
                conf = Newtonsoft.Json.JsonConvert.DeserializeObject<WallpaperConf>(json);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"Wallpaper {modId}/{confFile}: {ex.Message}");
                return null;
            }
            if (conf == null || string.IsNullOrEmpty(conf.Image) || !EXTENSIONS.Contains(Path.GetExtension(conf.Image).ToLowerInvariant()) || !imageExists(conf.Image))
            {
                DebugHelper.WriteLine($"Wallpaper {modId}/{confFile}: image '{conf?.Image}' skipped (missing in ModImg or not png/jpg/jpeg)");
                return null;
            }
            return new Wallpaper { ModId = modId, FileName = conf.Image, ConfFile = confFile, Conf = conf };
        }

        private static List<Wallpaper> _wallpapers;
        private static string _wallpapersOf;

        /// <summary>
        /// The wallpapers of the loaded mods: load order of the mods, then Order, then the file name (read once, again
        /// when the loaded mods change; the same image once).
        /// </summary>
        public static List<Wallpaper> GetWallpapers()
        {
            var mods = new List<string>();
            foreach (var mod in g.mod.allModPaths)
            {
                if (g.mod.IsLoadMod(mod.t1))
                    mods.Add(mod.t1);
            }
            var sign = string.Join("|", mods);
            if (_wallpapers != null && _wallpapersOf == sign)
                return _wallpapers;
            var rs = new List<Wallpaper>();
            foreach (var modId in mods)
            {
                try
                {
                    var folder = ConfHelper.GetConfFolderPath(modId);
                    var imgFolder = SpriteHelper.GetImgFolderPath(modId);
                    if (!Directory.Exists(folder))
                        continue;
                    var ws = new List<Wallpaper>();
                    foreach (var file in Directory.GetFiles(folder).Where(x => IsWallpaperConfFile(Path.GetFileName(x))))
                    {
                        var w = ReadWallpaper(modId, Path.GetFileName(file), File.ReadAllText(file), x => File.Exists(Path.Combine(imgFolder, x)));
                        if (w != null && !ws.Any(x => string.Equals(x.Key, w.Key, StringComparison.OrdinalIgnoreCase)))
                            ws.Add(w);
                    }
                    rs.AddRange(ws.OrderBy(x => x.Conf.Order).ThenBy(x => x.ConfFile, StringComparer.OrdinalIgnoreCase));
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine($"Wallpapers of {modId}: {ex.Message}");
                }
            }
            _wallpapers = rs;
            _wallpapersOf = sign;
            DebugHelper.WriteLine($"Wallpapers: {string.Join(", ", rs.Select(x => $"{x.ModId}/{x.ConfFile} -> {x.FileName} (startup {x.StartupRate})"))}");
            return rs;
        }

        /// <summary>
        /// The startup draw, for the player's saved choice (key or null). One draw over all the wallpapers with a
        /// StartupRate: each has its own chance (a total over 1 is scaled down to 1), the rest is "no startup wallpaper".
        /// A wallpaper is left out when the player chose a wallpaper of the same mod, unless StartupWhenChosen.
        /// Null: no startup wallpaper.
        /// </summary>
        public static string DrawStartupWallpaper(IEnumerable<Wallpaper> wallpapers, string chosenKey, System.Random r)
        {
            var chosenMod = chosenKey?.Split('/')[0];
            var ws = wallpapers.Where(x => x.StartupRate > 0 && (x.Conf.StartupWhenChosen || !string.Equals(x.ModId, chosenMod, StringComparison.OrdinalIgnoreCase))).ToList();
            var total = ws.Sum(x => x.StartupRate);
            if (total <= 0)
                return null;
            var scale = total > 1 ? 1 / total : 1;
            var pick = r.NextDouble();
            foreach (var w in ws)
            {
                var rate = w.StartupRate * scale;
                if (pick < rate)
                    return w.Key;
                pick -= rate;
            }
            return null;
        }

        /// <summary>
        /// The first title screen of the game session: the startup draw over the wallpapers of the loaded mods
        /// (DrawStartupWallpaper) sets StartupWallpaper. Once a session.
        /// </summary>
        public static void ShowStartupWallpaper(System.Random r = null)
        {
            if (StartupDrawn)
                return;
            StartupDrawn = true;
            var key = DrawStartupWallpaper(GetWallpapers(), ModTitleEvent.Instance?.Wallpaper, r ?? new System.Random());
            if (key == null)
                return;
            StartupWallpaper = key;
            DebugHelper.WriteLine($"Startup wallpaper: {key}");
        }

        /// <summary>The wallpaper of a key, null when it is not there (mod removed or disabled).</summary>
        public static Wallpaper Find(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;
            return GetWallpapers().FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>The sprite of a wallpaper (loaded once), null when the file cannot be read.</summary>
        public static Sprite GetSprite(Wallpaper w)
        {
            if (w == null)
                return null;
            if (_sprites.TryGetValue(w.Key, out var s) && s != null)
                return s;
            s = SpriteHelper.GetImage(w.ModId, w.FileName);
            if (s != null)
            {
                s.texture.wrapMode = TextureWrapMode.Clamp;
                _sprites[w.Key] = s;
            }
            return s;
        }

        //the title of the game on the title screen (calligraphy "鬼谷八荒 Tale of Immortal"), hidden over a mod wallpaper
        private static readonly List<GameObject> _hiddenTitle = new List<GameObject>();
        private static readonly System.Text.RegularExpressions.Regex TITLE_NAME =
            new System.Text.RegularExpressions.Regex("(?i)logo|biaoti|gamename|^title$|titleimg|imgtitle");
        private static bool _titleDumped;

        /// <summary>
        /// The objects drawing the game's title on the title screen (logo): the objects of the Login UI named like a logo
        /// (TITLE_NAME), not the mod ones.
        /// </summary>
        public static List<GameObject> GetGameTitleObjects(UILogin ui)
        {
            var rs = new List<GameObject>();
            if (ui == null)
                return rs;
            foreach (var t in ui.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.StartsWith(CompHelper.CLONE_PREFIX) || t.name.StartsWith("ModLib:"))
                    continue;
                if (TITLE_NAME.IsMatch(t.name) && (t.GetComponent<Image>() != null || t.GetComponent<SpriteRenderer>() != null || t.GetComponent<RawImage>() != null || t.childCount > 0))
                {
                    //the topmost match only (its children go with it)
                    if (!rs.Any(x => t.IsChildOf(x.transform)))
                        rs.Add(t.gameObject);
                }
            }
            return rs;
        }

        /// <summary>
        /// Whether the game's title is hidden over a mod wallpaper: the player's choice (toggle of the Change Theme
        /// window, ModTitleEvent.HideGameTitle) or, when the player did not choose, the setting of the wallpaper's pack.
        /// </summary>
        public static bool ShouldHideGameTitle(Wallpaper w)
        {
            return ModTitleEvent.Instance?.HideGameTitle ?? w?.HideGameTitle ?? true;
        }

        /// <summary>
        /// The game's title on the title screen hidden while a mod wallpaper is shown and ShouldHideGameTitle, shown
        /// again otherwise.
        /// </summary>
        public static void UpdateGameTitle()
        {
            var ui = g.ui.GetUI<UILogin>(UIType.Login);
            if (ui == null)
                return;
            var hide = IsShownOnTitle() && ShouldHideGameTitle(Find(GetCurrentKey()));
            if (hide)
            {
                var objs = GetGameTitleObjects(ui);
                if (!_titleDumped)
                {
                    _titleDumped = true;
                    DebugHelper.WriteLine($"Game title: {string.Join(", ", objs.Select(x => x.name))}\n{BattleSceneHelper.DumpHierarchy(ui.transform, 3)}");
                }
                foreach (var o in objs.Where(x => x.activeSelf))
                {
                    o.SetActive(false);
                    _hiddenTitle.Add(o);
                }
            }
            else
            {
                foreach (var o in _hiddenTitle.Where(x => x != null))
                    o.SetActive(true);
                _hiddenTitle.Clear();
            }
        }

        /// <summary>True when the game's title is hidden on the title screen.</summary>
        public static bool IsGameTitleHidden() => _hiddenTitle.Any(x => x != null && !x.activeSelf);

        /// <summary>The wallpaper the title screen shows: the one of this session, else the player's choice.</summary>
        public static string GetCurrentKey()
        {
            return StartupWallpaper ?? ModTitleEvent.Instance?.Wallpaper;
        }

        /// <summary>
        /// The player's choice in the Change Theme window: a mod wallpaper (key) or null (a wallpaper of the game). Saved,
        /// shown at once on the title screen; the wallpaper of the session is dropped.
        /// </summary>
        public static void Choose(string key)
        {
            StartupWallpaper = null;
            if (ModTitleEvent.Instance != null)
            {
                ModTitleEvent.Instance.Wallpaper = key;
                CacheHelper.SaveGlobalCache(ModTitleEvent.Instance);
            }
            DebugHelper.WriteLine($"Wallpaper: {key ?? "game"}");
            ShowOnTitle(key);
        }

        /// <summary>
        /// Shows a mod wallpaper on the title screen over the game's one (null or unknown key: the game's wallpaper).
        /// False when the title screen is not open or the wallpaper cannot be shown.
        /// </summary>
        public static bool ShowOnTitle(string key)
        {
            var ui = g.ui.GetUI<UILogin>(UIType.Login);
            if (ui == null)
                return false;
            var canvas = ui.goBG?.transform?.parent;
            if (canvas == null)
                return false;
            var old = canvas.Find(OVERLAY_NAME);
            var sprite = GetSprite(Find(key));
            if (sprite == null)
            {
                if (old != null)
                    UnityEngine.Object.DestroyImmediate(old.gameObject);
                //the animated wallpaper and the title of the game back
                if (_hiddenEffect != null)
                {
                    _hiddenEffect.SetActive(true);
                    _hiddenEffect = null;
                }
                UpdateGameTitle();
                return false;
            }
            //the animated wallpaper of the game (sprites drawn over the canvas) hidden while a mod wallpaper is shown
            var effect = ui.goEffectBG;
            if (effect != null && effect.activeSelf)
            {
                DebugHelper.WriteLine($"Title wallpaper: {effect.name} hidden:\n{BattleSceneHelper.DumpHierarchy(effect.transform, 2)}");
                effect.SetActive(false);
                _hiddenEffect = effect;
            }

            GameObject go;
            Image img;
            if (old != null)
            {
                go = old.gameObject;
                img = go.GetComponent<Image>();
            }
            else
            {
                DebugHelper.WriteLine($"Title wallpaper over {ui.goBG.name} in:\n{BattleSceneHelper.DumpHierarchy(canvas, 1)}");
                go = new GameObject(OVERLAY_NAME);
                go.transform.SetParent(canvas, false);
                img = go.AddComponent<Image>();
                img.raycastTarget = false;
                var rect = go.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
            img.sprite = sprite;
            img.preserveAspect = false;
            img.color = Color.white;
            //right over the game's wallpaper, under the buttons
            go.transform.SetSiblingIndex(ui.goBG.transform.GetSiblingIndex() + 1);
            go.SetActive(true);
            UpdateGameTitle();
            return true;
        }

        /// <summary>True when a mod wallpaper is shown on the title screen (and the game's animated one hidden).</summary>
        public static bool IsShownOnTitle()
        {
            var ui = g.ui.GetUI<UILogin>(UIType.Login);
            var o = ui?.goBG?.transform?.parent?.Find(OVERLAY_NAME);
            return o != null && o.gameObject.activeSelf && (ui.goEffectBG == null || !ui.goEffectBG.activeSelf);
        }

        /// <summary>
        /// Adds the mod wallpapers to the open Change Theme window (UIWallPaper): copies of its items with the picture and
        /// name of each wallpaper. Choosing one shows it on the title screen; choosing a wallpaper of the game removes it.
        /// Returns the number of items added.
        /// </summary>
        public static int AddToThemeWindow(UIWallPaper ui)
        {
            if (ui == null || ui.goItemRoot == null)
                return 0;
            var root = ui.goItemRoot.transform;
            var gameItems = new List<GameObject>();
            for (var i = 0; i < root.childCount; i++)
            {
                var c = root.GetChild(i).gameObject;
                if (!c.name.StartsWith(ITEM_PREFIX) && c.activeSelf)
                    gameItems.Add(c);
            }
            var sample = gameItems.FirstOrDefault() ?? ui.goItem;
            if (sample == null)
                return 0;
            DebugHelper.WriteLine($"Change Theme item:\n{BattleSceneHelper.DumpHierarchy(sample.transform, 5)}");

            //a wallpaper of the game chosen: the mod wallpaper removed
            foreach (var item in gameItems)
            {
                var t = item.GetComponentInChildren<Toggle>(true);
                var gameItem = item;
                if (t != null)
                    t.onValueChanged.AddListener(ActionHelper.TracedUnityAction<bool>(on => { if (on) { Choose(null); SetCurrentMark(root, gameItem); } }));
                else
                {
                    var b = item.GetComponentInChildren<Button>(true);
                    b?.onClick.AddListener(ActionHelper.TracedUnityAction(() => { Choose(null); SetCurrentMark(root, gameItem); }));
                }
            }

            var current = GetCurrentKey();
            var count = 0;
            var n = 0;
            foreach (var w in GetWallpapers())
            {
                n++;
                var sprite = GetSprite(w);
                if (sprite == null)
                    continue;
                var go = UnityEngine.Object.Instantiate(sample, root, false);
                go.name = ITEM_PREFIX + w.Key;
                go.SetActive(true);
                SetItemPicture(go, sample, sprite);
                //the items of the game are pictures only: a name only when the item has a text
                foreach (var txt in go.GetComponentsInChildren<Text>(true))
                    txt.text = string.IsNullOrEmpty(w.Name) ? string.Format(GameTool.LS("libtxt999990032"), n) : w.Name;

                var key = w.Key;
                var toggle = go.GetComponentInChildren<Toggle>(true);
                if (toggle != null)
                {
                    toggle.onValueChanged = new Toggle.ToggleEvent();
                    toggle.SetIsOnWithoutNotify(false);
                    toggle.onValueChanged.AddListener(ActionHelper.TracedUnityAction<bool>(on => { if (on) { Choose(key); SetCurrentMark(root, go); } }));
                    //the wallpaper shown: selected (without saving it: it may be the one of the session), the game's off
                    if (string.Equals(current, key, StringComparison.OrdinalIgnoreCase) && IsShownOnTitle())
                    {
                        foreach (var gi in gameItems)
                            gi.GetComponentInChildren<Toggle>(true)?.SetIsOnWithoutNotify(false);
                        toggle.SetIsOnWithoutNotify(true);
                        SetCurrentMark(root, go);
                    }
                }
                else
                {
                    var btn = go.GetComponentInChildren<Button>(true);
                    if (btn != null)
                    {
                        btn.onClick = new Button.ButtonClickedEvent();
                        btn.onClick.AddListener(ActionHelper.TracedUnityAction(() => { Choose(key); SetCurrentMark(root, go); }));
                    }
                }
                count++;
            }
            return count;
        }

        /// <summary>Name of the "Current" mark of an item of the Change Theme window (Tg > Select, off on the others).</summary>
        public const string CURRENT_MARK = "Select";

        /// <summary>The "Current" mark of the Change Theme window on one item only (the game sets it from its own wallpaper).</summary>
        public static void SetCurrentMark(Transform root, GameObject current)
        {
            if (root == null)
                return;
            for (var i = 0; i < root.childCount; i++)
            {
                var item = root.GetChild(i).gameObject;
                foreach (var t in item.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == CURRENT_MARK)
                        t.gameObject.SetActive(item == current);
                }
            }
        }

        /// <summary>The item of the Change Theme window with the "Current" mark, null when none.</summary>
        public static GameObject GetCurrentMarkItem()
        {
            var root = g.ui.GetUI<UIWallPaper>(UIType.WallPaper)?.goItemRoot?.transform;
            for (var i = 0; root != null && i < root.childCount; i++)
            {
                var item = root.GetChild(i).gameObject;
                if (item.GetComponentsInChildren<Transform>(true).Any(t => t.name == CURRENT_MARK && t.gameObject.activeSelf))
                    return item;
            }
            return null;
        }

        /// <summary>The items of the mod wallpapers in the open Change Theme window.</summary>
        public static List<GameObject> GetThemeWindowItems()
        {
            var rs = new List<GameObject>();
            var ui = g.ui.GetUI<UIWallPaper>(UIType.WallPaper);
            var root = ui?.goItemRoot?.transform;
            if (root == null)
                return rs;
            for (var i = 0; i < root.childCount; i++)
            {
                var c = root.GetChild(i).gameObject;
                if (c.name.StartsWith(ITEM_PREFIX))
                    rs.Add(c);
            }
            return rs;
        }

        //the picture of an item: its image "Paper" (item: Tg {Toggle, frame} > Paper, Select (off), ...), else the biggest
        //image under the toggle
        private static void SetItemPicture(GameObject item, GameObject sample, Sprite sprite)
        {
            var images = item.GetComponentsInChildren<Image>(true);
            Image best = images.FirstOrDefault(x => x.name == "Paper");
            var bestArea = 0f;
            foreach (var img in best == null ? images : new Image[0])
            {
                var r = img.rectTransform.rect;
                var area = r.width * r.height;
                if (img.gameObject == item || img.GetComponent<Toggle>() != null || img.GetComponent<Button>() != null)
                    continue;
                if (area > bestArea)
                {
                    best = img;
                    bestArea = area;
                }
            }
            if (best == null)
                best = item.GetComponent<Image>();
            if (best != null)
            {
                best.sprite = sprite;
                best.preserveAspect = false;
            }
        }
    }
}
