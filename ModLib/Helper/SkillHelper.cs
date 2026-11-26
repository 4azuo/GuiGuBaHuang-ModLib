using System.Collections.Generic;
using System.Linq;
using ModLib.Attributes;
using System;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for working with skills and abilities.
    /// Provides utilities for skill cost calculation and skill data retrieval.
    /// </summary>
    [ActionCat("Skill")]
    public static class SkillHelper
    {
        /// <summary>
        /// Gets the MP cost for a skill.
        /// </summary>
        /// <param name="props">Skill prop</param>
        /// <returns>MP cost</returns>
        public static int GetSkillMpCost(this DataProps.PropsData props)
        {
            if (props == null)
                return 0;
            var sd = props.To<DataProps.PropsSkillData>();
            var grade = props.propsInfoBase.grade;
            var mpCostKey = sd.skillAttackItem.mpCost;
            var skillValue = g.conf.battleSkillValue.GetItem(mpCostKey);
            if (skillValue != null)
                return skillValue.GetValue($"value{grade}").Parse<int>();
            return 0;
        }

        /// <summary>
        /// Gets the MP cost for a movement skill (step).
        /// </summary>
        /// <param name="props">Step prop</param>
        /// <returns>MP cost</returns>
        public static int GetStepMpCost(this DataProps.PropsData props)
        {
            if (props == null)
                return 0;
            var sd = props.To<DataProps.PropsStepData>();
            var grade = props.propsInfoBase.grade;
            var mpCostKey = sd.stepBaseItem.mpCost;
            var skillValue = g.conf.battleSkillValue.GetItem(mpCostKey);
            if (skillValue != null)
                return skillValue.GetValue($"value{grade}").Parse<int>();
            return 0;
        }

        /// <summary>
        /// Gets the currently casting skill from hit data.
        /// </summary>
        /// <param name="hitData">Hit data</param>
        /// <returns>Tuple of martial type and skill prop, or null</returns>
        public static Tuple<MartialType, DataProps.PropsData> GetCastingSkill(this MartialTool.HitData hitData)
        {
            var atkUnit = hitData?.attackUnit;
            if (atkUnit == null)
                return null;
            if (atkUnit.step != null && atkUnit.step.isUseStep)
            {
                return Tuple.Create(MartialType.Step, atkUnit.step?.data?.stepData?.data);
            }
            var skill = hitData?.skillBase?.TryCast<SkillAttack>();
            if (skill != null && skill.skillData != null)
            {
                return Tuple.Create(skill.skillData.martialType, skill.skillData.data);
            }
            return null;
        }

        /// <summary>Two martial arts of the same kind and base (one is a copy of the other, whatever their prefixes).</summary>
        public static bool IsSameMartial(DataProps.PropsData a, DataProps.PropsData b)
        {
            var x = a?.To<DataProps.MartialData>();
            var y = b?.To<DataProps.MartialData>();
            return x != null && y != null && x.martialType == y.martialType && x.baseID == y.baseID;
        }

        private static readonly Random _random = new Random();

        /// <summary>
        /// A new martial art as the game makes its rewards: values [baseID, grade, prefix ids..., -1, prefix values...],
        /// the prefixes drawn by the game for the quality <paramref name="level"/> (RewardTool.RandomPrefixLevel).
        /// </summary>
        public static DataProps.PropsData NewMartial(MartialType type, int baseId, int grade, int level)
        {
            var seed = new RandomSeedData();
            lock (_random) seed.seed = _random.Next(1, int.MaxValue);
            var prefixes = RewardTool.RandomPrefixLevel(type, 0, baseId, grade, level, seed);
            var values = new List<int> { baseId, grade };
            if (prefixes != null)
                values.AddRange(prefixes.ToArray());
            return DataProps.PropsData.NewMartial(type, values.ToArray());
        }

        /// <summary>
        /// Ids of the martial arts of a type a player can get at the realm <paramref name="grade"/>: BattleSkillAttack with a
        /// class (type 1/2 attack - SkillLeft -, 3 spell - SkillRight -, 4 ultimate), BattleStepBase (step), BattleAbilityBase
        /// with a class (heart art).
        /// </summary>
        public static List<int> GetMartialBaseIds(MartialType type, int grade)
        {
            if (type == MartialType.Ability)
                return g.conf.battleAbilityBase._allConfList.ToArray()
                    .Where(x => x.name != "0" && x.className != null && x.className.Length > 0 && x.className[0] > 0)
                    .Where(x => x.gradeMin <= grade && grade <= x.gradeMax)
                    .Select(x => x.id).ToList();
            if (type == MartialType.Step)
                return g.conf.battleStepBase._allConfList.ToArray()
                    .Where(x => x.gradeMin <= grade && grade <= x.gradeMax)
                    .Select(x => x.id).ToList();
            int[] attackTypes;
            switch (type)
            {
                case MartialType.SkillLeft: attackTypes = new[] { 1, 2 }; break;
                case MartialType.SkillRight: attackTypes = new[] { 3 }; break;
                case MartialType.Ultimate: attackTypes = new[] { 4 }; break;
                default: return new List<int>();
            }
            return g.conf.battleSkillAttack._allConfList.ToArray()
                .Where(x => x.className != "0" && x.name != "0" && attackTypes.Contains(x.type))
                .Where(x => x.gradeMin <= grade && grade <= x.gradeMax)
                .Select(x => x.id).ToList();
        }
    }
}