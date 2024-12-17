using ModLib.Enum;
using ModLib.Object;

namespace MOD_nE7UL2.Enum
{
    public class ApprenticeLuckEnum : EnumObject
    {
        public static ApprenticeLuckEnum Alchemy { get; } = new ApprenticeLuckEnum(420011000, UnitDynPropertyEnum.RefineElixir);
        public static ApprenticeLuckEnum Forge { get; } = new ApprenticeLuckEnum(420021000, UnitDynPropertyEnum.RefineWeapon);
        public static ApprenticeLuckEnum Talismans { get; } = new ApprenticeLuckEnum(420031000, UnitDynPropertyEnum.Symbol);
        public static ApprenticeLuckEnum FengShui { get; } = new ApprenticeLuckEnum(420041000, UnitDynPropertyEnum.Geomancy);
        public static ApprenticeLuckEnum Herbology { get; } = new ApprenticeLuckEnum(420051000, UnitDynPropertyEnum.Herbal);
        public static ApprenticeLuckEnum Mining { get; } = new ApprenticeLuckEnum(420061000, UnitDynPropertyEnum.Mine);

        public static int[] ApprenticeLevels { get; } = new int[] { 50, 100, 150, 250, 350, 500, 650, 850, 1050, 1300 };

        public int GetApprenticeLevel(WorldUnitBase wunit)
        {
            var propertyValue = wunit.GetDynProperty(this.PropertyEnum).value;
            for (int i = 0; i < ApprenticeLevels.Length; i++)
            {
                if (propertyValue < ApprenticeLevels[i])
                {
                    return i;
                }
            }
            return ApprenticeLevels.Length;
        }

        public int GetApprenticeLuckId(int lvl)
        {
            if (lvl <= 0)
                return 0;
            return Value.Parse<int>() + lvl;
        }

        public UnitDynPropertyEnum PropertyEnum { get; private set; }
        private ApprenticeLuckEnum(int id, UnitDynPropertyEnum propertyEnum) : base(id.ToString())
        {
            PropertyEnum = propertyEnum;
        }
    }
}
