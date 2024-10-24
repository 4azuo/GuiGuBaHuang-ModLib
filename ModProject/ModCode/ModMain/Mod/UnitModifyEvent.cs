﻿using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using ModLib.Enum;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.UNIT_MODIFY_EVENT)]
    public class UnitModifyEvent : ModEvent
    {
        [JsonIgnore]
        private List<string> _units = new List<string>();

        public override void OnLoadGame()
        {
            base.OnLoadGame();

            foreach (var wunit in g.world.unit.GetUnits())
            {
                AddWUnitModifier(wunit);
            }
        }

        public override void OnSave(ETypeData e)
        {
            base.OnSave(e);

            foreach (var wunit in g.world.unit.GetUnits())
            {
                AddWUnitModifier(wunit);
            }
        }

        private void AddWUnitModifier(WorldUnitBase wunit)
        {
            var unitId = wunit.GetUnitId();
            if (_units.Contains(unitId))
                return;
            _units.Add(unitId);

            wunit.GetDynProperty(UnitDynPropertyEnum.Attack).AddValue((DynInt.DynObjectAddHandler)(() =>
            {
                return GetAdjustAtk(wunit);
            }));
            wunit.GetDynProperty(UnitDynPropertyEnum.Defense).AddValue((DynInt.DynObjectAddHandler)(() =>
            {
                return GetAdjustDef(wunit);
            }));
            wunit.GetDynProperty(UnitDynPropertyEnum.HpMax).AddValue((DynInt.DynObjectAddHandler)(() =>
            {
                return GetAdjustMaxHp(wunit);
            }));
            wunit.GetDynProperty(UnitDynPropertyEnum.MpMax).AddValue((DynInt.DynObjectAddHandler)(() =>
            {
                return GetAdjustMaxMp(wunit);
            }));
            wunit.GetDynProperty(UnitDynPropertyEnum.SpMax).AddValue((DynInt.DynObjectAddHandler)(() =>
            {
                return GetAdjustMaxSp(wunit);
            }));
            wunit.GetDynProperty(UnitDynPropertyEnum.MoveSpeed).AddValue((DynInt.DynObjectAddHandler)(() =>
            {
                return GetAdjustSpeed(wunit);
            }));
        }

        public static int GetAdjustAtk(WorldUnitBase wunit)
        {
            var rs = 0;

            var atk = wunit.GetDynProperty(UnitDynPropertyEnum.Attack).baseValue;

            var artifacts = wunit.GetEquippedArtifacts();
            foreach (var artifact in artifacts)
            {
                var a = artifact.To<DataProps.PropsArtifact>();
                if (a.durable > 0)
                {
                    var aconf = artifact.propsItem.IsArtifact();
                    var r = 0.01f + (0.001f * Math.Pow(2, a.level)) + (0.02f * a.grade);
                    var r1 = (4.00f + (0.006f * Math.Pow(3, a.level)) + (1.00f * a.grade)) / 100.0f;
                    var expertLvl = ExpertEvent.GetExpertLvl(artifact.soleID, artifact.propsInfoBase.grade, artifact.propsInfoBase.level);
                    rs += (r * atk + r1 * aconf.atk).Parse<int>() +
                        ExpertEvent.GetArtifactExpertAtk(aconf.atk, expertLvl, artifact.propsInfoBase.grade, artifact.propsInfoBase.level);
                }
            }

            foreach (var abi in wunit.data.unitData.GetActionMartial(MartialType.Ability))
            {
                var martialData = abi.data.To<DataProps.MartialData>();
                if (wunit.data.unitData.abilitys.Contains(martialData.data.soleID))
                {
                    var expertLvl = ExpertEvent.GetExpertLvl(martialData.data.soleID, martialData.data.propsInfoBase.grade, martialData.data.propsInfoBase.level);
                    rs += ExpertEvent.GetAbilityExpertAtk(atk, expertLvl, martialData.data.propsInfoBase.grade, martialData.data.propsInfoBase.level);
                }
            }

            return rs;
        }

        public static int GetAdjustDef(WorldUnitBase wunit)
        {
            var rs = 0;

            var def = wunit.GetDynProperty(UnitDynPropertyEnum.Defense).baseValue;

            var artifacts = wunit.GetEquippedArtifacts();
            foreach (var artifact in artifacts)
            {
                var a = artifact.To<DataProps.PropsArtifact>();
                if (a.durable > 0)
                {
                    var aconf = artifact.propsItem.IsArtifact();
                    var r = 0.01f + (0.001f * Math.Pow(2, a.level)) + (0.02f * a.grade);
                    var r2 = (3.00f + (0.005f * Math.Pow(3, a.level)) + (0.80f * a.grade)) / 100.0f;
                    var expertLvl = ExpertEvent.GetExpertLvl(artifact.soleID, artifact.propsInfoBase.grade, artifact.propsInfoBase.level);
                    rs += (r * def + r2 * aconf.def).Parse<int>() +
                        ExpertEvent.GetArtifactExpertDef(aconf.def, expertLvl, artifact.propsInfoBase.grade, artifact.propsInfoBase.level);
                }
            }

            foreach (var abi in wunit.data.unitData.GetActionMartial(MartialType.Ability))
            {
                var martialData = abi.data.To<DataProps.MartialData>();
                if (wunit.data.unitData.abilitys.Contains(martialData.data.soleID))
                {
                    var expertLvl = ExpertEvent.GetExpertLvl(martialData.data.soleID, martialData.data.propsInfoBase.grade, martialData.data.propsInfoBase.level);
                    rs += ExpertEvent.GetAbilityExpertDef(def, expertLvl, martialData.data.propsInfoBase.grade, martialData.data.propsInfoBase.level);
                }
            }

            return rs;
        }

        public static int GetAdjustMaxHp(WorldUnitBase wunit)
        {
            var rs = 0;

            var gradeLvl = wunit.GetGradeLvl();
            var hpMax = wunit.GetDynProperty(UnitDynPropertyEnum.HpMax).baseValue;

            var artifacts = wunit.GetEquippedArtifacts();
            foreach (var artifact in artifacts)
            {
                var a = artifact.To<DataProps.PropsArtifact>();
                if (a.durable > 0)
                {
                    var aconf = artifact.propsItem.IsArtifact();
                    var r3 = 0.01f + 0.01f * a.level + 0.03f * a.grade;
                    rs += (r3 * hpMax + a.level * a.grade * aconf.hp / 10).Parse<int>();
                }
            }

            foreach (var abi in wunit.data.unitData.GetActionMartial(MartialType.Ability))
            {
                var martialData = abi.data.To<DataProps.MartialData>();
                if (wunit.data.unitData.abilitys.Contains(martialData.data.soleID))
                {
                    var expertLvl = ExpertEvent.GetExpertLvl(martialData.data.soleID, martialData.data.propsInfoBase.grade, martialData.data.propsInfoBase.level);
                    rs += ExpertEvent.GetAbilityExpertHp(hpMax, expertLvl, martialData.data.propsInfoBase.grade, martialData.data.propsInfoBase.level);
                }
            }

            rs += wunit.GetDynProperty(UnitDynPropertyEnum.AbilityPoint).value * 10 * gradeLvl;

            return rs;
        }

        public static int GetAdjustMaxMp(WorldUnitBase wunit)
        {
            var rs = 0;

            var mpMax = wunit.GetDynProperty(UnitDynPropertyEnum.MpMax).baseValue;

            foreach (var abi in wunit.data.unitData.GetActionMartial(MartialType.Ability))
            {
                var martialData = abi.data.To<DataProps.MartialData>();
                if (wunit.data.unitData.abilitys.Contains(martialData.data.soleID))
                {
                    var expertLvl = ExpertEvent.GetExpertLvl(martialData.data.soleID, martialData.data.propsInfoBase.grade, martialData.data.propsInfoBase.level);
                    rs += ExpertEvent.GetAbilityExpertMp(mpMax, expertLvl, martialData.data.propsInfoBase.grade, martialData.data.propsInfoBase.level);
                }
            }

            rs += wunit.GetDynProperty(UnitDynPropertyEnum.AbilityPoint).value;

            return rs;
        }

        public static int GetAdjustMaxSp(WorldUnitBase wunit)
        {
            var rs = 0;

            var spMax = wunit.GetDynProperty(UnitDynPropertyEnum.SpMax).baseValue;

            foreach (var abi in wunit.data.unitData.GetActionMartial(MartialType.Ability))
            {
                var martialData = abi.data.To<DataProps.MartialData>();
                if (wunit.data.unitData.abilitys.Contains(martialData.data.soleID))
                {
                    var expertLvl = ExpertEvent.GetExpertLvl(martialData.data.soleID, martialData.data.propsInfoBase.grade, martialData.data.propsInfoBase.level);
                    rs += ExpertEvent.GetAbilityExpertSp(spMax, expertLvl, martialData.data.propsInfoBase.grade, martialData.data.propsInfoBase.level);
                }
            }

            return rs;
        }

        public static int GetAdjustSpeed(WorldUnitBase wunit)
        {
            var rs = 0;

            foreach (var abi in wunit.data.unitData.GetActionMartial(MartialType.Step))
            {
                var martialData = abi.data.To<DataProps.MartialData>();
                if (wunit.data.unitData.abilitys.Contains(martialData.data.soleID))
                {
                    var expertLvl = ExpertEvent.GetExpertLvl(martialData.data.soleID, martialData.data.propsInfoBase.grade, martialData.data.propsInfoBase.level);
                    rs += ExpertEvent.GetStepExpertSpeed(expertLvl, martialData.data.propsInfoBase.grade, martialData.data.propsInfoBase.level);
                }
            }

            return rs;
        }

        public static int GetAdjustMpCost(UnitCtrlBase unit, SkillAttack s, DataProps.MartialData md)
        {
            var curMpCost = s.data.mpCost.baseValue;
            var soleId = md.data.soleID;
            var expertLvl = ExpertEvent.GetExpertLvl(md.data.soleID, md.data.propsInfoBase.grade, md.data.propsInfoBase.level);
            return ExpertEvent.GetSkillExpertMpCost(curMpCost, expertLvl, md.data.propsInfoBase.grade, md.data.propsInfoBase.level);
        }
    }
}
