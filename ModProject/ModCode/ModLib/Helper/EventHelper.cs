﻿using System.Linq;
using ModLib.Mod;
using System.Collections.Generic;
using UnhollowerBaseLib;
using UnityEngine.Events;
using System.Reflection;
using Newtonsoft.Json;
using ModLib.Enum;

public static class EventHelper
{
    public static void RunMinorEvents()
    {
        var methodName = TraceHelper.GetCurrentMethodInfo(2).Name;
        RunMinorEvents<object>(methodName, null);
    }

    public static void RunMinorEvents<T>(T e) where T : class
    {
        var methodName = TraceHelper.GetCurrentMethodInfo(2).Name;
        RunMinorEvents<T>(methodName, e);
    }

    public static void RunMinorEvents<T>(string methodName, T e) where T : class
    {
        foreach (var ev in GetEvents(methodName))
        {
            var method = ev.GetType().GetMethod(methodName);
            var condAttr = method.GetCustomAttribute<EventConditionAttribute>();
            if (condAttr != null)
            {
                if (condAttr.IsInGame && !GameHelper.IsInGame())
                    return;
                if (condAttr.IsInBattle && !GameHelper.IsInBattlle())
                    return;
                if (
                    (condAttr.WithLoadState == EvCondLoadEnum.Loaded && ev.IsLoading()) || 
                    (condAttr.WithLoadState == EvCondLoadEnum.Loading && !ev.IsLoading())
                    )
                    return;
                if (condAttr.NeedFlgUpdate && !ev.IsFlgUpdate(methodName))
                    return;
                if (condAttr.CustomCondition != null && !ev.GetType().GetMethod(condAttr.CustomCondition).Invoke(ev, null).Parse<bool>())
                    return;
            }

            if (method.GetParameters().Length == 0)
            {
                method.Invoke(ev, null);
            }
            else
            {
                method.Invoke(ev, new object[] { e });
            }

            ev.UpdateFlg2[methodName] = ev.UpdateFlg1[methodName];
        }
    }

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