using ModLib.Helper;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.UI
{
    /// <summary>TG-42 (UseSpiritStoneCulEvent): the cultivation screen button converts spirit stones into exp.</summary>
    public class TG42_UseSpiritStoneCulEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-42";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-42", Feature = "UseSpiritStoneCulEvent", Title = "Cultivate with spirit stones", Overlay = false }
                .Step("open cultivation", c =>
                {
                    if (Player.IsFullExp()) { c.Skip("player exp is full (button hidden)"); return; }
                    var cost = 10000 * Player.GetGradeLvl();
                    if (Player.GetUnitMoney() <= cost) Player.AddUnitMoney(cost * 2);
                    var ui = g.ui.GetUI<UIMapMain>(UIType.MapMain);
                    (ui.playerInfo.btnFateFeature ?? ui.playerInfo.btnFateFeature_En).onClick.Invoke();
                })
                .Step("cultivate", c =>
                {
                    var btn = FindButtons(UIType.FateFeature, GameTool.LS("other500020068")).FirstOrDefault();
                    if (!c.Assert(btn != null, "'cultivate with spirit stones' button")) return;
                    c.Bag["money"] = Player.GetUnitMoney();
                    c.Bag["exp"] = Player.GetExp();
                    btn.Item.onClick.Invoke();
                    var cost = 10000 * Player.GetGradeLvl();
                    c.AssertEqual((int)c.Bag["money"] - cost, Player.GetUnitMoney(), $"paid {cost} spirit stones");
                    c.Assert(Player.GetExp() > (int)c.Bag["exp"], $"exp {(int)c.Bag["exp"]} -> {Player.GetExp()}");
                }, 1500)
                .Step("close", c => { ClickButton(CONFIRM); if (g.ui.HasUI(UIType.FateFeature)) g.ui.CloseUI(UIType.FateFeature); }, 1500);
        }
    }
}
