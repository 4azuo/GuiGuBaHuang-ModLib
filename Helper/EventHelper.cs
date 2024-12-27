using System.Linq;
using ModLib.Mod;
using System.Collections.Generic;
using System;
using ModLib.Enum;
using ModLib.Object;

public static class EventHelper
{
    public static CachableObject RunningEvent { get; private set; }

    public static void RunMinorEvents(object e = null)
    {
        var methodName = TraceHelper.GetCurrentMethodInfo(2).Name;
        RunMinorEvents(methodName, e);
    }

    public static void RunMinorEvents(string methodName, object e)
    {
        var isInGame = GameHelper.IsInGame();
        var isInBattle = GameHelper.IsInBattlle();
        var isWorldRunning = GameHelper.IsWorldRunning();
        foreach (var ev in GetEvents(methodName))
        {
            RunningEvent = ev;
            var method = ev.GetType().GetMethod(methodName);
            try
            {
                var condAttr = method?.GetAttributeOnMethodOrClass<EventConditionAttribute>();
                if (condAttr != null)
                {
                    if (condAttr.IsInGame == HandleEnum.True && !isInGame)
                        continue;
                    if (condAttr.IsInGame == HandleEnum.False && isInGame)
                        continue;
                    if (condAttr.IsInBattle == HandleEnum.True && !isInBattle)
                        continue;
                    if (condAttr.IsInBattle == HandleEnum.False && isInBattle)
                        continue;
                    if (condAttr.IsWorldRunning == HandleEnum.True && !isWorldRunning)
                        continue;
                    if (condAttr.IsWorldRunning == HandleEnum.False && isWorldRunning)
                        continue;
                }
                //var sw = new Stopwatch();
                //sw.Start();
                if (method.GetParameters().Length == 0)
                {
                    method.Invoke(ev, null);
                }
                else
                {
                    method.Invoke(ev, new object[] { e });
                }
                //sw.Stop();
                //DebugHelper.WriteLine($"Benchmark: {method.DeclaringType.FullName}|{method.Name}: {sw.Elapsed}");
            }
            catch (Exception ex)
            {
                var exMethod = ex?.GetCallingMethod();
                if (method?.GetAttributeOnMethodOrClass<ErrorIgnoreAttribute>() == null &&
                    exMethod?.GetAttributeOnMethodOrClass<ErrorIgnoreAttribute>() == null)
                {
                    throw ex;
                }
            }
        }
    }

    public static T GetEvent<T>(string eventId) where T : ModEvent
    {
        return CacheHelper.GetCachableObject<T>(eventId);
    }

    public static ModEvent GetEvent(string methodName, string eventId)
    {
        return GetEvents(methodName).FirstOrDefault(x => x.CacheId == eventId);
    }

    public static List<ModEvent> GetEvents(string methodName)
    {
        return GetEvents().Where(x => x.IsDeclaredMethod(methodName)).ToList();
    }

    public static List<ModEvent> GetEvents()
    {
        return CacheHelper.GetAllCachableObjects<ModEvent>();
    }
}