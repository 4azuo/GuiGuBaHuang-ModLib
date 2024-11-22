using MOD_nE7UL2.Mod;
using ModLib.Enum;
using ModLib.Object;
using System;
using UnityEngine;

namespace MOD_nE7UL2.Enum
{
    public class AdjLevelEnum : EnumObject
    {
        public static AdjLevelEnum Common { get; } = new AdjLevelEnum("Common", 1, Color.white);
        public static AdjLevelEnum Uncommon { get; } = new AdjLevelEnum("Uncommon", 2, Color.green);
        public static AdjLevelEnum Rare { get; } = new AdjLevelEnum("Rare", 3, Color.blue);
        public static AdjLevelEnum Lgendary { get; } = new AdjLevelEnum("Lgendary", 4, Color.yellow);
        public static AdjLevelEnum Myst { get; } = new AdjLevelEnum("Myst", 5, Color.red);

        public string Label { get; private set; }
        public int Level { get; private set; }
        public Color Color { get; private set; }
        private AdjLevelEnum(string label, int lvl, Color color) : base()
        {
            Label = label;
            Level = lvl;
            Color = color;
        }

        public static AdjLevelEnum GetLevel(char c)
        {
            var allLvl = AdjLevelEnum.GetAllEnums<AdjLevelEnum>();
            return allLvl[c % allLvl.Length];
        }
    }
}
