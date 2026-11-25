using ModLib.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModLib.Object
{
    /// <summary>
    /// Windows waiting for their UI of the game: a UICustom is a UI of the game (UICustom1 = UITextInfoLong, UICustom2 =
    /// UITextInfo) and the game keeps one of each: a window opened while another of its type is open (a notice of a mod,
    /// the error reports consent on the title screen) would write over it. Open(uiName, open) runs <c>open</c> at once
    /// when that UI is free, else queues it; Update (every frame, ModMaster.OnMonoUpdate) opens the next one of a type
    /// once its UI is closed, one at a time, in the order they were asked. UICustom1.WhenFree / UICustom2.WhenFree.
    /// </summary>
    public static class UICustomQueue
    {
        /// <summary>A UI of the game is open (by its uiName). Tests replace it.</summary>
        public static Func<string, bool> IsOpen { get; set; } = name => g.ui.HasUI(UIType.GetUIType(name));

        /// <summary>Where the error of a window is written (DebugHelper). Tests replace it.</summary>
        public static Action<string> Log { get; set; } = msg => DebugHelper.WriteLine(msg);

        private static readonly List<KeyValuePair<string, Action>> _queue = new List<KeyValuePair<string, Action>>();

        /// <summary>Windows waiting (all types).</summary>
        public static int Count => _queue.Count;

        /// <summary>Windows waiting for a UI type.</summary>
        public static int CountOf(string uiName) => _queue.Count(x => x.Key == uiName);

        /// <summary>
        /// Opens a window when its UI of the game is free: now (nothing of that type open or waiting), else after the
        /// windows waiting before it. Returns true when opened now.
        /// </summary>
        public static bool Open(string uiName, Action open)
        {
            if (open == null) return false;
            if (CountOf(uiName) == 0 && !IsOpen(uiName))
            {
                Run(uiName, open);
                return true;
            }
            _queue.Add(new KeyValuePair<string, Action>(uiName, open));
            return false;
        }

        /// <summary>The next window of each type whose UI is closed, opened (every frame).</summary>
        public static void Update()
        {
            if (_queue.Count == 0) return;
            foreach (var name in _queue.Select(x => x.Key).Distinct().ToList())
            {
                if (IsOpen(name)) continue;
                var next = _queue.First(x => x.Key == name);
                _queue.Remove(next);
                Run(name, next.Value);
            }
        }

        /// <summary>The windows waiting dropped (leaving the title screen for a game, tests).</summary>
        public static void Clear() => _queue.Clear();

        private static void Run(string uiName, Action open)
        {
            try { open(); }
            catch (Exception ex) { Log?.Invoke($"UICustomQueue {uiName}: {ex}"); }
        }
    }
}
