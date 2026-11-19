using MOD_nE7UL2.Enum;
using MOD_nE7UL2.Mod;
using System.Linq;
using UnityEngine;
using ModLib.Helper;
using static MOD_nE7UL2.Object.ModStts;

namespace MOD_nE7UL2.Object
{
    /// <summary>
    /// One random adjustment slot of a refined item (type, quality level, random multiplier).
    /// </summary>
    public class CustomRefine
    {
        public static _CustomRefineConfigs Configs => ModMain.ModObj.ModSettings.CustomRefineConfigs;

        #region AdjTypeSeeder
        public static AdjTypeEnum[] RingAdjTypes = new AdjTypeEnum[]
        {
            AdjTypeEnum.Def, AdjTypeEnum.MHp, AdjTypeEnum.MMp, AdjTypeEnum.MSp, AdjTypeEnum.MDp,
            AdjTypeEnum.Nullify, AdjTypeEnum.RHp, AdjTypeEnum.RMp, AdjTypeEnum.RSp, AdjTypeEnum.RDp,
            AdjTypeEnum.SkillDamage, AdjTypeEnum.MinDamage, AdjTypeEnum.SummonPower, AdjTypeEnum.ItemCD, AdjTypeEnum.SkillCD,
            AdjTypeEnum.BasisBlade, AdjTypeEnum.BasisEarth, AdjTypeEnum.BasisFinger, AdjTypeEnum.BasisFire, AdjTypeEnum.BasisFist, AdjTypeEnum.BasisFroze,
            AdjTypeEnum.BasisPalm, AdjTypeEnum.BasisSpear, AdjTypeEnum.BasisSword, AdjTypeEnum.BasisThunder, AdjTypeEnum.BasisWind, AdjTypeEnum.BasisWood
        };

        public static AdjTypeEnum[] OutfitAdjTypes = new AdjTypeEnum[]
        {
            AdjTypeEnum.Def, AdjTypeEnum.MHp, AdjTypeEnum.MMp, AdjTypeEnum.MDp, AdjTypeEnum.RHp, AdjTypeEnum.RMp, AdjTypeEnum.RDp,
            AdjTypeEnum.Nullify, AdjTypeEnum.BlockChanceMax, AdjTypeEnum.BlockDmg,
            AdjTypeEnum.EvadeChance, AdjTypeEnum.EvadeChanceMax, AdjTypeEnum.ItemCD, AdjTypeEnum.SkillCD,
            AdjTypeEnum.BasisBlade, AdjTypeEnum.BasisEarth, AdjTypeEnum.BasisFinger, AdjTypeEnum.BasisFire, AdjTypeEnum.BasisFist, AdjTypeEnum.BasisFroze,
            AdjTypeEnum.BasisPalm, AdjTypeEnum.BasisSpear, AdjTypeEnum.BasisSword, AdjTypeEnum.BasisThunder, AdjTypeEnum.BasisWind, AdjTypeEnum.BasisWood
        };

        public static AdjTypeEnum[] ArtifactAdjTypes = new AdjTypeEnum[]
        {
            AdjTypeEnum.StealHp, AdjTypeEnum.StealMp, AdjTypeEnum.StealSp, AdjTypeEnum.InstantKill,
            AdjTypeEnum.Atk, AdjTypeEnum.Def, AdjTypeEnum.Speed, AdjTypeEnum.Manashield,
            AdjTypeEnum.Nullify, AdjTypeEnum.SkillDamage, AdjTypeEnum.MinDamage, AdjTypeEnum.SummonPower,
            AdjTypeEnum.BlockChanceMax, AdjTypeEnum.BlockDmg,
            AdjTypeEnum.EvadeChance, AdjTypeEnum.EvadeChanceMax, AdjTypeEnum.ItemCD, AdjTypeEnum.SkillCD,
            AdjTypeEnum.SCritChance, AdjTypeEnum.SCritChanceMax, AdjTypeEnum.SCritDamage
        };

        public static AdjTypeEnum[] MountAdjTypes = new AdjTypeEnum[]
        {
            AdjTypeEnum.Atk, AdjTypeEnum.Def, AdjTypeEnum.Nullify, AdjTypeEnum.Manashield, AdjTypeEnum.SummonPower,
            AdjTypeEnum.StealHp, AdjTypeEnum.StealMp, AdjTypeEnum.StealSp, AdjTypeEnum.InstantKill,
            AdjTypeEnum.SkillDamage, AdjTypeEnum.MinDamage
        };

