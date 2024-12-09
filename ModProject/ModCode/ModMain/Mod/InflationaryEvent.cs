using MOD_nE7UL2.Const;
using ModLib.Const;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using static MOD_nE7UL2.Object.InGameStts;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.INFLATIONARY_EVENT)]
    public class InflationaryEvent : ModEvent
    {
        public const int REACH_LIMIT_DRAMA = 499919998;
        public const int LIMIT = 500000000;

        public static _InflationaryConfigs Configs => ModMain.ModObj.InGameCustomSettings.InflationaryConfigs;
        public static Dictionary<string, int> ItemValues { get; } = new Dictionary<string, int>();

        public int Corruption { get; set; } = 0;

        public override void OnLoadGame()
        {
            base.OnLoadGame();
            ItemInflationary();
        }

        public override void OnYearly()
        {
            base.OnYearly();
            BackupItemmValues();
            ItemInflationary();
            Corrupt();
        }

        private void Corrupt()
        {
            if (GetHighestCost() > LIMIT)
            {
                Corruption++;
                var player = g.world.playerUnit;
                player.SetUnitMoney(0);
                player.RemoveStorageItem(ModLibConst.MONEY_PROP_ID);
                DramaTool.OpenDrama(REACH_LIMIT_DRAMA);
            }
        }

        public static void BackupItemmValues()
        {
            foreach (var props in g.conf.itemProps._allConfList)
            {
                ItemValues[$"itemProps_{props.id}_sale"] = props.sale;
                ItemValues[$"itemProps_{props.id}_worth"] = props.worth;
            }
            foreach (var item in g.conf.itemSkill._allConfList)
            {
                ItemValues[$"itemSkill_{item.id}_price"] = item.price;
                ItemValues[$"itemSkill_{item.id}_cost"] = item.cost;
                ItemValues[$"itemSkill_{item.id}_sale"] = item.sale;
                ItemValues[$"itemSkill_{item.id}_worth"] = item.worth;
            }
            foreach (var refine in g.conf.townRefine._allConfList)
            {
                ItemValues[$"townRefine_{refine.id}_moneyCost"] = refine.moneyCost;
            }
        }

        public static void ItemInflationary()
        {
            foreach (var props in g.conf.itemProps._allConfList)
            {
                if (props.type == (int)PropsType.Money)
                    continue;

                props.sale = CalculateInflationary(ItemValues[$"itemProps_{props.id}_sale"]);
                props.worth = CalculateInflationary(ItemValues[$"itemProps_{props.id}_worth"]);

                if (props.IsPillRecipe() != null)
                {
                    var factory = g.conf.townFactotySell.GetItem(props.id);
                    if (factory != null)
                        factory.makePrice = props.worth / 6;
                }
            }
            foreach (var item in g.conf.itemSkill._allConfList)
            {
                item.price = CalculateInflationary(ItemValues[$"itemSkill_{item.id}_price"]);
                item.cost = CalculateInflationary(ItemValues[$"itemSkill_{item.id}_cost"]);
                item.sale = CalculateInflationary(ItemValues[$"itemSkill_{item.id}_sale"]);
                item.worth = CalculateInflationary(ItemValues[$"itemSkill_{item.id}_worth"]);
            }
            foreach (var refine in g.conf.townRefine._allConfList)
            {
                refine.moneyCost = CalculateInflationary(ItemValues[$"townRefine_{refine.id}_moneyCost"]);
            }
        }

        public static int CalculateInflationary(int originValue)
        {
            if (originValue <= 0)
                return originValue;
            var smConfigs = EventHelper.GetEvent<SMLocalConfigsEvent>(ModConst.SM_LOCAL_CONFIGS_EVENT);
            var x = EventHelper.GetEvent<InflationaryEvent>(ModConst.INFLATIONARY_EVENT);
            return Convert.ToInt32(originValue * Math.Pow(smConfigs.Calculate(Configs.InflationaryRate, smConfigs.Configs.AddInflationRate), GameHelper.GetGameYear()) / Math.Pow(100, x.Corruption));
        }

        public static long CalculateInflationary(long originValue)
        {
            if (originValue <= 0)
                return originValue;
            var smConfigs = EventHelper.GetEvent<SMLocalConfigsEvent>(ModConst.SM_LOCAL_CONFIGS_EVENT);
            var x = EventHelper.GetEvent<InflationaryEvent>(ModConst.INFLATIONARY_EVENT);
            return Convert.ToInt64(originValue * Math.Pow(smConfigs.Calculate(Configs.InflationaryRate, smConfigs.Configs.AddInflationRate), GameHelper.GetGameYear()) / Math.Pow(100, x.Corruption));
        }

        public static int GetHighestCost()
        {
            return new int[] {
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
