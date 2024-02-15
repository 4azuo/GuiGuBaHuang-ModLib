using ModLib.Enum;
using ModLib.Object;
using System.Collections.Generic;

namespace MOD_JhUKQ7.Enum
{
    public class AnimaUpGradeEnum : EnumObject
    {
        public static AnimaUpGradeEnum QiRefining_Early { get; } = new AnimaUpGradeEnum(GradePhaseEnum.QiRefining_Early, GradePhaseEnum.QiRefining_Middle, 200, 1);
        public static AnimaUpGradeEnum QiRefining_Middle { get; } = new AnimaUpGradeEnum(GradePhaseEnum.QiRefining_Middle, GradePhaseEnum.QiRefining_Late, 400, 1);
        public static AnimaUpGradeEnum QiRefining_Late { get; } = new AnimaUpGradeEnum(GradePhaseEnum.QiRefining_Late, GradePhaseEnum.Foundation_Early1, 800, 0.9f);
        public static AnimaUpGradeEnum Foundation_Early1 { get; } = new AnimaUpGradeEnum(GradePhaseEnum.Foundation_Early1, GradePhaseEnum.Foundation_Middle, 1000, 0.9f);
        public static AnimaUpGradeEnum Foundation_Early2 { get; } = new AnimaUpGradeEnum(GradePhaseEnum.Foundation_Early2, GradePhaseEnum.Foundation_Middle, 1000, 0.9f);
        public static AnimaUpGradeEnum Foundation_Early3 { get; } = new AnimaUpGradeEnum(GradePhaseEnum.Foundation_Early3, GradePhaseEnum.Foundation_Middle, 1000, 0.9f);
        public static AnimaUpGradeEnum Foundation_Middle { get; } = new AnimaUpGradeEnum(GradePhaseEnum.Foundation_Middle, GradePhaseEnum.Foundation_Late, 1200, 0.9f);
        public static AnimaUpGradeEnum Foundation_Late { get; } = new AnimaUpGradeEnum(GradePhaseEnum.Foundation_Late, GradePhaseEnum.QiCondensation_Early, 1400, 0.8f);
        public static AnimaUpGradeEnum QiCondensation_Early { get; } = new AnimaUpGradeEnum(GradePhaseEnum.QiCondensation_Early, GradePhaseEnum.QiCondensation_Middle, 1600, 0.8f);
        public static AnimaUpGradeEnum QiCondensation_Middle { get; } = new AnimaUpGradeEnum(GradePhaseEnum.QiCondensation_Middle, GradePhaseEnum.QiCondensation_Late, 1800, 0.8f);
        public static AnimaUpGradeEnum QiCondensation_Late { get; } = new AnimaUpGradeEnum(GradePhaseEnum.QiCondensation_Late, GradePhaseEnum.GoldenCore_Early1, 2500, 0.7f);
        public static AnimaUpGradeEnum GoldenCore_Early1 { get; } = new AnimaUpGradeEnum(GradePhaseEnum.GoldenCore_Early1, GradePhaseEnum.GoldenCore_Middle, 2800, 0.7f);
        public static AnimaUpGradeEnum GoldenCore_Early2 { get; } = new AnimaUpGradeEnum(GradePhaseEnum.GoldenCore_Early2, GradePhaseEnum.GoldenCore_Middle, 2800, 0.7f);
        public static AnimaUpGradeEnum GoldenCore_Early3 { get; } = new AnimaUpGradeEnum(GradePhaseEnum.GoldenCore_Early3, GradePhaseEnum.GoldenCore_Middle, 2800, 0.7f);
        public static AnimaUpGradeEnum GoldenCore_Early4 { get; } = new AnimaUpGradeEnum(GradePhaseEnum.GoldenCore_Early4, GradePhaseEnum.GoldenCore_Middle, 2800, 0.7f);
        public static AnimaUpGradeEnum GoldenCore_Early5 { get; } = new AnimaUpGradeEnum(GradePhaseEnum.GoldenCore_Early5, GradePhaseEnum.GoldenCore_Middle, 2800, 0.7f);
        public static AnimaUpGradeEnum GoldenCore_Middle { get; } = new AnimaUpGradeEnum(GradePhaseEnum.GoldenCore_Middle, GradePhaseEnum.GoldenCore_Late, 3100, 0.7f);
        public static AnimaUpGradeEnum GoldenCore_Late { get; } = new AnimaUpGradeEnum(GradePhaseEnum.GoldenCore_Late, GradePhaseEnum.OriginSpirit_Early, 3400, 0.6f);
        public static AnimaUpGradeEnum OriginSpirit_Early { get; } = new AnimaUpGradeEnum(GradePhaseEnum.OriginSpirit_Early, GradePhaseEnum.OriginSpirit_Middle, 3700, 0.6f);
        public static AnimaUpGradeEnum OriginSpirit_Middle { get; } = new AnimaUpGradeEnum(GradePhaseEnum.OriginSpirit_Middle, GradePhaseEnum.OriginSpirit_Late, 4000, 0.6f);
        public static AnimaUpGradeEnum OriginSpirit_Late { get; } = new AnimaUpGradeEnum(GradePhaseEnum.OriginSpirit_Late, GradePhaseEnum.NascentSoul_Early1, 5000, 0.5f);
        public static AnimaUpGradeEnum NascentSoul_Early1 { get; } = new AnimaUpGradeEnum(GradePhaseEnum.NascentSoul_Early1, GradePhaseEnum.NascentSoul_Middle, 5500, 0.5f);
        public static AnimaUpGradeEnum NascentSoul_Early2 { get; } = new AnimaUpGradeEnum(GradePhaseEnum.NascentSoul_Early2, GradePhaseEnum.NascentSoul_Middle, 5500, 0.5f);
        public static AnimaUpGradeEnum NascentSoul_Early3 { get; } = new AnimaUpGradeEnum(GradePhaseEnum.NascentSoul_Early3, GradePhaseEnum.NascentSoul_Middle, 5500, 0.5f);
        public static AnimaUpGradeEnum NascentSoul_Middle { get; } = new AnimaUpGradeEnum(GradePhaseEnum.NascentSoul_Middle, GradePhaseEnum.NascentSoul_Late, 6000, 0.5f);
        public static AnimaUpGradeEnum NascentSoul_Late { get; } = new AnimaUpGradeEnum(GradePhaseEnum.NascentSoul_Late, GradePhaseEnum.SoulFormation_Early, 6500, 0.4f);
        public static AnimaUpGradeEnum SoulFormation_Early { get; } = new AnimaUpGradeEnum(GradePhaseEnum.SoulFormation_Early, GradePhaseEnum.SoulFormation_Middle, 7000, 0.4f);
        public static AnimaUpGradeEnum SoulFormation_Middle { get; } = new AnimaUpGradeEnum(GradePhaseEnum.SoulFormation_Middle, GradePhaseEnum.SoulFormation_Late, 7500, 0.4f);
        public static AnimaUpGradeEnum SoulFormation_Late { get; } = new AnimaUpGradeEnum(GradePhaseEnum.SoulFormation_Late, GradePhaseEnum.Enlightenment_Early1, 9000, 0.3f);
        public static AnimaUpGradeEnum Enlightenment_Early1 { get; } = new AnimaUpGradeEnum(GradePhaseEnum.Enlightenment_Early1, GradePhaseEnum.Enlightenment_Middle, 10000, 0.3f);
        public static AnimaUpGradeEnum Enlightenment_Early2 { get; } = new AnimaUpGradeEnum(GradePhaseEnum.Enlightenment_Early2, GradePhaseEnum.Enlightenment_Middle, 10000, 0.3f);
        public static AnimaUpGradeEnum Enlightenment_Early3 { get; } = new AnimaUpGradeEnum(GradePhaseEnum.Enlightenment_Early3, GradePhaseEnum.Enlightenment_Middle, 10000, 0.3f);
        public static AnimaUpGradeEnum Enlightenment_Early4 { get; } = new AnimaUpGradeEnum(GradePhaseEnum.Enlightenment_Early4, GradePhaseEnum.Enlightenment_Middle, 10000, 0.3f);
        public static AnimaUpGradeEnum Enlightenment_Middle { get; } = new AnimaUpGradeEnum(GradePhaseEnum.Enlightenment_Middle, GradePhaseEnum.Enlightenment_Late, 11000, 0.3f);
        //public static AnimaUpGradeEnum Enlightenment_Late { get; } = new AnimaUpGradeEnum(GradePhaseEnum.Enlightenment_Late, 4800, 1);
        //public static AnimaUpGradeEnum Reborn_Early { get; } = new AnimaUpGradeEnum(GradePhaseEnum.Reborn_Early, 100, 1);
        //public static AnimaUpGradeEnum Reborn_Middle { get; } = new AnimaUpGradeEnum(GradePhaseEnum.Reborn_Middle, 100, 1);
        //public static AnimaUpGradeEnum Reborn_Late { get; } = new AnimaUpGradeEnum(GradePhaseEnum.Reborn_Late, 100, 1);
        //public static AnimaUpGradeEnum Transendent_Early1 { get; } = new AnimaUpGradeEnum(GradePhaseEnum.Transendent_Early1, 100, 1);
        //public static AnimaUpGradeEnum Transendent_Early2 { get; } = new AnimaUpGradeEnum(GradePhaseEnum.Transendent_Early2, 100, 1);
        //public static AnimaUpGradeEnum Transendent_Early3 { get; } = new AnimaUpGradeEnum(GradePhaseEnum.Transendent_Early3, 100, 1);
        //public static AnimaUpGradeEnum Transendent_Early4 { get; } = new AnimaUpGradeEnum(GradePhaseEnum.Transendent_Early4, 100, 1);
        //public static AnimaUpGradeEnum Transendent_Middle { get; } = new AnimaUpGradeEnum(GradePhaseEnum.Transendent_Middle, 100, 1);
        //public static AnimaUpGradeEnum Transendent_Late { get; } = new AnimaUpGradeEnum(GradePhaseEnum.Transendent_Late, 100, 1);

        public GradePhaseEnum Grade { get; private set; }
        public GradePhaseEnum NextGrade { get; private set; }
        public int MinAnima { get; private set; }
        public float RatioPer100Anima { get; private set; }
        private AnimaUpGradeEnum(GradePhaseEnum grade, GradePhaseEnum nextGrade, int minAnima, float ratioPer100Anima) : base()
        {
            Grade = grade;
            NextGrade = nextGrade;
            MinAnima = minAnima;
            RatioPer100Anima = ratioPer100Anima;
        }
    }
}
