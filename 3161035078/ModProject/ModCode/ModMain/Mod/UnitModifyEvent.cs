using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using ModLib.Enum;
using System.Collections.Generic;
using MOD_nE7UL2.Enum;
using System;
using Newtonsoft.Json;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.UNIT_MODIFY_EVENT)]
    public class UnitModifyEvent : ModEvent
    {
        public static UnitModifyEvent Instance { get; set; }

        [JsonIgnore]
        public List<string> CachedUnits { get; } = new List<string>();
        [JsonIgnore]
        public Dictionary<string, int> CachedValues { get; } = new Dictionary<string, int>();

        public override void OnMonthly()
        {
            base.OnMonthly();
            CachedValues.Clear();
        }

        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);
            AddWUnitModifier(wunit);
        }

        public override void OnLoadGame()
        {
            base.OnLoadGame();
            foreach (var wunit in ModMaster.ModObj.WUnits)
            {
                AddWUnitModifier(wunit);
            }
        }

        private void AddWUnitModifier(WorldUnitBase wunit)
        {
            var unitId = wunit.GetUnitId();
            if (!CachedUnits.Contains(unitId))
            {
                CachedUnits.Add(unitId);

                //
                wunit.GetDynProperty(UnitDynPropertyEnum.Attack).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    try { return GetAdjustAtk(wunit); } catch (Exception e) { DebugHelper.WriteLine(e); return 0; }
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.Defense).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    try { return GetAdjustDef(wunit); } catch (Exception e) { DebugHelper.WriteLine(e); return 0; }
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.HpMax).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    try { return GetAdjustMaxHp(wunit); } catch (Exception e) { DebugHelper.WriteLine(e); return 0; }
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.MpMax).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    try { return GetAdjustMaxMp(wunit); } catch (Exception e) { DebugHelper.WriteLine(e); return 0; }
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.DpMax).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    try { return GetAdjustMaxDp(wunit); } catch (Exception e) { DebugHelper.WriteLine(e); return 0; }
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.SpMax).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    try { return GetAdjustMaxSp(wunit); } catch (Exception e) { DebugHelper.WriteLine(e); return 0; }
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.MoveSpeed).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    try { return GetAdjustSpeed(wunit); } catch (Exception e) { DebugHelper.WriteLine(e); return 0; }
                }));

                //basis
                wunit.GetDynProperty(UnitDynPropertyEnum.BasisBlade).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    try { return GetAdjustBlade(wunit); } catch (Exception e) { DebugHelper.WriteLine(e); return 0; }
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.BasisEarth).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    try { return GetAdjustEarth(wunit); } catch (Exception e) { DebugHelper.WriteLine(e); return 0; }
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.BasisFinger).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    try { return GetAdjustFinger(wunit); } catch (Exception e) { DebugHelper.WriteLine(e); return 0; }
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.BasisFire).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    try { return GetAdjustFire(wunit); } catch (Exception e) { DebugHelper.WriteLine(e); return 0; }
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.BasisFist).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    try { return GetAdjustFist(wunit); } catch (Exception e) { DebugHelper.WriteLine(e); return 0; }
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.BasisFroze).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    try { return GetAdjustFroze(wunit); } catch (Exception e) { DebugHelper.WriteLine(e); return 0; }
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.BasisPalm).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    try { return GetAdjustPalm(wunit); } catch (Exception e) { DebugHelper.WriteLine(e); return 0; }
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.BasisSpear).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    try { return GetAdjustSpear(wunit); } catch (Exception e) { DebugHelper.WriteLine(e); return 0; }
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.BasisSword).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    try { return GetAdjustSword(wunit); } catch (Exception e) { DebugHelper.WriteLine(e); return 0; }
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.BasisThunder).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    try { return GetAdjustThunder(wunit); } catch (Exception e) { DebugHelper.WriteLine(e); return 0; }
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.BasisWind).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    try { return GetAdjustWind(wunit); } catch (Exception e) { DebugHelper.WriteLine(e); return 0; }
                }));
                wunit.GetDynProperty(UnitDynPropertyEnum.BasisWood).AddValue((DynInt.DynObjectAddHandler)(() =>
                {
                    try { return GetAdjustWood(wunit); } catch (Exception e) { DebugHelper.WriteLine(e); return 0; }
                }));
            }
        }

        public static int GetAdjustAtk(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_atk";
            if (GameHelper.IsInBattlle())
                return Instance.CachedValues.ContainsKey(k) ? Instance.CachedValues[k] : 0;
            if (!Instance.CachedValues.ContainsKey(k) || wunit.IsPlayer())
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

                Instance.CachedValues[k] = rs;
            }

            return Instance.CachedValues[k];
        }

        public static int GetAdjustDef(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_def";
            if (GameHelper.IsInBattlle())
                return Instance.CachedValues.ContainsKey(k) ? Instance.CachedValues[k] : 0;
            if (!Instance.CachedValues.ContainsKey(k) || wunit.IsPlayer())
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

                var outfit = wunit.GetEquippedOutfit();
                rs += UnitModifyHelper.GetRefineOutfitAdjDef(def, outfit, CustomRefineEvent.GetRefineLvl(outfit));

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.Def));

                Instance.CachedValues[k] = rs;
            }

            return Instance.CachedValues[k];
        }

        public static int GetAdjustMaxHp(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_mhp";
            if (GameHelper.IsInBattlle())
                return Instance.CachedValues.ContainsKey(k) ? Instance.CachedValues[k] : 0;
            if (!Instance.CachedValues.ContainsKey(k) || wunit.IsPlayer())
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

                var ring = wunit.GetEquippedRing();
                rs += UnitModifyHelper.GetRefineRingAdjHp(hpMax, ring, CustomRefineEvent.GetRefineLvl(ring));

                var outfit = wunit.GetEquippedOutfit();
                rs += UnitModifyHelper.GetRefineOutfitAdjHp(hpMax, outfit, CustomRefineEvent.GetRefineLvl(outfit));

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.MHp));

                rs += UnitModifyHelper.GetQiAdjHp(wunit);

                Instance.CachedValues[k] = rs;
            }

            return Instance.CachedValues[k];
        }

        public static int GetAdjustMaxMp(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_mmp";
            if (GameHelper.IsInBattlle())
                return Instance.CachedValues.ContainsKey(k) ? Instance.CachedValues[k] : 0;
            if (!Instance.CachedValues.ContainsKey(k) || wunit.IsPlayer())
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

                var mount = wunit.GetEquippedMount();
                rs += UnitModifyHelper.GetRefineMountAdjMp(mpMax, mount, CustomRefineEvent.GetRefineLvl(mount));

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.MMp));

                rs += UnitModifyHelper.GetQiAdjMp(wunit);

                Instance.CachedValues[k] = rs;
            }

            return Instance.CachedValues[k];
        }

        public static int GetAdjustMaxSp(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_msp";
            if (GameHelper.IsInBattlle())
                return Instance.CachedValues.ContainsKey(k) ? Instance.CachedValues[k] : 0;
            if (!Instance.CachedValues.ContainsKey(k) || wunit.IsPlayer())
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

                Instance.CachedValues[k] = rs;
            }

            return Instance.CachedValues[k];
        }

        public static int GetAdjustMaxDp(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_mdp";
            if (GameHelper.IsInBattlle())
                return Instance.CachedValues.ContainsKey(k) ? Instance.CachedValues[k] : 0;
            if (!Instance.CachedValues.ContainsKey(k) || wunit.IsPlayer())
            {
                var rs = 0;

                var dpMax = wunit.GetDynProperty(UnitDynPropertyEnum.DpMax).baseValue;

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.MDp));

                Instance.CachedValues[k] = rs;
            }

            return Instance.CachedValues[k];
        }

        public static int GetAdjustSpeed(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_speed";
            if (GameHelper.IsInBattlle())
                return Instance.CachedValues.ContainsKey(k) ? Instance.CachedValues[k] : 0;
            if (!Instance.CachedValues.ContainsKey(k) || wunit.IsPlayer())
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

                Instance.CachedValues[k] = rs;
            }

            return Instance.CachedValues[k];
        }

        public static int GetAdjustBlade(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_blade";
            if (GameHelper.IsInBattlle())
                return Instance.CachedValues.ContainsKey(k) ? Instance.CachedValues[k] : 0;
            if (!Instance.CachedValues.ContainsKey(k) || wunit.IsPlayer())
            {
                var rs = 0;

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.BasisBlade));

                Instance.CachedValues[k] = rs;
            }

            return Instance.CachedValues[k];
        }

        public static int GetAdjustEarth(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_earth";
            if (GameHelper.IsInBattlle())
                return Instance.CachedValues.ContainsKey(k) ? Instance.CachedValues[k] : 0;
            if (!Instance.CachedValues.ContainsKey(k) || wunit.IsPlayer())
            {
                var rs = 0;

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.BasisEarth));

                Instance.CachedValues[k] = rs;
            }

            return Instance.CachedValues[k];
        }

        public static int GetAdjustFinger(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_finger";
            if (GameHelper.IsInBattlle())
                return Instance.CachedValues.ContainsKey(k) ? Instance.CachedValues[k] : 0;
            if (!Instance.CachedValues.ContainsKey(k) || wunit.IsPlayer())
            {
                var rs = 0;

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.BasisFinger));

                Instance.CachedValues[k] = rs;
            }

            return Instance.CachedValues[k];
        }

        public static int GetAdjustFire(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_fire";
            if (GameHelper.IsInBattlle())
                return Instance.CachedValues.ContainsKey(k) ? Instance.CachedValues[k] : 0;
            if (!Instance.CachedValues.ContainsKey(k) || wunit.IsPlayer())
            {
                var rs = 0;

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.BasisFire));

                Instance.CachedValues[k] = rs;
            }

            return Instance.CachedValues[k];
        }

        public static int GetAdjustFist(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_fist";
            if (GameHelper.IsInBattlle())
                return Instance.CachedValues.ContainsKey(k) ? Instance.CachedValues[k] : 0;
            if (!Instance.CachedValues.ContainsKey(k) || wunit.IsPlayer())
            {
                var rs = 0;

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.BasisFist));

                Instance.CachedValues[k] = rs;
            }

            return Instance.CachedValues[k];
        }

        public static int GetAdjustFroze(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_froze";
            if (GameHelper.IsInBattlle())
                return Instance.CachedValues.ContainsKey(k) ? Instance.CachedValues[k] : 0;
            if (!Instance.CachedValues.ContainsKey(k) || wunit.IsPlayer())
            {
                var rs = 0;

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.BasisFroze));

                Instance.CachedValues[k] = rs;
            }

            return Instance.CachedValues[k];
        }

        public static int GetAdjustPalm(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_pallm";
            if (GameHelper.IsInBattlle())
                return Instance.CachedValues.ContainsKey(k) ? Instance.CachedValues[k] : 0;
            if (!Instance.CachedValues.ContainsKey(k) || wunit.IsPlayer())
            {
                var rs = 0;

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.BasisPalm));

                Instance.CachedValues[k] = rs;
            }

            return Instance.CachedValues[k];
        }

        public static int GetAdjustSpear(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_spear";
            if (GameHelper.IsInBattlle())
                return Instance.CachedValues.ContainsKey(k) ? Instance.CachedValues[k] : 0;
            if (!Instance.CachedValues.ContainsKey(k) || wunit.IsPlayer())
            {
                var rs = 0;

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.BasisSpear));

                Instance.CachedValues[k] = rs;
            }

            return Instance.CachedValues[k];
        }

        public static int GetAdjustSword(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_sword";
            if (GameHelper.IsInBattlle())
                return Instance.CachedValues.ContainsKey(k) ? Instance.CachedValues[k] : 0;
            if (!Instance.CachedValues.ContainsKey(k) || wunit.IsPlayer())
            {
                var rs = 0;

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.BasisSword));

                Instance.CachedValues[k] = rs;
            }

            return Instance.CachedValues[k];
        }

        public static int GetAdjustThunder(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_thunder";
            if (GameHelper.IsInBattlle())
                return Instance.CachedValues.ContainsKey(k) ? Instance.CachedValues[k] : 0;
            if (!Instance.CachedValues.ContainsKey(k) || wunit.IsPlayer())
            {
                var rs = 0;

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.BasisThunder));

                Instance.CachedValues[k] = rs;
            }

            return Instance.CachedValues[k];
        }

        public static int GetAdjustWind(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_wind";
            if (GameHelper.IsInBattlle())
                return Instance.CachedValues.ContainsKey(k) ? Instance.CachedValues[k] : 0;
            if (!Instance.CachedValues.ContainsKey(k) || wunit.IsPlayer())
            {
                var rs = 0;

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.BasisWind));

                Instance.CachedValues[k] = rs;
            }

            return Instance.CachedValues[k];
        }

        public static int GetAdjustWood(WorldUnitBase wunit)
        {
            var k = $"{wunit.GetUnitId()}_wood";
            if (GameHelper.IsInBattlle())
                return Instance.CachedValues.ContainsKey(k) ? Instance.CachedValues[k] : 0;
            if (!Instance.CachedValues.ContainsKey(k) || wunit.IsPlayer())
            {
                var rs = 0;

                rs += Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.BasisWood));

                Instance.CachedValues[k] = rs;
            }

            return Instance.CachedValues[k];
        }
    }
}
