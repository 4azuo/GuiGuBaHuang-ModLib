using ModLib.Attributes;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for tracing method execution and call stacks.
    /// Provides utilities for debugging and tracking method calls.
    /// </summary>
    [ActionCatIgn]
    public static class TraceHelper
    {
        /// <summary>
        /// Gets all methods in the current call stack.
        /// </summary>
        /// <returns>Array of methods</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static MethodBase[] GetExecutedMethods()
        {
            var st = new StackTrace();
            var sf = st.GetFrames();
            return sf.Select(x => x.GetMethod()).ToArray();
        }

        /// <summary>
        /// Gets the name of the calling method.
        /// </summary>
        /// <param name="index">Stack frame index (1 = caller)</param>
        /// <returns>Method name</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetCurrentMethod(int index = 1)
        {
            var st = new StackTrace();
            var sf = st.GetFrame(index);
            return sf.GetMethod().Name;
        }

        /// <summary>
        /// Gets MethodBase of the calling method.
        /// </summary>
        /// <param name="index">Stack frame index (1 = caller)</param>
        /// <returns>MethodBase</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static MethodBase GetCurrentMethodInfo(int index = 1)
        {
            var st = new StackTrace();
            var sf = st.GetFrame(index);
            return sf.GetMethod();
        }

        /// <summary>
        /// Gets the name of method that threw exception.
        /// </summary>
        /// <param name="e">Exception</param>
        /// <param name="index">Stack frame index</param>
        /// <returns>Method name</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetErrorMethod(Exception e, int index = 1)
        {
            var st = new StackTrace(e);
            var sf = st.GetFrame(index);
            return sf.GetMethod().Name;
        }

        /// <summary>
        /// Gets MethodBase of method that threw exception.
        /// </summary>
        /// <param name="e">Exception</param>
        /// <param name="index">Stack frame index</param>
        /// <returns>MethodBase</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static MethodBase GetErrorMethodInfo(Exception e, int index = 1)
        {
            var st = new StackTrace(e);
            var sf = st.GetFrame(index);
            return sf.GetMethod();
        }
    }
}