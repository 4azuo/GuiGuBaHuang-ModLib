using ModLib.Attributes;
using ModLib.Const;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for creating and managing dialog drama events.
    /// Provides utilities for opening dialogs with text and clickable options.
    /// </summary>
    [ActionCat("Drama")]
    public static class DramaHelper
    {
        private const int DEFAULT_OPT_ID = 999790000;
        /// <summary>
        /// Dialogue with exactly n options (999890011-14 type 1, 999890021-24 type 2) for 1 to 4 options: the default
        /// dialogues list 10 options that the game does not hide reliably, and with one option left the game shows no
        /// button: a click closes the drama without calling onOptionsClickCall. 0 option and more than 4: the default
        /// dialogue (no option shown, a click closes it).
        /// </summary>
        private static bool IsExact(int n) => n >= 1 && n <= 4;

        private static int DialogueId(int defaultId, int n)
        {
            if (!IsExact(n))
                return defaultId;
            return (defaultId == ModLibConst.DEFAULT_TYPE1_DRAMA_ID ? 999890010 : 999890020) + n;
        }

        /// <summary>
        /// Speaker of the shared dialogue (its config value is read when the drama opens): 0 none, 1 left unit lit,
        /// 2 right unit lit (the other one is dimmed).
        /// </summary>
        private static void SetSpeaker(int dramaId, int speaker)
        {
            var item = g.conf.dramaDialogue.GetItem(dramaId);
            if (item != null && item.speaker != speaker)
                item.speaker = speaker;
        }

        private static int[] DEFAULT_OPTS = new int[] { 999790000, 999790001, 999790002, 999790003, 999790004, 999790005, 999790006, 999790007, 999790008, 999790009 };

        /// <summary>
        /// Opens a type-1 drama dialog with text and clickable options.
        /// </summary>
        /// <param name="text">The dialog text to display</param>
        /// <param name="optMsgs">List of option texts</param>
        /// <param name="optClickCall">Callback when an option is clicked</param>
        public static void OpenDrama1(string text, List<string> optMsgs, Action<ConfDramaOptionsItem> optClickCall)
        {
            var opts = optMsgs.Select((x, i) => new { Index = i, Value = x }).ToArray();
            var dramaId = DialogueId(ModLibConst.DEFAULT_TYPE1_DRAMA_ID, opts.Length);
            SetSpeaker(dramaId, 0);
            DramaTool.OpenDrama(dramaId, new DramaData
            {
                dialogueText = { [dramaId] = text },
                // exact dialogue: its own options, texts replaced (dialogueOptions would add them a second time)
                dialogueOptions = (IsExact(opts.Length) ? opts.Take(0) : opts).ToIl2CppDictionary(x => DEFAULT_OPT_ID + x.Index, x => x.Value),
                dialogueOptionsText = (IsExact(opts.Length) ? opts : opts.Take(0)).ToIl2CppDictionary(x => DEFAULT_OPT_ID + x.Index, x => x.Value),
                hideDialogueOptions = (IsExact(opts.Length) ? new int[0] : DEFAULT_OPTS.Except(opts.Select((x, i) => i))).ToIl2CppList(),
                onOptionsClickCall = optClickCall,
            });
        }

        /// <summary>
        /// Opens a type-1 drama dialog with text, options, and character portraits.
        /// </summary>
        /// <param name="text">The dialog text to display</param>
        /// <param name="optMsgs">List of option texts</param>
        /// <param name="optClickCall">Callback when an option is clicked</param>
        /// <param name="wunitLeft">World unit to display on left side</param>
        /// <param name="wunitRight">World unit to display on right side</param>
        /// <param name="speaker">Who speaks (lit portrait, the other is dimmed): 0 none (both lit), 1 left, 2 right</param>
        public static void OpenDrama1(string text, List<string> optMsgs, Action<ConfDramaOptionsItem> optClickCall,
            WorldUnitBase wunitLeft, WorldUnitBase wunitRight, int speaker = 0)
        {
            var opts = optMsgs.Select((x, i) => new { Index = i, Value = x }).ToArray();
            var dramaId = DialogueId(ModLibConst.DEFAULT_TYPE1_DRAMA_ID, opts.Length);
            SetSpeaker(dramaId, speaker);
            DramaTool.OpenDrama(dramaId, new DramaData
            {
                dialogueText = { [dramaId] = text },
                // exact dialogue: its own options, texts replaced (dialogueOptions would add them a second time)
                dialogueOptions = (IsExact(opts.Length) ? opts.Take(0) : opts).ToIl2CppDictionary(x => DEFAULT_OPT_ID + x.Index, x => x.Value),
                dialogueOptionsText = (IsExact(opts.Length) ? opts : opts.Take(0)).ToIl2CppDictionary(x => DEFAULT_OPT_ID + x.Index, x => x.Value),
                hideDialogueOptions = (IsExact(opts.Length) ? new int[0] : DEFAULT_OPTS.Except(opts.Select((x, i) => i))).ToIl2CppList(),
                onOptionsClickCall = optClickCall,
                unitLeft = wunitLeft,
                unitRight = wunitRight,
            });
        }

        /// <summary>
        /// Opens a type-2 drama dialog with text, options, and custom background.
        /// </summary>
        /// <param name="text">The dialog text to display</param>
        /// <param name="optMsgs">List of option texts</param>
        /// <param name="optClickCall">Callback when an option is clicked</param>
        /// <param name="background">Background image path or identifier</param>
        public static void OpenDrama2(string text, List<string> optMsgs, Action<ConfDramaOptionsItem> optClickCall, string background)
        {
            var opts = optMsgs.Select((x, i) => new { Index = i, Value = x }).ToArray();
            var dramaId = DialogueId(ModLibConst.DEFAULT_TYPE2_DRAMA_ID, opts.Length);
            DramaTool.OpenDrama(dramaId, new DramaData
            {
                dialogueText = { [dramaId] = text },
                // exact dialogue: its own options, texts replaced (dialogueOptions would add them a second time)
                dialogueOptions = (IsExact(opts.Length) ? opts.Take(0) : opts).ToIl2CppDictionary(x => DEFAULT_OPT_ID + x.Index, x => x.Value),
                dialogueOptionsText = (IsExact(opts.Length) ? opts : opts.Take(0)).ToIl2CppDictionary(x => DEFAULT_OPT_ID + x.Index, x => x.Value),
                hideDialogueOptions = (IsExact(opts.Length) ? new int[0] : DEFAULT_OPTS.Except(opts.Select((x, i) => i))).ToIl2CppList(),
                onOptionsClickCall = optClickCall,
                backgroud = { [dramaId] = background },
            });
        }

        /// <summary>
        /// Obsolete drama opening method. No longer used.
        /// </summary>
        [Obsolete]
        public static void OpenDrama3()
        {
        }

        /// <summary>
        /// Obsolete drama opening method. No longer used.
        /// </summary>
        [Obsolete]
        public static void OpenDrama4()
        {
        }

        /// <summary>
        /// Obsolete drama opening method. No longer used.
        /// </summary>
        [Obsolete]
        public static void OpenDrama5()
        {
        }

        /// <summary>
        /// Obsolete drama opening method. No longer used.
        /// </summary>
        [Obsolete]
        public static void OpenDrama6()
        {
        }

        /// <summary>
        /// Obsolete drama opening method. No longer used.
        /// </summary>
        [Obsolete]
        public static void OpenDrama7()
        {
        }

        /// <summary>
        /// Obsolete drama opening method. No longer used.
        /// </summary>
        [Obsolete]
        public static void OpenDrama8()
        {
        }

        #region Read and drive an open drama

        /// <summary>The open drama UI (any type: dialogue, fortuitous, NPCs around, heresy...), null when none.</summary>
        public static UIDramaBase GetOpenDrama() =>
            g.ui.uiRoot?.GetComponentsInChildren<UIDramaBase>().FirstOrDefault(x => x != null && x.gameObject.activeInHierarchy);

        /// <summary>
        /// Text of the open drama: its longest TextMeshPro text outside the option buttons (an option with an icon tag
        /// can be longer than the dialogue), empty when none.
        /// </summary>
        public static string GetDramaText()
        {
            var ui = GetOpenDrama();
            if (ui == null) return string.Empty;
            return ui.GetComponentsInChildren<TMPro.TextMeshProUGUI>()
                .Where(x => x.gameObject.activeInHierarchy && !string.IsNullOrEmpty(x.text) && x.GetComponentInParent<UnityEngine.UI.Button>() == null)
                .Select(x => x.text).OrderByDescending(x => x.Length).FirstOrDefault() ?? string.Empty;
        }

        /// <summary>
        /// Options shown by the open drama: (option, label). Options given in code (OpenDrama1...) come from the texts of
        /// the drama data; the others from the game's config (one per id: the game keeps a list per layout, CN/EN).
        /// </summary>
        public static List<Tuple<ConfDramaOptionsItem, string>> GetDramaOptions()
        {
            var ui = GetOpenDrama();
            var rs = new List<Tuple<ConfDramaOptionsItem, string>>();
            if (ui == null) return rs;
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
                if (rs.Any(x => x.Item1.id == o.id)) continue;
                var label = texts != null && texts.ContainsKey(o.id) ? texts[o.id] : custom != null && custom.ContainsKey(o.id) ? custom[o.id] : GameTool.LS(o.text);
                rs.Add(Tuple.Create(o, label));
            }
            return rs;
        }

        /// <summary>Chooses the option of the open drama with this label, as a click does. False when it is not shown.</summary>
        public static bool ChooseDramaOption(string label)
        {
            var ui = GetOpenDrama();
            var o = GetDramaOptions().FirstOrDefault(x => x.Item2 == label);
            if (ui == null || o == null) return false;
            ui.ClickOption(o.Item1);
            return true;
        }

        /// <summary>The next line of the open drama (a click on its text). False without a drama.</summary>
        public static bool NextDramaLine()
        {
            var d = g.ui.HasUI(UIType.DramaDialogue) ? g.ui.GetUI<UIDramaDialogue>(UIType.DramaDialogue) : null;
            if (d != null) { d.OnNextClick(); return true; }
            var f = g.ui.HasUI(UIType.DramaFortuitous) ? g.ui.GetUI<UIDramaFortuitous>(UIType.DramaFortuitous) : null;
            if (f != null) { f.OnNextClick(); return true; }
            var btn = GetOpenDrama()?.GetComponentsInChildren<UnityEngine.UI.Button>().FirstOrDefault(b => b.name.Contains("btnNext"));
            if (btn != null) { btn.onClick.Invoke(); return true; }
            return false;
        }

        /// <summary>
        /// Closes the open drama dialogs (an NPC joining a battle, stalkers, deals...) and an NPC info opened from them:
        /// they wait for a click and block the battle start / end and the game's timers. Also confirms the action
        /// course popup ("the target begs for their life..."; clicked, not closed: its callback goes on). Returns the count.
        /// </summary>
        public static int DismissDramas()
        {
            var n = 0;
            // closed, not clicked: the first labeled button of a drama is "View" (opens the NPC info and blocks again)
            foreach (var t in new[] { UIType.DramaDialogue, UIType.DramaFortuitous, UIType.DramaHerdNPC, UIType.DramaDialogueBigTexture, UIType.DramaBigTexture, UIType.NPCInfo })
            {
                if (!g.ui.HasUI(t)) continue;
                g.ui.CloseUI(t);
                n++;
            }
            var course = g.ui.GetUI<UIActionCourseInfo>(UIType.ActionCourseInfo);
            if (course != null && course.btnOK != null && course.btnOK.gameObject.activeInHierarchy && course.btnOK.interactable)
            {
                course.btnOK.onClick.Invoke();
                n++;
            }
            return n;
        }

        #endregion
    }
}