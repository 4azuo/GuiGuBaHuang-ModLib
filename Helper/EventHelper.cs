using System.Linq;
using ModLib.Mod;
using System.Collections.Generic;
using System.Reflection;
using System;

public static class EventHelper
{
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
            var method = ev.GetType().GetMethod(methodName);
            try
            {
                var condAttr = method.GetCustomAttribute<EventConditionAttribute>();
                if (condAttr != null)
                {
                    if (condAttr.IsInGame == 1 && !isInGame)
                        continue;
                    if (condAttr.IsInGame == 0 && isInGame)
                        continue;
                    if (condAttr.IsInBattle == 1 && !isInBattle)
                        continue;
                    if (condAttr.IsInBattle == 0 && isInBattle)
                        continue;
                    if (condAttr.IsWorldRunning == 1 && !isWorldRunning)
                        continue;
                    if (condAttr.IsWorldRunning == 0 && isWorldRunning)
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
                if (method?.GetCustomAttribute<ErrorIgnoreAttribute>() == null &&
                    method?.DeclaringType?.GetCustomAttribute<ErrorIgnoreAttribute>() == null &&
                    exMethod?.GetCustomAttribute<ErrorIgnoreAttribute>() == null &&
                    exMethod?.DeclaringType?.GetCustomAttribute<ErrorIgnoreAttribute>() == null)
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