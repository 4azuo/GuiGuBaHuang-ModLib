using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine.Events;
using UnityEngine;
using Il2CppSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using MOD_nE7UL2.Object;
using System.Text;
using System.Web.WebPages;
using System.Windows.Interop;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.MISSION_DECLARE_EVENT)]
    public class MissionDeclareEvent : ModEvent
    {
        //Components
        private UITownBounty uiTownBounty;
        private UIPropSelect uiSelector;
        private Button btnCommission;
        private Text txtCommissionInfo;
        private Text txtInfo1;
        private Text txtInfo2;
        private Text txtInfo3;

        //Variables
        private List<KeyValuePair<ConfItemPropsItem, int>> _availableItems;

        public List<CommissionTask> CommissionTasks { get; set; } = new List<CommissionTask>();

        public int LastMonthCommission { get; set; }

        public override void OnLoadGame()
        {
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

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            uiTownBounty = e.ui.TryCast<UITownBounty>();
            if (uiTownBounty != null)
            {
                btnCommission = MonoBehaviour.Instantiate(uiTownBounty.btnTaskPut, uiTownBounty.transform, false);
                btnCommission.transform.position = new Vector3(uiTownBounty.btnTaskPut.transform.position.x + 2.0f, uiTownBounty.btnTaskPut.transform.position.y);
                btnCommission.onClick.AddListener((UnityAction)SelectCommissionItems);
                var btnText = btnCommission.GetComponentInChildren<Text>();
                btnText.text = "Commission";

                txtCommissionInfo = MonoBehaviour.Instantiate(uiTownBounty.textTaskPut, uiTownBounty.transform, false);
                txtCommissionInfo.transform.position = new Vector3(uiTownBounty.ptextInfo.transform.position.x - 0.5f, uiTownBounty.ptextInfo.transform.position.y);
                txtCommissionInfo.verticalOverflow = VerticalWrapMode.Overflow;
                txtCommissionInfo.horizontalOverflow = HorizontalWrapMode.Overflow;
                txtCommissionInfo.fontSize = 15;
                txtCommissionInfo.color = Color.black;
                txtCommissionInfo.alignment = TextAnchor.UpperLeft;
            }
        }

        public override void OnCloseUIEnd(CloseUIEnd e)
        {
            uiTownBounty = MonoBehaviour.FindObjectOfType<UITownBounty>();
            if (uiTownBounty == null)
            {
                btnCommission = null;
            }

            uiSelector = MonoBehaviour.FindObjectOfType<UIPropSelect>();
            if (uiSelector == null)
            {
                txtInfo1 = null;
                txtInfo2 = null;
                txtInfo3 = null;
            }
        }

        public override void OnFrameUpdate()
        {
            if (uiTownBounty != null && btnCommission != null)
            {
                if (btnCommission != null)
                {
                    btnCommission.gameObject.SetActive(g.world.run.roundMonth != LastMonthCommission);
                }

                if (txtCommissionInfo != null)
                {
                    var msg = new StringBuilder();
                    foreach (var task in CommissionTasks)
                    {
                        if (task.Status == CommissionTask.CommissionTaskStatus.Progressing)
                        {
                            msg.AppendLine($"　{string.Join(Environment.NewLine, task.CommisionItems.Select((x, i) => $"{i + 1}. {g.conf.localText.allText[g.conf.itemProps.GetItem(x.Key).name].en} x{x.Value}: {((task.CostTime - task.PassTime) == 0 ? "next" : (task.CostTime - task.PassTime).ToString())} month"))}");
                        }
                    }

                    txtCommissionInfo.text = $@"
Your commissions:
{msg}
";
                    
                    txtCommissionInfo.gameObject.SetActive(!uiTownBounty.goTaskInfo.active);
                }

                if (uiSelector != null)
                {
                    if (UIPropSelect.allSlectDataProps.allProps.Count > 0)
                    {
                        var money = g.world.playerUnit.GetUnitMoney();
                        var degree = g.world.playerUnit.GetUnitMayorDegree();
                        var commissionTask = new CommissionTask(UIPropSelect.allSlectDataProps.allProps);
                        var allow = money >= commissionTask.Total + commissionTask.Fee && degree >= commissionTask.CostDegree;
                        uiSelector.btnOK.gameObject.SetActive(allow);
                        txtInfo1.color = allow ? Color.black : Color.red;
                        txtInfo1.text = $"Cost {commissionTask.Total + commissionTask.Fee:0} spirit stones (+{commissionTask.Fee:0} fee) and {commissionTask.CostDegree:0} Mayor's Degree";
                        txtInfo2.text = $"Cost {commissionTask.CostTime:0} months ({commissionTask.SuccessRate:0.0}% success rate)";
                    }
                    else
                    {
                        uiSelector.btnOK.gameObject.SetActive(true);
                        txtInfo1.color = Color.black;
                        txtInfo1.text = $"- - - - -";
                        txtInfo2.text = $"- - - - -";
                    }

                    uiSelector.UpdateUI();
                }
            }
        }

        private void SelectCommissionItems()
        {
            uiSelector = g.ui.OpenUI<UIPropSelect>(UIType.PropSelect);

            ClearCommisionItems();
            uiSelector.onOKCall = (Action)Commission;

            txtInfo1 = MonoBehaviour.Instantiate(uiSelector.textInfo, uiSelector.transform, false);
            txtInfo1.transform.position = new Vector3(uiSelector.btnOK.transform.position.x, uiSelector.btnOK.transform.position.y - 0.5f);
            txtInfo1.verticalOverflow = VerticalWrapMode.Overflow;
            txtInfo1.horizontalOverflow = HorizontalWrapMode.Overflow;
            txtInfo1.fontSize = 15;
            txtInfo1.color = Color.black;

            txtInfo2 = MonoBehaviour.Instantiate(uiSelector.textInfo, uiSelector.transform, false);
            txtInfo2.transform.position = new Vector3(uiSelector.btnOK.transform.position.x, uiSelector.btnOK.transform.position.y);
            txtInfo2.verticalOverflow = VerticalWrapMode.Overflow;
            txtInfo2.horizontalOverflow = HorizontalWrapMode.Overflow;
            txtInfo2.fontSize = 15;
            txtInfo2.color = Color.black;
            txtInfo2.alignment = TextAnchor.MiddleRight;

            txtInfo3 = MonoBehaviour.Instantiate(uiSelector.textInfo, uiSelector.transform, false);
            txtInfo3.text = $"{g.world.playerUnit.GetUnitMoney()} Spirit Stones, {g.world.playerUnit.GetUnitMayorDegree()} Mayor Degrees";
            txtInfo3.transform.position = new Vector3(uiSelector.textTitle1.transform.position.x, uiSelector.textTitle1.transform.position.y + 0.3f);
            txtInfo3.verticalOverflow = VerticalWrapMode.Overflow;
            txtInfo3.horizontalOverflow = HorizontalWrapMode.Overflow;
            txtInfo3.fontSize = 15;
            txtInfo3.color = Color.black;
            txtInfo3.alignment = TextAnchor.MiddleLeft;

            uiSelector.btnOK.transform.position = new Vector3(uiSelector.btnOK.transform.position.x - 1.5f, uiSelector.btnOK.transform.position.y);
        }

        private void ClearCommisionItems()
        {
            uiSelector.ClearSelectItem();
            uiSelector.allItems = new DataProps
            {
                allProps = new Il2CppSystem.Collections.Generic.List<DataProps.PropsData>()
            };
            foreach (var item in _availableItems)
            {
                var prop = item.Key;
                var grade = item.Value;
                if (grade <= uiTownBounty.town.gridData.areaBaseID && 
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
                g.world.playerUnit.AddUnitMoney(-(commissionTask.Total + commissionTask.Fee));
                g.world.playerUnit.AddUnitMayorDegree(-commissionTask.CostDegree);
                CommissionTasks.Add(commissionTask);
                //DramaTool.OpenDrama(480030100);
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

            var curMainTown = g.world.build.GetBuild<MapBuildTown>(g.world.playerUnit.data.unitData.GetPoint());
            if (curMainTown != null && CommissionTasks.Any(x => x.Status != CommissionTask.CommissionTaskStatus.Progressing))
            {
                var msg = new StringBuilder();
                var uiReward = g.ui.OpenUI<UIGetReward>(UIType.GetReward);
                var uiRewardText = MonoBehaviour.Instantiate(uiReward.textExpTitle, uiReward.transform, false);
                uiRewardText.transform.position = new Vector3(uiReward.btnOK.transform.position.x - 2.0f, uiReward.btnOK.transform.position.y + 1.5f);
                uiRewardText.verticalOverflow = VerticalWrapMode.Overflow;
                uiRewardText.horizontalOverflow = HorizontalWrapMode.Overflow;
                uiRewardText.fontSize = 15;
                uiRewardText.color = Color.black;
                uiRewardText.alignment = TextAnchor.UpperLeft;

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
                //                DramaTool.OpenDrama(480030200, new DramaData
                //                {
                //                    dialogueText = 
                //                    {
                //                        [480030200] = $@"
                //Your commissions:
                //{msg}
                //"
                //                    }
                //                });
            }
        }
    }
}
