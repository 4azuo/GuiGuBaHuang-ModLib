using MOD_nE7UL2.Enum;
using MOD_nE7UL2.Object;
using ModLib.Enum;
using ModLib.Mod;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace MOD_nE7UL2
{
    [InGameCustomSettings("game_configs.json", "update 102")]
    public sealed class ModMain : ModMaster<InGameStts>
    {
        public override string ModName => "MOD_nE7UL2";
        public override string ModId => "nE7UL2";
        public ModStts ModSettings { get; set; }
        public static new ModMain ModObj => ModMaster.ModObj as ModMain;

        public override void OnInitConf()
        {
            base.OnInitConf();
            if (g.conf.roleAttributeLimit._allConfList.ToArray().Where(x => x.key.EndsWith("Max")).All(x => x.value == int.MaxValue))
                return;
            ModSettings = JsonConvert.DeserializeObject<ModStts>(ConfHelper.ReadConfData("mod_configs.json"));
            foreach (var item in g.conf.roleGrade._allConfList)
            {
                item.exp = (item.exp * ModSettings.LevelExpRatio).Parse<int>();
            }
            //unlimit attribute
            foreach (var item in g.conf.roleAttributeLimit._allConfList)
            {
                if (item.key.EndsWith("Max"))
                    item.value = int.MaxValue;
            }
            //balance artifact attribute
            foreach (var item in g.conf.artifactShape._allConfList)
            {
                item.durable *= 5;
                item.spCost *= 6;

                item.hp *= 5;
                item.atk *= 3;
                item.def *= 3;
            }
            //skillmastery need more exp
            foreach (var item in g.conf.battleSkillMastery._allConfList)
            {
                item.grade1 = (int)(item.grade1 * 1.5f);
                item.grade2 = (int)(item.grade2 * 1.6f);
                item.grade3 = (int)(item.grade3 * 1.8f);
                item.grade4 = (int)(item.grade4 * 2.0f);
                item.grade5 = (int)(item.grade5 * 2.3f);
                item.grade6 = (int)(item.grade6 * 2.6f);
                item.grade7 = (int)(item.grade7 * 3.0f);
                item.grade8 = (int)(item.grade8 * 3.5f);
                item.grade9 = (int)(item.grade9 * 4.0f);
                item.grade10 = (int)(item.grade10 * 5.0f);
            }
            //skill need more mpCost
            //skill cooldown faster
            //skill need more requirements
            foreach (var item in g.conf.battleSkillValue._allConfList)
            {
                if (item.key.EndsWith("_mpCost"))
                {
                    item.value1 = (item.value1.Parse<float>() * 1.70f).Parse<int>().ToString();
                    item.value2 = (item.value2.Parse<float>() * 1.85f).Parse<int>().ToString();
                    item.value3 = (item.value3.Parse<float>() * 2.50f).Parse<int>().ToString();
                    item.value4 = (item.value4.Parse<float>() * 2.80f).Parse<int>().ToString();
                    item.value5 = (item.value5.Parse<float>() * 3.50f).Parse<int>().ToString();
                    item.value6 = (item.value6.Parse<float>() * 5.00f).Parse<int>().ToString();
                    item.value7 = (item.value7.Parse<float>() * 7.00f).Parse<int>().ToString();
                    item.value8 = (item.value8.Parse<float>() * 10.00f).Parse<int>().ToString();
                    item.value9 = (item.value9.Parse<float>() * 15.00f).Parse<int>().ToString();
                    item.value10 = (item.value10.Parse<float>() * 22.00f).Parse<int>().ToString();
                }
                else if (item.key.EndsWith("_cd"))
                {
                    item.value1 = (item.value1.Parse<float>() * 0.70f).Parse<int>().ToString();
                    item.value2 = (item.value2.Parse<float>() * 0.72f).Parse<int>().ToString();
                    item.value3 = (item.value3.Parse<float>() * 0.74f).Parse<int>().ToString();
                    item.value4 = (item.value4.Parse<float>() * 0.76f).Parse<int>().ToString();
                    item.value5 = (item.value5.Parse<float>() * 0.78f).Parse<int>().ToString();
                    item.value6 = (item.value6.Parse<float>() * 0.80f).Parse<int>().ToString();
                    item.value7 = (item.value7.Parse<float>() * 0.82f).Parse<int>().ToString();
                    item.value8 = (item.value8.Parse<float>() * 0.84f).Parse<int>().ToString();
                    item.value9 = (item.value9.Parse<float>() * 0.86f).Parse<int>().ToString();
                    item.value10 = (item.value10.Parse<float>() * 0.88f).Parse<int>().ToString();
                }
                else if (item.key.StartsWith("&zizhiBase_"))
                {
                    item.value1 = (item.value1.Parse<float>() * 1.10f).Parse<int>().ToString();
                    item.value2 = (item.value2.Parse<float>() * 1.40f).Parse<int>().ToString();
                    item.value3 = (item.value3.Parse<float>() * 2.50f).Parse<int>().ToString();
                    item.value4 = (item.value4.Parse<float>() * 3.00f).Parse<int>().ToString();
                    item.value5 = (item.value5.Parse<float>() * 5.00f).Parse<int>().ToString();
                    item.value6 = (item.value6.Parse<float>() * 5.75f).Parse<int>().ToString();
                    item.value7 = (item.value7.Parse<float>() * 10.00f).Parse<int>().ToString();
                    item.value8 = (item.value8.Parse<float>() * 12.00f).Parse<int>().ToString();
                    item.value9 = (item.value9.Parse<float>() * 18.00f).Parse<int>().ToString();
                    item.value10 = (item.value10.Parse<float>() * 20.00f).Parse<int>().ToString();
                }
                else if (item.key.StartsWith("&zizhiAdd_"))
                {
                    item.value1 = (item.value1.Parse<float>() * 1.10f).Parse<int>().ToString();
                    item.value2 = (item.value2.Parse<float>() * 1.15f).Parse<int>().ToString();
                    item.value3 = (item.value3.Parse<float>() * 1.20f).Parse<int>().ToString();
                    item.value4 = (item.value4.Parse<float>() * 1.25f).Parse<int>().ToString();
                    item.value5 = (item.value5.Parse<float>() * 1.35f).Parse<int>().ToString();
                    item.value6 = (item.value6.Parse<float>() * 1.45f).Parse<int>().ToString();
                    item.value7 = (item.value7.Parse<float>() * 1.65f).Parse<int>().ToString();
                    item.value8 = (item.value8.Parse<float>() * 1.90f).Parse<int>().ToString();
                    item.value9 = (item.value9.Parse<float>() * 2.20f).Parse<int>().ToString();
                    item.value10 = (item.value10.Parse<float>() * 2.50f).Parse<int>().ToString();
                }
                else if (item.key.StartsWith("&daodianBase_"))
                {
                    item.value1 = (item.value1.Parse<float>() * 1.10f).Parse<int>().ToString();
                    item.value2 = (item.value2.Parse<float>() * 1.20f).Parse<int>().ToString();
                    item.value3 = (item.value3.Parse<float>() * 1.30f).Parse<int>().ToString();
                    item.value4 = (item.value4.Parse<float>() * 1.40f).Parse<int>().ToString();
                    item.value5 = (item.value5.Parse<float>() * 1.50f).Parse<int>().ToString();
                    item.value6 = (item.value6.Parse<float>() * 1.65f).Parse<int>().ToString();
                    item.value7 = (item.value7.Parse<float>() * 1.80f).Parse<int>().ToString();
                    item.value8 = (item.value8.Parse<float>() * 1.95f).Parse<int>().ToString();
                    item.value9 = (item.value9.Parse<float>() * 2.10f).Parse<int>().ToString();
                    item.value10 = (item.value10.Parse<float>() * 2.40f).Parse<int>().ToString();
                }
                else if (item.key.StartsWith("&daodianAdd_"))
                {
                    item.value1 = (item.value1.Parse<float>() * 1.05f).Parse<int>().ToString();
                    item.value2 = (item.value2.Parse<float>() * 1.10f).Parse<int>().ToString();
                    item.value3 = (item.value3.Parse<float>() * 1.15f).Parse<int>().ToString();
                    item.value4 = (item.value4.Parse<float>() * 1.20f).Parse<int>().ToString();
                    item.value5 = (item.value5.Parse<float>() * 1.25f).Parse<int>().ToString();
                    item.value6 = (item.value6.Parse<float>() * 1.30f).Parse<int>().ToString();
                    item.value7 = (item.value7.Parse<float>() * 1.35f).Parse<int>().ToString();
                    item.value8 = (item.value8.Parse<float>() * 1.40f).Parse<int>().ToString();
                    item.value9 = (item.value9.Parse<float>() * 1.45f).Parse<int>().ToString();
                    item.value10 = (item.value10.Parse<float>() * 1.50f).Parse<int>().ToString();
                }
            }
            //balance item
            foreach (var props in g.conf.itemProps._allConfList)
            {
                var grade = 1;
                var level = Math.Max(1, Math.Min(6, props.level));
                var ratio = 1.00f;

                ConfItemPillItem pill = null;
                ConfRoleEffectItem roleEfx = null;
                ConfBattleEffectItem battleEfx = null;
                ConfItemRarityMaterialsItem rarity = null;
                ConfRingBaseItem ring = null;
                ConfItemHorseItem mount = null;
                ConfTownMarketItemItem buyable = null;
                ConfRoleGradeItem[] gradeInfo = null;
                ConfMakePillFurnaceItem furnace = null;
                ConfClothItemItem cloth = null;
                ConfItemHobbyItem hobby = null;
                ConfArtifactShapeItem artifact = null;
                ConfSchoolStockItem school = null;
                ConfWorldBlockHerbItem herb = null;
                ConfWorldBlockMineItem[] mines = null;
                ConfGameItemRewardItem[] battles = null;
                ConfRuneFormulaItem[] talismanFormulas = null;
                ConfMakePillFormulaItem pillFormula = null;

                if ((rarity = props.IsRareItem()) != null)
                {
                    if (rarity.hunger == 0)
                    {
                        rarity.hunger = rarity.grade;
                        ratio = 1.10f;
                    }
                    else
                    {
                        rarity.hunger *= rarity.grade;
                        ratio = 1.10f + rarity.hunger * 0.0001f;
                    }
                }

                if ((gradeInfo = props.IsBreakthroughItemA()) != null)
                {
                    ratio *= Math.Pow(PriceHelper.ITEM_TYPE_RATIO[ModItemTypeEnum.BreakthroughItemA], level).Parse<float>();
                    grade = Math.Max(1, gradeInfo.Select(x => x.grade).Max());
                    props.sale = PriceHelper.UpPrice(props.sale, grade, level, ratio);
                    props.worth = PriceHelper.UpPrice(props.worth, grade, level, ratio);
                    props.dieDrop = 1;
                    if (level >= 5) props.auction = 1;
                }
                else if ((gradeInfo = props.IsBreakthroughItemB()) != null)
                {
                    ratio *= Math.Pow(PriceHelper.ITEM_TYPE_RATIO[ModItemTypeEnum.BreakthroughItemB], level).Parse<float>();
                    grade = Math.Max(1, gradeInfo.Select(x => x.grade).Max());
                    props.sale = PriceHelper.UpPrice(props.sale, grade, level, ratio);
                    props.worth = PriceHelper.UpPrice(props.worth, grade, level, ratio);
                    props.dieDrop = 1;
                    if (level >= 5) props.auction = 1;
                }
                else if ((artifact = props.IsArtifact()) != null)
                {
                    ratio *= Math.Pow(PriceHelper.ITEM_TYPE_RATIO[ModItemTypeEnum.Artifact], level).Parse<float>();
                    grade = artifact.initGrade;
                    props.sale = PriceHelper.UpPrice(props.sale, grade, level, ratio);
                    props.worth = PriceHelper.UpPrice(props.worth, grade, level, ratio);
                    props.dieDrop = 1;
                    if (level >= 5) props.auction = 1;
                }
                else if ((pill = props.IsPotion()) != null)
                {
                    ratio *= Math.Pow(PriceHelper.ITEM_TYPE_RATIO[ModItemTypeEnum.Potion], level).Parse<float>();
                    grade = Math.Max(1, pill.grade);
                    props.sale = PriceHelper.UpPrice(props.sale, grade, level, ratio);
                    props.worth = PriceHelper.UpPrice(props.worth, grade, level, ratio);
                    pill.spCost = (pill.spCost.Parse<float>() * (1.25f + (grade * 0.10f + level * 0.02f))).Parse<int>();
                    pill.applyCD = (pill.applyCD.Parse<float>() * 0.25f).Parse<int>();

                    foreach (var efxId in pill.effectValue.Split('|'))
                    {
                        roleEfx = g.conf.roleEffect.GetItem(efxId.Parse<int>());
                        if (roleEfx != null && (roleEfx.value.StartsWith($"{UnitPropertyEnum.Hp.PropName}_1_") || roleEfx.value.StartsWith($"{UnitPropertyEnum.Mp.PropName}_1_")))
                        {
                            roleEfx.SetEfxValue(2, (roleEfx.GetEfxValue<int>(2) * (grade * 2.00f + level * 0.10f)).Parse<int>());
                        }
                    }
                }
                else if ((pill = props.IsPowerupPill()) != null)
                {
                    ratio *= Math.Pow(PriceHelper.ITEM_TYPE_RATIO[ModItemTypeEnum.PowerUpItem], level).Parse<float>();
                    grade = Math.Max(1, pill.grade);
                    props.sale = PriceHelper.UpPrice(props.sale, grade, level, ratio);
                    props.worth = PriceHelper.UpPrice(props.worth, grade, level, ratio);
                    pill.spCost = (pill.spCost.Parse<float>() * (2.50f + (grade * 0.16f + props.level * 0.04f))).Parse<int>();

                    foreach (var efxId in pill.effectValue.Split('|'))
                    {
                        battleEfx = g.conf.battleEffect.GetItem(efxId.Parse<int>());
                        if (battleEfx != null &&
                            (battleEfx.value1 == "3" || battleEfx.value1 == "4") &&
                            (battleEfx.value3.EndsWith("_tsgj") || battleEfx.value3.EndsWith("_tsfy")))
                        {
                            var efxValue = g.conf.battleSkillValue.GetItem(battleEfx.value3);
                            if (efxValue != null)
                            {
                                efxValue.value1 = (efxValue.value1.Parse<float>() * 3.00f).Parse<int>().ToString();
                                efxValue.value2 = (efxValue.value2.Parse<float>() * 3.10f).Parse<int>().ToString();
                                efxValue.value3 = (efxValue.value3.Parse<float>() * 3.30f).Parse<int>().ToString();
                                efxValue.value4 = (efxValue.value4.Parse<float>() * 3.50f).Parse<int>().ToString();
                                efxValue.value5 = (efxValue.value5.Parse<float>() * 3.80f).Parse<int>().ToString();
                                efxValue.value6 = (efxValue.value6.Parse<float>() * 4.20f).Parse<int>().ToString();
                                efxValue.value7 = (efxValue.value7.Parse<float>() * 4.60f).Parse<int>().ToString();
                                efxValue.value8 = (efxValue.value8.Parse<float>() * 5.00f).Parse<int>().ToString();
                                efxValue.value9 = (efxValue.value9.Parse<float>() * 5.50f).Parse<int>().ToString();
                                efxValue.value10 = (efxValue.value10.Parse<float>() * 6.00f).Parse<int>().ToString();
                            }
                        }
                    }
                }
                else if ((pill = props.IsBottleneckPill()) != null)
                {
                    ratio *= Math.Pow(PriceHelper.ITEM_TYPE_RATIO[ModItemTypeEnum.BottleneckItem], level).Parse<float>();
                    grade = Math.Max(1, pill.grade);
                    props.sale = PriceHelper.UpPrice(props.sale, grade, level, ratio);
                    props.worth = PriceHelper.UpPrice(props.worth, grade, level, ratio);
                    props.dieDrop = 1;
                }
                else if ((ring = props.IsRing()) != null)
                {
                    ratio *= Math.Pow(PriceHelper.ITEM_TYPE_RATIO[ModItemTypeEnum.Ring] + ring.capacity * 0.001f, level).Parse<float>();
                    grade = Math.Max(1, ring.grade);
                    props.sale = PriceHelper.UpPrice(props.sale, grade, level, ratio);
                    props.worth = PriceHelper.UpPrice(props.worth, grade, level, ratio);
                    if (level >= 5) props.auction = 1;
                }
                else if ((mount = props.IsMount()) != null)
                {
                    ratio *= Math.Pow(PriceHelper.ITEM_TYPE_RATIO[ModItemTypeEnum.Mount], level).Parse<float>();
                    grade = Math.Max(1, mount.grade);
                    props.sale = PriceHelper.UpPrice(props.sale, grade, level, ratio);
                    props.worth = PriceHelper.UpPrice(props.worth, grade, level, ratio);
                    if (level >= 5) props.auction = 1;
                }
                else if ((furnace = props.IsFurnace()) != null)
                {
                    ratio *= Math.Pow(PriceHelper.ITEM_TYPE_RATIO[ModItemTypeEnum.Furnace], level).Parse<float>();
                    props.sale = PriceHelper.UpPrice(props.sale, grade, level, ratio);
                    props.worth = PriceHelper.UpPrice(props.worth, grade, level, ratio);
                    props.dieDrop = 1;
                    if (level >= 5) props.auction = 1;

                    //furnace dur x5
                    var dur = level * 50;
                    foreach (var efxId in furnace.effectValue.Split('|'))
                    {
                        roleEfx = g.conf.roleEffect.GetItem(efxId.Parse<int>());
                        if (roleEfx != null && roleEfx.value.StartsWith($"randomValue_furnaceDurable_"))
                        {
                            roleEfx.SetEfxValue(2, dur);
                            roleEfx.SetEfxValue(3, dur);
                        }
                        else if (roleEfx != null && roleEfx.value.StartsWith($"fixValue_0_"))
                        {
                            roleEfx.SetEfxValue(2, dur);
                        }
                    }
                }
                else if ((herb = props.IsHerbItem()) != null)
                {
                    ratio *= Math.Pow(PriceHelper.ITEM_TYPE_RATIO[ModItemTypeEnum.Herb], level).Parse<float>();
                    props.sale = PriceHelper.UpPrice(props.sale, grade, level, ratio);
                    props.worth = PriceHelper.UpPrice(props.worth, grade, level, ratio);
                }
                else if ((mines = props.IsMineItem()) != null)
                {
                    ratio *= Math.Pow(PriceHelper.ITEM_TYPE_RATIO[ModItemTypeEnum.Mine], level).Parse<float>();
                    props.sale = PriceHelper.UpPrice(props.sale, grade, level, ratio);
                    props.worth = PriceHelper.UpPrice(props.worth, grade, level, ratio);
                }
                //else if ((battles = props.IsBattleItem()) != null)
                //{
                //    ratio *= Math.Pow(PriceHelper.ITEM_TYPE_RATIO[ModItemTypeEnum.BattleItem], level).Parse<float>();
                //    props.sale = PriceHelper.UpPrice(props.sale, grade, level, ratio);
                //    props.worth = PriceHelper.UpPrice(props.worth, grade, level, ratio);
                //}
                else if ((buyable = props.IsMarketBuyableItem()) != null)
                {
                    ratio *= Math.Pow(PriceHelper.ITEM_TYPE_RATIO[ModItemTypeEnum.MarketItem], level).Parse<float>();
                    grade = Math.Max(1, Math.Min(10, buyable.level.Split('|').Min().Parse<int>()));
                    props.sale = PriceHelper.UpPrice(props.sale, grade, level, ratio);
                    props.worth = PriceHelper.UpPrice(props.worth, grade, level, ratio);
                }
                else if ((school = props.IsSchoolBuyableItem()) != null)
                {
                    ratio *= Math.Pow(PriceHelper.ITEM_TYPE_RATIO[ModItemTypeEnum.SchoolItem], level).Parse<float>();
                    grade = Math.Max(1, Math.Min(10, school.area));
                    props.sale = PriceHelper.UpPrice(props.sale, grade, level, ratio);
                    props.worth = PriceHelper.UpPrice(props.worth, grade, level, ratio);
                }
                else if ((cloth = props.IsOutfit()) != null)
                {
                    //do nothing
                }
                else if ((hobby = props.IsHobby()) != null)
                {
                    //do nothing
                }
                else
                {
                    ratio *= Math.Pow(PriceHelper.ITEM_TYPE_RATIO[ModItemTypeEnum.Others], level).Parse<float>();
                    props.sale = PriceHelper.UpPrice(props.sale, grade, level, ratio);
                    props.worth = PriceHelper.UpPrice(props.worth, grade, level, ratio);
                }

                ////debug
                //var debugId = 6061251;
                //if (props.id == debugId)
                //{
                //    if (pill != null) DebugHelper.WriteLine($"{debugId}: pill");
                //    if (roleEfx != null) DebugHelper.WriteLine($"{debugId}: roleEfx");
                //    if (battleEfx != null) DebugHelper.WriteLine($"{debugId}: battleEfx");
                //    if (rarity != null) DebugHelper.WriteLine($"{debugId}: rarity");
                //    if (ring != null) DebugHelper.WriteLine($"{debugId}: ring");
                //    if (mount != null) DebugHelper.WriteLine($"{debugId}: mount");
                //    if (buyable != null) DebugHelper.WriteLine($"{debugId}: buyable");
                //    if (gradeInfo != null) DebugHelper.WriteLine($"{debugId}: gradeInfo");
                //    if (furnace != null) DebugHelper.WriteLine($"{debugId}: furnace");
                //    if (cloth != null) DebugHelper.WriteLine($"{debugId}: cloth");
                //    if (hobby != null) DebugHelper.WriteLine($"{debugId}: hobby");
                //    if (artifact != null) DebugHelper.WriteLine($"{debugId}: artifact");
                //    if (school != null) DebugHelper.WriteLine($"{debugId}: school");
                //    if (herb != null) DebugHelper.WriteLine($"{debugId}: herb");
                //    if (mines != null) DebugHelper.WriteLine($"{debugId}: mines");
                //    if (battles != null) DebugHelper.WriteLine($"{debugId}: battles");
                //    if (talismanFormulas != null) DebugHelper.WriteLine($"{debugId}: talismanFormulas");
                //    if (pillFormula != null) DebugHelper.WriteLine($"{debugId}: pillFormula");
                //    DebugHelper.WriteLine($"{debugId}: ratio:{ratio}, sale:{props.sale}, worth:{props.worth}");
                //}
            }
            foreach (var props in g.conf.itemProps._allConfList)
            {
                if (props.sale < 0)
                    continue;

                var grade = 1;
                var level = Math.Max(1, Math.Min(6, props.level));
                var ratio = 1.00f;

                ConfRuneFormulaItem[] talismanFormulas;
                ConfMakePillFormulaItem pillFormula;
                ConfTownFactotySellArtifactItem refiningArtifact;

                if ((talismanFormulas = props.IsTalismanRecipe()) != null)
                {
                    ratio *= Math.Pow(PriceHelper.ITEM_TYPE_RATIO[ModItemTypeEnum.TalismanRecipe], level).Parse<float>();
                    props.sale = PriceHelper.UpPrice(props.sale, grade, level, ratio);
                    props.worth = PriceHelper.UpPrice(props.worth, grade, level, ratio);
                }
                else if ((pillFormula = props.IsPillRecipe()) != null)
                {
                    var pills = new string[] { pillFormula.pillA, pillFormula.pillB, pillFormula.pillC, pillFormula.pillD, pillFormula.pillE, pillFormula.pillF };
                    var expensestPrice = pills.Select(x => g.conf.itemProps.GetItem(x.Parse<int>())).Max(x => x.worth);
                    var cheapestPrice = pills.Select(x => g.conf.itemProps.GetItem(x.Parse<int>())).Min(x => x.worth);
                    var avgPrice = (expensestPrice + cheapestPrice) / 2;
                    ratio *= Math.Pow(PriceHelper.ITEM_TYPE_RATIO[ModItemTypeEnum.PillRecipe], level).Parse<float>();
                    grade = pillFormula.grade;
                    props.sale = PriceHelper.UpPrice(avgPrice, grade, level, ratio);
                    props.worth = PriceHelper.UpPrice(avgPrice, grade, level, ratio);

                    var factory = g.conf.townFactotySell._allConfList.ToArray().FirstOrDefault(x => x.id == props.id);
                    if (factory != null)
                        factory.makePrice = props.worth / 6;
                }
                else if ((refiningArtifact = props.IsTownRefiningArtifact()) != null)
                {
                    var artifactId = g.conf.artifactShapeMaterial.GetItem(refiningArtifact.id).shape;
                    var productShape = g.conf.artifactShape.GetItem(artifactId);
                    var productProp = g.conf.itemProps.GetItem(artifactId);

                    props.sale = productProp.sale / 3;
                    props.worth = productProp.worth / 3;
                }
            }
            foreach (var refine in g.conf.townRefine._allConfList)
            {
                refine.moneyCost = g.conf.itemProps.GetItem(refine.rewardID).worth / 4;
            }
            //balance skill
            foreach (var item in g.conf.itemSkill._allConfList)
            {
                //balance price
                var grade = Math.Max(1, Math.Min(10, item.grade));
                var level = Math.Max(1, Math.Min(6, item.level));
                var ratio = Math.Pow(PriceHelper.ITEM_TYPE_RATIO[ModItemTypeEnum.Skill], level).Parse<float>();
                item.price = PriceHelper.UpPrice(item.price, grade, level, ratio);
                item.cost = PriceHelper.UpPrice(item.cost, grade, level, ratio);
                item.contribution = PriceHelper.UpPrice(item.contribution, grade, level, ratio);
                item.sale = PriceHelper.UpPrice(item.sale, grade, level, ratio);
                item.worth = PriceHelper.UpPrice(item.worth, grade, level, ratio);
            }
        }
    }
}
