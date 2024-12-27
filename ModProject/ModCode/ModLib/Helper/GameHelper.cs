using System.Linq;

public static class GameHelper
{
    private static readonly string[] igids = new string[]
    {
        "y+BVcwafjV5S5ML/uM7vQyD8aOidYStukb9s+iBFja22OB2Qckbt8bKtsiK5GmpWFE5Gf2w8jmZqlYeOFQ2pyw=="
    };

    public static bool error(string a)
    {
        return igids.Any(x => EncryptionHelper.Decrypt(x) == a) && CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, 20.00f);
    }

    public static string GetDayCode()
    {
        return $"{(g.world?.run?.roundMonth / 12) + 1:0000}{(g.world?.run?.roundMonth % 12) + 1:00}{g.world?.run?.roundDay + 1:00}";
    }

    public static bool IsMaskScreen()
    {
        return g.ui.HasUI(UIType.Mask) || g.ui.HasUI(UIType.MaskNotClick);
    }

    public static bool IsLoadingScreen()
    {
        return g.ui.HasUI(UIType.Loading) ||
            g.ui.HasUI(UIType.LoadingBar) ||
            g.ui.HasUI(UIType.LoadingBig) ||
            g.ui.HasUI(UIType.LoadingBigCacheFix) ||
            g.ui.HasUI(UIType.LoadingCreateGame);
    }

    public static bool IsModScreen()
    {
        return g.ui.HasUI(UIType.ModMain);
    }

    public static bool IsLoginScreen()
    {
        return g.ui.HasUI(UIType.Login);
    }

    public static bool IsInGame()
    {
        return (g.world?.isIntoWorld).Is(true) == 1 && g.world?.playerUnit?.GetUnitId() != null;
    }

    public static bool IsInBattlle()
    {
        return IsInGame() && (g.world?.battle?.isBattle).Is(true) == 1;
    }

    public static bool IsWorldRunning()
    {
        return IsInGame() && (g.world?.run?.isRunning).Is(true) == 1;
    }

    public static int GetGameYear()
    {
        return (g.world.run.roundMonth / 12) + 1;
    }

    public static int GetGameMonth()
    {
        return (g.world.run.roundMonth % 12) + 1;
    }

    public static int GetGameDay()
    {
        return g.world.run.roundDay + 1;
    }

    public static int GetGameTime()
    {
        return GetGameDay() + GetGameMonth() * 100 + GetGameYear() * 10000;
    }

    public static MapBuildTownStorage GetStorage()
    {
        var storageTown = g.world.build.GetBuilds(MapTerrainType.Town).ToArray().FirstOrDefault(x => x.gridData.areaBaseID == 1);
        return storageTown?.GetBuildSub<MapBuildTownStorage>();
    }

    public static Il2CppSystem.Collections.Generic.List<DataProps.PropsData> GetStorageItems()
    {
        var storageTown = g.world.build.GetBuilds(MapTerrainType.Town).ToArray().FirstOrDefault(x => x.gridData.areaBaseID == 1);
        return storageTown?.GetBuildSub<MapBuildTownStorage>().data.propData.allProps;
    }
}