using ModLib.Enum;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MOD_nE7UL2.Enum
{
    public class UnitTypeEnum : EnumObject
    {
        public static UnitTypeEnum Default { get; } = new UnitTypeEnum()
        {
            PropIncRatio = new List<MultiValue>()
            {
                MultiValue.Create(UnitPropertyEnum.Attack, 0.001, 1),
                MultiValue.Create(UnitPropertyEnum.Defense, 0.0001, 0),
                MultiValue.Create(UnitPropertyEnum.HpMax, 0.002, 1),
                MultiValue.Create(UnitPropertyEnum.MpMax, 0.001, 0),
                //MultiValue.Create(UnitPropertyEnum.MoveSpeed, 0, 0),
            }
        };
        public static UnitTypeEnum Hero { get; } = new UnitTypeEnum()
        {
            PropIncRatio = new List<MultiValue>()
            {
                MultiValue.Create(UnitPropertyEnum.Attack, 0.002, 1),
                MultiValue.Create(UnitPropertyEnum.Defense, 0.0005, 0),
                MultiValue.Create(UnitPropertyEnum.HpMax, 0.001, 1),
                MultiValue.Create(UnitPropertyEnum.MpMax, 0.001, 0),
                //MultiValue.Create(UnitPropertyEnum.MoveSpeed, 0, 0),
            }
        };
        public static UnitTypeEnum PowerUnit { get; } = new UnitTypeEnum(UnitTypeLuckEnum.PowerUp)
        {
            PropIncRatio = new List<MultiValue>()
            {
                MultiValue.Create(UnitPropertyEnum.Attack, 0.006, 1),
                MultiValue.Create(UnitPropertyEnum.Defense, 0.001, 0),
                MultiValue.Create(UnitPropertyEnum.HpMax, 0.003, 2),
                MultiValue.Create(UnitPropertyEnum.MpMax, 0.001, 0),
                //MultiValue.Create(UnitPropertyEnum.MoveSpeed, 0, 0),
            }
        };
        public static UnitTypeEnum SpeedUnit { get; } = new UnitTypeEnum(UnitTypeLuckEnum.SpeedUp)
        {
            PropIncRatio = new List<MultiValue>()
            {
                MultiValue.Create(UnitPropertyEnum.Attack, 0.005, 1),
                MultiValue.Create(UnitPropertyEnum.Defense, 0, 0),
                MultiValue.Create(UnitPropertyEnum.HpMax, 0.001, 1),
                MultiValue.Create(UnitPropertyEnum.MpMax, 0, 0),
                //MultiValue.Create(UnitPropertyEnum.MoveSpeed, 0, 1),
            }
        };
        public static UnitTypeEnum TaoistUnit { get; } = new UnitTypeEnum(UnitTypeLuckEnum.Taoist)
        {
            PropIncRatio = new List<MultiValue>()
            {
                MultiValue.Create(UnitPropertyEnum.Attack, 0.01, 1),
                MultiValue.Create(UnitPropertyEnum.Defense, 0, 0),
                MultiValue.Create(UnitPropertyEnum.HpMax, 0.004, 1),
                MultiValue.Create(UnitPropertyEnum.MpMax, 0.004, 0),
                //MultiValue.Create(UnitPropertyEnum.MoveSpeed, 0, 0),
            }
        };
        public static UnitTypeEnum AtkUnit { get; } = new UnitTypeEnum(UnitTypeLuckEnum.ProAtk)
        {
            PropIncRatio = new List<MultiValue>()
            {
                MultiValue.Create(UnitPropertyEnum.Attack, 0.010, 1),
                MultiValue.Create(UnitPropertyEnum.Defense, 0, 0),
                MultiValue.Create(UnitPropertyEnum.HpMax, 0.001, 1),
                MultiValue.Create(UnitPropertyEnum.MpMax, 0.001, 0),
                //MultiValue.Create(UnitPropertyEnum.MoveSpeed, 0, 0),
            }
        };
        public static UnitTypeEnum DefUnit { get; } = new UnitTypeEnum(UnitTypeLuckEnum.ProDef)
        {
            PropIncRatio = new List<MultiValue>()
            {
                MultiValue.Create(UnitPropertyEnum.Attack, 0.006, 1),
                MultiValue.Create(UnitPropertyEnum.Defense, 0.006, 1),
                MultiValue.Create(UnitPropertyEnum.HpMax, 0.008, 5),
                MultiValue.Create(UnitPropertyEnum.MpMax, 0, 0),
                //MultiValue.Create(UnitPropertyEnum.MoveSpeed, 0, 0),
            }
        };
        public static UnitTypeEnum Angel { get; } = new UnitTypeEnum(UnitTypeLuckEnum.Angel)
        {
            PropIncRatio = new List<MultiValue>()
            {
                MultiValue.Create(UnitPropertyEnum.Attack, 0.006, 1),
                MultiValue.Create(UnitPropertyEnum.Defense, 0.001, 0),
                MultiValue.Create(UnitPropertyEnum.HpMax, 0.005, 3),
                MultiValue.Create(UnitPropertyEnum.MpMax, 0.003, 1),
                //MultiValue.Create(UnitPropertyEnum.MoveSpeed, 0, 0),
            }
        };
        public static UnitTypeEnum Evil { get; } = new UnitTypeEnum(UnitTypeLuckEnum.Evil)
        {
            PropIncRatio = new List<MultiValue>()
            {
                MultiValue.Create(UnitPropertyEnum.Attack, 0.007, 1),
                MultiValue.Create(UnitPropertyEnum.Defense, 0.002, 0),
                MultiValue.Create(UnitPropertyEnum.HpMax, 0.007, 3),
                MultiValue.Create(UnitPropertyEnum.MpMax, 0, 1),
                //MultiValue.Create(UnitPropertyEnum.MoveSpeed, 0, 0),
            }
        };
        public static UnitTypeEnum Merchant { get; } = new UnitTypeEnum(UnitTypeLuckEnum.Merchant)
        {
            PropIncRatio = new List<MultiValue>()
            {
                MultiValue.Create(UnitPropertyEnum.Attack, 0.001, 1),
                MultiValue.Create(UnitPropertyEnum.Defense, 0.001, 1),
                MultiValue.Create(UnitPropertyEnum.HpMax, 0.001, 2),
                MultiValue.Create(UnitPropertyEnum.MpMax, 0.001, 1),
                //MultiValue.Create(UnitPropertyEnum.MoveSpeed, 0, 0),
            }
        };

        public UnitTypeLuckEnum CustomLuck { get; private set; }
        public IList<MultiValue> PropIncRatio { get; private set; }
        private UnitTypeEnum(UnitTypeLuckEnum customLuck = null) : base(customLuck?.Value)
        {
            CustomLuck = customLuck;
        }

        public int CalProp(UnitPropertyEnum pType, int propValue, float inputRatio = 1.00f)
        {
            var stt = PropIncRatio.FirstOrDefault(x => x.Values[0] == pType);
            if (stt == null)
                return propValue;
            var ratio = stt.Values[1].Parse<float>();
            var min = stt.Values[2].Parse<float>();
            return (int)Math.Max(MathLogicOptions.Multi.Exe(propValue, ratio) * inputRatio, min);
        }

        public int CalType(WorldUnitBase wunit, UnitPropertyEnum pType, float ratio = 1.00f)
        {
            var propValue = wunit.GetProperty<int>(pType);

            propValue += Default.CalProp(pType, propValue, ratio);
            if (Value != null)
                propValue += this.CalProp(pType, propValue, ratio);
            if (wunit.IsHero())
                propValue += Hero.CalProp(pType, propValue, ratio);

            return propValue;
        }
    }
}
