using MOD_nE7UL2.Const;
using ModLib.Const;
using ModLib.Mod;
using System;
using static MOD_nE7UL2.Object.ModStts;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.INFLATIONARY_EVENT)]
    public class InflationaryEvent : ModEvent
    {
        public static InflationaryEvent Instance { get; set; }

        public const int REACH_LIMIT_DRAMA = 499919998;
        public const int LIMIT = 10000;

        public static _InflationaryConfigs Configs => ModMain.ModObj.ModSettings.InflationaryConfigs;

        public int Corruption { get; set; } = 0;
        public int WorldPrice { get; set; } = -1;

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

        public override void OnYearly()
        {
            base.OnYearly();
            WorldPrice = (EconomyHelper.GetWorldPrice() * SMLocalConfigsEvent.Instance.Calculate(Configs.InflationaryRate, SMLocalConfigsEvent.Instance.Configs.AddInflationRate)).Parse<int>();
            EconomyHelper.SetWorldPrice(WorldPrice);
            Corrupt();
        }

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
                    if (wunit != null && !wunit.isDie)
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

        public static int CalculateInflationary(int originValue)
        {
            if (originValue <= 0)
                return originValue;
            return Convert.ToInt32((EconomyHelper.GetWorldPrice() / 100f) * originValue);
        }

        public static long CalculateInflationary(long originValue)
        {
            if (originValue <= 0)
                return originValue;
            return Convert.ToInt64((EconomyHelper.GetWorldPrice() / 100f) * originValue);
        }
    }
}
