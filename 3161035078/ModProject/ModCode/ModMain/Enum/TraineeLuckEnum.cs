using ModLib.Object;
using System;
using ModLib.Helper;

namespace MOD_nE7UL2.Enum
{
    public class TraineeLuckEnum : EnumObject
    {
        public static TraineeLuckEnum Martial { get; } = new TraineeLuckEnum(420071000, (wunit) => wunit.GetBasisPhysicSum());
        public static TraineeLuckEnum Magic { get; } = new TraineeLuckEnum(420081000, (wunit) => wunit.GetBasisMagicSum());

        public static int[] TraineeLevels { get; } = new int[] { 200, 400, 800, 1600, 3200, 6400, 12800, 25600, 51200, 102400 };

        public int GetTraineeLevel(WorldUnitBase wunit)
        {
            var propertyValue = CalFunc.Invoke(wunit);
            for (int i = 0; i < TraineeLevels.Length; i++)
            {
                if (propertyValue < TraineeLevels[i])
                {
                    return i;
                }
            }
            return TraineeLevels.Length;
        }

        public int GetTraineeLuckId(int lvl)
        {
            if (lvl <= 0)
                return 0;
            return Value.Parse<int>() + lvl;
        }

        public Func<WorldUnitBase, int> CalFunc { get; private set; }
        private TraineeLuckEnum(int id, Func<WorldUnitBase, int> calFunc) : base(id.ToString())
        {
            CalFunc = calFunc;
        }
    }
}
