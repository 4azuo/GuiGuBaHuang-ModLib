using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using static MOD_nE7UL2.Object.GameStts;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.EXPERT_EVENT)]
    public class ExpertEvent : ModEvent
    {
        public static ExpertEvent Instance { get; set; }
        public static _ExpertConfigs Configs => ModMain.ModObj.GameSettings.ExpertConfigs;

        public IDictionary<string, int> ExpertExps { get; set; } = new Dictionary<string, int>();

        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);
            foreach (var martial in wunit.GetActionMartials())
            {
                var martialData = martial.Value.data.To<DataProps.MartialData>();
                var martialType = martialData.martialType;
                if (Configs.SkillExpRatios.ContainsKey(martialType))
                {
                    if (wunit.IsPlayer())
                    {
                        if (martialType != MartialType.Ability || !wunit.data.unitData.abilitys.Contains(martialData.data.soleID))
                            continue;
                        AddExpertExp(wunit, martialData.data.soleID, Configs.SkillExpRatios[martialType]);
                    }
                    else
                    {
                        AddExpertExp(wunit, martialData.data.soleID, Configs.SkillExpRatios[martialType]);
                    }
                }
            }

            if (wunit.IsPlayer())
                return;
            foreach (var artifact in wunit.GetEquippedArtifacts())
            {
                AddExpertExp(wunit, artifact.soleID, Configs.AutoArtifactExpRate);
            }
        }

        public override void OnBattleUnitHit(UnitHit e)
        {
            base.OnBattleUnitHit(e);
            var humanData = e.hitData.attackUnit.data.TryCast<UnitDataHuman>();
            if (humanData?.worldUnitData?.unit != null)
            {
                var wunit = humanData.worldUnitData.unit;
                foreach (var artifact in wunit.GetEquippedArtifacts())
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
                if (skill != null && (skill?.skillData?.martialType == MartialType.SkillRight || skill?.skillData?.martialType == MartialType.Ultimate))
                {
                    var soleId = skill.skillData.data.soleID;
                    AddExpertExp(wunit, soleId, Configs.SkillExpRatios[skill.skillData.martialType]);
                }
            }
        }

        private int oldX;
        private int oldY;
        [ErrorIgnore]
        [EventCondition]
        public override void OnTimeUpdate200ms()
        {
            base.OnTimeUpdate200ms();
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
            if (Instance.ExpertExps == null)
                Instance.ExpertExps = new Dictionary<string, int>();
            return Instance.ExpertExps;
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
    }
}
