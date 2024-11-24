using MOD_nE7UL2.Enum;
using MOD_nE7UL2.Mod;

namespace MOD_nE7UL2.Object
{
    public class CustomRefine
    {
        public int Index { get; set; }
        public AdjTypeEnum AdjType { get; set; }
        public AdjLevelEnum AdjLevel { get; set; }
        public float RandomMultiplier { get; set; }

        public double GetRefineCustommAdjValue(WorldUnitBase wunit, DataProps.PropsData props, int refineLvl)
        {
            if (refineLvl < Index * 100)
                return 0;
            var r = 0.001f * props.propsInfoBase.grade + 0.0002f * props.propsInfoBase.level;
            return AdjType.GetBaseValue(wunit) * r * refineLvl * AdjLevel.Multiplier * RandomMultiplier;
        }
    }
}
