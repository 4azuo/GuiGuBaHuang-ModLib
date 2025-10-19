using System;

namespace ModLib.Helper
{
    public static class ActionHelper
    {
        #region Action
        public class TracedAction
        {
            public Action Act { get; private set; }
            public Action Fin { get; private set; }

            public TracedAction(Action act, Action fin = null)
            {
                Act = act;
                Fin = fin;
            }

            public void Invoke()
            {
                try
                {
                    Act?.Invoke();
                }
                catch (Exception ex)
                {
                    var exMethod = ex?.GetCallingMethod();
                    var exType = exMethod.DeclaringType;
                    DebugHelper.WriteLine(new Exception($"【Error】{exType}.{exMethod.Name}(Some of the lambda functions inside are causing errors)", ex));
                }
                finally
                {
                    Fin?.Invoke();
                }
            }
        }

        public class TracedAction<T1>
        {
            public Action<T1> Act { get; private set; }
            public Action<T1> Fin { get; private set; }

            public TracedAction(Action<T1> act, Action<T1> fin = null)
            {
                Act = act;
                Fin = fin;
            }

            public void Invoke(T1 arg1)
            {
                try
                {
                    Act?.Invoke(arg1);
                }
                catch (Exception ex)
                {
                    var exMethod = ex?.GetCallingMethod();
                    var exType = exMethod.DeclaringType;
                    DebugHelper.WriteLine(new Exception($"【Error】{exType}.{exMethod.Name}(Some of the lambda functions inside are causing errors)", ex));
                }
                finally
                {
                    Fin?.Invoke(arg1);
                }
            }
        }

        public class TracedAction<T1, T2>
        {
            public Action<T1, T2> Act { get; private set; }
            public Action<T1, T2> Fin { get; private set; }

            public TracedAction(Action<T1, T2> act, Action<T1, T2> fin = null)
            {
                Act = act;
                Fin = fin;
            }

            public void Invoke(T1 arg1, T2 arg2)
            {
                try
                {
                    Act?.Invoke(arg1, arg2);
                }
                catch (Exception ex)
                {
                    var exMethod = ex?.GetCallingMethod();
                    var exType = exMethod.DeclaringType;
                    DebugHelper.WriteLine(new Exception($"【Error】{exType}.{exMethod.Name}(Some of the lambda functions inside are causing errors)", ex));
                }
                finally
                {
                    Fin?.Invoke(arg1, arg2);
                }
            }
        }

        public class TracedAction<T1, T2, T3>
        {
            public Action<T1, T2, T3> Act { get; private set; }
            public Action<T1, T2, T3> Fin { get; private set; }

            public TracedAction(Action<T1, T2, T3> act, Action<T1, T2, T3> fin = null)
            {
                Act = act;
                Fin = fin;
            }

            public void Invoke(T1 arg1, T2 arg2, T3 arg3)
            {
                try
                {
                    Act?.Invoke(arg1, arg2, arg3);
                }
                catch (Exception ex)
                {
                    var exMethod = ex?.GetCallingMethod();
                    var exType = exMethod.DeclaringType;
                    DebugHelper.WriteLine(new Exception($"【Error】{exType}.{exMethod.Name}(Some of the lambda functions inside are causing errors)", ex));
                }
                finally
                {
                    Fin?.Invoke(arg1, arg2, arg3);
                }
            }
        }
        #endregion

        #region Func
        public class TracedFunc<T1>
        {
            public Func<T1> Act { get; private set; }
            public Func<T1> Fin { get; private set; }

            public TracedFunc(Func<T1> act, Func<T1> fin = null)
            {
                Act = act;
                Fin = fin;
            }

            public T1 Invoke()
            {
                try
                {
                    if (Act == null)
                        return default;
                    return Act.Invoke();
                }
                catch (Exception ex)
                {
                    var exMethod = ex?.GetCallingMethod();
                    var exType = exMethod.DeclaringType;
                    DebugHelper.WriteLine(new Exception($"【Error】{exType}.{exMethod.Name}(Some of the lambda functions inside are causing errors)", ex));
                }
                try
                {
                    if (Fin == null)
                        return default;
                    return Fin.Invoke();
                }
                catch
                {
                    return default;
                }
            }
        }

        public class TracedFunc<T1, T2>
        {
            public Func<T1, T2> Act { get; private set; }
            public Func<T1, T2> Fin { get; private set; }

            public TracedFunc(Func<T1, T2> act, Func<T1, T2> fin = null)
            {
                Act = act;
                Fin = fin;
            }

            public T2 Invoke(T1 arg1)
            {
                try
                {
                    if (Act == null)
                        return default;
                    return Act.Invoke(arg1);
                }
                catch (Exception ex)
                {
                    var exMethod = ex?.GetCallingMethod();
                    var exType = exMethod.DeclaringType;
                    DebugHelper.WriteLine(new Exception($"【Error】{exType}.{exMethod.Name}(Some of the lambda functions inside are causing errors)", ex));
                }
                try
                {
                    if (Fin == null)
                        return default;
                    return Fin.Invoke(arg1);
                }
                catch
                {
                    return default;
                }
            }
        }

        public class TracedFunc<T1, T2, T3>
        {
            public Func<T1, T2, T3> Act { get; private set; }
            public Func<T1, T2, T3> Fin { get; private set; }

            public TracedFunc(Func<T1, T2, T3> act, Func<T1, T2, T3> fin = null)
            {
                Act = act;
                Fin = fin;
            }

            public T3 Invoke(T1 arg1, T2 arg2)
            {
                try
                {
                    if (Act == null)
                        return default;
                    return Act.Invoke(arg1, arg2);
                }
                catch (Exception ex)
                {
                    var exMethod = ex?.GetCallingMethod();
                    var exType = exMethod.DeclaringType;
                    DebugHelper.WriteLine(new Exception($"【Error】{exType}.{exMethod.Name}(Some of the lambda functions inside are causing errors)", ex));
                }
                try
                {
                    if (Fin == null)
                        return default;
                    return Fin.Invoke(arg1, arg2);
                }
                catch
                {
                    return default;
                }
            }
        }

        public class TracedFunc<T1, T2, T3, T4>
        {
            public Func<T1, T2, T3, T4> Act { get; private set; }
            public Func<T1, T2, T3, T4> Fin { get; private set; }

            public TracedFunc(Func<T1, T2, T3, T4> act, Func<T1, T2, T3, T4> fin = null)
            {
                Act = act;
                Fin = fin;
            }

            public T4 Invoke(T1 arg1, T2 arg2, T3 arg3)
            {
                try
                {
                    if (Act == null)
                        return default;
                    return Act.Invoke(arg1, arg2, arg3);
                }
                catch (Exception ex)
                {
                    var exMethod = ex?.GetCallingMethod();
                    var exType = exMethod.DeclaringType;
                    DebugHelper.WriteLine(new Exception($"【Error】{exType}.{exMethod.Name}(Some of the lambda functions inside are causing errors)", ex));
                }
                try
                {
                    if (Fin == null)
                        return default;
                    return Fin.Invoke(arg1, arg2, arg3);
                }
                catch
                {
                    return default;
                }
            }
        }
        #endregion
    }
}
