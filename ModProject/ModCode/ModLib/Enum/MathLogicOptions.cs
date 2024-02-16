using ModLib.Object;
using System;
using static ModLib.Enum.MathLogicOptions;

namespace ModLib.Enum
{
    [EnumObjectIndex(517000)]
    public class MathLogicOptions : EnumObject, ILogicOptions
    {
        public static ILogicOptions Plus { get; } = new MathLogicOptions("+", (a, b) => a + b);
        public static ILogicOptions Minus { get; } = new MathLogicOptions("-", (a, b) => a - b);
        public static ILogicOptions Multi { get; } = new MathLogicOptions("*", (a, b) => a * b);
        public static ILogicOptions Devide { get; } = new MathLogicOptions("/", (a, b) => a / b);

        public string Logic { get; private set; }

        public Func<float, float, float> Method2Params { get; private set; }
        private MathLogicOptions(string logic, Func<float, float, float> method) : base(logic)
        {
            Logic = logic;
            Method2Params = method;
        }

        public Func<float, float> Method1Param { get; private set; }
        private MathLogicOptions(string logic, Func<float, float> method) : base(logic)
        {
            Logic = logic;
            Method1Param = method;
        }

        public float Exe(float a, float b)
        {
            return Method2Params.Invoke(a, b);
        }

        public float Exe(float a)
        {
            return Method1Param.Invoke(a);
        }

        public interface ILogicOptions
        {
            float Exe(float a, float b);
            float Exe(float a);
        }
    }
}
