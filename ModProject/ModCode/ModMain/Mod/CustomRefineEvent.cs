using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using System.Linq;
using ModLib.Const;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.CUSTOM_REFINE_EVENT)]
    public class CustomRefineEvent : ModEvent
    {
        public IDictionary<string, long> RefineExp { get; set; } = new Dictionary<string, long>();

        public override void OnMonthly()
        {
            base.OnMonthly();

            foreach (var wunit in g.world.unit.GetUnits())
            {
                foreach (var item in wunit.GetUnitProps())
                {
                    if (IsRefinableItem(item))
                    {
                        var money = wunit.GetUnitMoney();
                        var spend = Math.Max(Convert.ToInt32(Math.Sqrt(money)), 100);
                        AddRefineExp(item, spend);
                        wunit.AddUnitMoney(-spend);
                    }
                }
            }
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
                var value = UIPropSelect.allSlectDataProps.allProps.ToArray().Sum(x => x.propsInfoBase.worth * x.propsCount);
                AddRefineExp(refineItem, value);
                g.world.playerUnit.AddUnitMoney(-value);
                g.ui.CloseUI(ui);
            }));
            ui.UpdateUI();
        }

        public static bool IsRefinableItem(DataProps.PropsData props)
        {
            return props.propsItem.IsRing() != null || props.propsItem.IsOutfit() != null || props.propsItem.IsArtifact() != null;
        }

        public static bool IsRefinableMaterial(DataProps.PropsData props)
        {
            return props.propsID == ModLibConst.MONEY_PROP_ID;
        }

        public static void AddRefineExp(string soleId, long exp)
        {
            if (string.IsNullOrEmpty(soleId))
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

        public static int GetRefineLvl(string soleId)
        {
            if (string.IsNullOrEmpty(soleId))
                return 0;
            var curExp = GetRefineExp(soleId);
            for (int lvl = 0; ; lvl++)
            {
                if (1000 * Math.Pow(1.10d, lvl) > curExp)
                    return lvl;
            }
        }

        public static int GetRefineLvl(DataProps.PropsData props)
        {
            return GetRefineLvl(props?.soleID);
        }
    }
}
