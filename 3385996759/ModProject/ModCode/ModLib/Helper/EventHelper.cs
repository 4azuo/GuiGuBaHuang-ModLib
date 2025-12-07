using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for managing and executing mod events.
    /// Handles event discovery, conditional execution, timing, and error handling.
    /// </summary>
    [ActionCat("Event")]
    public static class EventHelper
    {
        /// <summary>
        /// Runs all minor events with the specified method name.
        /// Checks execution conditions and handles delays.
        /// </summary>
        /// <param name="methodName">Name of the event method to run</param>
        /// <param name="e">Event argument to pass to the method</param>
        /// <param name="delayMsec">Delay in milliseconds before execution</param>
        public static void RunMinorEvents(string methodName, object e = null, int delayMsec = 0)
        {
            var isInGame = GameHelper.IsInGame();
            var isInBattle = GameHelper.IsInBattlle();
            foreach (var ev in GetEvents(methodName))
            {
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
                        DebugHelper.WriteLine(new Exception($"【Error】{ev}.{methodName}({e}) : {ev.ModId}, {ev.CacheId}, {ev.CacheType}, {ev.WorkOn}", ex));
                    }
                }
            }
        }

        /// <summary>
        /// Runs a single minor event method with timing and debug logging.
        /// </summary>
        /// <param name="ev">The event object</param>
        /// <param name="method">The method to invoke</param>
        /// <param name="e">Event argument to pass to the method</param>
        public static void RunMinorEvent(ModEvent ev, MethodInfo method, object e = null)
        {
            if (ModMaster.ModObj.ModLibConfigs.DebugMode >= DebugModeEnum.Fine)
                DebugHelper.WriteLine($"【Start】{ev}.{method.Name}({e}) : {ev.ModId}, {ev.CacheId}, {ev.CacheType}, {ev.WorkOn}");
            var timeStart = DateTime.Now;
            if (method.GetParameters().Length == 0)
            {
                method.Invoke(ev, null);
            }
            else
            {
                method.Invoke(ev, new object[] { e });
            }
            var timeEnd = DateTime.Now;
            var processTime = timeEnd - timeStart;
            if (ModMaster.ModObj.ModLibConfigs.DebugMode >= DebugModeEnum.Finest)
                DebugHelper.WriteLine($"【{processTime.ToString(@"ss\.ff")}】{ev}.{method.Name}({e}) : {ev.ModId}, {ev.CacheId}, {ev.CacheType}, {ev.WorkOn}");
        }

        /// <summary>
        /// Runs a single minor event action with timing and debug logging.
        /// </summary>
        /// <param name="ev">The event object</param>
        /// <param name="method">The action to invoke</param>
        public static void RunMinorEvent(ModEvent ev, Action method)
        {
            if (ModMaster.ModObj.ModLibConfigs.DebugMode >= DebugModeEnum.Fine)
                DebugHelper.WriteLine($"【Start】{ev}.{method.Method.Name}() : {ev.ModId}, {ev.CacheId}, {ev.CacheType}, {ev.WorkOn}");
            var timeStart = DateTime.Now;
            method.Invoke();
            var timeEnd = DateTime.Now;
            var processTime = timeEnd - timeStart;
            if (ModMaster.ModObj.ModLibConfigs.DebugMode >= DebugModeEnum.Finest)
                DebugHelper.WriteLine($"【{processTime.ToString(@"ss\.ff")}】{ev}.{method.Method.Name}() : {ev.ModId}, {ev.CacheId}, {ev.CacheType}, {ev.WorkOn}");
        }

        /// <summary>
        /// Gets a specific event by ID and type.
        /// </summary>
        /// <typeparam name="T">The event type</typeparam>
        /// <param name="eventId">The event ID to find</param>
        /// <returns>The event object, or null if not found</returns>
        public static T GetEvent<T>(string eventId) where T : ModEvent
        {
            return CacheHelper.GetCachableObject<T>(eventId);
        }

        /// <summary>
        /// Gets a specific event by method name and event ID.
        /// </summary>
        /// <param name="methodName">The method name</param>
        /// <param name="eventId">The event ID</param>
        /// <returns>The event object, or null if not found</returns>
        public static ModEvent GetEvent(string methodName, string eventId)
        {
            return GetEvents(methodName).FirstOrDefault(x => x.CacheId == eventId);
        }

        /// <summary>
        /// Gets all events that declare a specific method.
        /// </summary>
        /// <param name="methodName">The method name to check for</param>
        /// <returns>List of events with the method</returns>
        public static List<ModEvent> GetEvents(string methodName)
        {
            return GetEvents().Where(x => x.IsDeclaredMethod(methodName)).ToList();
        }

        /// <summary>
        /// Gets all loaded event objects.
        /// </summary>
        /// <returns>List of all events</returns>
        public static List<ModEvent> GetEvents()
        {
            return CacheHelper.GetAllCachableObjects<ModEvent>();
        }

        /// <summary>
        /// Gets all loaded ModChild objects.
        /// </summary>
        /// <returns>List of all mod children</returns>
        public static List<ModChild> GetModChilds()
        {
            return CacheHelper.GetAllCachableObjects<ModChild>();
        }

        /// <summary>
        /// Calls a game event with a specific type parameter.
        /// </summary>
        /// <typeparam name="T">Event data type</typeparam>
        /// <param name="eventsId">Event identifier</param>
        public static void CallGameEvent<T>(string eventsId) where T : ETypeData, new()
        {
            foreach (var e in g.events.allEvents[eventsId])
            {
                e.Call(new T());
            }
        }
    }
}