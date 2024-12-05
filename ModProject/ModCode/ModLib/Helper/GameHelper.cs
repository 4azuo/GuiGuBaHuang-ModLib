using ModLib.Mod;
using ModLib.Object;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

public static class GameHelper
{
    public static Assembly GetModMasterAssembly()
    {
        return Assembly.GetAssembly(typeof(ModMaster));
    }

    public static Assembly GetModMainAssembly()
    {
        return Assembly.GetAssembly(ModMaster.ModObj.GetType());
    }

    public static string GetDayCode()
    {
        return $"{(g.world?.run?.roundMonth / 12) + 1:0000}{(g.world?.run?.roundMonth % 12) + 1:00}{g.world?.run?.roundDay + 1:00}";
    }

    public static bool IsMaskScreen()
    {
        return g.ui?.GetUI(UIType.Mask) != null || g.ui?.GetUI(UIType.MaskNotClick) != null;
    }

    public static bool IsLoadingScreen()
    {
        return g.ui?.GetUI(UIType.Loading) != null ||
            g.ui?.GetUI(UIType.LoadingBar) != null ||
            g.ui?.GetUI(UIType.LoadingBig) != null ||
            g.ui?.GetUI(UIType.LoadingBigCacheFix) != null ||
            g.ui?.GetUI(UIType.LoadingCreateGame) != null;
    }

    public static bool IsModScreen()
    {
        return g.ui?.GetUI(UIType.ModMain) != null;
    }

    public static bool IsLoginScreen()
    {
        return g.ui?.GetUI(UIType.Login) != null;
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

    public static void LoadEnumObj(Assembly ass)
    {
        var enumTypes = ass.GetTypes().Where(x => x.IsClass && x.IsSubclassOf(typeof(EnumObject))).OrderBy(x => x.FullName).ToList();
        foreach (var t in enumTypes)
        {
            RuntimeHelpers.RunClassConstructor(t.TypeHandle);
            DebugHelper.WriteLine($"Load EObj: {t.FullName}");
        }
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