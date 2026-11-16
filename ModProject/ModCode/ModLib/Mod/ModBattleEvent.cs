using EBattleTypeData;
using ModLib.Attributes;
using ModLib.Enum;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using ModLib.Helper;

namespace ModLib.Mod
{
    /// <summary>
    /// Built-in battle tracker (Local, order 100). Exposes the current battle (scene, player unit, units, town/school),
    /// the last attacking/hit units, kill list, and damage statistics per damage type for the battle (BattleDmg) and the whole save (GameDmg).
    /// </summary>
    [Cache("$BATTLE$", OrderIndex = 100, CacheType = CacheAttribute.CType.Local, WorkOn = CacheAttribute.WType.Local)]
    public class ModBattleEvent : ModEvent
    {
        public static ModBattleEvent Instance { get; set; }

        #region DmgKey
        /// <summary>
        /// Damage direction: dealt by the player or received by the player.
        /// </summary>
        public enum DmgEnum
        {
            DmgDealt = 1,
            DmgRecv = 2,
        }

        /// <summary>
        /// Damage categories: total, physical/magic, and each basis (weapon or element).
        /// </summary>
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

        /// <summary>
        /// Builds the dictionary key "&lt;DmgEnum&gt;_&lt;DmgTypeEnum&gt;".
        /// </summary>
        public static string GetDmgKey(DmgEnum dmgEnum, DmgTypeEnum dmgTypeEnum)
        {
            return $"{dmgEnum}_{dmgTypeEnum}";
        }

        /// <summary>
        /// Maps a basis damage type to its UnitDynPropertyEnum (BasisBlade, BasisFire...).
        /// </summary>
        public static UnitDynPropertyEnum GetDmgPropertyEnum(DmgTypeEnum dmgTypeEnum)
        {
            return UnitDynPropertyEnum.GetEnumByName<UnitDynPropertyEnum>($"Basis{dmgTypeEnum}");
        }

        /// <summary>
        /// Reads a dynamic property value of a battle unit.
        /// </summary>
        public static int GetUnitPropertyValue(UnitCtrlBase uc, DmgTypeEnum dmgTypeEnum)
        {
            return GetUnitPropertyValue(uc, GetDmgPropertyEnum(dmgTypeEnum));
        }

        /// <summary>
        /// Reads a dynamic property value of a battle unit.
        /// </summary>
        public static int GetUnitPropertyValue(UnitCtrlBase uc, UnitDynPropertyEnum pEnum)
        {
            if (pEnum == null)
                return 0;
            return (uc.data.GetValue(pEnum.PropName) as DynInt).value;
        }

        /// <summary>
        /// Basis type of a hit (weapon type or magic element), Damage when unknown.
        /// </summary>
        public static DmgTypeEnum GetDmgBasisType(MartialTool.HitData hitData)
        {
            if (hitData != null)
            {
                var weaponType = hitData?.weaponType ?? 0;
                if (weaponType > 0)
                {
                    var dmgType = (DmgTypeEnum)(((int)DmgTypeEnum.Physic) + weaponType);
                    return dmgType;
                }
                var magicType = hitData?.magicType ?? 0;
                if (magicType > 0)
                {
                    var dmgType = (DmgTypeEnum)(((int)DmgTypeEnum.Magic) + magicType);
                    return dmgType;
                }
            }
            return DmgTypeEnum.Damage;
        }

        /// <summary>
        /// Physical / Magic / Damage category of a hit, from the skill data.
        /// </summary>
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

        /// <summary>
        /// True for weapon or element basis types.
        /// </summary>
        public static bool IsBasisDmg(DmgTypeEnum type)
        {
            return (type >= DmgTypeEnum.Blade && type <= DmgTypeEnum.Finger) ||
                (type >= DmgTypeEnum.Fire && type <= DmgTypeEnum.Wood);
        }

        /// <summary>
        /// True for weapon basis types.
        /// </summary>
        public static bool IsBasisPhysic(DmgTypeEnum type)
        {
            return (type >= DmgTypeEnum.Blade && type <= DmgTypeEnum.Finger);
        }

        /// <summary>
        /// True for element basis types.
        /// </summary>
        public static bool IsBasisMagic(DmgTypeEnum type)
        {
            return (type >= DmgTypeEnum.Fire && type <= DmgTypeEnum.Wood);
        }

