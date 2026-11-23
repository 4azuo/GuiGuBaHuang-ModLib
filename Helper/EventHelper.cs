using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// One handler of a method name: the event, its method and what RunMinorEvents needs, found once (reflection)
        /// and kept until the cache changes (CacheHelper.Version).
        /// </summary>
        private sealed class Handler
        {
            public ModEvent Event;
            public MethodInfo Method;
            public EventConditionAttribute Condition;
            public bool IgnoreErrors;
            public Action<object> Invoke;
            public string Name;
        }

        private static readonly object handlersLock = new object();
        private static Dictionary<string, List<Handler>> handlers = new Dictionary<string, List<Handler>>();
        private static int handlersVersion = -1;

        /// <summary>
        /// Handlers of a method name in cache order: the loaded events whose own type declares the method. Built once
        /// per method name and cache version (before: GetMethod on every loaded object at every call, i.e. for every
        /// unit of OnMonthlyForEachWUnit).
        /// </summary>
        private static List<Handler> GetHandlers(string methodName)
        {
            lock (handlersLock)
            {
                if (handlersVersion != CacheHelper.Version)
                {
                    handlers = new Dictionary<string, List<Handler>>();
                    handlersVersion = CacheHelper.Version;
                }
                if (!handlers.TryGetValue(methodName, out var list))
                {
                    list = new List<Handler>();
                    foreach (var ev in GetEvents())
                    {
                        var method = ev.GetType().GetMethod(methodName);
                        if (method == null || method.DeclaringType != ev.GetType())
                            continue;
                        list.Add(new Handler
                        {
                            Event = ev,
                            Method = method,
                            Condition = method.GetAttributeOnMethodOrClass<EventConditionAttribute>(),
                            IgnoreErrors = method.GetAttributeOnMethodOrClass<ErrorIgnoreAttribute>() != null,
                            Invoke = CreateInvoker(ev, method),
                            Name = $"{ev.CacheId}.{method.Name}",
                        });
                    }
                    handlers[methodName] = list;
                }
                return list;
            }
        }

        /// <summary>A direct call of the method on the event (delegate; MethodInfo.Invoke when no delegate fits).</summary>
        private static Action<object> CreateInvoker(object target, MethodInfo method)
        {
            try
            {
                if (method.ReturnType == typeof(void))
                {
                    var ps = method.GetParameters();
                    if (ps.Length == 0)
                    {
                        var a = (Action)Delegate.CreateDelegate(typeof(Action), target, method);
                        return _ => a();
                    }
                    if (ps.Length == 1 && !ps[0].ParameterType.IsByRef)
                    {
                        return (Action<object>)typeof(EventHelper).GetMethod(nameof(CreateInvoker1), BindingFlags.NonPublic | BindingFlags.Static)
                            .MakeGenericMethod(ps[0].ParameterType).Invoke(null, new object[] { target, method });
                    }
                }
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"EventHelper: no delegate for {target}.{method.Name} ({ex.Message}), reflection call");
            }
            var n = method.GetParameters().Length;
            return x => InvokeReflection(target, method, n, x);
        }

        private static Action<object> CreateInvoker1<T>(object target, MethodInfo method)
        {
            var d = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), target, method);
            return x => d(x == null ? default : (T)x);
        }

        /// <summary>MethodInfo.Invoke that throws the exception of the method itself (not TargetInvocationException).</summary>
        private static void InvokeReflection(object target, MethodInfo method, int paramCount, object e)
        {
            try
            {
                method.Invoke(target, paramCount == 0 ? null : new object[] { e });
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            }
        }

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
            var debugMode = ModMaster.ModObj.ModLibConfigs.DebugMode;
            foreach (var h in GetHandlers(methodName))
                RunHandler(h, methodName, e, delayMsec, isInGame, isInBattle, debugMode);
        }

        /// <summary>
        /// Runs OnMonthlyForEachWUnit for every unit of units (the ModLib list), in the order unit by unit then
        /// handler by handler. A handler whose mod (ModChild) set its own ParameterStore this month gets only the
        /// units of that store (e.g. Taoist "All functions apply to nearest units"); the other handlers get every unit.
        /// </summary>
        public static void RunMinorEventsForEachWUnit(WorldUnitBase[] units)
        {
            const string methodName = "OnMonthlyForEachWUnit";
            if (units == null)
                return;
            var hs = GetHandlers(methodName);
            // the unit ids of each mod store (null: no store of its own, every unit)
            var sets = new Dictionary<ParameterStore, HashSet<string>>();
            var filters = new HashSet<string>[hs.Count];
            for (var i = 0; i < hs.Count; i++)
            {
                var store = hs[i].Event.Parent?.ParameterStore;
                if (store?.WUnits == null || store.WUnits == units)
                    continue;
                if (!sets.TryGetValue(store, out var set))
                    sets[store] = set = new HashSet<string>(store.WUnits.Where(x => x != null).Select(x => x.GetUnitId()));
                filters[i] = set;
            }
            var isInGame = GameHelper.IsInGame();
            var isInBattle = GameHelper.IsInBattlle();
            var debugMode = ModMaster.ModObj.ModLibConfigs.DebugMode;
            foreach (var wunit in units)
            {
                var unitId = sets.Count > 0 ? wunit?.GetUnitId() : null;
                for (var i = 0; i < hs.Count; i++)
                {
                    if (filters[i] != null && (unitId == null || !filters[i].Contains(unitId)))
                        continue;
                    RunHandler(hs[i], methodName, wunit, 0, isInGame, isInBattle, debugMode);
                }
            }
        }

        private static void RunHandler(Handler h, string methodName, object e, int delayMsec, bool isInGame, bool isInBattle, DebugModeEnum debugMode)
        {
            try
            {
                var condAttr = h.Condition;
                if (condAttr != null)
                {
                    if (condAttr.IsInGame == HandleEnum.True && !isInGame)
                        return;
                    if (condAttr.IsInGame == HandleEnum.False && isInGame)
                        return;
                    if (condAttr.IsInBattle == HandleEnum.True && !isInBattle)
                        return;
                    if (condAttr.IsInBattle == HandleEnum.False && isInBattle)
                        return;
                }
                if ((condAttr?.DelayMsec ?? 0) > 0)
                {
                    ModDelayEvent.Instance.DelayEvent(h.Event, h.Method, e, condAttr.DelayMsec);
                }
                else if (delayMsec > 0)
                {
                    ModDelayEvent.Instance.DelayEvent(h.Event, h.Method, e, delayMsec);
                }
                else
                {
                    Run(h.Event, h.Name, h.Invoke, e, debugMode);
                }
            }
            catch (Exception ex)
            {
                var exMethod = ex?.GetCallingMethod();
                if (!h.IgnoreErrors && exMethod?.GetAttributeOnMethodOrClass<ErrorIgnoreAttribute>() == null)
                {
                    DebugHelper.WriteLine(new Exception($"【Error】{h.Event}.{methodName}({e}) : {h.Event.ModId}, {h.Event.CacheId}, {h.Event.CacheType}, {h.Event.WorkOn}", ex));
                }
                else
                {
                    // [ErrorIgnore]: no dialog, but the error is still logged (once)
                    DebugHelper.WriteIgnoredError($"{h.Event}.{methodName} : {h.Event.ModId}, {h.Event.CacheId}", ex);
                }
            }
        }

        /// <summary>
        /// Runs one handler: the "Start" line (Fine), the event trace, the call, and its time (Fine: per handler
        /// statistics, see WriteProfile; Finest: also one line per call, except the per-unit handlers: too many lines).
        /// </summary>
        private static void Run(ModEvent ev, string name, Action<object> invoke, object e, DebugModeEnum debugMode)
        {
            var fine = debugMode >= DebugModeEnum.Fine;
            var perUnit = fine && IsPerUnit(name);
            if (fine && !perUnit)
                DebugHelper.WriteLine($"【Start】{ev}.{name}({e}) : {ev.ModId}, {ev.CacheId}, {ev.CacheType}, {ev.WorkOn}");
            var start = fine ? Stopwatch.GetTimestamp() : 0L;
            var trace = EventTraceHelper.Enter(name);
            try { invoke(e); }
            finally
            {
                EventTraceHelper.Exit(trace);
                if (fine)
                {
                    var ticks = Stopwatch.GetTimestamp() - start;
                    AddProfile(name, ticks);
                    if (debugMode >= DebugModeEnum.Finest && !perUnit)
                        DebugHelper.WriteLine($"【{TimeSpan.FromSeconds((double)ticks / Stopwatch.Frequency).ToString(@"ss\.ff")}】{ev}.{name}({e}) : {ev.ModId}, {ev.CacheId}, {ev.CacheType}, {ev.WorkOn}");
                }
            }
        }

        private static bool IsPerUnit(string name) => name.EndsWith(".OnMonthlyForEachWUnit", StringComparison.Ordinal);

        /// <summary>
        /// Runs a single minor event method with timing and debug logging.
        /// </summary>
        /// <param name="ev">The event object</param>
        /// <param name="method">The method to invoke</param>
        /// <param name="e">Event argument to pass to the method</param>
        public static void RunMinorEvent(ModEvent ev, MethodInfo method, object e = null)
        {
            var n = method.GetParameters().Length;
            Run(ev, $"{ev.CacheId}.{method.Name}", x => InvokeReflection(ev, method, n, x), e, ModMaster.ModObj.ModLibConfigs.DebugMode);
        }

        /// <summary>
        /// Runs a single minor event action with timing and debug logging.
        /// </summary>
        /// <param name="ev">The event object</param>
        /// <param name="method">The action to invoke</param>
        public static void RunMinorEvent(ModEvent ev, Action method)
        {
            Run(ev, $"{ev.CacheId}.{method.Method.Name}", _ => method(), null, ModMaster.ModObj.ModLibConfigs.DebugMode);
        }

        // ------------------------------------------------------------------ profile (DebugMode Fine and above)

        /// <summary>Calls and total time of one handler (or a phase of the month) since the last report.</summary>
        public sealed class ProfileEntry
        {
            public string Name;
            public int Calls;
            public long Ticks;
            public double TotalMs => Ticks * 1000.0 / Stopwatch.Frequency;
        }

        private static readonly Dictionary<string, ProfileEntry> profile = new Dictionary<string, ProfileEntry>();

        /// <summary>Adds a call of ticks (Stopwatch ticks) to the entry name.</summary>
        public static void AddProfile(string name, long ticks)
        {
            lock (profile)
            {
                if (!profile.TryGetValue(name, out var p))
                    profile[name] = p = new ProfileEntry { Name = name };
                p.Calls++;
                p.Ticks += ticks;
            }
        }

        /// <summary>The entries since the last call (largest total first), then cleared.</summary>
        public static List<ProfileEntry> TakeProfile()
        {
            lock (profile)
            {
                var rs = profile.Values.OrderByDescending(x => x.Ticks).ToList();
                profile.Clear();
                return rs;
            }
        }

        /// <summary>
        /// Writes the handler times since the last report into the log and clears them (DebugMode Fine and above):
        /// the phases of the month ("month:" entries), then the top handlers with calls, total and average.
        /// ModMaster writes one after every month.
        /// </summary>
        public static void WriteProfile(string title, int top = 20)
        {
            var list = TakeProfile();
            if (list.Count == 0)
                return;
            var phases = list.Where(x => x.Name.StartsWith("month:", StringComparison.Ordinal)).ToList();
            var handlerList = list.Except(phases).ToList();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"【Profile】{title}: {handlerList.Sum(x => x.TotalMs):0.0} ms in {handlerList.Count} handlers");
            foreach (var p in phases)
                sb.AppendLine($"  {p.TotalMs,9:0.0} ms  {p.Name}");
            foreach (var p in handlerList.Take(top))
                sb.AppendLine($"  {p.TotalMs,9:0.0} ms  {p.Calls,7} calls  {p.TotalMs * 1000 / Math.Max(1, p.Calls),9:0.0} us/call  {p.Name}");
            DebugHelper.WriteLine(sb.ToString().TrimEnd());
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
            return GetHandlers(methodName).Select(x => x.Event).ToList();
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

        /// <summary>
        /// True when the event with this CacheId is loaded (its OnCacheHandler accepted it). Do not test
        /// "XxxEvent.Instance == null": ModLib sets Instance in OnLoadClass, before OnCacheHandler can reject the object.
        /// </summary>
        public static bool IsLoaded(string cacheId) => GetEvents().Any(x => x.CacheId == cacheId);
    }
}