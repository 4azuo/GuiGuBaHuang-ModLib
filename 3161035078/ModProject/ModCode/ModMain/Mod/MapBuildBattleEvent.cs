using MOD_nE7UL2.Const;
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
        public static MapBuildBattleEvent Instance { get; set; }

        public const float MONST_WAVE_RATE = 1f;
        public const float TOWN_WAR_RATE = 0.4f;

        public const int TOWN_WAR_DUNGEON_BASE_ID = 480110990;
        public const int TOWN_MONST_WAVE_DUNGEON_BASE_ID = 480110991;
        public const int SECT_MONST_WAVE_DUNGEON_BASE_ID = 480110992;

        public Dictionary<string, int> LastYearEventHappen { get; set; } = new Dictionary<string, int>();

        [JsonIgnore]
        public int TeamAUnitCount { get; set; }
        [JsonIgnore]
        public int TeamBUnitCount { get; set; }

        public static readonly int[] enemyInTraits = new int[] { UnitTraitEnum.Evil.Parse<int>() };
        public static readonly int[] enemyOutTraits = new int[] { UnitTraitEnum.Power_hungry.Parse<int>(), UnitTraitEnum.Glory_Hound.Parse<int>() };

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
            CalTownWarInfo(townA_def, townB_atk);
            g.world.battle.IntoBattleInit(townA_def.GetOrigiPoint(), g.conf.dungeonBase.GetItem(TOWN_WAR_DUNGEON_BASE_ID), 1, new WorldBattleData
            {
                isRealBattle = true,
                isSelfBattle = false,
                schoolID = null,
            });
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
            CalMonstWaveInfo(teamAbuildBase);
            g.world.battle.IntoBattleInit(g.world.playerUnit.GetUnitPos(), g.conf.dungeonBase.GetItem(dungeonBaseId), 1, new WorldBattleData
            {
                isRealBattle = true,
                isSelfBattle = false,
                schoolID = null,
            });
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

        public static void CalTownWarInfo(MapBuildBase teamAbuildBase, MapBuildBase teamBbuildBase)
        {
            Instance.TeamAUnitCount = 10 + Math.Sqrt(MapBuildPropertyEvent.GetBuildProperty(teamAbuildBase)).Parse<int>() / 10;
            Instance.TeamBUnitCount = 10 + Math.Sqrt(MapBuildPropertyEvent.GetBuildProperty(teamBbuildBase)).Parse<int>() / 10;
        }

        public static void CalMonstWaveInfo(MapBuildBase teamAbuildBase)
        {
            Instance.TeamAUnitCount = 10 + Math.Sqrt(MapBuildPropertyEvent.GetBuildProperty(teamAbuildBase)).Parse<int>() / 10;
            Instance.TeamBUnitCount = 100 + g.world.run.roundMonth + Instance.TeamAUnitCount + Math.Pow(2, teamAbuildBase.gridData.areaBaseID).Parse<int>();
        }

        public override void OnBattleStart(ETypeData e)
        {
            //base.OnBattleStart(e);
            //if (g.world.battle.data.dungeonBaseItem.id == TOWN_WAR_DUNGEON_BASE_ID ||
            //    g.world.battle.data.dungeonBaseItem.id == TOWN_MONST_WAVE_DUNGEON_BASE_ID ||
            //    g.world.battle.data.dungeonBaseItem.id == SECT_MONST_WAVE_DUNGEON_BASE_ID)
            //{
            //    ModBattleEvent.SceneBattle.battleEnd.
            //}
        }
    }
}
