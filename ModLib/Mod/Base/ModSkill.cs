using ModLib.Attributes;
using Newtonsoft.Json;
using System.IO;
using ModLib.Helper;

namespace ModLib.Mod
{
    // [TraceIgnore]
    /// <summary>
    /// Base class of a custom skill hosted by ModSkillEvent (declare with [Skill("id")]).
    /// </summary>
    public abstract class ModSkill
    {
        public string ModId { get; set; }
        public string CacheId { get; set; }
        public bool IsCached { get; set; }

        /// <summary>
        /// Initializes ids from the SkillAttribute.
        /// </summary>
        public virtual void OnLoadClass(string modId, SkillAttribute attr)
        {
            ModId = modId;
            CacheId = attr.CacheId;
            IsCached = attr.IsCached;
        }

        /// <summary>
        /// Called when a battle starts.
        /// </summary>
        public virtual void OnLoadBattleStart() { }

        /// <summary>
        /// Called when a battle ends; saves the skill state when IsCached.
        /// </summary>
        public virtual void OnUnloadBattleEnd()
        {
            if (IsCached)
            {
                File.WriteAllText(CacheHelper.GetSkillCacheFilePath(ModId, CacheId), JsonConvert.SerializeObject(this));
            }
        }

        /// <summary>
        /// Called when any unit uses a skill, step or prop (only one of the three is not null).
        /// </summary>
        public virtual void OnCast(UnitCtrlBase cunit, SkillBase skill, StepBase step, PropItemBase prop) { }
    }
}
