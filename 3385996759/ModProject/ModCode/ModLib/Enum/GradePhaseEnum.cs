﻿using ModLib.Object;

namespace ModLib.Enum
{
    public class GradePhaseEnum : EnumObject
    {
        public static GradePhaseEnum QiRefining_Early { get; } = new GradePhaseEnum(1, GradeEnum.QiRefining, BottleneckEnum.QiRefining_Middle);
        public static GradePhaseEnum QiRefining_Middle { get; } = new GradePhaseEnum(2, GradeEnum.QiRefining, BottleneckEnum.QiRefining_Late);
        public static GradePhaseEnum QiRefining_Late { get; } = new GradePhaseEnum(3, GradeEnum.QiRefining);
        public static GradePhaseEnum Foundation_Early1 { get; } = new GradePhaseEnum(4, GradeEnum.Foundation, BottleneckEnum.Foundation_Middle);
        public static GradePhaseEnum Foundation_Early2 { get; } = new GradePhaseEnum(5, GradeEnum.Foundation, BottleneckEnum.Foundation_Middle);
        public static GradePhaseEnum Foundation_Early3 { get; } = new GradePhaseEnum(6, GradeEnum.Foundation, BottleneckEnum.Foundation_Middle);
        public static GradePhaseEnum Foundation_Middle { get; } = new GradePhaseEnum(7, GradeEnum.Foundation, BottleneckEnum.Foundation_Late);
        public static GradePhaseEnum Foundation_Late { get; } = new GradePhaseEnum(8, GradeEnum.Foundation);
        public static GradePhaseEnum QiCondensation_Early { get; } = new GradePhaseEnum(9, GradeEnum.QiCondensation, BottleneckEnum.QiCondensation_Middle);
        public static GradePhaseEnum QiCondensation_Middle { get; } = new GradePhaseEnum(10, GradeEnum.QiCondensation, BottleneckEnum.QiCondensation_Late);
        public static GradePhaseEnum QiCondensation_Late { get; } = new GradePhaseEnum(11, GradeEnum.QiCondensation);
        public static GradePhaseEnum GoldenCore_Early1 { get; } = new GradePhaseEnum(12, GradeEnum.GoldenCore, BottleneckEnum.GoldenCore_Middle);
        public static GradePhaseEnum GoldenCore_Early2 { get; } = new GradePhaseEnum(13, GradeEnum.GoldenCore, BottleneckEnum.GoldenCore_Middle);
        public static GradePhaseEnum GoldenCore_Early3 { get; } = new GradePhaseEnum(14, GradeEnum.GoldenCore, BottleneckEnum.GoldenCore_Middle);
        public static GradePhaseEnum GoldenCore_Early4 { get; } = new GradePhaseEnum(15, GradeEnum.GoldenCore, BottleneckEnum.GoldenCore_Middle);
        public static GradePhaseEnum GoldenCore_Early5 { get; } = new GradePhaseEnum(16, GradeEnum.GoldenCore, BottleneckEnum.GoldenCore_Middle);
        public static GradePhaseEnum GoldenCore_Middle { get; } = new GradePhaseEnum(17, GradeEnum.GoldenCore, BottleneckEnum.GoldenCore_Late);
        public static GradePhaseEnum GoldenCore_Late { get; } = new GradePhaseEnum(18, GradeEnum.GoldenCore);
        public static GradePhaseEnum OriginSpirit_Early { get; } = new GradePhaseEnum(19, GradeEnum.OriginSpirit, BottleneckEnum.OriginSpirit_Middle);
        public static GradePhaseEnum OriginSpirit_Middle { get; } = new GradePhaseEnum(20, GradeEnum.OriginSpirit, BottleneckEnum.OriginSpirit_Late);
        public static GradePhaseEnum OriginSpirit_Late { get; } = new GradePhaseEnum(21, GradeEnum.OriginSpirit);
        public static GradePhaseEnum NascentSoul_Early1 { get; } = new GradePhaseEnum(22, GradeEnum.NascentSoul, BottleneckEnum.NascentSoul_Middle);
        public static GradePhaseEnum NascentSoul_Early2 { get; } = new GradePhaseEnum(23, GradeEnum.NascentSoul, BottleneckEnum.NascentSoul_Middle);
        public static GradePhaseEnum NascentSoul_Early3 { get; } = new GradePhaseEnum(24, GradeEnum.NascentSoul, BottleneckEnum.NascentSoul_Middle);
        public static GradePhaseEnum NascentSoul_Middle { get; } = new GradePhaseEnum(25, GradeEnum.NascentSoul, BottleneckEnum.NascentSoul_Late);
        public static GradePhaseEnum NascentSoul_Late { get; } = new GradePhaseEnum(26, GradeEnum.NascentSoul);
        public static GradePhaseEnum SoulFormation_Early { get; } = new GradePhaseEnum(27, GradeEnum.SoulFormation, BottleneckEnum.SoulFormation_Middle);
        public static GradePhaseEnum SoulFormation_Middle { get; } = new GradePhaseEnum(28, GradeEnum.SoulFormation, BottleneckEnum.SoulFormation_Late);
        public static GradePhaseEnum SoulFormation_Late { get; } = new GradePhaseEnum(29, GradeEnum.SoulFormation);
        public static GradePhaseEnum Enlightenment_Early1 { get; } = new GradePhaseEnum(30, GradeEnum.Enlightenment, BottleneckEnum.Enlightenment_Middle);
        public static GradePhaseEnum Enlightenment_Early2 { get; } = new GradePhaseEnum(31, GradeEnum.Enlightenment, BottleneckEnum.Enlightenment_Middle);
        public static GradePhaseEnum Enlightenment_Early3 { get; } = new GradePhaseEnum(32, GradeEnum.Enlightenment, BottleneckEnum.Enlightenment_Middle);
        public static GradePhaseEnum Enlightenment_Early4 { get; } = new GradePhaseEnum(33, GradeEnum.Enlightenment, BottleneckEnum.Enlightenment_Middle);
        public static GradePhaseEnum Enlightenment_Middle { get; } = new GradePhaseEnum(34, GradeEnum.Enlightenment, BottleneckEnum.Enlightenment_Late);
        public static GradePhaseEnum Enlightenment_Late { get; } = new GradePhaseEnum(35, GradeEnum.Enlightenment);
        public static GradePhaseEnum Reborn_Early { get; } = new GradePhaseEnum(36, GradeEnum.Reborn, BottleneckEnum.Reborn_Middle);
        public static GradePhaseEnum Reborn_Middle { get; } = new GradePhaseEnum(37, GradeEnum.Reborn, BottleneckEnum.Reborn_Late);
        public static GradePhaseEnum Reborn_Late { get; } = new GradePhaseEnum(38, GradeEnum.Reborn);
        public static GradePhaseEnum Transendent_Early1 { get; } = new GradePhaseEnum(39, GradeEnum.Transendent, BottleneckEnum.Transcendent_Middle);
        public static GradePhaseEnum Transendent_Early2 { get; } = new GradePhaseEnum(40, GradeEnum.Transendent, BottleneckEnum.Transcendent_Middle);
        public static GradePhaseEnum Transendent_Early3 { get; } = new GradePhaseEnum(41, GradeEnum.Transendent, BottleneckEnum.Transcendent_Middle);
        public static GradePhaseEnum Transendent_Early4 { get; } = new GradePhaseEnum(42, GradeEnum.Transendent, BottleneckEnum.Transcendent_Middle);
        public static GradePhaseEnum Transendent_Middle { get; } = new GradePhaseEnum(43, GradeEnum.Transendent, BottleneckEnum.Transcendent_Late);
        public static GradePhaseEnum Transendent_Late { get; } = new GradePhaseEnum(44, GradeEnum.Transendent);

        public GradeEnum Grade { get; private set; }
        public BottleneckEnum Bottleneck { get; private set; }
        private GradePhaseEnum(int phaseId, GradeEnum grade, BottleneckEnum bottleneck = BottleneckEnum.None) : base(phaseId.ToString())
        {
            Grade = grade;
            Bottleneck = bottleneck;
        }
    }
}
