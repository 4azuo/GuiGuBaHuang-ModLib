using MOD_nE7UL2.Mod;

public static class SkillHelper
{
    public static int GetSkillMpCost(DataProps.MartialData md)
    {
        if (md == null)
            return 0;
        var sd = md.data.To<DataProps.PropsSkillData>();
        var grade = md.data.propsInfoBase.grade;
        var mpCostKey = sd.skillAttackItem.mpCost;
        var skillValue = g.conf.battleSkillValue.GetItem(mpCostKey);
        if (skillValue != null)
            return skillValue.GetValue($"value{grade}").Parse<int>();
        return 0;
    }

    public static int GetStepMpCost(DataProps.MartialData md)
    {
        if (md == null)
            return 0;
        var sd = md.data.To<DataProps.PropsStepData>();
        var grade = md.data.propsInfoBase.grade;
        var mpCostKey = sd.stepBaseItem.mpCost;
        var skillValue = g.conf.battleSkillValue.GetItem(mpCostKey);
        if (skillValue != null)
            return skillValue.GetValue($"value{grade}").Parse<int>();
        return 0;
    }
}