using ModLib.Helper;
using MOD_nE7UL2.Mod;
using MOD_nE7UL2.Object;
using System.Collections.Generic;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-38 (MissionDeclareEvent): a finished commission is delivered by the monthly handler: reward window with
    /// the items, items added to the bag, commission removed.
    /// </summary>
    public class TG38_MissionDeclareEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-38";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-38", Feature = "MissionDeclareEvent", Title = "Commission delivered by the monthly handler", Overlay = false }
                .Step("travel", c => TravelTo(c, FindTown(t => Has(t, MapBuildSubType.TownBounty))))
                .Step("commission", c =>
                {
                    var item = g.conf.itemProps._allConfList.ToArray().First(x => x.IsHerbItem() != null);
                    c.Bag["item"] = item.id;
                    c.Bag["count"] = Player.GetUnitPropCount(item.id);
                    var task = new CommissionTask { CommisionItems = new Dictionary<int, int> { [item.id] = 3 }, CostTime = 0, SuccessRate = 100, PassTime = 0 };
                    MissionDeclareEvent.Instance.CommissionTasks.Add(task);
                    c.Info($"commission of 3 x {GameTool.LS(item.name)} ({item.id}) due this month");
                })
                .Step("month passes", c => MissionDeclareEvent.Instance.OnMonthly())
                .Step("delivered", c =>
                {
                    var id = (int)c.Bag["item"];
                    c.Assert(g.ui.HasUI(UIType.GetReward), "reward window opened");
                    c.AssertEqual((int)c.Bag["count"] + 3, Player.GetUnitPropCount(id), "3 items added to the bag");
                    c.Assert(MissionDeclareEvent.Instance.CommissionTasks.All(x => !x.CommisionItems.ContainsKey(id) || x.Status == CommissionTask.CommissionTaskStatus.Progressing), "finished commission removed");
                }, 1500)
                .Step("close", c => CloseAll(UIType.GetReward));
        }
    }
}
