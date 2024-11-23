using MOD_nE7UL2.Mod;
using ModLib.Enum;
using ModLib.Object;
using System;

namespace MOD_nE7UL2.Enum
{
    public class AdjTypeEnum : EnumObject
    {
        public static AdjTypeEnum None { get; } = new AdjTypeEnum(null, null);
        public static AdjTypeEnum Atk { get; } = new AdjTypeEnum("Atk", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.Attack));
        public static AdjTypeEnum Def { get; } = new AdjTypeEnum("Def", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.Defense));
        public static AdjTypeEnum MHp { get; } = new AdjTypeEnum("Hp", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.HpMax));
        public static AdjTypeEnum MMp { get; } = new AdjTypeEnum("Mp", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.MpMax));
        public static AdjTypeEnum MSp { get; } = new AdjTypeEnum("Sp", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.SpMax));
        public static AdjTypeEnum RHp { get; } = new AdjTypeEnum("Heal Hp", (wunit) => BattleModifyEvent.GetHpRecoveryBase(wunit));
        public static AdjTypeEnum RMp { get; } = new AdjTypeEnum("Heal Mp", (wunit) => BattleModifyEvent.GetMpRecoveryBase(wunit));
        public static AdjTypeEnum RSp { get; } = new AdjTypeEnum("Heal Sp", (wunit) => BattleModifyEvent.GetSpRecoveryBase(wunit));
        public static AdjTypeEnum BasisBlade { get; } = new AdjTypeEnum("Blade", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisBlade));
        public static AdjTypeEnum BasisEarth { get; } = new AdjTypeEnum("Earth", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisEarth));
        public static AdjTypeEnum BasisFinger { get; } = new AdjTypeEnum("Finger", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisFinger));
        public static AdjTypeEnum BasisFire { get; } = new AdjTypeEnum("Fire", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisFire));
        public static AdjTypeEnum BasisFist { get; } = new AdjTypeEnum("Fist", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisFist));
        public static AdjTypeEnum BasisFroze { get; } = new AdjTypeEnum("Froze", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisFroze));
        public static AdjTypeEnum BasisPalm { get; } = new AdjTypeEnum("Palm", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisPalm));
        public static AdjTypeEnum BasisSpear { get; } = new AdjTypeEnum("Spear", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisSpear));
        public static AdjTypeEnum BasisSword { get; } = new AdjTypeEnum("Sword", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisSword));
        public static AdjTypeEnum BasisThunder { get; } = new AdjTypeEnum("Thunder", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisThunder));
        public static AdjTypeEnum BasisWind { get; } = new AdjTypeEnum("Wind", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisWind));
        public static AdjTypeEnum BasisWood { get; } = new AdjTypeEnum("Wood", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisWood));
        public static AdjTypeEnum Speed { get; } = new AdjTypeEnum("Speed", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.MoveSpeed));
        public static AdjTypeEnum BlockChanceMax { get; } = new AdjTypeEnum("Block(%Max)", (wunit) => BattleModifyEvent.GetBlockMaxBase(wunit));
        public static AdjTypeEnum BlockDmg { get; } = new AdjTypeEnum("Block(Dmg)", (wunit) => BattleModifyEvent.GetBlockDmgBase(wunit));
        public static AdjTypeEnum EvadeChance { get; } = new AdjTypeEnum("Evade(%)", (wunit) => BattleModifyEvent.GetEvadeBase(wunit));
        public static AdjTypeEnum EvadeChanceMax { get; } = new AdjTypeEnum("Evade(%Max)", (wunit) => BattleModifyEvent.GetEvadeMaxBase(wunit));
        public static AdjTypeEnum Nullify { get; } = new AdjTypeEnum("Nullify", (wunit) => 5);
        public static AdjTypeEnum Shield { get; } = new AdjTypeEnum("Shield", (wunit) => BattleManashieldEvent.GetManashieldBase(wunit));
        public static AdjTypeEnum SCritChance { get; } = new AdjTypeEnum("SCrit(%)", (wunit) => BattleModifyEvent.GetSCritChanceBase(wunit));
        public static AdjTypeEnum SCritChanceMax { get; } = new AdjTypeEnum("SCrit(%Max)", (wunit) => BattleModifyEvent.GetSCritChanceMaxBase(wunit));
        public static AdjTypeEnum SCritDamage { get; } = new AdjTypeEnum("SCrit(%Dmg)", (wunit) => BattleModifyEvent.GetSCritDamageBase(wunit));
        public static AdjTypeEnum SkillDamage { get; } = new AdjTypeEnum("Skill(%Dmg)", (wunit) => 100);
        public static AdjTypeEnum MinDamage { get; } = new AdjTypeEnum("Min-Dmg", (wunit) => 10);

        public string Label { get; private set; }
        public Func<WorldUnitBase, double> BaseValueFunc { get; private set; }
        private AdjTypeEnum(string label, Func<WorldUnitBase, double> baseValueFunc) : base()
        {
            Label = label;
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
