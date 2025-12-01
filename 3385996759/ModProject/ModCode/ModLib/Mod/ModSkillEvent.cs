using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using EBattleTypeData;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using ModLib.Attributes;

namespace ModLib.Mod
{
    [Cache("$SKILL$", OrderIndex = 80, CacheType = CacheAttribute.CType.Local, WorkOn = CacheAttribute.WType.Local)]
    public class ModSkillEvent : ModEvent
    {
        [JsonIgnore]
        public Dictionary<string, ModSkill> SkillList { get; } = new Dictionary<string, ModSkill>();
        [JsonIgnore]
        public List<ModSkill> ActiveSkillList { get; } = new List<ModSkill>();

        /*
         * Load Classes
         */
        public List<Tuple<string, SkillAttribute, Type>> GetSkillTypes(string modId, Assembly ass)
        {
            if (ass == null)
                return new List<Tuple<string, SkillAttribute, Type>>();
            return ass.GetLoadableTypes().Where(x => x.IsClass && x.IsSubclassOf(typeof(ModSkill))).Select(x => Tuple.Create(modId, x.GetCustomAttribute<SkillAttribute>(), x)).ToList();
        }

        public List<Tuple<string, SkillAttribute, Type>> GetSkillTypes()
        {
            var rs = new List<Tuple<string, SkillAttribute, Type>>();
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
            foreach (var t in GetSkillTypes())
            {
                var cacheFile = CacheHelper.GetSkillCacheFilePath(t.Item1, t.Item2.CacheId);
                if (File.Exists(cacheFile))
                {
                    var e = (ModSkill)JsonConvert.DeserializeObject(File.ReadAllText(cacheFile), t.Item3, CacheHelper.JSON_SETTINGS);
                    e.OnLoadClass(t.Item1, t.Item2);
                    LoadSkill(e);
                }
                else
                {
                    if (!SkillList.ContainsKey(t.Item2.CacheId))
                    {
                        var e = (ModSkill)Activator.CreateInstance(t.Item3);
                        e.OnLoadClass(t.Item1, t.Item2);
                        LoadSkill(e);
                    }
                }
            }
        }

        public void LoadSkill(ModSkill s)
        {
            if (!SkillList.ContainsKey(s.CacheId))
            {
                DebugHelper.WriteLine($"Load Skill: Mod={s.ModId}, Type={s.GetType().FullName}, CacheId={s.CacheId}");
                SkillList.Add(s.CacheId, s);
            }
        }

        public override void OnLoadClass(bool isNew, string modId, CacheAttribute attr)
        {
            base.OnLoadClass(isNew, modId, attr);
            LoadSkillList();
        }

        /*
         * Start Battle
         */
        public void LoadSkillEvents()
        {
            ActiveSkillList.Clear();
            foreach (var t in SkillList)
            {
                if (!ActiveSkillList.Contains(t.Value))
                {
                    t.Value.OnLoadBattleStart();
                    ActiveSkillList.Add(t.Value);
                }
            }
        }
        
        public override void OnBattleStart(ETypeData e)
        {
            base.OnBattleStart(e);
            LoadSkillEvents();
        }

        /*
         * End Battle
         */
        public void ClearSkillEvents()
        {
            foreach (var t in ActiveSkillList.ToArray())
            {
                ActiveSkillList.Remove(t);
                t.OnUnloadBattleEnd();
            }
        }

        public override void OnBattleEnd(BattleEnd e)
        {
            base.OnBattleEnd(e);
            ClearSkillEvents();
        }

        /*
         * Skill Start
         */
        public void MultiCast(UnitCtrlBase cunit, SkillBase skill, StepBase step, PropItemBase prop)
        {
            Parallel.ForEach(SkillList, s =>
            {
                s.Value.OnCast(cunit, skill, step, prop);
            });
        }

        public override void OnBattleUnitUseSkill(UnitUseSkill e)
        {
            base.OnBattleUnitUseSkill(e);
            MultiCast(e.unit, e.skill, null, null);
        }

        public override void OnBattleUnitUseStep(UnitUseStep e)
        {
            base.OnBattleUnitUseStep(e);
            MultiCast(e.unit, null, e.step, null);
        }

        public override void OnBattleUnitUseProp(UnitUseProp e)
        {
            base.OnBattleUnitUseProp(e);
            MultiCast(e.unit, null, null, e.prop);
        }
    }
}
