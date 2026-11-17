using Newtonsoft.Json;
using System.Collections.Generic;

namespace MOD_nE7UL2.Story
{
    /// <summary>
    /// Saved state of one storyline (StoryEvent.Lines): roles cast, act / step in progress, flags of the choices made,
    /// ending reached. See docs/Taoist/06-Storylines-Draft.md.
    /// </summary>
    public class StoryState
    {
        public string Id { get; set; }
        /// <summary>The line is running (cast, act 0 started).</summary>
        public bool Open { get; set; }
        /// <summary>The line is over (ending reached or closed); it never opens again.</summary>
        public bool Closed { get; set; }
        public string Ending { get; set; }
        public int Act { get; set; }
        /// <summary>Step of the act (1-based).</summary>
        public int Step { get; set; } = 1;
        /// <summary>Month the act started (time limits).</summary>
        public int ActStart { get; set; }
        /// <summary>The step runs from this month on.</summary>
        public int NextMonth { get; set; }
        /// <summary>The step runs as soon as the player is free (no drama, no battle), not only at the month tick.</summary>
        public bool Soon { get; set; }
        /// <summary>Role -> unit id.</summary>
        public Dictionary<string, string> Roles { get; set; } = new Dictionary<string, string>();
        /// <summary>Choices and results (flag -> value).</summary>
        public Dictionary<string, int> Flags { get; set; } = new Dictionary<string, int>();
        /// <summary>Month since the line waits for an NPC to cast (-1: not waiting).</summary>
        public int WaitingSince { get; set; } = -1;
        /// <summary>Battle in progress for the line (tag), null when none.</summary>
        public string Battle { get; set; }
        /// <summary>Month of the last yearly perk of the ending.</summary>
        public int LastPerkMonth { get; set; }

        [JsonIgnore]
        public string Key => $"{Id}{Act}";

        public int Flag(string name) => Flags.TryGetValue(name, out var v) ? v : 0;
        public bool Has(string name) => Flags.ContainsKey(name);
        public void Set(string name, int value = 1) => Flags[name] = value;
    }
}
