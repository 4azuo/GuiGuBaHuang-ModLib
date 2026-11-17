using ModLib.Const;
using ModLib.Helper;
using ModLib.Mod;
using MOD_nE7UL2.Mod;
using System;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-47 (ContributionExchangeEvent): in the player's own sect the "Exchange" button opens an item selector
    /// with the monthly ratio; confirming gives contribution = value / ratio, adds the value to the sect money
    /// and takes the items. A player without sect joins the nearest one for the test and leaves it afterwards.
    /// Replaces the manual TM-08.
    /// </summary>
    public class TG47_SectExchange : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-47";

        private static int HerbId => g.conf.itemProps._allConfList.ToArray().First(x => x.IsHerbItem() != null && x.sale > 0).id;

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            MapBuildSchool school = null;
            return new Scenario { Id = "TG-47", Feature = "ContributionExchangeEvent", Title = "Exchange items for sect contribution", Overlay = false }
                .Step("sect", c =>
                {
                    c.Bag["noExchange"] = SMLocalConfigsEvent.Instance.Configs.SectNoExchange;
                    SMLocalConfigsEvent.Instance.Configs.SectNoExchange = false;
                    var own = Player.data.school;
                    if (own != null)
                    {
                        school = own;
                        c.Info($"player is member of {school?.name}");
                    }
                    else
                    {
                        var pos = Player.GetUnitPos();
                        // the nearest sect that accepts the player (a sect may refuse it)
                        var schools = (ModMaster.ModObj.ParameterStore?.Schools ?? new MapBuildSchool[0])
                            .OrderBy(x => Math.Abs(x.GetOrigiPoint().x - pos.x) + Math.Abs(x.GetOrigiPoint().y - pos.y)).ToList();
                        if (!c.Assert(schools.Count > 0, "a sect exists")) return;
                        foreach (var sc in schools)
                        {
                            school = sc;
                            Player.CreateAction(new UnitActionSchoolJoin(sc.buildData.id, SchoolPostType.Out, SchoolDepartmentType.None));
                            if (Player.data.school?.schoolNameID == sc.schoolNameID) break;
                        }
                        c.Bag["joined"] = true;
                        c.Info($"player joins {school.name} for the test");
                    }
                    c.Assert(school != null && Player.data.school?.schoolNameID == school.schoolNameID, "player is a member of the sect");
                })
                .Step("open sect", c =>
                {
                    if (school == null) { c.Skip("no sect"); return; }
                    OpenTownUI<UISchool>(UIType.School, ui => ui.InitData(school.buildData));
                })
                .Step("exchange button", c =>
                {
                    if (!c.Assert(g.ui.HasUI(UIType.School), "sect window opened")) return;
                    var btn = FindButtons(UIType.School, GameTool.LS("other500020000")).FirstOrDefault();
                    if (!c.Assert(btn != null, "'Exchange' button in the own sect")) return;
                    btn.Item.onClick.Invoke();
                    c.Assert(g.ui.HasUI(UIType.PropSelect), "item selector opened");
                    c.Assert(FindTexts(g.ui.GetUI<UIPropSelect>(UIType.PropSelect), ContributionExchangeEvent.Instance.CurMonthRatio.ToString()).Any(), $"ratio {ContributionExchangeEvent.Instance.CurMonthRatio} shown");
                }, 1000)
                .Step("exchange", c =>
                {
                    var ui = g.ui.GetUI<UIPropSelect>(UIType.PropSelect);
                    if (ui == null) { c.Skip("no selector"); return; }
                    Player.AddUnitProp(HerbId, 5);
                    var prop = Player.GetUnitProps(HerbId).First();
                    ui.AddSelectProps(prop);
                    ui.UpdateUI();
                    var value = UIPropSelect.allSlectDataProps.allProps.ToArray().Sum(x => x.propsInfoBase.sale * x.propsCount);
                    var ratio = ContributionExchangeEvent.Instance.CurMonthRatio;
                    var contribution = value == 0 ? 0 : Math.Max(1, value / ratio);
                    var c0 = Player.GetUnitPropCount(ModLibConst.CONTRIBUTION_PROP_ID);
                    var m0 = school.buildData.money;
                    var h0 = Player.GetUnitPropCount(HerbId);
                    c.Info($"selected value {value}, ratio {ratio}");
                    ui.btnOK.onClick.Invoke();
                    c.AssertEqual(c0 + contribution, Player.GetUnitPropCount(ModLibConst.CONTRIBUTION_PROP_ID), $"contribution + {contribution}");
                    c.AssertEqual(m0 + value, school.buildData.money, $"sect money + {value}");
                    c.Assert(Player.GetUnitPropCount(HerbId) < h0, $"exchanged items taken from the bag ({h0} -> {Player.GetUnitPropCount(HerbId)})");
                }, 1000)
                .Step("close", c =>
                {
                    CloseAll(UIType.GetReward, UIType.PropSelect, UIType.School); // the reward window of the contribution too (still open during TG-51, the game crashed refreshing it)
                    if (c.Bag.ContainsKey("joined")) Player.CreateAction(new UnitActionSchoolExit());
                    SMLocalConfigsEvent.Instance.Configs.SectNoExchange = (bool)c.Bag["noExchange"];
                })
                .Step("left sect", c =>
                {
                    if (c.Bag.ContainsKey("joined") && Player.data.school != null) school.ExitSchool(Player);
                    c.Assert(!c.Bag.ContainsKey("joined") || Player.data.school == null, $"temporary sect membership removed ({Player.data.school?.name ?? "no sect"})");
                }, 1500);
        }
    }
}
