using ModLib.Attributes;

namespace ModLib.Helper
{
    [ActionCat("Eco")]
    public static class EconomyHelper
    {
        public static void ResetWorldPrice()
        {
            g.conf.gameDifficultyValue.curItem.price = 100;
        }

        public static void SetWorldPrice(int price)
        {
            g.conf.gameDifficultyValue.curItem.price = price;
        }

        public static int GetWorldPrice()
        {
            return g.conf.gameDifficultyValue.curItem.price;
        }
    }
}