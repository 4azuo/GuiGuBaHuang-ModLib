﻿using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using Il2CppSystem;
using System.Collections.Generic;
using System.Linq;
using MOD_nE7UL2.Object;
using System.Text;
using ModLib.Object;
using ModLib.Enum;
using static SpecialBattle83;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.MISSION_DECLARE_EVENT)]
    public class MissionDeclareEvent : ModEvent
    {
        public static MissionDeclareEvent Instance { get; set; }

        //Variables
        private List<KeyValuePair<ConfItemPropsItem, int>> _availableItems;

        public List<CommissionTask> CommissionTasks { get; set; } = new List<CommissionTask>();

        public int LastMonthCommission { get; set; }

        public override void OnLoadGame()
        {
            base.OnLoadGame();
            _availableItems = new List<KeyValuePair<ConfItemPropsItem, int>>();
            foreach (var prop in g.conf.itemProps._allConfList)
            {
                var breakthroughItem = prop.IsBreakthroughItem();
                var herbItem = prop.IsHerbItem();
                var mineItem = prop.IsMineItem();
                var ringItem = prop.IsRing();
                var marketItem = prop.IsMarketBuyableItem();
                //var schoolItem = prop.IsSchoolBuyableItem();
                var grade = Math.Min(10, Math.Max(1, breakthroughItem?.Select(x => x.grade).Max() ?? ringItem?.grade ?? 1));
                if ((
                        (breakthroughItem != null && (
                            (grade > 4 && prop.level <= 4) ||
                            grade <= 4
                        )) ||
                        herbItem != null ||
                        mineItem != null ||
                        ringItem != null
                    ) && (
                        marketItem == null/* &&
                        schoolItem == null*/
                    ))
                {
                    _availableItems.Add(new KeyValuePair<ConfItemPropsItem, int>(prop, grade));
                }
            }
        }

        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.TownBounty.uiName)
            {
                var ui = new UICover<UITownBounty>(e.ui);
                {
                    ui.AddButton(0, 0, SelectCommissionItems, GameTool.LS("other500020003")).Size(160, 40).Pos(ui.UI.btnTaskPut.transform, 200, 0).SetWork(new UIItemWork
                    {
                        UpdateAct = (x) => x.Active(g.world.run.roundMonth != LastMonthCommission)
                    });
                    ui.AddText(0, 0, string.Empty).Align(TextAnchor.UpperLeft).Format().Pos(ui.UI.ptextInfo.transform, 225, 150).SetWork(new UIItemWork
                    {
                        UpdateAct = (x) =>
                        {
                            var msg = new StringBuilder();
                            foreach (var task in CommissionTasks)
                            {
                                if (task.Status == CommissionTask.CommissionTaskStatus.Progressing)
                                {
                                    msg.AppendLine($"　{string.Join(Environment.NewLine, task.CommisionItems.Select((y, i) => $"{i + 1}. {g.conf.localText.allText[g.conf.itemProps.GetItem(y.Key).name].en} x{y.Value}: {(task.CostTime - task.PassTime) + 1} month"))}");
                                }
                            }
                            x.Set($@"
Your commissions:
{msg}
");
                            x.Active(!ui.UI.goTaskInfo.active);
                        }
                    });
                }
                ui.IsAutoUpdate = true;
            }
        }

        private void SelectCommissionItems()
        {
            var uiTownBounty = g.ui.GetUI<UITownBounty>(UIType.TownBounty);
            if (uiTownBounty.IsExists())
            {
                //open select ui
                var uiSelector = g.ui.OpenUISafe<UIPropSelect>(UIType.PropSelect);
                ClearCommisionItems(uiTownBounty, uiSelector);
                uiSelector.UpdateUI();

                var uiCover = new UICover<UIPropSelect>(uiSelector);
                {
                    var txt1 = uiCover.AddText(0, 0, string.Empty, uiSelector.textInfo).Format().Pos(uiSelector.btnOK.transform, 0, 100);
                    var txt2 = uiCover.AddText(0, 0, string.Empty, uiSelector.textInfo).Format().Pos(uiSelector.btnOK.transform, 0, 50);
                    var txt3 = uiCover.AddText(0, 0, $"{g.world.playerUnit.GetUnitMoney()} Spirit Stones, {g.world.playerUnit.GetUnitMayorDegree()} Mayor Degrees", uiSelector.textInfo)
                        .Align(TextAnchor.MiddleCenter).Format().Pos(uiSelector.textTitle1.transform, 0, 0);
                    uiCover.UIWork = new UICustomWork
                    {
                        UpdateAct = (ui) =>
                        {
                            if (UIPropSelect.allSlectDataProps.allProps.Count > 0)
                            {
                                var money = g.world.playerUnit.GetUnitMoney();
                                var degree = g.world.playerUnit.GetUnitMayorDegree();
                                var commissionTask = new CommissionTask(UIPropSelect.allSlectDataProps.allProps);
                                if (MapBuildPropertyEvent.IsTownGuardian(g.world.playerUnit.GetMapBuild<MapBuildTown>(), g.world.playerUnit))
                                {
                                    var allow = money >= commissionTask.Total;
                                    uiSelector.btnOK.gameObject.SetActive(allow);
                                    txt1.InnerText.color = allow ? Color.black : Color.red;
                                    txt1.Set($"Cost {commissionTask.Total + commissionTask.Fee:0} spirit stones");
                                    txt2.Set($"Cost {commissionTask.CostTime:0} months ({commissionTask.SuccessRate:0.0}% success rate)");
                                }
                                else
                                {
                                    var allow = money >= (commissionTask.Total + commissionTask.Fee) && degree >= commissionTask.CostDegree;
                                    uiSelector.btnOK.gameObject.SetActive(allow);
                                    txt1.InnerText.color = allow ? Color.black : Color.red;
                                    txt1.Set($"Cost {commissionTask.Total + commissionTask.Fee:0} spirit stones (+{commissionTask.Fee:0} fee) and {commissionTask.CostDegree:0} Mayor's Degree");
                                    txt2.Set($"Cost {commissionTask.CostTime:0} months ({commissionTask.SuccessRate:0.0}% success rate)");
                                }
                            }
                            else
                            {
                                uiSelector.btnOK.gameObject.SetActive(true);
                                txt1.InnerText.color = Color.black;
                                txt1.Set($"- - - - -");
                                txt2.Set($"- - - - -");
                            }
                        }
                    };
                }
                uiCover.IsAutoUpdate = true;

                //register OK button
                uiSelector.onOKCall = (Action)Commission;
            }
        }

        private void ClearCommisionItems(UITownBounty uiTownBounty, UIPropSelect uiSelector)
        {
            uiSelector.ClearSelectItem();
            uiSelector.allItems.ClearAllProps();
            foreach (var item in _availableItems)
            {
                var prop = item.Key;
                if (item.Value.IsBetween(uiTownBounty.town.gridData.areaBaseID - 1, uiTownBounty.town.gridData.areaBaseID + 1) &&
                    (
                        (prop.IsHerbItem() != null && (prop.level + 1) <= uiTownBounty.town.gridData.areaBaseID) ||
                        (prop.IsMineItem() != null && (prop.level + 1) <= uiTownBounty.town.gridData.areaBaseID) ||
                        (prop.IsHerbItem() == null && prop.IsMineItem() == null)
                    ))
                {
                    if (prop.isOverlay == 1 && prop.IsBreakthroughItem() == null)
                        uiSelector.allItems.AddProps(prop.id, 100);
                    else
                        uiSelector.allItems.AddProps(prop.id, 1);
                }
            }
        }

        private void Commission()
        {
            if (UIPropSelect.allSlectDataProps.allProps.Count > 0)
            {
                var commissionTask = new CommissionTask(UIPropSelect.allSlectDataProps.allProps);
                if (MapBuildPropertyEvent.IsTownGuardian(g.world.playerUnit.GetMapBuild<MapBuildTown>(), g.world.playerUnit))
                {
                    g.world.playerUnit.AddUnitMoney(-commissionTask.Total);
                }
                else
                {
                    g.world.playerUnit.AddUnitMoney(-(commissionTask.Total + commissionTask.Fee));
                    g.world.playerUnit.AddUnitMayorDegree(-commissionTask.CostDegree);
                }
                CommissionTasks.Add(commissionTask);
                LastMonthCommission = g.world.run.roundMonth;
            }
        }

        public override void OnMonthly()
        {
            foreach (var task in CommissionTasks)
            {
                if (task.Status == CommissionTask.CommissionTaskStatus.Progressing)
                {
                    task.PassTime++;
                    if (task.PassTime > task.CostTime)
                    {
                        task.Status = CommissionTask.CommissionTaskStatus.Success;
                    }
                    else
                    {
                        var r = CommonTool.Random(0.00f, 100.00f);
                        if (!ValueHelper.IsBetween(r, 0.00f, task.SuccessRate))
                        {
                            task.Status = CommissionTask.CommissionTaskStatus.Failed;
                        }
                    }
                }
            }

            var curMainTown = g.world.build.GetBuild<MapBuildTown>(g.world.playerUnit.GetUnitPos());
            if (curMainTown != null && CommissionTasks.Any(x => x.Status != CommissionTask.CommissionTaskStatus.Progressing))
            {
                var uiReward = g.ui.OpenUISafe<UIGetReward>(UIType.GetReward);
                var uiRewardText = uiReward.textExpTitle.Copy(uiReward.transform).Align().Format().Pos(uiReward.btnOK.transform, 0, 0);

                var msg = new StringBuilder();
                foreach (var task in CommissionTasks.Where(x => 
                    x.Status != CommissionTask.CommissionTaskStatus.Progressing
                ).ToList())
                {
                    msg.AppendLine($"　{string.Join(Environment.NewLine, task.CommisionItems.Select((x, i) => $"{i + 1}. {g.conf.localText.allText[g.conf.itemProps.GetItem(x.Key).name].en} x{x.Value}: {task.Status}"))}");
                    CommissionTasks.Remove(task);

                    if (task.Status == CommissionTask.CommissionTaskStatus.Success)
                    {
                        foreach (var item in task.CommisionItems)
                        {
                            g.world.playerUnit.AddUnitProp(item.Key, item.Value);
                            uiReward.UpdateProp(item.Key, item.Value);
                        }
                    }
                }
                uiRewardText.text = $@"
Your commissions:
{msg}
";
            }
        }
    }
}
