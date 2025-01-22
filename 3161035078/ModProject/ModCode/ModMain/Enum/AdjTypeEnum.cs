using MOD_nE7UL2.Mod;
using ModLib.Enum;
using ModLib.Object;
using System;

namespace MOD_nE7UL2.Enum
{
    public class AdjTypeEnum : EnumObject
    {
        public static AdjTypeEnum Atk { get; } = new AdjTypeEnum(GameTool.LS("state500010000desc"), "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.Attack));
        public static AdjTypeEnum Def { get; } = new AdjTypeEnum(GameTool.LS("state500010001desc"), "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.Defense));
        public static AdjTypeEnum MHp { get; } = new AdjTypeEnum(GameTool.LS("state500010002desc"), "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.HpMax));
        public static AdjTypeEnum MMp { get; } = new AdjTypeEnum(GameTool.LS("state500010003desc"), "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.MpMax));
        public static AdjTypeEnum MSp { get; } = new AdjTypeEnum(GameTool.LS("state500010004desc"), "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.SpMax));
        public static AdjTypeEnum RHp { get; } = new AdjTypeEnum(GameTool.LS("state500010005desc"), "0", (wunit) => BattleModifyEvent.GetHpRecoveryBase(wunit));
        public static AdjTypeEnum RMp { get; } = new AdjTypeEnum(GameTool.LS("state500010006desc"), "0", (wunit) => BattleModifyEvent.GetMpRecoveryBase(wunit));
        public static AdjTypeEnum RSp { get; } = new AdjTypeEnum(GameTool.LS("state500010007desc"), "0", (wunit) => BattleModifyEvent.GetSpRecoveryBase(wunit));
        public static AdjTypeEnum BasisBlade { get; } = new AdjTypeEnum(GameTool.LS("state500010008desc"), "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisBlade));
        public static AdjTypeEnum BasisEarth { get; } = new AdjTypeEnum(GameTool.LS("state500010009desc"), "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisEarth));
        public static AdjTypeEnum BasisFinger { get; } = new AdjTypeEnum(GameTool.LS("state500010010desc"), "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisFinger));
        public static AdjTypeEnum BasisFire { get; } = new AdjTypeEnum(GameTool.LS("state500010011desc"), "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisFire));
        public static AdjTypeEnum BasisFist { get; } = new AdjTypeEnum(GameTool.LS("state500010012desc"), "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisFist));
        public static AdjTypeEnum BasisFroze { get; } = new AdjTypeEnum(GameTool.LS("state500010013desc"), "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisFroze));
        public static AdjTypeEnum BasisPalm { get; } = new AdjTypeEnum(GameTool.LS("state500010014desc"), "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisPalm));
        public static AdjTypeEnum BasisSpear { get; } = new AdjTypeEnum(GameTool.LS("state500010015desc"), "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisSpear));
        public static AdjTypeEnum BasisSword { get; } = new AdjTypeEnum(GameTool.LS("state500010016desc"), "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisSword));
        public static AdjTypeEnum BasisThunder { get; } = new AdjTypeEnum(GameTool.LS("state500010017desc"), "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisThunder));
        public static AdjTypeEnum BasisWind { get; } = new AdjTypeEnum(GameTool.LS("state500010018desc"), "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisWind));
        public static AdjTypeEnum BasisWood { get; } = new AdjTypeEnum(GameTool.LS("state500010019desc"), "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.BasisWood));
        public static AdjTypeEnum Speed { get; } = new AdjTypeEnum(GameTool.LS("state500010020desc"), "0", (wunit) => wunit.GetProperty<int>(UnitPropertyEnum.MoveSpeed));
        public static AdjTypeEnum BlockChanceMax { get; } = new AdjTypeEnum("Block(%Max)", "0.00", (wunit) => BattleModifyEvent.GetBlockMaxBase(wunit));
        public static AdjTypeEnum BlockDmg { get; } = new AdjTypeEnum("Block(Dmg)", "0", (wunit) => BattleModifyEvent.GetBlockDmgBase(wunit));
        public static AdjTypeEnum EvadeChance { get; } = new AdjTypeEnum("Evade(%)", "0.00", (wunit) => BattleModifyEvent.GetEvadeBase(wunit));
        public static AdjTypeEnum EvadeChanceMax { get; } = new AdjTypeEnum("Evade(%Max)", "0.00", (wunit) => BattleModifyEvent.GetEvadeMaxBase(wunit));
        public static AdjTypeEnum Nullify { get; } = new AdjTypeEnum("Nullify", "0", (wunit) => 4 * wunit.GetGradeLvl());
        public static AdjTypeEnum Manashield { get; } = new AdjTypeEnum("Shield", "0", (wunit) => BattleManashieldEvent.GetManashieldBase(wunit));
        public static AdjTypeEnum SCritChance { get; } = new AdjTypeEnum("SCrit(%)", "0.00", (wunit) => BattleModifyEvent.GetSCritChanceBase(wunit));
        public static AdjTypeEnum SCritChanceMax { get; } = new AdjTypeEnum("SCrit(%Max)", "0.00", (wunit) => BattleModifyEvent.GetSCritChanceMaxBase(wunit));
        public static AdjTypeEnum SCritDamage { get; } = new AdjTypeEnum("SCrit(%Dmg)", "0.00", (wunit) => BattleModifyEvent.GetSCritDamageBase(wunit));
        public static AdjTypeEnum SkillDamage { get; } = new AdjTypeEnum("Skill(%Dmg)", "0.00", (wunit) => 100 + 10 * wunit.GetGradeLvl());
        public static AdjTypeEnum MinDamage { get; } = new AdjTypeEnum("Min-Dmg", "0", (wunit) => BattleModifyEvent.GetMinDmgBase(wunit));

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
