using EGameTypeData;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using MOD_nE7UL2.Object;
using ModLib.Const;
using ModLib.Enum;
using ModLib.Mod;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.CUSTOM_REFINE_EVENT)]
    public class CustomRefineEvent : ModEvent
    {
        public static CustomRefineEvent Instance { get; set; }

        public const int REFINE_EXP_RATE = 200;
        public const int MAX_ADJ_PER_PROP = 5;

        [JsonIgnore]
        public IDictionary<string, double> CachedValues { get; } = new Dictionary<string, double>();
        public IDictionary<string, long> RefineExp { get; set; } = new Dictionary<string, long>();
        public IDictionary<string, CustomRefine> CustomRefine { get; set; } = new Dictionary<string, CustomRefine>();

        public override void OnMonthly()
        {
            base.OnMonthly();
            CachedValues.Clear();
        }

        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);
            if (GameHelper.GetGameTotalMonth() % SMLocalConfigsEvent.Instance.Configs.GrowUpSpeed == 0)
            {
                var location = wunit.GetUnitPos();
                var town = g.world.build.GetBuild<MapBuildTown>(location);
                if (!wunit.IsPlayer() && town.IsSmallTown() && wunit.GetUnitMoney() > MapBuildPropertyEvent.GetTax(town, wunit) * 3)
                {
                    foreach (var item in GetEquippedRefinableItems(wunit))
                    {
                        NpcRefine(wunit, item);
                    }
                }
            }
        }

        private void NpcRefine(WorldUnitBase wunit, DataProps.PropsData item)
        {
            if (item == null)
                return;
            var money = wunit.GetUnitMoney();
            var spend = Convert.ToInt32(Math.Sqrt(money));
            var exp = spend * wunit.GetDynProperty(UnitDynPropertyEnum.RefineWeapon).value / REFINE_EXP_RATE;
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
            // Custom select để lấy prop
            DataProps.PropsData selectedItem = null;
            ui.onCustomSelectCall = (ReturnAction<string, DataProps.PropsData>)((x) =>
            {
                ui.ClearSelectItem();
                ui.AddSelectProps(x);
                ui.UpdateUI();
                selectedItem = x;
                return selectedItem.propsInfoBase.name;
            });
            ui.btnOK.onClick.RemoveAllListeners();
            ui.btnOK.onClick.AddListener((UnityAction)(() =>
            {
                if (selectedItem == null)
                    return;
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
            // Custom select để lấy prop
            ui.onCustomSelectCall = null;
            ui.btnOK.onClick.RemoveAllListeners();
            ui.btnOK.onClick.AddListener((UnityAction)(() =>
            {
                var oldLvl = GetRefineLvl(refineItem);

                var value = UIPropSelect.allSlectDataProps.allProps.ToArray().Sum(x => x.propsInfoBase.worth * x.propsCount);
                var exp = value * g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.RefineWeapon).value / REFINE_EXP_RATE;

                AddRefineExp(refineItem, exp);

                g.world.playerUnit.AddUnitMoney(-value);
                MapBuildPropertyEvent.AddBuildProperty(g.world.playerUnit.GetMapBuild<MapBuildBase>(), value);

                g.ui.CloseUI(ui);

                g.ui.MsgBox(GameTool.LS("other500020050"), string.Format(GameTool.LS("other500020051"), oldLvl, GetRefineLvl(refineItem), exp));
                CachedValues.RemoveKeysStartWith(refineItem.soleID);
            }));
            ui.UpdateUI();
        }

        public static List<DataProps.PropsData> GetEquippedRefinableItems(WorldUnitBase wunit)
        {
            return wunit.GetEquippedProps().Where(x => IsRefinableItem(x)).ToList();
        }

        public static bool IsRefinableItem(DataProps.PropsData props)
        {
            return props?.propsItem?.IsRing() != null || props?.propsItem?.IsOutfit() != null || props?.propsItem?.IsArtifact() != null || props?.propsItem?.IsMount() != null;
        }

        public static bool IsRefinableMaterial(DataProps.PropsData props)
        {
            return props.propsID == ModLibConst.MONEY_PROP_ID;
        }

        public static bool AddRefineExp(string soleId, long exp)
        {
            if (string.IsNullOrEmpty(soleId) || exp == 0)
                return false;
            if (!Instance.RefineExp.ContainsKey(soleId))
                Instance.RefineExp.Add(soleId, 0);
            Instance.RefineExp[soleId] += exp;
            return true;
        }

        public static bool AddRefineExp(DataProps.PropsData props, long exp)
        {
            return AddRefineExp(props?.soleID, exp);
        }

        public static long GetRefineExp(string soleId)
        {
            if (string.IsNullOrEmpty(soleId))
                return 0;
            if (!Instance.RefineExp.ContainsKey(soleId))
                return 0;
            return Instance.RefineExp[soleId];
        }

        public static long GetRefineExp(DataProps.PropsData props)
        {
            return GetRefineExp(props?.soleID);
        }

        public static int GetRefineLvl(DataProps.PropsData props)
        {
            if (props == null)
                return 0;

            var curExp = GetRefineExp(props);
            var tempLevel1ExpNeed = SMLocalConfigsEvent.Instance.Calculate(props.propsInfoBase.grade * 100 + props.propsInfoBase.level * 20, SMLocalConfigsEvent.Instance.Configs.AddRefineCost);
            var tempLevel = curExp / tempLevel1ExpNeed;
            var realLevel1ExpNeed = Math.Max(tempLevel1ExpNeed * Math.Sqrt(tempLevel), 1);
            var realLevel = curExp / realLevel1ExpNeed;

            return Convert.ToInt32(realLevel);
        }

        public static CustomRefine GetCustomAdjType(DataProps.PropsData props, int index)
        {
            if (!IsRefinableItem(props))
                return null;
            if (index < 1 || index > MAX_ADJ_PER_PROP)
                return null;
            var key = $"{props.soleID}_{index}";
            if (!Instance.CustomRefine.ContainsKey(key))
                Instance.CustomRefine.Add(key, new CustomRefine(props, index));
            var rs = Instance.CustomRefine[key];
            return rs;
        }

        public static List<CustomRefine> GetCustomAdjTypes(DataProps.PropsData props)
        {
            return Instance.CustomRefine.Keys.Where(x => x.StartsWith(props.soleID)).Select(x => Instance.CustomRefine[x]).ToList();
        }

        public static List<CustomRefine> GetCustomAdjTypes(DataProps.PropsData props, AdjTypeEnum condition)
        {
            return GetCustomAdjTypes(props).Where(x => x.AdjType == condition).ToList();
        }

        public static double GetCustomAdjValue(WorldUnitBase wunit, AdjTypeEnum adjType, dynamic optionalParams = null)
        {
            if (wunit == null || adjType == null)
                return 0d;

            var rs = 0d;
            var unitId = wunit.GetUnitId();

            foreach (var props in GetEquippedRefinableItems(wunit))
            {
                var refineLvl = GetRefineLvl(props);
                foreach (var a in GetCustomAdjTypes(props, adjType))
                {
                    rs += a.GetRefineCustommAdjValue(wunit, props, refineLvl, optionalParams);
                }
            }

            return rs;
        }

        public static void CopyAdj(DataProps.PropsData fromProp, DataProps.PropsData toProp)
        {
            var i = 1;
            foreach (var fromAdj in GetCustomAdjTypes(fromProp))
            {
                var toKey = $"{toProp.soleID}_{i}";
                Instance.CustomRefine[toKey] = GetCustomAdjType(fromProp, i).Clone();
                i++;
            }
            Instance.RefineExp[toProp.soleID] = GetRefineExp(fromProp);
        }
    }
}
