using System;
using static ModLib.Helper.ActionHelper;

namespace ModLib.Helper
{
    public partial class ActionHelper
    {
        #region TracedActionDelegate
        public class TracedActionWrapper
        {
            public Action Action { get; set; }
            public void Invoke()
            {
                Action.Invoke();
            }
        }

        public class TracedActionWrapper<T1>
        {
            public Action<T1> Action { get; set; }
            public void Invoke(T1 v1)
            {
                Action.Invoke(v1);
            }
        }

        public class TracedActionWrapper<T1, T2>
        {
            public Action<T1, T2> Action { get; set; }
            public void Invoke(T1 v1, T2 v2)
            {
                Action.Invoke(v1, v2);
            }
        }

        public class TracedActionWrapper<T1, T2, T3>
        {
            public Action<T1, T2, T3> Action { get; set; }
            public void Invoke(T1 v1, T2 v2, T3 v3)
            {
                Action.Invoke(v1, v2, v3);
            }
        }

        public static TracedActionWrapper WTracedAction(Action act, Action cat = null, Action fin = null)
        {
            return new TracedActionWrapper() { Action = TracedAction(act, cat, fin) };
        }

        public static TracedActionWrapper<T1> WTracedAction<T1>(Action<T1> act, Action<T1> cat = null, Action<T1> fin = null)
        {
            return new TracedActionWrapper<T1>() { Action = TracedAction(act, cat, fin) };
        }

        public static TracedActionWrapper<T1, T2> WTracedAction<T1, T2>(Action<T1, T2> act, Action<T1, T2> cat = null, Action<T1, T2> fin = null)
        {
            return new TracedActionWrapper<T1, T2>() { Action = TracedAction(act, cat, fin) };
        }

        public static TracedActionWrapper<T1, T2, T3> WTracedAction<T1, T2, T3>(Action<T1, T2, T3> act, Action<T1, T2, T3> cat = null, Action<T1, T2, T3> fin = null)
        {
            return new TracedActionWrapper<T1, T2, T3>() { Action = TracedAction(act, cat, fin) };
        }
        #endregion

        #region TracedFuncDelegate
        public class TracedFuncWrapper<T1>
        {
            public Func<T1> Action { get; set; }
            public T1 Invoke()
            {
                return Action.Invoke();
            }
        }

        public class TracedFuncWrapper<T1, T2>
        {
            public Func<T1, T2> Action { get; set; }
            public T2 Invoke(T1 v1)
            {
                return Action.Invoke(v1);
            }
        }

        public class TracedFuncWrapper<T1, T2, T3>
        {
            public Func<T1, T2, T3> Action { get; set; }
            public T3 Invoke(T1 v1, T2 v2)
            {
                return Action.Invoke(v1, v2);
            }
        }

        public class TracedFuncWrapper<T1, T2, T3, T4>
        {
            public Func<T1, T2, T3, T4> Action { get; set; }
            public T4 Invoke(T1 v1, T2 v2, T3 v3)
            {
                return Action.Invoke(v1, v2, v3);
            }
        }

        public static TracedFuncWrapper<T1> WTracedFunc<T1>(Func<T1> act, T1 def = default, Func<T1> cat = null, Action fin = null)
        {
            return new TracedFuncWrapper<T1>() { Action = TracedFunc(act, def, cat, fin) };
        }

        public static TracedFuncWrapper<T1, T2> WTracedFunc<T1, T2>(Func<T1, T2> act, T2 def = default, Func<T1, T2> cat = null, Action<T1> fin = null)
        {
            return new TracedFuncWrapper<T1, T2>() { Action = TracedFunc(act, def, cat, fin) };
        }

        public static TracedFuncWrapper<T1, T2, T3> WTracedFunc<T1, T2, T3>(Func<T1, T2, T3> act, T3 def = default, Func<T1, T2, T3> cat = null, Action<T1, T2> fin = null)
        {
            return new TracedFuncWrapper<T1, T2, T3>() { Action = TracedFunc(act, def, cat, fin) };
        }

        public static TracedFuncWrapper<T1, T2, T3, T4> WTracedFunc<T1, T2, T3, T4>(Func<T1, T2, T3, T4> act, T4 def = default, Func<T1, T2, T3, T4> cat = null, Action<T1, T2, T3> fin = null)
        {
            return new TracedFuncWrapper<T1, T2, T3, T4>() { Action = TracedFunc(act, def, cat, fin) };
        }
        #endregion
    }
}
