using ModLib.Enum;
using ModLib.Mod;
using ModLib.Object;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

public static class GameHelper
{
    public static string GetDayCode()
    {
        return $"{(g.world?.run?.roundMonth / 12) + 1:0000}{(g.world?.run?.roundMonth % 12) + 1:00}{g.world?.run?.roundDay + 1:00}";
    }

    public static bool IsInGame()
    {
        return g.world.isIntoWorld;
    }

    public static bool IsInBattlle()
    {
        return IsInGame() && g.world.battle.isBattle;
    }

    public static void LoadEnumObj(Assembly ass)
    {
        var enumTypes = ass.GetTypes().Where(x => x.IsClass && x.IsSubclassOf(typeof(EnumObject))).OrderBy(x => x.FullName).ToList();
        foreach (var t in enumTypes)
        {
            RuntimeHelpers.RunClassConstructor(t.TypeHandle);
        }
    }
}