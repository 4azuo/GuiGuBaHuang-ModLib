using System.Linq;
using ModLib.Mod;
using System.Collections.Generic;

public static class EventHelper
{
    public static T GetEvent<T>(string eventId) where T : ModEvent
    {
        return (T)GetEvents().FirstOrDefault(x => typeof(T).IsAssignableFrom(x.GetType()) && x.CacheId == eventId);
    }

    public static ModEvent GetEvent(string methodName, string eventId)
    {
        return GetEvents(methodName).FirstOrDefault(x => x.CacheId == eventId);
    }

    public static IList<ModEvent> GetEvents(string methodName)
    {
        return GetEvents().Where(x => x.IsDeclaredMethod(methodName)).ToList();
    }

    public static IList<ModEvent> GetEvents()
    {
        var rs = CacheHelper.GetGlobalCache().GetDatas<ModEvent>().OrderBy(x => x.OrderIndex).ToList();
        if (CacheHelper.IsGameCacheLoaded())
        {
            rs.AddRange(CacheHelper.GetGameCache().GetDatas<ModEvent>().OrderBy(x => x.OrderIndex));
        }
        return rs;
    }
}