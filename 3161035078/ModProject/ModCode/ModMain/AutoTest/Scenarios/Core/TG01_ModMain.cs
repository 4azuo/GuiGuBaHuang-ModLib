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

namespace MOD_nE7UL2.AutoTest.Scenarios.Core
{
    /// <summary>
    /// TG-01 (ModMain): Taoist settings and events are loaded in the save.
    /// </summary>
    public class TG01_ModMain : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-01";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-01", Feature = "ModMain", Title = "Taoist settings and events are loaded in the save" }
                .Step("load", c =>
                {
                    c.Assert(ModMain.ModObj?.ModSettings != null, "mod_configs.json loaded (ModSettings)");
                    var events = EventHelper.GetEvents().Where(x => x.ModId == ModMain.ModObj.ModId).Select(x => x.CacheId).ToList();
                    c.Info($"loaded Taoist events ({events.Count}): {string.Join(", ", events)}");
                    foreach (var id in new[] { ModConst.SM_LOCAL_CONFIGS_EVENT, ModConst.INFLATIONARY_EVENT, ModConst.UNIT_TYPE_EVENT, ModConst.MAP_BUILD_PROPERTY_EVENT,
                        ModConst.BATTLE_MODIFY_EVENT, ModConst.UNIT_MODIFY_EVENT, ModConst.REAL_MARKET_EVENT3, ModConst.HIRE_PEOPLE_EVENT, ModConst.UI_EVENT })
                        c.Assert(events.Contains(id), $"event {id} loaded");
                    var cfg = SMLocalConfigsEvent.Instance.Configs;
                    c.AssertEqual(cfg.EnableTrainer, events.Contains(ModConst.TRAINER_EVENT), "TrainerEvent loaded only when EnableTrainer");
                    c.AssertEqual(!cfg.NoQiCultivation, events.Contains(ModConst.QI_CUL_EVENT), "QiCulEvent loaded only when Qi cultivation is on");
                    c.AssertEqual(cfg.AllowUpgradeNaturally, events.Contains(ModConst.UPGRADE_NATURALLY_EVENT), "UpgradeNaturallyEvent loaded only when allowed");
                    c.AssertEqual(cfg.AllowRepairGodWeapon, events.Contains(ModConst.REPAIR_SHINKI_EVENT), "RepairShinkiEvent loaded only when allowed");
                });
        }
    }
}
