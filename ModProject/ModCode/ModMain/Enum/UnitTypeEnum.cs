using ModLib.Enum;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MOD_nE7UL2.Enum
{
    [EnumObjectIndex(114000)]
    public class UnitTypeEnum : EnumObject
    {
        public static UnitTypeEnum Default { get; } = new UnitTypeEnum()
        {
            PropIncRatio = new List<MultiValue>()
            {
                MultiValue.Create(UnitPropertyEnum.Attack, MathLogicOptions.Multi, 0.003, 1),
                MultiValue.Create(UnitPropertyEnum.Defense, MathLogicOptions.Multi, 0.001, 0),
                MultiValue.Create(UnitPropertyEnum.HpMax, MathLogicOptions.Multi, 0.005, 1),
                MultiValue.Create(UnitPropertyEnum.MpMax, MathLogicOptions.Multi, 0.002, 0),
                MultiValue.Create(UnitPropertyEnum.MoveSpeed, MathLogicOptions.Multi, 0, 0),
            }
        };
        public static UnitTypeEnum Hero { get; } = new UnitTypeEnum()
        {
            PropIncRatio = new List<MultiValue>()
            {
                MultiValue.Create(UnitPropertyEnum.Attack, MathLogicOptions.Multi, 0.004, 1),
                MultiValue.Create(UnitPropertyEnum.Defense, MathLogicOptions.Multi, 0.001, 0),
                MultiValue.Create(UnitPropertyEnum.HpMax, MathLogicOptions.Multi, 0.005, 1),
                MultiValue.Create(UnitPropertyEnum.MpMax, MathLogicOptions.Multi, 0.002, 0),
                MultiValue.Create(UnitPropertyEnum.MoveSpeed, MathLogicOptions.Multi, 0, 0),
            }
        };
        public static UnitTypeEnum PowerUnit { get; } = new UnitTypeEnum(UnitTypeLuckEnum.PowerUp)
        {
            PropIncRatio = new List<MultiValue>()
            {
                MultiValue.Create(UnitPropertyEnum.Attack, MathLogicOptions.Multi, 0.008, 1),
                MultiValue.Create(UnitPropertyEnum.Defense, MathLogicOptions.Multi, 0.001, 0),
                MultiValue.Create(UnitPropertyEnum.HpMax, MathLogicOptions.Multi, 0.004, 1),
                MultiValue.Create(UnitPropertyEnum.MpMax, MathLogicOptions.Multi, 0.001, 0),
                MultiValue.Create(UnitPropertyEnum.MoveSpeed, MathLogicOptions.Multi, 0, 0),
            }
        };
        public static UnitTypeEnum SpeedUnit { get; } = new UnitTypeEnum(UnitTypeLuckEnum.SpeedUp)
        {
            PropIncRatio = new List<MultiValue>()
            {
                MultiValue.Create(UnitPropertyEnum.Attack, MathLogicOptions.Multi, 0.007, 1),
                MultiValue.Create(UnitPropertyEnum.Defense, MathLogicOptions.Multi, 0, 0),
                MultiValue.Create(UnitPropertyEnum.HpMax, MathLogicOptions.Multi, 0.001, 0),
                MultiValue.Create(UnitPropertyEnum.MpMax, MathLogicOptions.Multi, 0, 0),
                MultiValue.Create(UnitPropertyEnum.MoveSpeed, MathLogicOptions.Multi, 0, 1),
            }
        };
        public static UnitTypeEnum TaoistUnit { get; } = new UnitTypeEnum(UnitTypeLuckEnum.Taoist)
        {
            PropIncRatio = new List<MultiValue>()
            {
                MultiValue.Create(UnitPropertyEnum.Attack, MathLogicOptions.Multi, 0.01, 1),
                MultiValue.Create(UnitPropertyEnum.Defense, MathLogicOptions.Multi, 0, 0),
                MultiValue.Create(UnitPropertyEnum.HpMax, MathLogicOptions.Multi, 0.004, 1),
                MultiValue.Create(UnitPropertyEnum.MpMax, MathLogicOptions.Multi, 0.004, 0),
                MultiValue.Create(UnitPropertyEnum.MoveSpeed, MathLogicOptions.Multi, 0, 0),
            }
        };
        public static UnitTypeEnum AtkUnit { get; } = new UnitTypeEnum(UnitTypeLuckEnum.ProAtk)
        {
            PropIncRatio = new List<MultiValue>()
            {
                MultiValue.Create(UnitPropertyEnum.Attack, MathLogicOptions.Multi, 0.015, 1),
                MultiValue.Create(UnitPropertyEnum.Defense, MathLogicOptions.Multi, 0, 0),
                MultiValue.Create(UnitPropertyEnum.HpMax, MathLogicOptions.Multi, 0.001, 1),
                MultiValue.Create(UnitPropertyEnum.MpMax, MathLogicOptions.Multi, 0.001, 0),
                MultiValue.Create(UnitPropertyEnum.MoveSpeed, MathLogicOptions.Multi, 0, 0),
            }
        };
        public static UnitTypeEnum DefUnit { get; } = new UnitTypeEnum(UnitTypeLuckEnum.ProDef)
        {
            PropIncRatio = new List<MultiValue>()
            {
                MultiValue.Create(UnitPropertyEnum.Attack, MathLogicOptions.Multi, 0.006, 1),
                MultiValue.Create(UnitPropertyEnum.Defense, MathLogicOptions.Multi, 0.006, 0),
                MultiValue.Create(UnitPropertyEnum.HpMax, MathLogicOptions.Multi, 0.008, 1),
                MultiValue.Create(UnitPropertyEnum.MpMax, MathLogicOptions.Multi, 0, 0),
                MultiValue.Create(UnitPropertyEnum.MoveSpeed, MathLogicOptions.Multi, 0, 0),
            }
        };
        public static UnitTypeEnum Angel { get; } = new UnitTypeEnum(UnitTypeLuckEnum.Angel)
        {
            PropIncRatio = new List<MultiValue>()
            {
                MultiValue.Create(UnitPropertyEnum.Attack, MathLogicOptions.Multi, 0.007, 1),
                MultiValue.Create(UnitPropertyEnum.Defense, MathLogicOptions.Multi, 0.001, 0),
                MultiValue.Create(UnitPropertyEnum.HpMax, MathLogicOptions.Multi, 0.005, 1),
                MultiValue.Create(UnitPropertyEnum.MpMax, MathLogicOptions.Multi, 0.003, 1),
                MultiValue.Create(UnitPropertyEnum.MoveSpeed, MathLogicOptions.Multi, 0, 0),
            }
        };
        public static UnitTypeEnum Evil { get; } = new UnitTypeEnum(UnitTypeLuckEnum.Evil)
        {
            PropIncRatio = new List<MultiValue>()
            {
                MultiValue.Create(UnitPropertyEnum.Attack, MathLogicOptions.Multi, 0.008, 1),
                MultiValue.Create(UnitPropertyEnum.Defense, MathLogicOptions.Multi, 0.002, 0),
                MultiValue.Create(UnitPropertyEnum.HpMax, MathLogicOptions.Multi, 0.007, 1),
                MultiValue.Create(UnitPropertyEnum.MpMax, MathLogicOptions.Multi, 0, 0),
                MultiValue.Create(UnitPropertyEnum.MoveSpeed, MathLogicOptions.Multi, 0, 0),
            }
        };

        public UnitTypeLuckEnum CustomLuck { get; private set; }
        public IList<MultiValue> PropIncRatio { get; private set; }
        private UnitTypeEnum(UnitTypeLuckEnum customLuck = null) : base(customLuck?.Value)
        {
            CustomLuck = customLuck;
        }

        public int CalProp(UnitPropertyEnum pType, int propValue)
        {
            var stt = PropIncRatio.FirstOrDefault(x => x.Values[0] == pType);
            if (stt == null)
                return propValue;
            var logic = stt.Values[1].Parse<MathLogicOptions>();
            var ratio = stt.Values[2].Parse<float>();
            var min = stt.Values[3].Parse<float>();
            return (int)Math.Max(logic.Exe(propValue, ratio), min);
        }

        public int CalType(WorldUnitBase wunit, UnitPropertyEnum pType)
        {
            var propValue = wunit.GetProperty<int>(pType);

            propValue += Default.CalProp(pType, propValue);
            if (Value != null)
                propValue += this.CalProp(pType, propValue);
            propValue += Hero.CalProp(pType, propValue);

            return propValue;
        }
    }
}
