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
        public enum TeamSideEnum
        {
            Unmanaged,
            TeamA,
            TeamB,
        }

        public enum TeamInfoEnum
        {
            UnitCountMax,
            UnitCount,
            WUnit,
            CUnit,
        }

        public static MapBuildBattleEvent Instance { get; set; }

        public const int JOIN_RANGE = 4;
        public const float MONST_WAVE_RATE = 1f;
        public const float TOWN_WAR_RATE = 0.4f;
        public const int MIN_UNIT = 4;
        public const int STP_UNIT = 1;

        public const int TOWN_WAR_DUNGEON_BASE_ID = 480110990;
        public const int TOWN_MONST_WAVE_DUNGEON_BASE_ID = 480110991;
        public const int SECT_MONST_WAVE_DUNGEON_BASE_ID = 480110992;

        public Dictionary<string, int> LastYearEventHappen { get; set; } = new Dictionary<string, int>();

        [JsonIgnore]
        public static bool InitFlg { get; set; } = false;
        [JsonIgnore]
        public static TeamSideEnum WinTeamSide { get; set; }
        [JsonIgnore]
        public static TeamSideEnum PlayerSide { get; set; }
        [JsonIgnore]
        public static MapBuildBase BuildBaseA { get; set; }
        [JsonIgnore]
        public static MapBuildBase BuildBaseB { get; set; }
        [JsonIgnore]
        public static int TeamAUnitCount => GetTeamInfo<int>(TeamSideEnum.TeamA, TeamInfoEnum.UnitCount);
        [JsonIgnore]
        public static int TeamBUnitCount => GetTeamInfo<int>(TeamSideEnum.TeamB, TeamInfoEnum.UnitCount);
        [JsonIgnore]
        public static List<WorldUnitBase> TeamAWUnits => GetTeamInfo<List<WorldUnitBase>>(TeamSideEnum.TeamA, TeamInfoEnum.WUnit);
        [JsonIgnore]
        public static List<WorldUnitBase> TeamBWUnits => GetTeamInfo<List<WorldUnitBase>>(TeamSideEnum.TeamB, TeamInfoEnum.WUnit);
        [JsonIgnore]
        public static List<UnitCtrlBase> TeamACUnits => GetTeamInfo<List<UnitCtrlBase>>(TeamSideEnum.TeamA, TeamInfoEnum.CUnit);
        [JsonIgnore]
        public static List<UnitCtrlBase> TeamBCUnits => GetTeamInfo<List<UnitCtrlBase>>(TeamSideEnum.TeamB, TeamInfoEnum.CUnit);
        [JsonIgnore]
        public static List<string> DieCUnits { get; } = new List<string>();
        [JsonIgnore]
        public static Dictionary<string, object> TeamInfo { get; set; } = new Dictionary<string, object>()
        {
            [$"{TeamSideEnum.TeamA}_{TeamInfoEnum.UnitCountMax}"] = 0,
            [$"{TeamSideEnum.TeamA}_{TeamInfoEnum.UnitCount}"] = 0,
            [$"{TeamSideEnum.TeamA}_{TeamInfoEnum.WUnit}"] = new List<WorldUnitBase>(),
            [$"{TeamSideEnum.TeamA}_{TeamInfoEnum.CUnit}"] = new List<UnitCtrlBase>(),

            [$"{TeamSideEnum.TeamB}_{TeamInfoEnum.UnitCountMax}"] = 0,
            [$"{TeamSideEnum.TeamB}_{TeamInfoEnum.UnitCount}"] = 0,
            [$"{TeamSideEnum.TeamB}_{TeamInfoEnum.WUnit}"] = new List<WorldUnitBase>(),
            [$"{TeamSideEnum.TeamB}_{TeamInfoEnum.CUnit}"] = new List<UnitCtrlBase>(),
        };

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
                    if (master == null)
                        return;
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
            //init
            InitBattle();
            //player side
            if (MapBuildPropertyEvent.IsTownGuardian(townA_def, g.world.playerUnit))
                PlayerSide = TeamSideEnum.TeamA;
            else if (MapBuildPropertyEvent.IsTownGuardian(townB_atk, g.world.playerUnit))
                PlayerSide = TeamSideEnum.TeamB;
            else
                PlayerSide = TeamSideEnum.TeamA;
            //battle info
            BuildBaseA = townA_def;
            BuildBaseB = townB_atk;
            CalTownWarInfo(townA_def, townB_atk);
            //settings
            BattleModifyEvent.IsShowCustomMonstCount = true;
            //battle into
            g.world.battle.IntoBattle(new DataMap.MonstData() { id = TOWN_WAR_DUNGEON_BASE_ID, level = townA_def.gridData.areaBaseID * 5 });
        }

        public static void SkipTownWar(MapBuildTown townA_def, MapBuildTown townB_atk)
        {
            //init
            InitBattle();
            //player side
            PlayerSide = TeamSideEnum.Unmanaged;
            //battle info
            BuildBaseA = townA_def;
            BuildBaseB = townB_atk;
            CalTownWarInfo(townA_def, townB_atk);
            //auto battle
            var teamAPoint = TeamAWUnits.Count == 0 ? 0 : TeamAWUnits.Average(x =>
            {
                return
                    (x.GetDynProperty(UnitDynPropertyEnum.Attack).value * 3) +
                    (x.GetDynProperty(UnitDynPropertyEnum.Defense).value * 10) +
                    (x.GetDynProperty(UnitDynPropertyEnum.HpMax).value) +
                    (x.GetDynProperty(UnitDynPropertyEnum.MpMax).value * 3) +
                    (x.GetDynProperty(UnitDynPropertyEnum.SpMax).value * 5) +
                    (x.GetEquippedArtifacts().Select(a => Math.Pow(2, a.propsInfoBase.grade) * 10000 + Math.Pow(2, a.propsInfoBase.level) * 2000).Sum());
            });
            var teamATotalPoint = teamAPoint * TeamAUnitCount * /*def point*/1.5;
            var teamBPoint = TeamBWUnits.Count == 0 ? 0 : TeamBWUnits.Average(x =>
            {
                return
                    (x.GetDynProperty(UnitDynPropertyEnum.Attack).value * 3) +
                    (x.GetDynProperty(UnitDynPropertyEnum.Defense).value * 10) +
                    (x.GetDynProperty(UnitDynPropertyEnum.HpMax).value) +
                    (x.GetDynProperty(UnitDynPropertyEnum.MpMax).value * 3) +
                    (x.GetDynProperty(UnitDynPropertyEnum.SpMax).value * 5) +
                    (x.GetEquippedArtifacts().Select(a => Math.Pow(2, a.propsInfoBase.grade) * 10000 + Math.Pow(2, a.propsInfoBase.level) * 2000).Sum());
            });
            var teamBTotalPoint = teamBPoint * TeamBUnitCount;
            //battle end
            var ratioAB = teamATotalPoint / teamBTotalPoint;
            var ratioBA = teamBTotalPoint / teamATotalPoint;
            if (teamATotalPoint >= teamBTotalPoint)
            {
                //A win
                WinTeamSide = TeamSideEnum.TeamA;
                //A damaged
                if (ratioAB < 2)
                {
                    //building damaged
                    BuildingArrangeEvent.Destroy(townA_def, BuildingCostEnum.GetAllEnums<BuildingCostEnum>().Where(x => BuildingArrangeEvent.IsBuilt(townA_def, x)).ToArray().Random());
                }
                MapBuildPropertyEvent.AddBuildProperty(townA_def, -(MapBuildPropertyEvent.GetBuildProperty(townA_def) * ratioBA / 5).Parse<long>());
                //B damaged
                MapBuildPropertyEvent.AddBuildProperty(townB_atk, -(MapBuildPropertyEvent.GetBuildProperty(townB_atk) * ratioAB / 10).Parse<long>());
                //hate
                foreach (var wunitB in MapBuildPropertyEvent.GetTownGuardians(BuildBaseB.TryCast<MapBuildTown>()))
                {
                    foreach (var wunitA in MapBuildPropertyEvent.GetTownGuardians(BuildBaseA.TryCast<MapBuildTown>()))
                    {
                        wunitB.data.unitData.relationData.AddHate(wunitA.GetUnitId(), 100);
                        wunitA.data.unitData.relationData.AddHate(wunitB.GetUnitId(), 50);
                        if (MapBuildPropertyEvent.IsTownMaster(wunitA))
                            wunitB.data.unitData.relationData.AddHate(wunitA.GetUnitId(), 100);
                    }
                }
            }
            else
            {
                //B win
                WinTeamSide = TeamSideEnum.TeamB;
                //A damaged x2
                BuildingArrangeEvent.Destroy(townA_def, BuildingCostEnum.GetAllEnums<BuildingCostEnum>().Where(x => BuildingArrangeEvent.IsBuilt(townA_def, x)).ToArray().Random());
                BuildingArrangeEvent.Destroy(townA_def, BuildingCostEnum.GetAllEnums<BuildingCostEnum>().Where(x => BuildingArrangeEvent.IsBuilt(townA_def, x)).ToArray().Random());
                var damagedBudget = MapBuildPropertyEvent.GetBuildProperty(townA_def);
                MapBuildPropertyEvent.AddBuildProperty(townA_def, -damagedBudget);
                //B damaged
                MapBuildPropertyEvent.AddBuildProperty(townB_atk, -(MapBuildPropertyEvent.GetBuildProperty(townB_atk) * ratioAB / 5).Parse<long>());
                //B pillage
                MapBuildPropertyEvent.AddBuildProperty(townB_atk, damagedBudget);
                //hate
                foreach (var wunitA in MapBuildPropertyEvent.GetTownGuardians(BuildBaseA.TryCast<MapBuildTown>()))
                {
                    foreach (var wunitB in MapBuildPropertyEvent.GetTownGuardians(BuildBaseB.TryCast<MapBuildTown>()))
                    {
                        wunitA.data.unitData.relationData.AddHate(wunitB.GetUnitId(), 100);
                        wunitB.data.unitData.relationData.AddHate(wunitA.GetUnitId(), 50);
                        if (MapBuildPropertyEvent.IsTownMaster(wunitB))
                            wunitA.data.unitData.relationData.AddHate(wunitB.GetUnitId(), 100);
                    }
                }
            }
            //intim A
            foreach (var wunitA1 in TeamACUnits.Select(x => x.GetWorldUnit()).Where(x => x != null))
            {
                foreach (var wunitA2 in TeamACUnits.Select(x => x.GetWorldUnit()).Where(x => x != null))
                {
                    wunitA1.data.unitData.relationData.AddIntim(wunitA2.GetUnitId(), 50);
                    wunitA2.data.unitData.relationData.AddIntim(wunitA1.GetUnitId(), 50);
                }
            }
            //intim B
            foreach (var wunitB1 in TeamBCUnits.Select(x => x.GetWorldUnit()).Where(x => x != null))
            {
                foreach (var wunitB2 in TeamBCUnits.Select(x => x.GetWorldUnit()).Where(x => x != null))
                {
                    wunitB1.data.unitData.relationData.AddIntim(wunitB2.GetUnitId(), 50);
                    wunitB2.data.unitData.relationData.AddIntim(wunitB1.GetUnitId(), 50);
                }
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
            //init
            InitBattle();
            //player side
            PlayerSide = TeamSideEnum.TeamA;
            //battle info
            BuildBaseA = teamAbuildBase;
            BuildBaseB = null;
            CalMonstWaveInfo(teamAbuildBase);
            //settings
            BattleModifyEvent.IsShowCustomMonstCount = true;
            //battle into
            g.world.battle.IntoBattle(new DataMap.MonstData() { id = dungeonBaseId, level = teamAbuildBase.gridData.areaBaseID * 5 });
        }

        public static void SkipMonstWave(MapBuildBase teamAbuildBase)
        {
            //init
            InitBattle();
            //player side
            PlayerSide = TeamSideEnum.TeamA;
            //battle info
            BuildBaseA = teamAbuildBase;
            BuildBaseB = null;
            CalMonstWaveInfo(teamAbuildBase);
            //auto battle
            var teamAPoint = TeamAWUnits.Count == 0 ? 0 : TeamAWUnits.Average(x =>
            {
                return
                    (x.GetDynProperty(UnitDynPropertyEnum.Attack).value * 3) +
                    (x.GetDynProperty(UnitDynPropertyEnum.Defense).value * 10) +
                    (x.GetDynProperty(UnitDynPropertyEnum.HpMax).value) +
                    (x.GetDynProperty(UnitDynPropertyEnum.MpMax).value * 3) +
                    (x.GetDynProperty(UnitDynPropertyEnum.SpMax).value * 5) +
                    (x.GetEquippedArtifacts().Select(a => Math.Pow(2, a.propsInfoBase.grade) * 10000 + Math.Pow(2, a.propsInfoBase.level) * 2000).Sum());
            });
            var teamATotalPoint = teamAPoint * TeamAUnitCount * /*def point*/1.5;
            var gameLvl = g.data.dataWorld.data.gameLevel.Parse<int>();
            var areaId = teamAbuildBase.gridData.areaBaseID;
            var teamBPoint = Math.Pow(3, areaId) * 10000 + Math.Pow(3, gameLvl) * 3000; /*Monster*/
            var teamBTotalPoint = teamBPoint * TeamBUnitCount;
            //battle end
            var ratioAB = teamATotalPoint / teamBTotalPoint;
            var ratioBA = teamBTotalPoint / teamATotalPoint;
            if (teamATotalPoint >= teamBTotalPoint)
            {
                //A win
                WinTeamSide = TeamSideEnum.TeamA;
                //A damaged
                MapBuildPropertyEvent.AddBuildProperty(teamAbuildBase, -(MapBuildPropertyEvent.GetBuildProperty(teamAbuildBase) * ratioBA / 3).Parse<long>());
            }
            else
            {
                //B win
                WinTeamSide = TeamSideEnum.TeamB;
                //A damaged
                foreach (var e in BuildingCostEnum.GetAllEnums<BuildingCostEnum>())
                {
                    if (BuildingArrangeEvent.IsBuilt(teamAbuildBase, e))
                        BuildingArrangeEvent.Destroy(teamAbuildBase, e);
                }
                var damagedBudget = MapBuildPropertyEvent.GetBuildProperty(teamAbuildBase);
                MapBuildPropertyEvent.AddBuildProperty(teamAbuildBase, -damagedBudget);
            }
            //intim A
            foreach (var wunitA1 in TeamACUnits.Select(x => x.GetWorldUnit()).Where(x => x != null))
            {
                foreach (var wunitA2 in TeamACUnits.Select(x => x.GetWorldUnit()).Where(x => x != null))
                {
                    wunitA1.data.unitData.relationData.AddIntim(wunitA2.GetUnitId(), 50);
                    wunitA2.data.unitData.relationData.AddIntim(wunitA1.GetUnitId(), 50);
                }
            }
        }

        public static void CalTownWarInfo(MapBuildTown townA_def, MapBuildTown townB_atk)
        {
            TeamAWUnits.AddRange(MapBuildPropertyEvent.GetTownGuardians(townA_def).Where(x => !x.IsPlayer()));
            TeamBWUnits.AddRange(MapBuildPropertyEvent.GetTownGuardians(townB_atk).Where(x => !x.IsPlayer()));

            var defCityR = townA_def.IsCity() ? 2 : 1;
            var atkCityR = townB_atk.IsCity() ? 2 : 1;
            InitTeamInfo(TeamSideEnum.TeamA, 8 + (((TeamAWUnits.Count * defCityR) + (MapBuildPropertyEvent.GetBuildProperty(townA_def) / 1000000)) * 1.5).Parse<int>());
            InitTeamInfo(TeamSideEnum.TeamB, 8 + (TeamBWUnits.Count * atkCityR) + (MapBuildPropertyEvent.GetBuildProperty(townB_atk) / 1000000).Parse<int>());
        }

        public static void CalMonstWaveInfo(MapBuildBase baseA_def)
        {
            var gameLvl = g.data.dataWorld.data.gameLevel.Parse<int>();
            var areaId = baseA_def.gridData.areaBaseID;

            TeamAWUnits.AddRange(g.world.unit.GetUnitExact(baseA_def.GetOrigiPoint(), JOIN_RANGE, true, false).ToArray().Take(MIN_UNIT + STP_UNIT * areaId));
            TeamBWUnits.Clear();

            var cityR = baseA_def.IsCity() ? 2 : 1;
            InitTeamInfo(TeamSideEnum.TeamA, 8 + (((TeamAWUnits.Count * cityR) + (MapBuildPropertyEvent.GetBuildProperty(baseA_def) / 1000000)) * 1.5).Parse<int>());
            InitTeamInfo(TeamSideEnum.TeamB, ((g.world.run.roundMonth + TeamAUnitCount * gameLvl + Math.Pow(2, areaId).Parse<int>()) * CommonTool.Random(0.8f, 1.1f)).Parse<int>());
        }

        public static void InitBattle()
        {
            TeamAWUnits.Clear();
            TeamBWUnits.Clear();
            TeamACUnits.Clear();
            TeamBCUnits.Clear();
            DieCUnits.Clear();
            InitFlg = false;
            WinTeamSide = TeamSideEnum.Unmanaged;
        }

        public override void OnBattleStart(ETypeData e)
        {
            base.OnBattleStart(e);
            if (ModBattleEvent.SceneBattle != null && (IsBattleTownWar() || IsBattleMonstWave()) && !InitFlg)
            {
                InitFlg = true;
                //init
                InitUnit(ModBattleEvent.PlayerUnit);
                //create team units
                if (IsBattleTownWar())
                {
                    var utA = PlayerSide == TeamSideEnum.TeamA ? UnitType.PlayerNPC : UnitType.Monst;
                    TeamACUnits.AddRange(TeamAWUnits.Select(x => ModBattleEvent.SceneBattle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(x.data, utA)).Cast<UnitCtrlBase>());
                    TeamACUnits.ForEach(x => InitUnit(x));
                    var utB = PlayerSide == TeamSideEnum.TeamB ? UnitType.PlayerNPC : UnitType.Monst;
                    TeamBCUnits.AddRange(TeamBWUnits.Select(x => ModBattleEvent.SceneBattle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(x.data, utB)).Cast<UnitCtrlBase>());
                    TeamBCUnits.ForEach(x => InitUnit(x));
                }
                else if (IsBattleMonstWave())
                {
                    TeamACUnits.AddRange(TeamAWUnits.Select(x => ModBattleEvent.SceneBattle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(x.data, UnitType.PlayerNPC)).Cast<UnitCtrlBase>());
                    TeamACUnits.ForEach(x => InitUnit(x));
                    var areaId = g.world.playerUnit.data.unitData.pointGridData.areaBaseID;
                    for (int i = 0; i < (MIN_UNIT + STP_UNIT * areaId) * 2; i++)
                    {
                        var monstLvl = CommonTool.Random(areaId - 1, areaId + 1).FixValue(0, monstList.Length - 1);
                        var cunit = ModBattleEvent.SceneBattle.unit.CreateUnitMonstNotAddList(monstList[monstLvl], Vector2.zero, UnitType.Monst);
                        TeamBCUnits.Add(cunit);
                        InitUnit(cunit);
                    }
                }
            }
        }

        public override void OnBattleUnitHit(UnitHit e)
        {
            base.OnBattleUnitHit(e);
            if (e.hitUnit.data.hp <= 1 && (IsBattleTownWar() || IsBattleMonstWave()))
            {
                if (IsBattleTownWar())
                {
                    var side = GetTeamSide(e.hitUnit);
                    if (side != TeamSideEnum.Unmanaged && e.hitUnit.IsHuman())
                    {
                        var count = GetTeamInfo<int>(side, TeamInfoEnum.UnitCount);
                        if (count > 0)
                        {
                            //-point
                            AddTeamCount(side, -1);
                            //revive
                            Revive(e.hitUnit);
                        }
                        else
                        {
                            Die(e.hitUnit);
                            //end
                            var winSide = side == TeamSideEnum.TeamA ? TeamSideEnum.TeamB : TeamSideEnum.TeamA;
                            BattleEnd(side, winSide);
                        }
                    }
                }
                else
                if (IsBattleMonstWave())
                {
                    var side = GetTeamSide(e.hitUnit);
                    if (side == TeamSideEnum.TeamA && e.hitUnit.IsHuman())
                    {
                        if (TeamAUnitCount > 0)
                        {
                            //-point
                            AddTeamCount(TeamSideEnum.TeamA, -1);
                            //revive
                            Revive(e.hitUnit);
                        }
                        else
                        {
                            Die(e.hitUnit);
                            //end
                            BattleEnd(TeamSideEnum.TeamA, TeamSideEnum.TeamB);
                        }
                    }
                }
            }
        }

        public override void OnBattleUnitDie(UnitDie e)
        {
            base.OnBattleUnitDie(e);
            if (IsBattleMonstWave() && e.unit.IsMonster())
            {
                if (TeamBUnitCount > 0)
                {
                    //-point
                    AddTeamCount(TeamSideEnum.TeamB, -1);
                    //create new monster
                    var areaId = g.world.playerUnit.data.unitData.pointGridData.areaBaseID;
                    var monstLvl = CommonTool.Random(areaId - 1, areaId + 1).FixValue(0, monstList.Length - 1);
                    TeamBCUnits.Add(ModBattleEvent.SceneBattle.unit.CreateUnitMonstNotAddList(monstList[monstLvl], Vector2.zero, UnitType.Monst));
                }
                else
                {
                    //end
                    BattleEnd(TeamSideEnum.TeamB, TeamSideEnum.TeamA);
                }
            }
        }

        public override void OnBattleEndOnce(BattleEnd e)
        {
            base.OnBattleEndOnce(e);
            if (ModBattleEvent.SceneBattle != null && (IsBattleTownWar() || IsBattleMonstWave()))
            {
                if (WinTeamSide == TeamSideEnum.TeamA)
                {
                    //A win
                    //A damaged
                    BuildingArrangeEvent.Destroy(BuildBaseA, BuildingCostEnum.GetAllEnums<BuildingCostEnum>().Where(x => BuildingArrangeEvent.IsBuilt(BuildBaseA, x)).ToArray().Random());
                    var lossrateA = 1f - (GetTeamInfo<float>(TeamSideEnum.TeamA, TeamInfoEnum.UnitCount) / GetTeamInfo<float>(TeamSideEnum.TeamA, TeamInfoEnum.UnitCountMax));
                    MapBuildPropertyEvent.AddBuildProperty(BuildBaseA, -(MapBuildPropertyEvent.GetBuildProperty(BuildBaseA) * lossrateA).Parse<long>());
                    //B damaged
                    if (IsBattleTownWar())
                    {
                        MapBuildPropertyEvent.AddBuildProperty(BuildBaseB, -(MapBuildPropertyEvent.GetBuildProperty(BuildBaseB) * 0.8).Parse<long>());
                        //hate
                        foreach (var wunitB in MapBuildPropertyEvent.GetTownGuardians(BuildBaseB.TryCast<MapBuildTown>()))
                        {
                            foreach (var wunitA in MapBuildPropertyEvent.GetTownGuardians(BuildBaseA.TryCast<MapBuildTown>()))
                            {
                                wunitB.data.unitData.relationData.AddHate(wunitA.GetUnitId(), 100);
                                wunitA.data.unitData.relationData.AddHate(wunitB.GetUnitId(), 50);
                                if (MapBuildPropertyEvent.IsTownMaster(wunitA))
                                    wunitB.data.unitData.relationData.AddHate(wunitA.GetUnitId(), 100);
                            }
                        }
                    }
                }
                else
                {
                    //B win
                    //A damaged x2
                    BuildingArrangeEvent.Destroy(BuildBaseA, BuildingCostEnum.GetAllEnums<BuildingCostEnum>().Where(x => BuildingArrangeEvent.IsBuilt(BuildBaseA, x)).ToArray().Random());
                    BuildingArrangeEvent.Destroy(BuildBaseA, BuildingCostEnum.GetAllEnums<BuildingCostEnum>().Where(x => BuildingArrangeEvent.IsBuilt(BuildBaseA, x)).ToArray().Random());
                    var damagedBudget = MapBuildPropertyEvent.GetBuildProperty(BuildBaseA);
                    MapBuildPropertyEvent.AddBuildProperty(BuildBaseA, -damagedBudget);
                    if (IsBattleTownWar())
                    {
                        //B damaged
                        var lossrateB = 1f - (GetTeamInfo<float>(TeamSideEnum.TeamB, TeamInfoEnum.UnitCount) / GetTeamInfo<float>(TeamSideEnum.TeamB, TeamInfoEnum.UnitCountMax));
                        MapBuildPropertyEvent.AddBuildProperty(BuildBaseB, -(MapBuildPropertyEvent.GetBuildProperty(BuildBaseB) * lossrateB).Parse<long>());
                        //B pillage
                        MapBuildPropertyEvent.AddBuildProperty(BuildBaseB, damagedBudget);
                        //hate
                        foreach (var wunitA in MapBuildPropertyEvent.GetTownGuardians(BuildBaseA.TryCast<MapBuildTown>()))
                        {
                            foreach (var wunitB in MapBuildPropertyEvent.GetTownGuardians(BuildBaseB.TryCast<MapBuildTown>()))
                            {
                                wunitA.data.unitData.relationData.AddHate(wunitB.GetUnitId(), 100);
                                wunitB.data.unitData.relationData.AddHate(wunitA.GetUnitId(), 50);
                                if (MapBuildPropertyEvent.IsTownMaster(wunitB))
                                    wunitA.data.unitData.relationData.AddHate(wunitB.GetUnitId(), 100);
                            }
                        }
                    }
                }
                //intim A
                foreach (var wunitA1 in TeamACUnits.Select(x => x.GetWorldUnit()).Where(x => x != null))
                {
                    foreach (var wunitA2 in TeamACUnits.Select(x => x.GetWorldUnit()).Where(x => x != null))
                    {
                        wunitA1.data.unitData.relationData.AddIntim(wunitA2.GetUnitId(), 50);
                        wunitA2.data.unitData.relationData.AddIntim(wunitA1.GetUnitId(), 50);
                    }
                }
                //intim B
                if (IsBattleTownWar())
                {
                    foreach (var wunitB1 in TeamBCUnits.Select(x => x.GetWorldUnit()).Where(x => x != null))
                    {
                        foreach (var wunitB2 in TeamBCUnits.Select(x => x.GetWorldUnit()).Where(x => x != null))
                        {
                            wunitB1.data.unitData.relationData.AddIntim(wunitB2.GetUnitId(), 50);
                            wunitB2.data.unitData.relationData.AddIntim(wunitB1.GetUnitId(), 50);
                        }
                    }
                }
                BattleModifyEvent.IsShowCustomMonstCount = false;
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

        public static TeamSideEnum GetTeamSide(UnitCtrlBase cunit)
        {
            if (cunit.IsPlayer())
                return PlayerSide;
            if (TeamACUnits.Any(x => x.data.createUnitSoleID == cunit.data.createUnitSoleID))
                return TeamSideEnum.TeamA;
            if (TeamBCUnits.Any(x => x.data.createUnitSoleID == cunit.data.createUnitSoleID))
                return TeamSideEnum.TeamB;
            else
                return TeamSideEnum.Unmanaged;
        }

        public static Vector2 GetTeamAPosi()
        {
            return new Vector2(ModBattleEvent.SceneBattle.battleMap.roomCenterPosi.x - 16,
                CommonTool.Random(ModBattleEvent.SceneBattle.battleMap.roomCenterPosi.y - 16, ModBattleEvent.SceneBattle.battleMap.roomCenterPosi.y + 16));
        }

        public static Vector2 GetTeamBPosi()
        {
            return new Vector2(ModBattleEvent.SceneBattle.battleMap.roomCenterPosi.x + 16,
                CommonTool.Random(ModBattleEvent.SceneBattle.battleMap.roomCenterPosi.y - 16, ModBattleEvent.SceneBattle.battleMap.roomCenterPosi.y + 16));
        }

        public static void InitUnitPosi(UnitCtrlBase cunit)
        {
            //set player posi
            var side = GetTeamSide(cunit);
            if (side == TeamSideEnum.TeamA)
                cunit.move.SetPosition(GetTeamAPosi());
            else if (side == TeamSideEnum.TeamB)
                cunit.move.SetPosition(GetTeamBPosi());
        }

        public static void InitUnitStatus(UnitCtrlBase cunit)
        {
            cunit.data.hp = cunit.data.maxHP.value;
            cunit.data.mp = cunit.data.maxHP.value;
            cunit.data.sp = cunit.data.maxHP.value;
        }

        public static void InitUnit(UnitCtrlBase cunit)
        {
            cunit.AddState(UnitStateType.NotDie, float.MaxValue);
            InitUnitPosi(cunit);
            InitUnitStatus(cunit);
        }

        public static void Revive(UnitCtrlBase cunit)
        {
            InitUnitPosi(cunit);
            InitUnitStatus(cunit);
        }

        public static void Pause(UnitCtrlBase cunit)
        {
            cunit.AddState(UnitStateType.NotDie, float.MaxValue);
            cunit.AddState(UnitStateType.NotMove, float.MaxValue);
            cunit.AddState(UnitStateType.LockHPAndEmit, float.MaxValue);
            cunit.AddState(UnitStateType.NotUseGodEyeSkill, float.MaxValue);
            cunit.AddState(UnitStateType.NotUserAllSkill, float.MaxValue);
            cunit.AddState(UnitStateType.NotUserFieldSkill, float.MaxValue);
            cunit.AddState(UnitStateType.NotUserImmortal, float.MaxValue);
            cunit.AddState(UnitStateType.NotUserMagicWeapon, float.MaxValue);
            cunit.AddState(UnitStateType.NotUserPill, float.MaxValue);
            cunit.AddState(UnitStateType.NotUserProp, float.MaxValue);
            cunit.AddState(UnitStateType.NotUserSkilll, float.MaxValue);
            cunit.AddState(UnitStateType.NotUserSkilllLeft, float.MaxValue);
            cunit.AddState(UnitStateType.NotUserSkilllRight, float.MaxValue);
            cunit.AddState(UnitStateType.NotUserStep, float.MaxValue);
            cunit.AddState(UnitStateType.NotUserUltimate, float.MaxValue);
        }

        public static void Die(UnitCtrlBase cunit)
        {
            DieCUnits.Add(cunit.data.createUnitSoleID);
            Pause(cunit);
            cunit.unitMono.gameObject.SetActive(false);
        }

        public static void BattleEnd(TeamSideEnum loseSide, TeamSideEnum winSide)
        {
            var loseSideUnits = GetTeamInfo<List<UnitCtrlBase>>(loseSide, TeamInfoEnum.CUnit);
            if (loseSideUnits.All(x => x.isDie || DieCUnits.Contains(x.data.createUnitSoleID)))
            {
                //pause all units
                foreach (var u in ModBattleEvent.SceneBattle.unit.allUnit)
                {
                    Pause(u);
                }
                //set game
                WinTeamSide = winSide;
                ModBattleEvent.SceneBattle.battleEnd.BattleEnd(PlayerSide == WinTeamSide);
            }
        }

        public static bool IsPlayerTeamWin()
        {
            return PlayerSide == WinTeamSide;
        }

        public static T GetTeamInfo<T>(TeamSideEnum side, TeamInfoEnum info)
        {
            if (side == TeamSideEnum.Unmanaged)
                return default;
            return TeamInfo[$"{side}_{info}"].Parse<T>();
        }

        public static void SetTeamInfo(TeamSideEnum side, TeamInfoEnum info, object value)
        {
            if (side == TeamSideEnum.Unmanaged)
                return;
            TeamInfo[$"{side}_{info}"] = value;
        }

        public static void AddTeamCount(TeamSideEnum side, int count)
        {
            SetTeamInfo(side, TeamInfoEnum.UnitCount, GetTeamInfo<int>(side, TeamInfoEnum.UnitCount) + count);
        }

        public static void InitTeamInfo(TeamSideEnum side, int unitCount)
        {
            SetTeamInfo(side, TeamInfoEnum.UnitCountMax, unitCount);
            SetTeamInfo(side, TeamInfoEnum.UnitCount, unitCount);
        }

        [ErrorIgnore]
        [EventCondition(IsInGame = HandleEnum.Ignore, IsInBattle = HandleEnum.True)]
        public override void OnTimeUpdate1s()
        {
            base.OnTimeUpdate1s();
            if (BattleModifyEvent.IsShowCustomMonstCount && (IsBattleTownWar() || IsBattleMonstWave()))
            {
                BattleModifyEvent.TextCustomMonstCount1.text = TeamAUnitCount.ToString();
                BattleModifyEvent.TextCustomMonstCount2.text = TeamBUnitCount.ToString();
            }
        }
    }
}
