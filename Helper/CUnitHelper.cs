using ModLib.Mod;
using System.Collections.Generic;
using System.Linq;

public static class CUnitHelper
{
    public static bool IsHuman(this UnitCtrlBase cunit)
    {
        return cunit?.TryCast<UnitCtrlHuman>() != null;
    }

    public static bool IsMonster(this UnitCtrlBase cunit)
    {
        return cunit?.TryCast<UnitCtrlMonst>() != null;
    }

    public static bool IsMonsterHuman(this UnitCtrlBase cunit)
    {
        return cunit?.TryCast<UnitCtrlMonstHuman>() != null;
    }

    public static bool IsNPC(this UnitCtrlBase cunit)
    {
        return cunit?.TryCast<UnitCtrlHumanNPC>() != null;
    }

    public static bool IsPlayer(this UnitCtrlBase cunit)
    {
        return cunit?.TryCast<UnitCtrlPlayer>() != null;
    }

    public static bool IsWorldUnit(this UnitCtrlBase cunit)
    {
        return cunit.GetWorldUnit() != null && !cunit.IsSummoned();
    }

    public static bool IsSummoned(this UnitCtrlBase cunit)
    {
        return cunit?.monstSummon != null || (cunit?.IsPotmon() ?? false);
    }

    public static UnitCtrlBase GetSummoner(this UnitCtrlBase cunit)
    {
        if (cunit == null || !cunit.IsSummoned())
            return null;
        if (cunit.IsPotmon())
            return ModBattleEvent.PlayerUnit;
        return cunit?.monstSummon?.skillValueData?.data?.unitCtrlBase ??
            cunit?.monstSummon?.copySummonUnit ??
            cunit?.monstSummon?.summonUnit;
    }

    public static UnitCtrlBase GetOriginSummoner(this UnitCtrlBase cunit)
    {
        if (cunit == null || !cunit.IsSummoned())
            return null;
        if (cunit.IsPotmon())
            return ModBattleEvent.PlayerUnit;
        var summoner = cunit.GetSummoner();
        while (summoner != null && summoner.IsSummoned())
            summoner = summoner.GetSummoner();
        return summoner;
    }

    public static WorldUnitBase GetWorldUnit(this UnitCtrlBase cunit)
    {
        if (cunit == null)
            return null;
        return g.world.unit.GetUnit(cunit);
    }

    public static Il2CppSystem.Collections.Generic.List<UnitCtrlBase> FindNearCUnits(this UnitCtrlBase cunit, float radius)
    {
        return ModBattleEvent.SceneBattle.unit.GetRangeUnit(cunit.transform.position, radius);
    }

    public static bool IsEnemy(this UnitCtrlBase aUnit, UnitCtrlBase bUnit)
    {
        return MartialTool.GetEnemyType(aUnit)?.Contains(bUnit.data.unitType) ?? false;
    }

    public static bool IsEnemy(this UnitCtrlBase aUnit, UnitType ut)
    {
        return MartialTool.GetEnemyType(aUnit)?.Contains(ut) ?? false;
    }

    public static bool IsEnemy(this UnitType aUt, UnitType bUt)
    {
        return MartialTool.GetEnemyType(aUt)?.Contains(bUt) ?? false;
    }

    public static bool IsSectGuardian(this UnitCtrlBase cunit)
    {
        return cunit.TryCast<UnitCtrlMonst>()?.data.unitAttrItem.id == 9371;
    }

    public static bool IsPotmon(this UnitCtrlBase cunit)
    {
        try
        {
            return MartialTool.IsPotmonUnit(cunit);
        }
        catch
        {
            return false;
        }
    }

    public static bool IsArtifactUnit(this UnitCtrlBase cunit)
    {
        return MartialTool.IsArtifactUnit(cunit);
    }

    public static List<UnitCtrlBase> FindNearCEnemys(this UnitCtrlBase cunit, float radius)
    {
        var enemyTypes = MartialTool.GetEnemyType(cunit);
        return ModBattleEvent.SceneBattle.unit.GetRangeUnit(cunit.transform.position, radius).ToArray().Where(x => enemyTypes.Contains(x.data.unitType)).ToList();
    }
}