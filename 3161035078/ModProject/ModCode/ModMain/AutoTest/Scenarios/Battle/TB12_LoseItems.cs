using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Battle
{
    /// <summary>
    /// TB-12 (BattleRewardEventV2, lost real duel): with LostItemWhenDie the sellable items of the player are lost (each kept with a chance of luck/10 % + ring lock) and the killer gets them.
    /// The lose options are set for the test only; the player state is restored afterwards.
    /// </summary>
    public class TB12_LoseItems : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TB-12";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TB-12", Feature = "BattleRewardEventV2", Title = "Battle lost with lost items when die: items go to the killer", Overlay = false }
                .Step("setup", c =>
                {
                    SetupLoss(c, lostItem: true);
                    var ids = g.conf.itemProps._allConfList.ToArray().Where(x => x.IsHerbItem() != null && x.sale > 0).Take(5).Select(x => x.id).ToArray();
                    foreach (var id in ids) Player.AddUnitProp(id, 1);
                    c.Bag["ids"] = ids;
                    c.Bag["mine"] = ids.Select(id => Player.GetUnitPropCount(id)).ToArray();
                    c.Bag["his"] = ids.Select(id => LossNpc(c)?.GetUnitPropCount(id) ?? 0).ToArray();
                    c.Info($"5 herb kinds added; luck {Player.GetDynProperty(UnitDynPropertyEnum.Luck).value} (keep chance luck/10 %)");
                })
                .LoseDuel()
                .Step("penalty", c =>
                {
                    if (LossNpc(c) == null) return;
                    c.Assert(OnMap, "player back on the world map");
                    var ids = (int[])c.Bag["ids"]; var mine = (int[])c.Bag["mine"]; var his = (int[])c.Bag["his"];
                    var lost = ids.Where((id, i) => Player.GetUnitPropCount(id) < mine[i]).ToList();
                    c.Assert(lost.Count > 0, $"items lost: {lost.Count}/5");
                    c.Assert(lost.All(id => LossNpc(c).GetUnitPropCount(id) > his[System.Array.IndexOf(ids, id)]), "the killer received every lost item");
                }, 2000)
                .Step("restore", c => RestoreAfterLoss(c));
        }
    }
}
