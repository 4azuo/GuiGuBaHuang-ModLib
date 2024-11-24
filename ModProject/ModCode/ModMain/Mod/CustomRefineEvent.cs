using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using System.Linq;
using ModLib.Const;
using System.Collections.Generic;
using UnityEngine.Events;
using System;
using ModLib.Object;
using MOD_nE7UL2.Enum;
using MOD_nE7UL2.Object;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.CUSTOM_REFINE_EVENT)]
    public class CustomRefineEvent : ModEvent
    {
        public static AdjTypeEnum[] RingAdjTypes = new AdjTypeEnum[]
        {
            AdjTypeEnum.Def, AdjTypeEnum.MHp, AdjTypeEnum.MMp, AdjTypeEnum.MSp,
            AdjTypeEnum.Nullify, AdjTypeEnum.RHp, AdjTypeEnum.RMp, AdjTypeEnum.RSp,
            AdjTypeEnum.SkillDamage, AdjTypeEnum.MinDamage,
            AdjTypeEnum.BasisBlade, AdjTypeEnum.BasisEarth, AdjTypeEnum.BasisFinger, AdjTypeEnum.BasisFire, AdjTypeEnum.BasisFist, AdjTypeEnum.BasisFroze,
            AdjTypeEnum.BasisPalm, AdjTypeEnum.BasisSpear, AdjTypeEnum.BasisSword, AdjTypeEnum.BasisThunder, AdjTypeEnum.BasisWind, AdjTypeEnum.BasisWood
        };
        public static AdjTypeEnum[] OutfitAdjTypes = new AdjTypeEnum[]
        {
            AdjTypeEnum.Def, AdjTypeEnum.MHp, AdjTypeEnum.MMp, AdjTypeEnum.RHp, AdjTypeEnum.RMp,
            AdjTypeEnum.Nullify, AdjTypeEnum.BlockChanceMax, AdjTypeEnum.BlockDmg,
            AdjTypeEnum.EvadeChance, AdjTypeEnum.EvadeChanceMax,
            AdjTypeEnum.BasisBlade, AdjTypeEnum.BasisEarth, AdjTypeEnum.BasisFinger, AdjTypeEnum.BasisFire, AdjTypeEnum.BasisFist, AdjTypeEnum.BasisFroze,
            AdjTypeEnum.BasisPalm, AdjTypeEnum.BasisSpear, AdjTypeEnum.BasisSword, AdjTypeEnum.BasisThunder, AdjTypeEnum.BasisWind, AdjTypeEnum.BasisWood
        };
        public static AdjTypeEnum[] ArtifactAdjTypes = new AdjTypeEnum[]
        {
            AdjTypeEnum.Atk, AdjTypeEnum.Def, AdjTypeEnum.Speed, AdjTypeEnum.Manashield,
            AdjTypeEnum.Nullify, AdjTypeEnum.SkillDamage, AdjTypeEnum.MinDamage,
            AdjTypeEnum.BlockChanceMax, AdjTypeEnum.BlockDmg,
            AdjTypeEnum.EvadeChance, AdjTypeEnum.EvadeChanceMax,
            AdjTypeEnum.SCritChance, AdjTypeEnum.SCritChanceMax, AdjTypeEnum.SCritDamage
        };

        public IDictionary<string, long> RefineExp { get; set; } = new Dictionary<string, long>();
        public IDictionary<string, CustomRefine> CustomRefine { get; set; } = new Dictionary<string, CustomRefine>();

        public override void OnMonthly()
        {
            base.OnMonthly();

            foreach (var wunit in g.world.unit.GetUnits())
            {
                if (wunit.IsPlayer())
                    continue;
                foreach (var item in wunit.GetUnitProps())
                {
                    if (IsRefinableItem(item))
                    {
                        NpcRefine(wunit, item);
                    }
                }
            }
            ClearCacheCustomAdjValues();
        }

        private void NpcRefine(WorldUnitBase wunit, DataProps.PropsData item)
        {
            if (item == null)
                return;
            var money = wunit.GetUnitMoney();
            var spend = Convert.ToInt32(Math.Sqrt(money));
            AddRefineExp(item, spend);
            //UnitModifyHelper.ClearCacheCustomAdjValues(wunit);
            wunit.AddUnitMoney(-spend);
        }

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);

            if (e.uiType.uiName == UIType.TownFactoryShapeOption.uiName)
            {
                var uiTownFactoryShapeOption = g.ui.GetUI<UITownFactoryShapeOption>(UIType.TownFactoryShapeOption);

                if (!uiTownFactoryShapeOption.town.buildTownData.isMainTown)
                {
                    var customEnhanceBtn = uiTownFactoryShapeOption.btnUpgrade.Replace();
                    customEnhanceBtn.onClick.AddListener((UnityAction)(() =>
                    {
                        OpenItemSelector();
                    }));
                }
            }
        }

        private void OpenItemSelector()
        {
            var ui = g.ui.OpenUI<UIPropSelect>(UIType.PropSelect);
            ui.textTitle1.text = "Select item";
            ui.textSearchTip.text = "Step 1";
            ui.btnSearch.gameObject.SetActive(false);
            ui.goTabRoot.SetActive(false);
            ui.goSubToggleRoot.SetActive(false);
            ui.ClearSelectItem();
            ui.selectOnePropID = true;
            ui.allItems = new DataProps
            {
                allProps = new Il2CppSystem.Collections.Generic.List<DataProps.PropsData>()
            };
            foreach (var item in g.world.playerUnit.GetUnitProps())
            {
                if (IsRefinableItem(item))
                {
                    ui.allItems.AddProps(item);
                }
            }
            ui.btnOK.onClick.RemoveAllListeners();
            ui.btnOK.onClick.AddListener((UnityAction)(() =>
            {
                var selectedItem = UIPropSelect.allSlectDataProps.allProps.ToArray().FirstOrDefault();
                OpenMaterialSelector(selectedItem);
            }));
            ui.UpdateUI();
        }

        private void OpenMaterialSelector(DataProps.PropsData refineItem)
        {
            if (refineItem == null)
                return;
            var ui = g.ui.OpenUI<UIPropSelect>(UIType.PropSelect);
            ui.textTitle1.text = "Select materials";
            ui.textSearchTip.text = "Step 2";
            ui.btnSearch.gameObject.SetActive(false);
            ui.goTabRoot.SetActive(false);
            ui.goSubToggleRoot.SetActive(false);
            ui.ClearSelectItem();
            ui.allItems = new DataProps
            {
                allProps = new Il2CppSystem.Collections.Generic.List<DataProps.PropsData>()
            };
            foreach (var item in g.world.playerUnit.GetUnitProps())
            {
                if (IsRefinableMaterial(item))
                {
                    ui.allItems.AddProps(item);
                }
            }
            ui.btnOK.onClick.RemoveAllListeners();
            ui.btnOK.onClick.AddListener((UnityAction)(() =>
            {
                var oldLvl = GetRefineLvl(refineItem);

                var value = UIPropSelect.allSlectDataProps.allProps.ToArray().Sum(x => x.propsInfoBase.worth * x.propsCount);
                AddRefineExp(refineItem, value);
                ClearCacheCustomAdjValues(g.world.playerUnit);
                g.world.playerUnit.AddUnitMoney(-value);
                g.ui.CloseUI(ui);

                var uiConfirm = g.ui.OpenUI<UICheckPopup>(UIType.CheckPopup);
                uiConfirm.InitData("Refine", $"Success! Level {oldLvl}→{GetRefineLvl(refineItem)}", 1);
            }));
            ui.UpdateUI();
        }

        public static bool IsRefinableItem(DataProps.PropsData props)
        {
            return props?.propsItem?.IsRing() != null || props?.propsItem?.IsOutfit() != null || props?.propsItem?.IsArtifact() != null;
        }

        public static bool IsRefinableMaterial(DataProps.PropsData props)
        {
            return props.propsID == ModLibConst.MONEY_PROP_ID;
        }

        public static void AddRefineExp(string soleId, long exp)
        {
            if (string.IsNullOrEmpty(soleId) || exp == 0)
                return;
            var x = EventHelper.GetEvent<CustomRefineEvent>(ModConst.CUSTOM_REFINE_EVENT);
            if (!x.RefineExp.ContainsKey(soleId))
                x.RefineExp.Add(soleId, 0);
            x.RefineExp[soleId] += exp;
        }

        public static void AddRefineExp(DataProps.PropsData props, long exp)
        {
            AddRefineExp(props?.soleID, exp);
        }

        public static long GetRefineExp(string soleId)
        {
            if (string.IsNullOrEmpty(soleId))
                return 0;
            var x = EventHelper.GetEvent<CustomRefineEvent>(ModConst.CUSTOM_REFINE_EVENT);
            if (!x.RefineExp.ContainsKey(soleId))
                x.RefineExp.Add(soleId, 0);
            return x.RefineExp[soleId];
        }

        public static long GetRefineExp(DataProps.PropsData props)
        {
            return GetRefineExp(props?.soleID);
        }

        public static double GetRefineExpNeed(DataProps.PropsData props, int lvl)
        {
            return (props.propsInfoBase.grade * 100 + props.propsInfoBase.level * 20) * Math.Pow(1.04d, lvl);
        }

        public static int GetRefineLvl(DataProps.PropsData props)
        {
            if (props == null)
                return 0;
            var curExp = GetRefineExp(props);
            for (int lvl = 0; ; lvl++)
            {
                if (GetRefineExpNeed(props, lvl) > curExp)
                    return lvl;
            }
        }

        public static AdjTypeEnum[] GetCustomAdjSeeder(DataProps.PropsData props)
        {
            if (props?.propsItem?.IsRing() != null)
                return RingAdjTypes;
            if (props?.propsItem?.IsOutfit() != null)
                return OutfitAdjTypes;
            if (props?.propsItem?.IsArtifact() != null)
                return ArtifactAdjTypes;
            return null;
        }

        public static CustomRefine GetCustomAdjType(DataProps.PropsData props, int index, AdjTypeEnum condition = null)
        {
            if (props == null)
                return null;
            if (index < 1 || index > 5) //soleID.MaxLength = 6
                return null;
            var seeder = GetCustomAdjSeeder(props);
            if (seeder == null)
                return null;
            var key = $"{props.soleID}_{index}";
            var x = EventHelper.GetEvent<CustomRefineEvent>(ModConst.CUSTOM_REFINE_EVENT);
            if (!x.CustomRefine.ContainsKey(key))
                x.CustomRefine.Add(key, new CustomRefine()
                {
                    Index = index,
                    AdjType = seeder[props.soleID[index - 1] % seeder.Length],
                    AdjLevel = AdjLevelEnum.GetLevel(props.soleID[index]),
                    RandomMultiplier = CommonTool.Random(0.50f, 1.50f)
                });
            var rs = x.CustomRefine[key];
            if (condition != null && rs.AdjType != condition)
                return null;
            return rs;
        }

        private static readonly IDictionary<string, double> _cacheCustomAdjValues = new Dictionary<string, double>();
        public static double GetRefineCustommAdjValue(WorldUnitBase wunit, AdjTypeEnum adjType)
        {
            if (wunit == null || adjType == null)
                return 0.0;
            var key = $"{wunit.GetUnitId()}_{adjType.Name}";
            if (_cacheCustomAdjValues.ContainsKey(key))
                return _cacheCustomAdjValues[key];
            var rs = 0d;
            foreach (var props in wunit.GetUnitProps())
            {
                if (CustomRefineEvent.IsRefinableItem(props))
                {
                    var refineLvl = CustomRefineEvent.GetRefineLvl(props);
                    if (props.propsItem.IsArtifact() != null)
                    {
                        rs += CustomRefineEvent.GetCustomAdjType(props, 1, adjType)?.GetRefineCustommAdjValue(wunit, props, refineLvl) ?? 0;
                        rs += CustomRefineEvent.GetCustomAdjType(props, 2, adjType)?.GetRefineCustommAdjValue(wunit, props, refineLvl) ?? 0;
                        rs += CustomRefineEvent.GetCustomAdjType(props, 3, adjType)?.GetRefineCustommAdjValue(wunit, props, refineLvl) ?? 0;
                    }
                    else if (props.propsItem.IsRing() != null)
                    {
                        rs += CustomRefineEvent.GetCustomAdjType(props, 1, adjType)?.GetRefineCustommAdjValue(wunit, props, refineLvl) ?? 0;
                        rs += CustomRefineEvent.GetCustomAdjType(props, 2, adjType)?.GetRefineCustommAdjValue(wunit, props, refineLvl) ?? 0;
                    }
                    else if (props.propsItem.IsOutfit() != null)
                    {
                        rs += CustomRefineEvent.GetCustomAdjType(props, 1, adjType)?.GetRefineCustommAdjValue(wunit, props, refineLvl) ?? 0;
                        rs += CustomRefineEvent.GetCustomAdjType(props, 2, adjType)?.GetRefineCustommAdjValue(wunit, props, refineLvl) ?? 0;
                    }
                }
            }
            _cacheCustomAdjValues.Add(key, rs);
            return rs;
        }

        public static void ClearCacheCustomAdjValues()
        {
            _cacheCustomAdjValues.Clear();
        }

        public static void ClearCacheCustomAdjValues(WorldUnitBase wunit)
        {
            foreach (var adjType in AdjTypeEnum.GetAllEnums<AdjTypeEnum>())
            {
                var key = $"{wunit.GetUnitId()}_{adjType.Name}";
                _cacheCustomAdjValues.Remove(key);
            }
        }
    }
}
