using System;

namespace ModLib.Helper
{
    public partial class ActionHelper
    {
        public static Action TracedAction(Action act, Action cat = null, Action fin = null)
        {
            return () =>
            {
                try
                {
                    act?.Invoke();
                }
                catch (Exception ex)
                {
                    Error(ex);
                    try { cat?.Invoke(); } catch { }
                }
                finally
                {
                    try { fin?.Invoke(); } catch { }
                }
            };
        }

        public static Action<T1> TracedAction<T1>(Action<T1> act, Action<T1> cat = null, Action<T1> fin = null)
        {
            return (v1) =>
            {
                try
                {
                    act?.Invoke(v1);
                }
                catch (Exception ex)
                {
                    Error(ex);
                    try { cat?.Invoke(v1); } catch { }
                }
                finally
                {
                    try { fin?.Invoke(v1); } catch { }
                }
            };
        }

        public static Action<T1, T2> TracedAction<T1, T2>(Action<T1, T2> act, Action<T1, T2> cat = null, Action<T1, T2> fin = null)
        {
            return (v1, v2) =>
            {
                try
                {
                    act?.Invoke(v1, v2);
                }
                catch (Exception ex)
                {
                    Error(ex);
                    try { cat?.Invoke(v1, v2); } catch { }
                }
                finally
                {
                    try { fin?.Invoke(v1, v2); } catch { }
                }
            };
        }

        public static Action<T1, T2, T3> TracedAction<T1, T2, T3>(Action<T1, T2, T3> act, Action<T1, T2, T3> cat = null, Action<T1, T2, T3> fin = null)
        {
            return (v1, v2, v3) =>
            {
                try
                {
                    act?.Invoke(v1, v2, v3);
                }
                catch (Exception ex)
                {
                    Error(ex);
                    try { cat?.Invoke(v1, v2, v3); } catch { }
                }
                finally
                {
                    try { fin?.Invoke(v1, v2, v3); } catch { }
                }
            };
        }
    }
}
