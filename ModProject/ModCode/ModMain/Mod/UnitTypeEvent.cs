using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using ModLib.Enum;
using ModLib.Mod;
using System.Collections.Generic;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.UNIT_TYPE_EVENT)]
    public class UnitTypeEvent : ModEvent
    {
        public IDictionary<string, UnitTypeEnum> UnitTypeDic { get; set; } = new Dictionary<string, UnitTypeEnum>();

        public override void OnMonthly()
        {
            base.OnMonthly();
            foreach (var wunit in g.world.unit.GetUnits())
            {
                var unitId = wunit.GetUnitId();

                //add luck
                if (!UnitTypeDic.ContainsKey(unitId))
                {
                    UnitTypeDic.Add(unitId, AddRandomUnitType(wunit));
                }

                //add apprentice luck
                AddApprenticeLuck(wunit);

                //merchant
                if (wunit.IsPlayer() && UnitTypeDic[unitId] == UnitTypeEnum.Merchant)
                {
                    wunit.AddUnitMoney((wunit.GetUnitMoney() * UnitTypeEnum.Merchant.CustomLuck.CustomEffects["PassiveIncome"].Value0.Parse<float>()).Parse<int>());
                }

                //add property
                if (wunit.IsPlayer())
                {
                    if (wunit.GetGradeLvl() >= 3)
                        AddPlayerProp(wunit, wunit.GetGradeLvl() / 3.0f);
                }
                else
                {
                    AddNpcProp(wunit, 1.00f + wunit.GetGradeLvl() * 0.03f);
                }
            }
        }

        public override void OnYearly()
        {
            base.OnYearly();
            foreach (var wunit in g.world.unit.GetUnits())
            {
                if (!wunit.IsPlayer() && UnitTypeDic.ContainsKey(wunit.GetUnitId()) && UnitTypeDic[wunit.GetUnitId()] == UnitTypeEnum.SpeedUnit)
                {
                    wunit.AddProperty<int>(UnitPropertyEnum.MoveSpeed, 1);
                }
            }
        }

        public static UnitTypeEnum AddRandomUnitType(WorldUnitBase wunit)
        {
            var t = ModMain.ModObj.InGameCustomSettings.UnitTypeConfigs.RandomUnitType(CommonTool.Random(0.00f, 100.00f));
            if (t.CustomLuck != null)
                wunit.AddLuck(t.Value.Parse<int>());
            return t;
        }

        public static void AddApprenticeLuck(WorldUnitBase wunit)
        {
            foreach (var apprenticeLuck in ApprenticeLuckEnum.GetAllEnums<ApprenticeLuckEnum>())
            {
                var apprenticeLevel = apprenticeLuck.GetApprenticeLevel(wunit);

                var apprenticePrevLuckId = apprenticeLuck.GetApprenticeLuckId(apprenticeLevel - 1);
                if (apprenticePrevLuckId > 0 && wunit.GetLuck(apprenticePrevLuckId) != null)
                    wunit.DelLuck(apprenticePrevLuckId);

                var apprenticeLuckId = apprenticeLuck.GetApprenticeLuckId(apprenticeLevel);
                if (apprenticeLuckId > 0 && wunit.GetLuck(apprenticeLuckId)  == null)
                    wunit.AddLuck(apprenticeLuckId);
            }
        }

        public void AddPlayerProp(WorldUnitBase wunit, float ratio = 1.00f)
        {
            foreach (var p in UnitTypeEnum.Player.PropIncRatio)
            {
                var pType = p.Values[0] as UnitPropertyEnum;
                wunit.AddProperty(pType, (UnitTypeEnum.Player.CalProp(pType, wunit.GetProperty<int>(pType)) * ratio).Parse<int>());
            }
        }

        public void AddNpcProp(WorldUnitBase wunit, float ratio = 1.00f)
        {
            var utype = UnitTypeDic[wunit.GetUnitId()];
            foreach (var p in utype.PropIncRatio)
            {
                wunit.SetProperty(p.Values[0] as UnitPropertyEnum, utype.CalType(wunit, p.Values[0] as UnitPropertyEnum, ratio));
            }
        }

        public static UnitTypeEnum GetUnitTypeEnum(WorldUnitBase wunit)
        {
            var unitId = wunit.GetUnitId();
            var x = EventHelper.GetEvent<UnitTypeEvent>(ModConst.UNIT_TYPE_EVENT);
            return x.UnitTypeDic.ContainsKey(unitId) ? x.UnitTypeDic[unitId] : null;
        }
    }
}
