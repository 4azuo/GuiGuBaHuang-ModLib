using System.Collections.Generic;

namespace ModLib.Object
{
    /// <summary>
    /// Options of a mod that a player may change (ModConf/&lt;prefix&gt;_options.json): each one a value of the mod's
    /// settings (dotted path), its kind, bounds and texts (localText keys). A settings window (Taoist S&amp;M Configs) shows
    /// a page per mod (ModOptionsPage); without one the player edits the values file (ModOptionsHelper.ValuesFilePath).
    /// </summary>
    public class ModOptions
    {
        /// <summary>Title of the page (localText key).</summary>
        public string Title { get; set; }
        /// <summary>A line under the title (localText key, optional).</summary>
        public string Note { get; set; }
        public List<ModOption> Options { get; set; } = new List<ModOption>();
    }

    /// <summary>One option: Key = dotted path of a property of the mod's settings ("SkillForge.DamageSkillRight").</summary>
    public class ModOption
    {
        public string Key { get; set; }
        /// <summary>float, int, bool or select (Choices: localText keys, the value is the index).</summary>
        public string Type { get; set; } = "float";
        public double Min { get; set; }
        public double Max { get; set; } = 1;
        public double Step { get; set; }
        public object Default { get; set; }
        public List<string> Choices { get; set; }
        /// <summary>localText keys: the label, a hint after the value, the group (a title over the options of a group).</summary>
        public string Label { get; set; }
        public string Desc { get; set; }
        public string Group { get; set; }
    }
}
