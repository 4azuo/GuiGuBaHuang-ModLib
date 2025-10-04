using MOD_nE7UL2.Mod;
using ModLib.Enum;
using ModLib.Object;
using System;

namespace MOD_nE7UL2.Enum
{
    public class AdjTypeEnum : EnumObject
    {
        //Formula: GetRefineCustommAdjValue
        //BaseValueFunc dont return 0
        public static AdjTypeEnum Atk { get; } = new AdjTypeEnum("adjType500010000", 1, "0", (wunit, optionalParams) => wunit.GetProperty<int>(UnitPropertyEnum.Attack));
        public static AdjTypeEnum Def { get; } = new AdjTypeEnum("adjType500010001", 1, "0", (wunit, optionalParams) => wunit.GetProperty<int>(UnitPropertyEnum.Defense));
        public static AdjTypeEnum MHp { get; } = new AdjTypeEnum("adjType500010002", 1, "0", (wunit, optionalParams) => wunit.GetProperty<int>(UnitPropertyEnum.HpMax));
        public static AdjTypeEnum MMp { get; } = new AdjTypeEnum("adjType500010003", 2, "0", (wunit, optionalParams) => wunit.GetProperty<int>(UnitPropertyEnum.MpMax));
        public static AdjTypeEnum MSp { get; } = new AdjTypeEnum("adjType500010004", 4, "0", (wunit, optionalParams) => wunit.GetProperty<int>(UnitPropertyEnum.SpMax));
        public static AdjTypeEnum MDp { get; } = new AdjTypeEnum("adjType500010037", 8, "0", (wunit, optionalParams) => (wunit.GetGradeLvl() + RebirthEvent.Instance.TotalGradeLvl) * 1);
        public static AdjTypeEnum RHp { get; } = new AdjTypeEnum("adjType500010005", 1, "0", (wunit, optionalParams) => BattleModifyEvent.GetHpRecoveryBase(wunit));
        public static AdjTypeEnum RMp { get; } = new AdjTypeEnum("adjType500010006", 3, "0", (wunit, optionalParams) => BattleModifyEvent.GetMpRecoveryBase(wunit));
        public static AdjTypeEnum RSp { get; } = new AdjTypeEnum("adjType500010007", 4, "0", (wunit, optionalParams) => (wunit.GetGradeLvl() + RebirthEvent.Instance.TotalGradeLvl) * 1);
        public static AdjTypeEnum RDp { get; } = new AdjTypeEnum("adjType500010038", 9, "0", (wunit, optionalParams) => (wunit.GetGradeLvl() + RebirthEvent.Instance.TotalGradeLvl) * 0.1);
        public static AdjTypeEnum BasisBlade { get; } = new AdjTypeEnum("adjType500010008", 1, "0", (wunit, optionalParams) => wunit.GetProperty<int>(UnitPropertyEnum.BasisBlade) * 0.1);
        public static AdjTypeEnum BasisEarth { get; } = new AdjTypeEnum("adjType500010009", 1, "0", (wunit, optionalParams) => wunit.GetProperty<int>(UnitPropertyEnum.BasisEarth) * 0.1);
        public static AdjTypeEnum BasisFinger { get; } = new AdjTypeEnum("adjType500010010", 1, "0", (wunit, optionalParams) => wunit.GetProperty<int>(UnitPropertyEnum.BasisFinger) * 0.1);
        public static AdjTypeEnum BasisFire { get; } = new AdjTypeEnum("adjType500010011", 1, "0", (wunit, optionalParams) => wunit.GetProperty<int>(UnitPropertyEnum.BasisFire) * 0.1);
        public static AdjTypeEnum BasisFist { get; } = new AdjTypeEnum("adjType500010012", 1, "0", (wunit, optionalParams) => wunit.GetProperty<int>(UnitPropertyEnum.BasisFist) * 0.1);
        public static AdjTypeEnum BasisFroze { get; } = new AdjTypeEnum("adjType500010013", 1, "0", (wunit, optionalParams) => wunit.GetProperty<int>(UnitPropertyEnum.BasisFroze) * 0.1);
        public static AdjTypeEnum BasisPalm { get; } = new AdjTypeEnum("adjType500010014", 1, "0", (wunit, optionalParams) => wunit.GetProperty<int>(UnitPropertyEnum.BasisPalm) * 0.1);
        public static AdjTypeEnum BasisSpear { get; } = new AdjTypeEnum("adjType500010015", 1, "0", (wunit, optionalParams) => wunit.GetProperty<int>(UnitPropertyEnum.BasisSpear) * 0.1);
        public static AdjTypeEnum BasisSword { get; } = new AdjTypeEnum("adjType500010016", 1, "0", (wunit, optionalParams) => wunit.GetProperty<int>(UnitPropertyEnum.BasisSword) * 0.1);
        public static AdjTypeEnum BasisThunder { get; } = new AdjTypeEnum("adjType500010017", 1, "0", (wunit, optionalParams) => wunit.GetProperty<int>(UnitPropertyEnum.BasisThunder) * 0.1);
        public static AdjTypeEnum BasisWind { get; } = new AdjTypeEnum("adjType500010018", 1, "0", (wunit, optionalParams) => wunit.GetProperty<int>(UnitPropertyEnum.BasisWind) * 0.1);
        public static AdjTypeEnum BasisWood { get; } = new AdjTypeEnum("adjType500010019", 1, "0", (wunit, optionalParams) => wunit.GetProperty<int>(UnitPropertyEnum.BasisWood) * 0.1);
        public static AdjTypeEnum Speed { get; } = new AdjTypeEnum("adjType500010020", 1, "0", (wunit, optionalParams) => wunit.GetProperty<int>(UnitPropertyEnum.MoveSpeed) * 0.1);
        public static AdjTypeEnum BlockChanceMax { get; } = new AdjTypeEnum("adjType500010021", 1, "0.000", (wunit, optionalParams) => BattleModifyEvent.GetBlockMaxBase(wunit));
        public static AdjTypeEnum BlockDmg { get; } = new AdjTypeEnum("adjType500010022", 1, "0", (wunit, optionalParams) => BattleModifyEvent.GetBlockDmgBase(wunit));
        public static AdjTypeEnum EvadeChance { get; } = new AdjTypeEnum("adjType500010023", 1, "0.000", (wunit, optionalParams) => BattleModifyEvent.GetEvadeBase(wunit));
        public static AdjTypeEnum EvadeChanceMax { get; } = new AdjTypeEnum("adjType500010024", 1, "0.000", (wunit, optionalParams) => BattleModifyEvent.GetEvadeMaxBase(wunit));
        public static AdjTypeEnum Nullify { get; } = new AdjTypeEnum("adjType500010025", 2, "0", (wunit, optionalParams) => 4 * wunit.GetGradeLvl());
        public static AdjTypeEnum Manashield { get; } = new AdjTypeEnum("adjType500010026", 1, "0", (wunit, optionalParams) => BattleManashieldEvent.GetManashieldBase(wunit));
        public static AdjTypeEnum SCritChance { get; } = new AdjTypeEnum("adjType500010027", 4, "0.000", (wunit, optionalParams) => BattleModifyEvent.GetSCritChanceBase(wunit));
        public static AdjTypeEnum SCritChanceMax { get; } = new AdjTypeEnum("adjType500010028", 5, "0.000", (wunit, optionalParams) => BattleModifyEvent.GetSCritChanceMaxBase(wunit));
        public static AdjTypeEnum SCritDamage { get; } = new AdjTypeEnum("adjType500010029", 5, "0.000", (wunit, optionalParams) => BattleModifyEvent.GetSCritDamageBase(wunit));
        public static AdjTypeEnum SkillDamage { get; } = new AdjTypeEnum("adjType500010030", 1, "0.000", (wunit, optionalParams) => 100 + 10 * wunit.GetGradeLvl());
        public static AdjTypeEnum MinDamage { get; } = new AdjTypeEnum("adjType500010031", 2, "0", (wunit, optionalParams) => BattleModifyEvent.GetMinDmgBase(wunit));
        public static AdjTypeEnum StealHp { get; } = new AdjTypeEnum("adjType500010032", 3, "0.000", (wunit, optionalParams) => wunit.GetGradeLvl() * 0.8);
        public static AdjTypeEnum StealMp { get; } = new AdjTypeEnum("adjType500010033", 4, "0.000", (wunit, optionalParams) => wunit.GetGradeLvl() * 0.4);
        public static AdjTypeEnum StealSp { get; } = new AdjTypeEnum("adjType500010034", 5, "0.000", (wunit, optionalParams) => wunit.GetGradeLvl() * 0.2);
        public static AdjTypeEnum InstantKill { get; } = new AdjTypeEnum("adjType500010035", 1, "0.000", (wunit, optionalParams) => wunit.GetGradeLvl() * 0.1);
        public static AdjTypeEnum SummonPower { get; } = new AdjTypeEnum("adjType500010036", 1, "0.000", (wunit, optionalParams) => wunit.GetGradeLvl() * 0.5);
        public static AdjTypeEnum ItemCD { get; } = new AdjTypeEnum("adjType500010039", 1, "0.000", (wunit, optionalParams) => (optionalParams ?? 0) * 0.01);
        public static AdjTypeEnum SkillCD { get; } = new AdjTypeEnum("adjType500010040", 3, "0.000", (wunit, optionalParams) => (optionalParams ?? 0) * 0.01);

        public string Label { get; private set; }
        public int MinGradeLvl { get; private set; }
        public string ValueFormat { get; private set; }
        public Func<WorldUnitBase, dynamic, double> BaseValueFunc { get; private set; }
        private AdjTypeEnum(string label, int minGradeLvl, string format, Func<WorldUnitBase, dynamic, double> baseValueFunc) : base(label)
        {
            Label = label;
            MinGradeLvl = minGradeLvl;
            ValueFormat = format;
            BaseValueFunc = baseValueFunc;
        }

        public double GetBaseValue(WorldUnitBase wunit, dynamic optionalParams = null)
        {
            if (BaseValueFunc == null)
                return 0;
            return BaseValueFunc.Invoke(wunit, optionalParams);
        }
    }
}
