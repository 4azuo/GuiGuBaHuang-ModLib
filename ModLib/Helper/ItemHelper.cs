using ModLib.Attributes;
using ModLib.Enum;
using System.Linq;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for working with game items and props.
    /// Provides utilities for item creation, copying, quality checks, and inventory management.
    /// </summary>
    [ActionCat("Item")]
    public static class ItemHelper
    {
        /// <summary>
        /// Creates a copy of prop data with optional count override.
        /// </summary>
        /// <param name="org">Original prop</param>
        /// <param name="count">New count (-1 to keep original)</param>
        /// <returns>Copied prop data</returns>
        public static DataProps.PropsData CopyProp(this DataProps.PropsData org, int count = -1)
        {
            if (count == -1)
                count = org.propsCount;
            return DataProps.PropsData.New(org.propsID, count, org.propsType, org.values);
        }

        /// <summary>
        /// Creates a new prop data from components.
        /// </summary>
        /// <param name="propId">Prop ID</param>
        /// <param name="type">Prop type</param>
        /// <param name="values">Prop values</param>
        /// <param name="count">Count</param>
        /// <returns>New prop data</returns>
        public static DataProps.PropsData CopyProp(int propId, DataProps.PropsDataType type, UnhollowerBaseLib.Il2CppStructArray<int> values, int count)
        {
            return DataProps.PropsData.New(propId, count, type, values);
        }

        /// <summary>
        /// Checks if prop is a potion (HP/MP recovery).
        /// </summary>
        /// <param name="props">Prop item</param>
        /// <returns>Pill item or null</returns>
        public static ConfItemPillItem IsPotion(this ConfItemPropsItem props)
        {
            var pill = g.conf.itemPill.GetItem(props.id);
            var roleEfx = g.conf.roleEffect.GetItem(props.id);
            if (pill != null && roleEfx != null && (roleEfx.value.StartsWith($"{UnitPropertyEnum.Hp.PropName}_1_") || roleEfx.value.StartsWith($"{UnitPropertyEnum.Mp.PropName}_1_")))
                return pill;
            return null;
        }

        /// <summary>
        /// Checks if prop is a power-up pill (temporary stat boost).
        /// </summary>
        /// <param name="props">Prop item</param>
        /// <returns>Pill item or null</returns>
        public static ConfItemPillItem IsPowerupPill(this ConfItemPropsItem props)
        {
            var pill = g.conf.itemPill.GetItem(props.id);
            var battleEfx = g.conf.battleEffect.GetItem(props.id);
            if (pill != null && battleEfx != null &&
                (battleEfx.value1 == "3" || battleEfx.value1 == "4") &&
                (battleEfx.value3.EndsWith("_tsgj") || battleEfx.value3.EndsWith("_tsfy")))
                return pill;
            return null;
        }

        /// <summary>
        /// Checks if prop is a bottleneck pill (removes cultivation bottlenecks).
        /// </summary>
        /// <param name="props">Prop item</param>
        /// <returns>Pill item or null</returns>
        public static ConfItemPillItem IsBottleneckPill(this ConfItemPropsItem props)
        {
            var pill = g.conf.itemPill.GetItem(props.id);
            var roleEfx = g.conf.roleEffect.GetItem(props.id);
            var bottleneckList = (BottleneckEnum[])System.Enum.GetValues(typeof(BottleneckEnum));
            if (pill != null && roleEfx != null && bottleneckList.Any(x => roleEfx.value == $"clearFeature_100_{x}"))
                return pill;
            return null;

        }

        /// <summary>
        /// Checks if prop is an outfit (clothing).
        /// </summary>
        /// <param name="props">Prop item</param>
        /// <returns>Outfit item or null</returns>
        public static ConfClothItemItem IsOutfit(this ConfItemPropsItem props)
        {
            return g.conf.clothItem.GetItem(props.id);
        }

        /// <summary>
        /// Checks if prop is a pill.
        /// </summary>
        /// <param name="props">Prop item</param>
        /// <returns>Pill item or null</returns>
        public static ConfItemPillItem IsPill(this ConfItemPropsItem props)
        {
            return g.conf.itemPill.GetItem(props.id);
        }

        /// <summary>
        /// Checks if prop is a rare/special material.
        /// </summary>
        /// <param name="props">Prop item</param>
        /// <returns>Rare item config or null</returns>
        public static ConfItemRarityMaterialsItem IsRareItem(this ConfItemPropsItem props)
        {
            return g.conf.itemRarityMaterials.GetItem(props.id);
        }

        /// <summary>
        /// Checks if prop is a ring.
        /// </summary>
        /// <param name="props">Prop item</param>
        /// <returns>Ring item or null</returns>
        public static ConfRingBaseItem IsRing(this ConfItemPropsItem props)
        {
            return g.conf.ringBase.GetItem(props.id);
        }

        /// <summary>
        /// Checks if prop is a mount.
        /// </summary>
        /// <param name="props">Prop item</param>
        /// <returns>Mount item or null</returns>
        public static ConfItemHorseItem IsMount(this ConfItemPropsItem props)
        {
            return g.conf.itemHorse.GetItem(props.id);
        }

        /// <summary>
        /// Checks if prop is a hobby item.
        /// </summary>
        /// <param name="props">Prop item</param>
        /// <returns>Hobby item or null</returns>
        public static ConfItemHobbyItem IsHobby(this ConfItemPropsItem props)
        {
            return g.conf.itemHobby.GetItem(props.id);
        }

        /// <summary>
        /// Checks if prop is an artifact.
        /// </summary>
        /// <param name="props">Prop item</param>
        /// <returns>Artifact config or null</returns>
        public static ConfArtifactShapeItem IsArtifact(this ConfItemPropsItem props)
        {
            return g.conf.artifactShape.GetItem(props.id);
        }

        /// <summary>
        /// Checks if prop is a town refining artifact.
        /// </summary>
        /// <param name="props">Prop item</param>
        /// <returns>Town artifact config or null</returns>
        public static ConfTownFactotySellArtifactItem IsTownRefiningArtifact(this ConfItemPropsItem props)
        {
            return g.conf.townFactotySellArtifact.GetItem(props.id);
        }

        /// <summary>
        /// Checks if prop is a charm.
        /// </summary>
        /// <param name="props">Prop item</param>
        /// <returns>True if charm</returns>
        public static bool IsCharm(this ConfItemPropsItem props)
        {
            return props.GetItemClassName() == "Charm";
        }

        /// <summary>
        /// Checks if prop is a transfer item.
        /// </summary>
        /// <param name="props">Prop item</param>
        /// <returns>Transfer item or null</returns>
        public static ConfItemTransferItem IsTranferItem(this ConfItemPropsItem props)
        {
            return g.conf.itemTransfer.GetItem(props.id);
        }

        /// <summary>
        /// Checks if prop is used for breakthrough (A or B).
        /// </summary>
        /// <param name="props">Prop item</param>
        /// <returns>Array of grade items or null</returns>
        public static ConfRoleGradeItem[] IsBreakthroughItem(this ConfItemPropsItem props)
        {
            var rs = g.conf.roleGrade._allConfList.ToArray().Where(x =>
                x.itemShowA.Split('_').Contains(props.id.ToString()) || x.itemShowB.Split('_').Contains(props.id.ToString())).ToArray();
            if (rs.Length == 0)
                return null;
            return rs;
        }

        /// <summary>
        /// Checks if prop is used for breakthrough type A.
        /// </summary>
        /// <param name="props">Prop item</param>
        /// <returns>Array of grade items or null</returns>
        public static ConfRoleGradeItem[] IsBreakthroughItemA(this ConfItemPropsItem props)
        {
            var rs = g.conf.roleGrade._allConfList.ToArray().Where(x => x.itemShowA.Split('_').Contains(props.id.ToString())).ToArray();
            if (rs.Length == 0)
                return null;
            return rs;
        }

        /// <summary>
        /// Checks if prop is used for breakthrough type B.
        /// </summary>
        /// <param name="props">Prop item</param>
        /// <returns>Array of grade items or null</returns>
        public static ConfRoleGradeItem[] IsBreakthroughItemB(this ConfItemPropsItem props)
        {
            var rs = g.conf.roleGrade._allConfList.ToArray().Where(x => x.itemShowB.Split('_').Contains(props.id.ToString())).ToArray();
            if (rs.Length == 0)
                return null;
            return rs;
        }

        /// <summary>
        /// Checks if prop is a pill-making furnace.
        /// </summary>
        /// <param name="props">Prop item</param>
        /// <returns>Furnace item or null</returns>
        public static ConfMakePillFurnaceItem IsFurnace(this ConfItemPropsItem props)
        {
            return g.conf.makePillFurnace.GetItem(props.id);
        }

        /// <summary>
        /// Checks if prop is a pill recipe.
        /// </summary>
        /// <param name="props">Prop item</param>
        /// <returns>Recipe item or null</returns>
        public static ConfMakePillFormulaItem IsPillRecipe(this ConfItemPropsItem props)
        {
            return g.conf.makePillFormula.GetItem(props.id);
        }

        /// <summary>
        /// Checks if prop is a talisman recipe.
        /// </summary>
        /// <param name="props">Prop item</param>
        /// <returns>Array of recipe items or null</returns>
        public static ConfRuneFormulaItem[] IsTalismanRecipe(this ConfItemPropsItem props)
        {
            var rs = g.conf.runeFormula._allConfList.ToArray().Where(x => x.formulaId == props.id).ToArray();
            if (rs.Length == 0)
                return null;
            return rs;
        }

        /// <summary>
        /// Checks if prop is a herb item (gatherable).
        /// </summary>
        /// <param name="props">Prop item</param>
        /// <returns>Herb config or null</returns>
        public static ConfWorldBlockHerbItem IsHerbItem(this ConfItemPropsItem props)
        {
            return g.conf.worldBlockHerb._allConfList.ToArray().FirstOrDefault(x => x.item == props.id);
        }

        /// <summary>
        /// Checks if prop is a mine item (minable resource).
        /// </summary>
        /// <param name="props">Prop item</param>
        /// <returns>Array of mine configs or null</returns>
        public static ConfWorldBlockMineItem[] IsMineItem(this ConfItemPropsItem props)
        {
            var rs = g.conf.worldBlockMine._allConfList.ToArray().Where(x => x.item.Contains(props.id)).ToArray();
            if (rs.Length == 0)
                return null;
            return rs;
        }

        //Too slow
        //public static ConfGameItemRewardItem[] IsBattleItem(this ConfItemPropsItem props)
        //{
        //    var rs = g.conf.gameItemReward._allConfList.ToArray().Where(x => x.item == props.id).ToArray();
        //    if (rs.Length == 0)
        //        return null;
        //    return rs;
        //}

        /// <summary>
        /// Checks if prop can be bought in market.
        /// </summary>
        /// <param name="props">Prop item</param>
        /// <returns>Market item config or null</returns>
        public static ConfTownMarketItemItem IsMarketBuyableItem(this ConfItemPropsItem props)
        {
            return g.conf.townMarketItem.GetItem(props.id);
        }

        /// <summary>
        /// Checks if prop can be bought in school.
        /// </summary>
        /// <param name="props">Prop item</param>
        /// <returns>School item config or null</returns>
        public static ConfSchoolStockItem IsSchoolBuyableItem(this ConfItemPropsItem props)
        {
            return g.conf.schoolStock.GetItemConf(props.id);
        }

        /// <summary>
        /// Checks if prop can ONLY be bought in market (not school).
        /// </summary>
        /// <param name="props">Prop item</param>
        /// <returns>Market item config or null</returns>
        public static ConfTownMarketItemItem IsOnlyMarketBuyableItem(this ConfItemPropsItem props)
        {
            var school = props.IsSchoolBuyableItem();
            var market = props.IsMarketBuyableItem();
            if (school == null && market != null)
                return market;
            return null;
        }

        /// <summary>
        /// Checks if prop can ONLY be bought in school (not market).
        /// </summary>
        /// <param name="props">Prop item</param>
        /// <returns>School item config or null</returns>
        public static ConfSchoolStockItem IsOnlySchoolBuyableItem(this ConfItemPropsItem props)
        {
            var school = props.IsSchoolBuyableItem();
            var market = props.IsMarketBuyableItem();
            if (school != null && market == null)
                return school;
            return null;
        }

        /// <summary>
        /// Gets the localized item class name.
        /// </summary>
        /// <param name="props">Prop item</param>
        /// <returns>English class name</returns>
        public static string GetItemClassName(this ConfItemPropsItem props)
        {
            var nameKey = g.conf.itemType.GetItem(props.className)?.name;
            if (g.conf.localText.allText.ContainsKey(nameKey))
            {
                return g.conf.localText.allText[nameKey].en;
            }
            return null;
        }

        /// <summary>
        /// Gets the martial type name.
        /// </summary>
        /// <param name="t">Martial type</param>
        /// <returns>Type name</returns>
        public static string GetMartialTypeName(this MartialType t)
        {
            return g.conf.battleSkillAttack.GetMartialTypeName(t);
        }

        /// <summary>
        /// Checks if prop is partial/stackable.
        /// </summary>
        /// <param name="p">Prop data</param>
        /// <returns>Check result</returns>
        public static CheckEnum IsPartialItem(this DataProps.PropsData p)
        {
            if (p == null)
                return CheckEnum.Null;
            if (p.propsItem?.isOverlay == 1 && p.propsType != DataProps.PropsDataType.Martial)
                return CheckEnum.True;
            return CheckEnum.False;
        }
    }
}