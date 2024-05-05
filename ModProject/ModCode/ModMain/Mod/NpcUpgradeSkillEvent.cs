using MOD_nE7UL2.Const;
using MOD_nE7UL2.Object;
using ModLib.Enum;
using ModLib.Mod;
using System;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.NPC_UPGRADE_SKILL_EVENT)]
    public class NpcUpgradeSkillEvent : ModEvent
    {
        //public static readonly IDictionary<MartialType, float> EXP_RATIO = new Dictionary<MartialType, float>
        //{
        //    [MartialType.SkillLeft] = 1.0f,
        //    [MartialType.SkillRight] = 0.9f,
        //    [MartialType.Step] = 0.8f,
        //    [MartialType.Ultimate] = 0.5f,
        //    [MartialType.Ability] = 1.2f,
        //};

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
            UpgradeMartial(wunit, ModMain.ModObj.InGameCustomSettings.NpcUpgradeSkillConfigs.RandomUpgradingMartial(CommonTool.Random(0.00f, 100.00f), ratio));
        }

        public void UpgradeMartial(WorldUnitBase wunit, MartialType martialType)
        {
            if (martialType == MartialType.None)
                return;
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
                var rateDec = 0;
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
