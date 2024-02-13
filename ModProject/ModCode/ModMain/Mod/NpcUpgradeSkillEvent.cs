using MOD_JhUKQ7.Const;
using MOD_JhUKQ7.Enum;
using ModLib.Enum;
using ModLib.Mod;
using System;
using System.Linq;

namespace MOD_JhUKQ7.Mod
{
    [Cache(ModConst.NPC_UPGRADE_SKILL_EVENT_KEY)]
    public sealed class NpcUpgradeSkillEvent : ModEvent
    {
        private const float SKILL_LEFT_RATIO = 1.0f;
        private const float SKILL_RIGHT_RATIO = 0.9f;
        private const float SKILL_STEP_RATIO = 0.8f;
        private const float SKILL_ULTIMATE_RATIO = 0.5f;
        private const float SKILL_ABILITY_RATIO = 1.2f;

        public override void OnMonthly()
        {
            foreach (var wunit in g.world.unit.GetUnits())
            {
                if (!wunit.IsPlayer())
                {
                    var luck = wunit.GetProperty<int>(UnitPropertyEnum.Luck);
                    UpgradeMartial(wunit, luck / 100);
                }
            }
        }

        public void UpgradeMartial(WorldUnitBase wunit, float ratio)
        {
            var r = CommonTool.Random(0.00f, 100.00f);
            if (ValueHelper.IsBetween(r, 0.00f * ratio, 10.00f * ratio))
            {
                UpgradeMartial(wunit, MartialType.SkillLeft, SKILL_LEFT_RATIO);
            }
            else if (ValueHelper.IsBetween(r, 10.00f * ratio, 20.00f * ratio))
            {
                UpgradeMartial(wunit, MartialType.SkillRight, SKILL_RIGHT_RATIO);
            }
            else if (ValueHelper.IsBetween(r, 20.00f * ratio, 35.00f * ratio))
            {
                UpgradeMartial(wunit, MartialType.Step, SKILL_STEP_RATIO);
            }
            else if (ValueHelper.IsBetween(r, 35.00f * ratio, 50.00f * ratio))
            {
                UpgradeMartial(wunit, MartialType.Ultimate, SKILL_ULTIMATE_RATIO);
            }
            else if (ValueHelper.IsBetween(r, 50.00f * ratio, 70.00f * ratio))
            {
                UpgradeMartial(wunit, MartialType.Ability, SKILL_ABILITY_RATIO);
            }
        }

        public void UpgradeMartial(WorldUnitBase wunit, MartialType martialType, float ratio)
        {
            foreach (var p in wunit.data.unitData.GetActionMartial(martialType))
            {
                AddMartialExp(wunit, p, ratio);
                UpgradeMartialPrefix(wunit, p);
            }
        }

        public void AddMartialExp(WorldUnitBase wunit, DataUnit.ActionMartialData actMartialData, float ratio)
        {
            var insight = wunit.GetProperty<int>(UnitPropertyEnum.Talent);
            var grade = wunit.GetProperty<int>(UnitPropertyEnum.GradeID);
            var mExp = (int)((insight * grade) * ratio);

            actMartialData.exp += mExp;
        }

        public void UpgradeMartialPrefix(WorldUnitBase wunit, DataUnit.ActionMartialData actMartialData)
        {
            var martialData = new DataProps.MartialData(actMartialData.Pointer);
            var insight = wunit.GetProperty<int>(UnitPropertyEnum.Talent);

            foreach (var prefix in actMartialData.GetPrefixsUnlock())
            {
                if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, insight / 10))
                {
                    var maxLvl = g.conf.battleSkillPrefixValue.GetPrefixMaxLevel(martialData, prefix.prefixValueItem);
                    martialData.SetPrefixLevel(prefix.index, g.conf.battleSkillPrefixValue.RandomPrefixLevel(martialData.martialInfo.level, prefix.prefixValueItem, Math.Min(prefix.prefixLevel + 1, maxLvl)));
                }
            }
        }
    }
}
