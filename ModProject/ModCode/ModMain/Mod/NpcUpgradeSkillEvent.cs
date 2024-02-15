using MOD_JhUKQ7.Const;
using MOD_JhUKQ7.Enum;
using ModLib.Enum;
using ModLib.Mod;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MOD_JhUKQ7.Mod
{
    [Cache(ModConst.NPC_UPGRADE_SKILL_EVENT_KEY)]
    public sealed class NpcUpgradeSkillEvent : ModEvent
    {
        public static readonly IDictionary<MartialType, float> EXP_RATIO = new Dictionary<MartialType, float>
        {
            [MartialType.SkillLeft] = 1.0f,
            [MartialType.SkillRight] = 0.9f,
            [MartialType.Step] = 0.8f,
            [MartialType.Ultimate] = 0.5f,
            [MartialType.Ability] = 1.2f,
        };

        public override void OnMonthly()
        {
            foreach (var wunit in g.world.unit.GetUnits())
            {
                if (!wunit.IsPlayer())
                {
                    var luck = wunit.GetDynProperty(UnitDynPropertyEnum.Luck).value;
                    UpgradeMartial(wunit, luck / 100);
                }
            }
        }

        public void UpgradeMartial(WorldUnitBase wunit, float ratio)
        {
            var r = CommonTool.Random(0.00f, 100.00f);
            if (ValueHelper.IsBetween(r, 0.00f * ratio, 10.00f * ratio))
            {
                UpgradeMartial(wunit, MartialType.SkillLeft);
            }
            else if (ValueHelper.IsBetween(r, 10.00f * ratio, 20.00f * ratio))
            {
                UpgradeMartial(wunit, MartialType.SkillRight);
            }
            else if (ValueHelper.IsBetween(r, 20.00f * ratio, 35.00f * ratio))
            {
                UpgradeMartial(wunit, MartialType.Step);
            }
            else if (ValueHelper.IsBetween(r, 35.00f * ratio, 50.00f * ratio))
            {
                UpgradeMartial(wunit, MartialType.Ultimate);
            }
            else if (ValueHelper.IsBetween(r, 50.00f * ratio, 70.00f * ratio))
            {
                UpgradeMartial(wunit, MartialType.Ability);
            }
        }

        public void UpgradeMartial(WorldUnitBase wunit, MartialType martialType)
        {
            foreach (var p in wunit.data.unitData.GetActionMartial(martialType))
            {
                //AddMartialExp(wunit, martialType, p);
                UpgradeMartialPrefix(wunit, p);
            }
        }

        //public void AddMartialExp(WorldUnitBase wunit, MartialType martialType, DataUnit.ActionMartialData actMartialData)
        //{
        //    var insight = wunit.GetProperty<int>(UnitPropertyEnum.Talent);
        //    var grade = wunit.GetProperty<int>(UnitPropertyEnum.GradeID);
        //    var mExp = (int)((insight * grade) * EXP_RATIO[martialType]);

        //    actMartialData.exp += mExp;
        //}

        public void UpgradeMartialPrefix(WorldUnitBase wunit, DataUnit.ActionMartialData actMartialData)
        {
            foreach (var prefix in actMartialData.GetPrefixsUnlock())
            {
                var insight = wunit.GetDynProperty(UnitDynPropertyEnum.Talent).value;
                if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, insight / 10))
                {
                    var mInfo = new MartialInfoData(actMartialData.data);
                    var martialData = mInfo.martialData;
                    var maxLvl = g.conf.battleSkillPrefixValue.GetPrefixMaxLevel(martialData, prefix.prefixValueItem);
                    martialData.SetPrefixLevel(prefix.index, g.conf.battleSkillPrefixValue.RandomPrefixLevel(mInfo.level, prefix.prefixValueItem, Math.Min(prefix.prefixLevel + 1, maxLvl)));
                }
            }
        }
    }
}
