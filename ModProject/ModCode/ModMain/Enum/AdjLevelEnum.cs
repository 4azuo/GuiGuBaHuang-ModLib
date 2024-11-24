using ModLib.Object;
using UnityEngine;

namespace MOD_nE7UL2.Enum
{
    public class AdjLevelEnum : EnumObject
    {
        public static AdjLevelEnum Common { get; } = new AdjLevelEnum("Common", 1, 1.0, Color.white);
        public static AdjLevelEnum Uncommon { get; } = new AdjLevelEnum("Uncommon", 2, 1.1, Color.green);
        public static AdjLevelEnum Rare { get; } = new AdjLevelEnum("Rare", 3, 1.22, Color.cyan);
        public static AdjLevelEnum Myst { get; } = new AdjLevelEnum("Myst", 4, 1.35, Color.magenta);
        public static AdjLevelEnum Lgendary { get; } = new AdjLevelEnum("Lgendary", 5, 1.5, Color.yellow);
        public static AdjLevelEnum Beast { get; } = new AdjLevelEnum("Myst", 6, 1.7, Color.red);

        public string Label { get; private set; }
        public int Level { get; private set; }
        public double Multiplier { get; private set; }
        public Color Color { get; private set; }
        private AdjLevelEnum(string label, int lvl, double multiplier, Color color) : base(label)
        {
            Label = label;
            Level = lvl;
            Multiplier = multiplier;
            Color = color;
        }

        public static AdjLevelEnum GetLevel(char c)
        {
            var allLvl = AdjLevelEnum.GetAllEnums<AdjLevelEnum>();
            return allLvl[c % allLvl.Length];
        }
    }
}
