using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Mod;
using System;
using ModLib.Helper;
using System.Collections.Generic;
using static MOD_nE7UL2.Object.ModStts;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Expertise: using skills, steps, abilities and artifacts grants expertise exp per item; levels add bonuses.
    /// </summary>
    [Cache(ModConst.EXPERT_EVENT)]
    public class ExpertEvent : ModEvent
    {
        public static ExpertEvent Instance { get; set; }
        public static _ExpertConfigs Configs => ModMain.ModObj.ModSettings.ExpertConfigs;

        public IDictionary<string, int> ExpertExps { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Monthly exp for equipped martial arts/abilities and NPC artifacts.
        /// </summary>
        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);

            var ticks = ModLib.Mod.ModMaster.Ticks(SMLocalConfigsEvent.Instance.Configs.GrowUpSpeed);
            if (ticks > 0)
            {
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
                            AddExpertExp(wunit, martialData.data.soleID, Configs.SkillExpRatios[martialType] * ticks);
                        }
                        else
                        {
                            AddExpertExp(wunit, martialData.data.soleID, Configs.SkillExpRatios[martialType] * ticks);
                        }
                    }
                }

                if (wunit.IsPlayer())
                    return;
                foreach (var artifact in wunit.GetEquippedArtifacts())
                {
                    AddExpertExp(wunit, artifact.soleID, Configs.AutoArtifactExpRate * ticks);
                }
            }
        }

        /// <summary>
        /// Exp for artifacts and left skill on hit.
        /// </summary>
        public override void OnBattleUnitHit(UnitHit e)
        {
            base.OnBattleUnitHit(e);
            var wunit = e?.hitData?.attackUnit?.data?.TryCast<UnitDataHuman>()?.worldUnitData?.unit;
            if (wunit != null)
            {
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

        /// <summary>
        /// Exp for right skill/ultimate.
        /// </summary>
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
        /// <summary>
        /// Exp for the step when the player moves on the map.
        /// </summary>
        [ErrorIgnore]
        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
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

        /// <summary>
        /// Exp dictionary.
        /// </summary>
        public static IDictionary<string, int> GetExpertExps()
        {
            if (Instance.ExpertExps == null)
                Instance.ExpertExps = new Dictionary<string, int>();
            return Instance.ExpertExps;
        }

        /// <summary>
        /// Adds exp (ratio x grade x talent x random).
        /// </summary>
        public static void AddExpertExp(WorldUnitBase wunit, string soleId, float exp)
        {
            var e = GetExpertExps();
            if (!e.ContainsKey(soleId))
                e.Add(soleId, 0);
            var insight = wunit.GetDynProperty(UnitDynPropertyEnum.Talent).value;
            // long sum capped at int.MaxValue: the exp of a skill used for a very long game must not wrap to negative
            var add = (double)exp * 100.0d * wunit.GetGradeLvl() * CommonTool.Random(1.0f, 2.0f) * (insight / 100.0d);
            e[soleId] = (int)Math.Max(0, Math.Min(int.MaxValue, e[soleId] + (long)Math.Max(0, Math.Min(int.MaxValue, add))));
        }

        /// <summary>
        /// Exp of an item.
        /// </summary>
        public static int GetExpertExp(string soleId)
        {
            var e = GetExpertExps();
            if (e.ContainsKey(soleId))
                return e[soleId];
            return 0;
        }

        /// <summary>
        /// Exp needed for a level (doubles each level).
        /// </summary>
        public static int GetExpertNeedExp(int expertLvl, int propsGrade, int propsLevel)
        {
            if (expertLvl <= 0)
                return 0;
            var r = 1000.0d * propsGrade * (1.0d + 0.2d * propsLevel);
            // capped at int.MaxValue (Convert.ToInt32 threw OverflowException for high levels)
            return (int)Math.Min(int.MaxValue, Math.Pow(2, expertLvl - 1) * r);
        }

        /// <summary>
        /// Level from exp.
        /// </summary>
        public static int GetExpertLvl(string soleId, int propsGrade, int propsLevel)
        {
            var exp = GetExpertExp(soleId);
            for (int i = 1; true; i++)
            {
                var need = GetExpertNeedExp(i, propsGrade, propsLevel);
                if (exp < need || need == int.MaxValue)
                    return i - 1;
            }
        }
    }
}
