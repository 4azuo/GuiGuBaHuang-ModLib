using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Helper;
using System;
using System.Collections.Generic;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.UNIT_TYPE_EVENT)]
    public class UnitTypeEvent : ModEvent
    {
        public static UnitTypeEvent Instance { get; set; }

        public IDictionary<string, UnitTypeEnum> UnitTypeDic { get; set; } = new Dictionary<string, UnitTypeEnum>();

        public override void OnLoadGame()
        {
            base.OnLoadGame();
            foreach (var wunit in GetModChildParameterStore().WUnits)
            {
                var unitId = wunit.GetUnitId();
                if (UnitTypeDic.ContainsKey(unitId) && UnitTypeDic[unitId].CustomLuck != null)
                {
                    RemoveUnitTypeLuck(wunit);
                    wunit.AddLuck(UnitTypeDic[unitId].Value.Parse<int>());
                }
            }
        }

        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);

            var unitId = wunit.GetUnitId();

            //add luck
            if (!UnitTypeDic.ContainsKey(unitId))
            {
                if (wunit.IsPlayer() && SMLocalConfigsEvent.Instance.Configs.StartupPlayerRole > 0)
                {
                    UnitTypeDic.Add(unitId, SMGlobalConfigsEvent.Instance.StartupPlayerRoles[SMLocalConfigsEvent.Instance.Configs.StartupPlayerRole]);
                }
                else
                {
                    UnitTypeDic.Add(unitId, AddRandomUnitType(wunit));
                }
            }

            //add apprentice luck
            AddApprenticeLuck(wunit);

            //add trainee luck
            AddTraineeLuck(wunit);

            //add merchant luck
            AddMerchantLuck(wunit);

            //merchant
            var money = wunit.GetUnitMoney();
            var income = wunit.IsPlayer() ? 0 : CommonTool.Random(Convert.ToInt32(Math.Pow(3, wunit.GetGradeLvl())), Convert.ToInt32(Math.Pow(4, wunit.GetGradeLvl())));

            if (UnitTypeDic[unitId] == UnitTypeEnum.Merchant)
            {
                income += (money * UnitTypeEnum.Merchant.CustomLuck.CustomEffects[ModConst.UTYPE_LUCK_EFX_PASSIVE_INCOME].Value0.Parse<float>()).Parse<int>();
            }

            var merchantLvl = MerchantLuckEnum.Merchant.GetCurLevel(wunit);
            if (merchantLvl > 0)
            {
                income += (money * merchantLvl * MerchantLuckEnum.Merchant.IncPassiveIncomeEachLvl).Parse<int>();
            }

            wunit.AddUnitMoney(income);

            //add property
            if (GameHelper.GetGameTotalMonth() % SMLocalConfigsEvent.Instance.Configs.GrowUpSpeed == 0)
            {
                if (wunit.IsPlayer())
                {
                    if (wunit.GetGradeLvl() >= 3)
                        AddPlayerProp(wunit, wunit.GetGradeLvl() / 3.0f);
                }
                else
                {
                    AddNpcProp(wunit, 1.00f + wunit.GetGradeLvl() * 0.03f);

                    foreach (var luck in ApprenticeLuckEnum.GetAllEnums<ApprenticeLuckEnum>())
                    {
                        if (CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, 8.00f))
                        {
                            wunit.AddProperty<int>(luck.PropertyEnum.GetPropertyEnum(), 1);
                        }
                    }
                }
            }
        }

        public override void OnYearly()
        {
            base.OnYearly();
            foreach (var wunit in GetModChildParameterStore().WUnits)
            {
                if (!wunit.IsPlayer() && UnitTypeDic.ContainsKey(wunit.GetUnitId()) && UnitTypeDic[wunit.GetUnitId()] == UnitTypeEnum.SpeedUnit)
                {
                    wunit.AddProperty<int>(UnitPropertyEnum.MoveSpeed, 1);
                }
            }
        }

        private void RemoveUnitTypeLuck(WorldUnitBase wunit)
        {
            foreach (var t in UnitTypeEnum.GetAllEnums<UnitTypeEnum>())
            {
                if (t.CustomLuck != null)
                    wunit.DelLuck(t.Value.Parse<int>());
            }
        }

        public static UnitTypeEnum AddRandomUnitType(WorldUnitBase wunit)
        {
            var t = ModMain.ModObj.ModSettings.UnitTypeConfigs.RandomUnitType(CommonTool.Random(0.00f, 100.00f));
            if (t.CustomLuck != null)
                wunit.AddLuck(t.Value.Parse<int>());
            return t;
        }

        public static void AddMerchantLuck(WorldUnitBase wunit)
        {
            foreach (var luck in MerchantLuckEnum.GetAllEnums<MerchantLuckEnum>())
            {
                var level = luck.GetNxtLevel(wunit);

                var luckId = luck.GetMerchantLuckId(level);
                if (luckId > 0 && wunit.GetLuck(luckId) == null)
                {
                    for (int i = 1; i <= MerchantLuckEnum.MerchantLevels.Length; i++)
                    {
                        wunit.DelLuck(luck.GetMerchantLuckId(i));
                    }
                    wunit.AddLuck(luckId);
                }
            }
        }

        public static void AddApprenticeLuck(WorldUnitBase wunit)
        {
            foreach (var luck in ApprenticeLuckEnum.GetAllEnums<ApprenticeLuckEnum>())
            {
                var level = luck.GetApprenticeLevel(wunit);

                var luckId = luck.GetApprenticeLuckId(level);
                if (luckId > 0 && wunit.GetLuck(luckId) == null)
                {
                    for (int i = 1; i <= ApprenticeLuckEnum.ApprenticeLevels.Length; i++)
                    {
                        wunit.DelLuck(luck.GetApprenticeLuckId(i));
                    }
                    wunit.AddLuck(luckId);
                }
            }
        }

        public static void AddTraineeLuck(WorldUnitBase wunit)
        {
            foreach (var luck in TraineeLuckEnum.GetAllEnums<TraineeLuckEnum>())
            {
                var level = luck.GetTraineeLevel(wunit);

                var luckId = luck.GetTraineeLuckId(level);
                if (luckId > 0 && wunit.GetLuck(luckId) == null)
                {
                    for (int i = 1; i <= TraineeLuckEnum.TraineeLevels.Length; i++)
                    {
                        wunit.DelLuck(luck.GetTraineeLuckId(i));
                    }
                    wunit.AddLuck(luckId);
                }
            }
        }

        public void AddPlayerProp(WorldUnitBase wunit, float ratio = 1.00f)
        {
            foreach (var p in UnitTypeEnum.Player.PropIncRatio)
            {
                var pType = p.Value0 as UnitPropertyEnum;
                wunit.AddProperty(pType, (UnitTypeEnum.Player.CalProp(pType, wunit.GetProperty<int>(pType)) * ratio).Parse<int>());
            }
        }

        public void AddNpcProp(WorldUnitBase wunit, float ratio = 1.00f)
        {
            var utype = UnitTypeDic[wunit.GetUnitId()];
            foreach (var p in utype.PropIncRatio)
            {
                var prop = p.Value0 as UnitPropertyEnum;
                var r2Player = Math.Max(1.0f, g.world.playerUnit.GetProperty<int>(prop).Parse<float>() / wunit.GetProperty<int>(prop).Parse<float>());
                wunit.SetProperty(prop, utype.CalType(wunit, prop, SMLocalConfigsEvent.Instance.Calculate(ratio * r2Player, SMLocalConfigsEvent.Instance.Configs.AddNpcGrowRate).Parse<float>()));
            }
        }

        public static UnitTypeEnum GetUnitTypeEnum(WorldUnitBase wunit)
        {
            var unitId = wunit.GetUnitId();
            return Instance.UnitTypeDic.ContainsKey(unitId) ? Instance.UnitTypeDic[unitId] : null;
        }
    }
}
