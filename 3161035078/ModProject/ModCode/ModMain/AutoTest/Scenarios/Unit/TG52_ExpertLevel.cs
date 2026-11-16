using ModLib.Enum;
using ModLib.Helper;
using System.Collections.Generic;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Unit
{
    /// <summary>
    /// TG-52 (ExpertEvent/UnitModifyHelper): expertise on a real equipped skill of the player. Using it adds
    /// exp (insight and grade scaled), enough exp raises the expertise level, and an ability with expertise
    /// gives Atk/Def/Hp/Mp bonuses. The exp of the skill is restored afterwards. Replaces the manual TM-33.
    /// </summary>
    public class TG52_ExpertLevel : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-52";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            DataProps.MartialData martial = null;
            return new Scenario { Id = "TG-52", Feature = "ExpertEvent", Title = "Expertise level of an equipped skill" }
                .Step("equipped skill", c =>
                {
                    var all = new List<DataProps.MartialData>();
                    foreach (var m in Player.GetActionMartials()) all.Add(m.Value.data.To<DataProps.MartialData>());
                    martial = all
                        .Where(x => x != null && ExpertEvent.Configs.SkillExpRatios.ContainsKey(x.martialType))
                        .OrderBy(x => x.martialType == MartialType.Ability ? 0 : 1)
                        .FirstOrDefault();
                    if (!c.Assert(martial != null, "the player has an equipped skill")) return;
                    c.Bag["exp"] = ExpertEvent.GetExpertExp(martial.data.soleID);
                    c.Info($"{martial.martialType} {martial.martialInfo.name}: grade {martial.martialInfo.grade}, level {martial.martialInfo.level}, expertise exp {c.Bag["exp"]}");
                })
                .Step("use the skill", c =>
                {
                    if (martial == null) return;
                    var id = martial.data.soleID;
                    var before = ExpertEvent.GetExpertExp(id);
                    var ratio = ExpertEvent.Configs.SkillExpRatios[martial.martialType];
                    ExpertEvent.AddExpertExp(Player, id, ratio);
                    var insight = Player.GetDynProperty(UnitDynPropertyEnum.Talent).value / 100f;
                    var min = (int)(ratio * 100f * Player.GetGradeLvl() * 1f * insight) - 1;
                    var max = (int)(ratio * 100f * Player.GetGradeLvl() * 2f * insight) + 1;
                    c.AssertBetween(ExpertEvent.GetExpertExp(id) - before, min, max, "exp gained by one use");
                })
                .Step("level up", c =>
                {
                    if (martial == null) return;
                    var id = martial.data.soleID;
                    int grade = martial.martialInfo.grade, level = martial.martialInfo.level;
                    var lvl = ExpertEvent.GetExpertLvl(id, grade, level);
                    ExpertEvent.GetExpertExps()[id] = ExpertEvent.GetExpertNeedExp(lvl + 1, grade, level);
                    c.AssertEqual(lvl + 1, ExpertEvent.GetExpertLvl(id, grade, level), $"expertise level {lvl} -> {lvl + 1} with {ExpertEvent.GetExpertNeedExp(lvl + 1, grade, level)} exp");
                    var atk = Player.GetDynProperty(UnitDynPropertyEnum.Attack).baseValue;
                    var bonus = UnitModifyHelper.GetAbilityExpertAtk(atk, lvl + 1, grade, level);
                    c.Assert(bonus >= 0, $"ability expertise Atk bonus at level {lvl + 1}: +{bonus}");
                    if (martial.martialType == MartialType.Ability)
                        c.Assert(bonus > UnitModifyHelper.GetAbilityExpertAtk(atk, 0, grade, level), "bonus grows with the expertise level");
                })
                .Step("restore", c =>
                {
                    if (martial == null) return;
                    ExpertEvent.GetExpertExps()[martial.data.soleID] = (int)c.Bag["exp"];
                    c.AssertEqual((int)c.Bag["exp"], ExpertEvent.GetExpertExp(martial.data.soleID), "expertise exp restored");
                });
        }
    }
}
