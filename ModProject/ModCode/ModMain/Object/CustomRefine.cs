using MOD_nE7UL2.Enum;
using Newtonsoft.Json;
using UnityEngine;

namespace MOD_nE7UL2.Object
{
    public class CustomRefine
    {
        #region AdjTypeSeeder
        public static AdjTypeEnum[] RingAdjTypes = new AdjTypeEnum[]
        {
            AdjTypeEnum.Def, AdjTypeEnum.MHp, AdjTypeEnum.MMp, AdjTypeEnum.MSp,
            AdjTypeEnum.Nullify, AdjTypeEnum.RHp, AdjTypeEnum.RMp, AdjTypeEnum.RSp,
            AdjTypeEnum.SkillDamage, AdjTypeEnum.MinDamage,
            AdjTypeEnum.BasisBlade, AdjTypeEnum.BasisEarth, AdjTypeEnum.BasisFinger, AdjTypeEnum.BasisFire, AdjTypeEnum.BasisFist, AdjTypeEnum.BasisFroze,
            AdjTypeEnum.BasisPalm, AdjTypeEnum.BasisSpear, AdjTypeEnum.BasisSword, AdjTypeEnum.BasisThunder, AdjTypeEnum.BasisWind, AdjTypeEnum.BasisWood
        };

        public static AdjTypeEnum[] OutfitAdjTypes = new AdjTypeEnum[]
        {
            AdjTypeEnum.Def, AdjTypeEnum.MHp, AdjTypeEnum.MMp, AdjTypeEnum.RHp, AdjTypeEnum.RMp,
            AdjTypeEnum.Nullify, AdjTypeEnum.BlockChanceMax, AdjTypeEnum.BlockDmg,
            AdjTypeEnum.EvadeChance, AdjTypeEnum.EvadeChanceMax,
            AdjTypeEnum.BasisBlade, AdjTypeEnum.BasisEarth, AdjTypeEnum.BasisFinger, AdjTypeEnum.BasisFire, AdjTypeEnum.BasisFist, AdjTypeEnum.BasisFroze,
            AdjTypeEnum.BasisPalm, AdjTypeEnum.BasisSpear, AdjTypeEnum.BasisSword, AdjTypeEnum.BasisThunder, AdjTypeEnum.BasisWind, AdjTypeEnum.BasisWood
        };

        public static AdjTypeEnum[] ArtifactAdjTypes = new AdjTypeEnum[]
        {
            AdjTypeEnum.Atk, AdjTypeEnum.Def, AdjTypeEnum.Speed, AdjTypeEnum.Manashield,
            AdjTypeEnum.Nullify, AdjTypeEnum.SkillDamage, AdjTypeEnum.MinDamage,
            AdjTypeEnum.BlockChanceMax, AdjTypeEnum.BlockDmg,
            AdjTypeEnum.EvadeChance, AdjTypeEnum.EvadeChanceMax,
            AdjTypeEnum.SCritChance, AdjTypeEnum.SCritChanceMax, AdjTypeEnum.SCritDamage
        };

        public static AdjLevelEnum[] AdjLevels = new AdjLevelEnum[]
        {
            AdjLevelEnum.Common, AdjLevelEnum.Uncommon, AdjLevelEnum.Rare, AdjLevelEnum.Myst, AdjLevelEnum.Lgendary, AdjLevelEnum.Beast, AdjLevelEnum.Antique
        };

        public static AdjTypeEnum[] GetCustomAdjSeeder(DataProps.PropsData props)
        {
            if (props?.propsItem?.IsRing() != null)
                return RingAdjTypes;
            if (props?.propsItem?.IsOutfit() != null)
                return OutfitAdjTypes;
            if (props?.propsItem?.IsArtifact() != null)
                return ArtifactAdjTypes;
            return AdjTypeEnum.GetAllEnums<AdjTypeEnum>();
        }
        #endregion

        public int Index { get; set; }
        public float RandomMultiplier { get; set; }
        public AdjTypeEnum AdjType { get; set; }
        public AdjLevelEnum AdjLevel { get; set; }

        public CustomRefine()
        {
        }

        public CustomRefine(DataProps.PropsData props, int index)
        {
            Index = index;
            var seeder = GetCustomAdjSeeder(props);
            AdjType = seeder[props.soleID[index - 1] % seeder.Length];
            AdjLevel = AdjLevels[props.soleID[index] % AdjLevels.Length];
            RandomMultiplier = CommonTool.Random(0.50f, 1.50f);
        }

        public double GetRefineCustommAdjValue(WorldUnitBase wunit, DataProps.PropsData props, int refineLvl)
        {
            if (!IsEnable(refineLvl))
                return 0;
            var r = 0.001f * props.propsInfoBase.grade + 0.0002f * props.propsInfoBase.level;
            return AdjType.GetBaseValue(wunit) * r * refineLvl * AdjLevel.Multiplier * RandomMultiplier;
        }

        public bool IsEnable(int refineLvl)
        {
            return refineLvl >= Index * 100;
        }

        public Color GetColor(int refineLvl)
        {
            return IsEnable(refineLvl) ? AdjLevel.Color : AdjLevelEnum.None.Color;
        }

        public string GetText(WorldUnitBase wunit, DataProps.PropsData props, int refineLvl)
        {
            return $"+{AdjType.Label}: {GetRefineCustommAdjValue(wunit, props, refineLvl).ToString(AdjType.ValueFormat)} {(IsEnable(refineLvl) ? string.Empty : $"(Req {Index * 100})")}";
        }
    }
}
