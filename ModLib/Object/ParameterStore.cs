using ModLib.Mod;
using Newtonsoft.Json;
using System.Linq;
using ModLib.Helper;

namespace ModLib.Object
{
    /// <summary>
    /// Snapshot of world data used by monthly events (units with a valid area, buildings, towns, schools).
    /// </summary>
    public class ParameterStore
    {
        [JsonIgnore]
        public WorldUnitBase[] WUnits { get; private set; }
        [JsonIgnore]
        public MapBuildBase[] Buildings { get; private set; }
        [JsonIgnore]
        public MapBuildTown[] Towns { get; private set; }
        [JsonIgnore]
        public MapBuildSchool[] Schools { get; private set; }

        /// <summary>
        /// Use the factory methods.
        /// </summary>
        private ParameterStore() { }

        /// <summary>
        /// Builds the snapshot from the current world.
        /// </summary>
        public static ParameterStore CreateModMasterParameterStore()
        {
            return new ParameterStore
            {
                WUnits = g.world.unit.GetUnits().ToArray().Where(x =>
                {
                    try
                    {
                        return x.GetUnitPosAreaId() > 0;
                    }
                    catch
                    {
                        return false;
                    }
                }).ToArray(),
                Buildings = g.world.build.GetBuilds().ToArray(),
                Towns = g.world.build.GetBuilds<MapBuildTown>().ToArray(),
                Schools = g.world.build.GetBuilds<MapBuildSchool>().ToArray()
            };
        }

        /// <summary>
        /// Builds a subset for a ModChild; missing arguments fall back to the ModMaster snapshot.
        /// </summary>
        public static ParameterStore CreateModChildParameterStore(WorldUnitBase[] wunits = null, MapBuildBase[] buildings = null, MapBuildTown[] towns = null, MapBuildSchool[] schools = null)
        {
            return new ParameterStore
            {
                WUnits = wunits ?? ModMaster.ModObj.ParameterStore.WUnits,
                Buildings = buildings ?? ModMaster.ModObj.ParameterStore.Buildings,
                Towns = towns ?? ModMaster.ModObj.ParameterStore.Towns,
                Schools = schools ?? ModMaster.ModObj.ParameterStore.Schools
            };
        }
    }
}
