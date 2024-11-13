using MOD_nE7UL2.Const;
using ModLib.Mod;
using System;
using static MOD_nE7UL2.Object.InGameStts;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.INFLATIONARY_EVENT)]
    public class InflationaryEvent : ModEvent
    {
        public static _InflationaryConfigs Configs => ModMain.ModObj.InGameCustomSettings.InflationaryConfigs;

        public override void OnLoadGame()
        {
            base.OnLoadGame();
            Inflationary(GameHelper.GetGameYear());
        }

        public override void OnYearly()
        {
            base.OnYearly();
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
            }
            foreach (var item in g.conf.itemSkill._allConfList)
            {
                item.price = CalculateInflationary(item.price, year);
                item.cost = CalculateInflationary(item.cost, year);
                item.sale = CalculateInflationary(item.sale, year);
                item.worth = CalculateInflationary(item.worth, year);
            }
        }

        public static int CalculateInflationary(int value, int year)
        {
            if (value <= 0)
                return value;
            return Convert.ToInt32(value * Math.Pow(Configs.InflationaryRate, year));
        }

        public static long CalculateInflationary(long value, int year)
        {
            if (value <= 0)
                return value;
            return Convert.ToInt64(value * Math.Pow(Configs.InflationaryRate, year));
        }
    }
}
