using System;
using System.Collections.Generic;

namespace ModLib.Helper
{
    /// <summary>
    /// Hits made by code (MartialTool.HitDanagePow) of a kind the game and the mods understand: a weapon type (blade..finger)
    /// or an element (fire..wood), the martial art it counts as (skillBase: the mods add its mastery, basis...).
    /// The game cannot take a hit of no kind (weapon type 0 and element 0: MartialTool.HitDanagePow throws IndexOutOfRange),
    /// so such a hit takes the kind of its martial art, else DEFAULT_MAGIC (fire). First made for Dao of Creation.
    /// </summary>
    public static class HitHelper
    {
        /// <summary>The element of a hit of no kind without a martial art behind it (fire).</summary>
        public const int DEFAULT_MAGIC = 1;

        /// <summary>Weapon types of the game (HitData.weaponType, MartialSkillData.weaponType): blade 1 .. finger 6.</summary>
        public static readonly IReadOnlyDictionary<string, int> WEAPON_TYPES = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["blade"] = 1, ["spear"] = 2, ["sword"] = 3, ["fist"] = 4, ["palm"] = 5, ["finger"] = 6,
        };

        /// <summary>Elements of the game (HitData.magicType, the six spiritual roots): fire 1 .. wood 6 (frost = froze).</summary>
        public static readonly IReadOnlyDictionary<string, int> MAGIC_TYPES = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["fire"] = 1, ["frost"] = 2, ["froze"] = 2, ["thunder"] = 3, ["wind"] = 4, ["earth"] = 5, ["wood"] = 6,
        };

        /// <summary>The 12 attributes of the game (weapon types, then elements), as named by the effect catalogs.</summary>
        public static readonly string[] ATTRIBUTES = { "blade", "spear", "sword", "fist", "palm", "finger", "fire", "frost", "thunder", "wind", "earth", "wood" };

        /// <summary>
        /// Weapon type / element (Key / Value) of the first of <paramref name="attributes"/> that is one of the 12 of the game
        /// ("light", "none"... skipped); 0 / 0 when none is.
        /// </summary>
        public static KeyValuePair<int, int> TypesOf(IEnumerable<string> attributes)
        {
            if (attributes == null) return new KeyValuePair<int, int>(0, 0);
            foreach (var a in attributes)
            {
                if (a == null) continue;
                if (WEAPON_TYPES.TryGetValue(a, out var w)) return new KeyValuePair<int, int>(w, 0);
                if (MAGIC_TYPES.TryGetValue(a, out var m)) return new KeyValuePair<int, int>(0, m);
            }
            return new KeyValuePair<int, int>(0, 0);
        }

        /// <summary>Weapon type / element of one attribute ("sword": 3 / 0, "wood": 0 / 6), 0 / 0 when it is not one of the 12.</summary>
        public static KeyValuePair<int, int> TypesOf(string attribute) => TypesOf(new[] { attribute });

        /// <summary>
        /// A kind the game takes: <paramref name="weaponType"/> / <paramref name="magicType"/> when one is set, else the kind of
        /// the martial art behind the hit (<paramref name="skillWeapon"/> / <paramref name="skillMagic"/>), else DEFAULT_MAGIC.
        /// </summary>
        public static KeyValuePair<int, int> SafeTypes(int weaponType, int magicType, int skillWeapon = 0, int skillMagic = 0)
        {
            if (weaponType > 0 || magicType > 0) return new KeyValuePair<int, int>(Math.Max(0, weaponType), Math.Max(0, magicType));
            if (skillWeapon > 0) return new KeyValuePair<int, int>(skillWeapon, 0);
            return new KeyValuePair<int, int>(0, skillMagic > 0 ? skillMagic : DEFAULT_MAGIC);
        }

        /// <summary>
        /// A hit of <paramref name="attacker"/> of that kind (SafeTypes: never 0 / 0), counted as a hit of
        /// <paramref name="skillBase"/> when given (the mods add its mastery). Damage clamped to an int.
        /// </summary>
        public static MartialTool.HitData NewHit(UnitCtrlBase attacker, int weaponType, int magicType, long damage, SkillAttack skillBase = null)
        {
            var sw = 0; var sm = 0;
            if (weaponType <= 0 && magicType <= 0 && skillBase != null)
            {
                try { sw = skillBase.data?.weaponType?.value ?? 0; sm = skillBase.data?.magicType?.value ?? 0; } catch { }
            }
            var t = SafeTypes(weaponType, magicType, sw, sm);
            var hd = new MartialTool.HitData(attacker, null, t.Key, t.Value, (int)Math.Min(int.MaxValue, Math.Max(0, damage)));
            if (skillBase != null)
            {
                try { hd.skillBase = skillBase; } catch (Exception ex) { DebugHelper.WriteLine($"HitHelper: skillBase: {ex.Message}"); }
            }
            return hd;
        }

        /// <summary>A hit of an attribute of the game ("sword", "fire"... see TypesOf; another: the kind of skillBase, else fire).</summary>
        public static MartialTool.HitData NewHit(UnitCtrlBase attacker, string attribute, long damage, SkillAttack skillBase = null)
        {
            var t = TypesOf(attribute);
            return NewHit(attacker, t.Key, t.Value, damage, skillBase);
        }

        /// <summary>A hit made and dealt to <paramref name="target"/> (UnitHit / UnitDie events of the game and the mods).</summary>
        public static MartialTool.HitData Deal(UnitCtrlBase target, MartialTool.HitData hit)
        {
            if (target == null || hit == null) return hit;
            MartialTool.HitDanagePow(hit, target);
            return hit;
        }
    }
}
