using ModLib.Object;

namespace ModLib.Enum
{
    public class LuckTypeEnum : EnumObject
    {
        public static LuckTypeEnum Nature { get; } = new LuckTypeEnum("1");
        public static LuckTypeEnum Nurture { get; } = new LuckTypeEnum("2");
        public static LuckTypeEnum RewriteDestiny { get; } = new LuckTypeEnum("3");

        public LuckTypeEnum(string value) : base(value) { }
    }
}
