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
                foreach (var martial in wunit.GetActionMartial())
                {
                    var martialData = martial.Value.data.To<DataProps.MartialData>();
                    if (wunit.data.unitData.abilitys.Contains(martialData.data.soleID))
                    {
                        var martialType = martialData.martialType;
                        if (martialType != MartialType.Ability && wunit.IsPlayer())
                            continue;
                        AddExpertExp(wunit, martialData.data.soleID, Configs.SkillExpRatios[martialType]);
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

                var skill = e?.hitData?.skillBase?.TryCast<SkillAttack>();
                if (skill != null && skill?.skillData?.martialType == MartialType.SkillLeft)
                {
                    var soleId = skill.skillData.data.soleID;
                    AddExpertExp(wunit, soleId, Configs.SkillExpRatios[MartialType.SkillLeft]);
                }
            }
        }

        public override void OnBattleUnitUseSkill(UnitUseSkill e)
        {
            base.OnBattleUnitUseSkill(e);
            var humanData = e.unit.data.TryCast<UnitDataHuman>();
            if (humanData?.worldUnitData?.unit != null)
            {
                var wunit = humanData.worldUnitData.unit;
                var skill = e?.skill?.TryCast<SkillAttack>();
                if (skill != null && skill?.skillData?.martialType == MartialType.SkillRight)
                {
                    var soleId = skill.skillData.data.soleID;
                    AddExpertExp(wunit, soleId, Configs.SkillExpRatios[MartialType.SkillRight]);
                }
            }
        }

        private int oldX;
        private int oldY;
        public override void OnTimeUpdate500ms()
        {
            base.OnTimeUpdate500ms();
            var wunit = g.world.playerUnit;
            var curX = wunit.data.unitData.pointX;
            var curY = wunit.data.unitData.pointY;
            if (oldX != curX || oldY != curY)
            {
                var step = wunit.GetMartialStep();
                if (step != null)
                {
                    AddExpertExp(wunit, step.data.soleID, Configs.SkillExpRatios[MartialType.Step]);
                }
                oldX = curX;
                oldY = curY;
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
                e.Add(soleId, 0);
            var insight = wunit.GetDynProperty(UnitDynPropertyEnum.Talent).value;
            e[soleId] += (exp * 100.0f * wunit.GetGradeLvl() * CommonTool.Random(1.0f, 2.0f) * (insight / 100.0f)).Parse<int>();
        }

        public static int GetExpertExp(string soleId)
        {
            var e = GetExpertExps();
            if (e.ContainsKey(soleId))
                return e[soleId];
            return 0;
        }

        public static int GetExpertNeedExp(int expertLvl, int propsGrade, int propsLevel)
        {
            if (expertLvl <= 0)
                return 0;
            var r = 1000.0d * propsGrade * (1.0d + 0.2d * propsLevel);
            return Convert.ToInt32(Math.Pow(2, expertLvl - 1) * r);
        }

        public static int GetExpertLvl(string soleId, int propsGrade, int propsLevel)
        {
            var exp = GetExpertExp(soleId);
            for (int i = 1; true; i++)
            {
                if (exp < GetExpertNeedExp(i, propsGrade, propsLevel))
                    return i - 1;
            }
        }

        public static int GetArtifactExpertAtk(int inputValue, int expertLvl, int propsGrade, int propsLevel)
        {
            if (expertLvl <= 0)
                return 0;
            var r = 0.010f * propsGrade + 0.002f * propsLevel;
            var v = 30 * propsGrade + 4 * propsLevel;
            return (inputValue * expertLvl * r).Parse<int>() + v;
        }

        public static int GetArtifactExpertDef(int inputValue, int expertLvl, int propsGrade, int propsLevel)
        {
            if (expertLvl <= 0)
                return 0;
            var r = 0.006f * propsGrade + 0.001f * propsLevel;
            var v = 8 * propsGrade + 1 * propsLevel;
            return (inputValue * expertLvl * r).Parse<int>() + v;
        }

        public static int GetSkillExpertAtk(int inputValue, int expertLvl, int propsGrade, int propsLevel, MartialType mType)
        {
            if (expertLvl <= 0)
                return 0;
            var r = 0.006f * propsGrade + 0.002f * propsLevel;
            var v = 20 * propsGrade + 5 * propsLevel;
            return ((inputValue * expertLvl * r + v) * Configs.SkillDmgRatios[mType]).Parse<int>();
        }

        public static int GetSkillExpertMpCost(int inputValue, int expertLvl, int propsGrade, int propsLevel)
        {
            if (expertLvl <= 0)
                return 0;
            var r = 0.10f * propsGrade + 0.01f * propsLevel;
            return (inputValue * expertLvl * r).Parse<int>();
        }

        public static int GetAbilityExpertAtk(int inputValue, int expertLvl, int propsGrade, int propsLevel)
        {
            if (expertLvl <= 0)
                return 0;
            var r = 0.005f * propsGrade + 0.001f * propsLevel;
            var v = 5 * propsGrade + 2 * propsLevel;
            return (inputValue * expertLvl * r).Parse<int>() + v;
        }

        public static int GetAbilityExpertDef(int inputValue, int expertLvl, int propsGrade, int propsLevel)
        {
            if (expertLvl <= 0)
                return 0;
            var r = 0.003f * propsGrade + 0.0006f * propsLevel;
            var v = 4 * propsGrade + 1 * propsLevel;
            return (inputValue * expertLvl * r).Parse<int>() + v;
        }

        public static int GetAbilityExpertHp(int inputValue, int expertLvl, int propsGrade, int propsLevel)
        {
            if (expertLvl <= 0)
                return 0;
            var r = 0.010f * propsGrade + 0.002f * propsLevel;
            var v = 100 * propsGrade + 20 * propsLevel;
            return (inputValue * expertLvl * r).Parse<int>() + v;
        }

        public static int GetAbilityExpertMp(int inputValue, int expertLvl, int propsGrade, int propsLevel)
        {
            if (expertLvl <= 0)
                return 0;
            var r = 0.010f * propsGrade + 0.002f * propsLevel;
            var v = 4 * propsGrade + 1 * propsLevel;
            return (inputValue * expertLvl * r).Parse<int>() + v;
        }

        public static int GetAbilityExpertSp(int inputValue, int expertLvl, int propsGrade, int propsLevel)
        {
            if (expertLvl <= 0)
                return 0;
            var r = 0.003f * propsGrade + 0.0005f * propsLevel;
            var v = 4 * propsGrade + 1 * propsLevel;
            return (inputValue * expertLvl * r).Parse<int>() + v;
        }

        public static int GetStepExpertSpeed(int expertLvl, int propsGrade, int propsLevel)
        {
            if (expertLvl <= 0)
                return 0;
            var v = 10 * propsGrade + 1 * propsLevel;
            return expertLvl * v;
        }

        public static float GetStepExpertEvade(int expertLvl, int propsGrade, int propsLevel)
        {
            if (expertLvl <= 0)
                return 0;
            var v = 0.10f * propsGrade + 0.03f * propsLevel;
            return expertLvl * v;
        }
    }
}
