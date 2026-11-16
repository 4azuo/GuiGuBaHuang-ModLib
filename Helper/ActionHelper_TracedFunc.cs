using System;

namespace ModLib.Helper
{
    /// <summary>
    /// Wraps delegates so exceptions are logged (partial: Func overloads).
    /// </summary>
    public partial class ActionHelper
    {
        /// <summary>
        /// Wraps a Func so exceptions are logged and default(T) is returned.
        /// </summary>
        public static Func<T1> TracedFunc<T1>(Func<T1> act, T1 def = default, Func<T1> cat = null, Action fin = null)
        {
            return () =>
            {
                try
                {
                    if (act == null)
                        return default;
                    return act.Invoke();
                }
                catch (Exception ex)
                {
                    Error(ex);
                    if (cat == null)
                        return def;
                    try { return cat.Invoke(); } catch { return def; }
                }
                finally
                {
                    try { fin?.Invoke(); } catch { }
                }
            };
        }

        /// <summary>
        /// Wraps a Func so exceptions are logged and default(T) is returned.
        /// </summary>
        public static Func<T1, T2> TracedFunc<T1, T2>(Func<T1, T2> act, T2 def = default, Func<T1, T2> cat = null, Action<T1> fin = null)
        {
            return (v1) =>
            {
                try
                {
                    if (act == null)
                        return default;
                    return act.Invoke(v1);
                }
                catch (Exception ex)
                {
                    Error(ex);
                    if (cat == null)
                        return def;
                    try { return cat.Invoke(v1); } catch { return def; }
                }
                finally
                {
                    try { fin?.Invoke(v1); } catch { }
                }
            };
        }

        /// <summary>
        /// Wraps a Func so exceptions are logged and default(T) is returned.
        /// </summary>
        public static Func<T1, T2, T3> TracedFunc<T1, T2, T3>(Func<T1, T2, T3> act, T3 def = default, Func<T1, T2, T3> cat = null, Action<T1, T2> fin = null)
        {
            return (v1, v2) =>
            {
                try
                {
                    if (act == null)
                        return default;
                    return act.Invoke(v1, v2);
                }
                catch (Exception ex)
                {
                    Error(ex);
                    if (cat == null)
                        return def;
                    try { return cat.Invoke(v1, v2); } catch { return def; }
                }
                finally
                {
                    try { fin?.Invoke(v1, v2); } catch { }
                }
            };
        }

        /// <summary>
        /// Wraps a Func so exceptions are logged and default(T) is returned.
        /// </summary>
        public static Func<T1, T2, T3, T4> TracedFunc<T1, T2, T3, T4>(Func<T1, T2, T3, T4> act, T4 def = default, Func<T1, T2, T3, T4> cat = null, Action<T1, T2, T3> fin = null)
        {
            return (v1, v2, v3) =>
            {
                try
                {
                    if (act == null)
                        return default;
                    return act.Invoke(v1, v2, v3);
                }
                catch (Exception ex)
                {
                    Error(ex);
                    if (cat == null)
                        return def;
                    try { return cat.Invoke(v1, v2, v3); } catch { return def; }
                }
                finally
                {
                    try { fin?.Invoke(v1, v2, v3); } catch { }
                }
            };
        }
    }
}
