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
    }
}