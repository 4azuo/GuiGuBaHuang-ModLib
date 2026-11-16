using EGameTypeData;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using MOD_nE7UL2.Object;
using ModLib.Attributes;
using ModLib.Const;
using ModLib.Enum;
using ModLib.Mod;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using ModLib.Helper;
using ModLib.Object;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Custom refine: spend spirit stones on rings/outfits/artifacts/mounts to gain refine exp, refine levels and random adjustment slots.
    /// </summary>
    [Cache(ModConst.CUSTOM_REFINE_EVENT)]
    public class CustomRefineEvent : ModEvent
    {
        public static CustomRefineEvent Instance { get; set; }

        public const int REFINE_EXP_RATE = 40;
        public const int MAX_ADJ_PER_PROP = 5;

        [JsonIgnore]
        public IDictionary<string, double> CachedValues { get; } = new Dictionary<string, double>();
        public IDictionary<string, long> RefineExp { get; set; } = new Dictionary<string, long>();
        public IDictionary<string, CustomRefine> CustomRefine { get; set; } = new Dictionary<string, CustomRefine>();

        /// <summary>
        /// Clears cached adjustment values.
        /// </summary>
        public override void OnMonthly()
        {
            base.OnMonthly();
            CachedValues.Clear();
        }

        /// <summary>
        /// Rich NPCs in small towns refine their equipment.
        /// </summary>
        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);
            var ticks = ModLib.Mod.ModMaster.Ticks(SMLocalConfigsEvent.Instance.Configs.GrowUpSpeed);
            for (var t = 0; t < ticks; t++)
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

        /// <summary>
        /// NPC spends sqrt(money) on an item.
        /// </summary>
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

        /// <summary>
        /// Small town forge: replaces the upgrade button with custom refine.
        /// </summary>
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
                    customEnhanceBtn.onClick.AddListener(ModLib.Helper.ActionHelper.TracedUnityAction(() =>
                    {
                        OpenItemSelector();
                    }));
                }
            }
        }

        /// <summary>
        /// Step 1: choose the item.
        /// </summary>
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
            ui.onCustomSelectCall = ModLib.Helper.ActionHelper.TracedFunc<DataProps.PropsData, string>((x) =>
            {
                ui.ClearSelectItem();
                ui.AddSelectProps(x);
                ui.UpdateUI();
                selectedItem = x;
                return selectedItem.propsInfoBase.name;
            });
            ui.btnOK.onClick.RemoveAllListeners();
            ui.btnOK.onClick.AddListener(ModLib.Helper.ActionHelper.TracedUnityAction(() =>
            {
                if (selectedItem == null)
                    return;
                OpenMaterialSelector(selectedItem);
            }));
            // progress of the selected item, and its slot rerolls
            var cover = new UICover<UIPropSelect>(ui);
            cover.AddText(0, 0, "{0}").Format(null, 14).SetWork(new UIItemWork
            {
                Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>(x => new object[] { selectedItem == null ? GameTool.LS("refine_pick") : ProgressText(selectedItem) })
            }).Pos(ui.btnOK.transform, 0, 60);
            cover.AddButton(0, 0, () =>
            {
                if (selectedItem != null)
                    OpenReroll(selectedItem);
            }, GameTool.LS("refine_reroll")).Size(160, 36).Pos(ui.btnOK.transform, 200, 0);
            cover.IsAutoUpdate = true;
            ui.UpdateUI();
        }

        /// <summary>Exp needed for a refine level (GetRefineLvl = round(sqrt(exp / level-1 exp))).</summary>
        public static long ExpForLevel(DataProps.PropsData props, int level)
        {
            var lvl1 = SMLocalConfigsEvent.Instance.Calculate(props.propsInfoBase.grade * 100 + props.propsInfoBase.level * 20, SMLocalConfigsEvent.Instance.Configs.AddRefineCost);
            var root = Math.Max(0d, level - 0.5d);
            return (long)Math.Ceiling(lvl1 * root * root);
        }

        /// <summary>"Refine level L (exp / next) - next slot at N".</summary>
        public static string ProgressText(DataProps.PropsData props)
        {
            var lvl = GetRefineLvl(props);
            var nextSlot = Enumerable.Range(1, MAX_ADJ_PER_PROP).Select(i => i * 100).FirstOrDefault(x => x > lvl);
            var slotText = nextSlot > 0 ? string.Format(GameTool.LS("refine_nextslot"), nextSlot) : GameTool.LS("refine_allslots");
            return string.Format(GameTool.LS("refine_progress"), lvl, GetRefineExp(props), ExpForLevel(props, lvl + 1), slotText);
        }

        /// <summary>Spirit stones to reroll a slot: 1,000 x grade² x slot index (inflation).</summary>
        public static int RerollCost(DataProps.PropsData props, int index)
        {
            var grade = Math.Max(1, props.propsInfoBase.grade);
            return InflationaryEvent.CalculateInflationary(1000 * grade * grade * index);
        }

        /// <summary>Rerolls an unlocked slot (new type, quality and multiplier), paid to the town budget. False when locked/unpaid.</summary>
        public static bool Reroll(DataProps.PropsData props, int index)
        {
            var player = g.world.playerUnit;
            var slot = GetCustomAdjType(props, index);
            var cost = RerollCost(props, index);
            if (slot == null || !slot.IsEnable(GetRefineLvl(props)) || player.GetUnitMoney() < cost)
                return false;
            player.AddUnitMoney(-cost);
            MapBuildPropertyEvent.AddBuildProperty(player.GetMapBuild<MapBuildBase>(), cost);
            var old = slot.GetText(player, props, GetRefineLvl(props));
            Instance.CustomRefine[$"{props.soleID}_{index}"] = new CustomRefine(props, index);
            Instance.CachedValues.RemoveKeysStartWith(props.soleID);
            var now = GetCustomAdjType(props, index).GetText(player, props, GetRefineLvl(props));
            TaoistLogEvent.Log("log_refine_reroll", props.propsInfoBase.name, index, old, now, cost);
            DebugHelper.WriteLine($"CustomRefineEvent: reroll {props.propsInfoBase.name} #{index}: {old} -> {now}");
            return true;
        }

        /// <summary>Reroll window: every slot with its value, a reroll button on the unlocked ones.</summary>
        public void OpenReroll(DataProps.PropsData props)
        {
            g.ui.CloseUI(UIType.TextInfoLong);
            var player = g.world.playerUnit;
            var lvl = GetRefineLvl(props);
            var ui = new UICustom1(GameTool.LS("refine_reroll"));
            var row = ui.FirstRow + 1;
            ui.AddText(ui.MidCol, row++, props.propsInfoBase.name).Format(null, 18);
            ui.AddText(ui.MidCol, row++, ProgressText(props)).Format(null, 14);
            row++;
            for (var i = 1; i <= MAX_ADJ_PER_PROP; i++)
            {
                var index = i;
                var slot = GetCustomAdjType(props, index);
                ui.AddText(ui.MidCol - 3, row, $"#{index}  {slot.GetText(player, props, lvl)}").Format(slot.GetColor(lvl), 15);
                if (slot.IsEnable(lvl))
                {
                    ui.AddButton(ui.MidCol + 8, row, () =>
                    {
                        if (!Reroll(props, index))
                            g.ui.MsgBox(GameTool.LS("refine_reroll"), GameTool.LS("other500020087"));
                        OpenReroll(props);
                    }, string.Format(GameTool.LS("refine_reroll_btn"), RerollCost(props, index))).Size(200, 32);
                }
                row++;
            }
            ui.AddText(ui.MidCol, ui.LastRow - 1, GameTool.LS("refine_reroll_desc")).Format(null, 13);
        }

        /// <summary>
        /// Step 2: choose spirit stones; adds exp and shows the new level.
        /// </summary>
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
            ui.btnOK.onClick.AddListener(ModLib.Helper.ActionHelper.TracedUnityAction(() =>
            {
                var oldLvl = GetRefineLvl(refineItem);

                var value = UIPropSelect.allSlectDataProps.allProps.ToArray().Sum(x => x.propsInfoBase.worth * x.propsCount);
                // long: many stones x a high refine skill overflow an int
                var exp = (long)value * g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.RefineWeapon).value / REFINE_EXP_RATE;

                AddRefineExp(refineItem, exp);

                g.world.playerUnit.AddUnitMoney(-value);
                MapBuildPropertyEvent.AddBuildProperty(g.world.playerUnit.GetMapBuild<MapBuildBase>(), value);

                g.ui.CloseUI(ui);

                g.ui.MsgBox(GameTool.LS("other500020050"), string.Format(GameTool.LS("other500020051"), oldLvl, GetRefineLvl(refineItem), exp));
                CachedValues.RemoveKeysStartWith(refineItem.soleID);
            }));
            ui.UpdateUI();
        }

        /// <summary>
        /// Equipped refinable items.
        /// </summary>
        public static List<DataProps.PropsData> GetEquippedRefinableItems(WorldUnitBase wunit)
        {
            return wunit.GetEquippedProps().Where(x => IsRefinableItem(x)).ToList();
        }

        /// <summary>
        /// Ring, outfit, artifact or mount.
        /// </summary>
        public static bool IsRefinableItem(DataProps.PropsData props)
        {
            return props?.propsItem?.IsRing() != null || props?.propsItem?.IsOutfit() != null || props?.propsItem?.IsArtifact() != null || props?.propsItem?.IsMount() != null;
        }

        /// <summary>
        /// Spirit stones.
        /// </summary>
        public static bool IsRefinableMaterial(DataProps.PropsData props)
        {
            return props.propsID == ModLibConst.MONEY_PROP_ID;
        }

        /// <summary>
        /// Adds refine exp to an item.
        /// </summary>
        public static bool AddRefineExp(string soleId, long exp)
        {
            if (string.IsNullOrEmpty(soleId) || exp == 0)
                return false;
            if (!Instance.RefineExp.ContainsKey(soleId))
                Instance.RefineExp.Add(soleId, 0);
            Instance.RefineExp[soleId] += exp;
            return true;
        }

        /// <summary>
        /// Adds refine exp to an item.
        /// </summary>
        public static bool AddRefineExp(DataProps.PropsData props, long exp)
        {
            return AddRefineExp(props?.soleID, exp);
        }

        /// <summary>
        /// Refine exp of an item.
        /// </summary>
        public static long GetRefineExp(string soleId)
        {
            if (string.IsNullOrEmpty(soleId))
                return 0;
            if (!Instance.RefineExp.ContainsKey(soleId))
                return 0;
            return Instance.RefineExp[soleId];
        }

        /// <summary>
        /// Refine exp of an item.
        /// </summary>
        public static long GetRefineExp(DataProps.PropsData props)
        {
            return GetRefineExp(props?.soleID);
        }

        /// <summary>
        /// Refine level from exp (square-root growth).
        /// </summary>
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

        /// <summary>
        /// Adjustment slot (created randomly on first access).
        /// </summary>
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

        /// <summary>
        /// Adjustment slots of an item (optionally of a type).
        /// </summary>
        public static List<CustomRefine> GetCustomAdjTypes(DataProps.PropsData props)
        {
            return Instance.CustomRefine.Keys.Where(x => x.StartsWith(props.soleID)).Select(x => Instance.CustomRefine[x]).ToList();
        }

        /// <summary>
        /// Adjustment slots of an item (optionally of a type).
        /// </summary>
        public static List<CustomRefine> GetCustomAdjTypes(DataProps.PropsData props, AdjTypeEnum condition)
        {
            return GetCustomAdjTypes(props).Where(x => x.AdjType == condition).ToList();
        }

        /// <summary>
        /// Total value of an adjustment type on the unit's equipment.
        /// </summary>
        public static double GetCustomAdjValue(WorldUnitBase wunit, AdjTypeEnum adjType)
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
                    rs += a.GetRefineCustommAdjValue(wunit, props, refineLvl);
                }
            }

            return rs;
        }

        /// <summary>
        /// Copies slots and exp to another item.
        /// </summary>
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
