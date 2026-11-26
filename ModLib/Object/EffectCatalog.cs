using ModLib.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModLib.Object
{
    /// <summary>
    /// A catalog of the visual effects of the game (a JSON list of <see cref="EffectCatalogItem"/> in the ModConf of a mod),
    /// read once at the first use: find an effect by its path, search by kind and words.
    /// <c>new EffectCatalog(modId, "x_effectcatalog.json")</c>, or <c>new EffectCatalog(items)</c> (tests).
    /// </summary>
    public class EffectCatalog
    {
        public static readonly string[] KINDS = { "Skill", "Unit", "Scenes" };

        private readonly string _modId;
        private readonly string _file;
        private List<EffectCatalogItem> _all;
        private Dictionary<string, EffectCatalogItem> _byPath;

        public EffectCatalog(string modId, string file)
        {
            _modId = modId;
            _file = file;
        }

        public EffectCatalog(IEnumerable<EffectCatalogItem> items)
        {
            _all = (items ?? Enumerable.Empty<EffectCatalogItem>()).ToList();
        }

        /// <summary>Every effect of the catalog (empty when the file is missing or broken: logged).</summary>
        public List<EffectCatalogItem> All => _all ?? (_all = ConfHelper.ReadConfJson(_modId, _file, new List<EffectCatalogItem>()));

        /// <summary>A path with '\' and no spaces around.</summary>
        public static string NormPath(string path) => path?.Replace('/', '\\').Trim();

        /// <summary>The effect of a path ('/' or '\', any case); null when not in the catalog.</summary>
        public EffectCatalogItem Find(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (_byPath == null)
            {
                _byPath = new Dictionary<string, EffectCatalogItem>(StringComparer.OrdinalIgnoreCase);
                foreach (var e in All) if (!string.IsNullOrEmpty(e?.Path)) _byPath[NormPath(e.Path)] = e;
            }
            return _byPath.TryGetValue(NormPath(path), out var r) ? r : null;
        }

        /// <summary>
        /// Effects of a kind (null or empty: all) whose name, note, tags, elements, usages or skill ideas hold every word
        /// of <paramref name="text"/> (any case).
        /// </summary>
        public List<EffectCatalogItem> Search(string kind, string text)
        {
            var words = (text ?? "").ToLowerInvariant().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            return All.Where(x => string.IsNullOrEmpty(kind) || x.Kind == kind)
                .Where(x => words.Length == 0 || words.All(w => Haystack(x).Contains(w)))
                .ToList();
        }

        private static string Haystack(EffectCatalogItem e) =>
            string.Join(" ", new[] { e.Name, e.Note }.Concat(e.Element ?? new List<string>()).Concat(e.Usage ?? new List<string>())
                .Concat(e.Tags ?? new List<string>()).Concat(e.Ideas ?? new List<string>())).ToLowerInvariant();
    }
}
