﻿using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using EBattleTypeData;
using Newtonsoft.Json;

namespace ModLib.Mod
{
    [Cache("$SKILL$", OrderIndex = 80, CacheType = CacheAttribute.CType.Local, WorkOn = CacheAttribute.WType.Local)]
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
            return ass.GetLoadableTypes().Where(x => x.IsClass && x.IsSubclassOf(typeof(ModSkill))).Select(x => new KeyValuePair<string, Type>(modId, x)).ToList();
        }

        public List<KeyValuePair<string, Type>> GetSkillTypes()
        {
            var rs = new List<KeyValuePair<string, Type>>();
            rs.AddRange(GetSkillTypes(ModMaster.ModObj.ModId, AssemblyHelper.GetModLibAssembly()));
            rs.AddRange(GetSkillTypes(ModMaster.ModObj.ModId, AssemblyHelper.GetModLibMainAssembly()));

            foreach (var mod in g.mod.allModPaths)
            {
                if (g.mod.IsLoadMod(mod.t1) && mod.t1 != ModMaster.ModObj.ModId)
                {
                    rs.AddRange(GetSkillTypes(mod.t1, AssemblyHelper.GetModRootAssembly(mod.t1)));
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
