using MOD_nE7UL2.Const;
using ModLib.Mod;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.MAP_BUILD_BATTLE_EVENT)]
    public class MapBuildBattleEvent : ModEvent
    {
        public const float MONST_WAVE_RATE = 1f;
        public const float TOWN_WAR_RATE = 0.4f;

        public const int TOWN_WAR_DUNGEON_BASE_ID = 480110990;
        public const int TOWN_MONST_WAVE_DUNGEON_BASE_ID = 480110991;
        public const int SECT_MONST_WAVE_DUNGEON_BASE_ID = 480110992;

        public Dictionary<string, int> LastYearEventHappen { get; set; } = new Dictionary<string, int>();

        public override void OnLoadGame()
        {
            base.OnLoadGame();
            foreach (var town in g.world.build.GetBuilds<MapBuildTown>())
            {
                if (!LastYearEventHappen.ContainsKey(town.buildData.id))
                {
                    LastYearEventHappen.Add(town.buildData.id, GameHelper.GetGameYear());
                }
            }
        }

        public override void OnMonthly()
        {
            base.OnMonthly();

            var curYear = GameHelper.GetGameYear();
            foreach (var town in g.world.build.GetBuilds<MapBuildTown>())
            {
                if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, MONST_WAVE_RATE * (curYear - LastYearEventHappen[town.buildData.id])))
                {
                    LastYearEventHappen[town.buildData.id] = curYear;
                    MonstWave(town);
                }
                else
                if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, TOWN_WAR_RATE * (curYear - LastYearEventHappen[town.buildData.id])))
                {
                    LastYearEventHappen[town.buildData.id] = curYear;
                    TownWar(town);
                }
            }

            foreach (var school in g.world.build.GetBuilds<MapBuildSchool>())
            {
                if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, MONST_WAVE_RATE * (curYear - LastYearEventHappen[school.buildData.id])))
                {
                    LastYearEventHappen[school.buildData.id] = curYear;
                    MonstWave(school);
                }
            }
        }

        public static void TownWar(MapBuildTown town)
        {
            g.world.battle.IntoBattleInit(g.world.playerUnit.GetUnitPos(), g.conf.dungeonBase.GetItem(TOWN_WAR_DUNGEON_BASE_ID), 1, new WorldBattleData
            {
                isRealBattle = true,
                isSelfBattle = false,
                schoolID = null,
            });
        }

        public static void MonstWave(MapBuildTown town)
        {
            g.world.battle.IntoBattleInit(g.world.playerUnit.GetUnitPos(), g.conf.dungeonBase.GetItem(TOWN_MONST_WAVE_DUNGEON_BASE_ID), 1, new WorldBattleData
            {
                isRealBattle = true,
                isSelfBattle = false,
                schoolID = null,
            });
        }

        public static void MonstWave(MapBuildSchool school)
        {
            g.world.battle.IntoBattleInit(g.world.playerUnit.GetUnitPos(), g.conf.dungeonBase.GetItem(SECT_MONST_WAVE_DUNGEON_BASE_ID), 1, new WorldBattleData
            {
                isRealBattle = true,
                isSelfBattle = false,
                schoolID = school.schoolNameID,
            });
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
