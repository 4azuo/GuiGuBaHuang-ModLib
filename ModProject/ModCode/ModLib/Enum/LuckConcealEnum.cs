﻿using ModLib.Object;

namespace ModLib.Enum
{
    public class LuckConcealEnum : EnumObject
    {
        public static LuckConcealEnum NotHidden { get; } = new LuckConcealEnum("0");
        public static LuckConcealEnum Hidden { get; } = new LuckConcealEnum("1");

        public LuckConcealEnum(string value) : base(value) { }
    }
}
