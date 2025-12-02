using ModLib.Attributes;
using Newtonsoft.Json;
using System.IO;
using ModLib.Helper;

namespace ModLib.Mod
{
    // [TraceIgnore]
    public abstract class ModSkill
    {
        public string ModId { get; set; }
        public string CacheId { get; set; }
        public bool IsCached { get; set; }

        public virtual void OnLoadClass(string modId, SkillAttribute attr)
        {
            ModId = modId;
            CacheId = attr.CacheId;
            IsCached = attr.IsCached;
        }

        public virtual void OnLoadBattleStart() { }

        public virtual void OnUnloadBattleEnd()
        {
            if (IsCached)
            {
                File.WriteAllText(CacheHelper.GetSkillCacheFilePath(ModId, CacheId), JsonConvert.SerializeObject(this));
            }
        }

        public virtual void OnCast(UnitCtrlBase cunit, SkillBase skill, StepBase step, PropItemBase prop) { }
    }
}
