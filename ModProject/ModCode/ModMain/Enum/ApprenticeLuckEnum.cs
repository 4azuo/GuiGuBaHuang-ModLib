using ModLib.Object;
using System.Collections.Generic;
using System.Management.Instrumentation;

namespace MOD_nE7UL2.Enum
{
    public class ApprenticeLuckEnum : EnumObject
    {
        public static ApprenticeLuckEnum Alchemy { get; } = new ApprenticeLuckEnum(420011000);
        public static ApprenticeLuckEnum Forge { get; } = new ApprenticeLuckEnum(420021000);
        public static ApprenticeLuckEnum Talismans { get; } = new ApprenticeLuckEnum(420031000);
        public static ApprenticeLuckEnum FengShui { get; } = new ApprenticeLuckEnum(420041000);
        public static ApprenticeLuckEnum Herbology { get; } = new ApprenticeLuckEnum(420051000);
        public static ApprenticeLuckEnum Mining { get; } = new ApprenticeLuckEnum(420061000);

        private ApprenticeLuckEnum(int id) : base(id.ToString())
        {
        }
    }
}
