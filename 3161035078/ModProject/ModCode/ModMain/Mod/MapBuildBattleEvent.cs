using EBattleTypeData;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using ModLib.Enum;
using ModLib.Mod;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.MAP_BUILD_BATTLE_EVENT)]
    public class MapBuildBattleEvent : ModEvent
    {
        public enum TeamSide
        {
            TeamA,
            TeamB,
        }

        public static MapBuildBattleEvent Instance { get; set; }

        public const int JOIN_RANGE = 4;
        public const float MONST_WAVE_RATE = 1f;
        public const float TOWN_WAR_RATE = 0.4f;
        public const int MIN_UNIT = 6;
        public const int STP_UNIT = 2;

        public const int TOWN_WAR_DUNGEON_BASE_ID = 480110990;
        public const int TOWN_MONST_WAVE_DUNGEON_BASE_ID = 480110991;
        public const int SECT_MONST_WAVE_DUNGEON_BASE_ID = 480110992;

        public Dictionary<string, int> LastYearEventHappen { get; set; } = new Dictionary<string, int>();

        [JsonIgnore]
        public static TeamSide PlayerSide { get; set; }
        [JsonIgnore]
        public static int TeamAUnitCount { get; set; } = 0;
        [JsonIgnore]
        public static int TeamBUnitCount { get; set; } = 0;
        [JsonIgnore]
        public static List<WorldUnitBase> TeamAWUnits { get; set; } = new List<WorldUnitBase>();
        [JsonIgnore]
        public static List<WorldUnitBase> TeamBWUnits { get; set; } = new List<WorldUnitBase>();
        [JsonIgnore]
        public static List<UnitCtrlBase> TeamACUnits { get; set; } = new List<UnitCtrlBase>();
        [JsonIgnore]
        public static List<UnitCtrlBase> TeamBCUnits { get; set; } = new List<UnitCtrlBase>();

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
                PlayerSide = TeamSide.TeamA;
            else if (MapBuildPropertyEvent.IsTownGuardian(townB_atk, g.world.playerUnit))
                PlayerSide = TeamSide.TeamB;
            else
                PlayerSide = TeamSide.TeamA;
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
            var teamAPoint = ValueHelper.SumBigNum(TeamAWUnits.Select(x =>
            {
                return
                    (x.GetDynProperty(UnitDynPropertyEnum.Attack).value * 3) +
                    (x.GetDynProperty(UnitDynPropertyEnum.Defense).value * 10) +
                    (x.GetDynProperty(UnitDynPropertyEnum.HpMax).value) +
                    (x.GetDynProperty(UnitDynPropertyEnum.MpMax).value * 3) +
                    (x.GetDynProperty(UnitDynPropertyEnum.SpMax).value * 5) +
                    (x.GetEquippedArtifacts().Select(a => Math.Pow(2, a.propsInfoBase.grade) * 10000 + Math.Pow(2, a.propsInfoBase.level) * 2000).Sum());
            }).ToArray());
            var teamATotalPoint = teamAPoint * TeamAUnitCount * /*def point*/1.2;
            var teamBPoint = ValueHelper.SumBigNum(TeamBWUnits.Select(x =>
            {
                return
                    (x.GetDynProperty(UnitDynPropertyEnum.Attack).value * 3) +
                    (x.GetDynProperty(UnitDynPropertyEnum.Defense).value * 10) +
                    (x.GetDynProperty(UnitDynPropertyEnum.HpMax).value) +
                    (x.GetDynProperty(UnitDynPropertyEnum.MpMax).value * 3) +
                    (x.GetDynProperty(UnitDynPropertyEnum.SpMax).value * 5) +
                    (x.GetEquippedArtifacts().Select(a => Math.Pow(2, a.propsInfoBase.grade) * 10000 + Math.Pow(2, a.propsInfoBase.level) * 2000).Sum());
            }).ToArray());
            var teamBTotalPoint = teamBPoint * TeamBUnitCount;
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
            PlayerSide = TeamSide.TeamA;
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
            var teamAPoint = ValueHelper.SumBigNum(TeamAWUnits.Select(x =>
            {
                return
                    (x.GetDynProperty(UnitDynPropertyEnum.Attack).value * 3) +
                    (x.GetDynProperty(UnitDynPropertyEnum.Defense).value * 10) +
                    (x.GetDynProperty(UnitDynPropertyEnum.HpMax).value) +
                    (x.GetDynProperty(UnitDynPropertyEnum.MpMax).value * 3) +
                    (x.GetDynProperty(UnitDynPropertyEnum.SpMax).value * 5) +
                    (x.GetEquippedArtifacts().Select(a => Math.Pow(2, a.propsInfoBase.grade) * 10000 + Math.Pow(2, a.propsInfoBase.level) * 2000).Sum());
            }).ToArray());
            var teamATotalPoint = teamAPoint * TeamAUnitCount * /*def point*/1.2;
            var gameLvl = g.data.dataWorld.data.gameLevel.Parse<int>();
            var areaId = teamAbuildBase.gridData.areaBaseID;
            var teamBPoint = Math.Pow(3, areaId) * 10000 + Math.Pow(3, gameLvl) * 3000;
            var teamBTotalPoint = teamBPoint * TeamBUnitCount;
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
            var areaId = townA_def.gridData.areaBaseID;

            TeamAWUnits = g.world.unit.GetUnitExact(townA_def.GetOrigiPoint(), JOIN_RANGE, true, false).ToArray().Take(MIN_UNIT + STP_UNIT * areaId).ToList();
            TeamBWUnits = MapBuildPropertyEvent.GetTownGuardians(townB_atk).Where(x => !x.IsPlayer()).ToList();

            var defCityR = townA_def.IsCity() ? 2 : 1;
            var atkCityR = townB_atk.IsCity() ? 2 : 1;
            TeamAUnitCount = 10 + (TeamAWUnits.Count * (20 / areaId) * defCityR) + (MapBuildPropertyEvent.GetBuildProperty(townA_def) / 1000000).Parse<int>();
            TeamBUnitCount = 10 + (TeamBWUnits.Count * (20 / areaId) * atkCityR) + (MapBuildPropertyEvent.GetBuildProperty(townB_atk) / 1000000).Parse<int>();
        }

        public static void CalMonstWaveInfo(MapBuildBase baseA_def)
        {
            var gameLvl = g.data.dataWorld.data.gameLevel.Parse<int>();
            var areaId = baseA_def.gridData.areaBaseID;

            TeamAWUnits = g.world.unit.GetUnitExact(baseA_def.GetOrigiPoint(), JOIN_RANGE, true, false).ToArray().Take(MIN_UNIT + STP_UNIT * areaId).ToList();
            TeamBWUnits = null;

            var cityR = baseA_def.IsCity() ? 2 : 1;
            TeamAUnitCount = 10 + (TeamAWUnits.Count * (20 / areaId) * cityR) + (MapBuildPropertyEvent.GetBuildProperty(baseA_def) / 1000000).Parse<int>();
            TeamBUnitCount = ((100 + g.world.run.roundMonth + TeamAUnitCount * gameLvl + Math.Pow(2, areaId).Parse<int>()) * CommonTool.Random(0.8f, 1.1f)).Parse<int>();
        }

        public static TeamSide GetBattleSide(UnitCtrlBase cunit)
        {
            if (cunit.IsPlayer())
                return PlayerSide;
            if (TeamACUnits.Any(x => x.data.createUnitSoleID == cunit.data.createUnitSoleID))
                return TeamSide.TeamA;
            if (TeamBCUnits.Any(x => x.data.createUnitSoleID == cunit.data.createUnitSoleID))
                return TeamSide.TeamB;
            return 0;
        }

        public override void OnBattleStart(ETypeData e)
        {
            base.OnBattleStart(e);
            if (ModBattleEvent.SceneBattle != null && (IsBattleTownWar() || IsBattleMonstWave()))
            {
                //create team units
                if (IsBattleTownWar())
                {
                    var utA = PlayerSide == TeamSide.TeamA ? UnitType.PlayerNPC : UnitType.Monst;
                    var utB = PlayerSide == TeamSide.TeamB ? UnitType.PlayerNPC : UnitType.Monst;
                    if (TeamACUnits == null)
                    {
                        TeamACUnits.AddRange(TeamAWUnits.Select(x => ModBattleEvent.SceneBattle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(x.data, utA)).Cast<UnitCtrlBase>());
                    }
                    if (TeamBCUnits == null)
                    {
                        TeamBCUnits.AddRange(TeamBWUnits.Select(x => ModBattleEvent.SceneBattle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(x.data, utB)).Cast<UnitCtrlBase>());
                    }
                }
                else if (IsBattleMonstWave())
                {
                    if (TeamACUnits == null)
                    {
                        TeamACUnits.AddRange(TeamAWUnits.Select(x => ModBattleEvent.SceneBattle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(x.data, UnitType.PlayerNPC)).Cast<UnitCtrlBase>());
                    }
                    if (TeamBCUnits == null)
                    {
                        TeamBCUnits = new List<UnitCtrlBase>();
                        var areaId = g.world.playerUnit.data.unitData.pointGridData.areaBaseID;
                        for (int i = 0; i < (MIN_UNIT + STP_UNIT * areaId) * 2; i++)
                        {
                            var monstLvl = CommonTool.Random(areaId - 1, areaId + 1).FixValue(0, monstList.Length - 1);
                            TeamBCUnits.Add(ModBattleEvent.SceneBattle.unit.CreateUnitMonstNotAddList(monstList[monstLvl], Vector2.zero, UnitType.Monst));
                        }
                    }
                }
            }
        }

        public override void OnBattleUnitDie(UnitDie e)
        {
            base.OnBattleUnitDie(e);
            if (ModBattleEvent.SceneBattle != null)
            {
                if (IsBattleTownWar())
                {
                    if (e.unit.IsHuman())
                    {
                        var side = GetBattleSide(e.unit);
                        var count = side == TeamSide.TeamA ? TeamAUnitCount : TeamBUnitCount;
                        if (count > 0)
                        {
                            //-point
                            if (side == TeamSide.TeamA)
                                TeamAUnitCount--;
                            else
                                TeamBUnitCount--;
                            //create new unit
                            var wunit = e.unit.GetWorldUnit();
                            if (wunit.IsPlayer())
                            {
                                var cunit = ModBattleEvent.SceneBattle.unit.CreateUnitPlayer();
                                //set posi
                                if (side == TeamSide.TeamA)
                                    TeamACUnits.Add(cunit);
                                else
                                    TeamBCUnits.Add(cunit);
                            }
                            else
                            {
                                var cunit = ModBattleEvent.SceneBattle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(wunit.data, e.unit.data.unitType);
                                //set posi
                                if (side == TeamSide.TeamA)
                                    TeamACUnits.Add(cunit);
                                else
                                    TeamBCUnits.Add(cunit);
                            }
                        }
                    }
                }
                else
                if (IsBattleMonstWave())
                {
                    if (e.unit.IsHuman())
                    {
                        if (TeamAUnitCount > 0)
                        {
                            //-point
                            TeamAUnitCount--;
                            //create new unit
                            var wunit = e.unit.GetWorldUnit();
                            if (wunit.IsPlayer())
                            {
                                TeamACUnits.Add(ModBattleEvent.SceneBattle.unit.CreateUnitPlayer());
                            }
                            else
                            {
                                TeamACUnits.Add(ModBattleEvent.SceneBattle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(wunit.data, e.unit.data.unitType));
                            }
                        }
                    }
                    else
                    if (e.unit.IsMonster())
                    {
                        if (TeamBUnitCount > 0)
                        {
                            //-point
                            TeamBUnitCount--;
                            //create new monster
                            var areaId = g.world.playerUnit.data.unitData.pointGridData.areaBaseID;
                            var monstLvl = CommonTool.Random(areaId - 1, areaId + 1).FixValue(0, monstList.Length - 1);
                            TeamBCUnits.Add(ModBattleEvent.SceneBattle.unit.CreateUnitMonstNotAddList(monstList[monstLvl], Vector2.zero, UnitType.Monst));
                        }
                    }
                }
            }
        }

        public override void OnBattleUnitInto(UnitCtrlBase e)
        {
            base.OnBattleUnitInto(e);
            //set player posi
            if (GetBattleSide(e) == TeamSide.TeamA)
                e.move.SetPosition(GetTeamAPosi());
            else
                e.move.SetPosition(GetTeamBPosi());
        }

        public override void OnBattleEndOnce(BattleEnd e)
        {
            base.OnBattleEndOnce(e);
            if (ModBattleEvent.SceneBattle != null && (IsBattleTownWar() || IsBattleMonstWave()))
            {
                TeamAWUnits.Clear();
                TeamBWUnits.Clear();
                TeamACUnits.Clear();
                TeamBCUnits.Clear();
            }
        }

        public static Vector2 GetTeamAPosi()
        {
            return new Vector2(ModBattleEvent.SceneBattle.battleMap.roomCenterPosi.x - 40,
                CommonTool.Random(ModBattleEvent.SceneBattle.battleMap.roomCenterPosi.y - 40, ModBattleEvent.SceneBattle.battleMap.roomCenterPosi.y + 40));
        }

        public static Vector2 GetTeamBPosi()
        {
            return new Vector2(ModBattleEvent.SceneBattle.battleMap.roomCenterPosi.x + 40,
                CommonTool.Random(ModBattleEvent.SceneBattle.battleMap.roomCenterPosi.y - 40, ModBattleEvent.SceneBattle.battleMap.roomCenterPosi.y + 40));
        }
    }
}
