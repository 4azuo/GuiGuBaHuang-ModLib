using MOD_nE7UL2.Mod;
using ModLib.Helper;
using ModLib.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Core
{
    /// <summary>
    /// TG-03 (PrepareEvent): with "All functions apply to nearest units" on, the monthly ParameterStore of Taoist only
    /// contains units within +-3 areas. The option is switched on temporarily when it is off.
    /// </summary>
    public class TG03_PrepareEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-03";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-03", Feature = "PrepareEvent", Title = "Nearest-units option limits the units used by Taoist events" }
                .Step("enable option", c =>
                {
                    var cfg = SMLocalConfigsEvent.Instance.Configs;
                    c.Bag["was"] = cfg.AllFunctionsApplyToNearestUnits;
                    cfg.AllFunctionsApplyToNearestUnits = true;
                    c.Info($"option was {(bool)c.Bag["was"]}, set to true for the test");
                    c.Info($"all units in the world snapshot: {ModMaster.ModObj.ParameterStore.WUnits.Length}");
                })
                .Step("refresh store", c =>
                {
                    var prepare = EventHelper.GetEvent<PrepareEvent>("$PrepareEvent$");
                    if (!c.Assert(prepare != null, "PrepareEvent loaded")) return;
                    prepare.OnRefreshParameterStoreOnMonthly();
                    var store = ModMain.ModObj.ParameterStore;
                    if (!c.Assert(store != null, "Taoist ParameterStore set")) return;
                    var area = Player.GetUnitPosAreaId();
                    c.Assert(store.WUnits.Length <= ModMaster.ModObj.ParameterStore.WUnits.Length, $"subset: {store.WUnits.Length} of {ModMaster.ModObj.ParameterStore.WUnits.Length} units");
                    c.Assert(store.WUnits.All(x => x.GetUnitPosAreaId().IsBetween(area - 3, area + 3)), $"all units within areas {area - 3}..{area + 3}");
                    c.Assert(store.Towns.Length == ModMaster.ModObj.ParameterStore.Towns.Length, "towns not filtered");
                })
                .Step("restore", c =>
                {
                    SMLocalConfigsEvent.Instance.Configs.AllFunctionsApplyToNearestUnits = (bool)c.Bag["was"];
                    ModMain.ModObj.SetParameterStore(null);
                    c.Assert(ModMain.ModObj.ParameterStore == null, "option and ParameterStore restored");
                });
        }
    }
}
