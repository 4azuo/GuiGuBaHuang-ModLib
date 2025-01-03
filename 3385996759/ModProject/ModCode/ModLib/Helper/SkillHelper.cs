﻿using System;

public static class SkillHelper
{
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