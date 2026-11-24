using ModLib.Object;

namespace ModLib.Enum
{
    /// <summary>
    /// Visibility of a luck in the UI.
    /// </summary>
    public class LuckConcealEnum : EnumObject
    {
        public static LuckConcealEnum NotHidden { get; } = new LuckConcealEnum("0");
        public static LuckConcealEnum Hidden { get; } = new LuckConcealEnum("1");

        /// <summary>
        /// Creates an entry.
        /// </summary>
        public LuckConcealEnum(string value) : base(value) { }
    }
}
