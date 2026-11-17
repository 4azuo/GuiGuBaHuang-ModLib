using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Time
{
    /// <summary>
    /// TX-01 (TimeSkipEvent/InflationaryEvent/monthly handlers): the "skip 1 year" map button asks for confirmation,
    /// then 12 months pass: the age follows the game time, the yearly price index is saved, the quest chains stay
    /// consistent. Changes the save (restored by run-autotest.ps1).
    /// </summary>
    public class TX01_TimeSkipYear : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TX-01";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TX-01", Feature = "TimeSkipEvent/InflationaryEvent", Title = "Skip 1 year with the map button", Overlay = false }
                .Step("skip button", c =>
                {
                    c.Bag["month"] = GameHelper.GetGameTotalMonth();
                    TimeSkipEvent.NoInterrupt = true; // a full year (an NPC drama would stop it: tested by TX-09)
                    c.Bag["price"] = EconomyHelper.GetWorldPrice();
                    c.Bag["age"] = Player.GetProperty<int>(UnitPropertyEnum.Age);
                    var btn = FindButtons(UIType.MapMain, string.Format(GameTool.LS("other500020030"), 1)).FirstOrDefault();
                    if (!c.Assert(btn != null, "'skip 1 year' button on the map")) return;
                    c.Info($"now {GameHelper.GetGameYear()}/{GameHelper.GetGameMonth()}, price index {c.Bag["price"]}");
                    btn.Item.onClick.Invoke();
                })
                .Step("confirm", c => c.Info($"clicked '{ClickButton("Yes", "是", "OK")}' in the confirmation"), 1500)
                .Wait("12 months passed", c => !TimeSkipEvent.Instance.IsProcessing && GameHelper.GetGameTotalMonth() >= (int)c.Bag["month"] + 12, 600000, 2000)
                .Step("after", c =>
                {
                    ClickButton(CONFIRM);
                    TimeSkipEvent.NoInterrupt = false;
                    c.AssertEqual((int)c.Bag["month"] + 12, GameHelper.GetGameTotalMonth(), $"time {GameHelper.GetGameYear()}/{GameHelper.GetGameMonth()}");
                    c.AssertEqual((int)c.Bag["age"] + 12, Player.GetProperty<int>(UnitPropertyEnum.Age), "the player aged 12 months");
                    c.AssertEqual(EconomyHelper.GetWorldPrice(), InflationaryEvent.Instance.WorldPrice, $"yearly price index saved: {c.Bag["price"]} -> {EconomyHelper.GetWorldPrice()}");
                    var s = SliceLifeQuestEvent.Instance;
                    c.AssertBetween(s.TaskLevel, 0, SliceLifeQuestEvent.TaskInfo.Count, "slice-life level still valid");
                }, 3000);
        }
    }
}
