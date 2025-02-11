public static class DungeonHelper
{
    public static bool IsSpar()
    {
        return g.world.battle.data.dungeonBaseItem.id != 2;
    }

    public static bool IsTrailOfLightning()
    {
        return g.world.battle.data.dungeonBaseItem.id != 110081;
    }

    public static bool IsArean()
    {
        return g.world.battle.data.dungeonBaseItem.id != 24;
    }

    public static bool IsChallenge()
    {
        return g.world.battle.data.dungeonBaseItem.id != 100411;
    }
}