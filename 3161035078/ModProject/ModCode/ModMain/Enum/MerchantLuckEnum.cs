using ModLib.Object;
using System;
using ModLib.Helper;

namespace MOD_nE7UL2.Enum
{
    public class MerchantLuckEnum : EnumObject
    {
        public static MerchantLuckEnum Merchant { get; } = new MerchantLuckEnum(420091000, (wunit) => wunit.GetUnitMoney(), 0.003f, 0.001f);

        public static long[] MerchantLevels { get; } = new long[]
        {
            /*01*/ 10000, 
            /*02*/ 20000, 
            /*03*/ 40000, 
            /*04*/ 80000, 
            /*05*/ 160000, 
            /*06*/ 480000, 
            /*07*/ 1440000, 
            /*08*/ 4320000, 
            /*09*/ 12960000, 
            /*10*/ 38880000
        };

        public int GetCurLevel(WorldUnitBase wunit)
        {
            for (int i = 0; i < MerchantLevels.Length; i++)
            {
                var level = i + 1;
                var luck = wunit.GetLuck(Merchant.Value.Parse<int>() + level);
                if (luck != null)
                    return level;
            }
            return 0;
        }

        public int GetNxtLevel(WorldUnitBase wunit)
        {
            var propertyValue = CalFunc.Invoke(wunit);
            for (int i = GetCurLevel(wunit); i < MerchantLevels.Length; i++)
            {
                if (propertyValue < MerchantLevels[i])
                {
                    return i;
                }
            }
            return MerchantLevels.Length;
        }

        public int GetMerchantLuckId(int lvl)
        {
            if (lvl <= 0)
                return 0;
            return Value.Parse<int>() + lvl;
        }

        public Func<WorldUnitBase, int> CalFunc { get; private set; }
        public float IncSellValueEachLvl { get; private set; }
        public float IncPassiveIncomeEachLvl { get; private set; }
        private MerchantLuckEnum(int id, Func<WorldUnitBase, int> calFunc, float incSellValueEachLvl, float incPassiveIncomeEachLvl) : base(id.ToString())
        {
            CalFunc = calFunc;
            IncSellValueEachLvl = incSellValueEachLvl;
            IncPassiveIncomeEachLvl = incPassiveIncomeEachLvl;
        }
    }
}
