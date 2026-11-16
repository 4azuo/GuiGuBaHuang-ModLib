using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Time
{
    /// <summary>
    /// TX-04 (SliceLifeQuestEvent): from game year 2 (runs after TX-01 skipped a year) the slice-of-life quest is
    /// active: each month without battle increments its counter, a battle resets it, and reaching the required
    /// count completes the level and gives its luck. Changes the save (restored by run-autotest.ps1).
    /// Replaces the manual TM-31.
    /// </summary>
    public class TX04_SliceLifeQuest : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TX-04";

        private static Task104 CurrentTask()
        {
            var s = SliceLifeQuestEvent.Instance;
            var info = SliceLifeQuestEvent.TaskInfo.FirstOrDefault(x => x.TaskLevel == s.TaskLevel + 1);
            return info == null ? null : Player.GetTask(info.TaskId).ToArray().FirstOrDefault()?.TryCast<Task104>();
        }

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            var s = SliceLifeQuestEvent.Instance;
            return new Scenario { Id = "TX-04", Feature = "SliceLifeQuestEvent", Title = "Slice-of-life quest: monthly counter, reset by battle, completion luck" }
                .Step("active quest", c =>
                {
                    s = SliceLifeQuestEvent.Instance;
                    var info = SliceLifeQuestEvent.TaskInfo.FirstOrDefault(x => x.TaskLevel == s.TaskLevel + 1);
                    if (info == null) { c.Skip("all slice-of-life levels completed"); return; }
                    if (!info.ActiveCondition()) { c.Skip($"level {info.TaskLevel} not active yet in year {GameHelper.GetGameYear()}"); return; }
                    s.OnMonthly();
                    c.Assert(CurrentTask() != null || s.TaskLevel > info.TaskLevel - 1, $"quest task of level {info.TaskLevel} exists (year {GameHelper.GetGameYear()})");
                    c.Bag["level"] = s.TaskLevel;
                })
                .Step("month without battle", c =>
                {
                    if (!c.Bag.ContainsKey("level")) return;
                    var before = s.LastCount;
                    s.OnMonthly();
                    c.Assert(s.LastCount == before + 1 || s.TaskLevel > (int)c.Bag["level"], $"counter {before} -> {s.LastCount}");
                })
                .Step("battle resets", c =>
                {
                    if (!c.Bag.ContainsKey("level")) return;
                    s.OnBattleEndHandler(null);
                    c.AssertEqual(0, s.LastCount, "counter reset by a battle");
                    c.AssertEqual(0, CurrentTask()?.taskData.curCount ?? 0, "task count reset");
                })
                .Step("complete level", c =>
                {
                    if (!c.Bag.ContainsKey("level")) return;
                    var task = CurrentTask();
                    if (!c.Assert(task != null, "quest task")) return;
                    var level = s.TaskLevel;
                    task.taskData.curCount = task.data.task104Item.requireNumber - 1;
                    s.OnMonthly();
                    c.AssertEqual(level + 1, s.TaskLevel, $"level {level} -> {s.TaskLevel} after {task.data.task104Item.requireNumber} months");
                    c.AssertEqual(0, s.LastCount, "counter restarts");
                    c.Assert(Player.GetLuck(SliceLifeQuestEvent.LUCK_BASE_ID + s.TaskLevel) != null, $"slice-of-life luck {SliceLifeQuestEvent.LUCK_BASE_ID + s.TaskLevel} added");
                });
        }
    }
}
