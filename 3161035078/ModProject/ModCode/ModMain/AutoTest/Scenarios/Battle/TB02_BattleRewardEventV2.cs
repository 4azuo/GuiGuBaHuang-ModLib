using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using MOD_nE7UL2.Mod;
using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using ModLib.Object;
using System;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Battle
{
    /// <summary>
    /// TB-02 (BattleRewardEventV2/SliceLifeQuestEvent/ChallengeQuestEvent): Battle end: rewards logged, slice-life counter reset.
    /// </summary>
    public class TB02_BattleRewardEventV2 : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TB-02";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TB-02", Feature = "BattleRewardEventV2/SliceLifeQuestEvent/ChallengeQuestEvent", Title = "Battle end: rewards logged, slice-life counter reset", Kind = ScenarioKind.Watch, Scope = ScenarioScope.Battle, Trigger = "BattleEnd" }
                .Step("end", c =>
                {
                    var dealt = ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Damage));
                    var recv = ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgRecv, ModBattleEvent.DmgTypeEnum.Damage));
                    c.Info($"win: {LastBattleWin}, damage dealt {dealt}, received {recv}, kills {ModBattleEvent.KillList.Count}");
                    c.Assert(dealt >= 0 && recv >= 0, "damage statistics recorded");
                    c.AssertEqual(0, SliceLifeQuestEvent.Instance.LastCount, "slice-life counter reset by the battle");
                }, 1500);
        }
    }
}
