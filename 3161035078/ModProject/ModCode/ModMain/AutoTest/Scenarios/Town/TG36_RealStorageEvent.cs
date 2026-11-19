using ModLib.Helper;
using MOD_nE7UL2.Enum;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-36 (RealStorageEvent): storage value/fee texts; with a debt the storage shows red warnings; the monthly
    /// handler pays the debt (plus the fee) when the player has the money.
    /// </summary>
    public class TG36_RealStorageEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-36";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-36", Feature = "RealStorageEvent", Title = "Storage fee and debt", Overlay = false }
                .Step("travel", c => TravelTo(c, FindTown(t => Has(t, MapBuildSubType.TownStorage))))
                .Step("storage with debt", c =>
                {
                    RealStorageEvent.Instance.Debt = 1234;
                    c.Info("debt set to 1234 for the test");
                    OpenTownUI<UITownStorageProps>(UIType.TownStorageProps, ui => ui.InitData(BagTown(c)));
                })
                .Step("texts and warnings", c =>
                {
                    var ui = g.ui.GetUI<UITownStorageProps>(UIType.TownStorageProps);
                    if (!c.Assert(ui != null, "storage items window opened")) return;
                    var value = FindTexts(ui, "Storage:").FirstOrDefault();
                    c.Assert(value != null, $"value text: {value}");
                    var fee = FindTexts(ui, "Fee:").FirstOrDefault() ?? FindTexts(ui, GameTool.LS("other500020004")).FirstOrDefault();
                    c.Assert(fee != null, $"fee text: {fee}");
                    c.Assert(FindTexts(ui, "pay your debt (1,234").Count >= 1, "red debt warning shown");
                }, 1500)
                .Step("monthly payment", c =>
                {
                    CloseAll(UIType.TownStorageProps, UIType.TownStorage);
                    var free = UnitTypeEvent.GetUnitTypeEnum(Player) == UnitTypeEnum.Merchant || MapBuildPropertyEvent.IsTownGuardian(CurrentTown, Player);
                    if (free) { c.Skip("player is a merchant or town guardian: storage is free"); RealStorageEvent.Instance.Debt = 0; return; }
                    var fee = (long)(RealStorageEvent.GetStorageValue() * RealStorageEvent.FeeRate());
                    if (Player.GetUnitMoney() < 1234 + fee) Player.AddUnitMoney((int)(1234 + fee));
                    var before = Player.GetUnitMoney();
                    RealStorageEvent.Instance.OnMonthly();
                    c.AssertEqual(0L, RealStorageEvent.Instance.Debt, "debt paid");
                    c.AssertEqual(before - 1234 - (int)fee, Player.GetUnitMoney(), $"money paid: debt 1234 + fee {fee}");
                });
        }
    }
}
