using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using static MOD_nE7UL2.Object.InGameStts;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.EXPERT_EVENT)]
    public class ExpertEvent : ModEvent
    {
        public static _SkillExpertConfigs Configs => ModMain.ModObj.InGameCustomSettings.SkillExpertConfigs;

        public IDictionary<string, IDictionary<string, int>> ExpertExps { get; set; } = new Dictionary<string, IDictionary<string, int>>();

        public override void OnMonthly()
        {
            base.OnMonthly();
            foreach (var wunit in g.world.unit.GetUnits())
            {
                foreach (var martialType in Configs.ExpRates.Keys)
                {
                    if (martialType != MartialType.Ability && wunit.IsPlayer())
                    {
                        continue;
                    }
                    foreach (var abi in wunit.data.unitData.GetActionMartial(martialType))
                    {
                        AddAIExpertExp(wunit, abi.data.soleID, Configs.ExpRates[martialType]);
                    }
                }

                var artifacts = wunit.data.unitData.propData.GetEquipProps().ToArray().Where(x => x?.propsItem?.IsArtifact() != null).ToArray();
                foreach (var art in artifacts)
                {
                    AddAIExpertExp(wunit, art.soleID);
                }
            }
        }

        public override void OnBattleUnitUseSkill(UnitUseSkill e)
        {
            base.OnBattleUnitUseSkill(e);
            var wunit = e.unit.data.TryCast<UnitDataHuman>()?.worldUnitData?.unit;
            if (wunit == null)
                return;
            var soleId = e.skill.data.valueData.data.abilityBase.data.abilityData.martialInfo.propsData.soleID;
            var martialType = e.skill.data.valueData.data.abilityBase.data.abilityData.martialType;
            if (martialType == MartialType.SkillLeft)
                AddExpertExp(wunit, soleId, 0.05f);
            else if (martialType == MartialType.SkillRight)
                AddExpertExp(wunit, soleId, 2.0f);
            else if (martialType == MartialType.Ultimate)
                AddExpertExp(wunit, soleId, 1.0f);
        }

        public override void OnBattleUnitUseStep(UnitUseStep e)
        {
            base.OnBattleUnitUseStep(e);
            var wunit = e.unit.data.TryCast<UnitDataHuman>()?.worldUnitData?.unit;
            if (wunit == null)
                return;
            var soleId = e.step.data.valueData.data.abilityBase.data.abilityData.martialInfo.propsData.soleID;
            AddExpertExp(wunit, soleId, 2.0f);
        }

        public override void OnBattleUnitUseProp(UnitUseProp e)
        {
            base.OnBattleUnitUseProp(e);
            var wunit = e.unit.data.TryCast<UnitDataHuman>()?.worldUnitData?.unit;
            if (wunit == null || e.prop.data.propsItem.IsArtifact() == null)
                return;
            var soleId = e.prop.data.propsData.soleID;
            AddExpertExp(wunit, soleId, 1.0f);
        }

        public IDictionary<string, int> GetExpertExp(WorldUnitBase wunit)
        {
            var unitId = wunit.GetUnitId();
            if (!ExpertExps.ContainsKey(unitId))
            {
                ExpertExps.Add(unitId, new Dictionary<string, int>());
            }
            return ExpertExps[unitId];
        }

        public void AddAIExpertExp(WorldUnitBase wunit, string soleId, float r = 1.0f)
        {
            var gradeLvl = wunit.GetGradeLvl();
            AddExpertExp(wunit, soleId, (Math.Pow(2, gradeLvl) * r).Parse<float>());
        }

        public void AddExpertExp(WorldUnitBase wunit, string soleId, float exp)
        {
            var exps = GetExpertExp(wunit);
            var insight = wunit.GetDynProperty(UnitDynPropertyEnum.Talent).value;
            if (!exps.ContainsKey(soleId))
            {
                exps.Add(soleId, 0);
            }
            exps[soleId] += (exp * 10 * CommonTool.Random(0.5f, 2.0f) * (insight / 100.0f)).Parse<int>();
        }

        public static int GetExpertExp(WorldUnitBase wunit, string soleId)
        {
            var e = EventHelper.GetEvent<ExpertEvent>(ModConst.EXPERT_EVENT);
            var exps = e.GetExpertExp(wunit);
            if (exps.ContainsKey(soleId))
                return exps[soleId];
            return 0;
        }

        public static int GetSkillExpertNeedExp(int expertLvl, int propsGrade, int propsLevel)
        {
            if (expertLvl <= 0)
                return 0;
            var r = 100.0d * propsGrade * (1.0d + 0.1d * propsLevel);
            return Convert.ToInt32(Math.Pow(2, expertLvl - 1) * r);
        }

        public static int GetExpertLvl(WorldUnitBase wunit, string soleId, int propsGrade, int propsLevel)
        {
            var exp = GetExpertExp(wunit, soleId);
            for (int i = 1; true; i++)
            {
                if (exp < GetSkillExpertNeedExp(i, propsGrade, propsLevel))
                    return i - 1;
            }
        }
    }
}
