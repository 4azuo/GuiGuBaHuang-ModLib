using MOD_nE7UL2.Const;
using ModLib.Const;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using static DataBuildTown;
using static Il2CppSystem.Collections.Hashtable;
using static MOD_nE7UL2.Object.GameStts;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.INFLATIONARY_EVENT)]
    public class InflationaryEvent : ModEvent
    {
        public static InflationaryEvent Instance { get; set; }

        public const int REACH_LIMIT_DRAMA = 499919998;
        public const int LIMIT = 1000000000;

        public static _InflationaryConfigs Configs => ModMain.ModObj.GameSettings.InflationaryConfigs;
        public static Dictionary<string, int> OriginItemValues { get; } = new Dictionary<string, int>();

        public int Corruption { get; set; } = 0;

        public override void OnLoadGame()
        {
            base.OnLoadGame();
            BackupItemmValues();
            ItemInflationary();
        }

        public override void OnYearly()
        {
            base.OnYearly();
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

                foreach (var town in g.world.build.GetBuilds<MapBuildTown>())
                {
                    MapBuildPropertyEvent.AddBuildProperty(town, -(MapBuildPropertyEvent.GetBuildProperty(town) * 0.8).Parse<long>());
                }
                foreach (var school in g.world.build.GetBuilds<MapBuildSchool>())
                {
                    MapBuildPropertyEvent.AddBuildProperty(school, -(MapBuildPropertyEvent.GetBuildProperty(school) * 0.8).Parse<long>());
                }
            }
        }

        public static void BackupItemmValues()
        {
            foreach (var props in g.conf.itemProps._allConfList)
            {
                OriginItemValues[$"itemProps_{props.id}_sale"] = props.sale;
                OriginItemValues[$"itemProps_{props.id}_worth"] = props.worth;
            }
            foreach (var item in g.conf.itemSkill._allConfList)
            {
                OriginItemValues[$"itemSkill_{item.id}_price"] = item.price;
                OriginItemValues[$"itemSkill_{item.id}_cost"] = item.cost;
                OriginItemValues[$"itemSkill_{item.id}_sale"] = item.sale;
                OriginItemValues[$"itemSkill_{item.id}_worth"] = item.worth;
            }
            foreach (var refine in g.conf.townRefine._allConfList)
            {
                OriginItemValues[$"townRefine_{refine.id}_moneyCost"] = refine.moneyCost;
            }
        }

        public static void ItemInflationary()
        {
            foreach (var props in g.conf.itemProps._allConfList)
            {
                if (props.type == (int)PropsType.Money)
                    continue;

                props.sale = CalculateInflationary(OriginItemValues[$"itemProps_{props.id}_sale"]);
                props.worth = CalculateInflationary(OriginItemValues[$"itemProps_{props.id}_worth"]);

                if (props.IsPillRecipe() != null)
                {
                    var factory = g.conf.townFactotySell.GetItem(props.id);
                    if (factory != null)
                        factory.makePrice = (props.worth * ModConst.PILL_RECIPE_RATE).Parse<int>();
                }
            }
            foreach (var item in g.conf.itemSkill._allConfList)
            {
                item.price = CalculateInflationary(OriginItemValues[$"itemSkill_{item.id}_price"]);
                item.cost = CalculateInflationary(OriginItemValues[$"itemSkill_{item.id}_cost"]);
                item.sale = CalculateInflationary(OriginItemValues[$"itemSkill_{item.id}_sale"]);
                item.worth = CalculateInflationary(OriginItemValues[$"itemSkill_{item.id}_worth"]);
            }
            foreach (var refine in g.conf.townRefine._allConfList)
            {
                refine.moneyCost = CalculateInflationary(OriginItemValues[$"townRefine_{refine.id}_moneyCost"]);
            }
        }

        public static int CalculateInflationary(int originValue)
        {
            if (originValue <= 0)
                return originValue;
            return Convert.ToInt32(originValue * Math.Pow(SMLocalConfigsEvent.Instance.Calculate(Configs.InflationaryRate, SMLocalConfigsEvent.Instance.Configs.AddInflationRate), GameHelper.GetGameYear()) / Math.Pow(100, Instance.Corruption));
        }

        public static long CalculateInflationary(long originValue)
        {
            if (originValue <= 0)
                return originValue;
            return Convert.ToInt64(originValue * Math.Pow(SMLocalConfigsEvent.Instance.Calculate(Configs.InflationaryRate, SMLocalConfigsEvent.Instance.Configs.AddInflationRate), GameHelper.GetGameYear()) / Math.Pow(100, Instance.Corruption));
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
