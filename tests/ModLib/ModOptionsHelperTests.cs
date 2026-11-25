using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModLib.Helper;
using ModLib.Object;
using System.Collections.Generic;

namespace UnitTests.ModLib
{
    /// <summary>ModOptionsHelper: options of a mod (schema, values clamped, applied to the settings object by dotted path).</summary>
    [TestClass]
    public class ModOptionsHelperTests
    {
        public class Inner { public float Rate { get; set; } = 0.5f; public int Count { get; set; } = 3; public bool On { get; set; } }
        public class Settings { public Inner Battle { get; set; } = new Inner(); public int Mode { get; set; } }

        private const string SCHEMA = @"{ ""Title"": ""t"", ""Options"": [
            { ""Key"": ""Battle.Rate"", ""Type"": ""float"", ""Min"": 0.1, ""Max"": 2, ""Step"": 0.05 },
            { ""Key"": ""Battle.Count"", ""Type"": ""int"", ""Min"": 1, ""Max"": 9, ""Step"": 1 },
            { ""Key"": ""Battle.On"", ""Type"": ""bool"" },
            { ""Key"": ""Mode"", ""Type"": ""select"", ""Choices"": [ ""a"", ""b"", ""c"" ] },
            { ""Key"": ""Battle.Missing"", ""Type"": ""int"", ""Min"": 0, ""Max"": 5 } ] }";

        [TestMethod, Description("Clamp: within the bounds, rounded to the step; int / select whole; bool parsed; null: the default")]
        public void Clamp_BoundsStepTypes()
        {
            var s = ModOptionsHelper.ParseSchema(SCHEMA);
            Check.Eq(5, s.Options.Count, "options read");
            Check.Eq(2.0, (double)ModOptionsHelper.Clamp(s.Options[0], 9.0), 1e-9, "above the max: the max");
            Check.Eq(0.75, (double)ModOptionsHelper.Clamp(s.Options[0], 0.76), 1e-9, "rounded to the step 0.05");
            Check.Eq(4, (int)ModOptionsHelper.Clamp(s.Options[1], 3.6), 0, "int rounded");
            Check.Eq(true, (bool)ModOptionsHelper.Clamp(s.Options[2], "true"), "bool from text");
            Check.Eq(2, (int)ModOptionsHelper.Clamp(s.Options[3], 7), 0, "select: the last choice at most");
        }

        [TestMethod, Description("Apply: the values set by dotted path within their bounds; unknown keys / missing properties ignored; Current reads them back")]
        public void Apply_And_Current()
        {
            var schema = ModOptionsHelper.ParseSchema(SCHEMA);
            var settings = new Settings();
            var values = ModOptionsHelper.ParseValues(@"{ ""Battle.Rate"": 5, ""Battle.Count"": 2, ""Mode"": 1, ""Unknown"": 3, ""Battle.Missing"": 1 }");
            Check.Eq(3, ModOptionsHelper.Apply(settings, schema, values), 0, "Rate, Count, Mode set (Missing: no such property)");
            Check.Eq(2f, settings.Battle.Rate, 1e-6, "clamped to its max");
            Check.Eq(2, settings.Battle.Count, 0, "count");
            Check.Eq(1, settings.Mode, 0, "select");
            Check.Eq(false, settings.Battle.On, "not in the values: unchanged");
            var current = ModOptionsHelper.Current(settings, schema);
            Check.Eq(2.0, System.Convert.ToDouble(current["Battle.Rate"]), 1e-9, "read back");
            Check.True(!current.ContainsKey("Unknown"), "only the options of the schema");
            Check.Eq(0, ModOptionsHelper.Apply(null, schema, new Dictionary<string, object>()), 0, "no settings: nothing");
        }
    }
}
