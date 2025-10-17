using ModLib.Object;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ModLib.Mod
{
    [Cache("$DELAY$", OrderIndex = 1, CacheType = CacheAttribute.CType.Global, WorkOn = CacheAttribute.WType.All)]
    public class ModDelayEvent : ModEvent
    {
        public static ModDelayEvent Instance { get; set; }
        private static List<DelayInfo> DelayEvents { get; } = new List<DelayInfo>();

        [JsonIgnore]
        public DelayInfo ProcessingEventInfo { get; set; }

        public override void OnTimeUpdate10ms()
        {
            base.OnTimeUpdate10ms();
            lock (DelayEvents)
            {
                var events = DelayEvents.Where(x => (DateTime.Now - x.Start).TotalMilliseconds > x.Delay).ToArray();
                DelayEvents.RemoveAll(x => events.Contains(x));
                foreach (var e in events)
                {
                    try
                    {
                        ProcessingEventInfo = e;
                        if (e.CustomMethod != null)
                        {
                            EventHelper.RunMinorEvent(e.Event, e.CustomMethod);
                        }
                        else
                        {
                            EventHelper.RunMinorEvent(e.Event, e.Method, e.Parameter);
                        }
                    }
                    catch (Exception ex)
                    {
                        var exMethod = ex?.GetCallingMethod();
                        if (e.Method?.GetAttributeOnMethodOrClass<ErrorIgnoreAttribute>() == null &&
                            exMethod?.GetAttributeOnMethodOrClass<ErrorIgnoreAttribute>() == null)
                        {
                            DebugHelper.WriteLine($"{e.Event}.{e.Method.Name}({e.Parameter}) : {e.Event.ModId}, {e.Event.CacheId}, {e.Event.CacheType}, {e.Event.WorkOn}");
                            DebugHelper.WriteLine(ex);
                        }
                    }
                }
            }
        }

        public void DelayEvent(ModEvent ev, MethodInfo method, object e, int delayMsec)
        {
            lock (DelayEvents)
            {
                DelayEvents.Add(new DelayInfo
                {
                    Event = ev,
                    Method = method,
                    Parameter = e,
                    Delay = delayMsec
                });
            }
        }

        public void DelayEvent(ModEvent ev, string methodName, object e, int delayMsec)
        {
            var method = ev.GetType().GetMethod(methodName);
            if (method != null)
            {
                DelayEvent(ev, method, e, delayMsec);
            }
        }

        public void DelayEvent(ModEvent ev, Action action, int delayMsec)
        {
            lock (DelayEvents)
            {
                DelayEvents.Add(new DelayInfo
                {
                    Event = ev,
                    CustomMethod = action,
                    Delay = delayMsec
                });
            }
        }
    }
}
