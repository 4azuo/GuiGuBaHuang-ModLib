using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Unit
{
    /// <summary>
    /// TG-80 (MeritEvent): merit levels and their bonuses (attack, defense, Luck kept in step), the essence above the top,
    /// the merit encounters (flood relief, plague, beast raid walked away), karma (Luck down, a bounty on the player),
    /// the merit window. Merit, Luck, money and bounties restored.
    /// </summary>
    public class TG80_Merit : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-80";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-80", Feature = "MeritEvent", Title = "Merit: levels, bonuses, encounters, karma", Overlay = false }
                .Step("merit levels", c =>
                {
                    if (!c.Assert(MeritEvent.Enabled, "MeritEvent loaded")) return;
                    var id = Player.GetUnitId();
                    c.Bag["merit"] = MeritEvent.Instance.Merits.TryGetValue(id, out var old) ? Newtonsoft.Json.JsonConvert.SerializeObject(old) : null;
                    c.Bag["luck"] = Player.GetProperty<int>(UnitPropertyEnum.Luck) - (old?.LuckGiven ?? 0);
                    c.Bag["money"] = Player.GetUnitMoney();
                    c.Bag["bounties"] = WantedEvent.Instance?.Bounties.Select(x => x.Id).ToList();
                    // start from no merit (Luck given by the old merit taken back)
                    if (old != null) { old.Merit = 0; old.Essence = 0; MeritEvent.Sync(Player); }
                    var luck0 = Player.GetProperty<int>(UnitPropertyEnum.Luck);
                    MeritEvent.Add(Player, 2000, "merit_src_donate");
                    c.AssertEqual(3, MeritEvent.Level(Player), "2,000 merit: Blessed Virtue (level 3)");
                    c.AssertEqual(luck0 + 1, Player.GetProperty<int>(UnitPropertyEnum.Luck), "Luck +1 (level 3)");
                    UnitModifyEvent.Instance.CachedValues.Clear();
                    var atkBase = Player.GetDynProperty(UnitDynPropertyEnum.Attack).baseValue;
                    var defBase = Player.GetDynProperty(UnitDynPropertyEnum.Defense).baseValue;
                    c.AssertEqual((int)System.Math.Round(atkBase * 0.03) + 3, MeritEvent.AdjAtk(Player, atkBase), $"attack +3 % of {atkBase} +3");
                    c.AssertEqual((int)System.Math.Round(defBase * 0.045) + 3, MeritEvent.AdjDef(Player, defBase), $"defense +4.5 % of {defBase} +3");
                    c.Assert(UnitModifyEvent.GetAdjustAtk(Player) >= MeritEvent.AdjAtk(Player, atkBase), "the attack bonus reaches the unit");
                    c.Assert(TaoistLogEvent.Instance.Entries.Any(x => x.Key == "log_merit_level"), "Taoist log: merit level");
                    // above the top: essence
                    MeritEvent.Add(Player, MeritEvent.MaxMerit);
                    var d = MeritEvent.Get(Player);
                    c.AssertEqual(6, MeritEvent.Level(Player), "Boundless Merit (top)");
                    c.AssertEqual(2000L, d.Essence, "the points above the top became essence");
                    c.AssertEqual(luck0 + 3, Player.GetProperty<int>(UnitPropertyEnum.Luck), "Luck +3 at the top");
                    c.AssertEqual(0.7f, MeritEvent.LightningRate(Player), "lightning -30 %");
                })
                .Step("cultivation screen", c =>
                {
                    var ui = g.ui.GetUI<UIMapMain>(UIType.MapMain);
                    (ui.playerInfo.btnFateFeature ?? ui.playerInfo.btnFateFeature_En).onClick.Invoke();
                }, 1500)
                .Step("merit window", c =>
                {
                    var btn = FindButtons(UIType.FateFeature, GameTool.LS("merit_button")).FirstOrDefault();
                    if (c.Assert(btn != null, "'Merit' button on the cultivation screen")) btn.Item.onClick.Invoke();
                    else MeritEvent.OpenWindow();
                    c.Assert(g.ui.HasUI(UIType.TextInfoLong), "merit window opened");
                }, 1500)
                .Step("merit encounters", c =>
                {
                    CloseAll(UIType.TextInfoLong, UIType.FateFeature);
                    var d = MeritEvent.Get(Player);
                    d.Merit = 0; d.Essence = 0; MeritEvent.Sync(Player);
                    var grade = System.Math.Max(1, Player.GetGradeLvl());
                    Player.AddUnitMoney(EncounterEvent.Cost(EncounterEvent.FLOOD_COST) * 2);
                    c.AssertEqual("enc_flood_r", EncounterEvent.Resolve(EncounterEvent.Encounter.Flood, 0, null, 50f), "flood relief: donated");
                    c.AssertEqual((long)EncounterEvent.FLOOD_MERIT * grade, d.Merit, $"merit +{EncounterEvent.FLOOD_MERIT} x grade {grade}");
                    var herb = g.conf.itemProps._allConfList.ToArray().FirstOrDefault(x => x.IsHerbItem() != null);
                    if (herb != null) Player.AddUnitProp(herb.id, EncounterEvent.PLAGUE_HERBS);
                    var before = d.Merit;
                    c.AssertEqual("enc_plague_r", EncounterEvent.Resolve(EncounterEvent.Encounter.Plague, 0, null, 50f), "plague: herbs given");
                    c.AssertEqual(before + EncounterEvent.PLAGUE_MERIT * grade, d.Merit, "merit of the plague");
                    before = d.Merit;
                    c.AssertEqual("enc_raid_flee", EncounterEvent.Resolve(EncounterEvent.Encounter.BeastRaid, 1, null, 50f), "beast raid: walked away");
                    c.AssertEqual(before + EncounterEvent.RAID_FLEE_MERIT * grade, d.Merit, "a little karma for walking away");
                    if (Player.GetMapBuild<MapBuildTown>() != null)
                        c.Assert(EncounterEvent.Possible().Contains(EncounterEvent.Encounter.Flood), "in a town: flood relief is possible");
                    else
                        c.Assert(EncounterEvent.Possible().Contains(EncounterEvent.Encounter.BeastRaid), "outside towns: a beast raid is possible");
                })
                .Step("karma", c =>
                {
                    var d = MeritEvent.Get(Player);
                    var luck0 = Player.GetProperty<int>(UnitPropertyEnum.Luck) - d.LuckGiven;
                    MeritEvent.Add(Player, -600 - d.Merit, "merit_src_kill_good");
                    c.AssertEqual(-2, MeritEvent.Level(Player), "-600: Wicked (karma -2)");
                    c.AssertEqual(luck0 - 2, Player.GetProperty<int>(UnitPropertyEnum.Luck), "Luck -2");
                    c.AssertEqual(1.2f, MeritEvent.LightningRate(Player), "lightning +20 %");
                    if (WantedEvent.Disabled)
                        c.Info("bounties off (S&M): no karma bounty");
                    else
                    {
                        var err = MeritEvent.PostKarmaBounty();
                        c.Assert(err == null || err == "wanted_err_already", $"a righteous cultivator put a bounty on the player ({err ?? "posted"})");
                        c.Assert(WantedEvent.On(Player).Any(), "a bounty on the player");
                    }
                })
                .Step("restore", c =>
                {
                    var id = Player.GetUnitId();
                    var d = MeritEvent.Get(Player);
                    if (d != null) { d.Merit = 0; d.Essence = 0; MeritEvent.Sync(Player); }
                    MeritEvent.Instance.Merits.Remove(id);
                    if (c.Bag["merit"] != null)
                    {
                        var old = Newtonsoft.Json.JsonConvert.DeserializeObject<MeritEvent.MeritData>((string)c.Bag["merit"]);
                        var given = old.LuckGiven;
                        old.LuckGiven = 0;
                        MeritEvent.Instance.Merits[id] = old;
                        MeritEvent.Sync(Player);
                        c.Info($"merit restored ({old.Merit}, Luck given {given})");
                    }
                    var keep = c.Bag["bounties"] as System.Collections.Generic.List<string>;
                    if (keep != null) WantedEvent.Instance.Bounties.RemoveAll(x => !keep.Contains(x.Id));
                    Player.AddUnitMoney((int)c.Bag["money"] - Player.GetUnitMoney());
                    UnitModifyEvent.Instance.CachedValues.Clear();
                    c.AssertEqual((int)c.Bag["luck"] + (MeritEvent.Get(Player)?.LuckGiven ?? 0), Player.GetProperty<int>(UnitPropertyEnum.Luck), "Luck restored");
                });
        }
    }
}
