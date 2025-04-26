using ModLib.Enum;
using System.Linq;

public static class ItemHelper
{
    public static DataProps.PropsData CopyProp(this DataProps.PropsData org, int count = -1)
    {
        if (count == -1)
            count = org.propsCount;
        return DataProps.PropsData.New(org.propsID, count, org.propsType, org.values);
    }

    public static DataProps.PropsData CopyProp(int propId, DataProps.PropsDataType type, UnhollowerBaseLib.Il2CppStructArray<int> values, int count)
    {
        return DataProps.PropsData.New(propId, count, type, values);
    }

    public static ConfItemPillItem IsPotion(this ConfItemPropsItem props)
    {
        var pill = g.conf.itemPill.GetItem(props.id);
        var roleEfx = g.conf.roleEffect.GetItem(props.id);
        if (pill != null && roleEfx != null && (roleEfx.value.StartsWith($"{UnitPropertyEnum.Hp.PropName}_1_") || roleEfx.value.StartsWith($"{UnitPropertyEnum.Mp.PropName}_1_")))
            return pill;
        return null;
    }

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

    public static ConfItemPillItem IsBottleneckPill(this ConfItemPropsItem props)
    {
        var pill = g.conf.itemPill.GetItem(props.id);
        var roleEfx = g.conf.roleEffect.GetItem(props.id);
        var bottleneckList = (BottleneckEnum[])System.Enum.GetValues(typeof(BottleneckEnum));
        if (pill != null && roleEfx != null && bottleneckList.Any(x => roleEfx.value == $"clearFeature_100_{x}"))
            return pill;
        return null;

    }

    public static ConfClothItemItem IsOutfit(this ConfItemPropsItem props)
    {
        return g.conf.clothItem.GetItem(props.id);
    }

    public static ConfItemPillItem IsPill(this ConfItemPropsItem props)
    {
        return g.conf.itemPill.GetItem(props.id);
    }

    public static ConfItemRarityMaterialsItem IsRareItem(this ConfItemPropsItem props)
    {
        return g.conf.itemRarityMaterials.GetItem(props.id);
    }

    public static ConfRingBaseItem IsRing(this ConfItemPropsItem props)
    {
        return g.conf.ringBase.GetItem(props.id);
    }

    public static ConfItemHorseItem IsMount(this ConfItemPropsItem props)
    {
        return g.conf.itemHorse.GetItem(props.id);
    }

    public static ConfItemHobbyItem IsHobby(this ConfItemPropsItem props)
    {
        return g.conf.itemHobby.GetItem(props.id);
    }

    public static ConfArtifactShapeItem IsArtifact(this ConfItemPropsItem props)
    {
        return g.conf.artifactShape.GetItem(props.id);
    }

    public static ConfTownFactotySellArtifactItem IsTownRefiningArtifact(this ConfItemPropsItem props)
    {
        return g.conf.townFactotySellArtifact.GetItem(props.id);
    }

    public static bool IsCharm(this ConfItemPropsItem props)
    {
        return props.GetItemClassName() == "Charm";
    }

    public static ConfItemTransferItem IsTranferItem(this ConfItemPropsItem props)
    {
        return g.conf.itemTransfer.GetItem(props.id);
    }

    public static ConfRoleGradeItem[] IsBreakthroughItem(this ConfItemPropsItem props)
    {
        var rs = g.conf.roleGrade._allConfList.ToArray().Where(x =>
            x.itemShowA.Split('_').Contains(props.id.ToString()) || x.itemShowB.Split('_').Contains(props.id.ToString())).ToArray();
        if (rs.Length == 0)
            return null;
        return rs;
    }

    public static ConfRoleGradeItem[] IsBreakthroughItemA(this ConfItemPropsItem props)
    {
        var rs = g.conf.roleGrade._allConfList.ToArray().Where(x => x.itemShowA.Split('_').Contains(props.id.ToString())).ToArray();
        if (rs.Length == 0)
            return null;
        return rs;
    }

    public static ConfRoleGradeItem[] IsBreakthroughItemB(this ConfItemPropsItem props)
    {
        var rs = g.conf.roleGrade._allConfList.ToArray().Where(x => x.itemShowB.Split('_').Contains(props.id.ToString())).ToArray();
        if (rs.Length == 0)
            return null;
        return rs;
    }

    public static ConfMakePillFurnaceItem IsFurnace(this ConfItemPropsItem props)
    {
        return g.conf.makePillFurnace.GetItem(props.id);
    }

    public static ConfMakePillFormulaItem IsPillRecipe(this ConfItemPropsItem props)
    {
        return g.conf.makePillFormula.GetItem(props.id);
    }

    public static ConfRuneFormulaItem[] IsTalismanRecipe(this ConfItemPropsItem props)
    {
        var rs = g.conf.runeFormula._allConfList.ToArray().Where(x => x.formulaId == props.id).ToArray();
        if (rs.Length == 0)
            return null;
        return rs;
    }

    public static ConfWorldBlockHerbItem IsHerbItem(this ConfItemPropsItem props)
    {
        return g.conf.worldBlockHerb._allConfList.ToArray().FirstOrDefault(x => x.item == props.id);
    }

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

    public static ConfTownMarketItemItem IsMarketBuyableItem(this ConfItemPropsItem props)
    {
        return g.conf.townMarketItem.GetItem(props.id);
    }

    public static ConfSchoolStockItem IsSchoolBuyableItem(this ConfItemPropsItem props)
    {
        return g.conf.schoolStock.GetItemConf(props.id);
    }

    public static ConfTownMarketItemItem IsOnlyMarketBuyableItem(this ConfItemPropsItem props)
    {
        var school = props.IsSchoolBuyableItem();
        var market = props.IsMarketBuyableItem();
        if (school == null && market != null)
            return market;
        return null;
    }

    public static ConfSchoolStockItem IsOnlySchoolBuyableItem(this ConfItemPropsItem props)
    {
        var school = props.IsSchoolBuyableItem();
        var market = props.IsMarketBuyableItem();
        if (school != null && market == null)
            return school;
        return null;
    }

    public static string GetItemClassName(this ConfItemPropsItem props)
    {
        var nameKey = g.conf.itemType.GetItem(props.className)?.name;
        if (g.conf.localText.allText.ContainsKey(nameKey))
        {
            return g.conf.localText.allText[nameKey].en;
        }
        return null;
    }

    public static string GetMartialTypeName(this MartialType t)
    {
        return g.conf.battleSkillAttack.GetMartialTypeName(t);
    }
}