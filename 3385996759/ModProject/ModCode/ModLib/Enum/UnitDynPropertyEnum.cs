using ModLib.Object;
using System;
using UnhollowerBaseLib;

namespace ModLib.Enum
{
    public class UnitDynPropertyEnum : EnumObject
    {
        //public static UnitDynPropertyEnum Example { get; } = new UnitDynPropertyEnum("example", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "example"));
        public static UnitDynPropertyEnum Sex { get; } = new UnitDynPropertyEnum("sex");
        public static UnitDynPropertyEnum Race { get; } = new UnitDynPropertyEnum("race");
        public static UnitDynPropertyEnum Age { get; } = new UnitDynPropertyEnum("age");
        public static UnitDynPropertyEnum Life { get; } = new UnitDynPropertyEnum("life");
        public static UnitDynPropertyEnum Beauty { get; } = new UnitDynPropertyEnum("beauty");
        public static UnitDynPropertyEnum StandUp { get; } = new UnitDynPropertyEnum("standUp");
        public static UnitDynPropertyEnum StandDown { get; } = new UnitDynPropertyEnum("standDown");
        public static UnitDynPropertyEnum Reputation { get; } = new UnitDynPropertyEnum("reputation");
        public static UnitDynPropertyEnum GradeID { get; } = new UnitDynPropertyEnum("gradeID");
        public static UnitDynPropertyEnum InTrait { get; } = new UnitDynPropertyEnum("inTrait");
        public static UnitDynPropertyEnum OutTrait1 { get; } = new UnitDynPropertyEnum("outTrait1");
        public static UnitDynPropertyEnum OutTrait2 { get; } = new UnitDynPropertyEnum("outTrait2");
        public static UnitDynPropertyEnum Hp { get; } = new UnitDynPropertyEnum("hp");
        public static UnitDynPropertyEnum HpMax { get; } = new UnitDynPropertyEnum("hpMax");
        public static UnitDynPropertyEnum HpRestore { get; } = new UnitDynPropertyEnum("hpRestore");
        public static UnitDynPropertyEnum Mp { get; } = new UnitDynPropertyEnum("mp");
        public static UnitDynPropertyEnum MpMax { get; } = new UnitDynPropertyEnum("mpMax");
        public static UnitDynPropertyEnum MpRestore { get; } = new UnitDynPropertyEnum("mpRestore");
        public static UnitDynPropertyEnum Sp { get; } = new UnitDynPropertyEnum("sp");
        public static UnitDynPropertyEnum SpMax { get; } = new UnitDynPropertyEnum("spMax");
        public static UnitDynPropertyEnum Dp { get; } = new UnitDynPropertyEnum("dp");
        public static UnitDynPropertyEnum DpMax { get; } = new UnitDynPropertyEnum("dpMax");
        public static UnitDynPropertyEnum SpRestore { get; } = new UnitDynPropertyEnum("spRestore");
        public static UnitDynPropertyEnum Attack { get; } = new UnitDynPropertyEnum("attack");
        public static UnitDynPropertyEnum Defense { get; } = new UnitDynPropertyEnum("defense");
        public static UnitDynPropertyEnum Crit { get; } = new UnitDynPropertyEnum("crit");
        public static UnitDynPropertyEnum CritValue { get; } = new UnitDynPropertyEnum("critValue");
        public static UnitDynPropertyEnum Guard { get; } = new UnitDynPropertyEnum("guard");
        public static UnitDynPropertyEnum GuardValue { get; } = new UnitDynPropertyEnum("guardValue");
        public static UnitDynPropertyEnum MoveSpeed { get; } = new UnitDynPropertyEnum("moveSpeed");
        public static UnitDynPropertyEnum PhycicalFree { get; } = new UnitDynPropertyEnum("phycicalFree");
        public static UnitDynPropertyEnum MagicFree { get; } = new UnitDynPropertyEnum("magicFree");
        public static UnitDynPropertyEnum Exp { get; } = new UnitDynPropertyEnum("exp");
        public static UnitDynPropertyEnum BasisFire { get; } = new UnitDynPropertyEnum("basisFire");
        public static UnitDynPropertyEnum BasisFroze { get; } = new UnitDynPropertyEnum("basisFroze");
        public static UnitDynPropertyEnum BasisThunder { get; } = new UnitDynPropertyEnum("basisThunder");
        public static UnitDynPropertyEnum BasisWind { get; } = new UnitDynPropertyEnum("basisWind");
        public static UnitDynPropertyEnum BasisEarth { get; } = new UnitDynPropertyEnum("basisEarth");
        public static UnitDynPropertyEnum BasisWood { get; } = new UnitDynPropertyEnum("basisWood");
        public static UnitDynPropertyEnum BasisBlade { get; } = new UnitDynPropertyEnum("basisBlade");
        public static UnitDynPropertyEnum BasisSpear { get; } = new UnitDynPropertyEnum("basisSpear");
        public static UnitDynPropertyEnum BasisSword { get; } = new UnitDynPropertyEnum("basisSword");
        public static UnitDynPropertyEnum BasisFist { get; } = new UnitDynPropertyEnum("basisFist");
        public static UnitDynPropertyEnum BasisPalm { get; } = new UnitDynPropertyEnum("basisPalm");
        public static UnitDynPropertyEnum BasisFinger { get; } = new UnitDynPropertyEnum("basisFinger");
        public static UnitDynPropertyEnum AbilityPoint { get; } = new UnitDynPropertyEnum("abilityPoint");
        public static UnitDynPropertyEnum Talent { get; } = new UnitDynPropertyEnum("talent");
        public static UnitDynPropertyEnum FootSpeed { get; } = new UnitDynPropertyEnum("footSpeed");
        public static UnitDynPropertyEnum Luck { get; } = new UnitDynPropertyEnum("luck");
        public static UnitDynPropertyEnum AbilityExp { get; } = new UnitDynPropertyEnum("abilityExp");
        public static UnitDynPropertyEnum ImmortalPoint { get; } = new UnitDynPropertyEnum("immortalPoint");
        public static UnitDynPropertyEnum RefineElixir { get; } = new UnitDynPropertyEnum("refineElixir");
        public static UnitDynPropertyEnum RefineWeapon { get; } = new UnitDynPropertyEnum("refineWeapon");
        public static UnitDynPropertyEnum Geomancy { get; } = new UnitDynPropertyEnum("geomancy");
        public static UnitDynPropertyEnum Symbol { get; } = new UnitDynPropertyEnum("symbol");
        public static UnitDynPropertyEnum Herbal { get; } = new UnitDynPropertyEnum("herbal");
        public static UnitDynPropertyEnum Mine { get; } = new UnitDynPropertyEnum("mine");
        public static UnitDynPropertyEnum Health { get; } = new UnitDynPropertyEnum("health");
        public static UnitDynPropertyEnum HealthMax { get; } = new UnitDynPropertyEnum("healthMax");
        public static UnitDynPropertyEnum Energy { get; } = new UnitDynPropertyEnum("energy");
        public static UnitDynPropertyEnum EnergyMax { get; } = new UnitDynPropertyEnum("energyMax");
        public static UnitDynPropertyEnum Mood { get; } = new UnitDynPropertyEnum("mood");
        public static UnitDynPropertyEnum MoodMax { get; } = new UnitDynPropertyEnum("moodMax");
        public static UnitDynPropertyEnum HealthUpRate { get; } = new UnitDynPropertyEnum("healthUpRate");
        public static UnitDynPropertyEnum HealthDownRate { get; } = new UnitDynPropertyEnum("healthDownRate");
        public static UnitDynPropertyEnum EnergyUpRate { get; } = new UnitDynPropertyEnum("energyUpRate");
        public static UnitDynPropertyEnum EnergyDownRate { get; } = new UnitDynPropertyEnum("energyDownRate");
        public static UnitDynPropertyEnum MoodUpRate { get; } = new UnitDynPropertyEnum("moodUpRate");
        public static UnitDynPropertyEnum MoodDownRate { get; } = new UnitDynPropertyEnum("moodDownRate");
        public static UnitDynPropertyEnum IntimRate { get; } = new UnitDynPropertyEnum("intimRate");
        public static UnitDynPropertyEnum HateRate { get; } = new UnitDynPropertyEnum("hateRate");
        public static UnitDynPropertyEnum ExpGrowRate { get; } = new UnitDynPropertyEnum("expGrowRate");
        public static UnitDynPropertyEnum TownBuyDeclineRate { get; } = new UnitDynPropertyEnum("townBuyDeclineRate");
        public static UnitDynPropertyEnum SchoolBuyDeclineRate { get; } = new UnitDynPropertyEnum("schoolBuyDeclineRate");
        public static UnitDynPropertyEnum StepGrowRate { get; } = new UnitDynPropertyEnum("stepGrowRate");
        public static UnitDynPropertyEnum LockInitScore { get; } = new UnitDynPropertyEnum("lockInitScore");
        public static UnitDynPropertyEnum PropGridNum { get; } = new UnitDynPropertyEnum("propGridNum");
        public static UnitDynPropertyEnum PropEquipNum { get; } = new UnitDynPropertyEnum("propEquipNum");
        public static UnitDynPropertyEnum HpUpRate { get; } = new UnitDynPropertyEnum("hpUpRate");
        public static UnitDynPropertyEnum HpDownRate { get; } = new UnitDynPropertyEnum("hpDownRate");
        public static UnitDynPropertyEnum MpUpRate { get; } = new UnitDynPropertyEnum("mpUpRate");
        public static UnitDynPropertyEnum MpDownRate { get; } = new UnitDynPropertyEnum("mpDownRate");
        public static UnitDynPropertyEnum SpUpRate { get; } = new UnitDynPropertyEnum("spUpRate");
        public static UnitDynPropertyEnum SpDownRate { get; } = new UnitDynPropertyEnum("spDownRate");
        public static UnitDynPropertyEnum SwordGrowRate { get; } = new UnitDynPropertyEnum("swordGrowRate");
        public static UnitDynPropertyEnum SpearGrowRate { get; } = new UnitDynPropertyEnum("spearGrowRate");
        public static UnitDynPropertyEnum BladeGrowRate { get; } = new UnitDynPropertyEnum("bladeGrowRate");
        public static UnitDynPropertyEnum FistGrowRate { get; } = new UnitDynPropertyEnum("fistGrowRate");
        public static UnitDynPropertyEnum PalmGrowRate { get; } = new UnitDynPropertyEnum("palmGrowRate");
        public static UnitDynPropertyEnum FingerGrowRate { get; } = new UnitDynPropertyEnum("fingerGrowRate");
        public static UnitDynPropertyEnum FireGrowRate { get; } = new UnitDynPropertyEnum("fireGrowRate");
        public static UnitDynPropertyEnum FrozeGrowRate { get; } = new UnitDynPropertyEnum("frozeGrowRate");
        public static UnitDynPropertyEnum ThunderGrowRate { get; } = new UnitDynPropertyEnum("thunderGrowRate");
        public static UnitDynPropertyEnum WindGrowRate { get; } = new UnitDynPropertyEnum("windGrowRate");
        public static UnitDynPropertyEnum EarthGrowRate { get; } = new UnitDynPropertyEnum("earthGrowRate");
        public static UnitDynPropertyEnum WoodGrowRate { get; } = new UnitDynPropertyEnum("woodGrowRate");
        public static UnitDynPropertyEnum PlayerView { get; } = new UnitDynPropertyEnum("playerView");
        //public static UnitDynPropertyEnum IsHide { get; } = new UnitDynPropertyEnum("isHide");
        //public static UnitDynPropertyEnum UnitData { get; } = new UnitDynPropertyEnum("unitData");
        //public static UnitDynPropertyEnum BattleModelData { get; } = new UnitDynPropertyEnum("battleModelData");
        //public static UnitDynPropertyEnum ModelData { get; } = new UnitDynPropertyEnum("modelData");
        //public static UnitDynPropertyEnum ArrayValuesList { get; } = new UnitDynPropertyEnum("arrayValuesList");
        //public static UnitDynPropertyEnum DynIntTab { get; } = new UnitDynPropertyEnum("dynIntTab");
        //public static UnitDynPropertyEnum AllBasWp { get; } = new UnitDynPropertyEnum("allBasWp");
        //public static UnitDynPropertyEnum AllBasMg { get; } = new UnitDynPropertyEnum("allBasMg");
        //public static UnitDynPropertyEnum CurGrade { get; } = new UnitDynPropertyEnum("curGrade");
        //public static UnitDynPropertyEnum LastUpdateGradeID { get; } = new UnitDynPropertyEnum("lastUpdateGradeID");

