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
using static SpecialBattle83;

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
        private static UINPCInfoPreview uiMartialExpertInfo;
        private static Text uiMartialExpertInfo_textExpertLvl;
        private static Text uiMartialExpertInfo_textAdj1;
        private static Text uiMartialExpertInfo_textAdj2;
        private static Text uiMartialExpertInfo_textAdj3;
        private static Text uiMartialExpertInfo_textAdj4;
        private static Text uiMartialExpertInfo_textAdj5;
        private static Text uiPropInfo_textBasicTitle;
        private static Text uiPropInfo_textBasicAdj1;
        private static Text uiPropInfo_textBasicAdj2;

        public static void OnUIOpen(OpenUIEnd e)
        {
            var player = g.world.playerUnit;

            /*
             * UI
             */
            //DebugHelper.WriteLine(e.uiType.uiName);

            //if (e.uiType.uiName == UIType.ArtifactShapeMade.uiName)
            //{
            //    var ui = g.ui.GetUI<UIArtifactShapeMade>(UIType.ArtifactShapeMade);
            //    if (!ui.isPlayerMade)
            //    {
            //        ui.
            //    }
            //}

            if (e.uiType.uiName == UIType.MapMain.uiName)
            {
                var uiMapMain = g.ui.GetUI<UIMapMain>(UIType.MapMain);
                uiMapMain.playerInfo.textPiscesPendantCount.gameObject.SetActive(false);
                uiMapMain.playerInfo.goAddLuckRoot.SetActive(false);
            }

            if (e.uiType.uiName == UIType.BattleInfo.uiName)
            {
                var uiBattleInfo = g.ui.GetUI<UIBattleInfo>(UIType.BattleInfo);
                uiBattleInfo.uiInfo.goMonstCount1.SetActive(false);
                uiBattleInfo.uiInfo.goMonstCount2.SetActive(false);
                uiBattleInfo.uiMap.goGroupRoot.SetActive(false);
            }

            if (e.uiType.uiName == UIType.NPCInfo.uiName)
            {
                var uiNPCInfo = g.ui.GetUI<UINPCInfo>(UIType.NPCInfo);

                var sampleText1 = uiNPCInfo.uiProperty.goGroupRoot.GetComponentInChildren<Text>();
                uiNPCInfo_textMartialAdjHp = ObjectHelper.Create(sampleText1).Align(TextAnchor.MiddleCenter).Format(Color.white).Pos(uiNPCInfo.uiProperty.goItem6, 0.4f, -1.0f);
                uiNPCInfo_textSpiritualAdjMp = ObjectHelper.Create(sampleText1).Align(TextAnchor.MiddleCenter).Format(Color.white).Pos(uiNPCInfo.uiProperty.goItem7, 0.4f, -1.0f);
                uiNPCInfo_textArtisanshipAdjSp = ObjectHelper.Create(sampleText1).Align(TextAnchor.MiddleCenter).Format(Color.white).Pos(uiNPCInfo.uiProperty.goItem8, 0.4f, -1.0f);

                var sampleText2 = uiNPCInfo.uiSkill.textPoint1;
                uiNPCInfoSkill_textAbiPointAdjHp = ObjectHelper.Create(sampleText2).Align(TextAnchor.MiddleCenter).Format(Color.white).Pos(uiNPCInfo.uiSkill.textPoint1.gameObject, 0f, -0.2f);
                uiNPCInfoSkill_textAbiPointAdjMp = ObjectHelper.Create(sampleText2).Align(TextAnchor.MiddleCenter).Format(Color.white).Pos(uiNPCInfo.uiSkill.textPoint1.gameObject, 0f, -0.4f);
            }

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

                uiArtifactInfo_textBasicTitle = ObjectHelper.Create(uiArtifactInfo.textGrade_En).Align().Format(Color.white, 16);
                uiArtifactInfo_textBasicAdj1 = ObjectHelper.Create(uiArtifactInfo.textGrade_En).Align().Format(Color.white);
                uiArtifactInfo_textBasicAdj2 = ObjectHelper.Create(uiArtifactInfo.textGrade_En).Align().Format(Color.white);
                uiArtifactInfo_textBasicAdj3 = ObjectHelper.Create(uiArtifactInfo.textGrade_En).Align().Format(Color.white);
                uiArtifactInfo_textExpertLvl = ObjectHelper.Create(uiArtifactInfo.textGrade_En).Align().Format(Color.white, 16);
                uiArtifactInfo_textExpertAdj1 = ObjectHelper.Create(uiArtifactInfo.textGrade_En).Align().Format(Color.white);
                uiArtifactInfo_textExpertAdj2 = ObjectHelper.Create(uiArtifactInfo.textGrade_En).Align().Format(Color.white);

                uiArtifactInfo_textBasicTitle.Pos(uiArtifactInfo.textGrade_En.gameObject, 0f, -0.3f);
                uiArtifactInfo_textBasicAdj1.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.1f, -0.5f);
                uiArtifactInfo_textBasicAdj2.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.1f, -0.7f);
                uiArtifactInfo_textBasicAdj3.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.1f, -0.9f);
                uiArtifactInfo_textExpertLvl.Pos(uiArtifactInfo.textGrade_En.gameObject, 0f, -1.2f);
                uiArtifactInfo_textExpertAdj1.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.1f, -1.4f);
                uiArtifactInfo_textExpertAdj2.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.1f, -1.6f);

                uiArtifactInfo_textBasicTitle.text = $"Basic:";
                uiArtifactInfo_textBasicAdj1.text = $"+Atk: {UnitModifyHelper.GetArtifactBasicAdjAtk(uiArtifactInfo.unit.GetDynProperty(UnitDynPropertyEnum.Attack).baseValue, uiArtifactInfo.shapeProp, artifact)}";
                uiArtifactInfo_textBasicAdj2.text = $"+Def: {UnitModifyHelper.GetArtifactBasicAdjDef(uiArtifactInfo.unit.GetDynProperty(UnitDynPropertyEnum.Defense).baseValue, uiArtifactInfo.shapeProp, artifact)}";
                uiArtifactInfo_textBasicAdj3.text = $"+Hp: {UnitModifyHelper.GetArtifactBasicAdjHp(uiArtifactInfo.unit.GetDynProperty(UnitDynPropertyEnum.HpMax).baseValue, uiArtifactInfo.shapeProp, artifact)}";
                uiArtifactInfo_textExpertLvl.text = $"Expert Lvl: {expertLvl} ({expertExp}/{expertNeedExp})";
                uiArtifactInfo_textExpertAdj1.text = $"+Atk: {UnitModifyHelper.GetArtifactExpertAtk(artifactConf.atk, expertLvl, propsGrade, propsLevel)}";
                uiArtifactInfo_textExpertAdj2.text = $"+Def: {UnitModifyHelper.GetArtifactExpertDef(artifactConf.def, expertLvl, propsGrade, propsLevel)}";
            }

            if (e.uiType.uiName == UIType.MartialInfo.uiName)
            {
                var uiMartialInfo = g.ui.GetUI<UIMartialInfo>(UIType.MartialInfo);

                var soleId = uiMartialInfo.martialData.martialInfo.propsData.soleID;
                var propsGrade = uiMartialInfo.martialData.martialInfo.grade;
                var propsLevel = uiMartialInfo.martialData.martialInfo.level;
                var mType = uiMartialInfo.martialData.martialType;
                var expertLvl = ExpertEvent.GetExpertLvl(soleId, propsGrade, propsLevel);
                var expertExp = ExpertEvent.GetExpertExp(soleId);
                var expertNeedExp = ExpertEvent.GetExpertNeedExp(expertLvl + 1, propsGrade, propsLevel);

                uiMartialExpertInfo = g.ui.OpenUI<UINPCInfoPreview>(UIType.NPCInfoPreview);

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
                    uiMartialExpertInfo_textAdj2.text = $"+Evade: {UnitModifyHelper.GetStepExpertEvade(expertLvl, propsGrade, propsLevel)}% (In Battle)";
                    uiMartialExpertInfo_textAdj3.gameObject.SetActive(false);
                    uiMartialExpertInfo_textAdj4.gameObject.SetActive(false);
                    uiMartialExpertInfo_textAdj5.gameObject.SetActive(false);
                }
                else
                {
                    uiMartialExpertInfo_textAdj1.text = $"+Dmg: {UnitModifyHelper.GetSkillExpertAtk(uiMartialInfo.unit.GetDynProperty(UnitDynPropertyEnum.Attack).baseValue, expertLvl, propsGrade, propsLevel, mType)} (In Battle)";
                    uiMartialExpertInfo_textAdj2.text = $"+MpCost: {UnitModifyHelper.GetSkillExpertMpCost(SkillHelper.GetSkillMpCost(uiMartialInfo.martialData), expertLvl, propsGrade, propsLevel, uiMartialInfo.unit.GetGradeLvl() * expertLvl / 10)} (In Battle)";
                    uiMartialExpertInfo_textAdj3.gameObject.SetActive(false);
                    uiMartialExpertInfo_textAdj4.gameObject.SetActive(false);
                    uiMartialExpertInfo_textAdj5.gameObject.SetActive(false);
                }

                foreach (var comp in uiMartialExpertInfo.GetComponentsInChildren<Text>())
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

            if (e.uiType.uiName == UIType.NPCInfoPreview.uiName && (g.ui.GetUI(UIType.MartialInfo)?.gameObject?.active ?? false))
            {
                uiMartialExpertInfo.transform.position = new Vector3(0f, 5f);
            }

            if (e.uiType.uiName == UIType.PropInfo.uiName)
            {
                var uiPropInfo = g.ui.GetUI<UIPropInfo>(UIType.PropInfo);
                if (uiPropInfo.propData.propsItem.IsRing() != null)
                {
                    uiPropInfo_textBasicTitle = ObjectHelper.Create(uiPropInfo.textGrade_En).Align().Format(Color.white, 16);
                    uiPropInfo_textBasicAdj1 = ObjectHelper.Create(uiPropInfo.textGrade_En).Align().Format(Color.white);

                    uiPropInfo_textBasicTitle.Pos(uiPropInfo.textGrade_En.gameObject, 0f, -0.3f);
                    uiPropInfo_textBasicAdj1.Pos(uiPropInfo.textGrade_En.gameObject, +0.1f, -0.5f);

                    var refineLvl = CustomRefineEvent.GetRefineLvl(uiPropInfo.propData);
                    uiPropInfo_textBasicTitle.text = $"Bonus (Refine {refineLvl}):";
                    uiPropInfo_textBasicAdj1.text = $"+Hp: {UnitModifyHelper.GetRingAdjHp(uiPropInfo.unit.GetDynProperty(UnitDynPropertyEnum.HpMax).baseValue, uiPropInfo.propData, refineLvl)}";
                }
                else if (uiPropInfo.propData.propsItem.IsOutfit() != null)
                {
                    uiPropInfo_textBasicTitle = ObjectHelper.Create(uiPropInfo.textName).Align().Format(Color.white, 16);
                    uiPropInfo_textBasicAdj1 = ObjectHelper.Create(uiPropInfo.textName).Align().Format(Color.white);
                    uiPropInfo_textBasicAdj2 = ObjectHelper.Create(uiPropInfo.textName).Align().Format(Color.white);

                    uiPropInfo_textBasicTitle.Pos(uiPropInfo.textName.gameObject, 0f, 0.9f);
                    uiPropInfo_textBasicAdj1.Pos(uiPropInfo.textName.gameObject, +0.1f, 0.7f);
                    uiPropInfo_textBasicAdj2.Pos(uiPropInfo.textName.gameObject, +0.1f, 0.5f);

                    var refineLvl = CustomRefineEvent.GetRefineLvl(uiPropInfo.propData);
                    uiPropInfo_textBasicTitle.text = $"Bonus (Refine {refineLvl}):";
                    uiPropInfo_textBasicAdj1.text = $"+Hp: {UnitModifyHelper.GetOutfitAdjHp(uiPropInfo.unit.GetDynProperty(UnitDynPropertyEnum.HpMax).baseValue, uiPropInfo.propData, refineLvl)}";
                    uiPropInfo_textBasicAdj2.text = $"+Def: {UnitModifyHelper.GetOutfitAdjDef(uiPropInfo.unit.GetDynProperty(UnitDynPropertyEnum.Defense).baseValue, uiPropInfo.propData, refineLvl)}";
                }
            }

            /*
             * Hide buttons
             */
            if (e?.uiType?.uiName == null || Configs?.ButtonConfigs == null)
                return;

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
        }

        public static void OnUIUpdate()
        {
            var uiNPCInfo = g.ui.GetUI<UINPCInfo>(UIType.NPCInfo);
            var uiPlayerInfo = g.ui.GetUI<UIPlayerInfo>(UIType.PlayerInfo);
            var uiArtifactInfo = g.ui.GetUI<UIArtifactInfo>(UIType.ArtifactInfo);
            var uiPropInfo = g.ui.GetUI<UIPropInfo>(UIType.PropInfo);

            if (uiPropInfo?.gameObject?.active ?? false)
            {
                if (uiPropInfo.propData.propsItem.IsRing() != null)
                {
                    uiPropInfo_textBasicTitle.Pos(uiPropInfo.textGrade_En.gameObject, 0f, -0.3f);
                    uiPropInfo_textBasicAdj1.Pos(uiPropInfo.textGrade_En.gameObject, +0.1f, -0.5f);
                }
                else if (uiPropInfo.propData.propsItem.IsOutfit() != null)
                {
                    uiPropInfo_textBasicTitle.Pos(uiPropInfo.textName.gameObject, 0f, 0.9f);
                    uiPropInfo_textBasicAdj1.Pos(uiPropInfo.textName.gameObject, +0.1f, 0.7f);
                    uiPropInfo_textBasicAdj2.Pos(uiPropInfo.textName.gameObject, +0.1f, 0.5f);
                }
            }

            if (uiArtifactInfo?.gameObject?.active ?? false)
            {
                uiArtifactInfo_textBasicTitle.Pos(uiArtifactInfo.textGrade_En.gameObject, 0f, -0.3f);
                uiArtifactInfo_textBasicAdj1.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.1f, -0.5f);
                uiArtifactInfo_textBasicAdj2.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.1f, -0.7f);
                uiArtifactInfo_textBasicAdj3.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.1f, -0.9f);
                uiArtifactInfo_textExpertLvl.Pos(uiArtifactInfo.textGrade_En.gameObject, 0f, -1.2f);
                uiArtifactInfo_textExpertAdj1.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.1f, -1.4f);
                uiArtifactInfo_textExpertAdj2.Pos(uiArtifactInfo.textGrade_En.gameObject, +0.1f, -1.6f);
            }

            if (uiNPCInfo?.gameObject?.active ?? false)
            {
                uiNPCInfo_textMartialAdjHp.text = $"+Hp: {UnitModifyHelper.GetMartialAdjHp(uiNPCInfo.unit)}";
                uiNPCInfo_textSpiritualAdjMp.text = $"+Mp: {UnitModifyHelper.GetSpiritualAdjMp(uiNPCInfo.unit)}";
                uiNPCInfo_textArtisanshipAdjSp.text = $"+Sp: {UnitModifyHelper.GetArtisanshipAdjSp(uiNPCInfo.unit)}";
                uiNPCInfoSkill_textAbiPointAdjHp.text = $"+Hp: {UnitModifyHelper.GetAbiPointAdjHp(uiNPCInfo.unit)}";
                uiNPCInfoSkill_textAbiPointAdjMp.text = $"+Mp: {UnitModifyHelper.GetAbiPointAdjMp(uiNPCInfo.unit)}";
            }

            if (uiPlayerInfo?.gameObject?.active ?? false)
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
            if (e.uiType.uiName == UIType.MartialInfo.uiName && (g.ui.GetUI(UIType.NPCInfoPreview)?.gameObject?.active ?? false))
            {
                g.ui.CloseUI(uiMartialExpertInfo);
            }
        }

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            OnUIOpen(e);
        }

        [ErrorIgnore]
        public override void OnTimeUpdate200ms()
        {
            base.OnTimeUpdate200ms();
            OnUIUpdate();
        }

        public override void OnCloseUIEnd(CloseUIEnd e)
        {
            base.OnCloseUIEnd(e);
            OnUIClose(e);
        }
    }
}
