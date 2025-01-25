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

        public override void OnMonthly()
        {
            base.OnMonthly();

            var towns = g.world.build.GetBuilds<MapBuildTown>().ToList();
            foreach (var t in towns)
            {
                if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, MONST_WAVE_RATE))
                {

                }
                else
                if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, TOWN_WAR_RATE))
                {

                }
            }
        }

        public static void TownWar()
        {

        }
    }
}
