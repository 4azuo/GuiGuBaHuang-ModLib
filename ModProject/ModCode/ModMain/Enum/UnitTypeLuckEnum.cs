using ModLib.Object;
using System.Management.Instrumentation;

namespace MOD_nE7UL2.Enum
{
    [EnumObjectIndex(115000)]
    public class UnitTypeLuckEnum : EnumObject
    {
        public static UnitTypeLuckEnum PowerUp { get; } = new UnitTypeLuckEnum(420010100);
        public static UnitTypeLuckEnum SpeedUp { get; } = new UnitTypeLuckEnum(420010200);
        public static UnitTypeLuckEnum Taoist { get; } = new UnitTypeLuckEnum(420010300);
        public static UnitTypeLuckEnum ProAtk { get; } = new UnitTypeLuckEnum(420010400);
        public static UnitTypeLuckEnum ProDef { get; } = new UnitTypeLuckEnum(420010500);
        public static UnitTypeLuckEnum Angel { get; } = new UnitTypeLuckEnum(420010600);
        public static UnitTypeLuckEnum Evil { get; } = new UnitTypeLuckEnum(420010700);

        private UnitTypeLuckEnum(int id) : base(id.ToString())
        {
            CheckExists();
        }

        private void CheckExists()
        {
            if (g.conf.roleCreateFeature.GetItemIndex(Value.Parse<int>()) == -1)
                throw new InstanceNotFoundException();
        }
    }
}
