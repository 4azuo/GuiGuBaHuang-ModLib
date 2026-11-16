using MOD_nE7UL2.Const;
using MOD_nE7UL2.Mod;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Core
{
    /// <summary>
    /// TG-29 (ModLib CachableObject): a feature switched off by OnCacheHandler must not be reachable through its
    /// static Instance. Reproduces BUG-16 (docs/Known-Issues.md): NG until the bug is fixed.
    /// </summary>
    public class TG29_DisabledFeatures : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-29";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-29", Feature = "CachableObject (ModLib)", Title = "Disabled features have no Instance (BUG-16)" }
                .Step("instances", c =>
                {
                    var checkedAny = false;
                    void Check(string id, object instance)
                    {
                        if (IsLoaded(id)) { c.Info($"{id} enabled"); return; }
                        checkedAny = true;
                        c.Assert(instance == null, $"{id} disabled: Instance is {(instance == null ? "null" : "NOT null (BUG-16)")}");
                    }
                    Check(ModConst.TRAINER_EVENT, TrainerEvent.Instance);
                    Check(ModConst.QI_CUL_EVENT, QiCulEvent.Instance);
                    Check(ModConst.UPGRADE_NATURALLY_EVENT, UpgradeNaturallyEvent.Instance);
                    if (!checkedAny) c.Skip("every optional feature is enabled in this save");
                });
        }
    }
}
