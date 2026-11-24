using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace UnitTests
{
    /// <summary>
    /// Assertions that also write what is checked and the values ("OK what: expected X, actual Y") to the test
    /// output. The output is stored in the trx (StdOut) and shown by test-viewer (Unit tests, expand a row).
    /// </summary>
    public static class Check
    {
        /// <summary>Formats a value for the output (lists as [a, b], strings quoted, floats invariant).</summary>
        public static string Fmt(object v)
        {
            switch (v)
            {
                case null: return "null";
                case string s: return "\"" + (s.Length > 200 ? s.Substring(0, 200) + "..." : s) + "\"";
                case IFormattable f: return f.ToString(null, CultureInfo.InvariantCulture);
                case IEnumerable e: var items = e.Cast<object>().ToList(); return "[" + string.Join(", ", items.Take(20).Select(Fmt)) + (items.Count > 20 ? $", ... ({items.Count})" : "") + "]";
                default: return v.ToString();
            }
        }

        private static void Write(bool ok, string text) => Console.WriteLine((ok ? "OK  " : "NG  ") + text);

        /// <summary>Writes an information line (input data of the test).</summary>
        public static void Info(string text) => Console.WriteLine("    " + text);

        /// <summary>expected == actual.</summary>
        public static void Eq<T>(T expected, T actual, string what)
        {
            Write(Equals(expected, actual), $"{what}: expected {Fmt(expected)}, actual {Fmt(actual)}");
            Assert.AreEqual(expected, actual, what);
        }

        /// <summary>|expected - actual| &lt;= delta.</summary>
        public static void Eq(double expected, double actual, double delta, string what)
        {
            Write(Math.Abs(expected - actual) <= delta, $"{what}: expected {Fmt(expected)} (+/- {Fmt(delta)}), actual {Fmt(actual)}");
            Assert.AreEqual(expected, actual, delta, what);
        }

        /// <summary>expected != actual.</summary>
        public static void NotEq<T>(T notExpected, T actual, string what)
        {
            Write(!Equals(notExpected, actual), $"{what}: not {Fmt(notExpected)}, actual {Fmt(actual)}");
            Assert.AreNotEqual(notExpected, actual, what);
        }

        /// <summary>Same instance.</summary>
        public static void Same(object expected, object actual, string what)
        {
            Write(ReferenceEquals(expected, actual), $"{what}: same instance as {Fmt(expected)}, actual {Fmt(actual)}");
            Assert.AreSame(expected, actual, what);
        }

        /// <summary>Condition is true; <paramref name="actual"/> (optional) is the value shown.</summary>
        public static void True(bool condition, string what, object actual = null)
        {
            Write(condition, $"{what}: {(actual != null ? "actual " + Fmt(actual) + " -> " : "")}{condition}");
            Assert.IsTrue(condition, what);
        }

        /// <summary>Condition is false.</summary>
        public static void False(bool condition, string what, object actual = null)
        {
            Write(!condition, $"{what}: {(actual != null ? "actual " + Fmt(actual) + " -> " : "")}{condition} (expected false)");
            Assert.IsFalse(condition, what);
        }

        /// <summary>Value is null.</summary>
        public static void Null(object actual, string what)
        {
            Write(actual == null, $"{what}: expected null, actual {Fmt(actual)}");
            Assert.IsNull(actual, what);
        }

        /// <summary>Same elements in the same order.</summary>
        public static void Seq(ICollection expected, ICollection actual, string what)
        {
            var ok = expected.Cast<object>().SequenceEqual(actual.Cast<object>());
            Write(ok, $"{what}: expected {Fmt(expected)}, actual {Fmt(actual)}");
            CollectionAssert.AreEqual(expected, actual, what);
        }

        /// <summary>Same elements in any order.</summary>
        public static void SetEq(ICollection expected, ICollection actual, string what)
        {
            var e = expected.Cast<object>().ToList(); var a = actual.Cast<object>().ToList();
            var ok = e.Count == a.Count && !e.Except(a).Any() && !a.Except(e).Any();
            Write(ok, $"{what}: {e.Count} expected, {a.Count} actual" + (ok ? "" : $"; only expected {Fmt(e.Except(a).ToList())}, only actual {Fmt(a.Except(e).ToList())}"));
            CollectionAssert.AreEquivalent(expected, actual, what);
        }

        /// <summary>Text contains a part.</summary>
        public static void Contains(string text, string part, string what)
        {
            Write(text != null && text.Contains(part), $"{what}: {Fmt(text)} contains {Fmt(part)}");
            StringAssert.Contains(text, part, what);
        }

        /// <summary>A list of problems is empty (each problem is written).</summary>
        public static void None(System.Collections.Generic.IList<string> problems, string what, int checkedCount = -1)
        {
            Write(problems.Count == 0, $"{what}: {problems.Count} problem(s){(checkedCount >= 0 ? $" in {checkedCount} checked" : "")}");
            foreach (var p in problems.Take(50))
                Console.WriteLine("      - " + p);
            Assert.AreEqual(0, problems.Count, what + ": " + string.Join(Environment.NewLine, problems));
        }
    }
}
