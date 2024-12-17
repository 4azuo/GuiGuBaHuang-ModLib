using ModLib.Mod;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

public static class GameHelper
{
    public static List<Assembly> GetAssembliesInChildren()
    {
        var rs = g.mod.allModPaths.ToArray().Select(x => GetModChildAssembly(x.t1)).ToList();
        rs.RemoveAll(item => item == null);
        return rs;
    }

    public static Assembly GetModLibAssembly()
    {
        return Assembly.GetAssembly(typeof(ModMaster));
    }

    public static Assembly GetModLibMainAssembly()
    {
        return Assembly.GetAssembly(ModMaster.ModObj.GetType());
    }

    public static Assembly GetModChildAssembly(string modId)
    {
        var assPath = $"{GetModChildPathRoot(modId)}\\ModCode\\dll\\Mod_{modId}.dll";
        if (!File.Exists(assPath))
            return null;
        return Assembly.LoadFrom(assPath);
    }

    public static string GetModChildPathRoot(string modId)
    {
        return g.mod.GetModPathRoot(modId);
    }

    public static string GetModChildPathSource(string modId)
    {
        return $"{GetModChildPathRoot(modId)}\\..\\..\\ModProject\\ModConf\\";
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