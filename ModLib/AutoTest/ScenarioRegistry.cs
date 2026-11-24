using System;
using System.Collections.Generic;
using System.Linq;
using ModLib.Helper;

namespace ModLib.AutoTest
{
    /// <summary>
    /// A scenario file of a mod: one class per scenario (e.g. AutoTest/Scenarios/&lt;Group&gt;/&lt;Id&gt;_&lt;Feature&gt;.cs).
    /// Ids must match &lt;mod project&gt;/tests/scenarios.json of the mod repository.
    /// </summary>
    public interface IScenarioDefinition
    {
        /// <summary>Scenario id, e.g. "TG-04".</summary>
        string Id { get; }

        /// <summary>Builds a new scenario instance (steps included).</summary>
        Scenario Create();
    }

    /// <summary>
    /// Registry of automatic scenarios: finds every IScenarioDefinition of a mod assembly, so adding a scenario only
    /// means adding a file (and an entry in scenarios.json).
    /// </summary>
    public static class ScenarioRegistry
    {
        /// <summary>
        /// Creates all scenarios of the assemblies ordered by id (a mod: its assembly + ModLib's, the tools of ModLib such
        /// as the effect galleries TE-xx); fails on duplicated or mismatching ids.
        /// </summary>
        public static List<Scenario> Create(params System.Reflection.Assembly[] assemblies)
        {
            var defs = assemblies.Distinct().SelectMany(a => a.GetLoadableTypes())
                .Where(IsScenarioDefinition)
                .Select(t => (IScenarioDefinition)Activator.CreateInstance(t))
                .OrderBy(x => PrefixOrder(x.Id))
                .ThenBy(x => x.Id)
                .ToList();
            var dup = defs.GroupBy(x => x.Id).FirstOrDefault(x => x.Count() > 1);
            if (dup != null)
                throw new InvalidOperationException($"Duplicated scenario id {dup.Key}: {string.Join(", ", dup.Select(x => x.GetType().Name))}");
            var list = new List<Scenario>();
            foreach (var d in defs)
            {
                var s = d.Create();
                if (s.Id != d.Id)
                    throw new InvalidOperationException($"{d.GetType().Name}: Id '{d.Id}' differs from scenario Id '{s.Id}'");
                list.Add(s);
            }
            return list;
        }

        /// <summary>
        /// True for a concrete IScenarioDefinition class. A type whose base types cannot be resolved (outside the game, the
        /// Il2Cpp proxies of ModLib: BadImageFormatException, TypeLoadException) is not a scenario.
        /// </summary>
        private static bool IsScenarioDefinition(Type t)
        {
            try { return t.IsClass && !t.IsAbstract && typeof(IScenarioDefinition).IsAssignableFrom(t); }
            catch (Exception) { return false; }
        }

        /// <summary>
        /// Run order of the id prefixes: title, pre-test (new character), game, code-started battles, time-changing,
        /// and TZ (ends the character, e.g. one-life) at the very end.
        /// </summary>
        private static int PrefixOrder(string id)
        {
            var i = Array.IndexOf(new[] { "TT", "TP", "TG", "TB", "TX", "TZ" }, id.Length >= 2 ? id.Substring(0, 2) : id);
            return i < 0 ? 99 : i;
        }
    }
}
