using ModLib.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModLib.Object
{
    /// <summary>
    /// Tags of the skill icons of the game (icon name -> the attributes / weapons the picture shows: fire, frost, blade...),
    /// a JSON file of a mod made from docs/GameAssets/SkillIcons/tags.json of ModLib: the icons that suit an effect.
    /// <c>new SkillIconTags(modId, "x_icontags.json")</c>, or <c>new SkillIconTags(dictionary)</c> (tests).
    /// </summary>
    public class SkillIconTags
    {
        private readonly string _modId;
        private readonly string _file;
        private Dictionary<string, List<string>> _tags;

        public SkillIconTags(string modId, string file)
        {
            _modId = modId;
            _file = file;
        }

        public SkillIconTags(Dictionary<string, List<string>> tags)
        {
            _tags = tags ?? new Dictionary<string, List<string>>();
        }

        /// <summary>Icon name -> tags (empty when the file is missing or broken: logged).</summary>
        public Dictionary<string, List<string>> Tags => _tags ?? (_tags = ConfHelper.ReadConfJson(_modId, _file, new Dictionary<string, List<string>>()));

        /// <summary>How well an icon suits: the wanted tags it has (0: none, or an icon without tags).</summary>
        public int Score(string icon, IEnumerable<string> wanted)
        {
            if (string.IsNullOrEmpty(icon) || wanted == null || !Tags.TryGetValue(icon, out var t) || t == null) return 0;
            return wanted.Distinct().Count(t.Contains);
        }

        /// <summary>
        /// The ids whose icon suits the wanted tags, the best first (<paramref name="iconOf"/>: id -> icon name); all of them
        /// when none suits (nothing wanted, or no such icon).
        /// </summary>
        public List<int> Suited(IList<int> ids, IEnumerable<string> wanted, Func<int, string> iconOf)
        {
            var w = (wanted ?? Enumerable.Empty<string>()).ToList();
            var scored = ids.Select(b => new { b, s = Score(iconOf(b), w) }).ToList();
            var best = scored.Where(x => x.s > 0).OrderByDescending(x => x.s).Select(x => x.b).ToList();
            return best.Count > 0 ? best : ids.ToList();
        }
    }
}
