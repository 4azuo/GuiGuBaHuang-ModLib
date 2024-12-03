public static class BuildHelper
{
    public static bool IsSchool(this MapBuildBase build)
    {
        return build?.TryCast<MapBuildSchool>() != null;
    }

    public static bool IsCity(this MapBuildBase build)
    {
        return !build.IsSchool() && (build?.TryCast<MapBuildTown>()?.buildTownData?.isMainTown).Is(true) == 1;
    }

    public static bool IsSmallTown(this MapBuildBase build)
    {
        return !build.IsSchool() && (build?.TryCast<MapBuildTown>()?.buildTownData?.isMainTown).Is(false) == 1;
    }
}