        /// <summary>
        /// Basis with the highest damage dealt by the player in the current battle.
        /// </summary>
        public DmgTypeEnum GetHighestDealtDmgTypeEnum()
        {
            var basises = new Dictionary<DmgTypeEnum, long>
            {
                [DmgTypeEnum.Palm] = GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Palm)),
                [DmgTypeEnum.Blade] = GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Blade)),
                [DmgTypeEnum.Fist] = GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Fist)),
                [DmgTypeEnum.Finger] = GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Finger)),
                [DmgTypeEnum.Sword] = GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Sword)),
                [DmgTypeEnum.Spear] = GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Spear)),
                [DmgTypeEnum.Frozen] = GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Frozen)),
                [DmgTypeEnum.Fire] = GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Fire)),
                [DmgTypeEnum.Wood] = GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Wood)),
                [DmgTypeEnum.Thunder] = GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Thunder)),
                [DmgTypeEnum.Wind] = GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Wind)),
                [DmgTypeEnum.Earth] = GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Earth))
            };
            var max = basises.Max(x => x.Value);
            return basises.FirstOrDefault(x => x.Value == max).Key;
        }

        /// <summary>
        /// Static shortcut of GetHighestDealtDmgTypeEnum.
        /// </summary>
        public static DmgTypeEnum sGetHighestDealtDmgTypeEnum()
        {
            return ModBattleEvent.Instance.GetHighestDealtDmgTypeEnum();
        }

        /// <summary>
        /// Basis with the highest damage received by the player in the current battle.
        /// </summary>
        public DmgTypeEnum GetHighestRecvDmgTypeEnum()
        {
            var basises = new Dictionary<DmgTypeEnum, long>
            {
                [DmgTypeEnum.Palm] = GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Palm)),
                [DmgTypeEnum.Blade] = GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Blade)),
                [DmgTypeEnum.Fist] = GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Fist)),
                [DmgTypeEnum.Finger] = GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Finger)),
                [DmgTypeEnum.Sword] = GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Sword)),
                [DmgTypeEnum.Spear] = GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Spear)),
                [DmgTypeEnum.Frozen] = GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Frozen)),
                [DmgTypeEnum.Fire] = GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Fire)),
                [DmgTypeEnum.Wood] = GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Wood)),
                [DmgTypeEnum.Thunder] = GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Thunder)),
                [DmgTypeEnum.Wind] = GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Wind)),
                [DmgTypeEnum.Earth] = GetDmg(DmgSaveEnum.Local, GetDmgKey(DmgEnum.DmgRecv, DmgTypeEnum.Earth))
            };
            var max = basises.Max(x => x.Value);
            return basises.FirstOrDefault(x => x.Value == max).Key;
        }

        /// <summary>
        /// Static shortcut of GetHighestRecvDmgTypeEnum.
        /// </summary>
        public static DmgTypeEnum sGetHighestRecvDmgTypeEnum()
        {
            return ModBattleEvent.Instance.GetHighestRecvDmgTypeEnum();
        }
        #endregion

        #region DmgInfo
        /// <summary>
        /// Damage statistic scope: Global = whole save game, Local = current battle.
        /// </summary>
        public enum DmgSaveEnum
        {
            Global = 1,
            Local = 2,
        }
        public IDictionary<string, long> GameDmg { get; set; } = new Dictionary<string, long>();

        [JsonIgnore]
        public IDictionary<string, long> BattleDmg { get; set; } = new Dictionary<string, long>();

        /// <summary>
        /// Reads a damage statistic (creates the key with 0 when missing).
        /// </summary>
        public long GetDmg(DmgSaveEnum dmgSaveEnum, string dmgKey)
        {
            switch (dmgSaveEnum)
            {
                case DmgSaveEnum.Global:
                    if (!GameDmg.ContainsKey(dmgKey))
                        GameDmg.Add(dmgKey, 0);
                    return GameDmg[dmgKey];
                case DmgSaveEnum.Local:
                    if (!BattleDmg.ContainsKey(dmgKey))
                        BattleDmg.Add(dmgKey, 0);
                    return BattleDmg[dmgKey];
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Static shortcut of GetDmg.
        /// </summary>
        public static long sGetDmg(DmgSaveEnum dmgSaveEnum, string dmgKey)
        {
            return Instance.GetDmg(dmgSaveEnum, dmgKey);
        }
        #endregion

        #region Funcs
        /// <summary>
        /// Unit that killed the given unit in the current battle.
        /// </summary>
        public static UnitCtrlBase GetKiller(UnitCtrlBase cunit)
        {
            return KillList.FirstOrDefault(x => x.Item1.data.createUnitSoleID == cunit.data.createUnitSoleID)?.Item2;
        }
        #endregion

        public static SceneBattle SceneBattle => SceneType.battle;
        public static UnitCtrlPlayer PlayerUnit => SceneBattle.battleData.playerUnit;
        public static UnitCtrlBase AttackingUnit { get; private set; }
        public static WorldUnitBase AttackingWorldUnit { get; private set; }
        public static bool IsWorldUnitAttacking => AttackingWorldUnit != null;
        public static bool IsPlayerAttacking => IsWorldUnitAttacking && AttackingWorldUnit.IsPlayer();
        public static UnitCtrlBase HitUnit { get; private set; }
        public static WorldUnitBase HitWorldUnit { get; private set; }
        public static bool IsWorldUnitHit => HitWorldUnit != null;
        public static Il2CppSystem.Collections.Generic.List<UnitCtrlBase> BattleUnits => SceneBattle.unit.allUnit;
        public static Il2CppSystem.Collections.Generic.List<UnitCtrlBase> BattleUnitsIncludeDie => SceneBattle.unit.allUnitIncludeDie;
        public static List<UnitCtrlBase> BattleMonsters => BattleUnits.ToArray().Where(x => x.IsMonster()).ToList();
        public static MapBuildTown Town { get; private set; }
        public static MapBuildSchool School { get; private set; }
        public static float BattleTime => SceneBattle.battleData.battleTime;
        public static float BattleStayTime => SceneBattle.battleData.battleStayTime;
        public static float RoomStayTime => SceneBattle.battleData.roomStayTime;
        public static List<Tuple<UnitCtrlBase, UnitCtrlBase>> KillList { get; } = new List<Tuple<UnitCtrlBase, UnitCtrlBase>>();

        /// <summary>
        /// Initializes every GameDmg key so later additions never fail.
        /// </summary>
        public override void OnLoadGame()
        {
            base.OnLoadGame();
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

        // [Trace]
        /// <summary>
        /// Resets battle statistics and the kill list; resolves the town/school at the player position.
        /// </summary>
        public override void OnBattleStart(ETypeData e)
        {
            base.OnBattleStart(e);

            var playerPos = g.world.playerUnit.GetUnitPos();

            Town = g.world.build.GetBuild<MapBuildTown>(playerPos);
            School = g.world.build.GetBuild<MapBuildSchool>(playerPos);
            BattleDmg.Clear();
            KillList.Clear();
            foreach (var de in (DmgEnum[])System.Enum.GetValues(typeof(DmgEnum)))
            {
                foreach (var type in (DmgTypeEnum[])System.Enum.GetValues(typeof(DmgTypeEnum)))
                {
                    var key = GetDmgKey(de, type);
                    BattleDmg.Add(key, 0);
                }
            }
        }

        /// <summary>
        /// Remembers the attacking and hit units (and their world units) before damage is applied.
        /// </summary>
        public override void OnBattleUnitHitDynIntHandler(UnitHitDynIntHandler e)
        {
            base.OnBattleUnitHitDynIntHandler(e);
            AttackingUnit = e?.hitData?.attackUnit;
            AttackingWorldUnit = AttackingUnit?.GetWorldUnit();
            HitUnit = e?.hitUnit;
            HitWorldUnit = HitUnit?.data?.TryCast<UnitDataHuman>()?.worldUnitData?.unit;
        }

        /// <summary>
        /// Adds the hit damage to the dealt/received statistics when the player is involved.
        /// </summary>
        public override void OnBattleUnitHit(UnitHit e)
        {
            base.OnBattleUnitHit(e);

            AttackingUnit = e?.hitData?.attackUnit;
            HitUnit = e?.hitUnit;

            var dmg = Math.Abs(e.hitData.hitValue);
            var attackUnitData = AttackingUnit.data.TryCast<UnitDataHuman>();
            var hitUnitData = HitUnit.data.TryCast<UnitDataHuman>();
            if (attackUnitData?.worldUnitData?.unit?.IsPlayer() ?? false)
            {
                var commonDmgKey = GetDmgKey(DmgEnum.DmgDealt, DmgTypeEnum.Damage);
                if (!GameDmg.ContainsKey(commonDmgKey))
                    GameDmg.Add(commonDmgKey, 0);
                if (!BattleDmg.ContainsKey(commonDmgKey))
                    BattleDmg.Add(commonDmgKey, 0);
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
                if (!GameDmg.ContainsKey(commonDmgKey))
                    GameDmg.Add(commonDmgKey, 0);
                if (!BattleDmg.ContainsKey(commonDmgKey))
                    BattleDmg.Add(commonDmgKey, 0);
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

        /// <summary>
        /// Records (victim, killer) in KillList.
        /// </summary>
        public override void OnBattleUnitDie(UnitDie e)
        {
            base.OnBattleUnitDie(e);
            if (e?.unit != null && e?.hitData?.attackUnit != null)
                KillList.Add(Tuple.Create(e.unit, e.hitData.attackUnit));
        }
    }
}
