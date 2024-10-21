using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using static MOD_nE7UL2.Object.InGameStts;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.EXPERT_EVENT)]
    public class ExpertEvent : ModEvent
    {
        public static _ExpertConfigs Configs => ModMain.ModObj.InGameCustomSettings.ExpertConfigs;

        public IDictionary<string, int> ExpertExps { get; set; } = new Dictionary<string, int>();

        public override void OnMonthly()
        {
            base.OnMonthly();
            foreach (var wunit in g.world.unit.GetUnits())
            {
                foreach (var martialType in Configs.AutoSkillExpRates.Keys)
                {
                    if (martialType != MartialType.Ability && wunit.IsPlayer())
                        continue;
                    foreach (var abi in wunit.data.unitData.GetActionMartial(martialType))
                    {
                        AddExpertExp(wunit, abi.data.soleID, Configs.AutoSkillExpRates[martialType]);
                    }
                }

                if (wunit.IsPlayer())
                    continue;
                var artifacts = wunit.GetEquippedArtifacts();
                foreach (var art in artifacts)
                {
                    AddExpertExp(wunit, art.soleID, Configs.AutoArtifactExpRate);
                }
            }
        }

        public override void OnBattleUnitHit(UnitHit e)
        {
            base.OnBattleUnitHit(e);
            var humanData = e.hitData.attackUnit.data.TryCast<UnitDataHuman>();
            if (humanData?.worldUnitData?.unit != null)
            {
                var wunit = humanData.worldUnitData.unit;
                var artifacts = wunit.GetEquippedArtifacts();
                foreach (var artifact in artifacts)
                {
                    var a = artifact.To<DataProps.PropsArtifact>();
                    if (a.durable > 0)
                    {
                        AddExpertExp(wunit, artifact.soleID, Configs.BattleArtifactExpRate);
                    }
                }

                var soleId = e?.hitData?.skillBase?.data?.valueData?.data?.abilityBase?.data?.abilityData?.martialInfo?.propsData?.soleID;
                DebugHelper.WriteLine("4");
                DebugHelper.WriteLine(soleId);
                if (soleId != null)
                {
                    var martialType = e.hitData.skillBase.data.valueData.data.abilityBase.data.abilityData.martialType;
                    DebugHelper.WriteLine(martialType.ToString());
                    AddExpertExp(wunit, soleId, Configs.BattleSkillExpRates[martialType]);
                    DebugHelper.WriteLine("5");
                }
            }
        }

        public static IDictionary<string, int> GetExpertExps()
        {
            var e = EventHelper.GetEvent<ExpertEvent>(ModConst.EXPERT_EVENT);
            if (e.ExpertExps == null)
                e.ExpertExps = new Dictionary<string, int>();
            return e.ExpertExps;
        }

        public static void AddExpertExp(WorldUnitBase wunit, string soleId, float exp)
        {
            var e = GetExpertExps();
            if (!e.ContainsKey(soleId))
            {
                e.Add(soleId, 0);
            }
            var insight = wunit.GetDynProperty(UnitDynPropertyEnum.Talent).value;
            e[soleId] += (exp * 100.0f * CommonTool.Random(1.0f, 2.0f) * (insight / 100.0f)).Parse<int>();
        }

        public static int GetExpertExp(WorldUnitBase wunit, string soleId)
        {
            var e = GetExpertExps();
            if (e.ContainsKey(soleId))
                return e[soleId];
            return 0;
        }

        public static int GetSkillExpertNeedExp(int expertLvl, int propsGrade, int propsLevel)
        {
            if (expertLvl <= 0)
                return 0;
            var r = 1000.0d * propsGrade * (1.0d + 0.2d * propsLevel);
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

        public static float GetArtifactExpertAtkRate(int expertLvl, int propsGrade, int propsLevel)
        {
            if (expertLvl <= 0)
                return 0f;
            var r = 0.200f * propsGrade + 0.040f * propsLevel;
            return expertLvl * r;
        }

        public static float GetArtifactExpertDefRate(int expertLvl, int propsGrade, int propsLevel)
        {
            if (expertLvl <= 0)
                return 0f;
            var r = 0.010f * propsGrade + 0.002f * propsLevel;
            return expertLvl * r;
        }
    }
}
