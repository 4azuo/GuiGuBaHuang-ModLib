using ModLib.Object;

namespace ModLib.Enum
{
    /// <summary>
    /// How the duration of a luck stacks when added again.
    /// </summary>
    public class LuckDurStackTypeEnum : EnumObject
    {
        public static LuckDurStackTypeEnum ExistAtSameTime { get; } = new LuckDurStackTypeEnum("0");
        public static LuckDurStackTypeEnum DurStacked { get; } = new LuckDurStackTypeEnum("1");
        public static LuckDurStackTypeEnum NotStack { get; } = new LuckDurStackTypeEnum("2");
        public static LuckDurStackTypeEnum Multi { get; } = new LuckDurStackTypeEnum("3");

        /// <summary>
        /// Creates an entry.
        /// </summary>
        public LuckDurStackTypeEnum(string value) : base(value) { }
    }
}
