using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using MOD_nE7UL2.Object;
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
        }

        public override void OnYearly()
        {
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
            var t = ModMain.ModObj.InGameSettings.UnitTypeConfigs.RandomUnitType(CommonTool.Random(0.00f, 100.00f));
            if (t.CustomLuck != null)
                wunit.AddLuck(t.Value.Parse<int>());
            return t;
        }

        public void AddProp(WorldUnitBase wunit, float ratio = 1.00f)
        {
            if (UnitTypeDic.ContainsKey(wunit.GetUnitId()))
            {
                var utype = UnitTypeDic[wunit.GetUnitId()];
                foreach (var p in utype.PropIncRatio)
                {
                    wunit.SetProperty(p.Values[0] as UnitPropertyEnum, utype.CalType(wunit, p.Values[0] as UnitPropertyEnum, ratio));
                }
            }
        }
    }
}