        public string PropName { get; private set; }
        public IntPtr PropAddr { get; private set; }
        public int PropOffset { get; private set; }
        private UnitDynPropertyEnum(string propName, IntPtr? propAddr = null) : base()
        {
            PropName = propName;
            PropAddr = propAddr == null ?
                IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, PropName) :
                propAddr.Value;
            PropOffset = (int)IL2CPP.il2cpp_field_get_offset(PropAddr);
        }

        public unsafe DynInt Get(WorldUnitDynData propData)
        {
            var num = (long)IL2CPP.Il2CppObjectBaseToPtrNotNull(propData) + PropOffset;
            var intPtr = *(System.IntPtr*)num;
            return (intPtr != (System.IntPtr)0) ? new DynInt(intPtr) : null;
        }

        public unsafe void Set(WorldUnitDynData propData, DynInt newValue)
        {
            var num = IL2CPP.Il2CppObjectBaseToPtrNotNull(propData);
            IL2CPP.il2cpp_gc_wbarrier_set_field(num, *(System.IntPtr*)((long)num + PropOffset), IL2CPP.Il2CppObjectBaseToPtr(newValue));
        }

        public UnitPropertyEnum GetPropertyEnum()
        {
            return UnitPropertyEnum.GetEnumByVal<UnitPropertyEnum>(this.PropName);
        }
    }
}
