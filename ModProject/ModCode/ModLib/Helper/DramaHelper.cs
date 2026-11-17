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
    }
}