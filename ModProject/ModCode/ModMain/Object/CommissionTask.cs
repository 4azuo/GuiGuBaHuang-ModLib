using Il2CppSystem.Collections.Generic;
using System;
using System.Linq;
using static MOD_nE7UL2.Object.InGameStts;

namespace MOD_nE7UL2.Object
{
    public class CommissionTask
    {
        public enum CommissionTaskStatus
        {
            Progressing,
            Success,
            Failed
        }
        public static _MissionDeclareConfigs Configs
        {
            get
            {
                return ModMain.ModObj.InGameCustomSettings.MissionDeclareConfigs;
            }
        }

        public System.Collections.Generic.Dictionary<int, int> CommisionItems { get; set; }
        public int Total { get; set; }
        public int Fee { get; set; }
        public int CostDegree { get; set; }
        public int CostTime { get; set; }
        public float SuccessRate { get; set; }
        public int PassTime { get; set; } = 0;
        public CommissionTaskStatus Status { get; set; } = CommissionTaskStatus.Progressing;

        public CommissionTask() { }

        public CommissionTask(List<DataProps.PropsData> selectedItems)
        {
            var lst = selectedItems.ToArray();
            CommisionItems = lst.ToDictionary(x => x.propsID, x => x.propsCount);
            Total = lst.Sum(x => x.propsCount * x.propsInfoBase.worth);
            Fee = ((Total * Configs.FeeRate) + Configs.FeeMinCost).Parse<int>();
            CostDegree = ((Total * Configs.DegreeCostRate) + Configs.DegreeMinCost).Parse<int>();
            CostTime = lst.Sum(x => Configs.CostTime[Math.Min(6, Math.Max(1, x.propsInfoBase.level)) - 1]);
            SuccessRate = lst.Min(x => Configs.SuccessRate[Math.Min(6, Math.Max(1, x.propsInfoBase.level)) - 1]);
        }
    }
}
