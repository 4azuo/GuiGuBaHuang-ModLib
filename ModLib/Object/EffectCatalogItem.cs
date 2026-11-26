using System.Collections.Generic;
using System.Linq;

namespace ModLib.Object
{
    /// <summary>
    /// One visual effect of the game in an effect catalog (<see cref="EffectCatalog"/>: a JSON list a mod makes from
    /// docs/Effects of ModLib - names, notes and tags of the effect viewer, measures of the effect galleries).
    /// </summary>
    public class EffectCatalogItem
    {
        /// <summary>Skill, Unit or Scenes (folder Effect\Battle\&lt;Kind&gt;).</summary>
        public string Kind { get; set; }
        public string Name { get; set; }
        /// <summary>Resource path of the prefab ("Effect\Battle\Skill\Aixin").</summary>
        public string Path { get; set; }
        /// <summary>What it shows (English, from the effect viewer labels).</summary>
        public string Note { get; set; }
        /// <summary>Attributes it shows (fire, frost, blade...: TextIconHelper.AttributeIcon; "none": no attribute).</summary>
        public List<string> Element { get; set; } = new List<string>();
        public List<string> Usage { get; set; } = new List<string>();
        public List<string> Tags { get; set; } = new List<string>();
        /// <summary>Ideas of skills using the effect.</summary>
        public List<string> Ideas { get; set; } = new List<string>();
        /// <summary>How long it shows (-1: unknown or looping).</summary>
        public int DurationMs { get; set; } = -1;
        public bool Loop { get; set; }
        /// <summary>small, medium, large, huge (empty: unknown).</summary>
        public string Size { get; set; }
        /// <summary>Visible size in world units (0: unknown).</summary>
        public float Width { get; set; }
        public float Height { get; set; }
        /// <summary>Share of the screen the effect covers (0-1; -1: unknown).</summary>
        public float Coverage { get; set; } = -1;

        /// <summary>The attributes of the effect, "none" left out.</summary>
        public List<string> Attributes() => (Element ?? new List<string>()).Where(x => x != "none").ToList();

        /// <summary>How long the effect is shown once (seconds): its measured duration within 0.5-6, 2 s when looping/unknown.</summary>
        public float ShowSec()
        {
            if (Loop || DurationMs <= 0) return 2f;
            return System.Math.Min(6f, System.Math.Max(0.5f, DurationMs / 1000f));
        }
    }
}
