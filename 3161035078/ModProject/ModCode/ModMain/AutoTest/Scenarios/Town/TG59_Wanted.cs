using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-59 (WantedEvent): bounties paid from a town budget. Posting takes the reward from the budget (duplicates and
    /// rewards under the minimum are refused); a hunter who wins kills the target and gets the reward; a failed hunt keeps
    /// the bounty; an expired bounty is refunded; a bounty on the player brings an ambush drama where paying 1.5 x the
    /// reward clears the name; the bounty board shows the wanted list.
    /// </summary>
    public class TG59_Wanted : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-59";

        private static WantedEvent W => WantedEvent.Instance;

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            MapBuildTown town = null;
            WorldUnitBase master = null, target = null, target2 = null, hunter = null;
            return new Scenario { Id = "TG-59", Feature = "WantedEvent", Title = "Bounties: post, hunt, expire, wanted player, wanted list", Overlay = false }
                .Step("town", c =>
                {
                    if (!c.Assert(W != null, "WantedEvent loaded")) return;
                    if (!TravelTo(c, FindTown(t => Has(t, MapBuildSubType.TownBounty) && MapBuildPropertyEvent.GetTownMaster(t) != null && !MapBuildPropertyEvent.GetTownMaster(t).IsPlayer()))) return;
                    town = BagTown(c);
                    master = MapBuildPropertyEvent.GetTownMaster(town);
                    var npcs = Npcs.Where(x => x.GetUnitId() != master.GetUnitId() && !HirePeopleEvent.IsHired(x)).OrderBy(x => x.GetGradeLvl()).ToList();
                    target = npcs.FirstOrDefault(); target2 = npcs.Skip(1).FirstOrDefault();
                    hunter = npcs.LastOrDefault();
                    MapBuildPropertyEvent.AddBuildProperty(town, WantedEvent.MinReward(Player) * 20);
                    c.Assert(target != null && target2 != null && hunter != null, $"master {master.GetName()}, targets {target?.GetName()}, {target2?.GetName()}, hunter {hunter?.GetName()}");
                })
                .Step("post", c =>
                {
                    if (target == null) return;
                    var min = WantedEvent.MinReward(target);
                    var budget = MapBuildPropertyEvent.GetBuildProperty(town);
                    c.AssertEqual("wanted_err_min", W.Post(town, master, target, min - 1), "a reward under the minimum is refused");
                    c.AssertEqual(null, W.Post(town, master, target, min), $"bounty of {min:#,##0} on {target.GetName()}");
                    c.AssertEqual(budget - min, MapBuildPropertyEvent.GetBuildProperty(town), "the reward left the town budget");
                    c.AssertEqual("wanted_err_already", W.Post(town, master, target, min), "a second bounty of the same town is refused");
                })
                .Step("hunt won", c =>
                {
                    var b = target == null ? null : WantedEvent.On(target).FirstOrDefault();
                    if (!c.Assert(b != null, "bounty listed")) return;
                    var hm = hunter.GetUnitMoney();
                    W.Hunt(b, hunter, 0f);
                    c.Assert(target.isDie, "the hunter killed the target");
                    c.AssertEqual(hm + b.Reward, hunter.GetUnitMoney(), "the hunter got the reward");
                    c.Assert(!W.Bounties.Contains(b), "bounty done");
                })
                .Step("hunt failed, expired", c =>
                {
                    if (target2 == null) return;
                    W.Post(town, master, target2, WantedEvent.MinReward(target2));
                    var b = WantedEvent.On(target2).First();
                    W.Hunt(b, hunter, 100f);
                    c.Assert(!target2.isDie && W.Bounties.Contains(b) && b.Attempts == 1, "a failed hunt keeps the bounty");
                    var budget = MapBuildPropertyEvent.GetBuildProperty(town);
                    b.PostedMonth = GameHelper.GetGameTotalMonth() - WantedEvent.EXPIRE_MONTHS;
                    W.OnMonthly();
                    c.Assert(!W.Bounties.Contains(b) && MapBuildPropertyEvent.GetBuildProperty(town) == budget + b.Reward, "expired: refunded to the town budget");
                })
                .Step("wanted player", c =>
                {
                    if (town == null) return;
                    c.AssertEqual(null, W.Post(town, master, Player, WantedEvent.MinReward(Player)), "bounty on the player");
                }, 1500)
                .Step("notice", c =>
                {
                    c.Assert(DramaText().Contains(town?.name ?? "?"), $"notice drama: {DramaText()}");
                    DismissDramas();
                }, 1500)
                .Step("hunter comes", c =>
                {
                    var b = WantedEvent.On(Player).FirstOrDefault();
                    if (b == null) return;
                    W.Hunt(b, hunter);
                }, 1500)
                .Step("ambush", c =>
                {
                    var b = WantedEvent.On(Player).FirstOrDefault();
                    if (!c.Assert(b != null, "bounty on the player")) return;
                    var labels = DramaOptions().Select(x => x.Item2).ToList();
                    var clear = string.Format(GameTool.LS("wanted_ambush_o2"), WantedEvent.ClearNamePrice(b));
                    c.Assert(labels.Contains(GameTool.LS("wanted_ambush_o1")) && labels.Contains(clear), $"ambush options: {string.Join(" | ", labels)}");
                    var price = WantedEvent.ClearNamePrice(b);
                    if (Player.GetUnitMoney() < price) Player.AddUnitMoney(price);
                    var m = Player.GetUnitMoney(); var budget = MapBuildPropertyEvent.GetBuildProperty(town);
                    c.Assert(ChooseOption(clear), "clear the name");
                    c.AssertEqual(m - price, Player.GetUnitMoney(), $"paid {price:#,##0}");
                    c.AssertEqual(budget + price + b.Reward, MapBuildPropertyEvent.GetBuildProperty(town), "the town gets the payment and its reward back");
                    c.Assert(WantedEvent.On(Player).Count == 0, "the player is no longer wanted");
                }, 1500)
                .Step("bounty board", c =>
                {
                    DismissDramas();
                    if (target2 == null) return;
                    W.Post(town, master, target2, WantedEvent.MinReward(target2));
                    OpenTownUI<UITownBounty>(UIType.TownBounty, ui => ui.InitData(town));
                }, 1500)
                .Step("wanted list", c =>
                {
                    var btn = FindButtons(UIType.TownBounty, GameTool.LS("wanted_list")).LastOrDefault();
                    if (!c.Assert(btn != null, "'Wanted list' button on the bounty board")) return;
                    btn.Item.onClick.Invoke();
                }, 1500)
                .Step("list shown", c =>
                {
                    var search = g.ui.GetUI<UINPCSearch>(UIType.NPCSearch);
                    c.Assert(search != null && WantedEvent.IsOpenWantedList, "wanted list opened (portraits)");
                    c.Assert(search != null && search.units.ToArray().Any(x => x.GetUnitId() == target2?.GetUnitId()), $"{target2?.GetName()} is in the wanted list");
                    // a click on a portrait opens its NPC info
                    if (target2 != null) g.ui.OpenUISafe<UINPCInfo>(UIType.NPCInfo).InitData(target2, false);
                }, 1500)
                .Step("details", c =>
                {
                    var info = g.ui.GetUI<UINPCInfo>(UIType.NPCInfo);
                    if (!c.Assert(info != null, "NPC info opened")) return;
                    c.Assert(info.tglTitle1.gameObject.activeSelf && !info.tglTitle2.gameObject.activeSelf && !info.tglTitle7.gameObject.activeSelf, "first tab only");
                    var b = WantedEvent.On(target2).FirstOrDefault();
                    c.Assert(b != null && FindTexts(info, b.Reward.ToString("#,##0")).Any(), "bounty details shown");
                    CloseAll(UIType.NPCInfo, UIType.NPCSearch, UIType.TownBounty);
                    c.Assert(!WantedEvent.IsOpenWantedList, "closed");
                }, 1000)
                .Step("month log", c =>
                {
                    var keys = TaoistLogEvent.Instance.Entries.Select(x => x.Key).ToList();
                    c.Assert(new[] { "log_wanted_post", "log_wanted_claimed", "log_wanted_failed", "log_wanted_expired", "log_wanted_ambush", "log_wanted_cleared" }.All(keys.Contains), "bounty events in the Taoist log");
                });
        }
    }
}
