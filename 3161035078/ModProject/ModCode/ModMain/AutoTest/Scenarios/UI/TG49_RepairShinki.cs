using ModLib.Helper;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.UI
{
    /// <summary>
    /// TG-49 (RepairShinkiEvent): a damaged god artifact (devil demon) gets a "Repair" button; it asks the cost,
    /// takes the money and repairs the artifact. The artifact is enabled and damaged for the test and restored
    /// afterwards; when AllowRepairGodWeapon is off the feature is tested with a temporary instance.
    /// Replaces the manual TM-15.
    /// </summary>
    public class TG49_RepairShinki : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-49";

        private static void RestoreWeapons(object saved) => GodArtifactHelper.SetAnimaWeapon((GameAnimaWeapon[])saved);

        // managed copy: ToArray() of the Il2Cpp list is an Il2Cpp array (the cast back to GameAnimaWeapon[] failed)
        private static object SaveWeapons()
        {
            var list = GodArtifactHelper.GetAnimaWeapon();
            var rs = new GameAnimaWeapon[list.Count];
            for (var i = 0; i < list.Count; i++) rs[i] = list[i];
            return rs;
        }

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            RepairShinkiEvent ev = null;
            var wasDamaged = false;
            return new Scenario { Id = "TG-49", Feature = "RepairShinkiEvent", Title = "Repair a damaged god artifact", Overlay = false }
                .Step("damage artifact", c =>
                {
                    c.Info(IsLoaded(ModConst.REPAIR_SHINKI_EVENT) ? "repair enabled" : "AllowRepairGodWeapon off: testing with a temporary instance");
                    ev = FeatureEvent<RepairShinkiEvent>(ModConst.REPAIR_SHINKI_EVENT);
                    c.Bag["weapons"] = SaveWeapons();
                    wasDamaged = GodArtifactHelper.IsPotmonDamaged();
                    GodArtifactHelper.EnableOnlyDevilDemon();
                    g.world.devilDemonMgr.devilDemonData.isDamage = true;
                    c.Assert(GodArtifactHelper.IsPotmonDamaged(), "devil demon damaged for the test");
                    Player.AddUnitMoney(10000000);
                })
                .Step("open artifact", c =>
                {
                    var ui = g.ui.OpenUISafe<UIDevilDemon>(UIType.DevilDemon);
                    RaiseOpenUIEnd(ev, ModConst.REPAIR_SHINKI_EVENT, ui, UIType.DevilDemon);
                })
                .Step("repair button", c =>
                {
                    if (!c.Assert(g.ui.HasUI(UIType.DevilDemon), "devil demon window opened")) return;
                    var btn = FindButtons(UIType.DevilDemon, GameTool.LS("other500020113")).FirstOrDefault();
                    if (!c.Assert(btn != null, "'Repair' button")) return;
                    c.Bag["money"] = Player.GetUnitMoney();
                    btn.Item.onClick.Invoke();
                    c.Assert(g.ui.HasUI(UIType.CheckPopup), "cost asked");
                }, 1000)
                .Step("confirm", c => c.Info($"clicked '{ClickButton("Yes", "是", "OK")}'"), 1000)
                .Step("repaired", c =>
                {
                    if (!c.Bag.ContainsKey("money")) { c.Skip("not repaired"); return; }
                    c.Assert(!GodArtifactHelper.IsPotmonDamaged(), "devil demon repaired");
                    c.Assert(Player.GetUnitMoney() < (int)c.Bag["money"], $"cost paid: {(int)c.Bag["money"] - Player.GetUnitMoney()}");
                    c.Assert(!g.ui.HasUI(UIType.DevilDemon), "window closed after the repair");
                }, 1000)
                .Step("restore", c =>
                {
                    ClickButton(CONFIRM);
                    CloseAll(UIType.DevilDemon);
                    g.world.devilDemonMgr.devilDemonData.isDamage = wasDamaged;
                    if (c.Bag.ContainsKey("weapons")) RestoreWeapons(c.Bag["weapons"]);
                    c.AssertEqual(wasDamaged, GodArtifactHelper.IsPotmonDamaged(), "artifact state restored");
                });
        }
    }
}
