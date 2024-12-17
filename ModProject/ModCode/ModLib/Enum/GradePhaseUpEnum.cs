using ModLib.Object;

namespace ModLib.Enum
{
    public class GradePhaseUpEnum : EnumObject
    {
        public static GradePhaseUpEnum QiRefining_Early { get; } = new GradePhaseUpEnum(GradePhaseEnum.QiRefining_Early, GradePhaseEnum.QiRefining_Middle);
        public static GradePhaseUpEnum QiRefining_Middle { get; } = new GradePhaseUpEnum(GradePhaseEnum.QiRefining_Middle, GradePhaseEnum.QiRefining_Late);
        public static GradePhaseUpEnum QiRefining_Late { get; } = new GradePhaseUpEnum(GradePhaseEnum.QiRefining_Late, GradePhaseEnum.Foundation_Early1);
        public static GradePhaseUpEnum Foundation_Early1 { get; } = new GradePhaseUpEnum(GradePhaseEnum.Foundation_Early1, GradePhaseEnum.Foundation_Middle);
        public static GradePhaseUpEnum Foundation_Early2 { get; } = new GradePhaseUpEnum(GradePhaseEnum.Foundation_Early2, GradePhaseEnum.Foundation_Middle);
        public static GradePhaseUpEnum Foundation_Early3 { get; } = new GradePhaseUpEnum(GradePhaseEnum.Foundation_Early3, GradePhaseEnum.Foundation_Middle);
        public static GradePhaseUpEnum Foundation_Middle { get; } = new GradePhaseUpEnum(GradePhaseEnum.Foundation_Middle, GradePhaseEnum.Foundation_Late);
        public static GradePhaseUpEnum Foundation_Late { get; } = new GradePhaseUpEnum(GradePhaseEnum.Foundation_Late, GradePhaseEnum.QiCondensation_Early);
        public static GradePhaseUpEnum QiCondensation_Early { get; } = new GradePhaseUpEnum(GradePhaseEnum.QiCondensation_Early, GradePhaseEnum.QiCondensation_Middle);
        public static GradePhaseUpEnum QiCondensation_Middle { get; } = new GradePhaseUpEnum(GradePhaseEnum.QiCondensation_Middle, GradePhaseEnum.QiCondensation_Late);
        public static GradePhaseUpEnum QiCondensation_Late { get; } = new GradePhaseUpEnum(GradePhaseEnum.QiCondensation_Late, GradePhaseEnum.GoldenCore_Early1);
        public static GradePhaseUpEnum GoldenCore_Early1 { get; } = new GradePhaseUpEnum(GradePhaseEnum.GoldenCore_Early1, GradePhaseEnum.GoldenCore_Middle);
        public static GradePhaseUpEnum GoldenCore_Early2 { get; } = new GradePhaseUpEnum(GradePhaseEnum.GoldenCore_Early2, GradePhaseEnum.GoldenCore_Middle);
        public static GradePhaseUpEnum GoldenCore_Early3 { get; } = new GradePhaseUpEnum(GradePhaseEnum.GoldenCore_Early3, GradePhaseEnum.GoldenCore_Middle);
        public static GradePhaseUpEnum GoldenCore_Early4 { get; } = new GradePhaseUpEnum(GradePhaseEnum.GoldenCore_Early4, GradePhaseEnum.GoldenCore_Middle);
        public static GradePhaseUpEnum GoldenCore_Early5 { get; } = new GradePhaseUpEnum(GradePhaseEnum.GoldenCore_Early5, GradePhaseEnum.GoldenCore_Middle);
        public static GradePhaseUpEnum GoldenCore_Middle { get; } = new GradePhaseUpEnum(GradePhaseEnum.GoldenCore_Middle, GradePhaseEnum.GoldenCore_Late);
        public static GradePhaseUpEnum GoldenCore_Late { get; } = new GradePhaseUpEnum(GradePhaseEnum.GoldenCore_Late, GradePhaseEnum.OriginSpirit_Early);
        public static GradePhaseUpEnum OriginSpirit_Early { get; } = new GradePhaseUpEnum(GradePhaseEnum.OriginSpirit_Early, GradePhaseEnum.OriginSpirit_Middle);
        public static GradePhaseUpEnum OriginSpirit_Middle { get; } = new GradePhaseUpEnum(GradePhaseEnum.OriginSpirit_Middle, GradePhaseEnum.OriginSpirit_Late);
        public static GradePhaseUpEnum OriginSpirit_Late { get; } = new GradePhaseUpEnum(GradePhaseEnum.OriginSpirit_Late, GradePhaseEnum.NascentSoul_Early1);
        public static GradePhaseUpEnum NascentSoul_Early1 { get; } = new GradePhaseUpEnum(GradePhaseEnum.NascentSoul_Early1, GradePhaseEnum.NascentSoul_Middle);
        public static GradePhaseUpEnum NascentSoul_Early2 { get; } = new GradePhaseUpEnum(GradePhaseEnum.NascentSoul_Early2, GradePhaseEnum.NascentSoul_Middle);
        public static GradePhaseUpEnum NascentSoul_Early3 { get; } = new GradePhaseUpEnum(GradePhaseEnum.NascentSoul_Early3, GradePhaseEnum.NascentSoul_Middle);
        public static GradePhaseUpEnum NascentSoul_Middle { get; } = new GradePhaseUpEnum(GradePhaseEnum.NascentSoul_Middle, GradePhaseEnum.NascentSoul_Late);
        public static GradePhaseUpEnum NascentSoul_Late { get; } = new GradePhaseUpEnum(GradePhaseEnum.NascentSoul_Late, GradePhaseEnum.SoulFormation_Early);
        public static GradePhaseUpEnum SoulFormation_Early { get; } = new GradePhaseUpEnum(GradePhaseEnum.SoulFormation_Early, GradePhaseEnum.SoulFormation_Middle);
        public static GradePhaseUpEnum SoulFormation_Middle { get; } = new GradePhaseUpEnum(GradePhaseEnum.SoulFormation_Middle, GradePhaseEnum.SoulFormation_Late);
        public static GradePhaseUpEnum SoulFormation_Late { get; } = new GradePhaseUpEnum(GradePhaseEnum.SoulFormation_Late, GradePhaseEnum.Enlightenment_Early1);
        public static GradePhaseUpEnum Enlightenment_Early1 { get; } = new GradePhaseUpEnum(GradePhaseEnum.Enlightenment_Early1, GradePhaseEnum.Enlightenment_Middle);
        public static GradePhaseUpEnum Enlightenment_Early2 { get; } = new GradePhaseUpEnum(GradePhaseEnum.Enlightenment_Early2, GradePhaseEnum.Enlightenment_Middle);
        public static GradePhaseUpEnum Enlightenment_Early3 { get; } = new GradePhaseUpEnum(GradePhaseEnum.Enlightenment_Early3, GradePhaseEnum.Enlightenment_Middle);
        public static GradePhaseUpEnum Enlightenment_Early4 { get; } = new GradePhaseUpEnum(GradePhaseEnum.Enlightenment_Early4, GradePhaseEnum.Enlightenment_Middle);
        public static GradePhaseUpEnum Enlightenment_Middle { get; } = new GradePhaseUpEnum(GradePhaseEnum.Enlightenment_Middle, GradePhaseEnum.Enlightenment_Late);
        public static GradePhaseUpEnum Enlightenment_Late { get; } = new GradePhaseUpEnum(GradePhaseEnum.Enlightenment_Late, GradePhaseEnum.Reborn_Early);
        public static GradePhaseUpEnum Reborn_Early { get; } = new GradePhaseUpEnum(GradePhaseEnum.Reborn_Early, GradePhaseEnum.Reborn_Middle);
        public static GradePhaseUpEnum Reborn_Middle { get; } = new GradePhaseUpEnum(GradePhaseEnum.Reborn_Middle, GradePhaseEnum.Reborn_Late);
        public static GradePhaseUpEnum Reborn_Late { get; } = new GradePhaseUpEnum(GradePhaseEnum.Reborn_Late, GradePhaseEnum.Transendent_Early1);
        public static GradePhaseUpEnum Transendent_Early1 { get; } = new GradePhaseUpEnum(GradePhaseEnum.Transendent_Early1, GradePhaseEnum.Transendent_Middle);
        public static GradePhaseUpEnum Transendent_Early2 { get; } = new GradePhaseUpEnum(GradePhaseEnum.Transendent_Early2, GradePhaseEnum.Transendent_Middle);
        public static GradePhaseUpEnum Transendent_Early3 { get; } = new GradePhaseUpEnum(GradePhaseEnum.Transendent_Early3, GradePhaseEnum.Transendent_Middle);
        public static GradePhaseUpEnum Transendent_Early4 { get; } = new GradePhaseUpEnum(GradePhaseEnum.Transendent_Early4, GradePhaseEnum.Transendent_Middle);
        public static GradePhaseUpEnum Transendent_Middle { get; } = new GradePhaseUpEnum(GradePhaseEnum.Transendent_Middle, GradePhaseEnum.Transendent_Late);
        public static GradePhaseUpEnum Transendent_Late { get; } = new GradePhaseUpEnum(GradePhaseEnum.Transendent_Late, null);

        public GradePhaseEnum Phase { get; private set; }
        public GradePhaseEnum NextPhase { get; private set; }
        private GradePhaseUpEnum(GradePhaseEnum phase, GradePhaseEnum nextPhase) : base(phase.Value)
        {
            Phase = phase;
            NextPhase = nextPhase;
        }
    }
}
