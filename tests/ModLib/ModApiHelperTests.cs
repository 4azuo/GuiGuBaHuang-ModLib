using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModLib.Helper;
using System;
using System.Collections.Generic;

namespace UnitTests.ModLib
{
    /// <summary>The API of a "mod" for ModApiHelperTests (found by its full name, as another mod would be).</summary>
    public static class SampleModApi
    {
        public const int VERSION = 3;
        public static bool IsReady => true;
        public static string Name = "sample";
        public static int Calls;
        public static int Add(int a, int b) => a + b;
        public static string Echo(string s) => s;
        public static void Touch() => Calls++;
        public static int Fail() => throw new InvalidOperationException("boom");
    }

    /// <summary>ModApiHelper: what a mod offers others by reflection, every call safe.</summary>
    [TestClass]
    public class ModApiHelperTests
    {
        private const string API = "UnitTests.ModLib.SampleModApi";

        [TestMethod, Description("ModApiHelper: finds the API type by full name, its VERSION, members and methods; a missing mod, member or a call that throws gives the fallback (logged)")]
        public void Api_CallsSafely()
        {
            var logged = new List<string>();
            var keep = ModApiHelper.Log;
            ModApiHelper.Log = logged.Add;
            try
            {
                Check.True(ModApiHelper.IsLoaded(API), "found in the loaded assemblies");
                Check.Eq(3, ModApiHelper.VersionOf(API), "VERSION");
                Check.True(ModApiHelper.Has(API, 3) && !ModApiHelper.Has(API, 4), "Has: by version");
                Check.True(ModApiHelper.Get(API, "IsReady", false), "static property");
                Check.Eq("sample", ModApiHelper.Get(API, "Name", ""), "static field");
                Check.Eq(5, ModApiHelper.Call(API, "Add", -1, 2, 3), "method with a result");
                Check.Eq("hi", ModApiHelper.Call(API, "Echo", "", "hi"), "string argument");
                var calls = SampleModApi.Calls;
                Check.True(ModApiHelper.Invoke(API, "Touch"), "method without a result");
                Check.Eq(calls + 1, SampleModApi.Calls, "called once");

                Check.Eq(-1, ModApiHelper.Call(API, "Fail", -1), "a call that throws: fallback");
                Check.True(logged.Count == 1 && logged[0].Contains("boom"), "the error logged");
                Check.Eq(-1, ModApiHelper.Call(API, "Add", -1, 1), "wrong number of arguments: fallback");
                Check.Eq(7, ModApiHelper.Get(API, "Missing", 7), "missing member: fallback");
                Check.Eq(0L + 3, ModApiHelper.Get(API, "VERSION", 0L), "converted to the asked type");

                Check.True(!ModApiHelper.IsLoaded("No.Such.ModApi"), "mod not loaded");
                Check.Eq(0, ModApiHelper.VersionOf("No.Such.ModApi"), "no version");
                Check.Eq("x", ModApiHelper.Call("No.Such.ModApi", "Echo", "x", "y"), "mod not loaded: fallback");
                Check.True(!ModApiHelper.Invoke("No.Such.ModApi", "Touch"), "mod not loaded: not called");
            }
            finally { ModApiHelper.Log = keep; }
        }
    }
}
