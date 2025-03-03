﻿using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using System;
using System.Collections.Generic;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.NPC_AUTO_EVENT)]
    public class NpcAutoEvent : ModEvent
    {
        public static NpcAutoEvent Instance { get; set; }

        public IDictionary<string, float> NpcUpGradeRate { get; set; } = new Dictionary<string, float>();

        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);
            if (!wunit.IsPlayer())
            {
                if (wunit.IsFullExp())
                {
                    //up grade
                    var nPhase = wunit.GetNextPhaseConf();
                    if (nPhase == null)
                        return;

                    var unitId = wunit.GetUnitId();
                    if (!NpcUpGradeRate.ContainsKey(unitId))
                        NpcUpGradeRate.Add(unitId, 0.000f);

                    var supportRate = 0.001f;
                    foreach (var oldPhaseInfo in wunit.data.unitData.npcUpGrade)
                        supportRate += (0.001f * Math.Pow(2, oldPhaseInfo.value.quality)).Parse<float>();
                    NpcUpGradeRate[unitId] += supportRate;

                    var r = CommonTool.Random(0.0000000f, 100.0000000f).Parse<double>();
                    if (r.IsBetween(0.0000000d, NpcUpGradeRate[unitId] / Math.Pow(2, nPhase.grade)))
                    {
                        wunit.SetProperty<int>(UnitPropertyEnum.GradeID, nPhase.id);
                        wunit.ClearExp();
                    }
                }
                else
                {
                    var exp = SMLocalConfigsEvent.Instance.Calculate(CommonTool.Random(10.00f, 200.00f) * wunit.GetGradeLvl() * wunit.GetDynProperty(UnitDynPropertyEnum.Talent).value / 100f, SMLocalConfigsEvent.Instance.Configs.AddNpcGrowRate);
                    if (wunit.IsHero())
                        exp *= CommonTool.Random(0.50f, 2.00f);
                    wunit.AddExp(exp.Parse<int>());
                }
            }
        }

        public override void OnYearly()
        {
            base.OnYearly();
            g.world.run.npc.AddUnit(CommonTool.Random(1, g.world.playerUnit.GetGradeLvl()), 1);
        }
    }
}
