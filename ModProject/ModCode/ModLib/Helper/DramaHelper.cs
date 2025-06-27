using Il2CppSystem;
using Il2CppSystem.Collections.Generic;

public static partial class DramaHelper
{
    public static void OpenDrama(int dramaID, string text, Dictionary<int, string> optIds, List<int> hideOptIds, Action<ConfDramaOptionsItem> optClickCall)
    {
        DramaTool.OpenDrama(dramaID, new DramaData
        {
            dialogueText = { [dramaID] = text },
            dialogueOptions = optIds,
            hideDialogueOptions = hideOptIds,
            onOptionsClickCall = optClickCall,
        });
    }

    public static void OpenDrama(int dramaID, string text, Dictionary<int, string> optIds, List<int> hideOptIds, Action<ConfDramaOptionsItem> optClickCall,
        WorldUnitBase wunitLeft, WorldUnitBase wunitRight)
    {
        DramaTool.OpenDrama(dramaID, new DramaData
        {
            dialogueText = { [dramaID] = text },
            dialogueOptions = optIds,
            hideDialogueOptions = hideOptIds,
            onOptionsClickCall = optClickCall,
            unitLeft = wunitLeft,
            unitRight = wunitRight,
        });
    }
}