﻿using EGameTypeData;
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
using Newtonsoft.Json;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.CUSTOM_REFINE_EVENT)]
    public class CustomRefineEvent : ModEvent
    {
        public static CustomRefineEvent Instance { get; set; }

        public const int REFINE_EXP_RATE = 200;
        public const int MAX_ADJ_PER_PROP = 5;

        [JsonIgnore]
        public Dictionary<string, double> CachedValues { get; } = new Dictionary<string, double>();
        public Dictionary<string, long> RefineExp { get; set; } = new Dictionary<string, long>();
        public Dictionary<string, CustomRefine> CustomRefine { get; set; } = new Dictionary<string, CustomRefine>();

        public override void OnMonthly()
        {
            base.OnMonthly();
            CachedValues.Clear();
        }

        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);
            var location = wunit.GetUnitPos();
            var town = g.world.build.GetBuild<MapBuildTown>(location);
            if (!wunit.IsPlayer() && town.IsSmallTown() && wunit.GetUnitMoney() > MapBuildPropertyEvent.GetTax(town, wunit) * 3)
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
            wunit.AddUnitMoney(-spend);
            MapBuildPropertyEvent.AddBuildProperty(wunit.GetMapBuild<MapBuildBase>(), spend);
        }

        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
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
            var ui = g.ui.OpenUISafe<UIPropSelect>(UIType.PropSelect);
            ui.textTitle1.text = GameTool.LS("other500020001");
            ui.textSearchTip.text = GameTool.LS("other500020048");
            ui.btnSearch.gameObject.SetActive(false);
            ui.goTabRoot.SetActive(false);
            ui.goSubToggleRoot.SetActive(false);
            ui.ClearSelectItem();
            ui.selectOnePropID = true;
            ui.allItems.ClearAllProps();
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
            var ui = g.ui.OpenUISafe<UIPropSelect>(UIType.PropSelect);
            ui.textTitle1.text = GameTool.LS("other500020002");
            ui.textSearchTip.text = GameTool.LS("other500020049");
            ui.btnSearch.gameObject.SetActive(false);
            ui.goTabRoot.SetActive(false);
            ui.goSubToggleRoot.SetActive(false);
            ui.ClearSelectItem();
            ui.allItems.ClearAllProps();
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

                g.ui.MsgBox(GameTool.LS("other500020050"), string.Format(GameTool.LS("other500020051"), oldLvl, GetRefineLvl(refineItem), exp));
                CachedValues.Remove(g.world.playerUnit.GetUnitId());
            }));
            ui.UpdateUI();
        }

        public static List<DataProps.PropsData> GetRefinableItems(WorldUnitBase wunit)
        {
            return wunit.GetUnitProps().Where(x => IsRefinableItem(x)).ToList();
        }

        public static bool IsRefinableItem(DataProps.PropsData props)
        {
            return props?.propsItem?.IsRing() != null || props?.propsItem?.IsOutfit() != null || props?.propsItem?.IsArtifact() != null || props?.propsItem?.IsMount() != null;
        }

        public static bool IsRefinableMaterial(DataProps.PropsData props)
        {
            return props.propsID == ModLibConst.MONEY_PROP_ID;
        }

        public static void AddRefineExp(string soleId, long exp)
        {
            if (string.IsNullOrEmpty(soleId) || exp == 0)
                return;
            if (!Instance.RefineExp.ContainsKey(soleId))
                Instance.RefineExp.Add(soleId, 0);
            Instance.RefineExp[soleId] += exp;
        }

        public static void AddRefineExp(DataProps.PropsData props, long exp)
        {
            AddRefineExp(props?.soleID, exp);
        }

        public static long GetRefineExp(string soleId)
        {
            if (string.IsNullOrEmpty(soleId))
                return 0;
            if (!Instance.RefineExp.ContainsKey(soleId))
                Instance.RefineExp.Add(soleId, 0);
            return Instance.RefineExp[soleId];
        }

        public static long GetRefineExp(DataProps.PropsData props)
        {
            return GetRefineExp(props?.soleID);
        }

        public static double GetRefineExpNeed(DataProps.PropsData props, int lvl)
        {
            return SMLocalConfigsEvent.Instance.Calculate((props.propsInfoBase.grade * 100 + props.propsInfoBase.level * 20) * Math.Pow(1.04d, lvl), SMLocalConfigsEvent.Instance.Configs.AddRefineCost);
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
            if (index < 1 || index > MAX_ADJ_PER_PROP) //soleID.MaxLength = 6
                return null;
            var key = $"{props.soleID}_{index}";
            if (!Instance.CustomRefine.ContainsKey(key))
                Instance.CustomRefine.Add(key, new CustomRefine(props, index));
            var rs = Instance.CustomRefine[key];
            return rs;
        }

        public static List<CustomRefine> GetCustomAdjTypes(DataProps.PropsData props)
        {
            var list = new List<CustomRefine>();
            for (int i = 1; i <= MAX_ADJ_PER_PROP; i++)
            {
                list.Add(GetCustomAdjType(props, i));
            }
            return list;
        }

        public static List<CustomRefine> GetCustomAdjTypes(DataProps.PropsData props, AdjTypeEnum condition)
        {
            return GetCustomAdjTypes(props).Where(x => x.AdjType == condition).ToList();
        }

        public static double GetCustomAdjValue(WorldUnitBase wunit, AdjTypeEnum adjType)
        {
            if (wunit == null || adjType == null)
                return 0d;
            var unitId = wunit.GetUnitId();
            if (!Instance.CachedValues.ContainsKey(unitId))
            {
                var rs = 0d;
                foreach (var props in GetRefinableItems(wunit))
                {
                    var refineLvl = GetRefineLvl(props);
                    foreach (var a in GetCustomAdjTypes(props, adjType))
                    {
                        rs += a.GetRefineCustommAdjValue(wunit, props, refineLvl);
                    }
                }
                Instance.CachedValues[unitId] = rs;
            }
            return Instance.CachedValues[unitId];
        }

        public static void CopyAdj(DataProps.PropsData fromProp, DataProps.PropsData toProp)
        {
            for (int i = 1; i <= MAX_ADJ_PER_PROP; i++)
            {
                var toKey = $"{toProp.soleID}_{i}";
                Instance.CustomRefine[toKey] = GetCustomAdjType(fromProp, i).Clone();
            }
            Instance.RefineExp[toProp.soleID] = GetRefineExp(fromProp);
        }
    }
}
