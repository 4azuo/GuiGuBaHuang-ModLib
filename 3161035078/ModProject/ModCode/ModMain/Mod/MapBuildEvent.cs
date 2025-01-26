using MOD_nE7UL2.Const;
using ModLib.Mod;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.MAP_BUILD_EVENT)]
    public class MapBuildEvent : ModEvent
    {
        public const float MONST_WAVE_RATE = 1f;
        public const float TOWN_WAR_RATE = 0.4f;

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
                    MonstWave(town);
                }
                else
                if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, TOWN_WAR_RATE * (curYear - LastYearEventHappen[town.buildData.id])))
                {
                    TownWar(town);
                }
            }

            foreach (var school in g.world.build.GetBuilds<MapBuildSchool>())
            {
                if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, MONST_WAVE_RATE * (curYear - LastYearEventHappen[school.buildData.id])))
                {
                    MonstWave(school);
                }
            }
        }

        public static void TownWar(MapBuildTown town)
        {

        }

        public static void MonstWave(MapBuildTown town)
        {
            
        }

        public static void MonstWave(MapBuildSchool school)
        {

        }
    }
}
