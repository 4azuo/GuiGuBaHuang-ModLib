using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Mod;
using System;
using ModLib.Helper;
using System.Collections.Generic;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// NPC martial arts growth: monthly level-ups, higher grade versions, mastery exp and prefix upgrades.
    /// </summary>
    [Cache(ModConst.NPC_UPGRADE_SKILL_EVENT)]
    public class NpcUpgradeSkillEvent : ModEvent
    {
        public static NpcUpgradeSkillEvent Instance { get; set; }

        public static readonly Dictionary<MartialType, float> EXP_RATIO = new Dictionary<MartialType, float>
        {
            [MartialType.SkillLeft] = 1.0f,
            [MartialType.SkillRight] = 0.9f,
            [MartialType.Step] = 0.8f,
            [MartialType.Ultimate] = 0.5f,
            [MartialType.Ability] = 1.2f,
        };

        /// <summary>
        /// Upgrades a random martial type of each NPC.
        /// </summary>
        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);
            if (!wunit.IsPlayer())
            {
                UpgradeMartial(wunit);
            }
        }

        /// <summary>
        /// Upgrades the martial arts of a type (chance from talent and luck).
        /// </summary>
        public void UpgradeMartial(WorldUnitBase wunit)
        {
            var luck = wunit.GetDynProperty(UnitDynPropertyEnum.Luck).value;
            UpgradeMartial(wunit, ModMain.ModObj.ModSettings.NpcUpgradeSkillConfigs.RandomUpgradingMartial(CommonTool.Random(0.00f, 100.00f), luck / 100f)); // float: luck / 100 (int) was 0 below 100 luck, NPCs never upgraded (found by autotest TG-44)
        }

        /// <summary>
        /// Upgrades the martial arts of a type (chance from talent and luck).
        /// </summary>
        public void UpgradeMartial(WorldUnitBase wunit, MartialType martialType)
        {
            if (martialType == MartialType.None)
                return;
            var wunitGrade = wunit.GetGradeLvl();
            var insight = wunit.GetDynProperty(UnitDynPropertyEnum.Talent).value;
            var luck = wunit.GetDynProperty(UnitDynPropertyEnum.Luck).value;
            foreach (var p in wunit.data.unitData.GetActionMartial(martialType))
            {
                //upgrade skill
                if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, insight / 1000f + luck / 100f))
                {
                    if (p.data.propsInfoBase.level < 6 && p.data.propsInfoBase.grade <= wunitGrade)
                    {
                        p.data.propsInfoBase.level++;
                        //DebugHelper.WriteLine($"{wunit.GetUnitId()} up level");
                    }
                    else
                    if (p.data.propsInfoBase.grade < wunitGrade && CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, luck / 10f))
                    {
                        var newSkill = wunit.AddUnitProp(martialType, p.data.propsInfoBase.baseID, p.data.propsInfoBase.grade + 1);
                        newSkill.data.propsInfoBase.level = CommonTool.Random(1, 5);
                        //DebugHelper.WriteLine($"{wunit.GetUnitId()} up grade");
                    }
                }
                //add skill exp
                AddMartialExp(wunit, martialType, p);
                //upgrade prefix
                UpgradeMartialPrefix(wunit, p);
            }
        }

        /// <summary>
        /// Adds mastery exp (talent x grade x type ratio).
        /// </summary>
        public void AddMartialExp(WorldUnitBase wunit, MartialType martialType, DataUnit.ActionMartialData actMartialData)
        {
            var insight = wunit.GetProperty<int>(UnitPropertyEnum.Talent);
            var grade = wunit.GetProperty<int>(UnitPropertyEnum.GradeID);
            var mExp = (int)((insight * grade) * EXP_RATIO[martialType]);

            actMartialData.exp += mExp;
        }

        /// <summary>
        /// May raise the level of unlocked prefixes.
        /// </summary>
        public void UpgradeMartialPrefix(WorldUnitBase wunit, DataUnit.ActionMartialData actMartialData)
        {
            var rateDec = 0;
            foreach (var prefix in actMartialData.GetPrefixsUnlock())
            {
                var insight = wunit.GetDynProperty(UnitDynPropertyEnum.Talent).value;
                if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, insight / (10 + rateDec)))
                {
                    var mInfo = new MartialInfoData(actMartialData.data);
                    var martialData = mInfo.martialData;
                    var maxLvl = g.conf.battleSkillPrefixValue.GetPrefixMaxLevel(martialData, prefix.prefixValueItem);
                    if (prefix.prefixLevel < maxLvl)
                    {
                        martialData.SetPrefixLevel(prefix.index, g.conf.battleSkillPrefixValue.RandomPrefixLevel(mInfo.level, prefix.prefixValueItem, Math.Min(prefix.prefixLevel + 1, maxLvl)));
                        rateDec += 2;
                    }
                }
            }
        }
    }
}
