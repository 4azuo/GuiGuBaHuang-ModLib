using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using EGameTypeData;
using static MOD_nE7UL2.Object.InGameStts._HideButtonConfigs;
using static MOD_nE7UL2.Object.InGameStts;
using System.Collections.Generic;
using ModLib.Enum;
using MOD_nE7UL2.Enum;
using static UIHelper;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.UI_EVENT)]
    public class UIEvent : ModEvent
    {
        public static _HideButtonConfigs Configs => ModMain.ModObj.InGameCustomSettings.HideButtonConfigs;

        private static Text uiNPCInfo_textMartialAdjHp;
        private static Text uiNPCInfo_textSpiritualAdjMp;
        private static Text uiNPCInfo_textArtisanshipAdjSp;
        private static Text uiNPCInfoSkill_textAbiPointAdjHp;
        private static Text uiNPCInfoSkill_textAbiPointAdjMp;
        private static Text uiPlayerInfo_textMartialAdjHp;
        private static Text uiPlayerInfo_textSpiritualAdjMp;
        private static Text uiPlayerInfo_textArtisanshipAdjSp;
        private static Text uiPlayerInfoSkill_textAbiPointAdjHp;
        private static Text uiPlayerInfoSkill_textAbiPointAdjMp;
        private static Text uiArtifactInfo_textBasicTitle;
        private static Text uiArtifactInfo_textBasicAdj1;
        private static Text uiArtifactInfo_textBasicAdj2;
        private static Text uiArtifactInfo_textBasicAdj3;
        private static Text uiArtifactInfo_textExpertLvl;
        private static Text uiArtifactInfo_textExpertAdj1;
        private static Text uiArtifactInfo_textExpertAdj2;
        private static Text uiArtifactInfo_textRefineTitle;
        private static Text uiArtifactInfo_textRefineAdj1;
        private static Text uiArtifactInfo_textRefineAdj2;
        private static Text uiArtifactInfo_textRefineAdj3;
        private static Text uiArtifactInfo_textRefineAdj4;
        private static Text uiArtifactInfo_textRefineAdj5;
        private static UINPCInfoPreview uiMartialExpertInfo;
        private static Text uiMartialExpertInfo_textExpertLvl;
        private static Text uiMartialExpertInfo_textAdj1;
        private static Text uiMartialExpertInfo_textAdj2;
        private static Text uiMartialExpertInfo_textAdj3;
        private static Text uiMartialExpertInfo_textAdj4;
        private static Text uiMartialExpertInfo_textAdj5;
        private static Text uiPropInfo_textRefineTitle;
        private static Text uiPropInfo_textRefineAdj1;
        private static Text uiPropInfo_textRefineAdj2;
        private static Text uiPropInfo_textRefineAdj3;
        private static Text uiPropInfo_textRefineAdj4;
        private static UITown uiTown;
        private static Button uiTown_tax;
        private static UISchool uiSchool;
        private static Button uiSchool_tax;

        public static void OnUIOpen(OpenUIEnd e)
        {
            var smConfigs = EventHelper.GetEvent<SMLocalConfigsEvent>(ModConst.SM_LOCAL_CONFIGS_EVENT);

            /*
             * Hide buttons
             */
            IDictionary<string, SelectOption> buttonConfigs;
            if (Configs.ButtonConfigs.TryGetValue(e.uiType.uiName, out buttonConfigs))
            {
                var ui = g.ui.GetUI(e.uiType);
                if (ui == null)
                    return;

                foreach (var buttonConfig in buttonConfigs)
                {
                    var comp = ui.GetComponentsInChildren<MonoBehaviour>().Where(x => buttonConfig.Key == x.name);
                    foreach (var c in comp)
                    {
                        c.gameObject.SetActive(buttonConfig.Value == SelectOption.Show);
                    }
                }
            }

            /*
             * UI
             */
            if (e.uiType.uiName == UIType.Town.uiName)
            {
                using (var a = new UISample())
                {
                    uiTown = g.ui.GetUI<UITown>(UIType.Town);
                    uiTown_tax = a.sampleUI.btnKeyOK.Create(uiTown.canvas.transform).AddSize(200f, 20f).Setup($"Tax: {MapBuildPropertyEvent.GetTax(g.world.playerUnit)} Spirit Stones");
                    uiTown_tax.enabled = false;
                }
            }
            else
            if (e.uiType.uiName == UIType.School.uiName)
            {
                using (var a = new UISample())
                {
                    uiSchool = g.ui.GetUI<UISchool>(UIType.School);
                    uiSchool_tax = a.sampleUI.btnKeyOK.Create(uiSchool.canvas.transform).AddSize(200f, 20f).Setup($"Tax: {MapBuildPropertyEvent.GetTax(g.world.playerUnit)} Spirit Stones");
                    uiSchool_tax.enabled = false;
                }
            }
            else
            if (e.uiType.uiName == UIType.MapMain.uiName)
            {
                var uiMapMain = g.ui.GetUI<UIMapMain>(UIType.MapMain);
                uiMapMain.playerInfo.textPiscesPendantCount.gameObject.SetActive(false);
                uiMapMain.playerInfo.goAddLuckRoot.SetActive(false);
            }
            else
            if (e.uiType.uiName == UIType.NPCInfo.uiName)
            {
                var uiNPCInfo = g.ui.GetUI<UINPCInfo>(UIType.NPCInfo);

                var sampleText1 = uiNPCInfo.uiProperty.goGroupRoot.GetComponentInChildren<Text>();
                uiNPCInfo_textMartialAdjHp = ObjectHelper.Create(sampleText1).Align(TextAnchor.MiddleCenter).Format(Color.white).Pos(uiNPCInfo.uiProperty.goItem6, 0.4f, -1.1f);
                uiNPCInfo_textSpiritualAdjMp = ObjectHelper.Create(sampleText1).Align(TextAnchor.MiddleCenter).Format(Color.white).Pos(uiNPCInfo.uiProperty.goItem7, 0.4f, -1.1f);
                uiNPCInfo_textArtisanshipAdjSp = ObjectHelper.Create(sampleText1).Align(TextAnchor.MiddleCenter).Format(Color.white).Pos(uiNPCInfo.uiProperty.goItem8, 0.4f, -1.1f);

                var sampleText2 = uiNPCInfo.uiSkill.textPoint1;
                uiNPCInfoSkill_textAbiPointAdjHp = ObjectHelper.Create(sampleText2).Align(TextAnchor.MiddleCenter).Format(Color.white).Pos(uiNPCInfo.uiSkill.textPoint1.gameObject, 0f, -0.2f);
                uiNPCInfoSkill_textAbiPointAdjMp = ObjectHelper.Create(sampleText2).Align(TextAnchor.MiddleCenter).Format(Color.white).Pos(uiNPCInfo.uiSkill.textPoint1.gameObject, 0f, -0.4f);
            }
            else
            if (e.uiType.uiName == UIType.PlayerInfo.uiName)
            {
                var uiPlayerInfo = g.ui.GetUI<UIPlayerInfo>(UIType.PlayerInfo);

                var sampleText1 = uiPlayerInfo.uiPropertyCommon.goGroupRoot.GetComponentInChildren<Text>();
                uiPlayerInfo_textMartialAdjHp = ObjectHelper.Create(sampleText1).Align(TextAnchor.MiddleCenter).Format().Pos(uiPlayerInfo.uiPropertyCommon.goItem6_En, 0.4f, -1.0f);
                uiPlayerInfo_textSpiritualAdjMp = ObjectHelper.Create(sampleText1).Align(TextAnchor.MiddleCenter).Format().Pos(uiPlayerInfo.uiPropertyCommon.goItem7_En, 0.4f, -1.0f);
                uiPlayerInfo_textArtisanshipAdjSp = ObjectHelper.Create(sampleText1).Align(TextAnchor.MiddleCenter).Format().Pos(uiPlayerInfo.uiPropertyCommon.goItem8_En, 0.4f, -1.0f);

                var sampleText2 = uiPlayerInfo.uiSkill.textPoint1;
                uiPlayerInfoSkill_textAbiPointAdjHp = ObjectHelper.Create(sampleText2).Align(TextAnchor.MiddleCenter).Format(Color.white).Pos(uiPlayerInfo.uiSkill.textPoint1.gameObject, 0f, -0.2f);
                uiPlayerInfoSkill_textAbiPointAdjMp = ObjectHelper.Create(sampleText2).Align(TextAnchor.MiddleCenter).Format(Color.white).Pos(uiPlayerInfo.uiSkill.textPoint1.gameObject, 0f, -0.4f);
            }
            else
            if (e.uiType.uiName == UIType.ArtifactInfo.uiName)
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
                uiArtifactInfo_textBasicTitle = ObjectHelper.Create(uiArtifactInfo.textGrade_En).Align().Format(Color.white, 15);
                uiArtifactInfo_textBasicAdj1 = ObjectHelper.Create(uiArtifactInfo.textGrade_En).Align().Format(Color.white, 14);
                uiArtifactInfo_textBasicAdj2 = ObjectHelper.Create(uiArtifactInfo.textGrade_En).Align().Format(Color.white, 14);
                uiArtifactInfo_textBasicAdj3 = ObjectHelper.Create(uiArtifactInfo.textGrade_En).Align().Format(Color.white, 14);

                uiArtifactInfo_textBasicTitle.Pos(uiArtifactInfo.textGrade_En.gameObject, 0f, -0.2f);
                uiArtifactInfo_textBasicAdj1.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.05f, -0.35f);
                uiArtifactInfo_textBasicAdj2.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.05f, -0.5f);
                uiArtifactInfo_textBasicAdj3.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.05f, -0.65f);

                uiArtifactInfo_textBasicTitle.text = $"Basic:";
                uiArtifactInfo_textBasicAdj1.text = $"+Atk: {UnitModifyHelper.GetArtifactBasicAdjAtk(uiArtifactInfo.unit.GetDynProperty(UnitDynPropertyEnum.Attack).baseValue, uiArtifactInfo.shapeProp, artifact)}";
                uiArtifactInfo_textBasicAdj2.text = $"+Def: {UnitModifyHelper.GetArtifactBasicAdjDef(uiArtifactInfo.unit.GetDynProperty(UnitDynPropertyEnum.Defense).baseValue, uiArtifactInfo.shapeProp, artifact)}";
                uiArtifactInfo_textBasicAdj3.text = $"+Hp: {UnitModifyHelper.GetArtifactBasicAdjHp(uiArtifactInfo.unit.GetDynProperty(UnitDynPropertyEnum.HpMax).baseValue, uiArtifactInfo.shapeProp, artifact)}";

                //expert
                uiArtifactInfo_textExpertLvl = ObjectHelper.Create(uiArtifactInfo.textGrade_En).Align().Format(Color.white, 15);
                uiArtifactInfo_textExpertAdj1 = ObjectHelper.Create(uiArtifactInfo.textGrade_En).Align().Format(Color.white, 14);
                uiArtifactInfo_textExpertAdj2 = ObjectHelper.Create(uiArtifactInfo.textGrade_En).Align().Format(Color.white, 14);

                uiArtifactInfo_textExpertLvl.Pos(uiArtifactInfo.textGrade_En.gameObject, 0f, -0.85f);
                uiArtifactInfo_textExpertAdj1.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.05f, -1.0f);
                uiArtifactInfo_textExpertAdj2.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.05f, -1.15f);

                uiArtifactInfo_textExpertLvl.text = $"Expert Lvl: {expertLvl} ({expertExp}/{expertNeedExp})";
                uiArtifactInfo_textExpertAdj1.text = $"+Atk: {UnitModifyHelper.GetArtifactExpertAtk(artifactConf.atk, expertLvl, propsGrade, propsLevel)}";
                uiArtifactInfo_textExpertAdj2.text = $"+Def: {UnitModifyHelper.GetArtifactExpertDef(artifactConf.def, expertLvl, propsGrade, propsLevel)}";

                //refine
                var refineLvl = CustomRefineEvent.GetRefineLvl(uiArtifactInfo.shapeProp);
                var customAdj1 = CustomRefineEvent.GetCustomAdjType(uiArtifactInfo.shapeProp, 1);
                var customAdj2 = CustomRefineEvent.GetCustomAdjType(uiArtifactInfo.shapeProp, 2);
                var customAdj3 = CustomRefineEvent.GetCustomAdjType(uiArtifactInfo.shapeProp, 3);

                uiArtifactInfo_textRefineTitle = ObjectHelper.Create(uiArtifactInfo.textGrade_En).Align(TextAnchor.MiddleRight).Format(Color.white, 15);
                uiArtifactInfo_textRefineAdj1 = ObjectHelper.Create(uiArtifactInfo.textGrade_En).Align(TextAnchor.MiddleRight).Format(Color.white, 14);
                uiArtifactInfo_textRefineAdj2 = ObjectHelper.Create(uiArtifactInfo.textGrade_En).Align(TextAnchor.MiddleRight).Format(Color.white, 14);
                uiArtifactInfo_textRefineAdj3 = ObjectHelper.Create(uiArtifactInfo.textGrade_En).Align(TextAnchor.MiddleRight).Format(customAdj1?.GetColor(refineLvl) ?? AdjLevelEnum.None.Color, 14);
                uiArtifactInfo_textRefineAdj4 = ObjectHelper.Create(uiArtifactInfo.textGrade_En).Align(TextAnchor.MiddleRight).Format(customAdj2?.GetColor(refineLvl) ?? AdjLevelEnum.None.Color, 14);
                uiArtifactInfo_textRefineAdj5 = ObjectHelper.Create(uiArtifactInfo.textGrade_En).Align(TextAnchor.MiddleRight).Format(customAdj3?.GetColor(refineLvl) ?? AdjLevelEnum.None.Color, 14);

                uiArtifactInfo_textRefineTitle.Pos(uiArtifactInfo.textGrade_En.gameObject, 0f, -1.35f);
                uiArtifactInfo_textRefineAdj1.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.05f, -1.5f);
                uiArtifactInfo_textRefineAdj2.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.05f, -1.65f);
                uiArtifactInfo_textRefineAdj3.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.05f, -1.8f);
                uiArtifactInfo_textRefineAdj4.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.05f, -1.95f);
                uiArtifactInfo_textRefineAdj5.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.05f, -2.1f);

                uiArtifactInfo_textRefineTitle.text = $"Refine ({refineLvl}):";
                uiArtifactInfo_textRefineAdj1.text = $"+Atk: {UnitModifyHelper.GetRefineArtifactAdjAtk(uiArtifactInfo.shapeProp, refineLvl)}";
                uiArtifactInfo_textRefineAdj2.text = $"+Def: {UnitModifyHelper.GetRefineArtifactAdjDef(uiArtifactInfo.shapeProp, refineLvl)}";
                uiArtifactInfo_textRefineAdj3.text = customAdj1?.GetText(uiArtifactInfo.unit, uiArtifactInfo.shapeProp, refineLvl);
                uiArtifactInfo_textRefineAdj4.text = customAdj2?.GetText(uiArtifactInfo.unit, uiArtifactInfo.shapeProp, refineLvl);
                uiArtifactInfo_textRefineAdj5.text = customAdj3?.GetText(uiArtifactInfo.unit, uiArtifactInfo.shapeProp, refineLvl);
            }
            else
            if (e.uiType.uiName == UIType.MartialInfo.uiName)
            {
                uiMartialExpertInfo = g.ui.OpenUI<UINPCInfoPreview>(UIType.NPCInfoPreview);
            }
            else
            if (e.uiType.uiName == UIType.NPCInfoPreview.uiName && (g.ui.GetUI(UIType.MartialInfo)?.gameObject?.active).Is(true) == 1)
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
            if (e.uiType.uiName == UIType.PropInfo.uiName)
            {
                var uiPropInfo = g.ui.GetUI<UIPropInfo>(UIType.PropInfo);
                if (uiPropInfo.propData.propsItem.IsRing() != null)
                {
                    var refineLvl = CustomRefineEvent.GetRefineLvl(uiPropInfo.propData);
                    var customAdj1 = CustomRefineEvent.GetCustomAdjType(uiPropInfo.propData, 1);
                    var customAdj2 = CustomRefineEvent.GetCustomAdjType(uiPropInfo.propData, 2);

                    uiPropInfo_textRefineTitle = ObjectHelper.Create(uiPropInfo.textGrade_En).Align().Format(Color.white, 15);
                    uiPropInfo_textRefineAdj1 = ObjectHelper.Create(uiPropInfo.textGrade_En).Align().Format(Color.white, 14);
                    uiPropInfo_textRefineAdj2 = ObjectHelper.Create(uiPropInfo.textGrade_En).Align().Format(customAdj1?.GetColor(refineLvl) ?? AdjLevelEnum.None.Color, 14);
                    uiPropInfo_textRefineAdj3 = ObjectHelper.Create(uiPropInfo.textGrade_En).Align().Format(customAdj2?.GetColor(refineLvl) ?? AdjLevelEnum.None.Color, 14);

                    uiPropInfo_textRefineTitle.Pos(uiPropInfo.textGrade_En.gameObject, 0f, -0.2f);
                    uiPropInfo_textRefineAdj1.Pos(uiPropInfo.textGrade_En.gameObject, +0.05f, -0.35f);
                    uiPropInfo_textRefineAdj2.Pos(uiPropInfo.textGrade_En.gameObject, +0.05f, -0.5f);
                    uiPropInfo_textRefineAdj3.Pos(uiPropInfo.textGrade_En.gameObject, +0.05f, -0.65f);

                    uiPropInfo_textRefineTitle.text = $"Refine ({refineLvl}):";
                    uiPropInfo_textRefineAdj1.text = $"+Hp: {UnitModifyHelper.GetRefineRingAdjHp(uiPropInfo.unit.GetDynProperty(UnitDynPropertyEnum.HpMax).baseValue, uiPropInfo.propData, refineLvl)}";
                    uiPropInfo_textRefineAdj2.text = customAdj1?.GetText(uiPropInfo.unit, uiPropInfo.propData, refineLvl);
                    uiPropInfo_textRefineAdj3.text = customAdj2?.GetText(uiPropInfo.unit, uiPropInfo.propData, refineLvl);
                }
                else if (uiPropInfo.propData.propsItem.IsOutfit() != null)
                {
                    var refineLvl = CustomRefineEvent.GetRefineLvl(uiPropInfo.propData);
                    var customAdj1 = CustomRefineEvent.GetCustomAdjType(uiPropInfo.propData, 1);
                    var customAdj2 = CustomRefineEvent.GetCustomAdjType(uiPropInfo.propData, 2);

                    uiPropInfo_textRefineTitle = ObjectHelper.Create(uiPropInfo.textName).Align().Format(Color.white, 15);
                    uiPropInfo_textRefineAdj1 = ObjectHelper.Create(uiPropInfo.textName).Align().Format(Color.white, 14);
                    uiPropInfo_textRefineAdj2 = ObjectHelper.Create(uiPropInfo.textName).Align().Format(Color.white, 14);
                    uiPropInfo_textRefineAdj3 = ObjectHelper.Create(uiPropInfo.textName).Align().Format(customAdj1?.GetColor(refineLvl) ?? AdjLevelEnum.None.Color, 14);
                    uiPropInfo_textRefineAdj4 = ObjectHelper.Create(uiPropInfo.textName).Align().Format(customAdj2?.GetColor(refineLvl) ?? AdjLevelEnum.None.Color, 14);

                    uiPropInfo_textRefineTitle.Pos(uiPropInfo.textName.gameObject, 0f, 0.8f);
                    uiPropInfo_textRefineAdj1.Pos(uiPropInfo.textName.gameObject, +0.05f, 0.65f);
                    uiPropInfo_textRefineAdj2.Pos(uiPropInfo.textName.gameObject, +0.05f, 0.5f);
                    uiPropInfo_textRefineAdj3.Pos(uiPropInfo.textName.gameObject, +0.05f, 0.35f);
                    uiPropInfo_textRefineAdj4.Pos(uiPropInfo.textName.gameObject, +0.05f, 0.2f);

                    uiPropInfo_textRefineTitle.text = $"Refine ({refineLvl}):";
                    uiPropInfo_textRefineAdj1.text = $"+Hp: {UnitModifyHelper.GetRefineOutfitAdjHp(uiPropInfo.unit.GetDynProperty(UnitDynPropertyEnum.HpMax).baseValue, uiPropInfo.propData, refineLvl)}";
                    uiPropInfo_textRefineAdj2.text = $"+Def: {UnitModifyHelper.GetRefineOutfitAdjDef(uiPropInfo.unit.GetDynProperty(UnitDynPropertyEnum.Defense).baseValue, uiPropInfo.propData, refineLvl)}";
                    uiPropInfo_textRefineAdj3.text = customAdj1?.GetText(uiPropInfo.unit, uiPropInfo.propData, refineLvl);
                    uiPropInfo_textRefineAdj4.text = customAdj2?.GetText(uiPropInfo.unit, uiPropInfo.propData, refineLvl);
                }
            }
            else
            if (e.uiType.uiName == UIType.GameMemu.uiName)
            {
                var ui = g.ui.GetUI<UIGameMemu>(UIType.GameMemu);
                ui.btnSave.gameObject.SetActive(!smConfigs.Configs.HideSaveButton);
                ui.btnReloadCache.gameObject.SetActive(!smConfigs.Configs.HideReloadButton);
            }
        }

        public static void OnUIUpdate()
        {
            var uiTown = g.ui.GetUI<UITown>(UIType.Town);
            var uiSchool = g.ui.GetUI<UISchool>(UIType.School);
            var uiNPCInfo = g.ui.GetUI<UINPCInfo>(UIType.NPCInfo);
            var uiPlayerInfo = g.ui.GetUI<UIPlayerInfo>(UIType.PlayerInfo);
            var uiArtifactInfo = g.ui.GetUI<UIArtifactInfo>(UIType.ArtifactInfo);
            var uiPropInfo = g.ui.GetUI<UIPropInfo>(UIType.PropInfo);

            if ((uiTown?.gameObject?.active).Is(true) == 1)
            {
                uiTown_tax.Pos(uiTown.btnClose.gameObject, -2f, 0f);
            }
            
            if ((uiSchool?.gameObject?.active).Is(true) == 1)
            {
                uiSchool_tax.Pos(uiSchool.btnClose.gameObject, -2f, 0f);
            }

            if ((uiPropInfo?.gameObject?.active).Is(true) == 1)
            {
                if (uiPropInfo.propData.propsItem.IsRing() != null)
                {
                    uiPropInfo_textRefineTitle.Pos(uiPropInfo.textGrade_En.gameObject, 0f, -0.2f);
                    uiPropInfo_textRefineAdj1.Pos(uiPropInfo.textGrade_En.gameObject, +0.05f, -0.35f);
                    uiPropInfo_textRefineAdj2.Pos(uiPropInfo.textGrade_En.gameObject, +0.05f, -0.5f);
                    uiPropInfo_textRefineAdj3.Pos(uiPropInfo.textGrade_En.gameObject, +0.05f, -0.65f);
                }
                else if (uiPropInfo.propData.propsItem.IsOutfit() != null)
                {
                    uiPropInfo_textRefineTitle.Pos(uiPropInfo.textName.gameObject, 0f, 0.8f);
                    uiPropInfo_textRefineAdj1.Pos(uiPropInfo.textName.gameObject, +0.05f, 0.65f);
                    uiPropInfo_textRefineAdj2.Pos(uiPropInfo.textName.gameObject, +0.05f, 0.5f);
                    uiPropInfo_textRefineAdj3.Pos(uiPropInfo.textName.gameObject, +0.05f, 0.35f);
                    uiPropInfo_textRefineAdj4.Pos(uiPropInfo.textName.gameObject, +0.05f, 0.2f);
                }
            }
            
            if ((uiArtifactInfo?.gameObject?.active).Is(true) == 1)
            {
                uiArtifactInfo_textBasicTitle.Pos(uiArtifactInfo.textGrade_En.gameObject, 0f, -0.2f);
                uiArtifactInfo_textBasicAdj1.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.05f, -0.35f);
                uiArtifactInfo_textBasicAdj2.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.05f, -0.5f);
                uiArtifactInfo_textBasicAdj3.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.05f, -0.65f);
                uiArtifactInfo_textExpertLvl.Pos(uiArtifactInfo.textGrade_En.gameObject, 0f, -0.85f);
                uiArtifactInfo_textExpertAdj1.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.05f, -1.0f);
                uiArtifactInfo_textExpertAdj2.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.05f, -1.15f);
                uiArtifactInfo_textRefineTitle.Pos(uiArtifactInfo.textGrade_En.gameObject, 0f, -1.35f);
                uiArtifactInfo_textRefineAdj1.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.05f, -1.5f);
                uiArtifactInfo_textRefineAdj2.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.05f, -1.65f);
                uiArtifactInfo_textRefineAdj3.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.05f, -1.8f);
                uiArtifactInfo_textRefineAdj4.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.05f, -1.95f);
                uiArtifactInfo_textRefineAdj5.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.05f, -2.1f);
            }
            
            if ((uiNPCInfo?.gameObject?.active).Is(true) == 1)
            {
                uiNPCInfo_textMartialAdjHp.text = $"+Hp: {UnitModifyHelper.GetMartialAdjHp(uiNPCInfo.unit)}";
                uiNPCInfo_textSpiritualAdjMp.text = $"+Mp: {UnitModifyHelper.GetSpiritualAdjMp(uiNPCInfo.unit)}";
                uiNPCInfo_textArtisanshipAdjSp.text = $"+Sp: {UnitModifyHelper.GetArtisanshipAdjSp(uiNPCInfo.unit)}";
                uiNPCInfoSkill_textAbiPointAdjHp.text = $"+Hp: {UnitModifyHelper.GetAbiPointAdjHp(uiNPCInfo.unit)}";
                uiNPCInfoSkill_textAbiPointAdjMp.text = $"+Mp: {UnitModifyHelper.GetAbiPointAdjMp(uiNPCInfo.unit)}";
            }
            
            if ((uiPlayerInfo?.gameObject?.active).Is(true) == 1)
            {
                uiPlayerInfo_textMartialAdjHp.text = $"+Hp: {UnitModifyHelper.GetMartialAdjHp(uiPlayerInfo.unit)}";
                uiPlayerInfo_textSpiritualAdjMp.text = $"+Mp: {UnitModifyHelper.GetSpiritualAdjMp(uiPlayerInfo.unit)}";
                uiPlayerInfo_textArtisanshipAdjSp.text = $"+Sp: {UnitModifyHelper.GetArtisanshipAdjSp(uiPlayerInfo.unit)}";
                uiPlayerInfoSkill_textAbiPointAdjHp.text = $"+Hp: {UnitModifyHelper.GetAbiPointAdjHp(uiPlayerInfo.unit)}";
                uiPlayerInfoSkill_textAbiPointAdjMp.text = $"+Mp: {UnitModifyHelper.GetAbiPointAdjMp(uiPlayerInfo.unit)}";
            }
        }

        public static void OnUIClose(CloseUIEnd e)
        {
            if (e.uiType.uiName == UIType.MartialInfo.uiName && (g.ui.GetUI(UIType.NPCInfoPreview)?.gameObject?.active).Is(true) == 1)
            {
                g.ui.CloseUI(uiMartialExpertInfo);
            }
        }

        [EventCondition]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            OnUIOpen(e);
        }

        [ErrorIgnore]
        [EventCondition]
        public override void OnTimeUpdate()
        {
            base.OnTimeUpdate();
            OnUIUpdate();
        }

        public override void OnCloseUIEnd(CloseUIEnd e)
        {
            base.OnCloseUIEnd(e);
            OnUIClose(e);
        }
    }
}
