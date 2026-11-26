using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using EBattleTypeData;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using ModLib.Attributes;
using ModLib.Helper;

namespace ModLib.Mod
{
    /// <summary>
    /// Built-in custom skill host (Local, order 80). Discovers ModSkill classes of all mods, activates them at battle start
    /// and forwards skill/step/prop usage to every skill's OnCast.
    /// </summary>
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
        /// <summary>
        /// Finds ModSkill types (with their SkillAttribute) in an assembly, or in ModLib and all loaded mods.
        /// </summary>
        public List<Tuple<string, SkillAttribute, Type>> GetSkillTypes(string modId, Assembly ass)
        {
            if (ass == null)
                return new List<Tuple<string, SkillAttribute, Type>>();
            return ass.GetLoadableTypes().Where(x => x.IsClass && x.IsSubclassOf(typeof(ModSkill))).Select(x => Tuple.Create(modId, x.GetCustomAttribute<SkillAttribute>(), x)).ToList();
        }

        /// <summary>
        /// Finds ModSkill types (with their SkillAttribute) in an assembly, or in ModLib and all loaded mods.
        /// </summary>
        public List<Tuple<string, SkillAttribute, Type>> GetSkillTypes()
        {
            var rs = new List<Tuple<string, SkillAttribute, Type>>();
            rs.AddRange(GetSkillTypes(ModMaster.ModObj.ModId, AssemblyHelper.GetModLibAssembly()));
            rs.AddRange(GetSkillTypes(ModMaster.ModObj.ModId, AssemblyHelper.GetModLibMainAssembly()));

            foreach (var mod in g.mod.allModPaths)
            {
                if (g.mod.IsLoadMod(mod.t1) && mod.t1 != ModMaster.ModObj.ModId)
                {
                    var ass = AssemblyHelper.GetModRootAssembly(mod.t1);
                    //licensed mod without a valid key: its skills are not loaded either (LicenseHelper)
                    var child = ass?.GetLoadableTypes().FirstOrDefault(x => x.IsClass && x.IsSubclassOf(typeof(ModChild)));
                    if (child != null && !LicenseHelper.IsUnlocked(mod.t1, child))
                        continue;
                    rs.AddRange(GetSkillTypes(mod.t1, ass));
                }
            }
            return rs;
        }

        /// <summary>
        /// Creates every skill, restoring cached state from the skill cache file when present.
        /// </summary>
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

        /// <summary>
        /// Adds a skill to SkillList (once per CacheId).
        /// </summary>
        public void LoadSkill(ModSkill s)
        {
            if (!SkillList.ContainsKey(s.CacheId))
            {
                DebugHelper.WriteLine($"Load Skill: Mod={s.ModId}, Type={s.GetType().FullName}, CacheId={s.CacheId}");
                SkillList.Add(s.CacheId, s);
            }
        }

        /// <summary>
        /// Loads the skill list when the event is created.
        /// </summary>
        public override void OnLoadClass(bool isNew, string modId, CacheAttribute attr)
        {
            base.OnLoadClass(isNew, modId, attr);
            LoadSkillList();
        }

        /*
         * Start Battle
         */
        /// <summary>
        /// Calls OnLoadBattleStart on each skill and marks it active.
        /// </summary>
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
        
        /// <summary>
        /// Activates skills for the new battle.
        /// </summary>
        public override void OnBattleStart(ETypeData e)
        {
            base.OnBattleStart(e);
            LoadSkillEvents();
        }

        /*
         * End Battle
         */
        /// <summary>
        /// Deactivates skills and lets them save their state (OnUnloadBattleEnd).
        /// </summary>
        public void ClearSkillEvents()
        {
            foreach (var t in ActiveSkillList.ToArray())
            {
                ActiveSkillList.Remove(t);
                t.OnUnloadBattleEnd();
            }
        }

        /// <summary>
        /// Deactivates skills at the end of the battle.
        /// </summary>
        public override void OnBattleEnd(BattleEnd e)
        {
            base.OnBattleEnd(e);
            ClearSkillEvents();
        }

        /*
         * Skill Start
         */
        /// <summary>
        /// Calls OnCast of every skill, one after the other on the main thread (the skills use the game's battle
        /// objects, which are not thread-safe: in parallel they could fail or crash at random).
        /// </summary>
        public void MultiCast(UnitCtrlBase cunit, SkillBase skill, StepBase step, PropItemBase prop)
        {
            foreach (var s in SkillList.ToArray())
                s.Value.OnCast(cunit, skill, step, prop);
        }

        /// <summary>
        /// Forwards a skill cast to the skills.
        /// </summary>
        public override void OnBattleUnitUseSkill(UnitUseSkill e)
        {
            base.OnBattleUnitUseSkill(e);
            MultiCast(e.unit, e.skill, null, null);
        }

        /// <summary>
        /// Forwards a movement step to the skills.
        /// </summary>
        public override void OnBattleUnitUseStep(UnitUseStep e)
        {
            base.OnBattleUnitUseStep(e);
            MultiCast(e.unit, null, e.step, null);
        }

        /// <summary>
        /// Forwards an item use to the skills.
        /// </summary>
        public override void OnBattleUnitUseProp(UnitUseProp e)
        {
            base.OnBattleUnitUseProp(e);
            MultiCast(e.unit, null, null, e.prop);
        }
    }
}
