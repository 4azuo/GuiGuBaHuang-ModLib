using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using System.Linq;
using ModLib.Const;
using System.Collections.Generic;
using UnityEngine.Events;
using System;
using MOD_nE7UL2.Enum;
using MOD_nE7UL2.Object;
using ModLib.Enum;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.CUSTOM_REFINE_EVENT)]
    public class CustomRefineEvent : ModEvent
    {
        public const int REFINE_EXP_RATE = 200;

        public Dictionary<string, long> RefineExp { get; set; } = new Dictionary<string, long>();
        public Dictionary<string, CustomRefine> CustomRefine { get; set; } = new Dictionary<string, CustomRefine>();

        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);
            var location = wunit.data.unitData.GetPoint();
            var town = g.world.build.GetBuild<MapBuildTown>(location);
            if (!wunit.IsPlayer() && (town?.buildTownData?.isMainTown).Is(false) == 1 && wunit.GetUnitMoney() > MapBuildPropertyEvent.GetTax(wunit) * 10)
            {
                foreach (var item in GetRefinableItems(wunit))
                {
                    NpcRefine(wunit, item);
                }
            }
        }

        private void NpcRefine(WorldUnitBase wunit, DataProps.PropsData item)
        {
            if (item == null)
                return;
            var money = wunit.GetUnitMoney();
            var exp = money * wunit.GetDynProperty(UnitDynPropertyEnum.RefineWeapon).value / REFINE_EXP_RATE;
            var spend = Convert.ToInt32(Math.Sqrt(money));
            AddRefineExp(item, exp);
            //UnitModifyHelper.ClearCacheCustomAdjValues(wunit);
            wunit.AddUnitMoney(-spend);
        }

        [EventCondition]
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
                var exp = value * g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.RefineWeapon).value / REFINE_EXP_RATE;
                AddRefineExp(refineItem, exp);
                g.world.playerUnit.AddUnitMoney(-value);
                g.ui.CloseUI(ui);

                var uiConfirm = g.ui.OpenUI<UICheckPopup>(UIType.CheckPopup);
                uiConfirm.InitData("Refine", $"Success! Level {oldLvl}→{GetRefineLvl(refineItem)} (+{exp}Exp)", 1);
            }));
            ui.UpdateUI();
        }

        public static List<DataProps.PropsData> GetRefinableItems(WorldUnitBase wunit)
        {
            return wunit.GetUnitProps().Where(x => IsRefinableItem(x)).ToList();
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
            var smConfigs = EventHelper.GetEvent<SMLocalConfigsEvent>(ModConst.SM_LOCAL_CONFIGS_EVENT);
            return smConfigs.Calculate((props.propsInfoBase.grade * 100 + props.propsInfoBase.level * 20) * Math.Pow(1.04d, lvl), smConfigs.Configs.AddRefineCost);
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

        public static CustomRefine GetCustomAdjType(DataProps.PropsData props, int index)
        {
            if (!IsRefinableItem(props))
                return null;
            if (index < 1 || index > 5) //soleID.MaxLength = 6
                return null;
            var key = $"{props.soleID}_{index}";
            var x = EventHelper.GetEvent<CustomRefineEvent>(ModConst.CUSTOM_REFINE_EVENT);
            if (!x.CustomRefine.ContainsKey(key))
                x.CustomRefine.Add(key, new CustomRefine(props, index));
            var rs = x.CustomRefine[key];
            return rs;
        }

        public static List<CustomRefine> GetCustomAdjTypes(DataProps.PropsData props, AdjTypeEnum condition)
        {
            var list = new List<CustomRefine>();
            var x = EventHelper.GetEvent<CustomRefineEvent>(ModConst.CUSTOM_REFINE_EVENT);
            for (int i = 1; i <= 5; i++)
            {
                var key = $"{props.soleID}_{i}";
                if (x.CustomRefine.ContainsKey(key) && x.CustomRefine[key].AdjType == condition)
                {
                    list.Add(x.CustomRefine[key]);
                }
            }
            return list;
        }

        public static double GetRefineCustommAdjValue(WorldUnitBase wunit, AdjTypeEnum adjType)
        {
            if (wunit == null || adjType == null)
                return 0d;
            var rs = 0d;
            foreach (var props in GetRefinableItems(wunit))
            {
                var refineLvl = GetRefineLvl(props);
                foreach (var a in GetCustomAdjTypes(props, adjType))
                {
                    rs += a.GetRefineCustommAdjValue(wunit, props, refineLvl);
                }
            }
            return rs;
        }
    }
}
