using ModLib.Const;
using System;
using System.Collections.Generic;
using System.Linq;

public static class DramaHelper
{
    public static void OpenDrama1(string text, Dictionary<int, string> optIds, List<int> hideOptIds, Action<ConfDramaOptionsItem> optClickCall)
    {
        DramaTool.OpenDrama(ModLibConst.DEFAULT_TYPE1_DRAMA_ID, new DramaData
        {
            dialogueText = { [ModLibConst.DEFAULT_TYPE1_DRAMA_ID] = text },
            dialogueOptions = (optIds ?? GetDefaultOpt())?.ToIl2CppDictionary(x => x.Key, x => x.Value),
            hideDialogueOptions = hideOptIds?.ToIl2CppList(),
            onOptionsClickCall = optClickCall,
        });
    }

    public static void OpenDrama1(string text, Dictionary<int, string> optIds, List<int> hideOptIds, Action<ConfDramaOptionsItem> optClickCall,
        WorldUnitBase wunitLeft, WorldUnitBase wunitRight)
    {
        DramaTool.OpenDrama(ModLibConst.DEFAULT_TYPE1_DRAMA_ID, new DramaData
        {
            dialogueText = { [ModLibConst.DEFAULT_TYPE1_DRAMA_ID] = text },
            dialogueOptions = (optIds ?? GetDefaultOpt())?.ToIl2CppDictionary(x => x.Key, x => x.Value),
            hideDialogueOptions = hideOptIds?.ToIl2CppList(),
            onOptionsClickCall = optClickCall,
            unitLeft = wunitLeft,
            unitRight = wunitRight,
        });
    }

    public static void OpenDrama2(string text, Dictionary<int, string> optIds, List<int> hideOptIds, Action<ConfDramaOptionsItem> optClickCall, string background)
    {
        DramaTool.OpenDrama(ModLibConst.DEFAULT_TYPE2_DRAMA_ID, new DramaData
        {
            dialogueText = { [ModLibConst.DEFAULT_TYPE2_DRAMA_ID] = text },
            dialogueOptions = (optIds ?? GetDefaultOpt())?.ToIl2CppDictionary(x => x.Key, x => x.Value),
            hideDialogueOptions = hideOptIds?.ToIl2CppList(),
            onOptionsClickCall = optClickCall,
            backgroud = { [ModLibConst.DEFAULT_TYPE2_DRAMA_ID] = background },
        });
    }

    public static Dictionary<int, string> GetDefaultOpt()
    {
        return new Tuple<int, string>[] { new Tuple<int, string>(999890005, GameTool.LS("libtxt999990005")) }.ToDictionary(x => x.Item1, x => x.Item2);
    }

    public static Dictionary<int, string> CreateOpts(params string[] opts)
    {
        return opts.Select((x, i) => new { Index = i, Value = x }).ToDictionary(x => x.Index, x => x.Value);
    }
}