using MOD_nE7UL2;
using MOD_nE7UL2.Mod;
using ModLib.Enum;
using System;
using static MOD_nE7UL2.Object.ModStts;
using ModLib.Helper;

/// <summary>
/// Formulas of the bonuses Taoist adds to units (artifact, expertise, refine, ability, Qi...).
/// </summary>
public static class UnitModifyHelper
{
    public static _ExpertConfigs ExpertConfigs => ModMain.ModObj.ModSettings.ExpertConfigs;

    #region Artifact
    /// <summary>
    /// Formula: artifact basic adj atk.
    /// </summary>
    public static int GetArtifactBasicAdjAtk(int baseValue, DataProps.PropsData props, DataProps.PropsArtifact artifact)
    {
        if (props == null)
            return 0;
        var aconf = props.propsItem.IsArtifact();
        var r = 0.01f + (0.001f * Math.Pow(2, artifact.level)) + (0.02f * artifact.grade);
        var r1 = (4.00f + (0.006f * Math.Pow(3, artifact.level)) + (1.00f * artifact.grade)) / 100.0f;
        return (r * baseValue + r1 * aconf.atk).Parse<int>();
    }

    /// <summary>
    /// Formula: artifact basic adj def.
    /// </summary>
    public static int GetArtifactBasicAdjDef(int baseValue, DataProps.PropsData props, DataProps.PropsArtifact artifact)
    {
        if (props == null)
            return 0;
        var aconf = props.propsItem.IsArtifact();
        var r = 0.01f + (0.001f * Math.Pow(2, artifact.level)) + (0.02f * artifact.grade);
        var r1 = (3.00f + (0.005f * Math.Pow(3, artifact.level)) + (0.80f * artifact.grade)) / 100.0f;
        return (r * baseValue + r1 * aconf.def).Parse<int>();
    }

    /// <summary>
    /// Formula: artifact basic adj hp.
    /// </summary>
    public static int GetArtifactBasicAdjHp(int baseValue, DataProps.PropsData props, DataProps.PropsArtifact artifact)
    {
        if (props == null)
            return 0;
        var aconf = props.propsItem.IsArtifact();
        var r3 = 0.01f + 0.01f * artifact.level + 0.03f * artifact.grade;
        return (r3 * baseValue + artifact.level * artifact.grade * aconf.hp / 10).Parse<int>();
    }

    /// <summary>
    /// Formula: artifact expert atk.
    /// </summary>
    public static int GetArtifactExpertAtk(int inputValue, int expertLvl, int propsGrade, int propsLevel)
    {
        if (expertLvl <= 0)
            return 0;
        var r = 0.012f * propsGrade + 0.004f * propsLevel;
        var v = 24 * propsGrade + 6 * propsLevel;
        return (inputValue * expertLvl * r).Parse<int>() + v;
    }

    /// <summary>
    /// Formula: artifact expert def.
    /// </summary>
    public static int GetArtifactExpertDef(int inputValue, int expertLvl, int propsGrade, int propsLevel)
    {
        if (expertLvl <= 0)
            return 0;
        var r = 0.008f * propsGrade + 0.002f * propsLevel;
        var v = 6 * propsGrade + 1 * propsLevel;
        return (inputValue * expertLvl * r).Parse<int>() + v;
    }

    /// <summary>
    /// Formula: refine artifact adj atk.
    /// </summary>
    public static int GetRefineArtifactAdjAtk(DataProps.PropsData props, int refineLvl)
    {
        if (props == null)
            return 0;
        var aconf = props.propsItem.IsArtifact();
        var r = 0.004f * props.propsInfoBase.grade + 0.001f * props.propsInfoBase.level;
        return (refineLvl * r * aconf.atk).Parse<int>();
    }

    /// <summary>
    /// Formula: refine artifact adj def.
    /// </summary>
    public static int GetRefineArtifactAdjDef(DataProps.PropsData props, int refineLvl)
    {
        if (props == null)
            return 0;
        var aconf = props.propsItem.IsArtifact();
        var r = 0.001f * props.propsInfoBase.grade + 0.0002f * props.propsInfoBase.level;
        return (refineLvl * r * aconf.def).Parse<int>();
    }
    #endregion

    #region Outfit
    /// <summary>
    /// Formula: refine outfit adj hp.
    /// </summary>
    public static int GetRefineOutfitAdjHp(int baseValue, DataProps.PropsData props, int refineLvl)
    {
        if (props == null)
            return 0;
        var r = 0.04f * props.propsInfoBase.level + 0.001f * refineLvl;
        return (r * baseValue).Parse<int>();
    }