        /// <summary>
        /// Adjustment types allowed for the item kind (ring, outfit, artifact, mount).
        /// </summary>
        public static AdjTypeEnum[] GetCustomAdjSeeder(DataProps.PropsData props)
        {
            if (props?.propsItem?.IsRing() != null)
                return RingAdjTypes;
            if (props?.propsItem?.IsOutfit() != null)
                return OutfitAdjTypes;
            if (props?.propsItem?.IsArtifact() != null)
                return ArtifactAdjTypes;
            if (props?.propsItem?.IsMount() != null)
                return MountAdjTypes;
            return AdjTypeEnum.GetAllEnums<AdjTypeEnum>();
        }

        /// <summary>
        /// Random adjustment type allowed at the grade.
        /// </summary>
        public static AdjTypeEnum RandomCustomAdj(AdjTypeEnum[] seeder, int gradeLvl)
        {
            var t = seeder.Where(x => x.MinGradeLvl <= gradeLvl).ToArray();
            // an item below every minimum grade (e.g. grade 0 outfits): the types of the lowest minimum grade
            // (was a division by zero when its info window opened, 2026-11-18)
            if (t.Length == 0 && seeder.Length > 0)
            {
                var min = seeder.Min(x => x.MinGradeLvl);
                t = seeder.Where(x => x.MinGradeLvl == min).ToArray();
            }
            return t[CommonTool.Random(0, 10000) % t.Length];
        }
        #endregion

        public int Index { get; set; }
        public float RandomMultiplier { get; set; }
        public AdjTypeEnum AdjType { get; set; }
        public AdjLevelEnum AdjLevel { get; set; }

        /// <summary>
        /// Creates a slot (empty for deserialization, or random for an item and index).
        /// </summary>
        public CustomRefine()
        {
        }

        /// <summary>
        /// Creates a slot (empty for deserialization, or random for an item and index).
        /// </summary>
        public CustomRefine(DataProps.PropsData props, int index)
        {
            Index = index;
            var seeder = GetCustomAdjSeeder(props);
            AdjType = RandomCustomAdj(seeder, props.propsInfoBase.grade);
            AdjLevel = Configs.RandomRefineLevel(CommonTool.Random(0.00f, 100.00f));
            RandomMultiplier = CommonTool.Random(0.60f, 1.40f);
        }

        /// <summary>
        /// Value of the adjustment: base value x grade/level ratio x effective refine level x quality x multiplier (cached monthly).
        /// </summary>
        public double GetRefineCustommAdjValue(WorldUnitBase wunit, DataProps.PropsData props, int refineLvl)
        {
            var key = $"{props.soleID}_{Index}";
            if (CustomRefineEvent.Instance.CachedValues.ContainsKey(key))
                return CustomRefineEvent.Instance.CachedValues[key];
            if (!IsEnable(refineLvl))
                return 0;
            refineLvl = _DecreaseLevel(refineLvl);
            var r = 0.001f * props.propsInfoBase.grade + 0.0002f * props.propsInfoBase.level;
            var v = AdjType.GetBaseValue(wunit) * r * refineLvl * AdjLevel.Multiplier * RandomMultiplier;
            CustomRefineEvent.Instance.CachedValues[key] = v;
            return v;
        }

        /// <summary>
        /// Effective refine level for this slot (each slot needs 50 more levels).
        /// </summary>
        private int _DecreaseLevel(int refineLvl)
        {
            return refineLvl - (Index * 50);
        }

        /// <summary>
        /// True when the refine level unlocks this slot.
        /// </summary>
        public bool IsEnable(int refineLvl)
        {
            return refineLvl >= Index * 100;
        }

        /// <summary>
        /// Display color of the quality.
        /// </summary>
        public Color GetColor(int refineLvl)
        {
            return IsEnable(refineLvl) ? AdjLevel.Color : AdjLevelEnum.None.Color;
        }

        /// <summary>
        /// Display text of the adjustment.
        /// </summary>
        public string GetText(WorldUnitBase wunit, DataProps.PropsData props, int refineLvl)
        {
            return $"+{GameTool.LS(AdjType.Label)}: {GetRefineCustommAdjValue(wunit, props, refineLvl).ToString(AdjType.ValueFormat)} {(IsEnable(refineLvl) ? string.Empty : $"(Req {Index * 100})")}";
        }

        /// <summary>
        /// Copy of the slot.
        /// </summary>
        public CustomRefine Clone()
        {
            var clone = new CustomRefine
            {
                Index = this.Index,
                AdjType = this.AdjType,
                AdjLevel = this.AdjLevel,
                RandomMultiplier = this.RandomMultiplier
            };
            return clone;
        }
    }
}
