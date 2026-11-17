using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MOD_nE7UL2.Story
{
    /// <summary>
    /// Casting rules of a story role (docs/Taoist/06-Storylines-Draft.md, section 1): sex, share of the lifespan lived,
    /// looks, character traits, alignment, grade compared with the player, best weapon basis / art. Every rule is
    /// optional; the best-scoring fitting NPC gets the role.
    /// </summary>
    public class StoryRole
    {
        public string Name;
        public UnitSexType? Sex;
        /// <summary>Share of the lifespan lived (0-1).</summary>
        public float MinLived = 0f, MaxLived = 1f;
        /// <summary>Least beauty (game tiers: 50 average, 250 pretty, 450 beautiful, 650 divine).</summary>
        public int MinBeauty = int.MinValue;
        public int MaxBeauty = int.MaxValue;
        /// <summary>Inner traits wanted (1-7), any of them; preferred, not required.</summary>
        public int[] InTraits;
        /// <summary>Outer traits wanted (8-19), any of them; preferred, not required.</summary>
        public int[] OutTraits;
        /// <summary>true righteous, false demonic, null any.</summary>
        public bool? Righteous;
        /// <summary>Grade compared with the player (inclusive).</summary>
        public int MinGradeOffset = -99, MaxGradeOffset = 99;
        /// <summary>Best basis among the weapon bases (preferred): UnitPropertyEnum of a basis.</summary>
        public UnitPropertyEnum[] BestBasis;
        /// <summary>Must be a sect member.</summary>
        public bool InSect;
        /// <summary>Must have no sect.</summary>
        public bool NoSect;
        /// <summary>Extra filter.</summary>
        public Func<WorldUnitBase, bool> Filter;

        public static readonly UnitPropertyEnum[] WEAPON_BASES =
        {
            UnitPropertyEnum.BasisBlade, UnitPropertyEnum.BasisSpear, UnitPropertyEnum.BasisSword,
            UnitPropertyEnum.BasisFist, UnitPropertyEnum.BasisPalm, UnitPropertyEnum.BasisFinger,
        };

        /// <summary>Hard rules (a unit failing one cannot get the role).</summary>
        public bool Fits(WorldUnitBase u, WorldUnitBase player)
        {
            if (u == null || u.isDie || u.IsPlayer()) return false;
            var p = u.data.unitData.propertyData;
            if (Sex.HasValue && p.sex != Sex.Value) return false;
            var lived = Lived(u);
            if (lived < MinLived || lived > MaxLived) return false;
            if (p.beauty < MinBeauty || p.beauty > MaxBeauty) return false;
            if (Righteous.HasValue && u.IsRighteous() != Righteous.Value) return false;
            var off = u.GetGradeLvl() - player.GetGradeLvl();
            if (off < MinGradeOffset || off > MaxGradeOffset) return false;
            var sect = !string.IsNullOrEmpty(u.data.unitData.schoolID);
            if (InSect && !sect) return false;
            if (NoSect && sect) return false;
            return Filter == null || Filter(u);
        }

        /// <summary>Soft rules: wanted traits and best basis.</summary>
        public int Score(WorldUnitBase u)
        {
            var p = u.data.unitData.propertyData;
            var s = 0;
            if (InTraits != null && InTraits.Contains(p.inTrait)) s += 3;
            if (OutTraits != null && (OutTraits.Contains(p.outTrait1) || OutTraits.Contains(p.outTrait2))) s += 2;
            if (BestBasis != null && BestBasis.Contains(BestWeaponBasis(u))) s += 2;
            return s;
        }

        public static float Lived(WorldUnitBase u) =>
            u.GetProperty<int>(UnitPropertyEnum.Age) / (float)Math.Max(1, u.GetProperty<int>(UnitPropertyEnum.Life));

        public static UnitPropertyEnum BestWeaponBasis(WorldUnitBase u) =>
            WEAPON_BASES.OrderByDescending(b => u.GetProperty<int>(b)).First();

        /// <summary>
        /// Makes a created NPC fit the role: sex (the game creates units of both sexes: retried by the caller), age,
        /// traits, alignment, best basis.
        /// </summary>
        public void Shape(WorldUnitBase u)
        {
            var p = u.data.unitData.propertyData;
            var life = Math.Max(12, u.GetProperty<int>(UnitPropertyEnum.Life));
            var lived = (Math.Max(0f, MinLived) + Math.Min(1f, MaxLived)) / 2f;
            u.SetProperty<int>(UnitPropertyEnum.Age, Math.Max(16 * 12, (int)(life * lived)));
            if (InTraits != null && InTraits.Length > 0) p.inTrait = InTraits[0];
            if (OutTraits != null && OutTraits.Length > 0 && !OutTraits.Contains(p.outTrait1) && !OutTraits.Contains(p.outTrait2))
                p.outTrait1 = OutTraits[0];
            if (Righteous.HasValue)
            {
                u.SetProperty<int>(UnitPropertyEnum.StandUp, Righteous.Value ? 500 : 0);
                u.SetProperty<int>(UnitPropertyEnum.StandDown, Righteous.Value ? 0 : 500);
            }
            if (BestBasis != null && BestBasis.Length > 0)
            {
                var top = WEAPON_BASES.Max(b => u.GetProperty<int>(b));
                u.SetProperty<int>(BestBasis[0], top + 10);
            }
            if (MinBeauty > int.MinValue && p.beauty < MinBeauty) p.beauty = MinBeauty;
        }
    }

    /// <summary>Picks or creates the NPC of a role.</summary>
    public static class StoryCasting
    {
        /// <summary>Yearly chance (%) to create the NPC of a role nobody fits, after `years` of waiting: 2 % a year, at most 50 %.</summary>
        public static float CreateChance(int years) => Math.Min(50f, 2f * Math.Max(0, years));

        /// <summary>Best fitting living NPC (not already in a story, not the player's companion / descendant), or null.</summary>
        public static WorldUnitBase Pick(StoryRole role, HashSet<string> taken, IEnumerable<WorldUnitBase> units, WorldUnitBase player)
        {
            var family = new HashSet<string>(FamilyFundEvent.Members().Select(x => x.GetUnitId()));
            return units.Where(u => u != null && !taken.Contains(u.GetUnitId()) && !family.Contains(u.GetUnitId()) && role.Fits(u, player))
                .OrderByDescending(role.Score).ThenBy(u => CommonTool.Random(0, 1000)).FirstOrDefault();
        }

        /// <summary>
        /// Creates an NPC for the role near the player: units of the grade are created until one has the sex
        /// (the others are removed), then shaped. Null when it failed.
        /// </summary>
        public static WorldUnitBase Create(StoryRole role, WorldUnitBase player)
        {
            var grade = Math.Max(1, Math.Min(10, player.GetGradeLvl() + Math.Max(role.MinGradeOffset, Math.Min(0, role.MaxGradeOffset))));
            for (var i = 0; i < 20; i++) // a random sex: 20 tries (one in a million to miss)
            {
                var units = g.world.run.npc.AddUnit(grade, 1);
                var u = units == null || units.Count == 0 ? null : units[0];
                if (u == null) return null;
                if (role.Sex.HasValue && u.data.unitData.propertyData.sex != role.Sex.Value)
                {
                    g.world.unit.DelUnitSecret(u);
                    continue;
                }
                role.Shape(u);
                try { u.SetUnitRandomPos(player.GetUnitPos(), 6); } catch { }
                return u;
            }
            return null;
        }
    }
}
