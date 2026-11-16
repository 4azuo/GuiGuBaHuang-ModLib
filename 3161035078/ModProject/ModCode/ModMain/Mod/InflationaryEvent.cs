using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Const;
using ModLib.Mod;
using ModLib.Helper;
using System;
using static MOD_nE7UL2.Object.ModStts;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Inflation: the world price index grows every year; most prices are scaled by it.
    /// When the index exceeds LIMIT a currency reform ("corruption") divides all money by 10000.
    /// </summary>
    [Cache(ModConst.INFLATIONARY_EVENT)]
    public class InflationaryEvent : ModEvent
    {
        public static InflationaryEvent Instance { get; set; }

        public const int REACH_LIMIT_DRAMA = 499919998;
        public const int LIMIT = 10000;

        public static _InflationaryConfigs Configs => ModMain.ModObj.ModSettings.InflationaryConfigs;

        public int Corruption { get; set; } = 0;
        public int WorldPrice { get; set; } = -1;

        /// <summary>
        /// First load: computes the index from the game year; applies it to the economy.
        /// </summary>
        public override void OnLoadGame()
        {
            base.OnLoadGame();
            if (WorldPrice == -1)
            {
                WorldPrice = SMLocalConfigsEvent.Instance.Calculate(EconomyHelper.GetWorldPrice(), SMLocalConfigsEvent.Instance.Configs.AddItemValueRate).Parse<int>();
                for (int i = 0; i < GameHelper.GetGameYear(); i++)
                {
                    WorldPrice = (WorldPrice * SMLocalConfigsEvent.Instance.Calculate(Configs.InflationaryRate, SMLocalConfigsEvent.Instance.Configs.AddInflationRate)).Parse<int>();
                }
            }
            EconomyHelper.SetWorldPrice(WorldPrice);
        }

        /// <summary>
        /// Raises the index by the inflation rate and checks the limit.
        /// </summary>
        public override void OnYearly()
        {
            base.OnYearly();
            WorldPrice = (EconomyHelper.GetWorldPrice() * SMLocalConfigsEvent.Instance.Calculate(Configs.InflationaryRate, SMLocalConfigsEvent.Instance.Configs.AddInflationRate)).Parse<int>();
            EconomyHelper.SetWorldPrice(WorldPrice);
            Corrupt();
        }

        /// <summary>
        /// Currency reform: resets the index, divides money of player/NPCs, cuts town/sect budgets by 80 %, clears the market, shows a drama.
        /// </summary>
        private void Corrupt()
        {
            if (EconomyHelper.GetWorldPrice() > LIMIT)
            {
                Corruption++;
                EconomyHelper.ResetWorldPrice();
                WorldPrice = EconomyHelper.GetWorldPrice();

                //affect to player
                var player = g.world.playerUnit;
                player.SetUnitMoney(player.GetUnitMoney() / 10000);
                player.RemoveStorageItem(ModLibConst.MONEY_PROP_ID);
                DramaTool.OpenDrama(REACH_LIMIT_DRAMA);

                //affect to npc
                foreach (var wunit in GetModChildParameterStore().WUnits)
                {
                    // the player is in WUnits too: already divided above (was divided twice -> 0, autotest TX-02)
                    if (wunit != null && !wunit.isDie && !wunit.IsPlayer())
                    {
                        wunit.SetUnitMoney(wunit.GetUnitMoney() / 10000);
                    }
                }

                //affect to town & school
                foreach (var town in GetModChildParameterStore().Towns)
                {
                    MapBuildPropertyEvent.AddBuildProperty(town, -(MapBuildPropertyEvent.GetBuildProperty(town) * 0.8).Parse<long>());
                }
                foreach (var school in GetModChildParameterStore().Schools)
                {
                    MapBuildPropertyEvent.AddBuildProperty(school, -(MapBuildPropertyEvent.GetBuildProperty(school) * 0.8).Parse<long>());
                }
                RealMarketEvent3.Instance.MarketStack.Clear();
            }
        }

        /// <summary>
        /// Scales a base price by the world price index (index 100 = x1).
        /// </summary>
        public static int CalculateInflationary(int originValue)
        {
            if (originValue <= 0)
                return originValue;
            // clamped: Convert.ToInt32 threw OverflowException for big prices with a high index
            return ValueHelper.SafeMulInt(originValue, EconomyHelper.GetWorldPrice() / 100d);
        }

        /// <summary>
        /// Scales a base price by the world price index (index 100 = x1).
        /// </summary>
        public static long CalculateInflationary(long originValue)
        {
            if (originValue <= 0)
                return originValue;
            return Convert.ToInt64((EconomyHelper.GetWorldPrice() / 100f) * originValue);
        }
    }
}
