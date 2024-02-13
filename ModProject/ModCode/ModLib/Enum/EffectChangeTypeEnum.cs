using ModLib.Object;

namespace ModLib.Enum
{
    public class EffectChangeTypeEnum : EnumObject
    {
        public static EffectChangeTypeEnum Percentage { get; } = new EffectChangeTypeEnum("0");
        public static EffectChangeTypeEnum FixedValue { get; } = new EffectChangeTypeEnum("1");

        public EffectChangeTypeEnum(string value) : base(value) { }
    }
}