    /// <summary>
    /// Formula: refine outfit adj def.
    /// </summary>
    public static int GetRefineOutfitAdjDef(int baseValue, DataProps.PropsData props, int refineLvl)
    {
        if (props == null)
            return 0;
        var r = 0.01f * props.propsInfoBase.level + 0.0001f * refineLvl;
        return (r * baseValue).Parse<int>();
    }
    #endregion

    #region Mount
    /// <summary>
    /// Formula: refine mount adj mp.
    /// </summary>
    public static int GetRefineMountAdjMp(int baseValue, DataProps.PropsData props, int refineLvl)
    {
        if (props == null)
            return 0;
        var r = 0.04f * props.propsInfoBase.level + 0.001f * refineLvl;
        return (r * baseValue).Parse<int>();
    }
    #endregion

    #region Ring
    /// <summary>
    /// Formula: refine ring adj hp.
    /// </summary>
    public static int GetRefineRingAdjHp(int baseValue, DataProps.PropsData props, int refineLvl)
    {
        if (props == null)
            return 0;
        var r = 0.04f * props.propsInfoBase.grade + 0.007f * props.propsInfoBase.level + 0.001f * refineLvl;
        return (r * baseValue).Parse<int>();
    }
    #endregion

    #region Skill
    /// <summary>
    /// Formula: skill expert atk.
    /// </summary>
    public static int GetSkillExpertAtk(int inputValue, int expertLvl, int propsGrade, int propsLevel, MartialType mType)
    {
        if (expertLvl <= 0)
            return 0;
        var r = 0.010f * propsGrade + 0.002f * propsLevel;
        var v = 24 * propsGrade + 6 * propsLevel;
        return ((inputValue * expertLvl * r + v) * ExpertConfigs.SkillDmgRatios[mType]).Parse<int>();
    }

    /// <summary>
    /// Formula: skill expert mp cost.
    /// </summary>
    public static int GetSkillExpertMpCost(int inputValue, int expertLvl, int propsGrade, int propsLevel, int least)
    {
        if (expertLvl <= 0)
            return 0;
        var r = 0.06f * propsGrade + 0.01f * propsLevel;
        return Math.Max((inputValue * expertLvl * r).Parse<int>(), least);
    }
    #endregion

    #region Ability
    /// <summary>
    /// Formula: ability expert atk.
    /// </summary>
    public static int GetAbilityExpertAtk(int inputValue, int expertLvl, int propsGrade, int propsLevel)
    {
        if (expertLvl <= 0)
            return 0;
        var r = 0.005f * propsGrade + 0.001f * propsLevel;
        var v = 5 * propsGrade + 2 * propsLevel;
        return (inputValue * expertLvl * r).Parse<int>() + v;
    }

    /// <summary>
    /// Formula: ability expert def.
    /// </summary>
    public static int GetAbilityExpertDef(int inputValue, int expertLvl, int propsGrade, int propsLevel)
    {
        if (expertLvl <= 0)
            return 0;
        var r = 0.003f * propsGrade + 0.0006f * propsLevel;
        var v = 4 * propsGrade + 1 * propsLevel;
        return (inputValue * expertLvl * r).Parse<int>() + v;
    }

    /// <summary>
    /// Formula: ability expert hp.
    /// </summary>
    public static int GetAbilityExpertHp(int inputValue, int expertLvl, int propsGrade, int propsLevel)
    {
        if (expertLvl <= 0)
            return 0;
        var r = 0.010f * propsGrade + 0.002f * propsLevel;
        var v = 100 * propsGrade + 20 * propsLevel;
        return (inputValue * expertLvl * r).Parse<int>() + v;
    }

    /// <summary>
    /// Formula: ability expert mp.
    /// </summary>
    public static int GetAbilityExpertMp(int inputValue, int expertLvl, int propsGrade, int propsLevel)
    {
        if (expertLvl <= 0)
            return 0;
        var r = 0.010f * propsGrade + 0.002f * propsLevel;
        var v = 4 * propsGrade + 1 * propsLevel;
        return (inputValue * expertLvl * r).Parse<int>() + v;
    }

    /// <summary>
    /// Formula: ability expert sp.
    /// </summary>
    public static int GetAbilityExpertSp(int inputValue, int expertLvl, int propsGrade, int propsLevel)
    {
        if (expertLvl <= 0)
            return 0;
        var r = 0.003f * propsGrade + 0.0005f * propsLevel;
        var v = 4 * propsGrade + 1 * propsLevel;
        return (inputValue * expertLvl * r).Parse<int>() + v;
    }
    #endregion

