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
        public static AdjTypeEnum Atk { get; } = new AdjTypeEnum("adjType500010000", "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.Attack));
        public static AdjTypeEnum Def { get; } = new AdjTypeEnum("adjType500010001", "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.Defense));
        public static AdjTypeEnum MHp { get; } = new AdjTypeEnum("adjType500010002", "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.HpMax));
        public static AdjTypeEnum MMp { get; } = new AdjTypeEnum("adjType500010003", "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.MpMax));
        public static AdjTypeEnum MSp { get; } = new AdjTypeEnum("adjType500010004", "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.SpMax));
        public static AdjTypeEnum RHp { get; } = new AdjTypeEnum("adjType500010005", "0", (wunit) => BattleModifyEvent.GetHpRecoveryBase(wunit));
        public static AdjTypeEnum RMp { get; } = new AdjTypeEnum("adjType500010006", "0", (wunit) => BattleModifyEvent.GetMpRecoveryBase(wunit));
        public static AdjTypeEnum RSp { get; } = new AdjTypeEnum("adjType500010007", "0", (wunit) => 1 * wunit.GetGradeLvl());
        public static AdjTypeEnum BasisBlade { get; } = new AdjTypeEnum("adjType500010008", "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisBlade));
        public static AdjTypeEnum BasisEarth { get; } = new AdjTypeEnum("adjType500010009", "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisEarth));
        public static AdjTypeEnum BasisFinger { get; } = new AdjTypeEnum("adjType500010010", "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisFinger));
        public static AdjTypeEnum BasisFire { get; } = new AdjTypeEnum("adjType500010011", "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisFire));
        public static AdjTypeEnum BasisFist { get; } = new AdjTypeEnum("adjType500010012", "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisFist));
        public static AdjTypeEnum BasisFroze { get; } = new AdjTypeEnum("adjType500010013", "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisFroze));
        public static AdjTypeEnum BasisPalm { get; } = new AdjTypeEnum("adjType500010014", "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisPalm));
        public static AdjTypeEnum BasisSpear { get; } = new AdjTypeEnum("adjType500010015", "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisSpear));
        public static AdjTypeEnum BasisSword { get; } = new AdjTypeEnum("adjType500010016", "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisSword));
        public static AdjTypeEnum BasisThunder { get; } = new AdjTypeEnum("adjType500010017", "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisThunder));
        public static AdjTypeEnum BasisWind { get; } = new AdjTypeEnum("adjType500010018", "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisWind));
        public static AdjTypeEnum BasisWood { get; } = new AdjTypeEnum("adjType500010019", "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisWood));
        public static AdjTypeEnum Speed { get; } = new AdjTypeEnum("adjType500010020", "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.MoveSpeed));
        public static AdjTypeEnum BlockChanceMax { get; } = new AdjTypeEnum("adjType500010021", "0.000", (wunit) => BattleModifyEvent.GetBlockMaxBase(wunit));
        public static AdjTypeEnum BlockDmg { get; } = new AdjTypeEnum("adjType500010022", "0", (wunit) => BattleModifyEvent.GetBlockDmgBase(wunit));
        public static AdjTypeEnum EvadeChance { get; } = new AdjTypeEnum("adjType500010023", "0.000", (wunit) => BattleModifyEvent.GetEvadeBase(wunit));
        public static AdjTypeEnum EvadeChanceMax { get; } = new AdjTypeEnum("adjType500010024", "0.000", (wunit) => BattleModifyEvent.GetEvadeMaxBase(wunit));
        public static AdjTypeEnum Nullify { get; } = new AdjTypeEnum("adjType500010025", "0", (wunit) => 4 * wunit.GetGradeLvl());
        public static AdjTypeEnum Manashield { get; } = new AdjTypeEnum("adjType500010026", "0", (wunit) => BattleManashieldEvent.GetManashieldBase(wunit));
        public static AdjTypeEnum SCritChance { get; } = new AdjTypeEnum("adjType500010027", "0.000", (wunit) => BattleModifyEvent.GetSCritChanceBase(wunit));
        public static AdjTypeEnum SCritChanceMax { get; } = new AdjTypeEnum("adjType500010028", "0.000", (wunit) => BattleModifyEvent.GetSCritChanceMaxBase(wunit));
        public static AdjTypeEnum SCritDamage { get; } = new AdjTypeEnum("adjType500010029", "0.000", (wunit) => BattleModifyEvent.GetSCritDamageBase(wunit));
        public static AdjTypeEnum SkillDamage { get; } = new AdjTypeEnum("adjType500010030", "0.000", (wunit) => 100 + 10 * wunit.GetGradeLvl());
        public static AdjTypeEnum MinDamage { get; } = new AdjTypeEnum("adjType500010031", "0", (wunit) => BattleModifyEvent.GetMinDmgBase(wunit));
        public static AdjTypeEnum StealHp { get; } = new AdjTypeEnum("adjType500010032", "0.000", (wunit) => 0.8 * wunit.GetGradeLvl());
        public static AdjTypeEnum StealMp { get; } = new AdjTypeEnum("adjType500010033", "0.000", (wunit) => 0.4 * wunit.GetGradeLvl());
        public static AdjTypeEnum StealSp { get; } = new AdjTypeEnum("adjType500010034", "0.000", (wunit) => 0.2 * wunit.GetGradeLvl());
        public static AdjTypeEnum InstantKill { get; } = new AdjTypeEnum("adjType500010035", "0.000", (wunit) => 0.1 * wunit.GetGradeLvl());

        public string Label { get; private set; }
        public string ValueFormat { get; private set; }
        public Func<WorldUnitBase, double> BaseValueFunc { get; private set; }
        private AdjTypeEnum(string label, string format, Func<WorldUnitBase, double> baseValueFunc) : base(label)
        {
            Label = label;
            ValueFormat = format;
            BaseValueFunc = baseValueFunc;
        }

        public double GetBaseValue(WorldUnitBase wunit)
        {
            if (BaseValueFunc == null)
                return 0;
            return BaseValueFunc.Invoke(wunit);
        }
    }
}
