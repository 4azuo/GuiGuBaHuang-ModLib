using ModLib.Object;

namespace ModLib.Enum
{
    [EnumObjectIndex(512000)]
    public class FeatureTypeEnum : EnumObject
    {
        public static FeatureTypeEnum Nature { get; } = new FeatureTypeEnum("1");
        public static FeatureTypeEnum Nurture { get; } = new FeatureTypeEnum("2");
        public static FeatureTypeEnum RewriteDestiny { get; } = new FeatureTypeEnum("3");

        public FeatureTypeEnum(string value) : base(value) { }
    }
}
