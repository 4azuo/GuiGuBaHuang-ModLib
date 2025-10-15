using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using UnityEngine.UI;
using EGameTypeData;
using ModLib.Enum;
using MOD_nE7UL2.Enum;
using ModLib.Object;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.UI_EVENT)]
    public class UIEvent : ModEvent
    {
        public static UIEvent Instance { get; set; }

        private Text uiArtifactInfo_textBasicTitle;
        private Text uiArtifactInfo_textBasicAdj1;
        private Text uiArtifactInfo_textBasicAdj2;
        private Text uiArtifactInfo_textBasicAdj3;
        private Text uiArtifactInfo_textExpertLvl;
        private Text uiArtifactInfo_textExpertAdj1;
        private Text uiArtifactInfo_textExpertAdj2;
        private Text uiArtifactInfo_textRefineTitle;
        private Text uiArtifactInfo_textRefineAdj1;
        private Text uiArtifactInfo_textRefineAdj2;
        private Text uiArtifactInfo_textRefineAdj3;
        private Text uiArtifactInfo_textRefineAdj4;
        private Text uiArtifactInfo_textRefineAdj5;
        private UINPCInfoPreview uiMartialExpertInfo;
        private Text uiMartialExpertInfo_textExpertLvl;
        private Text uiMartialExpertInfo_textAdj1;
        private Text uiMartialExpertInfo_textAdj2;
        private Text uiMartialExpertInfo_textAdj3;
        private Text uiMartialExpertInfo_textAdj4;
        private Text uiMartialExpertInfo_textAdj5;
        private Text uiPropInfo_textRefineTitle;
        private Text uiPropInfo_textRefineAdj1;
        private Text uiPropInfo_textRefineAdj2;
        private Text uiPropInfo_textRefineAdj3;
        private Text uiPropInfo_textRefineAdj4;

        public void OnUIOpen(OpenUIEnd e)
        {
            /*
             * UI
             */
            if (e.uiType.uiName == UIType.MapMain.uiName)
            {
                var ui = g.ui.GetUI<UIMapMain>(UIType.MapMain);
                ui.playerInfo.textPiscesPendantCount.gameObject.SetActive(false);
                ui.playerInfo.goAddLuckRoot.SetActive(false);
            }
            else
            if (e.uiType.uiName == UIType.Town.uiName)
            {
                var ui = new UICover<UITown>(e.ui);
                {
                    if (MapBuildPropertyEvent.IsTownGuardian(ui.UI.town, g.world.playerUnit))
                    {
                        ui.AddButton(ui.LastCol - 5, ui.FirstRow + 3, () => MapBuildPropertyEvent.OpenUITownManage(ui.UI.town), GameTool.LS("other500020005")).Size(300, 80);
                    }
                    else
                    {
                        ui.AddButton(ui.LastCol - 5, ui.FirstRow + 3, () => MapBuildPropertyEvent.OpenUITownGuardians(ui.UI.town), $"Tax: {MapBuildPropertyEvent.GetTax(ui.UI.town, g.world.playerUnit)} Spirit Stones\nView the town guardians").Size(300, 80);
                    }
                    ui.AddToolTipButton(ui.LastCol - 8, ui.FirstRow + 3, GameTool.LS("other500020020"));
                }
            }
            else
            if (e.uiType.uiName == UIType.School.uiName)
            {
                var ui = new UICover<UISchool>(e.ui);
                {
                    if (!MapBuildPropertyEvent.IsSchoolMember(ui.UI.school, g.world.playerUnit))
                    {
                        ui.AddButton(ui.LastCol - 5, ui.FirstRow + 3, null, $"Tax: {MapBuildPropertyEvent.GetTax(ui.UI.school, g.world.playerUnit)} Spirit Stones").Size(300, 80).Enable = false;
                    }
                }
            }
            else
            if (e.uiType.uiName == UIType.NPCInfo.uiName)
            {
                var ui = new UICover<UINPCInfo>(UIType.NPCInfo);
                {
                    ui.AddText(0, 0, string.Empty).Format(Color.white).SetParentTransform(ui.UI.uiProperty.goGroupRoot)
                        .Pos(ui.UI.uiProperty.goItem6, 36, -150).SetWork(new UIItemWork
                        {
                            UpdateAct = (x) => x.Set($"+Hp: {UnitModifyHelper.GetMartialAdjHp(ui.UI.unit)}")
                        });
                    ui.AddText(0, 0, string.Empty).Format(Color.white).SetParentTransform(ui.UI.uiProperty.goGroupRoot)
                        .Pos(ui.UI.uiProperty.goItem7, 72, -150).SetWork(new UIItemWork
                        {
                            UpdateAct = (x) => x.Set($"+Mp: {UnitModifyHelper.GetSpiritualAdjMp(ui.UI.unit)}")
                        });
                    ui.AddText(0, 0, string.Empty).Format(Color.white).SetParentTransform(ui.UI.uiProperty.goGroupRoot)
                        .Pos(ui.UI.uiProperty.goItem8, 72, -150).SetWork(new UIItemWork
                        {
                            UpdateAct = (x) => x.Set($"+Sp: {UnitModifyHelper.GetArtisanshipAdjSp(ui.UI.unit)}")
                        });

                    ui.AddText(0, 0, string.Empty).Format(Color.white).SetParentTransform(ui.UI.uiSkill.textPoint1.transform)
                        .Pos(ui.UI.uiSkill.textPoint1.transform, 0, -12).SetWork(new UIItemWork
                        {
                            UpdateAct = (x) => x.Set($"+Hp: {UnitModifyHelper.GetAbiPointAdjHp(ui.UI.unit)}")
                        });
                    ui.AddText(0, 0, string.Empty).Format(Color.white).SetParentTransform(ui.UI.uiSkill.textPoint1.transform)
                        .Pos(ui.UI.uiSkill.textPoint1.transform, 0, -36).SetWork(new UIItemWork
                        {
                            UpdateAct = (x) => x.Set($"+Mp: {UnitModifyHelper.GetAbiPointAdjMp(ui.UI.unit)}")
                        });
                }
                ui.IsAutoUpdate = true;
            }
            else
            if (e.uiType.uiName == UIType.PlayerInfo.uiName)
            {
                var ui = new UICover<UIPlayerInfo>(UIType.PlayerInfo);
                {
                    ui.AddToolTipButton(GameTool.LS("other500020028")).SetParentTransform(ui.UI.uiPropertyCommon.goGroupRoot).Pos(ui.UI.uiPropertyCommon.goItem1_En, 0, -160);
                    ui.AddToolTipButton(GameTool.LS("other500020038")).SetParentTransform(ui.UI.uiPropertyCommon.goGroupRoot).Pos(ui.UI.uiPropertyCommon.goItem2_En, 0, -160);

                    ui.AddText(0, 0, string.Empty).SetParentTransform(ui.UI.uiPropertyCommon.goGroupRoot)
                        .Pos(ui.UI.uiPropertyCommon.goItem6_En, 100, -160).SetWork(new UIItemWork
                        {
                            UpdateAct = (x) => x.Set($"+Hp: {UnitModifyHelper.GetMartialAdjHp(ui.UI.unit)}")
                        });
                    ui.AddToolTipButton(GameTool.LS("other500020033")).SetParentTransform(ui.UI.uiPropertyCommon.goGroupRoot).Pos(ui.UI.uiPropertyCommon.goItem6_En, 40, -160);
                    ui.AddText(0, 0, string.Empty).SetParentTransform(ui.UI.uiPropertyCommon.goGroupRoot)
                        .Pos(ui.UI.uiPropertyCommon.goItem7_En, 100, -160).SetWork(new UIItemWork
                        {
                            UpdateAct = (x) => x.Set($"+Mp: {UnitModifyHelper.GetSpiritualAdjMp(ui.UI.unit)}")
                        });
                    ui.AddToolTipButton(GameTool.LS("other500020034")).SetParentTransform(ui.UI.uiPropertyCommon.goGroupRoot).Pos(ui.UI.uiPropertyCommon.goItem7_En, 40, -160);
                    ui.AddText(0, 0, string.Empty).SetParentTransform(ui.UI.uiPropertyCommon.goGroupRoot)
                        .Pos(ui.UI.uiPropertyCommon.goItem8_En, 100, -160).SetWork(new UIItemWork
                        {
                            UpdateAct = (x) => x.Set($"+Sp: {UnitModifyHelper.GetArtisanshipAdjSp(ui.UI.unit)}")
                        });
                    ui.AddToolTipButton(GameTool.LS("other500020035")).SetParentTransform(ui.UI.uiPropertyCommon.goGroupRoot).Pos(ui.UI.uiPropertyCommon.goItem8_En, 40, -160);

                    ui.AddText(0, 0, string.Empty).Format(Color.white).SetParentTransform(ui.UI.uiSkill.textPoint1.transform)
                        .Pos(ui.UI.uiSkill.textPoint1.transform, 0, -12).SetWork(new UIItemWork
                        {
                            UpdateAct = (x) => x.Set($"+Hp: {UnitModifyHelper.GetAbiPointAdjHp(ui.UI.unit)}")
                        });
                    ui.AddText(0, 0, string.Empty).Format(Color.white).SetParentTransform(ui.UI.uiSkill.textPoint1.transform)
                        .Pos(ui.UI.uiSkill.textPoint1.transform, 0, -36).SetWork(new UIItemWork
                        {
                            UpdateAct = (x) => x.Set($"+Mp: {UnitModifyHelper.GetAbiPointAdjMp(ui.UI.unit)}")
                        });
                    ui.AddToolTipButton(GameTool.LS("other500020036")).SetParentTransform(ui.UI.uiSkill.textPoint1.transform).Pos(ui.UI.uiSkill.textPoint1.transform, 0, -60);
                    ui.AddToolTipButton(GameTool.LS("other500020037")).SetParentTransform(ui.UI.uiSkill.goGroupRoot).Pos(ui.UI.uiSkill.goActionMartialRoot.transform, 0, 200);
                }
                ui.IsAutoUpdate = true;
            }
            else
            if (e.uiType.uiName == UIType.ArtifactInfo.uiName &&
                (g.ui.HasUI(UIType.PropSelect) || g.ui.HasUI(UIType.PlayerInfo) || g.ui.HasUI(UIType.TownStorage) || (g.ui.HasUI(UIType.NPCInfo) && g.world.playerUnit.GetLuck(UnitTypeLuckEnum.Merchant.Value.Parse<int>()) != null)))
            {
                var uiArtifactInfo = g.ui.GetUI<UIArtifactInfo>(UIType.ArtifactInfo);

                uiArtifactInfo.transform.position = new Vector3(0f, 0f);

                var artifactConf = uiArtifactInfo.shapeProp.propsItem.IsArtifact();
                var artifact = uiArtifactInfo.shapeProp.To<DataProps.PropsArtifact>();
                var soleId = uiArtifactInfo.shapeProp.soleID;
                var propsGrade = uiArtifactInfo.shapeProp.propsInfoBase.grade;
                var propsLevel = uiArtifactInfo.shapeProp.propsInfoBase.level;
                var expertLvl = ExpertEvent.GetExpertLvl(soleId, propsGrade, propsLevel);
                var expertExp = ExpertEvent.GetExpertExp(soleId);
                var expertNeedExp = ExpertEvent.GetExpertNeedExp(expertLvl + 1, propsGrade, propsLevel);

                //basic
                uiArtifactInfo_textBasicTitle = uiArtifactInfo.textGrade_En.Copy().Align().Format(Color.white, 15);
                uiArtifactInfo_textBasicAdj1 = uiArtifactInfo.textGrade_En.Copy().Align().Format(Color.white, 14);
                uiArtifactInfo_textBasicAdj2 = uiArtifactInfo.textGrade_En.Copy().Align().Format(Color.white, 14);
                uiArtifactInfo_textBasicAdj3 = uiArtifactInfo.textGrade_En.Copy().Align().Format(Color.white, 14);

                uiArtifactInfo_textBasicTitle.Pos(uiArtifactInfo.textGrade_En.gameObject, 0, -20);
                uiArtifactInfo_textBasicAdj1.Pos(uiArtifactInfo.textGrade_En.gameObject, 5, -35);
                uiArtifactInfo_textBasicAdj2.Pos(uiArtifactInfo.textGrade_En.gameObject, 5, -50);
                uiArtifactInfo_textBasicAdj3.Pos(uiArtifactInfo.textGrade_En.gameObject, 5, -65);

                uiArtifactInfo_textBasicTitle.text = $"Basic:";
                uiArtifactInfo_textBasicAdj1.text = $"+Atk: {UnitModifyHelper.GetArtifactBasicAdjAtk(uiArtifactInfo.unit.GetDynProperty(UnitDynPropertyEnum.Attack).baseValue, uiArtifactInfo.shapeProp, artifact)}";
                uiArtifactInfo_textBasicAdj2.text = $"+Def: {UnitModifyHelper.GetArtifactBasicAdjDef(uiArtifactInfo.unit.GetDynProperty(UnitDynPropertyEnum.Defense).baseValue, uiArtifactInfo.shapeProp, artifact)}";
                uiArtifactInfo_textBasicAdj3.text = $"+Hp: {UnitModifyHelper.GetArtifactBasicAdjHp(uiArtifactInfo.unit.GetDynProperty(UnitDynPropertyEnum.HpMax).baseValue, uiArtifactInfo.shapeProp, artifact)}";

                //expert
                uiArtifactInfo_textExpertLvl = uiArtifactInfo.textGrade_En.Copy().Align().Format(Color.white, 15);
                uiArtifactInfo_textExpertAdj1 = uiArtifactInfo.textGrade_En.Copy().Align().Format(Color.white, 14);
                uiArtifactInfo_textExpertAdj2 = uiArtifactInfo.textGrade_En.Copy().Align().Format(Color.white, 14);

                uiArtifactInfo_textExpertLvl.Pos(uiArtifactInfo.textGrade_En.gameObject, 0, -80);
                uiArtifactInfo_textExpertAdj1.Pos(uiArtifactInfo.textGrade_En.gameObject, 5, -95);
                uiArtifactInfo_textExpertAdj2.Pos(uiArtifactInfo.textGrade_En.gameObject, 5, -110);

                uiArtifactInfo_textExpertLvl.text = $"Expert Lvl: {expertLvl} ({expertExp}/{expertNeedExp})";
                uiArtifactInfo_textExpertAdj1.text = $"+Atk: {UnitModifyHelper.GetArtifactExpertAtk(artifactConf.atk, expertLvl, propsGrade, propsLevel)}";
                uiArtifactInfo_textExpertAdj2.text = $"+Def: {UnitModifyHelper.GetArtifactExpertDef(artifactConf.def, expertLvl, propsGrade, propsLevel)}";

                //refine
                var refineLvl = CustomRefineEvent.GetRefineLvl(uiArtifactInfo.shapeProp);
                var customAdj1 = CustomRefineEvent.GetCustomAdjType(uiArtifactInfo.shapeProp, 1);
                var customAdj2 = CustomRefineEvent.GetCustomAdjType(uiArtifactInfo.shapeProp, 2);
                var customAdj3 = CustomRefineEvent.GetCustomAdjType(uiArtifactInfo.shapeProp, 3);

                uiArtifactInfo_textRefineTitle = uiArtifactInfo.textGrade_En.Copy().Align(TextAnchor.MiddleRight).Format(Color.white, 15);
                uiArtifactInfo_textRefineAdj1 = uiArtifactInfo.textGrade_En.Copy().Align(TextAnchor.MiddleRight).Format(Color.white, 14);
                uiArtifactInfo_textRefineAdj2 = uiArtifactInfo.textGrade_En.Copy().Align(TextAnchor.MiddleRight).Format(Color.white, 14);
                uiArtifactInfo_textRefineAdj3 = uiArtifactInfo.textGrade_En.Copy().Align(TextAnchor.MiddleRight).Format(customAdj1?.GetColor(refineLvl) ?? AdjLevelEnum.None.Color, 14);
                uiArtifactInfo_textRefineAdj4 = uiArtifactInfo.textGrade_En.Copy().Align(TextAnchor.MiddleRight).Format(customAdj2?.GetColor(refineLvl) ?? AdjLevelEnum.None.Color, 14);
                uiArtifactInfo_textRefineAdj5 = uiArtifactInfo.textGrade_En.Copy().Align(TextAnchor.MiddleRight).Format(customAdj3?.GetColor(refineLvl) ?? AdjLevelEnum.None.Color, 14);

                uiArtifactInfo_textRefineTitle.Pos(uiArtifactInfo.textGrade_En.gameObject, 0, -125);
                uiArtifactInfo_textRefineAdj1.Pos(uiArtifactInfo.textGrade_En.gameObject, 5, -140);
                uiArtifactInfo_textRefineAdj2.Pos(uiArtifactInfo.textGrade_En.gameObject, 5, -155);
                uiArtifactInfo_textRefineAdj3.Pos(uiArtifactInfo.textGrade_En.gameObject, 5, -170);
                uiArtifactInfo_textRefineAdj4.Pos(uiArtifactInfo.textGrade_En.gameObject, 5, -185);
                uiArtifactInfo_textRefineAdj5.Pos(uiArtifactInfo.textGrade_En.gameObject, 5, -200);

                uiArtifactInfo_textRefineTitle.text = $"Refine ({refineLvl}):";
                uiArtifactInfo_textRefineAdj1.text = $"+Atk: {UnitModifyHelper.GetRefineArtifactAdjAtk(uiArtifactInfo.shapeProp, refineLvl)}";
                uiArtifactInfo_textRefineAdj2.text = $"+Def: {UnitModifyHelper.GetRefineArtifactAdjDef(uiArtifactInfo.shapeProp, refineLvl)}";
                if (g.ui.HasUI(UIType.PropSelect) && g.world.playerUnit.GetUnitProp(uiArtifactInfo.shapeProp.soleID) == null && g.world.playerUnit.GetLuck(UnitTypeLuckEnum.Merchant.Value.Parse<int>()) == null)
                {
                    uiArtifactInfo_textRefineAdj3.text = GameTool.LS("other500020083");
                    uiArtifactInfo_textRefineAdj4.text = GameTool.LS("other500020083");
                    uiArtifactInfo_textRefineAdj5.text = GameTool.LS("other500020083");
                }
                else
                {
                    uiArtifactInfo_textRefineAdj3.text = customAdj1?.GetText(uiArtifactInfo.unit, uiArtifactInfo.shapeProp, refineLvl);
                    uiArtifactInfo_textRefineAdj4.text = customAdj2?.GetText(uiArtifactInfo.unit, uiArtifactInfo.shapeProp, refineLvl);
                    uiArtifactInfo_textRefineAdj5.text = customAdj3?.GetText(uiArtifactInfo.unit, uiArtifactInfo.shapeProp, refineLvl);
                }
            }
            else
            if (e.uiType.uiName == UIType.MartialInfo.uiName && g.ui.HasUI(UIType.PlayerInfo))
            {
                ModDelayEvent.Instance.DelayEvent(this, "OnUIMartialInfoAsync", e, 1000);
            }
            else
            if (e.uiType.uiName == UIType.NPCInfoPreview.uiName && g.ui.HasUI(UIType.MartialInfo))
            {
                var uiMartialInfo = g.ui.GetUI<UIMartialInfo>(UIType.MartialInfo);
                //var soleId = uiMartialInfo.martialData.martialInfo.propsData.soleID;
                var soleId = uiMartialInfo.martialData.data.soleID;
                var propsGrade = uiMartialInfo.martialData.martialInfo.grade;
                var propsLevel = uiMartialInfo.martialData.martialInfo.level;
                var mType = uiMartialInfo.martialData.martialType;
                var expertLvl = ExpertEvent.GetExpertLvl(soleId, propsGrade, propsLevel);
                var expertExp = ExpertEvent.GetExpertExp(soleId);
                var expertNeedExp = ExpertEvent.GetExpertNeedExp(expertLvl + 1, propsGrade, propsLevel);

                uiMartialExpertInfo.transform.position = new Vector3(0f, 5f);

                var texts = uiMartialExpertInfo.GetComponentsInChildren<Text>();
                uiMartialExpertInfo_textExpertLvl = texts[1];
                uiMartialExpertInfo_textAdj1 = texts[5];
                uiMartialExpertInfo_textAdj2 = texts[7];
                uiMartialExpertInfo_textAdj3 = texts[9];
                uiMartialExpertInfo_textAdj4 = texts[11];
                uiMartialExpertInfo_textAdj5 = texts[13];

                uiMartialExpertInfo_textExpertLvl.fontSize++;
                uiMartialExpertInfo_textExpertLvl.fontStyle = FontStyle.Bold;
                uiMartialExpertInfo_textExpertLvl.text = $"Expert Lvl: {expertLvl} ({expertExp}/{expertNeedExp})";

                if (mType == MartialType.Ability)
                {
                    uiMartialExpertInfo_textAdj1.text = $"+Atk: {UnitModifyHelper.GetAbilityExpertAtk(uiMartialInfo.unit.GetDynProperty(UnitDynPropertyEnum.Attack).baseValue, expertLvl, propsGrade, propsLevel)}";
                    uiMartialExpertInfo_textAdj2.text = $"+Def: {UnitModifyHelper.GetAbilityExpertDef(uiMartialInfo.unit.GetDynProperty(UnitDynPropertyEnum.Defense).baseValue, expertLvl, propsGrade, propsLevel)}";
                    uiMartialExpertInfo_textAdj3.text = $"+Hp: {UnitModifyHelper.GetAbilityExpertHp(uiMartialInfo.unit.GetDynProperty(UnitDynPropertyEnum.HpMax).baseValue, expertLvl, propsGrade, propsLevel)}";
                    uiMartialExpertInfo_textAdj4.text = $"+Mp: {UnitModifyHelper.GetAbilityExpertMp(uiMartialInfo.unit.GetDynProperty(UnitDynPropertyEnum.MpMax).baseValue, expertLvl, propsGrade, propsLevel)}";
                    uiMartialExpertInfo_textAdj5.text = $"+Sp: {UnitModifyHelper.GetAbilityExpertSp(uiMartialInfo.unit.GetDynProperty(UnitDynPropertyEnum.SpMax).baseValue, expertLvl, propsGrade, propsLevel)}";
                }
                else if (mType == MartialType.Step)
                {
                    uiMartialExpertInfo_textAdj1.text = $"+Speed: {UnitModifyHelper.GetStepExpertSpeed(expertLvl, propsGrade, propsLevel)} (In Battle)";
                    uiMartialExpertInfo_textAdj2.text = $"+Evade(%): {UnitModifyHelper.GetStepExpertEvade(expertLvl, propsGrade, propsLevel)}% (In Battle)";
                    uiMartialExpertInfo_textAdj3.text = $"+Mp Cost: {UnitModifyHelper.GetStepExpertMpCost(SkillHelper.GetStepMpCost(uiMartialInfo.martialData.data), expertLvl, propsGrade, propsLevel, uiMartialInfo.unit.GetGradeLvl() * expertLvl / 5)} (In Battle)";
                    uiMartialExpertInfo_textAdj4.gameObject.SetActive(false);
                    uiMartialExpertInfo_textAdj5.gameObject.SetActive(false);
                }
                else
                {
                    uiMartialExpertInfo_textAdj1.text = $"+Dmg: {UnitModifyHelper.GetSkillExpertAtk(uiMartialInfo.unit.GetDynProperty(UnitDynPropertyEnum.Attack).baseValue, expertLvl, propsGrade, propsLevel, mType)} (In Battle)";
                    uiMartialExpertInfo_textAdj2.text = $"+Mp Cost: {UnitModifyHelper.GetSkillExpertMpCost(SkillHelper.GetSkillMpCost(uiMartialInfo.martialData.data), expertLvl, propsGrade, propsLevel, uiMartialInfo.unit.GetGradeLvl() * expertLvl / 10)} (In Battle)";
                    uiMartialExpertInfo_textAdj3.gameObject.SetActive(false);
                    uiMartialExpertInfo_textAdj4.gameObject.SetActive(false);
                    uiMartialExpertInfo_textAdj5.gameObject.SetActive(false);
                }

                foreach (var comp in texts)
                {
                    if (comp != uiMartialExpertInfo_textExpertLvl &&
                        comp != uiMartialExpertInfo_textAdj1 &&
                        comp != uiMartialExpertInfo_textAdj2 &&
                        comp != uiMartialExpertInfo_textAdj3 &&
                        comp != uiMartialExpertInfo_textAdj4 &&
                        comp != uiMartialExpertInfo_textAdj5)
                        comp.gameObject.SetActive(false);
                }
            }
            else
            if (e.uiType.uiName == UIType.PropInfo.uiName &&
                 (g.ui.HasUI(UIType.PropSelect) || g.ui.HasUI(UIType.PlayerInfo) || g.ui.HasUI(UIType.TownStorage) || (g.ui.HasUI(UIType.NPCInfo) && g.world.playerUnit.GetLuck(UnitTypeLuckEnum.Merchant.Value.Parse<int>()) != null)))
            {
                var uiPropInfo = g.ui.GetUI<UIPropInfo>(UIType.PropInfo);
                if (uiPropInfo.propData.propsItem.IsRing() != null)
                {
                    var refineLvl = CustomRefineEvent.GetRefineLvl(uiPropInfo.propData);
                    var customAdj1 = CustomRefineEvent.GetCustomAdjType(uiPropInfo.propData, 1);
                    var customAdj2 = CustomRefineEvent.GetCustomAdjType(uiPropInfo.propData, 2);

                    uiPropInfo_textRefineTitle = uiPropInfo.textGrade_En.Copy().Align().Format(Color.white, 15);
                    uiPropInfo_textRefineAdj1 = uiPropInfo.textGrade_En.Copy().Align().Format(Color.white, 14);
                    uiPropInfo_textRefineAdj2 = uiPropInfo.textGrade_En.Copy().Align().Format(customAdj1?.GetColor(refineLvl) ?? AdjLevelEnum.None.Color, 14);
                    uiPropInfo_textRefineAdj3 = uiPropInfo.textGrade_En.Copy().Align().Format(customAdj2?.GetColor(refineLvl) ?? AdjLevelEnum.None.Color, 14);

                    uiPropInfo_textRefineTitle.Pos(uiPropInfo.textGrade_En.gameObject, 0, -20);
                    uiPropInfo_textRefineAdj1.Pos(uiPropInfo.textGrade_En.gameObject, 5, -35);
                    uiPropInfo_textRefineAdj2.Pos(uiPropInfo.textGrade_En.gameObject, 5, -50);
                    uiPropInfo_textRefineAdj3.Pos(uiPropInfo.textGrade_En.gameObject, 5, -65);

                    uiPropInfo_textRefineTitle.text = $"Refine ({refineLvl}):";
                    uiPropInfo_textRefineAdj1.text = $"+Hp: {UnitModifyHelper.GetRefineRingAdjHp(uiPropInfo.unit.GetDynProperty(UnitDynPropertyEnum.HpMax).baseValue, uiPropInfo.propData, refineLvl)}";
                    if (g.ui.HasUI(UIType.PropSelect) && g.world.playerUnit.GetUnitProp(uiPropInfo.propData.soleID) == null && g.world.playerUnit.GetLuck(UnitTypeLuckEnum.Merchant.Value.Parse<int>()) == null)
                    {
                        uiPropInfo_textRefineAdj2.text = GameTool.LS("other500020083");
                        uiPropInfo_textRefineAdj3.text = GameTool.LS("other500020083");
                    }
                    else
                    {
                        uiPropInfo_textRefineAdj2.text = customAdj1?.GetText(uiPropInfo.unit, uiPropInfo.propData, refineLvl);
                        uiPropInfo_textRefineAdj3.text = customAdj2?.GetText(uiPropInfo.unit, uiPropInfo.propData, refineLvl);
                    }
                }
                else if (uiPropInfo.propData.propsItem.IsOutfit() != null)
                {
                    var refineLvl = CustomRefineEvent.GetRefineLvl(uiPropInfo.propData);
                    var customAdj1 = CustomRefineEvent.GetCustomAdjType(uiPropInfo.propData, 1);
                    var customAdj2 = CustomRefineEvent.GetCustomAdjType(uiPropInfo.propData, 2);

                    uiPropInfo_textRefineTitle = uiPropInfo.textName.Copy().Align().Format(Color.white, 15);
                    uiPropInfo_textRefineAdj1 = uiPropInfo.textName.Copy().Align().Format(Color.white, 14);
                    uiPropInfo_textRefineAdj2 = uiPropInfo.textName.Copy().Align().Format(Color.white, 14);
                    uiPropInfo_textRefineAdj3 = uiPropInfo.textName.Copy().Align().Format(customAdj1?.GetColor(refineLvl) ?? AdjLevelEnum.None.Color, 14);
                    uiPropInfo_textRefineAdj4 = uiPropInfo.textName.Copy().Align().Format(customAdj2?.GetColor(refineLvl) ?? AdjLevelEnum.None.Color, 14);

                    uiPropInfo_textRefineTitle.Pos(uiPropInfo.textName.gameObject, 0, -0);
                    uiPropInfo_textRefineAdj1.Pos(uiPropInfo.textName.gameObject, 5, -15);
                    uiPropInfo_textRefineAdj2.Pos(uiPropInfo.textName.gameObject, 5, -30);
                    uiPropInfo_textRefineAdj3.Pos(uiPropInfo.textName.gameObject, 5, -45);
                    uiPropInfo_textRefineAdj4.Pos(uiPropInfo.textName.gameObject, 5, -60);

                    uiPropInfo_textRefineTitle.text = $"Refine ({refineLvl}):";
                    uiPropInfo_textRefineAdj1.text = $"+Hp: {UnitModifyHelper.GetRefineOutfitAdjHp(uiPropInfo.unit.GetDynProperty(UnitDynPropertyEnum.HpMax).baseValue, uiPropInfo.propData, refineLvl)}";
                    uiPropInfo_textRefineAdj2.text = $"+Def: {UnitModifyHelper.GetRefineOutfitAdjDef(uiPropInfo.unit.GetDynProperty(UnitDynPropertyEnum.Defense).baseValue, uiPropInfo.propData, refineLvl)}";
                    if (RealMarketEvent3.IsOpenSellingList && g.world.playerUnit.GetLuck(UnitTypeLuckEnum.Merchant.Value.Parse<int>()) == null)
                    {
                        uiPropInfo_textRefineAdj3.text = GameTool.LS("other500020083");
                        uiPropInfo_textRefineAdj4.text = GameTool.LS("other500020083");
                    }
                    else
                    {
                        uiPropInfo_textRefineAdj3.text = customAdj1?.GetText(uiPropInfo.unit, uiPropInfo.propData, refineLvl);
                        uiPropInfo_textRefineAdj4.text = customAdj2?.GetText(uiPropInfo.unit, uiPropInfo.propData, refineLvl);
                    }
                }
                else if (uiPropInfo.propData.propsItem.IsMount() != null)
                {
                    var refineLvl = CustomRefineEvent.GetRefineLvl(uiPropInfo.propData);
                    var customAdj1 = CustomRefineEvent.GetCustomAdjType(uiPropInfo.propData, 1);
                    var customAdj2 = CustomRefineEvent.GetCustomAdjType(uiPropInfo.propData, 2);

                    uiPropInfo_textRefineTitle = uiPropInfo.textGrade_En.Copy().Align().Format(Color.white, 15);
                    uiPropInfo_textRefineAdj1 = uiPropInfo.textGrade_En.Copy().Align().Format(Color.white, 14);
                    uiPropInfo_textRefineAdj2 = uiPropInfo.textGrade_En.Copy().Align().Format(customAdj1?.GetColor(refineLvl) ?? AdjLevelEnum.None.Color, 14);
                    uiPropInfo_textRefineAdj3 = uiPropInfo.textGrade_En.Copy().Align().Format(customAdj2?.GetColor(refineLvl) ?? AdjLevelEnum.None.Color, 14);

                    uiPropInfo_textRefineTitle.Pos(uiPropInfo.textGrade_En.gameObject, 0, -20);
                    uiPropInfo_textRefineAdj1.Pos(uiPropInfo.textGrade_En.gameObject, 5, -35);
                    uiPropInfo_textRefineAdj2.Pos(uiPropInfo.textGrade_En.gameObject, 5, -50);
                    uiPropInfo_textRefineAdj3.Pos(uiPropInfo.textGrade_En.gameObject, 5, -65);

                    uiPropInfo_textRefineTitle.text = $"Refine ({refineLvl}):";
                    uiPropInfo_textRefineAdj1.text = $"+Mp: {UnitModifyHelper.GetRefineMountAdjMp(uiPropInfo.unit.GetDynProperty(UnitDynPropertyEnum.MpMax).baseValue, uiPropInfo.propData, refineLvl)}";
                    if (RealMarketEvent3.IsOpenSellingList && g.world.playerUnit.GetLuck(UnitTypeLuckEnum.Merchant.Value.Parse<int>()) == null)
                    {
                        uiPropInfo_textRefineAdj2.text = GameTool.LS("other500020083");
                        uiPropInfo_textRefineAdj3.text = GameTool.LS("other500020083");
                    }
                    else
                    {
                        uiPropInfo_textRefineAdj2.text = customAdj1?.GetText(uiPropInfo.unit, uiPropInfo.propData, refineLvl);
                        uiPropInfo_textRefineAdj3.text = customAdj2?.GetText(uiPropInfo.unit, uiPropInfo.propData, refineLvl);
                    }
                }
            }
            else
            if (e.uiType.uiName == UIType.GameMemu.uiName)
            {
                var ui = g.ui.GetUI<UIGameMemu>(UIType.GameMemu);
                ui.btnSave.gameObject.SetActive(!SMLocalConfigsEvent.Instance.Configs.HideSaveButton);
                ui.btnReloadCache.gameObject.SetActive(!SMLocalConfigsEvent.Instance.Configs.HideReloadButton);
            }
        }

        public void OnUIMartialInfoAsync(OpenUIEnd e)
        {
            if (g.ui.HasUI(UIType.MartialInfo))
            {
                uiMartialExpertInfo = g.ui.OpenUISafe<UINPCInfoPreview>(UIType.NPCInfoPreview);
            }
        }

        public void OnUIUpdate()
        {
            var uiArtifactInfo = g.ui.GetUI<UIArtifactInfo>(UIType.ArtifactInfo);
            var uiPropInfo = g.ui.GetUI<UIPropInfo>(UIType.PropInfo);

            if (uiPropInfo.IsExists() &&
                (g.ui.HasUI(UIType.PropSelect) || g.ui.HasUI(UIType.PlayerInfo) || g.ui.HasUI(UIType.TownStorage) || (g.ui.HasUI(UIType.NPCInfo) && g.world.playerUnit.GetLuck(UnitTypeLuckEnum.Merchant.Value.Parse<int>()) != null)))
            {
                if (uiPropInfo.propData.propsItem.IsRing() != null)
                {
                    uiPropInfo_textRefineTitle.Pos(uiPropInfo.textGrade_En.gameObject, 0, -20);
                    uiPropInfo_textRefineAdj1.Pos(uiPropInfo.textGrade_En.gameObject, 5, -35);
                    uiPropInfo_textRefineAdj2.Pos(uiPropInfo.textGrade_En.gameObject, 5, -50);
                    uiPropInfo_textRefineAdj3.Pos(uiPropInfo.textGrade_En.gameObject, 5, -65);
                }
                else if (uiPropInfo.propData.propsItem.IsOutfit() != null)
                {
                    uiPropInfo_textRefineTitle.Pos(uiPropInfo.textName.gameObject, 0, -0);
                    uiPropInfo_textRefineAdj1.Pos(uiPropInfo.textName.gameObject, 5, -15);
                    uiPropInfo_textRefineAdj2.Pos(uiPropInfo.textName.gameObject, 5, -30);
                    uiPropInfo_textRefineAdj3.Pos(uiPropInfo.textName.gameObject, 5, -45);
                    uiPropInfo_textRefineAdj4.Pos(uiPropInfo.textName.gameObject, 5, -60);
                }
                else if (uiPropInfo.propData.propsItem.IsMount() != null)
                {
                    uiPropInfo_textRefineTitle.Pos(uiPropInfo.textGrade_En.gameObject, 0, -20);
                    uiPropInfo_textRefineAdj1.Pos(uiPropInfo.textGrade_En.gameObject, 5, -35);
                    uiPropInfo_textRefineAdj2.Pos(uiPropInfo.textGrade_En.gameObject, 5, -50);
                    uiPropInfo_textRefineAdj3.Pos(uiPropInfo.textGrade_En.gameObject, 5, -65);
                }
            }

            if (uiArtifactInfo.IsExists() &&
                (g.ui.HasUI(UIType.PropSelect) || g.ui.HasUI(UIType.PlayerInfo) || g.ui.HasUI(UIType.TownStorage) || (g.ui.HasUI(UIType.NPCInfo) && g.world.playerUnit.GetLuck(UnitTypeLuckEnum.Merchant.Value.Parse<int>()) != null)))
            {
                uiArtifactInfo_textBasicTitle.Pos(uiArtifactInfo.textGrade_En.gameObject, 0, -20);
                uiArtifactInfo_textBasicAdj1.Pos(uiArtifactInfo.textGrade_En.gameObject, 5, -35);
                uiArtifactInfo_textBasicAdj2.Pos(uiArtifactInfo.textGrade_En.gameObject, 5, -50);
                uiArtifactInfo_textBasicAdj3.Pos(uiArtifactInfo.textGrade_En.gameObject, 5, -65);
                uiArtifactInfo_textExpertLvl.Pos(uiArtifactInfo.textGrade_En.gameObject, 0, -80);
                uiArtifactInfo_textExpertAdj1.Pos(uiArtifactInfo.textGrade_En.gameObject, 5, -95);
                uiArtifactInfo_textExpertAdj2.Pos(uiArtifactInfo.textGrade_En.gameObject, 5, -110);
                uiArtifactInfo_textRefineTitle.Pos(uiArtifactInfo.textGrade_En.gameObject, 0, -125);
                uiArtifactInfo_textRefineAdj1.Pos(uiArtifactInfo.textGrade_En.gameObject, 5, -140);
                uiArtifactInfo_textRefineAdj2.Pos(uiArtifactInfo.textGrade_En.gameObject, 5, -155);
                uiArtifactInfo_textRefineAdj3.Pos(uiArtifactInfo.textGrade_En.gameObject, 5, -170);
                uiArtifactInfo_textRefineAdj4.Pos(uiArtifactInfo.textGrade_En.gameObject, 5, -185);
                uiArtifactInfo_textRefineAdj5.Pos(uiArtifactInfo.textGrade_En.gameObject, 5, -200);
            }
        }

        public void OnUIClose(CloseUIEnd e)
        {
            if (e.uiType.uiName == UIType.MartialInfo.uiName && g.ui.HasUI(UIType.NPCInfoPreview))
            {
                g.ui.CloseUI(uiMartialExpertInfo);
            }
        }

        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            OnUIOpen(e);
        }

        [ErrorIgnore]
        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
        public override void OnTimeUpdate100ms()
        {
            base.OnTimeUpdate100ms();
            OnUIUpdate();
        }

        public override void OnCloseUIEnd(CloseUIEnd e)
        {
            base.OnCloseUIEnd(e);
            OnUIClose(e);
        }
    }
}
