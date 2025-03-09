using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using System;
using System.Collections.Generic;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.NPC_AUTO_CRAFT_ARTIFACT)]
    public class NpcAutoCraftArtifact : ModEvent
    {
        public static NpcAutoCraftArtifact Instance { get; set; }

    }
}
