using ModLib.Enum;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOD_nE7UL2.Enum
{
    [EnumObjectIndex(113000)]
    public class NpcUpGradeRatioEnum : EnumObject
    {
        public static NpcUpGradeRatioEnum QiRefining_Early { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.QiRefining_Early, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.QiRefining_Middle, 3.00d, 0.00d),
        });
        public static NpcUpGradeRatioEnum QiRefining_Middle { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.QiRefining_Middle, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.QiRefining_Late, 2.96d, 0.00d),
        });
        public static NpcUpGradeRatioEnum QiRefining_Late { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.QiRefining_Late, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.Foundation_Early1, 2.00d, 0.00d),
            MultiValue.Create(GradePhaseEnum.Foundation_Early2, 1.50d, 0.02d),
            MultiValue.Create(GradePhaseEnum.Foundation_Early3, 0.80d, 0.05d),
        });
        public static NpcUpGradeRatioEnum Foundation_Early1 { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.Foundation_Early1, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.Foundation_Middle, 2.50d, 0.00d),
        });
        public static NpcUpGradeRatioEnum Foundation_Early2 { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.Foundation_Early2, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.Foundation_Middle, 2.70d, 0.00d),
        });
        public static NpcUpGradeRatioEnum Foundation_Early3 { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.Foundation_Early3, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.Foundation_Middle, 3.00d, 0.00d),
        });
        public static NpcUpGradeRatioEnum Foundation_Middle { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.Foundation_Middle, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.Foundation_Late, 2.85d, 0.00d),
        });
        public static NpcUpGradeRatioEnum Foundation_Late { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.Foundation_Late, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.QiCondensation_Early, 1.20d, 0.00d),
        });
        public static NpcUpGradeRatioEnum QiCondensation_Early { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.QiCondensation_Early, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.QiCondensation_Middle, 2.50d, 0.00d),
        });
        public static NpcUpGradeRatioEnum QiCondensation_Middle { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.QiCondensation_Middle, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.QiCondensation_Late, 2.40d, 0.00d),
        });
        public static NpcUpGradeRatioEnum QiCondensation_Late { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.QiCondensation_Late, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.GoldenCore_Early1, 0.90d, 0.00d),
            MultiValue.Create(GradePhaseEnum.GoldenCore_Early2, 0.85d, 0.01d),
            MultiValue.Create(GradePhaseEnum.GoldenCore_Early3, 0.80d, 0.02d),
            MultiValue.Create(GradePhaseEnum.GoldenCore_Early4, 0.70d, 0.04d),
            MultiValue.Create(GradePhaseEnum.GoldenCore_Early5, 0.55d, 0.06d),
        });
        public static NpcUpGradeRatioEnum GoldenCore_Early1 { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.GoldenCore_Early1, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.GoldenCore_Middle, 2.00d, 0.00d),
        });
        public static NpcUpGradeRatioEnum GoldenCore_Early2 { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.GoldenCore_Early2, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.GoldenCore_Middle, 2.05d, 0.00d),
        });
        public static NpcUpGradeRatioEnum GoldenCore_Early3 { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.GoldenCore_Early3, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.GoldenCore_Middle, 2.10d, 0.00d),
        });
        public static NpcUpGradeRatioEnum GoldenCore_Early4 { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.GoldenCore_Early4, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.GoldenCore_Middle, 2.25d, 0.00d),
        });
        public static NpcUpGradeRatioEnum GoldenCore_Early5 { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.GoldenCore_Early5, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.GoldenCore_Middle, 2.40d, 0.00d),
        });
        public static NpcUpGradeRatioEnum GoldenCore_Middle { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.GoldenCore_Middle, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.GoldenCore_Late, 2.20d, 0.00d),
        });
        public static NpcUpGradeRatioEnum GoldenCore_Late { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.GoldenCore_Late, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.OriginSpirit_Early, 1.00d, 0.00d),
        });
        public static NpcUpGradeRatioEnum OriginSpirit_Early { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.OriginSpirit_Early, new List<MultiValue>() 
        {
            MultiValue.Create(GradePhaseEnum.OriginSpirit_Middle, 2.00d, 0.00d),
        });
        public static NpcUpGradeRatioEnum OriginSpirit_Middle { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.OriginSpirit_Middle, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.OriginSpirit_Late, 1.90d, 0.00d),
        });
        public static NpcUpGradeRatioEnum OriginSpirit_Late { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.OriginSpirit_Late, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.NascentSoul_Early1, 0.60d, 0.00d),
            MultiValue.Create(GradePhaseEnum.NascentSoul_Early2, 0.50d, 0.03d),
            MultiValue.Create(GradePhaseEnum.NascentSoul_Early3, 0.30d, 0.07d),
        });
        public static NpcUpGradeRatioEnum NascentSoul_Early1 { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.NascentSoul_Early1, new List<MultiValue>() 
        {
            MultiValue.Create(GradePhaseEnum.NascentSoul_Middle, 1.50d, 0.00d),
        });
        public static NpcUpGradeRatioEnum NascentSoul_Early2 { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.NascentSoul_Early2, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.NascentSoul_Middle, 1.60d, 0.00d),
        });
        public static NpcUpGradeRatioEnum NascentSoul_Early3 { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.NascentSoul_Early3, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.NascentSoul_Middle, 1.80d, 0.00d),
        });
        public static NpcUpGradeRatioEnum NascentSoul_Middle { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.NascentSoul_Middle, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.NascentSoul_Late, 1.50d, 0.00d),
        });
        public static NpcUpGradeRatioEnum NascentSoul_Late { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.NascentSoul_Late, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.SoulFormation_Early, 0.30d, 0.00d),
        });
        public static NpcUpGradeRatioEnum SoulFormation_Early { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.SoulFormation_Early, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.SoulFormation_Middle, 1.00d, 0.00d),
        });
        public static NpcUpGradeRatioEnum SoulFormation_Middle { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.SoulFormation_Middle, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.SoulFormation_Late, 0.90d, 0.00d),
        });
        public static NpcUpGradeRatioEnum SoulFormation_Late { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.SoulFormation_Late, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.Enlightenment_Early1, 0.10d, 0.00d),
            MultiValue.Create(GradePhaseEnum.Enlightenment_Early2, 0.08d, 0.01d),
            MultiValue.Create(GradePhaseEnum.Enlightenment_Early3, 0.05d, 0.02d),
            MultiValue.Create(GradePhaseEnum.Enlightenment_Early4, 0.01d, 0.04d),
        });
        public static NpcUpGradeRatioEnum Enlightenment_Early1 { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.Enlightenment_Early1, new List<MultiValue>() 
        { 
            MultiValue.Create(GradePhaseEnum.Enlightenment_Middle, 0.50d, 0.00d), 
        });
        public static NpcUpGradeRatioEnum Enlightenment_Early2 { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.Enlightenment_Early2, new List<MultiValue>() 
        { 
            MultiValue.Create(GradePhaseEnum.Enlightenment_Middle, 0.60d, 0.00d), 
        });
        public static NpcUpGradeRatioEnum Enlightenment_Early3 { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.Enlightenment_Early3, new List<MultiValue>() 
        { 
            MultiValue.Create(GradePhaseEnum.Enlightenment_Middle, 0.65d, 0.00d), 
        });
        public static NpcUpGradeRatioEnum Enlightenment_Early4 { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.Enlightenment_Early4, new List<MultiValue>() 
        { 
            MultiValue.Create(GradePhaseEnum.Enlightenment_Middle, 0.70d, 0.00d), 
        });
        public static NpcUpGradeRatioEnum Enlightenment_Middle { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.Enlightenment_Middle, new List<MultiValue>() 
        { 
            MultiValue.Create(GradePhaseEnum.Enlightenment_Late, 0.20d, 0.00d), 
        });
        public static NpcUpGradeRatioEnum Enlightenment_Late { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.Enlightenment_Late, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.Reborn_Early, 0.001d, 0.00d),
        });
        public static NpcUpGradeRatioEnum Reborn_Early { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.Reborn_Early, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.Reborn_Middle, 0.01d, 0.00d),
        });
        public static NpcUpGradeRatioEnum Reborn_Middle { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.Reborn_Middle, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.Reborn_Late, 0.004d, 0.00d),
        });
        public static NpcUpGradeRatioEnum Reborn_Late { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.Reborn_Late, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.Transendent_Early1, 0.00005d, 0.00d),
            MultiValue.Create(GradePhaseEnum.Transendent_Early2, 0.00004d, 0.01d),
            MultiValue.Create(GradePhaseEnum.Transendent_Early3, 0.00003d, 0.02d),
            MultiValue.Create(GradePhaseEnum.Transendent_Early4, 0.00001d, 0.03d),
        });
        public static NpcUpGradeRatioEnum Transendent_Early1 { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.Transendent_Early1, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.Transendent_Middle, 0.000001d, 0.00d),
        });
        public static NpcUpGradeRatioEnum Transendent_Early2 { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.Transendent_Early2, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.Transendent_Middle, 0.000002d, 0.00d),
        });
        public static NpcUpGradeRatioEnum Transendent_Early3 { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.Transendent_Early3, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.Transendent_Middle, 0.000003d, 0.00d),
        });
        public static NpcUpGradeRatioEnum Transendent_Early4 { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.Transendent_Early4, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.Transendent_Middle, 0.000004d, 0.00d),
        });
        public static NpcUpGradeRatioEnum Transendent_Middle { get; } = new NpcUpGradeRatioEnum(GradePhaseEnum.Transendent_Middle, new List<MultiValue>()
        {
            MultiValue.Create(GradePhaseEnum.Transendent_Late, 0.0000001d, 0.00d),
        });

        public GradePhaseEnum Grade { get; private set; }
        public IList<MultiValue> NextGrade { get; private set; }
        private NpcUpGradeRatioEnum(GradePhaseEnum grade, IList<MultiValue> nextGrade) : base()
        {
            Grade = grade;
            NextGrade = nextGrade;
        }
    }
}
