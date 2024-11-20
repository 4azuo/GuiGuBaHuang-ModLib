using MOD_nE7UL2.Const;
using ModLib.Const;
using ModLib.Mod;
using System;
using System.Linq;
using static MOD_nE7UL2.Object.InGameStts;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.INFLATIONARY_EVENT)]
    public class InflationaryEvent : ModEvent
    {
        public const int REACH_LIMIT_DRAMA = 499919998;
        public const int LIMIT = 200000000;

        public static _InflationaryConfigs Configs => ModMain.ModObj.InGameCustomSettings.InflationaryConfigs;

        public int Corruption { get; set; } = 0;

        public override void OnLoadGame()
        {
            base.OnLoadGame();
            Inflationary(GameHelper.GetGameYear());
        }

        public override void OnYearly()
        {
            base.OnYearly();
            if (GetHighestCost() > LIMIT)
            {
                Corruption++;
                var player = g.world.playerUnit;
                player.SetUnitMoney(0);
                player.RemoveStorageItem(ModLibConst.MONEY_PROP_ID);
                DramaTool.OpenDrama(REACH_LIMIT_DRAMA);
            }
            Inflationary(1);
        }

        public static void Inflationary(int year)
        {
            foreach (var props in g.conf.itemProps._allConfList)
            {
                if (props.type == (int)PropsType.Money)
                    continue;

                props.sale = CalculateInflationary(props.sale, year);
                props.worth = CalculateInflationary(props.worth, year);

                if (props.IsPillRecipe() != null)
                {
                    var factory = g.conf.townFactotySell.GetItem(props.id);
                    if (factory != null)
                        factory.makePrice = props.worth / 6;
                }
            }
            foreach (var item in g.conf.itemSkill._allConfList)
            {
                item.price = CalculateInflationary(item.price, year);
                item.cost = CalculateInflationary(item.cost, year);
                item.sale = CalculateInflationary(item.sale, year);
                item.worth = CalculateInflationary(item.worth, year);
            }
            foreach (var refine in g.conf.townRefine._allConfList)
            {
                refine.moneyCost = CalculateInflationary(refine.moneyCost, year);
            }
        }

        public static int CalculateInflationary(int value, int year)
        {
            if (value <= 0)
                return value;
            var x = EventHelper.GetEvent<InflationaryEvent>(ModConst.INFLATIONARY_EVENT);
            return Convert.ToInt32(value * Math.Pow(Configs.InflationaryRate, year) / Math.Pow(100, x.Corruption));
        }

        public static long CalculateInflationary(long value, int year)
        {
            if (value <= 0)
                return value;
            var x = EventHelper.GetEvent<InflationaryEvent>(ModConst.INFLATIONARY_EVENT);
            return Convert.ToInt64(value * Math.Pow(Configs.InflationaryRate, year) / Math.Pow(100, x.Corruption));
        }

        public static int GetHighestCost()
        {
            return new int[] {
                BankAccountEvent.Cost(10),
                g.conf.itemProps._allConfList.ToArray().Max(x => x.sale),
                g.conf.itemProps._allConfList.ToArray().Max(x => x.worth),
                g.conf.itemSkill._allConfList.ToArray().Max(x => x.price),
                g.conf.itemSkill._allConfList.ToArray().Max(x => x.cost),
                g.conf.itemSkill._allConfList.ToArray().Max(x => x.sale),
                g.conf.itemSkill._allConfList.ToArray().Max(x => x.worth)
            }.Max();
        }
    }
}
