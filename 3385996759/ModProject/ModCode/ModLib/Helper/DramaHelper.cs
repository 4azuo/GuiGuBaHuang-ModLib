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
            DramaTool.OpenDrama(ModLibConst.DEFAULT_TYPE1_DRAMA_ID, new DramaData
            {
                dialogueText = { [ModLibConst.DEFAULT_TYPE1_DRAMA_ID] = text },
                dialogueOptions = opts.ToIl2CppDictionary(x => DEFAULT_OPT_ID + x.Index, x => x.Value),
                hideDialogueOptions = DEFAULT_OPTS.Except(opts.Select((x, i) => i)).ToIl2CppList(),
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
        public static void OpenDrama1(string text, List<string> optMsgs, Action<ConfDramaOptionsItem> optClickCall,
            WorldUnitBase wunitLeft, WorldUnitBase wunitRight)
        {
            var opts = optMsgs.Select((x, i) => new { Index = i, Value = x }).ToArray();
            DramaTool.OpenDrama(ModLibConst.DEFAULT_TYPE1_DRAMA_ID, new DramaData
            {
                dialogueText = { [ModLibConst.DEFAULT_TYPE1_DRAMA_ID] = text },
                dialogueOptions = opts.ToIl2CppDictionary(x => DEFAULT_OPT_ID + x.Index, x => x.Value),
                hideDialogueOptions = DEFAULT_OPTS.Except(opts.Select((x, i) => i)).ToIl2CppList(),
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
            DramaTool.OpenDrama(ModLibConst.DEFAULT_TYPE2_DRAMA_ID, new DramaData
            {
                dialogueText = { [ModLibConst.DEFAULT_TYPE2_DRAMA_ID] = text },
                dialogueOptions = opts.ToIl2CppDictionary(x => DEFAULT_OPT_ID + x.Index, x => x.Value),
                hideDialogueOptions = DEFAULT_OPTS.Except(opts.Select((x, i) => i)).ToIl2CppList(),
                onOptionsClickCall = optClickCall,
                backgroud = { [ModLibConst.DEFAULT_TYPE2_DRAMA_ID] = background },
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