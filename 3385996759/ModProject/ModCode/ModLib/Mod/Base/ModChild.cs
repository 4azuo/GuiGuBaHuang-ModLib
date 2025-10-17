namespace ModLib.Mod
{
    public abstract class ModChild : ModEvent
    {

        #region Caches On ModChild
        /// <summary>
        /// Feel free to use, but dont change if you dont know!
        /// </summary>
        public WorldUnitBase[] WUnits { get; set; }
        /// <summary>
        /// Feel free to use, but dont change if you dont know!
        /// </summary>
        public MapBuildBase[] Buildings { get; set; }
        /// <summary>
        /// Feel free to use, but dont change if you dont know!
        /// </summary>
        public MapBuildTown[] Towns { get; set; }
        /// <summary>
        /// Feel free to use, but dont change if you dont know!
        /// </summary>
        public MapBuildSchool[] Schools { get; set; }
        #endregion

        public void RefreshDataCaches()
        {
            WUnits = g.world.unit.GetUnits().ToArray();
            Buildings = g.world.build.GetBuilds().ToArray();
            Towns = g.world.build.GetBuilds<MapBuildTown>().ToArray();
            Schools = g.world.build.GetBuilds<MapBuildSchool>().ToArray();
        }
    }
}
