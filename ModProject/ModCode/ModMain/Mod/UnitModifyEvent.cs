using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using ModLib.Enum;
using System.Collections.Generic;
using MOD_nE7UL2.Enum;
using System;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.UNIT_MODIFY_EVENT)]
    public class UnitModifyEvent : ModEvent
    {
        private static readonly List<string> _units = new List<string>();

        [ErrorIgnore]
        [EventCondition]
        public override void OnTimeUpdate1s()
        {
            base.OnTimeUpdate1s();
            foreach (var wunit in g.world.playerUnit.GetUnitsAround())
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

                //
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

                //basis
                wunit.GetDynProperty(UnitDynPropertyEnum.BasisBlade).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    return GetAdjustBlade(wunit);
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.BasisEarth).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    return GetAdjustEarth(wunit);
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.BasisFinger).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    return GetAdjustFinger(wunit);
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.BasisFire).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    return GetAdjustFire(wunit);
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.BasisFist).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    return GetAdjustFist(wunit);
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.BasisFroze).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    return GetAdjustFroze(wunit);
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.BasisPalm).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    return GetAdjustPalm(wunit);
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.BasisSpear).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    return GetAdjustSpear(wunit);
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.BasisSword).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    return GetAdjustSword(wunit);
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.BasisThunder).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    return GetAdjustThunder(wunit);
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.BasisWind).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    return GetAdjustWind(wunit);
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.BasisWood).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    return GetAdjustWood(wunit);
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
                    var aconf = artifact.propsItem.IsArtifact();

                    rs += UnitModifyHelper.GetArtifactBasicAdjAtk(atk, artifact, a);

                    rs += UnitModifyHelper.GetArtifactExpertAtk(aconf.atk, ExpertEvent.GetExpertLvl(artifact.soleID, artifact.propsInfoBase.grade, artifact.propsInfoBase.level), artifact.propsInfoBase.grade, artifact.propsInfoBase.level);

                    rs += UnitModifyHelper.GetRefineArtifactAdjAtk(artifact, CustomRefineEvent.GetRefineLvl(artifact));
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

            rs += Convert.ToInt32(CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.Atk));

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
                    var aconf = artifact.propsItem.IsArtifact();

                    rs += UnitModifyHelper.GetArtifactBasicAdjDef(def, artifact, a);

                    rs += UnitModifyHelper.GetArtifactExpertDef(aconf.def, ExpertEvent.GetExpertLvl(artifact.soleID, artifact.propsInfoBase.grade, artifact.propsInfoBase.level), artifact.propsInfoBase.grade, artifact.propsInfoBase.level);

                    rs += UnitModifyHelper.GetRefineArtifactAdjDef(artifact, CustomRefineEvent.GetRefineLvl(artifact));
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

            rs += UnitModifyHelper.GetRefineOutfitAdjDef(def, wunit.GetEquippedOutfit(), CustomRefineEvent.GetRefineLvl(wunit.GetEquippedOutfit()));

            rs += Convert.ToInt32(CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.Def));

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

            rs += UnitModifyHelper.GetRefineRingAdjHp(hpMax, wunit.GetEquippedRing(), CustomRefineEvent.GetRefineLvl(wunit.GetEquippedRing()));

            rs += UnitModifyHelper.GetRefineOutfitAdjHp(hpMax, wunit.GetEquippedOutfit(), CustomRefineEvent.GetRefineLvl(wunit.GetEquippedOutfit()));

            rs += Convert.ToInt32(CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.MHp));

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

            rs += Convert.ToInt32(CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.MMp));

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

            rs += Convert.ToInt32(CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.MSp));

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

            rs += Convert.ToInt32(CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.Speed));

            return rs;
        }

        public static int GetAdjustBlade(WorldUnitBase wunit)
        {
            var rs = 0;

            rs += Convert.ToInt32(CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.BasisBlade));

            return rs;
        }

        public static int GetAdjustEarth(WorldUnitBase wunit)
        {
            var rs = 0;

            rs += Convert.ToInt32(CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.BasisEarth));

            return rs;
        }

        public static int GetAdjustFinger(WorldUnitBase wunit)
        {
            var rs = 0;

            rs += Convert.ToInt32(CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.BasisFinger));

            return rs;
        }

        public static int GetAdjustFire(WorldUnitBase wunit)
        {
            var rs = 0;

            rs += Convert.ToInt32(CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.BasisFire));

            return rs;
        }

        public static int GetAdjustFist(WorldUnitBase wunit)
        {
            var rs = 0;

            rs += Convert.ToInt32(CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.BasisFist));

            return rs;
        }

        public static int GetAdjustFroze(WorldUnitBase wunit)
        {
            var rs = 0;

            rs += Convert.ToInt32(CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.BasisFroze));

            return rs;
        }

        public static int GetAdjustPalm(WorldUnitBase wunit)
        {
            var rs = 0;

            rs += Convert.ToInt32(CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.BasisPalm));

            return rs;
        }

        public static int GetAdjustSpear(WorldUnitBase wunit)
        {
            var rs = 0;

            rs += Convert.ToInt32(CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.BasisSpear));

            return rs;
        }

        public static int GetAdjustSword(WorldUnitBase wunit)
        {
            var rs = 0;

            rs += Convert.ToInt32(CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.BasisSword));

            return rs;
        }

        public static int GetAdjustThunder(WorldUnitBase wunit)
        {
            var rs = 0;

            rs += Convert.ToInt32(CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.BasisThunder));

            return rs;
        }

        public static int GetAdjustWind(WorldUnitBase wunit)
        {
            var rs = 0;

            rs += Convert.ToInt32(CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.BasisWind));

            return rs;
        }

        public static int GetAdjustWood(WorldUnitBase wunit)
        {
            var rs = 0;

            rs += Convert.ToInt32(CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.BasisWood));

            return rs;
        }
    }
}
