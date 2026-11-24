using ModLib.Attributes;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for working with game buildings (schools, towns, cities).
    /// Provides utilities to identify and classify different building types.
    /// </summary>
    [ActionCat("Building")]
    public static class BuildHelper
    {
        /// <summary>
        /// Checks if a building is a school.
        /// </summary>
        /// <param name="build">The building to check</param>
        /// <returns>True if the building is a school, false otherwise</returns>
        public static bool IsSchool(this MapBuildBase build)
        {
            return build?.TryCast<MapBuildSchool>() != null;
        }

        /// <summary>
        /// Checks if a building is a town (either main city or small town).
        /// </summary>
        /// <param name="build">The building to check</param>
        /// <returns>True if the building is a town, false otherwise</returns>
        public static bool IsTown(this MapBuildBase build)
        {
            return build?.TryCast<MapBuildTown>() != null;
        }

        /// <summary>
        /// Checks if a building is a main city (large town).
        /// </summary>
        /// <param name="build">The building to check</param>
        /// <returns>True if the building is a main city, false otherwise</returns>
        public static bool IsCity(this MapBuildBase build)
        {
            return (build?.TryCast<MapBuildTown>()?.buildTownData?.isMainTown).Is(true) == 1;
        }

        /// <summary>
        /// Checks if a building is a small town (not a main city).
        /// </summary>
        /// <param name="build">The building to check</param>
        /// <returns>True if the building is a small town, false otherwise</returns>
        public static bool IsSmallTown(this MapBuildBase build)
        {
            return (build?.TryCast<MapBuildTown>()?.buildTownData?.isMainTown).Is(false) == 1;
        }
    }
}