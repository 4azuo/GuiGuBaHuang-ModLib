using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.INFLATIONARY_EVENT)]
    public class InflationaryEvent : ModEvent
    {
        public const double InflationaryRate = 1.01;

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

        public void Inflationary(int year)
        {
            foreach (var props in g.conf.itemProps._allConfList)
            {
                props.sale = CalInflationary(props.sale, year);
                props.worth = CalInflationary(props.worth, year);
            }
            foreach (var item in g.conf.itemSkill._allConfList)
            {
                item.price = CalInflationary(item.price, year);
                item.cost = CalInflationary(item.cost, year);
                item.sale = CalInflationary(item.sale, year);
                item.worth = CalInflationary(item.worth, year);
            }
        }

        public int CalInflationary(int value, int year)
        {
            return Convert.ToInt32(value * Math.Pow(InflationaryRate, year));
        }
    }
}
