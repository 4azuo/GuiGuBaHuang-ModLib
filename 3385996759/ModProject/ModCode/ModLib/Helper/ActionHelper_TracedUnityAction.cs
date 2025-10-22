using System;
using UnityEngine.Events;

namespace ModLib.Helper
{
    public partial class ActionHelper
    {
        public static UnityAction TracedUnityAction(Action act, Action cat = null, Action fin = null)
        {
            return (UnityAction)(() => TracedAction(act, cat, fin).Invoke());
        }

        public static UnityAction<T1> TracedUnityAction<T1>(Action<T1> act, Action<T1> cat = null, Action<T1> fin = null)
        {
            return (UnityAction<T1>)((v1) => TracedAction(act, cat, fin).Invoke(v1));
        }

        public static UnityAction<T1, T2> TracedUnityAction<T1, T2>(Action<T1, T2> act, Action<T1, T2> cat = null, Action<T1, T2> fin = null)
        {
            return (UnityAction<T1, T2>)((v1, v2) => TracedAction(act, cat, fin).Invoke(v1, v2));
        }

        public static UnityAction<T1, T2, T3> TracedUnityAction<T1, T2, T3>(Action<T1, T2, T3> act, Action<T1, T2, T3> cat = null, Action<T1, T2, T3> fin = null)
        {
            return (UnityAction<T1, T2, T3>)((v1, v2, v3) => TracedAction(act, cat, fin).Invoke(v1, v2, v3));
        }
    }
}
