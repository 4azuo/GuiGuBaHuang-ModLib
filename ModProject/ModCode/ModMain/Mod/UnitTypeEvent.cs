using MOD_JhUKQ7.Const;
using MOD_JhUKQ7.Enum;
using ModLib.Enum;
using ModLib.Mod;
using System.Collections.Generic;

namespace MOD_JhUKQ7.Mod
{
    [Cache(ModConst.UNIT_TYPE_EVENT_KEY)]
    public sealed class UnitTypeEvent : ModEvent
    {
        public IDictionary<string, UnitTypeEnum> UnitTypeDic { get; set; } = new Dictionary<string, UnitTypeEnum>();

        public override void OnMonthly()
        {
            DebugHelper.WriteLine($"Start: UnitTypeEvent: Count={UnitTypeDic.Count}");

            foreach (var wunit in g.world.unit.GetUnits())
            {
                //add luck
                if (!UnitTypeDic.ContainsKey(wunit.GetUnitId()))
                {
                    UnitTypeDic.Add(wunit.GetUnitId(), AddRandomUnitType(wunit));
                }

                //add property
                if (wunit.IsPlayer())
                    continue;
                AddProp(wunit);
            }

            DebugHelper.WriteLine($"End: UnitTypeEvent: Count={UnitTypeDic.Count}, PlayerType={UnitTypeDic[g.world.playerUnit.GetUnitId()].Name}");
        }

        public static UnitTypeEnum AddRandomUnitType(WorldUnitBase wunit)
        {
            var r = CommonTool.Random(0.00f, 100.00f);
            UnitTypeEnum t;
            if (ValueHelper.IsBetween(r, 0.00f, 10.00f))
            {
                t = UnitTypeEnum.PowerUnit;
            }
            else if (ValueHelper.IsBetween(r, 10.00f, 20.00f))
            {
                t = UnitTypeEnum.SpeedUnit;
            }
            else if (ValueHelper.IsBetween(r, 20.00f, 30.00f))
            {
                t = UnitTypeEnum.TaoistUnit;
            }
            else if (ValueHelper.IsBetween(r, 30.00f, 50.00f))
            {
                t = UnitTypeEnum.AtkUnit;
            }
            else if (ValueHelper.IsBetween(r, 50.00f, 70.00f))
            {
                t = UnitTypeEnum.DefUnit;
            }
            else if (ValueHelper.IsBetween(r, 70.00f, 72.00f))
            {
                t = UnitTypeEnum.Angel;
            }
            else if (ValueHelper.IsBetween(r, 72.00f, 75.00f))
            {
                t = UnitTypeEnum.Evil;
            }
            else
            {
                t = UnitTypeEnum.Default;
            }
            if (t.CustomLuck != null)
                wunit.AddLuck(t.Value.Parse<int>());

            return t;
        }

        public void AddProp(WorldUnitBase wunit)
        {
            var utype = UnitTypeDic[wunit.GetUnitId()];
            foreach (var p in utype.PropIncRatio)
            {
                wunit.SetProperty(p.Values[0] as UnitPropertyEnum, utype.CalType(wunit, p.Values[0] as UnitPropertyEnum));
            }
        }
    }
}
