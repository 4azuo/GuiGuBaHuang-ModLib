using ModLib.Object;
using System;
using UnhollowerBaseLib;

namespace ModLib.Enum
{
    public class UnitDynPropertyEnum : EnumObject
    {
        public static UnitDynPropertyEnum Sex { get; } = new UnitDynPropertyEnum("sex", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "sex"));
        public static UnitDynPropertyEnum Race { get; } = new UnitDynPropertyEnum("race", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "race"));
        public static UnitDynPropertyEnum Age { get; } = new UnitDynPropertyEnum("age", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "age"));
        public static UnitDynPropertyEnum Life { get; } = new UnitDynPropertyEnum("life", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "life"));
        public static UnitDynPropertyEnum Beauty { get; } = new UnitDynPropertyEnum("beauty", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "beauty"));
        public static UnitDynPropertyEnum StandUp { get; } = new UnitDynPropertyEnum("standUp", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "standUp"));
        public static UnitDynPropertyEnum StandDown { get; } = new UnitDynPropertyEnum("standDown", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "standDown"));
        public static UnitDynPropertyEnum Reputation { get; } = new UnitDynPropertyEnum("reputation", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "reputation"));
        public static UnitDynPropertyEnum GradeID { get; } = new UnitDynPropertyEnum("gradeID", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "gradeID"));
        public static UnitDynPropertyEnum InTrait { get; } = new UnitDynPropertyEnum("inTrait", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "inTrait"));
        public static UnitDynPropertyEnum OutTrait1 { get; } = new UnitDynPropertyEnum("outTrait1", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "outTrait1"));
        public static UnitDynPropertyEnum OutTrait2 { get; } = new UnitDynPropertyEnum("outTrait2", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "outTrait2"));
        public static UnitDynPropertyEnum Hp { get; } = new UnitDynPropertyEnum("hp", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "hp"));
        public static UnitDynPropertyEnum HpMax { get; } = new UnitDynPropertyEnum("hpMax", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "hpMax"));
        public static UnitDynPropertyEnum HpRestore { get; } = new UnitDynPropertyEnum("hpRestore", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "hpRestore"));
        public static UnitDynPropertyEnum Mp { get; } = new UnitDynPropertyEnum("mp", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "mp"));
        public static UnitDynPropertyEnum MpMax { get; } = new UnitDynPropertyEnum("mpMax", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "mpMax"));
        public static UnitDynPropertyEnum MpRestore { get; } = new UnitDynPropertyEnum("mpRestore", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "mpRestore"));
        public static UnitDynPropertyEnum Sp { get; } = new UnitDynPropertyEnum("sp", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "sp"));
        public static UnitDynPropertyEnum SpMax { get; } = new UnitDynPropertyEnum("spMax", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "spMax"));
        public static UnitDynPropertyEnum Dp { get; } = new UnitDynPropertyEnum("dp", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "dp"));
        public static UnitDynPropertyEnum DpMax { get; } = new UnitDynPropertyEnum("dpMax", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "dpMax"));
        public static UnitDynPropertyEnum SpRestore { get; } = new UnitDynPropertyEnum("spRestore", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "spRestore"));
        public static UnitDynPropertyEnum Attack { get; } = new UnitDynPropertyEnum("attack", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "attack"));
        public static UnitDynPropertyEnum Defense { get; } = new UnitDynPropertyEnum("defense", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "defense"));
        public static UnitDynPropertyEnum Crit { get; } = new UnitDynPropertyEnum("crit", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "crit"));
        public static UnitDynPropertyEnum CritValue { get; } = new UnitDynPropertyEnum("critValue", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "critValue"));
        public static UnitDynPropertyEnum Guard { get; } = new UnitDynPropertyEnum("guard", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "guard"));
        public static UnitDynPropertyEnum GuardValue { get; } = new UnitDynPropertyEnum("guardValue", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "guardValue"));
        public static UnitDynPropertyEnum MoveSpeed { get; } = new UnitDynPropertyEnum("moveSpeed", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "moveSpeed"));
        public static UnitDynPropertyEnum PhycicalFree { get; } = new UnitDynPropertyEnum("phycicalFree", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "phycicalFree"));
        public static UnitDynPropertyEnum MagicFree { get; } = new UnitDynPropertyEnum("magicFree", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "magicFree"));
        public static UnitDynPropertyEnum Exp { get; } = new UnitDynPropertyEnum("exp", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "exp"));
        public static UnitDynPropertyEnum BasisFire { get; } = new UnitDynPropertyEnum("basisFire", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "basisFire"));
        public static UnitDynPropertyEnum BasisFroze { get; } = new UnitDynPropertyEnum("basisFroze", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "basisFroze"));
        public static UnitDynPropertyEnum BasisThunder { get; } = new UnitDynPropertyEnum("basisThunder", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "basisThunder"));
        public static UnitDynPropertyEnum BasisWind { get; } = new UnitDynPropertyEnum("basisWind", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "basisWind"));
        public static UnitDynPropertyEnum BasisEarth { get; } = new UnitDynPropertyEnum("basisEarth", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "basisEarth"));
        public static UnitDynPropertyEnum BasisWood { get; } = new UnitDynPropertyEnum("basisWood", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "basisWood"));
        public static UnitDynPropertyEnum BasisBlade { get; } = new UnitDynPropertyEnum("basisBlade", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "basisBlade"));
        public static UnitDynPropertyEnum BasisSpear { get; } = new UnitDynPropertyEnum("basisSpear", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "basisSpear"));
        public static UnitDynPropertyEnum BasisSword { get; } = new UnitDynPropertyEnum("basisSword", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "basisSword"));
        public static UnitDynPropertyEnum BasisFist { get; } = new UnitDynPropertyEnum("basisFist", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "basisFist"));
        public static UnitDynPropertyEnum BasisPalm { get; } = new UnitDynPropertyEnum("basisPalm", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "basisPalm"));
        public static UnitDynPropertyEnum BasisFinger { get; } = new UnitDynPropertyEnum("basisFinger", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "basisFinger"));
        public static UnitDynPropertyEnum AbilityPoint { get; } = new UnitDynPropertyEnum("abilityPoint", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "abilityPoint"));
        public static UnitDynPropertyEnum Talent { get; } = new UnitDynPropertyEnum("talent", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "talent"));
        public static UnitDynPropertyEnum FootSpeed { get; } = new UnitDynPropertyEnum("footSpeed", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "footSpeed"));
        public static UnitDynPropertyEnum Luck { get; } = new UnitDynPropertyEnum("luck", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "luck"));
        public static UnitDynPropertyEnum AbilityExp { get; } = new UnitDynPropertyEnum("abilityExp", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "abilityExp"));
        public static UnitDynPropertyEnum ImmortalPoint { get; } = new UnitDynPropertyEnum("immortalPoint", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "immortalPoint"));
        public static UnitDynPropertyEnum RefineElixir { get; } = new UnitDynPropertyEnum("refineElixir", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "refineElixir"));
        public static UnitDynPropertyEnum RefineWeapon { get; } = new UnitDynPropertyEnum("refineWeapon", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "refineWeapon"));
        public static UnitDynPropertyEnum Geomancy { get; } = new UnitDynPropertyEnum("geomancy", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "geomancy"));
        public static UnitDynPropertyEnum Symbol { get; } = new UnitDynPropertyEnum("symbol", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "symbol"));
        public static UnitDynPropertyEnum Herbal { get; } = new UnitDynPropertyEnum("herbal", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "herbal"));
        public static UnitDynPropertyEnum Mine { get; } = new UnitDynPropertyEnum("mine", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "mine"));
        public static UnitDynPropertyEnum Health { get; } = new UnitDynPropertyEnum("health", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "health"));
        public static UnitDynPropertyEnum HealthMax { get; } = new UnitDynPropertyEnum("healthMax", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "healthMax"));
        public static UnitDynPropertyEnum Energy { get; } = new UnitDynPropertyEnum("energy", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "energy"));
        public static UnitDynPropertyEnum EnergyMax { get; } = new UnitDynPropertyEnum("energyMax", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "energyMax"));
        public static UnitDynPropertyEnum Mood { get; } = new UnitDynPropertyEnum("mood", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "mood"));
        public static UnitDynPropertyEnum MoodMax { get; } = new UnitDynPropertyEnum("moodMax", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "moodMax"));
        public static UnitDynPropertyEnum HealthUpRate { get; } = new UnitDynPropertyEnum("healthUpRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "healthUpRate"));
        public static UnitDynPropertyEnum HealthDownRate { get; } = new UnitDynPropertyEnum("healthDownRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "healthDownRate"));
        public static UnitDynPropertyEnum EnergyUpRate { get; } = new UnitDynPropertyEnum("energyUpRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "energyUpRate"));
        public static UnitDynPropertyEnum EnergyDownRate { get; } = new UnitDynPropertyEnum("energyDownRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "energyDownRate"));
        public static UnitDynPropertyEnum MoodUpRate { get; } = new UnitDynPropertyEnum("moodUpRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "moodUpRate"));
        public static UnitDynPropertyEnum MoodDownRate { get; } = new UnitDynPropertyEnum("moodDownRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "moodDownRate"));
        public static UnitDynPropertyEnum IntimRate { get; } = new UnitDynPropertyEnum("intimRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "intimRate"));
        public static UnitDynPropertyEnum HateRate { get; } = new UnitDynPropertyEnum("hateRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "hateRate"));
        public static UnitDynPropertyEnum ExpGrowRate { get; } = new UnitDynPropertyEnum("expGrowRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "expGrowRate"));
        public static UnitDynPropertyEnum TownBuyDeclineRate { get; } = new UnitDynPropertyEnum("townBuyDeclineRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "townBuyDeclineRate"));
        public static UnitDynPropertyEnum SchoolBuyDeclineRate { get; } = new UnitDynPropertyEnum("schoolBuyDeclineRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "schoolBuyDeclineRate"));
        public static UnitDynPropertyEnum StepGrowRate { get; } = new UnitDynPropertyEnum("stepGrowRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "stepGrowRate"));
        public static UnitDynPropertyEnum LockInitScore { get; } = new UnitDynPropertyEnum("lockInitScore", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "lockInitScore"));
        public static UnitDynPropertyEnum PropGridNum { get; } = new UnitDynPropertyEnum("propGridNum", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "propGridNum"));
        public static UnitDynPropertyEnum PropEquipNum { get; } = new UnitDynPropertyEnum("propEquipNum", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "propEquipNum"));
        public static UnitDynPropertyEnum HpUpRate { get; } = new UnitDynPropertyEnum("hpUpRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "hpUpRate"));
        public static UnitDynPropertyEnum HpDownRate { get; } = new UnitDynPropertyEnum("hpDownRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "hpDownRate"));
        public static UnitDynPropertyEnum MpUpRate { get; } = new UnitDynPropertyEnum("mpUpRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "mpUpRate"));
        public static UnitDynPropertyEnum MpDownRate { get; } = new UnitDynPropertyEnum("mpDownRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "mpDownRate"));
        public static UnitDynPropertyEnum SpUpRate { get; } = new UnitDynPropertyEnum("spUpRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "spUpRate"));
        public static UnitDynPropertyEnum SpDownRate { get; } = new UnitDynPropertyEnum("spDownRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "spDownRate"));
        public static UnitDynPropertyEnum SwordGrowRate { get; } = new UnitDynPropertyEnum("swordGrowRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "swordGrowRate"));
        public static UnitDynPropertyEnum SpearGrowRate { get; } = new UnitDynPropertyEnum("spearGrowRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "spearGrowRate"));
        public static UnitDynPropertyEnum BladeGrowRate { get; } = new UnitDynPropertyEnum("bladeGrowRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "bladeGrowRate"));
        public static UnitDynPropertyEnum FistGrowRate { get; } = new UnitDynPropertyEnum("fistGrowRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "fistGrowRate"));
        public static UnitDynPropertyEnum PalmGrowRate { get; } = new UnitDynPropertyEnum("palmGrowRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "palmGrowRate"));
        public static UnitDynPropertyEnum FingerGrowRate { get; } = new UnitDynPropertyEnum("fingerGrowRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "fingerGrowRate"));
        public static UnitDynPropertyEnum FireGrowRate { get; } = new UnitDynPropertyEnum("fireGrowRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "fireGrowRate"));
        public static UnitDynPropertyEnum FrozeGrowRate { get; } = new UnitDynPropertyEnum("frozeGrowRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "frozeGrowRate"));
        public static UnitDynPropertyEnum ThunderGrowRate { get; } = new UnitDynPropertyEnum("thunderGrowRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "thunderGrowRate"));
        public static UnitDynPropertyEnum WindGrowRate { get; } = new UnitDynPropertyEnum("windGrowRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "windGrowRate"));
        public static UnitDynPropertyEnum EarthGrowRate { get; } = new UnitDynPropertyEnum("earthGrowRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "earthGrowRate"));
        public static UnitDynPropertyEnum WoodGrowRate { get; } = new UnitDynPropertyEnum("woodGrowRate", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "woodGrowRate"));
        public static UnitDynPropertyEnum PlayerView { get; } = new UnitDynPropertyEnum("playerView", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "playerView"));
        //public static UnitDynPropertyEnum IsHide { get; } = new UnitDynPropertyEnum("isHide", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "isHide"));
        //public static UnitDynPropertyEnum UnitData { get; } = new UnitDynPropertyEnum("unitData", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "unitData"));
        //public static UnitDynPropertyEnum BattleModelData { get; } = new UnitDynPropertyEnum("battleModelData", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "battleModelData"));
        //public static UnitDynPropertyEnum ModelData { get; } = new UnitDynPropertyEnum("modelData", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "modelData"));
        //public static UnitDynPropertyEnum ArrayValuesList { get; } = new UnitDynPropertyEnum("arrayValuesList", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "arrayValuesList"));
        //public static UnitDynPropertyEnum DynIntTab { get; } = new UnitDynPropertyEnum("dynIntTab", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "dynIntTab"));
        //public static UnitDynPropertyEnum AllBasWp { get; } = new UnitDynPropertyEnum("allBasWp", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "allBasWp"));
        //public static UnitDynPropertyEnum AllBasMg { get; } = new UnitDynPropertyEnum("allBasMg", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "allBasMg"));
        //public static UnitDynPropertyEnum CurGrade { get; } = new UnitDynPropertyEnum("curGrade", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "curGrade"));
        //public static UnitDynPropertyEnum LastUpdateGradeID { get; } = new UnitDynPropertyEnum("lastUpdateGradeID", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<WorldUnitDynData>.NativeClassPtr, "lastUpdateGradeID"));

        public string PropName { get; private set; }
        public IntPtr PropAddr { get; private set; }
        private UnitDynPropertyEnum(string propName, IntPtr propAddr) : base()
        {
            PropName = propName;
            PropAddr = propAddr;
        }

        public unsafe DynInt Get(WorldUnitDynData propData)
        {
            var num = (long)IL2CPP.Il2CppObjectBaseToPtrNotNull(propData) + (int)IL2CPP.il2cpp_field_get_offset(PropAddr);
            var intPtr = *(System.IntPtr*)num;
            return (intPtr != (System.IntPtr)0) ? new DynInt(intPtr) : null;
        }

        public unsafe void Set(WorldUnitDynData propData, DynInt newValue)
        {
            var num = IL2CPP.Il2CppObjectBaseToPtrNotNull(propData);
            IL2CPP.il2cpp_gc_wbarrier_set_field(num, *(System.IntPtr*)((long)num + (int)IL2CPP.il2cpp_field_get_offset(PropAddr)), IL2CPP.Il2CppObjectBaseToPtr(newValue));
        }
    }
}
