using EBattleTypeData;
using MOD_nE7UL2.Const;
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
                        TownWar(town, townAtk);
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

        public static void TownWar(MapBuildTown townA_def, MapBuildTown townB_atk)
        {
            if (townA_def.GetOpenBuildPoints().ToList().All(x => x != g.world.playerUnit.GetUnitPos()))
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
            else if (townA_def.GetOpenBuildPoints().ToList().All(x => x != g.world.playerUnit.GetUnitPos()))
                playerSide = 1;
            else if (townB_atk.GetOpenBuildPoints().ToList().All(x => x != g.world.playerUnit.GetUnitPos()))
                playerSide = 2;
            else
                playerSide = 1;
            //battle info
            CalTownWarInfo(townA_def, townB_atk);
            //battle into
            g.world.battle.IntoBattleInit(townA_def.GetOrigiPoint(), g.conf.dungeonBase.GetItem(TOWN_WAR_DUNGEON_BASE_ID), 1, new WorldBattleData
            {
                isRealBattle = true,
                isSelfBattle = false,
                schoolID = null,
            });
            //unallow flee
            BattleModifyEvent.HideFleeBattle = true;
        }

        public static void SkipTownWar(MapBuildTown townA_def, MapBuildTown townB_atk)
        {
            CalTownWarInfo(townA_def, townB_atk);
        }

        public static void MonstWave(MapBuildTown town)
        {
            if (town.GetOpenBuildPoints().ToList().All(x => x != g.world.playerUnit.GetUnitPos()))
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
            if (school.GetOpenBuildPoints().ToList().All(x => x != g.world.playerUnit.GetUnitPos()))
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
            g.world.battle.IntoBattleInit(g.world.playerUnit.GetUnitPos(), g.conf.dungeonBase.GetItem(dungeonBaseId), 1, new WorldBattleData
            {
                isRealBattle = true,
                isSelfBattle = false,
                schoolID = null,
            });
            //unallow flee
            BattleModifyEvent.HideFleeBattle = true;
        }

        public static void SkipMonstWave(MapBuildBase teamAbuildBase)
        {
            CalMonstWaveInfo(teamAbuildBase);
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
            teamAUnitCount = 10 + Math.Sqrt(MapBuildPropertyEvent.GetBuildProperty(townA_def)).Parse<int>() / 10;
            teamAWUnits = g.world.unit.GetUnitExact(townA_def.GetOrigiPoint(), JOIN_RANGE, false, false).ToList();
            teamBUnitCount = 10 + Math.Sqrt(MapBuildPropertyEvent.GetBuildProperty(townB_atk)).Parse<int>() / 10;
            teamBWUnits = MapBuildPropertyEvent.GetTownGuardians(townB_atk);
        }

        public static void CalMonstWaveInfo(MapBuildBase baseA_def)
        {
            var gameLvl = g.data.dataWorld.data.gameLevel.Parse<int>();
            teamAUnitCount = 10 + Math.Sqrt(MapBuildPropertyEvent.GetBuildProperty(baseA_def)).Parse<int>() / 10;
            teamAWUnits = g.world.unit.GetUnitExact(baseA_def.GetOrigiPoint(), JOIN_RANGE, false, false).ToList();
            teamBUnitCount = 100 + g.world.run.roundMonth + teamAUnitCount * gameLvl + Math.Pow(2, baseA_def.gridData.areaBaseID).Parse<int>();
            teamBWUnits = null;
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
                    teamACUnits = teamAWUnits.Select(x => ModBattleEvent.SceneBattle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(x.data, utA)).Cast<UnitCtrlBase>().ToList();
                    teamBCUnits = teamAWUnits.Select(x => ModBattleEvent.SceneBattle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(x.data, utB)).Cast<UnitCtrlBase>().ToList();
                }
                else if (IsBattleMonstWave())
                {
                    teamACUnits = teamAWUnits.Select(x => ModBattleEvent.SceneBattle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(x.data, UnitType.PlayerNPC)).Cast<UnitCtrlBase>().ToList();
                }
            }
        }

        public override void OnBattleUnitDie(UnitDie e)
        {
            base.OnBattleUnitDie(e);
            if (ModBattleEvent.SceneBattle != null && IsBattleTownWar() && e.unit.IsHuman())
            {
                var side = GetBattleSide(e.unit);
                var count = side == 1 ? teamAUnitCount : teamBUnitCount;
                if (count > 0)
                {
                    e.unit.Resurge(3, (Il2CppSystem.Action)(() =>
                    {
                        if (side == 1)
                            teamAUnitCount--;
                        else
                            teamBUnitCount--;
                    }));
                }
            }
        }

        public override void OnBattleEndOnce(BattleEnd e)
        {
            base.OnBattleEndOnce(e);
            if (ModBattleEvent.SceneBattle != null && (IsBattleTownWar() || IsBattleMonstWave()))
            {
                BattleModifyEvent.HideFleeBattle = false;
                teamAWUnits = null;
                teamBWUnits = null;
            }
        }

        [ErrorIgnore]
        [EventCondition(IsInGame = HandleEnum.Ignore, IsInBattle = HandleEnum.True)]
        public override void OnTimeUpdate1s()
        {
            base.OnTimeUpdate1s();
            var areaId = g.world.playerUnit.data.unitData.pointGridData.areaBaseID;
            if (ModBattleEvent.SceneBattle != null && IsBattleMonstWave() && teamBUnitCount > 0 &&
                ModBattleEvent.BattleMonsters.Count < (MIN_MONST + STP_MONST * areaId))
            {
                var monstLvl = CommonTool.Random(areaId - 1, areaId + 1).FixValue(0, monstList.Length - 1);
                var cunit = ModBattleEvent.SceneBattle.unit.CreateUnitMonstNotAddList(monstList[monstLvl], Vector2.zero, UnitType.Monst);
                teamBUnitCount--;
            }
        }
    }
}
