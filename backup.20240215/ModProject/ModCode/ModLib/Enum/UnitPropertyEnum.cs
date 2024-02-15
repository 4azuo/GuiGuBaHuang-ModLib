using ModLib.Object;
using System;
using UnhollowerBaseLib;
using static DataUnit;

namespace ModLib.Enum
{
    public class UnitPropertyEnum : EnumObject
    {
        //public static UnitPropertyEnum Name { get; } = new UnitPropertyEnum("name", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "name"));
        //public static UnitPropertyEnum BattleModelData { get; } = new UnitPropertyEnum("battleModelData", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "battleModelData"));
        //public static UnitPropertyEnum ModelData { get; } = new UnitPropertyEnum("modelData", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "modelData"));
        public static UnitPropertyEnum Sex { get; } = new UnitPropertyEnum("sex", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "sex"));
        public static UnitPropertyEnum Race { get; } = new UnitPropertyEnum("race", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "race"));
        public static UnitPropertyEnum Age { get; } = new UnitPropertyEnum("age", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "age"));
        public static UnitPropertyEnum Life { get; } = new UnitPropertyEnum("life", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "life"));
        public static UnitPropertyEnum Beauty { get; } = new UnitPropertyEnum("beauty", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "beauty"));
        public static UnitPropertyEnum StandUp { get; } = new UnitPropertyEnum("standUp", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "standUp"));
        public static UnitPropertyEnum StandDown { get; } = new UnitPropertyEnum("standDown", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "standDown"));
        public static UnitPropertyEnum Reputation { get; } = new UnitPropertyEnum("reputation", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "reputation"));
        public static UnitPropertyEnum Exp { get; } = new UnitPropertyEnum("exp", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "exp"));
        public static UnitPropertyEnum GradeID { get; } = new UnitPropertyEnum("gradeID", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "gradeID"));
        public static UnitPropertyEnum InTrait { get; } = new UnitPropertyEnum("inTrait", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "inTrait"));
        public static UnitPropertyEnum OutTrait1 { get; } = new UnitPropertyEnum("outTrait1", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "outTrait1"));
        public static UnitPropertyEnum OutTrait2 { get; } = new UnitPropertyEnum("outTrait2", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "outTrait2"));
        public static UnitPropertyEnum AbilityPoint { get; } = new UnitPropertyEnum("abilityPoint", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "abilityPoint"));
        public static UnitPropertyEnum AbilityExp { get; } = new UnitPropertyEnum("abilityExp", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "abilityExp"));
        public static UnitPropertyEnum AbilityUpLevel { get; } = new UnitPropertyEnum("abilityUpLevel", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "abilityUpLevel"));
        public static UnitPropertyEnum Talent { get; } = new UnitPropertyEnum("talent", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "talent"));
        public static UnitPropertyEnum Health { get; } = new UnitPropertyEnum("health", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "health"));
        public static UnitPropertyEnum HealthMax { get; } = new UnitPropertyEnum("healthMax", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "healthMax"));
        public static UnitPropertyEnum Hp { get; } = new UnitPropertyEnum("hp", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "hp"));
        public static UnitPropertyEnum HpMax { get; } = new UnitPropertyEnum("hpMax", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "hpMax"));
        public static UnitPropertyEnum Mp { get; } = new UnitPropertyEnum("mp", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "mp"));
        public static UnitPropertyEnum MpMax { get; } = new UnitPropertyEnum("mpMax", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "mpMax"));
        public static UnitPropertyEnum Sp { get; } = new UnitPropertyEnum("sp", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "sp"));
        public static UnitPropertyEnum SpMax { get; } = new UnitPropertyEnum("spMax", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "spMax"));
        public static UnitPropertyEnum Attack { get; } = new UnitPropertyEnum("attack", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "attack"));
        public static UnitPropertyEnum Defense { get; } = new UnitPropertyEnum("defense", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "defense"));
        public static UnitPropertyEnum FootSpeed { get; } = new UnitPropertyEnum("footSpeed", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "footSpeed"));
        public static UnitPropertyEnum Luck { get; } = new UnitPropertyEnum("luck", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "luck"));
        public static UnitPropertyEnum Energy { get; } = new UnitPropertyEnum("energy", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "energy"));
        public static UnitPropertyEnum EnergyMax { get; } = new UnitPropertyEnum("energyMax", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "energyMax"));
        public static UnitPropertyEnum Crit { get; } = new UnitPropertyEnum("crit", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "crit"));
        public static UnitPropertyEnum CritValue { get; } = new UnitPropertyEnum("critValue", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "critValue"));
        public static UnitPropertyEnum Guard { get; } = new UnitPropertyEnum("guard", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "guard"));
        public static UnitPropertyEnum GuardValue { get; } = new UnitPropertyEnum("guardValue", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "guardValue"));
        public static UnitPropertyEnum MoveSpeed { get; } = new UnitPropertyEnum("moveSpeed", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "moveSpeed"));
        public static UnitPropertyEnum PhycicalFree { get; } = new UnitPropertyEnum("phycicalFree", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "phycicalFree"));
        public static UnitPropertyEnum MagicFree { get; } = new UnitPropertyEnum("magicFree", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "magicFree"));
        public static UnitPropertyEnum BasisFire { get; } = new UnitPropertyEnum("basisFire", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "basisFire"));
        public static UnitPropertyEnum BasisFroze { get; } = new UnitPropertyEnum("basisFroze", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "basisFroze"));
        public static UnitPropertyEnum BasisThunder { get; } = new UnitPropertyEnum("basisThunder", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "basisThunder"));
        public static UnitPropertyEnum BasisWind { get; } = new UnitPropertyEnum("basisWind", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "basisWind"));
        public static UnitPropertyEnum BasisEarth { get; } = new UnitPropertyEnum("basisEarth", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "basisEarth"));
        public static UnitPropertyEnum BasisWood { get; } = new UnitPropertyEnum("basisWood", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "basisWood"));
        public static UnitPropertyEnum BasisSword { get; } = new UnitPropertyEnum("basisSword", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "basisSword"));
        public static UnitPropertyEnum BasisSpear { get; } = new UnitPropertyEnum("basisSpear", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "basisSpear"));
        public static UnitPropertyEnum BasisBlade { get; } = new UnitPropertyEnum("basisBlade", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "basisBlade"));
        public static UnitPropertyEnum BasisFist { get; } = new UnitPropertyEnum("basisFist", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "basisFist"));
        public static UnitPropertyEnum BasisPalm { get; } = new UnitPropertyEnum("basisPalm", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "basisPalm"));
        public static UnitPropertyEnum BasisFinger { get; } = new UnitPropertyEnum("basisFinger", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "basisFinger"));
        public static UnitPropertyEnum RefineElixir { get; } = new UnitPropertyEnum("refineElixir", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "refineElixir"));
        public static UnitPropertyEnum RefineWeapon { get; } = new UnitPropertyEnum("refineWeapon", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "refineWeapon"));
        public static UnitPropertyEnum Geomancy { get; } = new UnitPropertyEnum("geomancy", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "geomancy"));
        public static UnitPropertyEnum Symbol { get; } = new UnitPropertyEnum("symbol", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "symbol"));
        public static UnitPropertyEnum Herbal { get; } = new UnitPropertyEnum("herbal", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "herbal"));
        public static UnitPropertyEnum Mine { get; } = new UnitPropertyEnum("mine", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "mine"));
        public static UnitPropertyEnum Mood { get; } = new UnitPropertyEnum("mood", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "mood"));
        public static UnitPropertyEnum MoodMax { get; } = new UnitPropertyEnum("moodMax", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "moodMax"));
        //public static UnitPropertyEnum Hobby { get; } = new UnitPropertyEnum("hobby", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "hobby"));
        //public static UnitPropertyEnum BornLuck { get; } = new UnitPropertyEnum("bornLuck", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "bornLuck"));
        //public static UnitPropertyEnum AddLuck { get; } = new UnitPropertyEnum("addLuck", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "addLuck"));
        //public static UnitPropertyEnum LooksLove { get; } = new UnitPropertyEnum("looksLove", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "looksLove"));
        //public static UnitPropertyEnum UnitInfo { get; } = new UnitPropertyEnum("unitInfo", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "unitInfo"));
        //public static UnitPropertyEnum Unit { get; } = new UnitPropertyEnum("_unit", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "_unit"));

        public string PropName { get; private set; }
        public IntPtr PropAddr { get; private set; }
        private UnitPropertyEnum(string propName, IntPtr propAddr) : base()
        {
            PropName = propName;
            PropAddr = propAddr;
        }

        public unsafe T Get<T>(PropertyData propData)
        {
            var rs = *(T*)((long)IL2CPP.Il2CppObjectBaseToPtrNotNull(propData) + (int)IL2CPP.il2cpp_field_get_offset(PropAddr));
            //DebugHelper.WriteLog($"Get Property ({PropName}): {rs}");
            return rs;
        }

        public unsafe void Set<T>(PropertyData propData, T newValue)
        {
            //var oldValue = Get<T>(propData);
            //DebugHelper.WriteLog($"Set Property ({PropName}): {oldValue} → {newValue}");
            *(T*)((long)IL2CPP.Il2CppObjectBaseToPtrNotNull(propData) + (int)IL2CPP.il2cpp_field_get_offset(PropAddr)) = newValue;
        }
    }
}
