using ModLib.Object;
using System;
using UnhollowerBaseLib;
using static DataUnit;

namespace ModLib.Enum
{
    public class UnitPropertyEnum : EnumObject
    {
        //public static UnitPropertyEnum Example { get; } = new UnitPropertyEnum("example", IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, "example"));
        //public static UnitPropertyEnum Name { get; } = new UnitPropertyEnum("name");
        //public static UnitPropertyEnum BattleModelData { get; } = new UnitPropertyEnum("battleModelData");
        //public static UnitPropertyEnum ModelData { get; } = new UnitPropertyEnum("modelData");
        public static UnitPropertyEnum Sex { get; } = new UnitPropertyEnum("sex");
        public static UnitPropertyEnum Race { get; } = new UnitPropertyEnum("race");
        public static UnitPropertyEnum Age { get; } = new UnitPropertyEnum("age");
        public static UnitPropertyEnum Life { get; } = new UnitPropertyEnum("life");
        public static UnitPropertyEnum Beauty { get; } = new UnitPropertyEnum("beauty");
        public static UnitPropertyEnum StandUp { get; } = new UnitPropertyEnum("standUp");
        public static UnitPropertyEnum StandDown { get; } = new UnitPropertyEnum("standDown");
        public static UnitPropertyEnum Reputation { get; } = new UnitPropertyEnum("reputation");
        public static UnitPropertyEnum Exp { get; } = new UnitPropertyEnum("exp");
        public static UnitPropertyEnum GradeID { get; } = new UnitPropertyEnum("gradeID");
        public static UnitPropertyEnum InTrait { get; } = new UnitPropertyEnum("inTrait");
        public static UnitPropertyEnum OutTrait1 { get; } = new UnitPropertyEnum("outTrait1");
        public static UnitPropertyEnum OutTrait2 { get; } = new UnitPropertyEnum("outTrait2");
        public static UnitPropertyEnum AbilityPoint { get; } = new UnitPropertyEnum("abilityPoint");
        public static UnitPropertyEnum AbilityExp { get; } = new UnitPropertyEnum("abilityExp");
        public static UnitPropertyEnum AbilityUpLevel { get; } = new UnitPropertyEnum("abilityUpLevel");
        public static UnitPropertyEnum Talent { get; } = new UnitPropertyEnum("talent");
        public static UnitPropertyEnum Health { get; } = new UnitPropertyEnum("health");
        public static UnitPropertyEnum HealthMax { get; } = new UnitPropertyEnum("healthMax");
        public static UnitPropertyEnum Hp { get; } = new UnitPropertyEnum("hp");
        public static UnitPropertyEnum HpMax { get; } = new UnitPropertyEnum("hpMax");
        public static UnitPropertyEnum Mp { get; } = new UnitPropertyEnum("mp");
        public static UnitPropertyEnum MpMax { get; } = new UnitPropertyEnum("mpMax");
        public static UnitPropertyEnum Sp { get; } = new UnitPropertyEnum("sp");
        public static UnitPropertyEnum SpMax { get; } = new UnitPropertyEnum("spMax");
        public static UnitPropertyEnum Attack { get; } = new UnitPropertyEnum("attack");
        public static UnitPropertyEnum Defense { get; } = new UnitPropertyEnum("defense");
        public static UnitPropertyEnum FootSpeed { get; } = new UnitPropertyEnum("footSpeed");
        public static UnitPropertyEnum Luck { get; } = new UnitPropertyEnum("luck");
        public static UnitPropertyEnum Energy { get; } = new UnitPropertyEnum("energy");
        public static UnitPropertyEnum EnergyMax { get; } = new UnitPropertyEnum("energyMax");
        public static UnitPropertyEnum Crit { get; } = new UnitPropertyEnum("crit");
        public static UnitPropertyEnum CritValue { get; } = new UnitPropertyEnum("critValue");
        public static UnitPropertyEnum Guard { get; } = new UnitPropertyEnum("guard");
        public static UnitPropertyEnum GuardValue { get; } = new UnitPropertyEnum("guardValue");
        public static UnitPropertyEnum MoveSpeed { get; } = new UnitPropertyEnum("moveSpeed");
        public static UnitPropertyEnum PhycicalFree { get; } = new UnitPropertyEnum("phycicalFree");
        public static UnitPropertyEnum MagicFree { get; } = new UnitPropertyEnum("magicFree");
        public static UnitPropertyEnum BasisFire { get; } = new UnitPropertyEnum("basisFire");
        public static UnitPropertyEnum BasisFroze { get; } = new UnitPropertyEnum("basisFroze");
        public static UnitPropertyEnum BasisThunder { get; } = new UnitPropertyEnum("basisThunder");
        public static UnitPropertyEnum BasisWind { get; } = new UnitPropertyEnum("basisWind");
        public static UnitPropertyEnum BasisEarth { get; } = new UnitPropertyEnum("basisEarth");
        public static UnitPropertyEnum BasisWood { get; } = new UnitPropertyEnum("basisWood");
        public static UnitPropertyEnum BasisSword { get; } = new UnitPropertyEnum("basisSword");
        public static UnitPropertyEnum BasisSpear { get; } = new UnitPropertyEnum("basisSpear");
        public static UnitPropertyEnum BasisBlade { get; } = new UnitPropertyEnum("basisBlade");
        public static UnitPropertyEnum BasisFist { get; } = new UnitPropertyEnum("basisFist");
        public static UnitPropertyEnum BasisPalm { get; } = new UnitPropertyEnum("basisPalm");
        public static UnitPropertyEnum BasisFinger { get; } = new UnitPropertyEnum("basisFinger");
        public static UnitPropertyEnum RefineElixir { get; } = new UnitPropertyEnum("refineElixir");
        public static UnitPropertyEnum RefineWeapon { get; } = new UnitPropertyEnum("refineWeapon");
        public static UnitPropertyEnum Geomancy { get; } = new UnitPropertyEnum("geomancy");
        public static UnitPropertyEnum Symbol { get; } = new UnitPropertyEnum("symbol");
        public static UnitPropertyEnum Herbal { get; } = new UnitPropertyEnum("herbal");
        public static UnitPropertyEnum Mine { get; } = new UnitPropertyEnum("mine");
        public static UnitPropertyEnum Mood { get; } = new UnitPropertyEnum("mood");
        public static UnitPropertyEnum MoodMax { get; } = new UnitPropertyEnum("moodMax");
        //public static UnitPropertyEnum Hobby { get; } = new UnitPropertyEnum("hobby");
        //public static UnitPropertyEnum BornLuck { get; } = new UnitPropertyEnum("bornLuck");
        //public static UnitPropertyEnum AddLuck { get; } = new UnitPropertyEnum("addLuck");
        //public static UnitPropertyEnum LooksLove { get; } = new UnitPropertyEnum("looksLove");
        //public static UnitPropertyEnum UnitInfo { get; } = new UnitPropertyEnum("unitInfo");
        //public static UnitPropertyEnum Unit { get; } = new UnitPropertyEnum("_unit");

