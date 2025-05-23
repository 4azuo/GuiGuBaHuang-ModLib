﻿using System.Linq;
using ModLib.Mod;
using System.Collections.Generic;
using System;
using ModLib.Enum;
using ModLib.Object;
using System.Reflection;

public static class EventHelper
{
    public static CachableObject RunningEvent { get; private set; }

    public static void RunMinorEvents(string methodName, object e = null, int delayMsec = 0)
    {
        var isInGame = GameHelper.IsInGame();
        var isInBattle = GameHelper.IsInBattlle();
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
                }
                if ((condAttr?.DelayMsec ?? 0) > 0)
                {
                    ModDelayEvent.Instance.DelayEvent(ev, method, e, condAttr.DelayMsec);
                }
                else if (delayMsec > 0)
                {
                    ModDelayEvent.Instance.DelayEvent(ev, method, e, delayMsec);
                }
                else
                {
                    RunMinorEvent(ev, method, e);
                }
            }
            catch (Exception ex)
            {
                var exMethod = ex?.GetCallingMethod();
                if (method?.GetAttributeOnMethodOrClass<ErrorIgnoreAttribute>() == null &&
                    exMethod?.GetAttributeOnMethodOrClass<ErrorIgnoreAttribute>() == null)
                {
                    DebugHelper.WriteLine($"{ev}.{methodName}({e}) : {ev.ModId}, {ev.CacheId}, {ev.CacheType}, {ev.WorkOn}");
                    DebugHelper.WriteLine(ex);
                }
            }
        }
    }

    public static void RunMinorEvent(ModEvent ev, MethodInfo method, object e = null)
    {
        if (method.GetParameters().Length == 0)
        {
            method.Invoke(ev, null);
        }
        else
        {
            method.Invoke(ev, new object[] { e });
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