    #region SkillStep
    /// <summary>
    /// Formula: step expert speed.
    /// </summary>
    public static int GetStepExpertSpeed(int expertLvl, int propsGrade, int propsLevel)
    {
        if (expertLvl <= 0)
            return 0;
        var v = 6 * propsGrade + 2 * propsLevel;
        return expertLvl * v;
    }

    /// <summary>
    /// Formula: step expert evade.
    /// </summary>
    public static float GetStepExpertEvade(int expertLvl, int propsGrade, int propsLevel)
    {
        if (expertLvl <= 0)
            return 0;
        var v = 0.10f * propsGrade + 0.03f * propsLevel;
        return expertLvl * v;
    }

    /// <summary>
    /// Formula: step expert mp cost.
    /// </summary>
    public static int GetStepExpertMpCost(int inputValue, int expertLvl, int propsGrade, int propsLevel, int least)
    {
        if (expertLvl <= 0)
            return 0;
        var r = 0.08f * propsGrade + 0.01f * propsLevel;
        return Math.Max((inputValue * expertLvl * r).Parse<int>(), least);
    }
    #endregion

    /// <summary>
    /// Formula: abi point adj hp.
    /// </summary>
    public static int GetAbiPointAdjHp(WorldUnitBase wunit)
    {
        if (wunit == null)
            return 0;
        return wunit.GetDynProperty(UnitDynPropertyEnum.AbilityPoint).value * 10 * wunit.GetGradeLvl();
    }

    /// <summary>
    /// Formula: abi point adj mp.
    /// </summary>
    public static int GetAbiPointAdjMp(WorldUnitBase wunit)
    {
        if (wunit == null)
            return 0;
        return wunit.GetDynProperty(UnitDynPropertyEnum.AbilityPoint).value;
    }

    /// <summary>
    /// Formula: martial adj hp.
    /// </summary>
    public static int GetMartialAdjHp(WorldUnitBase wunit)
    {
        if (wunit == null)
            return 0;
        return wunit.GetBasisPhysicSum() * wunit.GetGradeLvl();
    }

    /// <summary>
    /// Formula: spiritual adj mp.
    /// </summary>
    public static int GetSpiritualAdjMp(WorldUnitBase wunit)
    {
        if (wunit == null)
            return 0;
        return wunit.GetBasisMagicSum();
    }

    /// <summary>
    /// Formula: artisanship adj sp.
    /// </summary>
    public static int GetArtisanshipAdjSp(WorldUnitBase wunit)
    {
        if (wunit == null)
            return 0;
        return wunit.GetArtisanshipSum();
    }

    /// <summary>
    /// Formula: qi adj atk.
    /// </summary>
    public static int GetQiAdjAtk(WorldUnitBase wunit)
    {
        if (wunit == null || QiCulEvent.Instance == null) // Qi cultivation disabled (no Instance since BUG-16)
            return 0;
        var wunitId = wunit.GetUnitId();
        if (!QiCulEvent.Instance.Qi.ContainsKey(wunitId))
            QiCulEvent.Instance.Qi.Add(wunitId, 0);
        return Convert.ToInt32(QiCulEvent.Instance.Qi[wunitId] / (10000 * wunit.GetGradeLvl()));
    }

    /// <summary>
    /// Formula: qi adj hp.
    /// </summary>
    public static int GetQiAdjHp(WorldUnitBase wunit)
    {
        if (wunit == null || QiCulEvent.Instance == null) // Qi cultivation disabled (no Instance since BUG-16)
            return 0;
        var wunitId = wunit.GetUnitId();
        if (!QiCulEvent.Instance.Qi.ContainsKey(wunitId))
            QiCulEvent.Instance.Qi.Add(wunitId, 0);
        return Convert.ToInt32(QiCulEvent.Instance.Qi[wunitId] / (1000 * wunit.GetGradeLvl()));
    }

    /// <summary>
    /// Formula: qi adj mp.
    /// </summary>
    public static int GetQiAdjMp(WorldUnitBase wunit)
    {
        if (wunit == null || QiCulEvent.Instance == null) // Qi cultivation disabled (no Instance since BUG-16)
            return 0;
        var wunitId = wunit.GetUnitId();
        if (!QiCulEvent.Instance.Qi.ContainsKey(wunitId))
            QiCulEvent.Instance.Qi.Add(wunitId, 0);
        return Convert.ToInt32(QiCulEvent.Instance.Qi[wunitId] / (4000 * wunit.GetGradeLvl()));
    }
}