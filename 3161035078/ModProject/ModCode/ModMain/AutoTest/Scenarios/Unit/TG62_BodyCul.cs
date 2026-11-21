using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Unit
{
    /// <summary>
    /// TG-62 (BodyCulEvent): the player takes up body refining from the cultivation screen; exp from hits, layers going up
    /// by themselves, the bottleneck at the end of a realm, the grade gate, a failed breakthrough (injury), a successful one,
    /// the medicinal bath (once a month); the bonuses reach the unit (max HP, defense, fist). Body data and money restored.
    /// </summary>
    public class TG62_BodyCul : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-62";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            BodyCulEvent.BodyData saved = null;
            var money = 0;
            return new Scenario { Id = "TG-62", Feature = "BodyCulEvent", Title = "Body refining: realms, breakthrough, bath, bonuses", Overlay = false }
                .Step("open cultivation", c =>
                {
                    if (!c.Assert(BodyCulEvent.Instance != null, "BodyCulEvent loaded")) return;
                    var id = Player.GetUnitId();
                    if (BodyCulEvent.Instance.Bodies.TryGetValue(id, out var d))
                        saved = new BodyCulEvent.BodyData { Level = d.Level, Exp = d.Exp, InjuryMonths = d.InjuryMonths, LastBathMonth = d.LastBathMonth };
                    BodyCulEvent.Instance.Bodies.Remove(id);
                    money = Player.GetUnitMoney();
                    var ui = g.ui.GetUI<UIMapMain>(UIType.MapMain);
                    (ui.playerInfo.btnFateFeature ?? ui.playerInfo.btnFateFeature_En).onClick.Invoke();
                }, 1000)
                .Step("body window (not started)", c =>
                {
                    var btn = FindButtons(UIType.FateFeature, GameTool.LS("body_button")).FirstOrDefault();
                    if (!c.Assert(btn != null, "'Body Refining' button on the cultivation screen")) return;
                    btn.Item.onClick.Invoke();
                    c.Assert(g.ui.HasUI(UIType.TextInfoLong), "body refining window opened");
                    c.Assert(FindButtons(UIType.TextInfoLong, GameTool.LS("body_start")).Any(), "'Take up body refining' button");
                }, 1000)
                .Step("take up", c =>
                {
                    BodyCulEvent.Start(Player);
                    c.AssertEqual(1, BodyCulEvent.GetLevel(Player), "level 1 (Copper Skin - Early)");
                    c.AssertEqual(0.9f, BodyCulEvent.QiRate(Player), "Qi gain of a Copper Skin body refiner");
                    BodyCulEvent.OpenWindow();
                    c.Assert(ScreenTexts().Any(x => x.Contains(BodyCulEvent.RealmName(1))), "the realm is shown");
                }, 1000)
                .Step("cultivation exp absorbed", c =>
                {
                    c.Bag["exp0"] = Player.GetExp();
                    var d = BodyCulEvent.Get(Player);
                    BodyCulEvent.Track(Player);
                    // room in the phase for the gain (the exp of the phase is capped at its max)
                    var room = Player.GetMaxExpCurrentPhase() - Player.GetExp();
                    if (room < 110)
                    {
                        c.Skip($"exp of the phase almost full ({room} left)");
                        return;
                    }
                    var before = Player.GetExp();
                    var gain = (int)Math.Min(1100, room);
                    Player.AddExp(gain);
                    BodyCulEvent.Track(Player);
                    var kept = Player.GetExp() - before;
                    c.AssertEqual(BodyCulEvent.KeptExp(gain, 1), kept, $"x1.10 exp needed: {gain} gained, {kept} kept");
                    c.AssertEqual((long)(gain - kept), d.Absorbed, "absorbed by the body");
                    Player.SetProperty<int>(UnitPropertyEnum.Exp, (int)c.Bag["exp0"]);
                    BodyCulEvent.Track(Player);
                })
                .Step("exp and layers", c =>
                {
                    var d = BodyCulEvent.Get(Player);
                    c.AssertEqual(50f, BodyCulEvent.HitExp(500, 1000), "a hit of half the max HP = 50 exp");
                    BodyCulEvent.AddExp(Player, BodyCulEvent.HitExp(5000, 1000));
                    c.AssertEqual(100L, d.Exp, "a hit counts at most one max HP (100 exp)");
                    var up = BodyCulEvent.AddExp(Player, BodyCulEvent.NeedExp(1) + BodyCulEvent.NeedExp(2) + 1000000);
                    c.AssertEqual(2, up, "two layers up, then the bottleneck");
                    c.AssertEqual(3, d.Level, "Copper Skin - Late");
                    c.Assert(BodyCulEvent.IsBottleneck(d), "bottleneck at the end of the realm");
                    c.AssertEqual(BodyCulEvent.NeedExp(3), d.Exp, "exp stops at the bottleneck");
                    // at the bottleneck a bath still helps: the exp over the level becomes essence
                    var essence = d.Essence;
                    BodyCulEvent.AddExp(Player, 500);
                    c.AssertEqual(essence + 500, d.Essence, "at the bottleneck the exp becomes essence");
                    c.Assert(BodyCulEvent.CanBath(Player), "baths go on at the bottleneck");
                    UnitModifyEvent.Instance.CachedValues.Clear();
                    var hpBase = Player.GetDynProperty(UnitDynPropertyEnum.HpMax).baseValue;
                    c.Assert(BodyCulEvent.AdjHp(Player, hpBase) > 0, $"max HP bonus {BodyCulEvent.AdjHp(Player, hpBase)}");
                    var steps = (int)BodyCulEvent.EssenceSteps(d.Essence);
                    c.Assert(d.Essence > 0, $"the exp over the bottleneck became essence: {d.Essence:#,##0}");
                    c.AssertEqual(12 + steps, BodyCulEvent.AdjBasis(Player), $"fist / palm / finger +12 at level 3, +{steps} of essence");
                    c.Assert(UnitModifyEvent.GetAdjustFist(Player) >= 12, $"fist bonus reaches the unit ({UnitModifyEvent.GetAdjustFist(Player)})");
                    c.Assert(UnitModifyEvent.GetAdjustMaxHp(Player) >= BodyCulEvent.AdjHp(Player, hpBase), "max HP bonus reaches the unit");
                })
                .Step("breakthrough", c =>
                {
                    var d = BodyCulEvent.Get(Player);
                    if (Player.GetGradeLvl() < BodyCulEvent.RealmGrade(4))
                        c.AssertEqual(BodyCulEvent.BreakResult.GradeTooLow, BodyCulEvent.Breakthrough(Player, 0f), "Iron Bone needs grade 3");
                    else
                        c.Info($"grade {Player.GetGradeLvl()}: no grade gate to test");
                    // the gate does not stop the rest of the test: a unit of a high grade
                    var grade = Player.GetProperty<int>(UnitPropertyEnum.GradeID);
                    c.Bag["grade"] = grade;
                    var high = g.conf.roleGrade._allConfList.ToArray().Where(x => x.grade >= BodyCulEvent.RealmGrade(4)).OrderBy(x => x.id).FirstOrDefault();
                    if (Player.GetGradeLvl() < BodyCulEvent.RealmGrade(4) && high != null)
                        Player.SetProperty<int>(UnitPropertyEnum.GradeID, high.id);
                    // the player pays natural treasures and herbs for every try
                    var breakNeeds = BodyCulEvent.BreakNeeds(d.Level);
                    if (!BodyCulEvent.HasAll(Player, breakNeeds))
                        c.AssertEqual(BodyCulEvent.BreakResult.NoMaterials, BodyCulEvent.Breakthrough(Player, 0f), "no breakthrough without the materials");
                    foreach (var n in breakNeeds) Player.AddUnitProp(BodyCulEvent.SampleId(Player, n), n.Count * 5);
                    var herbs = BodyCulEvent.Have(Player, breakNeeds[1]);
                    var life = Player.GetProperty<int>(UnitPropertyEnum.Life);
                    c.AssertEqual(BodyCulEvent.BreakResult.Failure, BodyCulEvent.Breakthrough(Player, 99.9f, 2), "a bad roll fails (dose x2)");
                    c.Assert(BodyCulEvent.LastLifeLost > 0 && Player.GetProperty<int>(UnitPropertyEnum.Life) == life - BodyCulEvent.LastLifeLost,
                        $"a failed breakthrough cost {BodyCulEvent.LastLifeLost} months of lifespan (about {BodyCulEvent.BreakLifeLoss(3, 2, BodyCulEvent.Toughness(Player))})");
                    Player.SetProperty<int>(UnitPropertyEnum.Life, life);
                    c.Assert(BodyCulEvent.Have(Player, breakNeeds[1]) <= herbs - breakNeeds[1].Count, $"{breakNeeds[1].Count} herbs (or pills) used, even by a failure: {herbs} -> {BodyCulEvent.Have(Player, breakNeeds[1])}");
                    c.AssertEqual(BodyCulEvent.NeedExp(3) / 2, d.Exp, "half of the exp lost");
                    c.Assert(BodyCulEvent.IsInjured(Player), "the body is injured");
                    c.AssertEqual(6 + (int)BodyCulEvent.EssenceSteps(d.Essence), BodyCulEvent.AdjBasis(Player), "injured: the bonuses of the levels halved (not the essence)");
                    BodyCulEvent.AddExp(Player, BodyCulEvent.NeedExp(3));
                    d.InjuryMonths = 0;
                    c.AssertEqual(BodyCulEvent.BreakResult.Success, BodyCulEvent.Breakthrough(Player, 0f, 3), "a good roll breaks through (dose x3)");
                    c.Assert(System.Math.Abs(d.Temper - 0.5) < 0.0001, $"a x3 breakthrough tempers the body for good: +50 % of a realm's bonuses ({d.Temper})");
                    c.AssertEqual(4, d.Level, "Iron Bone - Early");
                    var keys = TaoistLogEvent.Instance.Entries.Select(x => x.Key).ToList();
                    c.Assert(new[] { "log_body_start", "log_body_layer", "log_body_fail", "log_body_realm" }.All(keys.Contains), "Taoist log entries");
                    Player.SetProperty<int>(UnitPropertyEnum.GradeID, grade);
                })
                .Step("medicinal bath", c =>
                {
                    var d = BodyCulEvent.Get(Player);
                    // natural treasures (item class 508) instead of spirit stones: Iron Bone needs 2 of item level 4+
                    var need = BodyCulEvent.BathNeed(d.Level);
                    c.AssertEqual(4, need.count, "Iron Bone: 4 natural treasures a bath");
                    var id = BodyCulEvent.BathTreasureId(Player);
                    if (!c.Assert(id != 0, $"a natural treasure for the bath: {id}")) return;
                    var have = BodyCulEvent.BathTreasures(Player).Sum(x => x.propsCount);
                    if (have < need.count)
                        c.Assert(!BodyCulEvent.Bath(Player), $"no bath without the treasures ({have} in the bag)");
                    Player.AddUnitProp(id, need.count);
                    var herbNeed = BodyCulEvent.BathNeeds(d.Level)[1];
                    Player.AddUnitProp(BodyCulEvent.SampleId(Player, herbNeed), herbNeed.Count);
                    c.AssertEqual(100, BodyCulEvent.HerbCounts(herbNeed).low, "and 100 low herbs (40 middle, 20 high)");
                    var before = BodyCulEvent.BathTreasures(Player).Sum(x => x.propsCount);
                    var exp = d.Exp;
                    c.Assert(BodyCulEvent.Bath(Player), "bath taken");
                    c.AssertEqual(before - need.count, BodyCulEvent.BathTreasures(Player).Sum(x => x.propsCount), "4 natural treasures used from the bag");
                    c.AssertEqual(exp + BodyCulEvent.BathExp(4), d.Exp, "bath exp");
                    c.Assert(!BodyCulEvent.Bath(Player), "only one bath a month");
                    BodyCulEvent.OpenWindow();
                    var texts = ScreenTexts();
                    c.Assert(texts.Any(x => x.Contains(BodyCulEvent.RealmName(4))), "the window shows Iron Bone");
                    var herbsPrefix = GameTool.LS("body_need_herbs").Split('{')[0];
                    c.Assert(texts.Count(x => x.StartsWith(herbsPrefix)) >= 3, "the herbs of every dose are shown");
                }, 1000)
                .Step("body essence", c =>
                {
                    // the top level: the exp gained becomes essence, the stats go on growing
                    var d = BodyCulEvent.Get(Player);
                    d.Level = BodyCulEvent.MAX_LEVEL;
                    d.Exp = BodyCulEvent.NeedExp(BodyCulEvent.MAX_LEVEL);
                    d.Essence = 0;
                    d.InjuryMonths = 0;
                    var basis = BodyCulEvent.AdjBasis(Player);
                    var hpBase = Player.GetDynProperty(UnitDynPropertyEnum.HpMax).baseValue;
                    var hp = BodyCulEvent.AdjHp(Player, hpBase);
                    c.AssertEqual(0, BodyCulEvent.AddExp(Player, BodyCulEvent.EssenceStep * 2), "no level above the top");
                    c.AssertEqual(BodyCulEvent.EssenceStep * 2, d.Essence, "the exp became essence");
                    c.AssertEqual(basis + 2, BodyCulEvent.AdjBasis(Player), "fist / palm / finger +2");
                    c.Assert(BodyCulEvent.AdjHp(Player, hpBase) > hp, $"max HP +{BodyCulEvent.AdjHp(Player, hpBase) - hp}");
                    d.LastBathMonth = -1;
                    c.Assert(BodyCulEvent.CanBath(Player), "baths go on at the top level");
                    BodyCulEvent.OpenWindow();
                    c.Assert(ScreenTexts().Any(x => x.StartsWith(string.Format(GameTool.LS("body_essence"), d.Essence, 0, 0, 0).Split('(')[0])), "the window shows the essence");
                }, 1000)
                .Step("restore", c =>
                {
                    CloseAll(UIType.TextInfoLong, UIType.FateFeature);
                    var id = Player.GetUnitId();
                    BodyCulEvent.Instance.Bodies.Remove(id);
                    if (saved != null) BodyCulEvent.Instance.Bodies[id] = saved;
                    Player.SetUnitMoney(money);
                    UnitModifyEvent.Instance.CachedValues.Clear();
                    c.AssertEqual(saved?.Level ?? 0, BodyCulEvent.GetLevel(Player), "body data restored");
                });
        }
    }
}
