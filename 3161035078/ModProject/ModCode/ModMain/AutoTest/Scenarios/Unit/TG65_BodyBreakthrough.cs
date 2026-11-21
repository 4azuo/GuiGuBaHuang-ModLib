using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Unit
{
    /// <summary>
    /// TG-65 (BodyCulEvent with a real breakthrough): a body refiner of the Jade Marrow realm (realm 3) fills the exp of his
    /// phase (the body absorbs a part: x1.3 exp needed) and breaks through with the game's own action
    /// (UnitActionRoleUpGrade: attributes and life of the new phase); the new phase gives the body bonuses: 15 % of the base max HP gained, 9 % of
    /// the defense, 3 % of the attack. Grade, exp and body data restored.
    /// </summary>
    public class TG65_BodyBreakthrough : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-65";

        private const int LEVEL = 7; // Jade Marrow - Early (realm 3)

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-65", Feature = "BodyCulEvent", Title = "Body refining: bonuses of a real breakthrough", Overlay = false }
                .Step("a body refiner with full exp", c =>
                {
                    if (!c.Assert(BodyCulEvent.Instance != null, "BodyCulEvent loaded")) return;
                    var id = Player.GetUnitId();
                    c.Bag["grade"] = Player.GetProperty<int>(UnitPropertyEnum.GradeID);
                    c.Bag["exp"] = Player.GetExp();
                    c.Bag["body"] = BodyCulEvent.Instance.Bodies.TryGetValue(id, out var old) ? Newtonsoft.Json.JsonConvert.SerializeObject(old) : null;
                    BodyCulEvent.Instance.Bodies.Remove(id);
                    var d = BodyCulEvent.Start(Player);
                    d.Level = LEVEL;
                    BodyCulEvent.Track(Player);
                    // the body absorbs a part of every gain: fill the phase gain by gain (as the game does every second)
                    for (var i = 0; i < 200 && !Player.IsFullExp(); i++)
                    {
                        Player.AddExp(100000);
                        BodyCulEvent.Track(Player);
                    }
                    c.Assert(Player.IsFullExp(), "player exp is full");
                    c.Assert(d.Absorbed > 0, $"the body absorbed {d.Absorbed:#,##0} exp on the way (x{BodyCulEvent.ExpRate(LEVEL):0.0} needed)");
                    var next = Player.GetNextPhaseConf();
                    if (!c.Assert(next != null, "a next phase exists")) return;
                    c.Bag["next"] = next.id;
                    c.Bag["hp"] = Player.GetDynProperty(UnitDynPropertyEnum.HpMax).baseValue;
                    c.Bag["def"] = Player.GetDynProperty(UnitDynPropertyEnum.Defense).baseValue;
                    c.Bag["atk"] = Player.GetDynProperty(UnitDynPropertyEnum.Attack).baseValue;
                    c.Bag["bonus"] = new[] { d.BonusHp, d.BonusDef, d.BonusAtk };
                })
                .Step("break through", c =>
                {
                    // the game's own breakthrough (attributes, life): UnitActionRoleUpGrade, forced to succeed
                    var act = new UnitActionRoleUpGrade((int)c.Bag["next"], new string[0], new string[0]) { upgradePhaseForceSuccess = true };
                    var rs = Player.CreateAction(act, false);
                    c.Info($"UnitActionRoleUpGrade: {rs}");
                    c.AssertEqual((int)c.Bag["next"], Player.GetProperty<int>(UnitPropertyEnum.GradeID), "player moved to the next phase");
                    var d = BodyCulEvent.Get(Player);
                    var b0 = (int[])c.Bag["bonus"];
                    BodyCulEvent.Track(Player);
                    var dHp = Player.GetDynProperty(UnitDynPropertyEnum.HpMax).baseValue - (int)c.Bag["hp"];
                    var dDef = Player.GetDynProperty(UnitDynPropertyEnum.Defense).baseValue - (int)c.Bag["def"];
                    var dAtk = Player.GetDynProperty(UnitDynPropertyEnum.Attack).baseValue - (int)c.Bag["atk"];
                    if (!c.Assert(dHp > 0, $"the breakthrough raised the base max HP (+{dHp}, defense +{dDef}, attack +{dAtk})")) return;
                    c.AssertEqual(BodyCulEvent.BreakBonus(dHp, BodyCulEvent.BREAK_HP_PER_REALM, LEVEL), d.BonusHp - b0[0], "max HP bonus: 15 % of the gain (realm 3 x 5 %)");
                    c.AssertEqual(BodyCulEvent.BreakBonus(dDef, BodyCulEvent.BREAK_DEF_PER_REALM, LEVEL), d.BonusDef - b0[1], "defense bonus: 9 % of the gain");
                    c.AssertEqual(BodyCulEvent.BreakBonus(dAtk, BodyCulEvent.BREAK_ATK_PER_REALM, LEVEL), d.BonusAtk - b0[2], "attack bonus: 3 % of the gain");
                    UnitModifyEvent.Instance.CachedValues.Clear();
                    var hpBase = Player.GetDynProperty(UnitDynPropertyEnum.HpMax).baseValue;
                    c.Assert(UnitModifyEvent.GetAdjustMaxHp(Player) >= BodyCulEvent.AdjHp(Player, hpBase), $"the bonuses reach the unit (max HP +{BodyCulEvent.AdjHp(Player, hpBase)})");
                    c.Assert(TaoistLogEvent.Instance.Entries.Any(x => x.Key == "log_body_breakbonus"), "Taoist log: breakthrough bonus");
                }, 1000)
                .Step("restore", c =>
                {
                    Player.SetProperty<int>(UnitPropertyEnum.GradeID, (int)c.Bag["grade"]);
                    Player.ClearExp();
                    Player.AddExp((int)c.Bag["exp"]);
                    var id = Player.GetUnitId();
                    BodyCulEvent.Instance.Bodies.Remove(id);
                    if (c.Bag["body"] != null)
                        BodyCulEvent.Instance.Bodies[id] = Newtonsoft.Json.JsonConvert.DeserializeObject<BodyCulEvent.BodyData>((string)c.Bag["body"]);
                    UnitModifyEvent.Instance.CachedValues.Clear();
                    c.AssertEqual((int)c.Bag["grade"], Player.GetProperty<int>(UnitPropertyEnum.GradeID), "grade restored");
                });
        }
    }
}
