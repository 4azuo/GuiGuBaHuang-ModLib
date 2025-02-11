using EBattleTypeData;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using ModLib.Enum;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.MAP_BUILD_BATTLE_EVENT)]
    public class MapBuildBattleEvent : ModEvent
    {
        public static MapBuildBattleEvent Instance { get; set; }

        public const int JOIN_RANGE = 4;
        public const float MONST_WAVE_RATE = 1f;
        public const float TOWN_WAR_RATE = 0.4f;
        public const int MIN_MONST = 10;
        public const int STP_MONST = 5;

        public const int TOWN_WAR_DUNGEON_BASE_ID = 480110990;
        public const int TOWN_MONST_WAVE_DUNGEON_BASE_ID = 480110991;
        public const int SECT_MONST_WAVE_DUNGEON_BASE_ID = 480110992;

        public Dictionary<string, int> LastYearEventHappen { get; set; } = new Dictionary<string, int>();

        public static int playerSide;
        public static int teamAUnitCount;
        public static int teamBUnitCount;
        public static List<WorldUnitBase> teamAWUnits;
        public static List<WorldUnitBase> teamBWUnits;
        public static List<UnitCtrlBase> teamACUnits;
        public static List<UnitCtrlBase> teamBCUnits;

        public static readonly int[] enemyInTraits = new int[] { UnitTraitEnum.Evil.Parse<int>() };
        public static readonly int[] enemyOutTraits = new int[] { UnitTraitEnum.Power_hungry.Parse<int>(), UnitTraitEnum.Glory_Hound.Parse<int>() };
        public static readonly int[] monstList = new int[] { 480112870, 480112871, 480112872, 480112873, 480112874, 480112875, 480112876, 480112877, 480112878, 480112879 };

        public override void OnMonthly()
        {
            base.OnMonthly();

            var curYear = GameHelper.GetGameYear();
            foreach (var town in g.world.build.GetBuilds<MapBuildTown>())
            {
                if (!LastYearEventHappen.ContainsKey(town.buildData.id))
                    LastYearEventHappen.Add(town.buildData.id, curYear);

                if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, MONST_WAVE_RATE * (curYear - LastYearEventHappen[town.buildData.id])))
                {
                    LastYearEventHappen[town.buildData.id] = curYear;
                    MonstWave(town);
                }
                else
                if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, TOWN_WAR_RATE * (curYear - LastYearEventHappen[town.buildData.id])))
                {
                    var master = MapBuildPropertyEvent.GetTownMaster(town);
                    MapBuildTown townAtk = null;
                    var hasAttacker = MapBuildPropertyEvent.Instance.TownMasters.Any(x =>
                    {
                        var townAtkId = x.Key;
                        if (townAtkId == town.buildData.id)
                            return false;
                        townAtk = g.world.build.GetBuild<MapBuildTown>(townAtkId);
                        if (townAtk == null)
                            return false;
                        var townAtkMaster = MapBuildPropertyEvent.GetTownMaster(townAtk);
                        if (townAtkMaster == null)
                            return false;
                        return townAtkMaster.data.unitData.relationData.GetIntim(master) <= -200 ||
                            enemyInTraits.Contains(townAtkMaster.data.unitData.propertyData.inTrait) ||
                            enemyOutTraits.Contains(townAtkMaster.data.unitData.propertyData.outTrait1) ||
                            enemyOutTraits.Contains(townAtkMaster.data.unitData.propertyData.outTrait2);
                    });
                    if (hasAttacker && townAtk != null)
                    {
                        LastYearEventHappen[town.buildData.id] = curYear;
                        LastYearEventHappen[townAtk.buildData.id] = curYear;
                        TownWar(town, townAtk, false);
                    }
                }
            }

            foreach (var school in g.world.build.GetBuilds<MapBuildSchool>())
            {
                if (!LastYearEventHappen.ContainsKey(school.buildData.id))
                    LastYearEventHappen.Add(school.buildData.id, curYear);

                if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, MONST_WAVE_RATE * (curYear - LastYearEventHappen[school.buildData.id])))
                {
                    LastYearEventHappen[school.buildData.id] = curYear;
                    MonstWave(school);
                }
            }
        }

        public static void TownWar(MapBuildTown townA_def, MapBuildTown townB_atk, bool proactive)
        {
            if (proactive)
            {
                JoinTownWar(townA_def, townB_atk);
            }
            else
            if (townA_def.GetOpenBuildPoints().ToList().Any(x => x == g.world.playerUnit.GetUnitPos()))
            {
                JoinTownWar(townA_def, townB_atk);
            }
            else
            if (MapBuildPropertyEvent.IsTownGuardian(townA_def, g.world.playerUnit))
            {
                g.ui.MsgBox("Warning", $"Your town ({townA_def.name}) is under attack!{Environment.NewLine}Would you like to help?", MsgBoxButtonEnum.YesNo, 
                () =>
                {
                    JoinTownWar(townA_def, townB_atk);
                }, 
                () =>
                {
                    SkipTownWar(townA_def, townB_atk);
                });
            }
            else
            {
                SkipTownWar(townA_def, townB_atk);
            }
        }

        public static void JoinTownWar(MapBuildTown townA_def, MapBuildTown townB_atk)
        {
            //player side
            if (MapBuildPropertyEvent.IsTownGuardian(townA_def, g.world.playerUnit))
                playerSide = 1;
            else if (MapBuildPropertyEvent.IsTownGuardian(townB_atk, g.world.playerUnit))
                playerSide = 2;
            else
                playerSide = 1;
            //battle info
            CalTownWarInfo(townA_def, townB_atk);
            //battle into
            g.world.battle.IntoBattle(new DataMap.MonstData() { id = TOWN_WAR_DUNGEON_BASE_ID, level = townA_def.gridData.areaBaseID * 5 });
        }

        public static void SkipTownWar(MapBuildTown townA_def, MapBuildTown townB_atk)
        {
            //battle info
            CalTownWarInfo(townA_def, townB_atk);
            //auto battle
            var teamAPoint = ValueHelper.SumBigNum(teamAWUnits.Select(x =>
            {
                return
                    (x.GetDynProperty(UnitDynPropertyEnum.Attack).value * 3) +
                    (x.GetDynProperty(UnitDynPropertyEnum.Defense).value * 10) +
                    (x.GetDynProperty(UnitDynPropertyEnum.HpMax).value) +
                    (x.GetDynProperty(UnitDynPropertyEnum.MpMax).value * 3) +
                    (x.GetDynProperty(UnitDynPropertyEnum.SpMax).value * 5) +
                    (x.GetEquippedArtifacts().Select(a => Math.Pow(2, a.propsInfoBase.grade) * 10000 + Math.Pow(2, a.propsInfoBase.level) * 2000).Sum());
            }).ToArray());
            var teamATotalPoint = teamAPoint * teamAUnitCount * /*def point*/1.2;
            var teamBPoint = ValueHelper.SumBigNum(teamBWUnits.Select(x =>
            {
                return
                    (x.GetDynProperty(UnitDynPropertyEnum.Attack).value * 3) +
                    (x.GetDynProperty(UnitDynPropertyEnum.Defense).value * 10) +
                    (x.GetDynProperty(UnitDynPropertyEnum.HpMax).value) +
                    (x.GetDynProperty(UnitDynPropertyEnum.MpMax).value * 3) +
                    (x.GetDynProperty(UnitDynPropertyEnum.SpMax).value * 5) +
                    (x.GetEquippedArtifacts().Select(a => Math.Pow(2, a.propsInfoBase.grade) * 10000 + Math.Pow(2, a.propsInfoBase.level) * 2000).Sum());
            }).ToArray());
            var teamBTotalPoint = teamBPoint * teamBUnitCount;
            //battle end
            var ratioAB = teamATotalPoint / teamBTotalPoint;
            var ratioBA = teamBTotalPoint / teamATotalPoint;
            if (teamATotalPoint >= teamBTotalPoint)
            {
                //A win
                //A damaged
                if (ratioAB < 2)
                {
                    //building damaged
                    BuildingArrangeEvent.Destroy(townA_def, BuildingCostEnum.GetAllEnums<BuildingCostEnum>().Where(x => BuildingArrangeEvent.IsBuilt(townA_def, x)).ToArray().Random());
                }
                MapBuildPropertyEvent.AddBuildProperty(townA_def, -(MapBuildPropertyEvent.GetBuildProperty(townA_def) * ratioBA / 5).Parse<long>());
                //B damaged
                MapBuildPropertyEvent.AddBuildProperty(townB_atk, -(MapBuildPropertyEvent.GetBuildProperty(townB_atk) * ratioAB / 10).Parse<long>());
            }
            else
            {
                //B win
                //A damaged x2
                BuildingArrangeEvent.Destroy(townA_def, BuildingCostEnum.GetAllEnums<BuildingCostEnum>().Where(x => BuildingArrangeEvent.IsBuilt(townA_def, x)).ToArray().Random());
                BuildingArrangeEvent.Destroy(townA_def, BuildingCostEnum.GetAllEnums<BuildingCostEnum>().Where(x => BuildingArrangeEvent.IsBuilt(townA_def, x)).ToArray().Random());
                var damagedBudget = MapBuildPropertyEvent.GetBuildProperty(townA_def);
                MapBuildPropertyEvent.AddBuildProperty(townA_def, -damagedBudget);
                //B damaged
                MapBuildPropertyEvent.AddBuildProperty(townB_atk, -(MapBuildPropertyEvent.GetBuildProperty(townB_atk) * ratioAB / 5).Parse<long>());
                //B pillage
                MapBuildPropertyEvent.AddBuildProperty(townB_atk, damagedBudget);
            }
        }

        public static void MonstWave(MapBuildTown town)
        {
            if (town.GetOpenBuildPoints().ToList().Any(x => x == g.world.playerUnit.GetUnitPos()))
            {
                JoinMonstWave(town, TOWN_MONST_WAVE_DUNGEON_BASE_ID);
            }
            else
            if (MapBuildPropertyEvent.IsTownGuardian(town, g.world.playerUnit))
            {
                g.ui.MsgBox("Warning", $"Your town ({town.name}) is under attack!{Environment.NewLine}Would you like to help?", MsgBoxButtonEnum.YesNo,
                () =>
                {
                    JoinMonstWave(town, TOWN_MONST_WAVE_DUNGEON_BASE_ID);
                },
                () =>
                {
                    SkipMonstWave(town);
                });
            }
            else
            {
                SkipMonstWave(town);
            }
        }

        public static void MonstWave(MapBuildSchool school)
        {
            if (school.GetOpenBuildPoints().ToList().Any(x => x == g.world.playerUnit.GetUnitPos()))
            {
                JoinMonstWave(school, SECT_MONST_WAVE_DUNGEON_BASE_ID);
            }
            else
            if (school.schoolNameID == g.world.playerUnit.data.unitData.schoolID)
            {
                g.ui.MsgBox("Warning", $"Your sect ({g.world.playerUnit.data.school.GetName(true)}) is under attack!{Environment.NewLine}Would you like to help?", MsgBoxButtonEnum.YesNo,
                () =>
                {
                    JoinMonstWave(school, SECT_MONST_WAVE_DUNGEON_BASE_ID);
                },
                () =>
                {
                    SkipMonstWave(school);
                });
            }
            else
            {
                SkipMonstWave(school);
            }
        }

        public static void JoinMonstWave(MapBuildBase teamAbuildBase, int dungeonBaseId)
        {
            //player side
            playerSide = 1;
            //battle info
            CalMonstWaveInfo(teamAbuildBase);
            //battle into
            g.world.battle.IntoBattle(new DataMap.MonstData() { id = dungeonBaseId, level = teamAbuildBase.gridData.areaBaseID * 5 });
        }

        public static void SkipMonstWave(MapBuildBase teamAbuildBase)
        {
            //battle info
            CalMonstWaveInfo(teamAbuildBase);
            //auto battle
            var teamAPoint = ValueHelper.SumBigNum(teamAWUnits.Select(x =>
            {
                return
                    (x.GetDynProperty(UnitDynPropertyEnum.Attack).value * 3) +
                    (x.GetDynProperty(UnitDynPropertyEnum.Defense).value * 10) +
                    (x.GetDynProperty(UnitDynPropertyEnum.HpMax).value) +
                    (x.GetDynProperty(UnitDynPropertyEnum.MpMax).value * 3) +
                    (x.GetDynProperty(UnitDynPropertyEnum.SpMax).value * 5) +
                    (x.GetEquippedArtifacts().Select(a => Math.Pow(2, a.propsInfoBase.grade) * 10000 + Math.Pow(2, a.propsInfoBase.level) * 2000).Sum());
            }).ToArray());
            var teamATotalPoint = teamAPoint * teamAUnitCount * /*def point*/1.2;
            var gameLvl = g.data.dataWorld.data.gameLevel.Parse<int>();
            var areaId = teamAbuildBase.gridData.areaBaseID;
            var teamBPoint = Math.Pow(3, areaId) * 10000 + Math.Pow(3, gameLvl) * 3000;
            var teamBTotalPoint = teamBPoint * teamBUnitCount;
            //battle end
            var ratioAB = teamATotalPoint / teamBTotalPoint;
            var ratioBA = teamBTotalPoint / teamATotalPoint;
            if (teamATotalPoint >= teamBTotalPoint)
            {
                //A win
                //A damaged
                MapBuildPropertyEvent.AddBuildProperty(teamAbuildBase, -(MapBuildPropertyEvent.GetBuildProperty(teamAbuildBase) * ratioBA / 3).Parse<long>());
            }
            else
            {
                //B win
                //A damaged
                foreach (var e in BuildingCostEnum.GetAllEnums<BuildingCostEnum>())
                {
                    if (BuildingArrangeEvent.IsBuilt(teamAbuildBase, e))
                        BuildingArrangeEvent.Destroy(teamAbuildBase, e);
                }
                var damagedBudget = MapBuildPropertyEvent.GetBuildProperty(teamAbuildBase);
                MapBuildPropertyEvent.AddBuildProperty(teamAbuildBase, -damagedBudget);
            }
        }

        public static bool IsBattleTownWar()
        {
            return g.world.battle.data.dungeonBaseItem.id == TOWN_WAR_DUNGEON_BASE_ID;
        }

        public static bool IsBattleMonstWave()
        {
            return g.world.battle.data.dungeonBaseItem.id == TOWN_MONST_WAVE_DUNGEON_BASE_ID ||
                g.world.battle.data.dungeonBaseItem.id == SECT_MONST_WAVE_DUNGEON_BASE_ID;
        }

        public static void CalTownWarInfo(MapBuildTown townA_def, MapBuildTown townB_atk)
        {
            teamAWUnits = g.world.unit.GetUnitExact(townA_def.GetOrigiPoint(), JOIN_RANGE, false, false).ToList();
            teamBWUnits = MapBuildPropertyEvent.GetTownGuardians(townB_atk);

            var areaId = townA_def.gridData.areaBaseID;
            var defCityR = townA_def.IsCity() ? 2 : 1;
            var atkCityR = townB_atk.IsCity() ? 2 : 1;
            teamAUnitCount = 10 + (teamAWUnits.Count * (20 / areaId) * defCityR) + (MapBuildPropertyEvent.GetBuildProperty(townA_def) / 1000000).Parse<int>();
            teamBUnitCount = 10 + (teamBWUnits.Count * (20 / areaId) * atkCityR) + (MapBuildPropertyEvent.GetBuildProperty(townB_atk) / 1000000).Parse<int>();
        }

        public static void CalMonstWaveInfo(MapBuildBase baseA_def)
        {
            teamAWUnits = g.world.unit.GetUnitExact(baseA_def.GetOrigiPoint(), JOIN_RANGE, false, false).ToList();
            teamBWUnits = null;

            var gameLvl = g.data.dataWorld.data.gameLevel.Parse<int>();
            var areaId = baseA_def.gridData.areaBaseID;
            var cityR = baseA_def.IsCity() ? 2 : 1;
            teamAUnitCount = 10 + (teamAWUnits.Count * (20 / areaId) * cityR) + (MapBuildPropertyEvent.GetBuildProperty(baseA_def) / 1000000).Parse<int>();
            teamBUnitCount = ((100 + g.world.run.roundMonth + teamAUnitCount * gameLvl + Math.Pow(2, areaId).Parse<int>()) * CommonTool.Random(0.8f, 1.1f)).Parse<int>();
        }

        public static int GetBattleSide(UnitCtrlBase cunit)
        {
            if (teamACUnits.Any(x => x.data.createUnitSoleID == cunit.data.createUnitSoleID))
                return 1;
            if (teamBCUnits.Any(x => x.data.createUnitSoleID == cunit.data.createUnitSoleID))
                return 2;
            return 0;
        }

        public override void OnBattleStart(ETypeData e)
        {
            base.OnBattleStart(e);
            if (ModBattleEvent.SceneBattle != null && (IsBattleTownWar() || IsBattleMonstWave()))
            {
                if (IsBattleTownWar())
                {
                    var utA = playerSide == 1 ? UnitType.PlayerNPC : UnitType.Monst;
                    var utB = playerSide == 2 ? UnitType.PlayerNPC : UnitType.Monst;
                    if (teamACUnits == null)
                        teamACUnits = teamAWUnits.Select(x => ModBattleEvent.SceneBattle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(x.data, utA)).Cast<UnitCtrlBase>().ToList();
                    if (teamBCUnits == null)
                        teamBCUnits = teamAWUnits.Select(x => ModBattleEvent.SceneBattle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(x.data, utB)).Cast<UnitCtrlBase>().ToList();
                }
                else if (IsBattleMonstWave())
                {
                    teamACUnits = teamAWUnits.Select(x => ModBattleEvent.SceneBattle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(x.data, UnitType.PlayerNPC)).Cast<UnitCtrlBase>().ToList();
                }
            }
        }

        //public override void OnBattleUnitDie(UnitDie e)
        //{
        //    base.OnBattleUnitDie(e);
        //    if (ModBattleEvent.SceneBattle != null && IsBattleTownWar() && e.unit.IsHuman())
        //    {
        //        var side = GetBattleSide(e.unit);
        //        var count = side == 1 ? teamAUnitCount : teamBUnitCount;
        //        if (count > 0)
        //        {
        //            e.unit.Resurge(3, (Il2CppSystem.Action)(() =>
        //            {
        //                if (side == 1)
        //                    teamAUnitCount--;
        //                else
        //                    teamBUnitCount--;
        //            }));
        //        }
        //    }
        //}

        public override void OnBattleEndOnce(BattleEnd e)
        {
            base.OnBattleEndOnce(e);
            if (ModBattleEvent.SceneBattle != null && (IsBattleTownWar() || IsBattleMonstWave()))
            {
                teamAWUnits = null;
                teamBWUnits = null;
                teamACUnits = null;
                teamBCUnits = null;
            }
        }

        //[ErrorIgnore]
        //[EventCondition(IsInGame = HandleEnum.Ignore, IsInBattle = HandleEnum.True)]
        //public override void OnTimeUpdate1s()
        //{
        //    base.OnTimeUpdate1s();
        //    var areaId = g.world.playerUnit.data.unitData.pointGridData.areaBaseID;
        //    if (ModBattleEvent.SceneBattle != null && IsBattleMonstWave() && teamBUnitCount > 0 &&
        //        ModBattleEvent.BattleMonsters.Count < (MIN_MONST + STP_MONST * areaId))
        //    {
        //        var monstLvl = CommonTool.Random(areaId - 1, areaId + 1).FixValue(0, monstList.Length - 1);
        //        var cunit = ModBattleEvent.SceneBattle.unit.CreateUnitMonstNotAddList(monstList[monstLvl], Vector2.zero, UnitType.Monst);
        //        teamBUnitCount--;
        //    }
        //}
    }
}
