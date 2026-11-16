using System;

namespace ModLib.Helper
{
    /// <summary>
    /// Wraps delegates so exceptions are logged (partial: Il2CppSystem.Action overloads).
    /// </summary>
    public partial class ActionHelper
    {
        /// <summary>
        /// Wraps an action into an Il2CppSystem.Action that logs exceptions.
        /// </summary>
        public static Il2CppSystem.Action TracedIl2Action(Action act, Action cat = null, Action fin = null)
        {
            return (Il2CppSystem.Action)(() => TracedAction(act, cat, fin).Invoke());
        }

        /// <summary>
        /// Wraps an action into an Il2CppSystem.Action that logs exceptions.
        /// </summary>
        public static Il2CppSystem.Action<T1> TracedIl2Action<T1>(Action<T1> act, Action<T1> cat = null, Action<T1> fin = null)
        {
            return (Il2CppSystem.Action<T1>)((v1) => TracedAction(act, cat, fin).Invoke(v1));
        }

        /// <summary>
        /// Wraps an action into an Il2CppSystem.Action that logs exceptions.
        /// </summary>
        public static Il2CppSystem.Action<T1, T2> TracedIl2Action<T1, T2>(Action<T1, T2> act, Action<T1, T2> cat = null, Action<T1, T2> fin = null)
        {
            return (Il2CppSystem.Action<T1, T2>)((v1, v2) => TracedAction(act, cat, fin).Invoke(v1, v2));
        }

        /// <summary>
        /// Wraps an action into an Il2CppSystem.Action that logs exceptions.
        /// </summary>
        public static Il2CppSystem.Action<T1, T2, T3> TracedIl2Action<T1, T2, T3>(Action<T1, T2, T3> act, Action<T1, T2, T3> cat = null, Action<T1, T2, T3> fin = null)
        {
            return (Il2CppSystem.Action<T1, T2, T3>)((v1, v2, v3) => TracedAction(act, cat, fin).Invoke(v1, v2, v3));
        }
    }
}
