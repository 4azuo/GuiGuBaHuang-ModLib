using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using ModLib.Enum;
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
            if (!_units.Contains(unitId))
            {
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
        }

        public static int GetAdjustAtk(WorldUnitBase wunit)
        {
            var rs = 0;

            var atk = wunit.GetDynProperty(UnitDynPropertyEnum.Attack).baseValue;

            foreach (var artifact in wunit.GetEquippedArtifacts())
            {
                var a = artifact.To<DataProps.PropsArtifact>();
                if (a.durable > 0)
                {
                    rs += UnitModifyHelper.GetArtifactTotalAdjAtk(atk, artifact, a);
                }
            }

            foreach (var abi in wunit.data.unitData.GetActionMartial(MartialType.Ability))
            {
                var martialData = abi.data.To<DataProps.MartialData>();
                if (wunit.data.unitData.abilitys.Contains(martialData.data.soleID))
                {
                    var expertLvl = ExpertEvent.GetExpertLvl(martialData.data.soleID, martialData.data.propsInfoBase.grade, martialData.data.propsInfoBase.level);
                    rs += UnitModifyHelper.GetAbilityExpertAtk(atk, expertLvl, martialData.data.propsInfoBase.grade, martialData.data.propsInfoBase.level);
                }
            }

            return rs;
        }

        public static int GetAdjustDef(WorldUnitBase wunit)
        {
            var rs = 0;

            var def = wunit.GetDynProperty(UnitDynPropertyEnum.Defense).baseValue;

            foreach (var artifact in wunit.GetEquippedArtifacts())
            {
                var a = artifact.To<DataProps.PropsArtifact>();
                if (a.durable > 0)
                {
                    rs += UnitModifyHelper.GetArtifactTotalAdjDef(def, artifact, a);
                }
            }

            foreach (var abi in wunit.data.unitData.GetActionMartial(MartialType.Ability))
            {
                var martialData = abi.data.To<DataProps.MartialData>();
                if (wunit.data.unitData.abilitys.Contains(martialData.data.soleID))
                {
                    var expertLvl = ExpertEvent.GetExpertLvl(martialData.data.soleID, martialData.data.propsInfoBase.grade, martialData.data.propsInfoBase.level);
                    rs += UnitModifyHelper.GetAbilityExpertDef(def, expertLvl, martialData.data.propsInfoBase.grade, martialData.data.propsInfoBase.level);
                }
            }

            rs += UnitModifyHelper.GetOutfitAdjDef(def, wunit.GetEquippedOutfit(), CustomRefineEvent.GetRefineLvl(wunit.GetEquippedOutfit()));

            return rs;
        }

        public static int GetAdjustMaxHp(WorldUnitBase wunit)
        {
            var rs = 0;

            var hpMax = wunit.GetDynProperty(UnitDynPropertyEnum.HpMax).baseValue;

            foreach (var artifact in wunit.GetEquippedArtifacts())
            {
                var a = artifact.To<DataProps.PropsArtifact>();
                if (a.durable > 0)
                {
                    rs += UnitModifyHelper.GetArtifactBasicAdjHp(hpMax, artifact, a);
                }
            }

            foreach (var abi in wunit.data.unitData.GetActionMartial(MartialType.Ability))
            {
                var martialData = abi.data.To<DataProps.MartialData>();
                if (wunit.data.unitData.abilitys.Contains(martialData.data.soleID))
                {
                    var expertLvl = ExpertEvent.GetExpertLvl(martialData.data.soleID, martialData.data.propsInfoBase.grade, martialData.data.propsInfoBase.level);
                    rs += UnitModifyHelper.GetAbilityExpertHp(hpMax, expertLvl, martialData.data.propsInfoBase.grade, martialData.data.propsInfoBase.level);
                }
            }

            rs += UnitModifyHelper.GetAbiPointAdjHp(wunit);

            rs += UnitModifyHelper.GetMartialAdjHp(wunit);

            rs += UnitModifyHelper.GetRingAdjHp(hpMax, wunit.GetEquippedRing(), CustomRefineEvent.GetRefineLvl(wunit.GetEquippedRing()));

            rs += UnitModifyHelper.GetOutfitAdjHp(hpMax, wunit.GetEquippedOutfit(), CustomRefineEvent.GetRefineLvl(wunit.GetEquippedOutfit()));

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
                    rs += UnitModifyHelper.GetAbilityExpertMp(mpMax, expertLvl, martialData.data.propsInfoBase.grade, martialData.data.propsInfoBase.level);
                }
            }

            rs += UnitModifyHelper.GetAbiPointAdjMp(wunit);

            rs += UnitModifyHelper.GetSpiritualAdjMp(wunit);

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
                    rs += UnitModifyHelper.GetAbilityExpertSp(spMax, expertLvl, martialData.data.propsInfoBase.grade, martialData.data.propsInfoBase.level);
                }
            }

            rs += UnitModifyHelper.GetArtisanshipAdjSp(wunit);

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
                    rs += UnitModifyHelper.GetStepExpertSpeed(expertLvl, martialData.data.propsInfoBase.grade, martialData.data.propsInfoBase.level);
                }
            }

            return rs;
        }
    }
}
