using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using ModLib.Object;
using static CacheMgr;
using EBattleTypeData;
using Newtonsoft.Json;

namespace ModLib.Mod
{
    [Cache("$SKILL$", OrderIndex = 80)]
    public class ModSkillEvent : ModEvent
    {
        [JsonIgnore]
        public List<ModSkill> SkillList { get; } = new List<ModSkill>();
        [JsonIgnore]
        public List<ModSkill> ActiveSkillList { get; } = new List<ModSkill>();

        public List<KeyValuePair<string, Type>> GetSkillTypes(string modId, Assembly ass)
        {
            if (ass == null)
                return new List<KeyValuePair<string, Type>>();
            return ass.GetTypes().Where(x => x.IsClass && x.IsSubclassOf(typeof(ModSkill))).Select(x => new KeyValuePair<string, Type>(modId, x)).ToList();
        }

        public List<KeyValuePair<string, Type>> GetSkillTypes(bool includeInactive = false)
        {
            var rs = new List<KeyValuePair<string, Type>>();
            rs.AddRange(GetSkillTypes(ModMaster.ModObj.ModId, GameHelper.GetModLibAssembly()));
            rs.AddRange(GetSkillTypes(ModMaster.ModObj.ModId, GameHelper.GetModLibMainAssembly()));

            foreach (var mod in g.mod.allModPaths)
            {
                if (g.mod.IsLoadMod(mod.t1) || includeInactive)
                {
                    rs.AddRange(GetSkillTypes(mod.t1, GameHelper.GetModChildAssembly(mod.t1)));
                }
            }
            return rs;
        }

        public void LoadSkillList()
        {
            var cacheAttr = this.GetType().GetCustomAttribute<CacheAttribute>();
            foreach (var t in GetSkillTypes())
            {
                var attr = t.Value.GetCustomAttribute<SkillAttribute>();
                if (attr != null)
                {
                    var e = (ModSkill)Activator.CreateInstance(t.Value);
                    DebugHelper.WriteLine($"Load Skill: Mod={t.Key}, Type={t.Value.FullName}, Id={cacheAttr.CacheId}");
                    //e.OnLoadClass(true, t.Key, cacheAttr);
                    SkillList.Add(e);
                }
            }
        }

        public override void OnLoadClass(bool isNew, string modId, CacheAttribute attr)
        {
            base.OnLoadClass(isNew, modId, attr);
            LoadSkillList();
        }

        public void LoadSkillEvents()
        {
            ActiveSkillList.Clear();
            foreach (var t in SkillList)
            {
                if (!ActiveSkillList.Contains(t))
                {
                    ActiveSkillList.Add(t);
                    //t.
                }
            }
        }
        
        public override void OnBattleStart(ETypeData e)
        {
            base.OnBattleStart(e);
            LoadSkillEvents();
        }

        public void ClearSkillEvents()
        {
            ActiveSkillList.Clear();
        }

        public override void OnBattleEnd(BattleEnd e)
        {
            base.OnBattleEnd(e);
            ClearSkillEvents();
        }
    }
}
