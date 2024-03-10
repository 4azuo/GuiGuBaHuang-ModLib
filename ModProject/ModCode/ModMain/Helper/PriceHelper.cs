using MOD_nE7UL2.Enum;
using System;
using System.Collections.Generic;

public static class PriceHelper
{
    public static readonly IDictionary<ModItemTypeEnum, float> ITEM_TYPE_RATIO = new Dictionary<ModItemTypeEnum, float>
    {
        //[ModItemTypeEnum.BreakthroughItem] = 1.00f,
        [ModItemTypeEnum.BreakthroughItemA] = 1.35f,
        [ModItemTypeEnum.BreakthroughItemB] = 1.42f,
        [ModItemTypeEnum.Skill] = 1.30f,
        [ModItemTypeEnum.Artifact] = 1.26f,
        [ModItemTypeEnum.TalismanRecipe] = 1.24f,
        [ModItemTypeEnum.PillRecipe] = 1.20f,
        [ModItemTypeEnum.Mount] = 1.20f,
        [ModItemTypeEnum.Ring] = 1.16f,
        [ModItemTypeEnum.Furnace] = 1.80f, //grade null
        [ModItemTypeEnum.BottleneckItem] = 1.10f,
        [ModItemTypeEnum.PowerUpItem] = 1.08f,
        [ModItemTypeEnum.Potion] = 1.05f,

        [ModItemTypeEnum.Herb] = 1.15f, //grade null
        [ModItemTypeEnum.Mine] = 1.15f, //grade null
        //[ModItemTypeEnum.BattleItem] = 1.00f,

        [ModItemTypeEnum.MarketItem] = 1.00f,
        [ModItemTypeEnum.SchoolItem] = 1.06f,
        //[ModItemTypeEnum.Cloth] = 1.00f,
        //[ModItemTypeEnum.Hobby] = 1.00f,
        [ModItemTypeEnum.Others] = 1.04f, //grade null
    };
    public static readonly IList<float> GRADE_RATIO = new List<float>
    {
        1.00f, 1.10f, 1.20f, 1.35f,
        1.50f, 1.70f, 2.00f, 2.40f,
        3.00f, 4.00f,
    };
    public static readonly IList<float> LEVEL_RATIO = new List<float>
    {
        1.00f, 1.05f, 1.10f, 1.20f, 1.40f, 1.80f,
    };

    public static int UpPrice(int cur, int grade, int level, float customRatio = 1.00f, int min = -1, int max = int.MaxValue)
    {
        grade = Math.Max(Math.Min(grade - 1, 9), 0);
        level = Math.Max(Math.Min(level - 1, 5), 0);
        var upPrice = cur.Parse<float>();
        upPrice *= GRADE_RATIO[grade];
        upPrice *= LEVEL_RATIO[level];
        upPrice *= customRatio;
        return Math.Min(max, Math.Max(min, upPrice.Parse<int>()));
    }
}