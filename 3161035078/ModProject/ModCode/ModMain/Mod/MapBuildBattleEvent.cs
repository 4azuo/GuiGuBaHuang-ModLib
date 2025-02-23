using EBattleTypeData;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using ModLib.Enum;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MOD_nE7UL2.Object.GameStts;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.MAP_BUILD_BATTLE_EVENT)]
    public class MapBuildBattleEvent : ModEvent
    {
        public enum BattleTypeEnum
        {
            TownVsTown,
            TownVsMonster,
            SectVsMonster,
        }

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
        public static _MapBuildBattleConfigs Configs => ModMain.ModObj.GameSettings.MapBuildBattleConfigs;

        public const int JOIN_RANGE = 4;
        public const float MONST_WAVE_RATE = 0.5f;
        public const float TOWN_WAR_RATE = 0.2f;
        public const int MIN_UNIT = 4;
        public const int STP_UNIT = 1;

        public const int TOWN_WAR_DUNGEON_BASE_ID = 480110990;
        public const int TOWN_MONST_WAVE_DUNGEON_BASE_ID = 480110991;
        public const int SECT_MONST_WAVE_DUNGEON_BASE_ID = 480110992;

        public const int DRAMA_ID = 480110600;
        public const int DRAMA_HELP_DEF_OPT_ID = 480110601;
        public const int DRAMA_HELP_ATK_OPT_ID = 480110602;
        public const int DRAMA_TRY_ESCAPE_OPT_ID = 480110603;
        public const int DRAMA_DONT_CARE_OPT_ID = 480110604;
        public const int DRAMA_HELP_OPT_ID = 480110605;

        public const int HELP_REPUTATION = 1000;

        public static int JoinBattleFlg { get; set; } = -1;
        public static bool InitBattleFlg { get; set; } = false;
        public static BattleTypeEnum BattleType { get; set; }
        public static TeamSideEnum WinTeamSide { get; set; }
        public static TeamSideEnum PlayerSide { get; set; }
        public static MapBuildBase BuildBaseA { get; set; }
        public static MapBuildBase BuildBaseB { get; set; }
        public static int TeamAUnitCount => GetTeamInfo<int>(TeamSideEnum.TeamA, TeamInfoEnum.UnitCount);
        public static int TeamBUnitCount => GetTeamInfo<int>(TeamSideEnum.TeamB, TeamInfoEnum.UnitCount);
        public static List<WorldUnitBase> TeamAWUnits => GetTeamInfo<List<WorldUnitBase>>(TeamSideEnum.TeamA, TeamInfoEnum.WUnit);
        public static List<WorldUnitBase> TeamBWUnits => GetTeamInfo<List<WorldUnitBase>>(TeamSideEnum.TeamB, TeamInfoEnum.WUnit);
        public static List<UnitCtrlBase> TeamACUnits => GetTeamInfo<List<UnitCtrlBase>>(TeamSideEnum.TeamA, TeamInfoEnum.CUnit);
        public static List<UnitCtrlBase> TeamBCUnits => GetTeamInfo<List<UnitCtrlBase>>(TeamSideEnum.TeamB, TeamInfoEnum.CUnit);
        public static List<string> DieCUnits { get; } = new List<string>();
        public static Dictionary<string, object> TeamInfo { get; } = new Dictionary<string, object>()
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

        public Dictionary<string, int> LastYearEventHappen { get; set; } = new Dictionary<string, int>();

        public override void OnMonthly()
        {
            base.OnMonthly();
            var curYear = GameHelper.GetGameYear();

            foreach (var town in g.world.build.GetBuilds<MapBuildTown>())
            {
                if (!LastYearEventHappen.ContainsKey(town.buildData.id))
                    LastYearEventHappen.Add(town.buildData.id, curYear);

                if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, 100))//MONST_WAVE_RATE * (curYear - LastYearEventHappen[town.buildData.id])))
                {
                    DebugHelper.WriteLine($"{town.name} monster wave");
                    LastYearEventHappen[town.buildData.id] = curYear;
                    MonstWave(town);
                }
            }

            foreach (var school in g.world.build.GetBuilds<MapBuildSchool>())
            {
                if (!LastYearEventHappen.ContainsKey(school.buildData.id))
                    LastYearEventHappen.Add(school.buildData.id, curYear);

                if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, MONST_WAVE_RATE * (curYear - LastYearEventHappen[school.buildData.id])))
                {
                    DebugHelper.WriteLine($"{school.name} monster wave");
                    LastYearEventHappen[school.buildData.id] = curYear;
                    MonstWave(school);
                }
            }

            foreach (var town in g.world.build.GetBuilds<MapBuildTown>())
            {
                if (!LastYearEventHappen.ContainsKey(town.buildData.id))
                    LastYearEventHappen.Add(town.buildData.id, curYear);

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
                        if (townAtkMaster == null || townAtkMaster.IsPlayer())
                            return false;
                        return
                            (
                                townAtkMaster.data.unitData.relationData.GetIntim(master) <= -200 ||
                                enemyInTraits.Contains(townAtkMaster.data.unitData.propertyData.inTrait) ||
                                enemyOutTraits.Contains(townAtkMaster.data.unitData.propertyData.outTrait1) ||
                                enemyOutTraits.Contains(townAtkMaster.data.unitData.propertyData.outTrait2)
                            ) &&
                            CalWUnitBattlePower(MapBuildPropertyEvent.GetTownGuardians(townAtk)) > CalWUnitBattlePower(g.world.unit.GetUnitExact(town.GetOrigiPoint(), JOIN_RANGE, true, false).ToList()) &&
                            Math.Abs(townAtk.gridData.areaBaseID - town.gridData.areaBaseID) < 3;
                    });
                    if (hasAttacker && townAtk != null)
                    {
                        DebugHelper.WriteLine($"{town.name}/{townAtk.name} war");
                        LastYearEventHappen[town.buildData.id] = curYear;
                        LastYearEventHappen[townAtk.buildData.id] = curYear;
                        TownWar(town, townAtk, false);
                    }
                }
            }
        }

        private static void Hated(MapBuildSchool school)
        {
            g.world.unit.GetUnitsInArea(g.world.playerUnit.data.unitData.pointGridData.areaBaseID).ToArray()
                .Where(y =>
                {
                    return !y.IsPlayer() && y.data.school?.schoolNameID == school.schoolNameID;
                }).ToList()
                .ForEach(y =>
                {
                    if (!y.IsPlayer())
                        y.data.unitData.relationData.AddHate(g.world.playerUnit.GetUnitId(), 100);
                });
        }

        private static void Loved(MapBuildSchool school)
        {
            if (school.schoolNameID == g.world.playerUnit.data.unitData.schoolID)
            {
                g.world.unit.GetUnitsInArea(g.world.playerUnit.data.unitData.pointGridData.areaBaseID).ToArray()
                    .Where(y =>
                    {
                        return !y.IsPlayer() && y.data.school?.schoolNameID == school.schoolNameID;
                    }).ToList()
                    .ForEach(y =>
                    {
                        if (!y.IsPlayer())
                            y.data.unitData.relationData.AddIntim(g.world.playerUnit.GetUnitId(), 50);
                    });
            }
        }

        private static void Hated(MapBuildTown town)
        {
            MapBuildPropertyEvent.GetTownGuardians(town).ForEach(y =>
            {
                if (!y.IsPlayer())
                    y.data.unitData.relationData.AddHate(g.world.playerUnit.GetUnitId(), 100);
            });
        }

        private static void Loved(MapBuildTown town)
        {
            MapBuildPropertyEvent.GetTownGuardians(town).ForEach(y =>
            {
                if (!y.IsPlayer())
                    y.data.unitData.relationData.AddIntim(g.world.playerUnit.GetUnitId(), 50);
            });
        }

        public static void TownWar(MapBuildTown townA_def, MapBuildTown townB_atk, bool proactive)
        {
            if (JoinBattleFlg == GameHelper.GetGameTotalMonth())
            {
                SkipTownWar(townA_def, townB_atk);
            }
            else
            if (proactive)
            {
                JoinTownWar(townA_def, townB_atk, TeamSideEnum.TeamB);
            }
            else
            if (townA_def.GetOpenBuildPoints().ToList().Any(x => x == g.world.playerUnit.GetUnitPos()))
            {
                DramaTool.OpenDrama(DRAMA_ID, new DramaData
                {
                    dialogueText = { [DRAMA_ID] = GameTool.LS("battleevent480112890") },
                    hideDialogueOptions = new int[] { DRAMA_DONT_CARE_OPT_ID, DRAMA_HELP_OPT_ID }.ToIl2CppList(),
                    onOptionsClickCall = (Il2CppSystem.Action<ConfDramaOptionsItem>)((x) =>
                    {
                        switch (x.id)
                        {
                            case DRAMA_HELP_DEF_OPT_ID:
                                Loved(townA_def);
                                Hated(townB_atk);
                                JoinTownWar(townA_def, townB_atk, TeamSideEnum.TeamA);
                                break;
                            case DRAMA_HELP_ATK_OPT_ID:
                                Loved(townB_atk);
                                Hated(townA_def);
                                JoinTownWar(townA_def, townB_atk, TeamSideEnum.TeamB);
                                break;
                            case DRAMA_TRY_ESCAPE_OPT_ID:
                                if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, Configs.EscapeChance))
                                {
                                    g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.Reputation, g.world.playerUnit.GetProperty<int>(UnitPropertyEnum.Reputation) / 2);
                                    g.world.playerUnit.SetUnitRandomPos(g.world.playerUnit.GetUnitPos(), 4);
                                    if (MapBuildPropertyEvent.IsTownGuardian(townA_def, g.world.playerUnit))
                                    {
                                        g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.Reputation, g.world.playerUnit.GetProperty<int>(UnitPropertyEnum.Reputation) / 5);
                                        Hated(townA_def);
                                    }
                                    else if (MapBuildPropertyEvent.IsTownGuardian(townB_atk, g.world.playerUnit))
                                    {
                                        g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.Reputation, g.world.playerUnit.GetProperty<int>(UnitPropertyEnum.Reputation) / 5);
                                        Hated(townB_atk);
                                    }
                                }
                                else
                                {
                                    JoinTownWar(townA_def, townB_atk, TeamSideEnum.TeamA);
                                    g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.Reputation, g.world.playerUnit.GetProperty<int>(UnitPropertyEnum.Reputation) / 3);
                                }
                                break;
                        }
                    })
                });
            }
            else
            if (townA_def.gridData.areaBaseID == g.world.playerUnit.data.unitData.pointGridData.areaBaseID &&
                townA_def.GetOrigiPoint().CalRange(g.world.playerUnit.data.unitData.GetPoint()) < 8)
            {
                DramaTool.OpenDrama(DRAMA_ID, new DramaData
                {
                    dialogueText = { [DRAMA_ID] = string.Format(GameTool.LS("battleevent480112894"), townB_atk.name, townA_def.name) },
                    hideDialogueOptions = new int[] { DRAMA_DONT_CARE_OPT_ID, DRAMA_TRY_ESCAPE_OPT_ID }.ToIl2CppList(),
                    onOptionsClickCall = (Il2CppSystem.Action<ConfDramaOptionsItem>)((x) =>
                    {
                        switch (x.id)
                        {
                            case DRAMA_HELP_DEF_OPT_ID:
                                Loved(townA_def);
                                Hated(townB_atk);
                                JoinTownWar(townA_def, townB_atk, TeamSideEnum.TeamA);
                                break;
                            case DRAMA_HELP_ATK_OPT_ID:
                                Loved(townB_atk);
                                Hated(townA_def);
                                JoinTownWar(townA_def, townB_atk, TeamSideEnum.TeamB);
                                break;
                            case DRAMA_DONT_CARE_OPT_ID:
                                if (MapBuildPropertyEvent.IsTownGuardian(townA_def, g.world.playerUnit))
                                    Hated(townA_def);
                                else if (MapBuildPropertyEvent.IsTownGuardian(townB_atk, g.world.playerUnit))
                                    Hated(townB_atk);
                                break;
                        }
                    })
                });
            }
            else
            {
                SkipTownWar(townA_def, townB_atk);
            }
        }

        public static void JoinTownWar(MapBuildTown townA_def, MapBuildTown townB_atk, TeamSideEnum side)
        {
            DebugHelper.WriteLine($"Join ({side}): {townA_def.name} vs {townB_atk.name}");
            JoinBattleFlg = GameHelper.GetGameTotalMonth();
            //init
            InitBattle(BattleTypeEnum.TownVsTown);
            //player side
            PlayerSide = side;
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
            DebugHelper.WriteLine($"Skip: {townA_def.name} vs {townB_atk.name}");
            //init
            InitBattle(BattleTypeEnum.TownVsTown);
            //player side
            PlayerSide = TeamSideEnum.Unmanaged;
            //battle info
            BuildBaseA = townA_def;
            BuildBaseB = townB_atk;
            CalTownWarInfo(townA_def, townB_atk);
            //auto battle
            var teamAPoint = CalWUnitBattlePower(TeamAWUnits);
            var teamATotalPoint = teamAPoint * TeamAUnitCount * /*def point*/1.5;
            var teamBPoint = CalWUnitBattlePower(TeamBWUnits);
            var teamBTotalPoint = teamBPoint * TeamBUnitCount;
            //battle end
            var ratioAB = teamATotalPoint / teamBTotalPoint;
            var ratioBA = teamBTotalPoint / teamATotalPoint;
            if (teamATotalPoint >= teamBTotalPoint)
            {
                //A win
                DebugHelper.WriteLine($"Team A win!");
                WinTeamSide = TeamSideEnum.TeamA;
                //A damaged
                if (ratioAB < 2)
                {
                    //building damaged
                    BuildingArrangeEvent.Destroy(townA_def, BuildingCostEnum.GetAllEnums<BuildingCostEnum>().Where(x => BuildingArrangeEvent.IsBuilt(townA_def, x)).ToArray().Random());
                }
                MapBuildPropertyEvent.AddBuildProperty(townA_def, -(MapBuildPropertyEvent.GetBuildProperty(townA_def) * 0.3).Parse<long>());
                //B damaged
                MapBuildPropertyEvent.AddBuildProperty(townB_atk, -(MapBuildPropertyEvent.GetBuildProperty(townB_atk) * 0.2).Parse<long>());
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
                DebugHelper.WriteLine($"Team B win!");
                WinTeamSide = TeamSideEnum.TeamB;
                //A damaged x2
                BuildingArrangeEvent.Destroy(townA_def, BuildingCostEnum.GetAllEnums<BuildingCostEnum>().Where(x => BuildingArrangeEvent.IsBuilt(townA_def, x)).ToArray().Random());
                BuildingArrangeEvent.Destroy(townA_def, BuildingCostEnum.GetAllEnums<BuildingCostEnum>().Where(x => BuildingArrangeEvent.IsBuilt(townA_def, x)).ToArray().Random());
                var damagedBudget = MapBuildPropertyEvent.GetBuildProperty(townA_def);
                MapBuildPropertyEvent.AddBuildProperty(townA_def, -damagedBudget);
                //B damaged
                MapBuildPropertyEvent.AddBuildProperty(townB_atk, -(MapBuildPropertyEvent.GetBuildProperty(townB_atk) * 0.1).Parse<long>());
                //B pillage
                var tmp = damagedBudget / 2;
                var give_for_joint = (tmp / TeamBWUnits.Count).FixValue(0, ModConst.MAX_VALUE).Parse<int>();
                var pillagedBudget = damagedBudget - (give_for_joint * TeamBWUnits.Count);
                MapBuildPropertyEvent.AddBuildProperty(BuildBaseB, pillagedBudget);
                foreach (var wunit in TeamBWUnits)
                {
                    wunit.AddUnitMoney(give_for_joint);
                }
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
            if (JoinBattleFlg == GameHelper.GetGameTotalMonth())
            {
                SkipMonstWave(town);
            }
            else
            if (town.GetOpenBuildPoints().ToList().Any(x => x == g.world.playerUnit.GetUnitPos()))
            {
                DramaTool.OpenDrama(DRAMA_ID, new DramaData
                {
                    dialogueText = { [DRAMA_ID] = GameTool.LS("battleevent480112890") },
                    hideDialogueOptions = new int[] { DRAMA_HELP_DEF_OPT_ID, DRAMA_HELP_ATK_OPT_ID, DRAMA_DONT_CARE_OPT_ID }.ToIl2CppList(),
                    onOptionsClickCall = (Il2CppSystem.Action<ConfDramaOptionsItem>)((x) =>
                    {
                        switch (x.id)
                        {
                            case DRAMA_HELP_OPT_ID:
                                Loved(town);
                                g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.Reputation, HELP_REPUTATION);
                                JoinMonstWave(town, TOWN_MONST_WAVE_DUNGEON_BASE_ID);
                                break;
                            case DRAMA_TRY_ESCAPE_OPT_ID:
                                if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, Configs.EscapeChance))
                                {
                                    g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.Reputation, g.world.playerUnit.GetProperty<int>(UnitPropertyEnum.Reputation) / 2);
                                    g.world.playerUnit.SetUnitRandomPos(g.world.playerUnit.GetUnitPos(), 4);
                                    Hated(town);
                                }
                                else
                                {
                                    g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.Reputation, g.world.playerUnit.GetProperty<int>(UnitPropertyEnum.Reputation) / 5);
                                    JoinMonstWave(town, TOWN_MONST_WAVE_DUNGEON_BASE_ID);
                                }
                                break;
                        }
                    })
                });
            }
            else
            if (town.gridData.areaBaseID == g.world.playerUnit.data.unitData.pointGridData.areaBaseID &&
                town.GetOrigiPoint().CalRange(g.world.playerUnit.data.unitData.GetPoint()) < 8)
            {
                DramaTool.OpenDrama(DRAMA_ID, new DramaData
                {
                    dialogueText = { [DRAMA_ID] = string.Format(GameTool.LS("battleevent480112895"), town.name) },
                    hideDialogueOptions = new int[] { DRAMA_HELP_DEF_OPT_ID, DRAMA_HELP_ATK_OPT_ID, DRAMA_TRY_ESCAPE_OPT_ID }.ToIl2CppList(),
                    onOptionsClickCall = (Il2CppSystem.Action<ConfDramaOptionsItem>)((x) =>
                    {
                        switch (x.id)
                        {
                            case DRAMA_HELP_OPT_ID:
                                Loved(town);
                                g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.Reputation, HELP_REPUTATION);
                                JoinMonstWave(town, TOWN_MONST_WAVE_DUNGEON_BASE_ID);
                                break;
                            case DRAMA_DONT_CARE_OPT_ID:
                                if (MapBuildPropertyEvent.IsTownGuardian(town, g.world.playerUnit))
                                    Hated(town);
                                break;
                        }
                    })
                });
            }
            else
            {
                SkipMonstWave(town);
            }
        }

        public static void MonstWave(MapBuildSchool school)
        {
            if (JoinBattleFlg == GameHelper.GetGameTotalMonth())
            {
                SkipMonstWave(school);
            }
            else
            if (school.GetOpenBuildPoints().ToList().Any(x => x == g.world.playerUnit.GetUnitPos()))
            {
                DramaTool.OpenDrama(DRAMA_ID, new DramaData
                {
                    dialogueText = { [DRAMA_ID] = GameTool.LS("battleevent480112890") },
                    hideDialogueOptions = new int[] { DRAMA_HELP_DEF_OPT_ID, DRAMA_HELP_ATK_OPT_ID, DRAMA_DONT_CARE_OPT_ID }.ToIl2CppList(),
                    onOptionsClickCall = (Il2CppSystem.Action<ConfDramaOptionsItem>)((x) =>
                    {
                        switch (x.id)
                        {
                            case DRAMA_HELP_OPT_ID:
                                Loved(school);
                                g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.Reputation, HELP_REPUTATION);
                                JoinMonstWave(school, SECT_MONST_WAVE_DUNGEON_BASE_ID);
                                break;
                            case DRAMA_TRY_ESCAPE_OPT_ID:
                                if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, Configs.EscapeChance))
                                {
                                    g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.Reputation, g.world.playerUnit.GetProperty<int>(UnitPropertyEnum.Reputation) / 2);
                                    g.world.playerUnit.SetUnitRandomPos(g.world.playerUnit.GetUnitPos(), 4);
                                    if (school.schoolNameID == g.world.playerUnit.data.unitData.schoolID)
                                        Hated(school);
                                }
                                else
                                {
                                    g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.Reputation, g.world.playerUnit.GetProperty<int>(UnitPropertyEnum.Reputation) / 5);
                                    JoinMonstWave(school, SECT_MONST_WAVE_DUNGEON_BASE_ID);
                                }
                                break;
                        }
                    })
                });
            }
            else
            if (school.gridData.areaBaseID == g.world.playerUnit.data.unitData.pointGridData.areaBaseID &&
                school.GetOrigiPoint().CalRange(g.world.playerUnit.data.unitData.GetPoint()) < 8)
            {
                DramaTool.OpenDrama(DRAMA_ID, new DramaData
                {
                    dialogueText = { [DRAMA_ID] = string.Format(GameTool.LS("battleevent480112895"), g.world.playerUnit.data.school.GetName(true)) },
                    hideDialogueOptions = new int[] { DRAMA_HELP_DEF_OPT_ID, DRAMA_HELP_ATK_OPT_ID, DRAMA_TRY_ESCAPE_OPT_ID }.ToIl2CppList(),
                    onOptionsClickCall = (Il2CppSystem.Action<ConfDramaOptionsItem>)((x) =>
                    {
                        switch (x.id)
                        {
                            case DRAMA_HELP_OPT_ID:
                                Loved(school);
                                g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.Reputation, HELP_REPUTATION);
                                JoinMonstWave(school, SECT_MONST_WAVE_DUNGEON_BASE_ID);
                                break;
                            case DRAMA_DONT_CARE_OPT_ID:
                                if (school.schoolNameID == g.world.playerUnit.data.unitData.schoolID)
                                    Hated(school);
                                break;
                        }
                    })
                });
            }
            else
            {
                SkipMonstWave(school);
            }
        }

        public static void JoinMonstWave(MapBuildBase teamAbuildBase, int dungeonBaseId)
        {
            DebugHelper.WriteLine($"Join ({dungeonBaseId}): {teamAbuildBase.name}");
            JoinBattleFlg = GameHelper.GetGameTotalMonth();
            //init
            InitBattle(teamAbuildBase.IsTown() ? BattleTypeEnum.TownVsMonster : BattleTypeEnum.SectVsMonster);
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
            DebugHelper.WriteLine($"Join: {teamAbuildBase.name}");
            //init
            InitBattle(teamAbuildBase.IsTown() ? BattleTypeEnum.TownVsMonster : BattleTypeEnum.SectVsMonster);
            //player side
            PlayerSide = TeamSideEnum.TeamA;
            //battle info
            BuildBaseA = teamAbuildBase;
            BuildBaseB = null;
            CalMonstWaveInfo(teamAbuildBase);
            //auto battle
            var teamAPoint = CalWUnitBattlePower(TeamAWUnits);
            var teamATotalPoint = teamAPoint * TeamAUnitCount * /*def point*/1.5;
            var gameLvl = g.data.dataWorld.data.gameLevel.Parse<int>();
            var areaId = teamAbuildBase.gridData.areaBaseID;
            var teamBPoint = CalMonstBattlePower(gameLvl, areaId);
            var teamBTotalPoint = teamBPoint * TeamBUnitCount;
            //battle end
            var ratioAB = teamATotalPoint / teamBTotalPoint;
            var ratioBA = teamBTotalPoint / teamATotalPoint;
            if (teamATotalPoint >= teamBTotalPoint)
            {
                //A win
                DebugHelper.WriteLine($"Team A win!");
                WinTeamSide = TeamSideEnum.TeamA;
                //A damaged
                MapBuildPropertyEvent.AddBuildProperty(teamAbuildBase, -(MapBuildPropertyEvent.GetBuildProperty(teamAbuildBase) * 0.2).Parse<long>());
            }
            else
            {
                //B win
                DebugHelper.WriteLine($"Monsters win!");
                WinTeamSide = TeamSideEnum.TeamB;
                //A damaged
                if (IsBattleMonstWaveOnTown())
                {
                    foreach (var e in BuildingCostEnum.GetAllEnums<BuildingCostEnum>())
                    {
                        if (BuildingArrangeEvent.IsBuilt(teamAbuildBase, e))
                            BuildingArrangeEvent.Destroy(teamAbuildBase, e);
                    }
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
            var areaId = townA_def.gridData.areaBaseID;

            //guardians + others
            TeamAWUnits.AddRange(g.world.unit.GetUnitExact(townA_def.GetOrigiPoint(), JOIN_RANGE, true, false).ToArray().Take(MIN_UNIT + STP_UNIT * areaId));
            TeamAWUnits.AddRange(MapBuildPropertyEvent.GetTownGuardians(townA_def).Where(x => !x.IsPlayer()));
            //guardians
            TeamBWUnits.AddRange(MapBuildPropertyEvent.GetTownGuardians(townB_atk).Where(x => !x.IsPlayer()));

            var defCityR = townA_def.IsCity() ? 2 : 1;
            var atkCityR = townB_atk.IsCity() ? 2 : 1;
            InitTeamInfo(TeamSideEnum.TeamA, 8 + (((TeamAWUnits.Count * defCityR) + (MapBuildPropertyEvent.GetBuildProperty(townA_def) / 1000000)) * 1.5).Parse<int>());
            InitTeamInfo(TeamSideEnum.TeamB, 8 + (TeamBWUnits.Count * atkCityR) + (MapBuildPropertyEvent.GetBuildProperty(townB_atk) / 1000000).Parse<int>());

            DebugHelper.WriteLine($"{TeamAUnitCount} vs {TeamBUnitCount}");
        }

        public static void CalMonstWaveInfo(MapBuildBase baseA_def)
        {
            var gameLvl = g.data.dataWorld.data.gameLevel.Parse<int>();
            var areaId = baseA_def.gridData.areaBaseID;

            //guardians + others
            TeamAWUnits.AddRange(g.world.unit.GetUnitExact(baseA_def.GetOrigiPoint(), JOIN_RANGE, true, false).ToArray().Take(MIN_UNIT + STP_UNIT * areaId));
            TeamAWUnits.AddRange(MapBuildPropertyEvent.GetTownGuardians(baseA_def.TryCast<MapBuildTown>()).Where(x => !x.IsPlayer()));
            //monsters
            TeamBWUnits.Clear();

            var cityR = baseA_def.IsCity() ? 2 : 1;
            InitTeamInfo(TeamSideEnum.TeamA, 8 + (((TeamAWUnits.Count * cityR) + (MapBuildPropertyEvent.GetBuildProperty(baseA_def) / 1000000)) * 1.5).Parse<int>());
            InitTeamInfo(TeamSideEnum.TeamB, ((g.world.run.roundMonth + TeamAUnitCount * gameLvl + Math.Pow(2, areaId).Parse<int>()) * CommonTool.Random(0.8f, 1.1f)).Parse<int>());

            DebugHelper.WriteLine($"{TeamAUnitCount} vs {TeamBUnitCount}");
        }

        public static void InitBattle(BattleTypeEnum battleType)
        {
            BattleType = battleType;
            InitBattleFlg = false;
            TeamAWUnits.Clear();
            TeamBWUnits.Clear();
            TeamACUnits.Clear();
            TeamBCUnits.Clear();
            DieCUnits.Clear();
            WinTeamSide = TeamSideEnum.Unmanaged;
        }

        public override void OnBattleStart(ETypeData e)
        {
            base.OnBattleStart(e);
            if (ModBattleEvent.SceneBattle != null && (IsBattleTownWar() || IsBattleMonstWave()) && !InitBattleFlg)
            {
                InitBattleFlg = true;
                //init
                InitUnit(ModBattleEvent.PlayerUnit);
                //create team units
                if (IsBattleTownWar())
                {
                    //team A
                    var utA = PlayerSide == TeamSideEnum.TeamA ? UnitType.PlayerNPC : UnitType.Monst;
                    TeamACUnits.AddRange(TeamAWUnits.Select(x => ModBattleEvent.SceneBattle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(x.data, utA)).Cast<UnitCtrlBase>());
                    TeamACUnits.ForEach(x => InitUnit(x));
                    //team B
                    var utB = PlayerSide == TeamSideEnum.TeamB ? UnitType.PlayerNPC : UnitType.Monst;
                    TeamBCUnits.AddRange(TeamBWUnits.Select(x => ModBattleEvent.SceneBattle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(x.data, utB)).Cast<UnitCtrlBase>());
                    TeamBCUnits.ForEach(x => InitUnit(x));
                }
                else if (IsBattleMonstWave())
                {
                    //team A
                    TeamACUnits.AddRange(TeamAWUnits.Select(x => ModBattleEvent.SceneBattle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(x.data, UnitType.PlayerNPC)).Cast<UnitCtrlBase>());
                    TeamACUnits.ForEach(x => InitUnit(x));
                    //team B
                    try
                    {
                        var areaId = g.world.playerUnit.data.unitData.pointGridData.areaBaseID;
                        for (int i = 0; i < (MIN_UNIT + STP_UNIT * areaId) * 2; i++)
                        {
                            var monstLvl = CommonTool.Random(areaId - 1, areaId + 1).FixValue(0, monstList.Length - 1);
                            var cunit = ModBattleEvent.SceneBattle.unit.CreateUnitMonstNotAddList(monstList[monstLvl], Vector2.zero, UnitType.Monst, 4 * areaId + monstLvl);
                            TeamBCUnits.Add(cunit);
                            InitUnitPosi(cunit);
                        }
                    }
                    catch (Exception ex)
                    {
                        DebugHelper.WriteLine(ex);
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
                    var cunit = ModBattleEvent.SceneBattle.unit.CreateUnitMonstNotAddList(monstList[monstLvl], Vector2.zero, UnitType.Monst, 4 * areaId + monstLvl);
                    TeamBCUnits.Add(cunit);
                    InitUnitPosi(cunit);
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
                    DebugHelper.WriteLine($"Team A win!");
                    //A damaged
                    if (IsBattleTownWar() || IsBattleMonstWaveOnTown())
                    {
                        BuildingArrangeEvent.Destroy(BuildBaseA, BuildingCostEnum.GetAllEnums<BuildingCostEnum>().Where(x => BuildingArrangeEvent.IsBuilt(BuildBaseA, x)).ToArray().Random());
                    }
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
                    DebugHelper.WriteLine($"Team B win!");
                    //A damaged x2
                    if (IsBattleTownWar() || IsBattleMonstWaveOnTown())
                    {
                        BuildingArrangeEvent.Destroy(BuildBaseA, BuildingCostEnum.GetAllEnums<BuildingCostEnum>().Where(x => BuildingArrangeEvent.IsBuilt(BuildBaseA, x)).ToArray().Random());
                        BuildingArrangeEvent.Destroy(BuildBaseA, BuildingCostEnum.GetAllEnums<BuildingCostEnum>().Where(x => BuildingArrangeEvent.IsBuilt(BuildBaseA, x)).ToArray().Random());
                    }
                    var damagedBudget = MapBuildPropertyEvent.GetBuildProperty(BuildBaseA);
                    MapBuildPropertyEvent.AddBuildProperty(BuildBaseA, -damagedBudget);
                    if (IsBattleTownWar())
                    {
                        //B damaged
                        var lossrateB = 1f - (GetTeamInfo<float>(TeamSideEnum.TeamB, TeamInfoEnum.UnitCount) / GetTeamInfo<float>(TeamSideEnum.TeamB, TeamInfoEnum.UnitCountMax));
                        MapBuildPropertyEvent.AddBuildProperty(BuildBaseB, -(MapBuildPropertyEvent.GetBuildProperty(BuildBaseB) * lossrateB).Parse<long>());
                        //B pillage
                        var tmp = damagedBudget / 2;
                        var give_for_joint = (tmp / TeamBWUnits.Count).FixValue(0, ModConst.MAX_VALUE).Parse<int>();
                        var pillagedBudget = damagedBudget - (give_for_joint * TeamBWUnits.Count);
                        MapBuildPropertyEvent.AddBuildProperty(BuildBaseB, pillagedBudget);
                        foreach (var wunit in TeamBWUnits)
                        {
                            wunit.AddUnitMoney(give_for_joint);
                        }
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

        public static double CalMonstBattlePower(int gameLvl, int areaId)
        {
            return Math.Pow(3, areaId) * 10000 + Math.Pow(3, gameLvl) * 3000;
        }

        public static double CalWUnitBattlePower(List<WorldUnitBase> wunits)
        {
            if (wunits == null || wunits.Count == 0)
                return 0;
            return wunits.Average(x =>
            {
                return
                    (x.GetDynProperty(UnitDynPropertyEnum.Attack).value * 3) +
                    (x.GetDynProperty(UnitDynPropertyEnum.Defense).value * 10) +
                    (x.GetDynProperty(UnitDynPropertyEnum.HpMax).value) +
                    (x.GetDynProperty(UnitDynPropertyEnum.MpMax).value * 3) +
                    (x.GetDynProperty(UnitDynPropertyEnum.SpMax).value * 5) +
                    (x.GetEquippedArtifacts().Select(a => Math.Pow(2, a.propsInfoBase.grade) * 10000 + Math.Pow(2, a.propsInfoBase.level) * 2000 + CustomRefineEvent.GetRefineLvl(a) * 100).Sum());
            });
        }

        public static bool IsBattleTownWar()
        {
            return g.world.battle?.data?.dungeonBaseItem?.id == TOWN_WAR_DUNGEON_BASE_ID || BattleType == BattleTypeEnum.TownVsTown;
        }

        public static bool IsBattleMonstWave()
        {
            return IsBattleMonstWaveOnTown() || IsBattleMonstWaveOnSect();
        }

        public static bool IsBattleMonstWaveOnTown()
        {
            return g.world.battle?.data?.dungeonBaseItem?.id == TOWN_MONST_WAVE_DUNGEON_BASE_ID || BattleType == BattleTypeEnum.TownVsMonster;
        }

        public static bool IsBattleMonstWaveOnSect()
        {
            return g.world.battle?.data?.dungeonBaseItem?.id == SECT_MONST_WAVE_DUNGEON_BASE_ID || BattleType == BattleTypeEnum.SectVsMonster;
        }

        public static TeamSideEnum GetTeamSide(UnitCtrlBase cunit)
        {
            if (cunit.IsPlayer())
                return PlayerSide;
            if (cunit.IsMonster())
                return TeamSideEnum.TeamB;
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

        public static void BattleEndForce(TeamSideEnum loseSide, TeamSideEnum winSide)
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

                if (TeamBUnitCount == 0)
                {
                    BattleEndForce(TeamSideEnum.TeamB, TeamSideEnum.TeamA);
                }
                else
                if (TeamAUnitCount == 0)
                {
                    BattleEndForce(TeamSideEnum.TeamA, TeamSideEnum.TeamB);
                }
            }
        }
    }
}
