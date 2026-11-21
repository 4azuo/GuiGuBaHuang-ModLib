using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using UnityEngine.UI;
using EGameTypeData;
using ModLib.Enum;
using MOD_nE7UL2.Enum;
using ModLib.Object;
using ModLib.Attributes;
using ModLib.Helper;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// UI tweaks: hides some map widgets, shows Taoist bonuses (artifact/expert/refine/Qi...) in item, martial and player UIs, hides save/reload buttons.
    /// </summary>
    [Cache(ModConst.UI_EVENT)]
    public class UIEvent : ModEvent
    {
        public static UIEvent Instance { get; set; }

        /// <summary>Player info: the "Damage Calc" button and the S&amp;M score line on the top edge, offsets from the close button (canvas units).</summary>
        public const float DMG_BUTTON_DX = -190f, SCORE_DX = -1150f, REBIRTH_BUTTON_DX = -185f;

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

        /// <summary>
        /// Adds Taoist info texts to the opened UI.
        /// </summary>
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
                ModDelayEvent.Instance.DelayEvent(this, () =>
                {
                    if (g.ui.HasUI(UIType.Town))
                    {
                        var ui = new UICover<UITown>(e.ui);
                        {
                            if (MapBuildPropertyEvent.IsTownGuardian(ui.UI.town, g.world.playerUnit))
                            {
                                ui.AddButton(ui.LastCol - 5, ui.FirstRow + 3, () => MapBuildPropertyEvent.OpenUITownManage(ui.UI.town), GameTool.LS("other500020005")).Size(300, 80);
                            }
                            else
                            {
                                // a house in the town: its fixed tax replaces the entry tax (EstateEvent)
                                var taxLabel = EstateEvent.HasHouse(ui.UI.town, g.world.playerUnit)
                                    ? GameTool.LS("estate_tax_free")
                                    : ModTextHelper.LSF("ui_tax_town", MapBuildPropertyEvent.GetTax(ui.UI.town, g.world.playerUnit));
                                ui.AddButton(ui.LastCol - 5, ui.FirstRow + 3, () => MapBuildPropertyEvent.OpenUITownGuardians(ui.UI.town), taxLabel).Size(300, 80);
                            }
                            ui.AddToolTipButton(ui.LastCol - 8, ui.FirstRow + 3, GameTool.LS("other500020020"));
                            //plots of the town (EstateEvent)
                            // right under the tax button, above the game's task panel ("Newest")
                            if (EstateEvent.Enabled)
                                ui.AddButton(ui.LastCol - 5, ui.FirstRow + 5, () => EstateEvent.OpenPlots(ui.UI.town), GameTool.LS("estate_button")).Size(300, 36);
                        }
                    }
                }, 1);
            }
            else
            if (e.uiType.uiName == UIType.School.uiName)
            {
                ModDelayEvent.Instance.DelayEvent(this, () =>
                {
                    if (g.ui.HasUI(UIType.School))
                    {
                        var ui = new UICover<UISchool>(e.ui);
                        {
                            if (!MapBuildPropertyEvent.IsSchoolMember(ui.UI.school, g.world.playerUnit))
                            {
                                ui.AddButton(ui.LastCol - 5, ui.FirstRow + 3, null, ModTextHelper.LSF("ui_tax", MapBuildPropertyEvent.GetTax(ui.UI.school, g.world.playerUnit))).Size(300, 80).Enable = false;
                            }
                        }
                    }
                }, 1);
            }
            else
            if (e.uiType.uiName == UIType.NPCInfo.uiName)
            {
                var ui = new UICover<UINPCInfo>(UIType.NPCInfo);
                {
                    ui.AddText(0, 0, string.Empty).Format(Color.white).SetParentTransform(ui.UI.uiProperty.goGroupRoot)
                        .Pos(ui.UI.uiProperty.goItem6, 36, -150).SetWork(new UIItemWork
                        {
                            UpdateAct = ModLib.Helper.ActionHelper.WTracedAction<UIItemBase>((x) => x.Set(ModTextHelper.LSF("ui_adj_hp", UnitModifyHelper.GetMartialAdjHp(ui.UI.unit))))
                        });
                    ui.AddText(0, 0, string.Empty).Format(Color.white).SetParentTransform(ui.UI.uiProperty.goGroupRoot)
                        .Pos(ui.UI.uiProperty.goItem7, 72, -150).SetWork(new UIItemWork
                        {
                            UpdateAct = ModLib.Helper.ActionHelper.WTracedAction<UIItemBase>((x) => x.Set(ModTextHelper.LSF("ui_adj_mp", UnitModifyHelper.GetSpiritualAdjMp(ui.UI.unit))))
                        });
                    ui.AddText(0, 0, string.Empty).Format(Color.white).SetParentTransform(ui.UI.uiProperty.goGroupRoot)
                        .Pos(ui.UI.uiProperty.goItem8, 72, -150).SetWork(new UIItemWork
                        {
                            UpdateAct = ModLib.Helper.ActionHelper.WTracedAction<UIItemBase>((x) => x.Set(ModTextHelper.LSF("ui_adj_sp", UnitModifyHelper.GetArtisanshipAdjSp(ui.UI.unit))))
                        });

                    ui.AddText(0, 0, string.Empty).Format(Color.white).SetParentTransform(ui.UI.uiSkill.textPoint1.transform)
                        .Pos(ui.UI.uiSkill.textPoint1.transform, 0, -12).SetWork(new UIItemWork
                        {
                            UpdateAct = ModLib.Helper.ActionHelper.WTracedAction<UIItemBase>((x) => x.Set(ModTextHelper.LSF("ui_adj_hp", UnitModifyHelper.GetAbiPointAdjHp(ui.UI.unit))))
                        });
                    ui.AddText(0, 0, string.Empty).Format(Color.white).SetParentTransform(ui.UI.uiSkill.textPoint1.transform)
                        .Pos(ui.UI.uiSkill.textPoint1.transform, 0, -36).SetWork(new UIItemWork
                        {
                            UpdateAct = ModLib.Helper.ActionHelper.WTracedAction<UIItemBase>((x) => x.Set(ModTextHelper.LSF("ui_adj_mp", UnitModifyHelper.GetAbiPointAdjMp(ui.UI.unit))))
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
                            UpdateAct = ModLib.Helper.ActionHelper.WTracedAction<UIItemBase>((x) => x.Set(ModTextHelper.LSF("ui_adj_hp", UnitModifyHelper.GetMartialAdjHp(ui.UI.unit))))
                        });
                    ui.AddToolTipButton(GameTool.LS("other500020033")).SetParentTransform(ui.UI.uiPropertyCommon.goGroupRoot).Pos(ui.UI.uiPropertyCommon.goItem6_En, 40, -160);
                    ui.AddText(0, 0, string.Empty).SetParentTransform(ui.UI.uiPropertyCommon.goGroupRoot)
                        .Pos(ui.UI.uiPropertyCommon.goItem7_En, 100, -160).SetWork(new UIItemWork
                        {
                            UpdateAct = ModLib.Helper.ActionHelper.WTracedAction<UIItemBase>((x) => x.Set(ModTextHelper.LSF("ui_adj_mp", UnitModifyHelper.GetSpiritualAdjMp(ui.UI.unit))))
                        });
                    ui.AddToolTipButton(GameTool.LS("other500020034")).SetParentTransform(ui.UI.uiPropertyCommon.goGroupRoot).Pos(ui.UI.uiPropertyCommon.goItem7_En, 40, -160);
                    ui.AddText(0, 0, string.Empty).SetParentTransform(ui.UI.uiPropertyCommon.goGroupRoot)
                        .Pos(ui.UI.uiPropertyCommon.goItem8_En, 100, -160).SetWork(new UIItemWork
                        {
                            UpdateAct = ModLib.Helper.ActionHelper.WTracedAction<UIItemBase>((x) => x.Set(ModTextHelper.LSF("ui_adj_sp", UnitModifyHelper.GetArtisanshipAdjSp(ui.UI.unit))))
                        });
                    ui.AddToolTipButton(GameTool.LS("other500020035")).SetParentTransform(ui.UI.uiPropertyCommon.goGroupRoot).Pos(ui.UI.uiPropertyCommon.goItem8_En, 40, -160);

                    ui.AddText(0, 0, string.Empty).Format(Color.white).SetParentTransform(ui.UI.uiSkill.textPoint1.transform)
                        .Pos(ui.UI.uiSkill.textPoint1.transform, 0, -12).SetWork(new UIItemWork
                        {
                            UpdateAct = ModLib.Helper.ActionHelper.WTracedAction<UIItemBase>((x) => x.Set(ModTextHelper.LSF("ui_adj_hp", UnitModifyHelper.GetAbiPointAdjHp(ui.UI.unit))))
                        });
                    ui.AddText(0, 0, string.Empty).Format(Color.white).SetParentTransform(ui.UI.uiSkill.textPoint1.transform)
                        .Pos(ui.UI.uiSkill.textPoint1.transform, 0, -36).SetWork(new UIItemWork
                        {
                            UpdateAct = ModLib.Helper.ActionHelper.WTracedAction<UIItemBase>((x) => x.Set(ModTextHelper.LSF("ui_adj_mp", UnitModifyHelper.GetAbiPointAdjMp(ui.UI.unit))))
                        });
                    ui.AddToolTipButton(GameTool.LS("other500020036")).SetParentTransform(ui.UI.uiSkill.textPoint1.transform).Pos(ui.UI.uiSkill.textPoint1.transform, 0, -60);
                    ui.AddToolTipButton(GameTool.LS("other500020037")).SetParentTransform(ui.UI.uiSkill.goGroupRoot).Pos(ui.UI.uiSkill.goActionMartialRoot.transform, 0, 200);

                    // damage calculation before a battle (DamageInfo; in a battle: its button / hotkey)
                    if (ui.UI.unit != null && ui.UI.unit.IsPlayer())
                    {
                        // on the top edge of the scroll, on every tab: the button left of the close button (top right), the
                        // score at the top left (same parent as the close button: positions relative to it)
                        var close = ui.UI.btnClose;
                        ui.AddButton(0, 0, () => DamageInfo.Open(), GameTool.LS("dmginfo_button")).Size(170, 32)
                            .SetParentTransform(close.transform.parent).Pos(close, DMG_BUTTON_DX, 0);
                        // the rebirth (RebirthEvent; until 6.0 at the pub), left of the damage calculation: the player info
                        // closes when it is done
                        if (!SMLocalConfigsEvent.Instance.Configs.NoRebirth && RebirthEvent.Instance != null)
                        {
                            var info = e.ui;
                            ui.AddButton(0, 0, () => RebirthEvent.Open(() => g.ui.CloseUI(info)), RebirthEvent.ButtonText).Size(170, 32)
                                .SetParentTransform(close.transform.parent).Pos(close, DMG_BUTTON_DX + REBIRTH_BUTTON_DX, 0);
                            ui.AddToolTipButton(GameTool.LS("rebirth420103103")).SetParentTransform(close.transform.parent).Pos(close, DMG_BUTTON_DX + REBIRTH_BUTTON_DX - 105f, 0);
                        }
                        // S&M score of the save: title of its tier, P, reward bonus (SMScoreEvent)
                        ui.AddText(0, 0, SMScoreEvent.TitleLine(SMScoreEvent.P)).Format(new Color(0.45f, 0.12f, 0.08f), 16).Align(TextAnchor.MiddleCenter)
                            .SetParentTransform(close.transform.parent).Pos(close, SCORE_DX, 0);
                    }
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

                uiArtifactInfo_textBasicTitle.Set(GameTool.LS("ui_basic"));
                uiArtifactInfo_textBasicAdj1.Set(ModTextHelper.LSF("ui_adj_atk", UnitModifyHelper.GetArtifactBasicAdjAtk(uiArtifactInfo.unit.GetDynProperty(UnitDynPropertyEnum.Attack).baseValue, uiArtifactInfo.shapeProp, artifact)));
                uiArtifactInfo_textBasicAdj2.Set(ModTextHelper.LSF("ui_adj_def", UnitModifyHelper.GetArtifactBasicAdjDef(uiArtifactInfo.unit.GetDynProperty(UnitDynPropertyEnum.Defense).baseValue, uiArtifactInfo.shapeProp, artifact)));
                uiArtifactInfo_textBasicAdj3.Set(ModTextHelper.LSF("ui_adj_hp", UnitModifyHelper.GetArtifactBasicAdjHp(uiArtifactInfo.unit.GetDynProperty(UnitDynPropertyEnum.HpMax).baseValue, uiArtifactInfo.shapeProp, artifact)));

                //expert
                uiArtifactInfo_textExpertLvl = uiArtifactInfo.textGrade_En.Copy().Align().Format(Color.white, 15);
                uiArtifactInfo_textExpertAdj1 = uiArtifactInfo.textGrade_En.Copy().Align().Format(Color.white, 14);
                uiArtifactInfo_textExpertAdj2 = uiArtifactInfo.textGrade_En.Copy().Align().Format(Color.white, 14);

                uiArtifactInfo_textExpertLvl.Pos(uiArtifactInfo.textGrade_En.gameObject, 0, -80);
                uiArtifactInfo_textExpertAdj1.Pos(uiArtifactInfo.textGrade_En.gameObject, 5, -95);
                uiArtifactInfo_textExpertAdj2.Pos(uiArtifactInfo.textGrade_En.gameObject, 5, -110);

                uiArtifactInfo_textExpertLvl.Set(ModTextHelper.LSF("ui_expert_lvl", expertLvl, expertExp, expertNeedExp));
                uiArtifactInfo_textExpertAdj1.Set(ModTextHelper.LSF("ui_adj_atk", UnitModifyHelper.GetArtifactExpertAtk(artifactConf.atk, expertLvl, propsGrade, propsLevel)));
                uiArtifactInfo_textExpertAdj2.Set(ModTextHelper.LSF("ui_adj_def", UnitModifyHelper.GetArtifactExpertDef(artifactConf.def, expertLvl, propsGrade, propsLevel)));

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

                uiArtifactInfo_textRefineTitle.Set(ModTextHelper.LSF("ui_refine_lvl", refineLvl));
                uiArtifactInfo_textRefineAdj1.Set(ModTextHelper.LSF("ui_adj_atk", UnitModifyHelper.GetRefineArtifactAdjAtk(uiArtifactInfo.shapeProp, refineLvl)));
                uiArtifactInfo_textRefineAdj2.Set(ModTextHelper.LSF("ui_adj_def", UnitModifyHelper.GetRefineArtifactAdjDef(uiArtifactInfo.shapeProp, refineLvl)));
                if (g.ui.HasUI(UIType.PropSelect) && g.world.playerUnit.GetUnitProp(uiArtifactInfo.shapeProp.soleID) == null && g.world.playerUnit.GetLuck(UnitTypeLuckEnum.Merchant.Value.Parse<int>()) == null)
                {
                    uiArtifactInfo_textRefineAdj3.Set(GameTool.LS("other500020083"));
                    uiArtifactInfo_textRefineAdj4.Set(GameTool.LS("other500020083"));
                    uiArtifactInfo_textRefineAdj5.Set(GameTool.LS("other500020083"));
                }
                else
                {
                    uiArtifactInfo_textRefineAdj3.Set(customAdj1?.GetText(uiArtifactInfo.unit, uiArtifactInfo.shapeProp, refineLvl));
                    uiArtifactInfo_textRefineAdj4.Set(customAdj2?.GetText(uiArtifactInfo.unit, uiArtifactInfo.shapeProp, refineLvl));
                    uiArtifactInfo_textRefineAdj5.Set(customAdj3?.GetText(uiArtifactInfo.unit, uiArtifactInfo.shapeProp, refineLvl));
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
                uiMartialExpertInfo_textExpertLvl.Set(ModTextHelper.LSF("ui_expert_lvl", expertLvl, expertExp, expertNeedExp));

                if (mType == MartialType.Ability)
                {
                    uiMartialExpertInfo_textAdj1.Set(ModTextHelper.LSF("ui_adj_atk", UnitModifyHelper.GetAbilityExpertAtk(uiMartialInfo.unit.GetDynProperty(UnitDynPropertyEnum.Attack).baseValue, expertLvl, propsGrade, propsLevel)));
                    uiMartialExpertInfo_textAdj2.Set(ModTextHelper.LSF("ui_adj_def", UnitModifyHelper.GetAbilityExpertDef(uiMartialInfo.unit.GetDynProperty(UnitDynPropertyEnum.Defense).baseValue, expertLvl, propsGrade, propsLevel)));
                    uiMartialExpertInfo_textAdj3.Set(ModTextHelper.LSF("ui_adj_hp", UnitModifyHelper.GetAbilityExpertHp(uiMartialInfo.unit.GetDynProperty(UnitDynPropertyEnum.HpMax).baseValue, expertLvl, propsGrade, propsLevel)));
                    uiMartialExpertInfo_textAdj4.Set(ModTextHelper.LSF("ui_adj_mp", UnitModifyHelper.GetAbilityExpertMp(uiMartialInfo.unit.GetDynProperty(UnitDynPropertyEnum.MpMax).baseValue, expertLvl, propsGrade, propsLevel)));
                    uiMartialExpertInfo_textAdj5.Set(ModTextHelper.LSF("ui_adj_sp", UnitModifyHelper.GetAbilityExpertSp(uiMartialInfo.unit.GetDynProperty(UnitDynPropertyEnum.SpMax).baseValue, expertLvl, propsGrade, propsLevel)));
                }
                else if (mType == MartialType.Step)
                {
                    uiMartialExpertInfo_textAdj1.Set(ModTextHelper.LSF("ui_adj_speed", UnitModifyHelper.GetStepExpertSpeed(expertLvl, propsGrade, propsLevel)));
                    uiMartialExpertInfo_textAdj2.Set(ModTextHelper.LSF("ui_adj_evade", UnitModifyHelper.GetStepExpertEvade(expertLvl, propsGrade, propsLevel)));
                    uiMartialExpertInfo_textAdj3.Set(ModTextHelper.LSF("ui_adj_mpcost", UnitModifyHelper.GetStepExpertMpCost(SkillHelper.GetStepMpCost(uiMartialInfo.martialData.data), expertLvl, propsGrade, propsLevel, uiMartialInfo.unit.GetGradeLvl() * expertLvl / 5)));
                    uiMartialExpertInfo_textAdj4.gameObject.SetActive(false);
                    uiMartialExpertInfo_textAdj5.gameObject.SetActive(false);
                }
                else
                {
                    uiMartialExpertInfo_textAdj1.Set(ModTextHelper.LSF("ui_adj_dmg", UnitModifyHelper.GetSkillExpertAtk(uiMartialInfo.unit.GetDynProperty(UnitDynPropertyEnum.Attack).baseValue, expertLvl, propsGrade, propsLevel, mType)));
                    uiMartialExpertInfo_textAdj2.Set(ModTextHelper.LSF("ui_adj_mpcost", UnitModifyHelper.GetSkillExpertMpCost(SkillHelper.GetSkillMpCost(uiMartialInfo.martialData.data), expertLvl, propsGrade, propsLevel, uiMartialInfo.unit.GetGradeLvl() * expertLvl / 10)));
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

                    uiPropInfo_textRefineTitle.Set(ModTextHelper.LSF("ui_refine_lvl", refineLvl));
                    uiPropInfo_textRefineAdj1.Set(ModTextHelper.LSF("ui_adj_hp", UnitModifyHelper.GetRefineRingAdjHp(uiPropInfo.unit.GetDynProperty(UnitDynPropertyEnum.HpMax).baseValue, uiPropInfo.propData, refineLvl)));
                    if (g.ui.HasUI(UIType.PropSelect) && g.world.playerUnit.GetUnitProp(uiPropInfo.propData.soleID) == null && g.world.playerUnit.GetLuck(UnitTypeLuckEnum.Merchant.Value.Parse<int>()) == null)
                    {
                        uiPropInfo_textRefineAdj2.Set(GameTool.LS("other500020083"));
                        uiPropInfo_textRefineAdj3.Set(GameTool.LS("other500020083"));
                    }
                    else
                    {
                        uiPropInfo_textRefineAdj2.Set(customAdj1?.GetText(uiPropInfo.unit, uiPropInfo.propData, refineLvl));
                        uiPropInfo_textRefineAdj3.Set(customAdj2?.GetText(uiPropInfo.unit, uiPropInfo.propData, refineLvl));
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

                    // top-left corner of the window (as the ring's block): over the name it hid the name and the type
                    uiPropInfo_textRefineTitle.Pos(uiPropInfo.textName.gameObject, 0, 80);
                    uiPropInfo_textRefineAdj1.Pos(uiPropInfo.textName.gameObject, 5, 65);
                    uiPropInfo_textRefineAdj2.Pos(uiPropInfo.textName.gameObject, 5, 50);
                    uiPropInfo_textRefineAdj3.Pos(uiPropInfo.textName.gameObject, 5, 35);
                    uiPropInfo_textRefineAdj4.Pos(uiPropInfo.textName.gameObject, 5, 20);

                    uiPropInfo_textRefineTitle.Set(ModTextHelper.LSF("ui_refine_lvl", refineLvl));
                    uiPropInfo_textRefineAdj1.Set(ModTextHelper.LSF("ui_adj_hp", UnitModifyHelper.GetRefineOutfitAdjHp(uiPropInfo.unit.GetDynProperty(UnitDynPropertyEnum.HpMax).baseValue, uiPropInfo.propData, refineLvl)));
                    uiPropInfo_textRefineAdj2.Set(ModTextHelper.LSF("ui_adj_def", UnitModifyHelper.GetRefineOutfitAdjDef(uiPropInfo.unit.GetDynProperty(UnitDynPropertyEnum.Defense).baseValue, uiPropInfo.propData, refineLvl)));
                    if (RealMarketEvent3.IsOpenSellingList && g.world.playerUnit.GetLuck(UnitTypeLuckEnum.Merchant.Value.Parse<int>()) == null)
                    {
                        uiPropInfo_textRefineAdj3.Set(GameTool.LS("other500020083"));
                        uiPropInfo_textRefineAdj4.Set(GameTool.LS("other500020083"));
                    }
                    else
                    {
                        uiPropInfo_textRefineAdj3.Set(customAdj1?.GetText(uiPropInfo.unit, uiPropInfo.propData, refineLvl));
                        uiPropInfo_textRefineAdj4.Set(customAdj2?.GetText(uiPropInfo.unit, uiPropInfo.propData, refineLvl));
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

                    uiPropInfo_textRefineTitle.Set(ModTextHelper.LSF("ui_refine_lvl", refineLvl));
                    uiPropInfo_textRefineAdj1.Set(ModTextHelper.LSF("ui_adj_mp", UnitModifyHelper.GetRefineMountAdjMp(uiPropInfo.unit.GetDynProperty(UnitDynPropertyEnum.MpMax).baseValue, uiPropInfo.propData, refineLvl)));
                    if (RealMarketEvent3.IsOpenSellingList && g.world.playerUnit.GetLuck(UnitTypeLuckEnum.Merchant.Value.Parse<int>()) == null)
                    {
                        uiPropInfo_textRefineAdj2.Set(GameTool.LS("other500020083"));
                        uiPropInfo_textRefineAdj3.Set(GameTool.LS("other500020083"));
                    }
                    else
                    {
                        uiPropInfo_textRefineAdj2.Set(customAdj1?.GetText(uiPropInfo.unit, uiPropInfo.propData, refineLvl));
                        uiPropInfo_textRefineAdj3.Set(customAdj2?.GetText(uiPropInfo.unit, uiPropInfo.propData, refineLvl));
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

        /// <summary>
        /// Opens the expertise preview next to MartialInfo.
        /// </summary>
        public void OnUIMartialInfoAsync(OpenUIEnd e)
        {
            if (g.ui.HasUI(UIType.MartialInfo))
            {
                uiMartialExpertInfo = g.ui.OpenUISafe<UINPCInfoPreview>(UIType.NPCInfoPreview);
            }
        }

        /// <summary>
        /// Keeps info texts positioned on PropInfo/ArtifactInfo.
        /// </summary>
        private static bool IsReady(Text text, UIBase ui)
        {
            return text != null && ui != null && text.transform.IsChildOf(ui.transform);
        }

        public void OnUIUpdate()
        {
            var uiArtifactInfo = g.ui.GetUI<UIArtifactInfo>(UIType.ArtifactInfo);
            var uiPropInfo = g.ui.GetUI<UIPropInfo>(UIType.PropInfo);

            if (uiPropInfo.IsExists() &&
                (g.ui.HasUI(UIType.PropSelect) || g.ui.HasUI(UIType.PlayerInfo) || g.ui.HasUI(UIType.TownStorage) || (g.ui.HasUI(UIType.NPCInfo) && g.world.playerUnit.GetLuck(UnitTypeLuckEnum.Merchant.Value.Parse<int>()) != null)))
            {
                if (uiPropInfo.propData.propsItem.IsRing() != null && IsReady(uiPropInfo_textRefineAdj3, uiPropInfo))
                {
                    uiPropInfo_textRefineTitle.Pos(uiPropInfo.textGrade_En.gameObject, 0, -20);
                    uiPropInfo_textRefineAdj1.Pos(uiPropInfo.textGrade_En.gameObject, 5, -35);
                    uiPropInfo_textRefineAdj2.Pos(uiPropInfo.textGrade_En.gameObject, 5, -50);
                    uiPropInfo_textRefineAdj3.Pos(uiPropInfo.textGrade_En.gameObject, 5, -65);
                }
                else if (uiPropInfo.propData.propsItem.IsOutfit() != null && IsReady(uiPropInfo_textRefineAdj4, uiPropInfo))
                {
                    // top-left corner of the window (as the ring's block): over the name it hid the name and the type
                    uiPropInfo_textRefineTitle.Pos(uiPropInfo.textName.gameObject, 0, 80);
                    uiPropInfo_textRefineAdj1.Pos(uiPropInfo.textName.gameObject, 5, 65);
                    uiPropInfo_textRefineAdj2.Pos(uiPropInfo.textName.gameObject, 5, 50);
                    uiPropInfo_textRefineAdj3.Pos(uiPropInfo.textName.gameObject, 5, 35);
                    uiPropInfo_textRefineAdj4.Pos(uiPropInfo.textName.gameObject, 5, 20);
                }
                else if (uiPropInfo.propData.propsItem.IsMount() != null && IsReady(uiPropInfo_textRefineAdj3, uiPropInfo))
                {
                    uiPropInfo_textRefineTitle.Pos(uiPropInfo.textGrade_En.gameObject, 0, -20);
                    uiPropInfo_textRefineAdj1.Pos(uiPropInfo.textGrade_En.gameObject, 5, -35);
                    uiPropInfo_textRefineAdj2.Pos(uiPropInfo.textGrade_En.gameObject, 5, -50);
                    uiPropInfo_textRefineAdj3.Pos(uiPropInfo.textGrade_En.gameObject, 5, -65);
                }
            }

            // the texts are created by OnOpenUIEnd: this tick can come first (or still hold the texts of a closed window)
            if (uiArtifactInfo.IsExists() && IsReady(uiArtifactInfo_textRefineAdj5, uiArtifactInfo) &&
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

        /// <summary>
        /// Closes the expertise preview with MartialInfo.
        /// </summary>
        public void OnUIClose(CloseUIEnd e)
        {
            if (e.uiType.uiName == UIType.MartialInfo.uiName && g.ui.HasUI(UIType.NPCInfoPreview))
            {
                g.ui.CloseUI(uiMartialExpertInfo);
            }
        }

        /// <summary>
        /// Calls OnUIOpen.
        /// </summary>
        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            OnUIOpen(e);
        }

        /// <summary>
        /// Calls OnUIUpdate.
        /// </summary>
        [ErrorIgnore]
        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
        public override void OnTimeUpdate100ms()
        {
            base.OnTimeUpdate100ms();
            OnUIUpdate();
        }

        /// <summary>
        /// Calls OnUIClose.
        /// </summary>
        public override void OnCloseUIEnd(CloseUIEnd e)
        {
            base.OnCloseUIEnd(e);
            OnUIClose(e);
        }
    }
}