        public object obj, PropertyInfo prop, object oldValue, object newValue { get; private set; }
        public IntPtr PropAddr { get; private set; }
        public int PropOffset { get; private set; }
        private UnitPropertyEnum(object obj, PropertyInfo prop, object oldValue, object newValue, IntPtr? propAddr = null) : base(propName)
        {
            PropName = propName;
            PropAddr = propAddr == null ?
                IL2CPP.GetIl2CppField(Il2CppClassPointerStore<PropertyData>.NativeClassPtr, PropName) :
                propAddr.Value;
            PropOffset = (int)IL2CPP.il2cpp_field_get_offset(PropAddr);
        }

        public unsafe T Get<T>(PropertyData propData)
        {
            var rs = *(T*)((long)IL2CPP.Il2CppObjectBaseToPtrNotNull(propData) + PropOffset);
            //DebugHelper.WriteLog($"Get Property ({PropName}): {rs}");
            return rs;
        }

        public unsafe void Set<T>(PropertyData propData, T newValue)
        {
            //var oldValue = Get<T>(propData);
            //DebugHelper.WriteLog($"Set Property ({PropName}): {oldValue} → {newValue}");
            *(T*)((long)IL2CPP.Il2CppObjectBaseToPtrNotNull(propData) + PropOffset) = newValue;
        }

        public UnitDynPropertyEnum GetDynPropertyEnum()
        {
            return UnitDynPropertyEnum.GetEnumByVal<UnitDynPropertyEnum>(this.PropName);
        }
    }
}
