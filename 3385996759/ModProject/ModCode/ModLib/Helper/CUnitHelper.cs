using ModLib.Attributes;
using ModLib.Mod;
using System.Collections.Generic;
using System.Linq;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for working with combat units (UnitCtrlBase and derived types).
    /// Provides utilities for unit type checking, summoner relationships, and unit queries.
    /// </summary>
    [ActionCat("CUnit")]
    public static class CUnitHelper
    {
        /// <summary>
        /// Checks if a combat unit is a human character.
        /// </summary>
        /// <param name="cunit">The combat unit to check</param>
        /// <returns>True if the unit is human, false otherwise</returns>
        public static bool IsHuman(this UnitCtrlBase cunit)
        {
            return cunit?.TryCast<UnitCtrlHuman>() != null;
        }

        /// <summary>
        /// Checks if a combat unit is a monster.
        /// </summary>
        /// <param name="cunit">The combat unit to check</param>
        /// <returns>True if the unit is a monster, false otherwise</returns>
        public static bool IsMonster(this UnitCtrlBase cunit)
        {
            return cunit?.TryCast<UnitCtrlMonst>() != null;
        }

        /// <summary>
        /// Checks if a combat unit is a humanoid monster.
        /// </summary>
        /// <param name="cunit">The combat unit to check</param>
        /// <returns>True if the unit is a monster-human, false otherwise</returns>
        public static bool IsMonsterHuman(this UnitCtrlBase cunit)
        {
            return cunit?.TryCast<UnitCtrlMonstHuman>() != null;
        }

        /// <summary>
        /// Checks if a combat unit is an NPC.
        /// </summary>
        /// <param name="cunit">The combat unit to check</param>
        /// <returns>True if the unit is an NPC, false otherwise</returns>
        public static bool IsNPC(this UnitCtrlBase cunit)
        {
            return cunit?.TryCast<UnitCtrlHumanNPC>() != null;
        }

        /// <summary>
        /// Checks if a combat unit is the player character.
        /// </summary>
        /// <param name="cunit">The combat unit to check</param>
        /// <returns>True if the unit is the player, false otherwise</returns>
        public static bool IsPlayer(this UnitCtrlBase cunit)
        {
            return cunit?.TryCast<UnitCtrlPlayer>() != null;
        }

        /// <summary>
        /// Checks if a combat unit is a world unit (not a summon).
        /// </summary>
        /// <param name="cunit">The combat unit to check</param>
        /// <returns>True if the unit exists in the world and is not summoned</returns>
        public static bool IsWorldUnit(this UnitCtrlBase cunit)
        {
            return cunit.GetWorldUnit() != null && !cunit.IsSummoned();
        }

        /// <summary>
        /// Checks if a combat unit is a summoned creature.
        /// </summary>
        /// <param name="cunit">The combat unit to check</param>
        /// <returns>True if the unit is summoned, false otherwise</returns>
        public static bool IsSummoned(this UnitCtrlBase cunit)
        {
            return cunit?.monstSummon != null || (cunit?.IsPotmon() ?? false);
        }

        /// <summary>
        /// Gets the direct summoner of a summoned unit.
        /// </summary>
        /// <param name="cunit">The summoned combat unit</param>
        /// <returns>The unit that summoned this unit, or null if not summoned</returns>
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

        /// <summary>
        /// Gets the original summoner by traversing the summon chain (for nested summons).
        /// </summary>
        /// <param name="cunit">The summoned combat unit</param>
        /// <returns>The root summoner in the chain, or null if not summoned</returns>
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

        /// <summary>
        /// Gets the world unit associated with this combat unit.
        /// </summary>
        /// <param name="cunit">The combat unit</param>
        /// <returns>The corresponding WorldUnitBase, or null if not found</returns>
        public static WorldUnitBase GetWorldUnit(this UnitCtrlBase cunit)
        {
            if (cunit == null)
                return null;
            return g.world.unit.GetUnit(cunit);
        }

        /// <summary>
        /// Finds all combat units within a specified radius of this unit.
        /// </summary>
        /// <param name="cunit">The center unit</param>
        /// <param name="radius">Search radius</param>
        /// <returns>List of units within range</returns>
        public static Il2CppSystem.Collections.Generic.List<UnitCtrlBase> FindNearCUnits(this UnitCtrlBase cunit, float radius)
        {
            return ModBattleEvent.SceneBattle.unit.GetRangeUnit(cunit.transform.position, radius);
        }

        /// <summary>
        /// Checks if two combat units are enemies.
        /// </summary>
        /// <param name="aUnit">First unit</param>
        /// <param name="bUnit">Second unit</param>
        /// <returns>True if units are enemies, false otherwise</returns>
        public static bool IsEnemy(this UnitCtrlBase aUnit, UnitCtrlBase bUnit)
        {
            return MartialTool.GetEnemyType(aUnit)?.Contains(bUnit.data.unitType) ?? false;
        }

        /// <summary>
        /// Checks if a combat unit is an enemy of a specific unit type.
        /// </summary>
        /// <param name="aUnit">The combat unit</param>
        /// <param name="ut">The unit type to check against</param>
        /// <returns>True if the unit type is an enemy, false otherwise</returns>
        public static bool IsEnemy(this UnitCtrlBase aUnit, UnitType ut)
        {
            return MartialTool.GetEnemyType(aUnit)?.Contains(ut) ?? false;
        }

        /// <summary>
        /// Checks if two unit types are enemies.
        /// </summary>
        /// <param name="aUt">First unit type</param>
        /// <param name="bUt">Second unit type</param>
        /// <returns>True if unit types are enemies, false otherwise</returns>
        public static bool IsEnemy(this UnitType aUt, UnitType bUt)
        {
            return MartialTool.GetEnemyType(aUt)?.Contains(bUt) ?? false;
        }

        /// <summary>
        /// Checks if a combat unit is a sect guardian (special NPC type).
        /// </summary>
        /// <param name="cunit">The combat unit to check</param>
        /// <returns>True if the unit is a sect guardian, false otherwise</returns>
        public static bool IsSectGuardian(this UnitCtrlBase cunit)
        {
            return cunit.TryCast<UnitCtrlMonst>()?.data.unitAttrItem.id == 9371;
        }

        /// <summary>
        /// Checks if a combat unit is Potmon (Devil Demon artifact creature).
        /// </summary>
        /// <param name="cunit">The combat unit to check</param>
        /// <returns>True if the unit is Potmon, false otherwise</returns>
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

        /// <summary>
        /// Checks if a combat unit is an artifact unit (divine artifact creature).
        /// </summary>
        /// <param name="cunit">The combat unit to check</param>
        /// <returns>True if the unit is an artifact unit, false otherwise</returns>
        public static bool IsArtifactUnit(this UnitCtrlBase cunit)
        {
            return MartialTool.IsArtifactUnit(cunit);
        }

        /// <summary>
        /// Finds all enemy combat units within a specified radius of this unit.
        /// </summary>
        /// <param name="cunit">The center unit</param>
        /// <param name="radius">Search radius</param>
        /// <returns>List of enemy units within range</returns>
        public static List<UnitCtrlBase> FindNearCEnemys(this UnitCtrlBase cunit, float radius)
        {
            var enemyTypes = MartialTool.GetEnemyType(cunit);
            return ModBattleEvent.SceneBattle.unit.GetRangeUnit(cunit.transform.position, radius).ToArray().Where(x => enemyTypes.Contains(x.data.unitType)).ToList();
        }
    }
}