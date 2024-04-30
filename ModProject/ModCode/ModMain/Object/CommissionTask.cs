using Il2CppSystem.Collections.Generic;
using MOD_nE7UL2.Mod;
using System;
using System.Linq;

namespace MOD_nE7UL2.Object
{
    public class CommissionTask
    {
        public System.Collections.Generic.Dictionary<int, int> CommisionItems { get; set; }
        public int Total { get; set; }
        public int Fee { get; set; }
        public int CostDegree { get; set; }
        public int CostTime { get; set; }
        public float SuccessRate { get; set; }
        public int PassTime { get; set; } = 0;

        public CommissionTask() { }

        public CommissionTask(List<DataProps.PropsData> selectedItems)
        {
            var lst = selectedItems.ToArray();
            CommisionItems = lst.ToDictionary(x => x.propsID, x => x.propsCount);
            Total = lst.Sum(x => x.propsCount * x.propsInfoBase.worth);
            Fee = ((Total * MissionDeclareEvent.FEE_RATE) + MissionDeclareEvent.FEE_MIN_COST).Parse<int>();
            CostDegree = ((Total * MissionDeclareEvent.DEGREE_COST_RATE) + MissionDeclareEvent.DEGREE_MIN_COST).Parse<int>();
            CostTime = lst.Sum(x => MissionDeclareEvent.COST_TIME[Math.Min(6, Math.Max(1, x.propsInfoBase.level)) - 1]);
            SuccessRate = lst.Min(x => MissionDeclareEvent.SUCCESS_RATE[Math.Min(6, Math.Max(1, x.propsInfoBase.level)) - 1]);
        }
    }
}
