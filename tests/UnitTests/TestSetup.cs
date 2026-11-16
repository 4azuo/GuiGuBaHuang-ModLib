using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace UnitTests
{
    /// <summary>
    /// Assembly setup: resolves game assemblies (Il2Cpp proxies, MelonLoader) from the game folder so that
    /// ModLib/Taoist types can be loaded. Tests must not call Il2Cpp members: they need the running game.
    /// </summary>
    [TestClass]
    public static class TestSetup
    {
        /// <summary>&lt;game&gt;\MelonLoader\Managed (from the build).</summary>
        public static string GameManaged => Metadata("GameManaged");

        /// <summary>Repository root (from the build).</summary>
        public static string RepoRoot => Metadata("RepoRoot");

        /// <summary>Path relative to the repository root.</summary>
        public static string Repo(params string[] parts) => Path.Combine(new[] { RepoRoot }.Concat(parts).ToArray());

        private static string Metadata(string key) =>
            typeof(TestSetup).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().First(x => x.Key == key).Value;

        [AssemblyInitialize]
        public static void Init(TestContext _)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                var name = new AssemblyName(e.Name).Name + ".dll";
                foreach (var dir in new[] { GameManaged, Path.GetDirectoryName(GameManaged) })
                {
                    var file = Path.Combine(dir, name);
                    if (File.Exists(file))
                        return Assembly.LoadFrom(file);
                }
                return null;
            };
        }
    }
}
