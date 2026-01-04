using MOD_nE7UL2.Const;
using ModLib.Object;
using System;
using System.Collections.Generic;
using ModLib.Helper;

namespace MOD_nE7UL2.Enum
{
    public class UnitTypeLuckEnum : EnumObject
    {
        public static UnitTypeLuckEnum PowerUp { get; } = new UnitTypeLuckEnum(420010100);
        public static UnitTypeLuckEnum SpeedUp { get; } = new UnitTypeLuckEnum(420010200);
        public static UnitTypeLuckEnum Taoist { get; } = new UnitTypeLuckEnum(420010300);
        public static UnitTypeLuckEnum ProAtk { get; } = new UnitTypeLuckEnum(420010400);
        public static UnitTypeLuckEnum ProDef { get; } = new UnitTypeLuckEnum(420010500);
        public static UnitTypeLuckEnum Angel { get; } = new UnitTypeLuckEnum(420010600);
        public static UnitTypeLuckEnum Evil { get; } = new UnitTypeLuckEnum(420010700);
        public static UnitTypeLuckEnum Merchant { get; } = new UnitTypeLuckEnum(420010800)
        {
            CustomEffects = new Dictionary<string, MultiValue>()
            {
                [ModConst.UTYPE_LUCK_EFX_FREE_STORAGE] = MultiValue.Create(true),
                [ModConst.UTYPE_LUCK_EFX_BUY_COST] = MultiValue.Create(-0.10f),
                [ModConst.UTYPE_LUCK_EFX_SELL_VALUE] = MultiValue.Create(+0.10f),
                [ModConst.UTYPE_LUCK_EFX_PASSIVE_INCOME] = MultiValue.Create(+0.03f),
            }
        };

        public IDictionary<string, MultiValue> CustomEffects { get; private set; }
        private UnitTypeLuckEnum(int id) : base(id.ToString())
        {
            CheckExists();
        }

        private void CheckExists()
        {
           if (g.conf.roleCreateFeature.GetItemIndex(Value.Parse<int>()) == -1)
               throw new Exception("Instance Not Found");
        }
    }
}
