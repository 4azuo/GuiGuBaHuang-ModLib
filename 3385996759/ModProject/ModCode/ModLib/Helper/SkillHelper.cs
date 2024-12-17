public static class SkillHelper
{
    public static int GetSkillMpCost(DataProps.PropsData props)
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

    public static int GetStepMpCost(DataProps.PropsData props)
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
}