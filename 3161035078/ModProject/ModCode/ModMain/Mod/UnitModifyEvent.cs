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
        public static UnitModifyEvent Instance { get; set; }

        private static readonly List<string> _units = new List<string>();
        private static readonly Dictionary<string, int> _values = new Dictionary<string, int>();

        public override void OnLoadClass(bool isNew, string modId, CacheAttribute attr)
        {
            base.OnLoadClass(isNew, modId, attr);
            _units.Clear();
            _values.Clear();
        }

        public override void OnMonthly()
        {
            base.OnMonthly();
            _values.Clear();
        }

        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);
            AddWUnitModifier(wunit);
        }

        public override void OnLoadGame()
        {
            base.OnLoadGame();
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
            var k = $"{wunit.GetUnitId()}_atk";
            if ((!_values.ContainsKey(k) || wunit.IsPlayer()) && !GameHelper.IsInBattlle())
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

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.Atk));

                rs += UnitModifyHelper.GetQiAdjAtk(wunit);

                _values[k] = rs;
            }

            return _values[k];
        }

        public static int GetAdjustDef(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_def";
            if ((!_values.ContainsKey(k) || wunit.IsPlayer()) && !GameHelper.IsInBattlle())
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

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.Def));

                _values[k] = rs;
            }

            return _values[k];
        }

        public static int GetAdjustMaxHp(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_mhp";
            if ((!_values.ContainsKey(k) || wunit.IsPlayer()) && !GameHelper.IsInBattlle())
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

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.MHp));

                rs += UnitModifyHelper.GetQiAdjHp(wunit);

                _values[k] = rs;
            }

            return _values[k];
        }

        public static int GetAdjustMaxMp(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_mmp";
            if ((!_values.ContainsKey(k) || wunit.IsPlayer()) && !GameHelper.IsInBattlle())
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

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.MMp));

                rs += UnitModifyHelper.GetQiAdjMp(wunit);

                _values[k] = rs;
            }

            return _values[k];
        }

        public static int GetAdjustMaxSp(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_msp";
            if ((!_values.ContainsKey(k) || wunit.IsPlayer()) && !GameHelper.IsInBattlle())
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

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.MSp));

                _values[k] = rs;
            }

            return _values[k];
        }

        public static int GetAdjustSpeed(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_speed";
            if ((!_values.ContainsKey(k) || wunit.IsPlayer()) && !GameHelper.IsInBattlle())
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

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.Speed));

                _values[k] = rs;
            }

            return _values[k];
        }

        public static int GetAdjustBlade(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_blade";
            if ((!_values.ContainsKey(k) || wunit.IsPlayer()) && !GameHelper.IsInBattlle())
            {
                var rs = 0;

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.BasisBlade));

                _values[k] = rs;
            }

            return _values[k];
        }

        public static int GetAdjustEarth(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_earth";
            if ((!_values.ContainsKey(k) || wunit.IsPlayer()) && !GameHelper.IsInBattlle())
            {
                var rs = 0;

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.BasisEarth));

                _values[k] = rs;
            }

            return _values[k];
        }

        public static int GetAdjustFinger(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_finger";
            if ((!_values.ContainsKey(k) || wunit.IsPlayer()) && !GameHelper.IsInBattlle())
            {
                var rs = 0;

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.BasisFinger));

                _values[k] = rs;
            }

            return _values[k];
        }

        public static int GetAdjustFire(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_fire";
            if ((!_values.ContainsKey(k) || wunit.IsPlayer()) && !GameHelper.IsInBattlle())
            {
                var rs = 0;

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.BasisFire));

                _values[k] = rs;
            }

            return _values[k];
        }

        public static int GetAdjustFist(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_fist";
            if ((!_values.ContainsKey(k) || wunit.IsPlayer()) && !GameHelper.IsInBattlle())
            {
                var rs = 0;

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.BasisFist));

                _values[k] = rs;
            }

            return _values[k];
        }

        public static int GetAdjustFroze(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_froze";
            if ((!_values.ContainsKey(k) || wunit.IsPlayer()) && !GameHelper.IsInBattlle())
            {
                var rs = 0;

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.BasisFroze));

                _values[k] = rs;
            }

            return _values[k];
        }

        public static int GetAdjustPalm(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_pallm";
            if ((!_values.ContainsKey(k) || wunit.IsPlayer()) && !GameHelper.IsInBattlle())
            {
                var rs = 0;

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.BasisPalm));

                _values[k] = rs;
            }

            return _values[k];
        }

        public static int GetAdjustSpear(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_spear";
            if ((!_values.ContainsKey(k) || wunit.IsPlayer()) && !GameHelper.IsInBattlle())
            {
                var rs = 0;

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.BasisSpear));

                _values[k] = rs;
            }

            return _values[k];
        }

        public static int GetAdjustSword(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_sword";
            if ((!_values.ContainsKey(k) || wunit.IsPlayer()) && !GameHelper.IsInBattlle())
            {
                var rs = 0;

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.BasisSword));

                _values[k] = rs;
            }

            return _values[k];
        }

        public static int GetAdjustThunder(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_thunder";
            if ((!_values.ContainsKey(k) || wunit.IsPlayer()) && !GameHelper.IsInBattlle())
            {
                var rs = 0;

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.BasisThunder));

                _values[k] = rs;
            }

            return _values[k];
        }

        public static int GetAdjustWind(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_wind";
            if ((!_values.ContainsKey(k) || wunit.IsPlayer()) && !GameHelper.IsInBattlle())
            {
                var rs = 0;

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.BasisWind));

                _values[k] = rs;
            }

            return _values[k];
        }

        public static int GetAdjustWood(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_wood";
            if ((!_values.ContainsKey(k) || wunit.IsPlayer()) && !GameHelper.IsInBattlle())
            {
                var rs = 0;

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.BasisWood));

                _values[k] = rs;
            }

            return _values[k];
        }
    }
}
