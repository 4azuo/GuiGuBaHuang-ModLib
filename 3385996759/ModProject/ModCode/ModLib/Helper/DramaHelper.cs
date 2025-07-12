using ModLib.Const;
using System;
using System.Collections.Generic;
using System.Linq;

public static class DramaHelper
{
    private const int DEFAULT_OPT_ID = 999790000;
    private static int[] DEFAULT_OPTS = new int[] { 999790000, 999790001, 999790002, 999790003, 999790004, 999790005, 999790006, 999790007, 999790008, 999790009 };

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

    [Obsolete]
    public static void OpenDrama3()
    {
    }

    [Obsolete]
    public static void OpenDrama4()
    {
    }

    [Obsolete]
    public static void OpenDrama5()
    {
    }

    [Obsolete]
    public static void OpenDrama6()
    {
    }

    [Obsolete]
    public static void OpenDrama7()
    {
    }

    [Obsolete]
    public static void OpenDrama8()
    {
    }
}