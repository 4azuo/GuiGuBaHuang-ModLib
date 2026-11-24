using System;
using UnityEngine.Events;

namespace ModLib.Helper
{
    /// <summary>
    /// Wraps delegates so exceptions are logged (partial: UnityAction overloads).
    /// </summary>
    public partial class ActionHelper
    {
        /// <summary>
        /// Wraps an action into a UnityAction (button listeners) that logs exceptions.
        /// </summary>
        public static UnityAction TracedUnityAction(Action act, Action cat = null, Action fin = null)
        {
            return (UnityAction)(() => TracedAction(act, cat, fin).Invoke());
        }

        /// <summary>
        /// Wraps an action into a UnityAction (button listeners) that logs exceptions.
        /// </summary>
        public static UnityAction<T1> TracedUnityAction<T1>(Action<T1> act, Action<T1> cat = null, Action<T1> fin = null)
        {
            return (UnityAction<T1>)((v1) => TracedAction(act, cat, fin).Invoke(v1));
        }

        /// <summary>
        /// Wraps an action into a UnityAction (button listeners) that logs exceptions.
        /// </summary>
        public static UnityAction<T1, T2> TracedUnityAction<T1, T2>(Action<T1, T2> act, Action<T1, T2> cat = null, Action<T1, T2> fin = null)
        {
            return (UnityAction<T1, T2>)((v1, v2) => TracedAction(act, cat, fin).Invoke(v1, v2));
        }

        /// <summary>
        /// Wraps an action into a UnityAction (button listeners) that logs exceptions.
        /// </summary>
        public static UnityAction<T1, T2, T3> TracedUnityAction<T1, T2, T3>(Action<T1, T2, T3> act, Action<T1, T2, T3> cat = null, Action<T1, T2, T3> fin = null)
        {
            return (UnityAction<T1, T2, T3>)((v1, v2, v3) => TracedAction(act, cat, fin).Invoke(v1, v2, v3));
        }
    }
}
