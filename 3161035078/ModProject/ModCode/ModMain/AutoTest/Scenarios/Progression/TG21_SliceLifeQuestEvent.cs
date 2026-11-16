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

namespace MOD_nE7UL2.AutoTest.Scenarios.Progression
{
    /// <summary>
    /// TG-21 (SliceLifeQuestEvent/ChallengeQuestEvent): Quest chains have a consistent state.
    /// </summary>
    public class TG21_SliceLifeQuestEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-21";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-21", Feature = "SliceLifeQuestEvent/ChallengeQuestEvent", Title = "Quest chains have a consistent state" }
                .Step("quests", c =>
                {
                    var s = SliceLifeQuestEvent.Instance;
                    c.AssertBetween(s.TaskLevel, 0, SliceLifeQuestEvent.TaskInfo.Count, "slice-life level");
                    var next = SliceLifeQuestEvent.TaskInfo.FirstOrDefault(x => x.TaskLevel == s.TaskLevel + 1);
                    if (next != null && next.ActiveCondition())
                        c.Assert(Player.GetTask(next.TaskId).ToArray().Any(), $"slice-life task {next.TaskId} active");
                    var ch = ChallengeQuestEvent.Instance;
                    c.AssertBetween(ch.TaskLevel, 0, ChallengeQuestEvent.TaskInfo.Count, "challenge level");
                    var chNext = ChallengeQuestEvent.TaskInfo.FirstOrDefault(x => x.TaskLevel == ch.TaskLevel + 1);
                    if (chNext != null)
                    {
                        c.Assert(Player.GetTask(chNext.TaskId).ToArray().Any(), $"challenge task {chNext.TaskId} active");
                        c.Assert(ch.CurrentWins < chNext.RequiredWins, $"wins {ch.CurrentWins} < required {chNext.RequiredWins}");
                    }
                });
        }
    }
}
