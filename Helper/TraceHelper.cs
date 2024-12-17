using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

public static class TraceHelper
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static MethodBase[] GetExecutedMethods()
    {
        var st = new StackTrace();
        var sf = st.GetFrames();
        return sf.Select(x => x.GetMethod()).ToArray();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string GetCurrentMethod(int index = 1)
    {
        var st = new StackTrace();
        var sf = st.GetFrame(index);
        return sf.GetMethod().Name;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static MethodBase GetCurrentMethodInfo(int index = 1)
    {
        var st = new StackTrace();
        var sf = st.GetFrame(index);
        return sf.GetMethod();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string GetErrorMethod(Exception e, int index = 1)
    {
        var st = new StackTrace(e);
        var sf = st.GetFrame(index);
        return sf.GetMethod().Name;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static MethodBase GetErrorMethodInfo(Exception e, int index = 1)
    {
        var st = new StackTrace(e);
        var sf = st.GetFrame(index);
        return sf.GetMethod();
    }
}