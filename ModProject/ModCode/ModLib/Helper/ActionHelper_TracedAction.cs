using System;

namespace ModLib.Helper
{
    /// <summary>
    /// Wraps delegates so exceptions are logged instead of crashing the game (partial: Action overloads).
    /// </summary>
    public partial class ActionHelper
    {
        /// <summary>
        /// Wraps an Action so exceptions are written to the ModLib log.
        /// </summary>
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

        /// <summary>
        /// Wraps an Action so exceptions are written to the ModLib log.
        /// </summary>
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

        /// <summary>
        /// Wraps an Action so exceptions are written to the ModLib log.
        /// </summary>
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

        /// <summary>
        /// Wraps an Action so exceptions are written to the ModLib log.
        /// </summary>
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
