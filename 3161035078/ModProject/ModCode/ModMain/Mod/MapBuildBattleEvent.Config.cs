using MOD_nE7UL2.Object;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Town wars and monster waves (MapBuildBattleEvent), read from ModConf/map_build_battle_configs.json; the defaults below are the same as the file.
    /// </summary>
    public class MapBuildBattleConfig : JsonConfig<MapBuildBattleConfig>
    {
        public const string FILE = "map_build_battle_configs.json";
        public override string FileName => FILE;

        /// <summary>Chance in % that the player escapes from a town war / monster wave.</summary>
        public float EscapeChance { get; set; } = 10.00f;

        protected override void Fix(MapBuildBattleConfig d)
        {
            EscapeChance = Rate(EscapeChance);
        }
    }
}
