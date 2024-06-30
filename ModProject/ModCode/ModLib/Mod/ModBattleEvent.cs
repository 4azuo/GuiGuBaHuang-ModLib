using EBattleTypeData;
using ModLib.Enum;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ModLib.Mod
{
    public abstract class ModBattleEvent : ModEvent
    {
        #region DmgKey
        public enum DmgEnum
        {
            DmgDealt = 1,
            DmgRecv = 2,
        }

        public enum DmgTypeEnum
        {
            Damage = 1,

            Physic = 100,
            Magic = 200,

            Blade = 101,
            Spear = 102,
            Sword = 103,
            Fist = 104,
            Palm = 105,
            Finger = 106,

            Fire = 201,
            Frozen = 202,
            Thunder = 203,
            Wind = 204,
            Earth = 205,
            Wood = 206,
        }

        public static string GetDmgKey(DmgEnum dmgEnum, DmgTypeEnum dmgTypeEnum)
        {
            return $"{dmgEnum}_{dmgTypeEnum}";
        }

        public static UnitPropertyEnum GetDmgPropertyEnum(DmgTypeEnum dmgTypeEnum)
        {
            return UnitPropertyEnum.GetEnumByName<UnitPropertyEnum>($"Basis{dmgTypeEnum}");
        }

        public static DmgTypeEnum GetDmgBasisType(MartialTool.HitData hitData)
        {
            if (hitData != null)
            {
                var weaponType = hitData?.skillBase?.data?.weaponType?.value ?? 0;
                if (weaponType > 0)
                {
                    var dmgType = (DmgTypeEnum)(((int)DmgTypeEnum.Physic) + weaponType);
                    return dmgType;
                }
                var magicType = hitData?.skillBase?.data?.magicType?.value ?? 0;
                if (magicType > 0)
                {
                    var dmgType = (DmgTypeEnum)(((int)DmgTypeEnum.Magic) + magicType);
                    return dmgType;
                }
            }
            return DmgTypeEnum.Damage;
        }

        public static DmgTypeEnum GetDmgType(MartialTool.HitData hitData)
        {
            if (hitData != null)
            {
                var weaponType = hitData?.skillBase?.data?.weaponType?.value ?? 0;
                if (weaponType > 0)
                {
                    return DmgTypeEnum.Physic;
                }
                var magicType = hitData?.skillBase?.data?.magicType?.value ?? 0;
                if (magicType > 0)
                {
                    return DmgTypeEnum.Magic;
                }
            }
            return DmgTypeEnum.Damage;
        }
        #endregion

        #region DmgInfo
        public enum DmgSaveEnum
        {
            Global = 1,
            Local = 2,
        }
        public IDictionary<string, long> GameDmg { get; set; } = new Dictionary<string, long>();

        [JsonIgnore]
        public IDictionary<string, long> BattleDmg { get; set; } = new Dictionary<string, long>();

        public long GetDmg(DmgSaveEnum dmgSaveEnum, string dmgKey)
        {
            switch (dmgSaveEnum)
            {
                case DmgSaveEnum.Global:
                    return GameDmg[dmgKey];
                case DmgSaveEnum.Local:
                    return BattleDmg[dmgKey];
                default:
                    return 0;
            }
        }
        #endregion

        [JsonIgnore]
        public bool IsPlayerDie { get; set; }

        public override void OnLoadGame()
        {
            foreach (var de in (DmgEnum[])System.Enum.GetValues(typeof(DmgEnum)))
            {
                foreach (var type in (DmgTypeEnum[])System.Enum.GetValues(typeof(DmgTypeEnum)))
                {
                    var key = GetDmgKey(de, type);
                    if (!GameDmg.ContainsKey(key))
                    {
                        GameDmg.Add(key, 0);
                    }
                }
            }
        }

        public override void OnBattleStart(ETypeData e)
        {
            IsPlayerDie = false;
            BattleDmg.Clear();
            foreach (var de in (DmgEnum[])System.Enum.GetValues(typeof(DmgEnum)))
            {
                foreach (var type in (DmgTypeEnum[])System.Enum.GetValues(typeof(DmgTypeEnum)))
                {
                    var key = GetDmgKey(de, type);
                    BattleDmg.Add(key, 0);
                }
            }
        }

        public override void OnBattleUnitDie(UnitDie e)
        {
            var dieUnit = e?.unit?.data?.TryCast<UnitDataHuman>();
            if (dieUnit?.worldUnitData != null && g.world.battle.data.isRealBattle)
            {
                if (dieUnit.worldUnitData.unit.IsPlayer())
                {
                    IsPlayerDie = true;
                }
            }
        }

        public override void OnBattleUnitHit(UnitHit e)
        {
            var dmg = Math.Abs(e.hitData.hitValue);
            var attackUnitData = e?.hitData?.attackUnit?.data?.TryCast<UnitDataHuman>();
            var hitUnitData = e?.hitUnit?.data?.TryCast<UnitDataHuman>();
            if (attackUnitData?.worldUnitData?.unit?.IsPlayer() ?? false)
            {
                var commonDmgKey = GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Damage);
                GameDmg[commonDmgKey] += dmg;
                BattleDmg[commonDmgKey] += dmg;

                var dmgType = GetDmgType(e?.hitData);
                if (dmgType != DmgTypeEnum.Damage)
                {
                    GameDmg[GetDmgKey(DmgEnum.DmgDealt, dmgType)] += dmg;
                    BattleDmg[GetDmgKey(DmgEnum.DmgDealt, dmgType)] += dmg;

                    var basisKey = GetDmgBasisType(e?.hitData);
                    GameDmg[GetDmgKey(DmgEnum.DmgDealt, basisKey)] += dmg;
                    BattleDmg[GetDmgKey(DmgEnum.DmgDealt, basisKey)] += dmg;
                }
            }
            else if (hitUnitData?.worldUnitData?.unit?.IsPlayer() ?? false)
            {
                var commonDmgKey = GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Damage);
                GameDmg[commonDmgKey] += dmg;
                BattleDmg[commonDmgKey] += dmg;

                var dmgType = GetDmgType(e?.hitData);
                if (dmgType != DmgTypeEnum.Damage)
                {
                    GameDmg[GetDmgKey(DmgEnum.DmgRecv, dmgType)] += dmg;
                    BattleDmg[GetDmgKey(DmgEnum.DmgRecv, dmgType)] += dmg;

                    var basisKey = GetDmgBasisType(e?.hitData);
                    GameDmg[GetDmgKey(DmgEnum.DmgRecv, basisKey)] += dmg;
                    BattleDmg[GetDmgKey(DmgEnum.DmgRecv, basisKey)] += dmg;
                }
            }
        }
    }
}
