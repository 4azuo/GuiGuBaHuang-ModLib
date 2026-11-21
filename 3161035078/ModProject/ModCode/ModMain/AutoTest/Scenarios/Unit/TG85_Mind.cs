using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Unit
{
    /// <summary>
    /// TG-85 (DaoHeartEvent, SpiritSenseEvent): the Dao heart at the top gives max SP +10 % and max MP +10 % to the unit;
    /// a heart demon seclusion is beaten by the Dao heart and feeds the heart (essence) and the spirit sense; killing a
    /// friend in a real duel is a betrayal (-150); the spirit sense at level 5 gives max SP +20 % and max MP +10 %, a
    /// bounty hunter's ambush is sensed and avoided (no ambush drama), the scan (window button) reports the surroundings
    /// once a month; both windows from the cultivation screen. Data restored (the save is restored after the run).
    /// </summary>
    public class TG85_Mind : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-85";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-85", Feature = "DaoHeartEvent", Title = "Dao heart and spirit sense: max SP / MP, heart demon, betrayal, ambush, scan", Overlay = false }
                .Step("Dao heart at the top", c =>
                {
                    if (!c.Assert(DaoHeartEvent.Enabled && SpiritSenseEvent.Enabled, "DaoHeartEvent and SpiritSenseEvent loaded")) return;
                    var id = Player.GetUnitId();
                    c.Bag["heart"] = DaoHeartEvent.Instance.Hearts.TryGetValue(id, out var h) ? Newtonsoft.Json.JsonConvert.SerializeObject(h) : null;
                    c.Bag["sense"] = SpiritSenseEvent.Instance.Senses.TryGetValue(id, out var s) ? Newtonsoft.Json.JsonConvert.SerializeObject(s) : null;
                    c.Bag["exp"] = Player.GetExp();
                    c.Bag["bounties"] = WantedEvent.Instance?.Bounties.Select(x => x.Id).ToList();
                    DaoHeartEvent.Instance.Hearts.Remove(id);
                    SpiritSenseEvent.Instance.Senses.Remove(id);
                    c.AssertEqual(1, DaoHeartEvent.Level(Player), "a new heart is Steady");
                    DaoHeartEvent.Add(Player, DaoHeartEvent.MaxValue, "heart_src_seclusion");
                    c.AssertEqual(5, DaoHeartEvent.Level(Player), "Heart of Heaven");
                    UnitModifyEvent.Instance.CachedValues.Clear();
                    var sp = Player.GetDynProperty(UnitDynPropertyEnum.SpMax).baseValue;
                    var mp = Player.GetDynProperty(UnitDynPropertyEnum.MpMax).baseValue;
                    c.AssertEqual((int)System.Math.Round(sp * 0.1, System.MidpointRounding.AwayFromZero), DaoHeartEvent.AdjSp(Player, sp), $"max SP +10 % of {sp}");
                    c.AssertEqual((int)System.Math.Round(mp * 0.1, System.MidpointRounding.AwayFromZero), DaoHeartEvent.AdjMp(Player, mp), $"max MP +10 % of {mp}");
                    c.Assert(UnitModifyEvent.GetAdjustMaxSp(Player) >= DaoHeartEvent.AdjSp(Player, sp), "the SP bonus reaches the unit");
                    c.Assert(UnitModifyEvent.GetAdjustMaxMp(Player) >= DaoHeartEvent.AdjMp(Player, mp), "the MP bonus reaches the unit");
                })
                .Step("heart demon beaten", c =>
                {
                    var essence0 = DaoHeartEvent.Get(Player).Essence;
                    var sense0 = SpiritSenseEvent.Get(Player, true).Exp;
                    var text = SeclusionEvent.Reward(new SeclusionEvent.Running { Kind = SeclusionEvent.Kind.HeartDemon, Months = 24, Roll = 80 });
                    c.Info(text);
                    c.AssertEqual(GameTool.LS("secl_res_HeartDemon_win"), text, "roll 80 < 90 (Heart of Heaven): the heart demon is beaten");
                    c.AssertEqual(essence0 + DaoHeartEvent.HEART_DEMON_WIN, DaoHeartEvent.Get(Player).Essence, "above the top: +100 to the essence");
                    c.AssertEqual(sense0 + SpiritSenseEvent.HEART_DEMON_WIN, SpiritSenseEvent.Get(Player).Exp, "spirit sense +200");
                    c.Assert(SeclusionEvent.HeartDemonRate < SeclusionEvent.OfferRate * 0.3f + 0.0001f, "the heart demons come no more often");
                })
                .Step("a friend for the duel", c =>
                {
                    var npc = Player.GetUnitsAround(12, false, false).ToArray().FirstOrDefault(x => x != null && !x.isDie && !x.IsPlayer() && !HirePeopleEvent.IsHired(x))
                        ?? ModMain.Store.WUnits.First(x => x != null && !x.isDie && !x.IsPlayer());
                    for (var i = 0; i < 10 && npc.data.unitData.relationData.GetIntim(Player) < DaoHeartEvent.BETRAY_INTIM; i++)
                        npc.data.unitData.relationData.AddIntim(Player.GetUnitId(), 200);
                    c.Assert(DaoHeartEvent.IsBond(Player, npc), $"{npc.GetName()} is a bond (intimacy {npc.data.unitData.relationData.GetIntim(Player)})");
                    c.Bag["npc"] = npc.GetUnitId();
                    DaoHeartEvent.Get(Player).Value = 1000;
                    DaoHeartEvent.Get(Player).Essence = 0;
                    BattleHelper.StartRealDuel(npc);
                })
                .Wait("duel loaded", c => { StartBattle(); ProtectPlayer(); return InBattle; }, 90000, 1000)
                .Wait("duel started", c => { ProtectPlayer(); return StartBattle(); }, 60000, 1000)
                .Step("the friend killed", c =>
                {
                    if (!InBattle) return;
                    ProtectPlayer();
                    var npc = g.world.unit.GetUnit((string)c.Bag["npc"]);
                    c.Info($"kill: {PlayerKill(BattleUnitOf(npc))}");
                }, 1500)
                .Step("betrayal", c =>
                {
                    c.AssertEqual(1000L + DaoHeartEvent.BETRAYAL, DaoHeartEvent.Get(Player).Value, "a betrayal: Dao heart -150");
                    c.Assert(TaoistLogEvent.Instance.Entries.Any(x => x.Key == "log_heart_down"), "Taoist log: Dao heart down");
                    if (InBattle) EndBattle(true);
                }, 1500)
                .Wait("back to the map", c => LeaveBattle(), 90000, 2000)
                .Step("spirit sense", c =>
                {
                    DramaHelper.DismissDramas();
                    SpiritSenseEvent.AddExp(Player, SpiritSenseEvent.MaxExp);
                    c.AssertEqual(5, SpiritSenseEvent.Level(Player), "Sense of Heaven");
                    UnitModifyEvent.Instance.CachedValues.Clear();
                    var mp = Player.GetDynProperty(UnitDynPropertyEnum.MpMax).baseValue;
                    var sp = Player.GetDynProperty(UnitDynPropertyEnum.SpMax).baseValue;
                    c.AssertEqual((int)System.Math.Round(mp * 0.1, System.MidpointRounding.AwayFromZero), SpiritSenseEvent.AdjMp(Player, mp), $"max MP +10 % of {mp}");
                    c.AssertEqual((int)System.Math.Round(sp * 0.2, System.MidpointRounding.AwayFromZero), SpiritSenseEvent.AdjSp(Player, sp), $"max SP +20 % of {sp}");
                    c.Assert(UnitModifyEvent.GetAdjustMaxSp(Player) >= SpiritSenseEvent.AdjSp(Player, sp) + DaoHeartEvent.AdjSp(Player, sp), "SP of the Dao heart and of the sense together");
                })
                .Step("an ambush sensed", c =>
                {
                    if (WantedEvent.Disabled || WantedEvent.Instance == null) { c.Info("bounties off"); return; }
                    // a town (the list of the store may hold null) and a poster who is not the player
                    var town = Player.GetMapBuild<MapBuildTown>() ?? ModMain.Store.Towns.First(t => t != null);
                    var poster = ModMain.Store.WUnits.First(x => x != null && !x.isDie && x.GetUnitId() != Player.GetUnitId());
                    // the town pays the bounty from its budget (earlier scenarios of the same game may have spent it)
                    var reward = WantedEvent.MinReward(Player);
                    if (MapBuildPropertyEvent.GetBuildProperty(town) < reward) MapBuildPropertyEvent.AddBuildProperty(town, reward);
                    var err = WantedEvent.Instance.Post(town, poster, Player, reward);
                    var b = WantedEvent.On(Player).FirstOrDefault();
                    if (!c.Assert(b != null, $"a bounty on the player{(err == null ? "" : $" (refused: {err})")}")) return;
                    var avoided0 = SpiritSenseEvent.Get(Player).Avoided;
                    SpiritSenseEvent.TestRoll = 10f;
                    WantedEvent.Instance.Hunt(b);
                    SpiritSenseEvent.TestRoll = null;
                    c.AssertEqual(avoided0 + 1, SpiritSenseEvent.Get(Player).Avoided, "the ambush sensed and avoided (roll 10 < 90)");
                    c.Assert(TaoistLogEvent.Instance.Entries.Any(x => x.Key == "log_sense_ambush"), "Taoist log: ambush sensed");
                    c.Assert(!TaoistLogEvent.Instance.Entries.Skip(System.Math.Max(0, TaoistLogEvent.Instance.Entries.Count - 2)).Any(x => x.Key == "log_wanted_ambush"), "no ambush");
                }, 1500)
                .Step("cultivation screen", c =>
                {
                    DramaHelper.DismissDramas();
                    var ui = g.ui.GetUI<UIMapMain>(UIType.MapMain);
                    (ui.playerInfo.btnFateFeature ?? ui.playerInfo.btnFateFeature_En).onClick.Invoke();
                }, 1500)
                .Step("spirit sense window", c =>
                {
                    var btn = FindButtons(UIType.FateFeature, GameTool.LS("sense_button")).FirstOrDefault();
                    if (c.Assert(btn != null, "'Spirit Sense' button on the cultivation screen")) btn.Item.onClick.Invoke();
                    c.Assert(g.ui.HasUI(UIType.TextInfoLong), "spirit sense window opened");
                }, 1500)
                .Step("scan", c =>
                {
                    var btn = FindButtons(UIType.TextInfoLong, string.Format(GameTool.LS("sense_scan_button"), SpiritSenseEvent.ScanRange(5), SpiritSenseEvent.SCAN_EXP)).FirstOrDefault();
                    if (!c.Assert(btn != null, "the scan button")) return;
                    btn.Item.onClick.Invoke();
                    var r = SpiritSenseEvent.LastScan;
                    if (c.Assert(r != null, "a scan made"))
                    {
                        c.Info($"{r.Units} around, {r.Hostile} hostile, strongest {r.Strongest} g{r.StrongestGrade}, herbs {r.Herbs}, ores {r.Ores}; places: {string.Join(", ", r.Places.Select(x => $"{x.Name} {x.Distance}"))}");
                        c.AssertEqual(r.Units, r.UnitList.Count, "every cultivator around listed");
                        c.Assert(r.Places.Count > 0, "places listed (the nearest spot at least)");
                        c.Assert(g.ui.HasUI(UIType.TextInfoLong), "the scan window");
                    }
                    c.AssertEqual(GameHelper.GetGameTotalMonth(), SpiritSenseEvent.Get(Player).LastScanMonth, "once a month");
                }, 1500)
                .Step("a cultivator from the scan", c =>
                {
                    var r = SpiritSenseEvent.LastScan;
                    var npc = r?.UnitList?.FirstOrDefault();
                    if (npc == null) { c.Info("nobody around"); return; }
                    var btn = FindButtons(UIType.TextInfoLong, npc.GetName()).FirstOrDefault();
                    if (c.Assert(btn != null, $"the name button of {npc.GetName()}")) btn.Item.onClick.Invoke();
                    c.Assert(g.ui.HasUI(UIType.NPCInfo), "the NPC info opened");
                }, 1500)
                .Step("Dao heart window", c =>
                {
                    CloseAll(UIType.NPCInfo, UIType.TextInfoLong, UIType.TextInfo);
                    var btn = FindButtons(UIType.FateFeature, GameTool.LS("heart_button")).FirstOrDefault();
                    if (c.Assert(btn != null, "'Dao Heart' button on the cultivation screen")) btn.Item.onClick.Invoke();
                    c.Assert(g.ui.HasUI(UIType.TextInfoLong), "Dao heart window opened");
                }, 1500)
                .Step("restore", c =>
                {
                    CloseAll(UIType.TextInfoLong, UIType.TextInfo, UIType.FateFeature);
                    var id = Player.GetUnitId();
                    DaoHeartEvent.Instance.Hearts.Remove(id);
                    SpiritSenseEvent.Instance.Senses.Remove(id);
                    if (c.Bag["heart"] != null) DaoHeartEvent.Instance.Hearts[id] = Newtonsoft.Json.JsonConvert.DeserializeObject<DaoHeartEvent.HeartData>((string)c.Bag["heart"]);
                    if (c.Bag["sense"] != null) SpiritSenseEvent.Instance.Senses[id] = Newtonsoft.Json.JsonConvert.DeserializeObject<SpiritSenseEvent.SenseData>((string)c.Bag["sense"]);
                    var keep = c.Bag["bounties"] as System.Collections.Generic.List<string>;
                    if (keep != null) WantedEvent.Instance.Bounties.RemoveAll(x => !keep.Contains(x.Id));
                    Player.ClearExp();
                    Player.AddExp((int)c.Bag["exp"]);
                    UnitModifyEvent.Instance.CachedValues.Clear();
                });
        }
    }
}
