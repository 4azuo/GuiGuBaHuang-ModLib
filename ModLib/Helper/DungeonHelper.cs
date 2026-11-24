using ModLib.Attributes;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for identifying dungeon and battle types.
    /// Provides utilities to check specific dungeon scenarios (sparring, arena, trials, etc.).
    /// </summary>
    [ActionCat("Dungeon")]
    public static class DungeonHelper
    {
        /// <summary>
        /// Checks if current battle is a sparring match.
        /// </summary>
        /// <returns>True if sparring</returns>
        public static bool IsSpar()
        {
            return g.world.battle.data.dungeonBaseItem.id == 2;
        }

        /// <summary>
        /// Checks if current battle is the Trail of Lightning dungeon.
        /// </summary>
        /// <returns>True if Trail of Lightning</returns>
        public static bool IsTrailOfLightning()
        {
            return g.world.battle.data.dungeonBaseItem.id == 110081;
        }

        /// <summary>
        /// Checks if current battle is an arena fight.
        /// </summary>
        /// <returns>True if arena</returns>
        public static bool IsArena()
        {
            return g.world.battle.data.dungeonBaseItem.id == 24;
        }

        /// <summary>
        /// Checks if current battle is a challenge dungeon.
        /// </summary>
        /// <returns>True if challenge</returns>
        public static bool IsChallenge()
        {
            return g.world.battle.data.dungeonBaseItem.id == 100411;
        }

        /// <summary>
        /// Checks if current battle is the Reborn Bamboo Challenge.
        /// </summary>
        /// <returns>True if Reborn Bamboo Challenge</returns>
        public static bool IsRebornBambooChallenge()
        {
            return g.world.battle.data.dungeonBaseItem.id == 2303;
        }

        /// <summary>
        /// Checks if current battle is a skill preview test.
        /// </summary>
        /// <returns>True if skill preview</returns>
        public static bool IsSkillPreview()
        {
            return g.world.battle.data.dungeonBaseItem.id == 10;
        }

        /// <summary>
        /// Checks if current battle is a real battle (not sparring or preview).
        /// </summary>
        /// <returns>True if real battle</returns>
        public static bool IsRealBattle()
        {
            return g.world.battle.data.isRealBattle;
        }

        /// <summary>
        /// Determines whether the current battle is a self battle.
        /// </summary>
        /// <returns><see langword="true"/> if the current battle involves the player themselves; otherwise, <see
        /// langword="false"/>.</returns>
        public static bool IsSelfBattle()
        {
            return g.world.battle.data.isSelfBattle;
        }

        /// <summary>
        /// Determines whether the current battle is a "Three Legs Crow" battle.
        /// </summary>
        /// <returns><see langword="true"/> if the current battle is identified as a "Three Legs Crow" battle; otherwise, <see
        /// langword="false"/>.</returns>
        public static bool IsThreeLegsCrowBattle()
        {
            return g.world.battle.data.dungeonBaseItem.id == 2039; //have to die
        }
    }
}