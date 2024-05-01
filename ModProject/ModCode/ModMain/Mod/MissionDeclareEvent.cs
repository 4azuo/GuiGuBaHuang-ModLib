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
using ModLib.Enum;
using static DataBuildSchool;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.MISSION_DECLARE_EVENT)]
    public class MissionDeclareEvent : ModEvent
    {
        //Constants
        public const float FEE_RATE = 0.2f;
        public const int FEE_MIN_COST = 100;
        public const float DEGREE_COST_RATE = 0.001f;
        public const int DEGREE_MIN_COST = 1;
        public static readonly int[] COST_TIME = new int[] { 1, 2, 3, 5, 8, 16 };
        public static readonly float[] SUCCESS_RATE = new float[] { 100.0f, 98.0f, 96.0f, 94.0f, 90.0f, 85.0f };

        //Components
        private UITownBounty uiTownBounty;
        private UIPropSelect uiSelector;
        private Button btnCommission;
        private Text txtInfo1;
        private Text txtInfo2;

        //Variables
        private List<KeyValuePair<ConfItemPropsItem, int>> _availableItems;

        public List<CommissionTask> CommissionTasks { get; set; } = new List<CommissionTask>();

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
                //btnClear = null;
                txtInfo1 = null;
                txtInfo2 = null;
            }
        }

        public override void OnFrameUpdate()
        {
            if (uiTownBounty != null && uiSelector != null)
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
            }
        }

        public override void OnMonthly()
        {
            foreach (var task in CommissionTasks)
            {
                task.PassTime++;
                if (task.PassTime >= task.CostTime)
                {
                    var uiReward = g.ui.OpenUI<UIGetReward>(UIType.GetReward);
                    foreach (var item in task.CommisionItems)
                    {
                        g.world.playerUnit.AddUnitProp(item.Key, item.Value);
                        uiReward.UpdateProp(item.Key, item.Value);
                    }
                    CommissionTasks.Remove(task);
                }
                else
                {
                    var r = CommonTool.Random(0.00f, 100.00f);
                    if (!ValueHelper.IsBetween(r, 0.00f, task.SuccessRate))
                    {
                        CommissionTasks.Remove(task);
                    }
                }
            }
        }
    }
}
