using ModLib.Attributes;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for managing world economy settings.
    /// Provides utilities to get and set world pricing multipliers.
    /// </summary>
    [ActionCat("Eco")]
    public static class EconomyHelper
    {
        /// <summary>
        /// Resets world price multiplier to default (100%).
        /// </summary>
        public static void ResetWorldPrice()
        {
            g.conf.gameDifficultyValue.curItem.price = 100;
        }

        /// <summary>
        /// Sets the world price multiplier.
        /// </summary>
        /// <param name="price">Price percentage (100 = normal)</param>
        public static void SetWorldPrice(int price)
        {
            g.conf.gameDifficultyValue.curItem.price = price;
        }

        /// <summary>
        /// Gets the current world price multiplier.
        /// </summary>
        /// <returns>Price percentage</returns>
        public static int GetWorldPrice()
        {
            return g.conf.gameDifficultyValue.curItem.price;
        }
    }
}