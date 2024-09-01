using MOD_nE7UL2.Const;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Obsolete]
    [Cache(ModConst.BATTLE_REWARD_EVENT)]
    public class BattleRewardEvent : ModBattleEvent
    {
        public IDictionary<string, double> TraningValues { get; set; } = new Dictionary<string, double>();
    }
}
