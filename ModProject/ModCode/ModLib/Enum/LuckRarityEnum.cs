using ModLib.Object;

namespace ModLib.Enum
{
    [EnumObjectIndex(516000)]
    public class LuckRarityEnum : EnumObject
    {
        public static LuckRarityEnum Common { get; } = new LuckRarityEnum("1");
        public static LuckRarityEnum Uncommon { get; } = new LuckRarityEnum("2");
        public static LuckRarityEnum Rare { get; } = new LuckRarityEnum("3");
        public static LuckRarityEnum Epic { get; } = new LuckRarityEnum("4");
        public static LuckRarityEnum Legendary { get; } = new LuckRarityEnum("5");
        public static LuckRarityEnum Mythic { get; } = new LuckRarityEnum("6");
        public static LuckRarityEnum NegativeCommon { get; } = new LuckRarityEnum("-1");
        public static LuckRarityEnum NegativeUncommon { get; } = new LuckRarityEnum("-2");
        public static LuckRarityEnum NegativeRare { get; } = new LuckRarityEnum("-3");
        public static LuckRarityEnum NegativeEpic { get; } = new LuckRarityEnum("-4");
        public static LuckRarityEnum NegativeLegendary { get; } = new LuckRarityEnum("-5");
        public static LuckRarityEnum NegativeMythic { get; } = new LuckRarityEnum("-6");

        public LuckRarityEnum(string value) : base(value) { }
    }
}
