using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using System;
using System.Collections.Generic;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.NPC_AUTO_UP_GRADE_EVENT)]
    public class NpcAutoUpGradeEvent : ModEvent
    {
        public IDictionary<string, float> NpcUpGradeRate { get; set; } = new Dictionary<string, float>();

        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);
            if (!wunit.IsPlayer() && wunit.IsFullExp())
            {
                var nPhase = wunit.GetNextPhaseConf();
                if (nPhase == null)
                    return;

                var unitId = wunit.GetUnitId();
                if (!NpcUpGradeRate.ContainsKey(unitId))
                    NpcUpGradeRate.Add(unitId, 0.0000000f);

                var supportRate = 0.0000001f;
                foreach (var oldPhaseInfo in wunit.data.unitData.npcUpGrade)
                    supportRate += (0.0000001f * Math.Pow(2, oldPhaseInfo.value.quality)).Parse<float>();
                NpcUpGradeRate[unitId] += supportRate;

                var r = CommonTool.Random(0.0000000f, 100.0000000f);
                if (r.IsBetween(0.0000000f, NpcUpGradeRate[unitId] / nPhase.grade))
                {
                    wunit.SetProperty<int>(UnitPropertyEnum.GradeID, nPhase.id);
                    wunit.ClearExp();
                }
            }
        }
    }
}
