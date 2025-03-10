using ModLib.Object;
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

        public override void OnTimeUpdate()
        {
            base.OnTimeUpdate();
            lock (DelayEvents)
            {
                var cur = DelayEvents.Where(x => (DateTime.Now - x.Start).TotalMilliseconds > x.Delay).ToArray();
                DelayEvents.RemoveAll(x => cur.Contains(x));
                foreach (var d in cur)
                {
                    EventHelper.RunMinorEvent(d.Event, d.Method, d.Parameter);
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
    }
}
