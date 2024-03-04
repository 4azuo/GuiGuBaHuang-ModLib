//using MOD_nE7UL2.Const;
//using MOD_nE7UL2.Enum;
//using ModLib.Enum;
//using ModLib.Mod;
//using ModLib.Object;
//using System.Collections.Generic;
//using System.Linq;

//namespace MOD_nE7UL2.Mod
//{
//    //TESTING
//    //[Cache(ModConst.NPC_AUTO_UP_GRADE_EVENT)]
//    //public class NpcAutoUpGradeEvent : ModEvent
//    //{
//    //    public IDictionary<string, double> NpcUpGradeRatio { get; set; } = new Dictionary<string, double>();

//    //    public override void OnMonthly()
//    //    {
//    //        var rewrites = g.conf.roleCreateFeature._allConfList.ToArray().Where(x => x.type == FeatureTypeEnum.RewriteDestiny.Value.Parse<int>()).ToList();
//    //        foreach (var wunit in g.world.unit.GetUnits())
//    //        {
//    //            if (wunit.GetProperty<int>(UnitPropertyEnum.Exp) >= wunit.GetMaxExpCurrentGrade())
//    //            {
//    //                var setting = NpcUpGradeRatioEnum.GetAllEnums<NpcUpGradeRatioEnum>().FirstOrDefault(x => x.Grade.Value.Parse<int>() == wunit.GetProperty<int>(UnitPropertyEnum.GradeID));
//    //                if (setting != null)
//    //                {
//    //                    var supportRatio = 0.00d;
//    //                    foreach (var oldGrade in wunit.data.unitData.npcUpGrade)
//    //                    {
//    //                        var oldGradeStt = NpcUpGradeRatioEnum.GetAllEnums<NpcUpGradeRatioEnum>().FirstOrDefault(x => x.Grade.Value.Parse<int>() == oldGrade.Key);
//    //                        if (oldGradeStt != null)
//    //                        {
//    //                            supportRatio += oldGradeStt.NextGrade
//    //                                .FirstOrDefault(x => wunit.data.unitData.npcUpGrade.ContainsKey(((x.Values[0] as MultiValue).Values[0] as GradePhaseEnum).Value.Parse<int>()))
//    //                                ?.Values[2].Parse<double>() ?? 0.00d;
//    //                        }
//    //                    }

//    //                    foreach (var nGrade in setting.NextGrade)
//    //                    {
//    //                        var nGradeID = ((nGrade.Values[0] as MultiValue).Values[0] as GradePhaseEnum).Value.Parse<int>();
//    //                        var nGradeRatioKey = $"{wunit.GetUnitId()}_{nGradeID}";
//    //                        NpcUpGradeRatio[nGradeRatioKey] += nGrade.Values[1].Parse<double>() * (1.00d + supportRatio);

//    //                        var r = CommonTool.Random(0.000000f, 100.000000f);
//    //                        if (r.Parse<double>().IsBetween(0.000000d, NpcUpGradeRatio[nGradeRatioKey]))
//    //                        {
//    //                            var aRewrites = wunit.data.unitData.npcUpGrade.Values.ToList().Select(x => x.luck).ToList();
//    //                            var bRewrites = rewrites.Where(x => !aRewrites.Contains(x.id)).ToList();
//    //                            wunit.data.unitData.npcUpGrade.Add(nGradeID, new DataWorld.World.PlayerLogData.GradeData
//    //                            {
//    //                                luck = bRewrites.Random().id,
//    //                                quality = g.conf.roleGrade.GetItem(nGradeID).quality,
//    //                            });
//    //                            wunit.SetProperty<int>(UnitPropertyEnum.GradeID, nGradeID);
//    //                            goto gradeUp;
//    //                        }
//    //                    }
//    //                }
//    //            }
//    //        gradeUp:;
//    //        }
//    //    }
//    //}
//}
