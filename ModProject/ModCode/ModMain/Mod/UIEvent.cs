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
using TMPro;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.UI_EVENT)]
    public class UIEvent : ModEvent
    {
        public static _HideButtonConfigs Configs => ModMain.ModObj.InGameCustomSettings.HideButtonConfigs;

        //public static bool IsForeHideConfigOK(string conf)
        //{
        //    if (string.IsNullOrEmpty(conf))
        //        return false;
        //    conf = conf.Replace("${gradelevel}", g.world.playerUnit.GetGradeLvl().ToString())
        //        .Replace("${gamelevel}", g.data.dataWorld.data.gameLevel.Parse<int>().ToString());
        //    return Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScript.EvaluateAsync<bool>(conf).Result;
        //}

        private static bool uiArtifactInfo_mod = false;
        private static Text uiArtifactInfo_textExpertLvl;
        private static Text uiArtifactInfo_textAdjAtk;
        private static Text uiArtifactInfo_textAdjDef;
        private static bool uiMartialInfo_mod = false;
        private static UINPCInfoPreview uiMartialExpertInfo;
        private static Text uiMartialExpertInfo_textExpertLvl;
        private static Text uiMartialExpertInfo_textAdj1;
        private static Text uiMartialExpertInfo_textAdj2;
        private static Text uiMartialExpertInfo_textAdj3;
        private static Text uiMartialExpertInfo_textAdj4;
        private static Text uiMartialExpertInfo_textAdj5;
        public static void OnUIOpen(OpenUIEnd e)
        {
            var uiMapMain = MonoBehaviour.FindObjectOfType<UIMapMain>();
            if (uiMapMain != null)
            {
                uiMapMain.playerInfo.textPiscesPendantCount.gameObject.SetActive(false);
                uiMapMain.playerInfo.goAddLuckRoot.SetActive(false);
            }

            var uiBattleInfo = MonoBehaviour.FindObjectOfType<UIBattleInfo>();
            if (uiBattleInfo != null)
            {
                uiBattleInfo.uiInfo.goMonstCount1.SetActive(false);
                uiBattleInfo.uiInfo.goMonstCount2.SetActive(false);
                uiBattleInfo.uiMap.goGroupRoot.SetActive(false);
            }

            var uiArtifactInfo = MonoBehaviour.FindObjectOfType<UIArtifactInfo>();
            if (uiArtifactInfo != null)
            {
                if (!uiArtifactInfo_mod)
                {
                    var artifactConf = uiArtifactInfo.shapeProp.propsItem.IsArtifact();
                    var soleId = uiArtifactInfo.shapeProp.soleID;
                    var propsGrade = uiArtifactInfo.shapeProp.propsInfoBase.grade;
                    var propsLevel = uiArtifactInfo.shapeProp.propsInfoBase.level;
                    var expertLvl = ExpertEvent.GetExpertLvl(soleId, propsGrade, propsLevel);
                    var expertExp = ExpertEvent.GetExpertExp(soleId);
                    var expertNeedExp = ExpertEvent.GetExpertNeedExp(expertLvl + 1, propsGrade, propsLevel);

                    uiArtifactInfo_textExpertLvl = MonoBehaviour.Instantiate(uiArtifactInfo.textGrade_En, uiArtifactInfo.transform, false);
                    uiArtifactInfo_textExpertLvl.transform.position = new Vector3(uiArtifactInfo.textGrade_En.transform.position.x, uiArtifactInfo.textGrade_En.transform.position.y - 0.3f);
                    uiArtifactInfo_textExpertLvl.verticalOverflow = VerticalWrapMode.Overflow;
                    uiArtifactInfo_textExpertLvl.horizontalOverflow = HorizontalWrapMode.Overflow;
                    uiArtifactInfo_textExpertLvl.alignment = TextAnchor.MiddleLeft;
                    uiArtifactInfo_textExpertLvl.fontStyle = FontStyle.Bold;
                    uiArtifactInfo_textExpertLvl.fontSize = 16;
                    uiArtifactInfo_textExpertLvl.color = Color.white;
                    uiArtifactInfo_textExpertLvl.text = $"Expert Level: {expertLvl} ({expertExp}/{expertNeedExp})";

                    uiArtifactInfo_textAdjAtk = MonoBehaviour.Instantiate(uiArtifactInfo.textGrade_En, uiArtifactInfo.transform, false);
                    uiArtifactInfo_textAdjAtk.transform.position = new Vector3(uiArtifactInfo.textGrade_En.transform.position.x + 0.2f, uiArtifactInfo.textGrade_En.transform.position.y - 0.5f);
                    uiArtifactInfo_textAdjAtk.verticalOverflow = VerticalWrapMode.Overflow;
                    uiArtifactInfo_textAdjAtk.horizontalOverflow = HorizontalWrapMode.Overflow;
                    uiArtifactInfo_textAdjAtk.alignment = TextAnchor.MiddleLeft;
                    uiArtifactInfo_textAdjAtk.fontSize = 15;
                    uiArtifactInfo_textAdjAtk.color = Color.white;
                    uiArtifactInfo_textAdjAtk.text = $"+Atk: {ExpertEvent.GetArtifactExpertAtk(artifactConf.atk, expertLvl, propsGrade, propsLevel)}";

                    uiArtifactInfo_textAdjDef = MonoBehaviour.Instantiate(uiArtifactInfo.textGrade_En, uiArtifactInfo.transform, false);
                    uiArtifactInfo_textAdjDef.transform.position = new Vector3(uiArtifactInfo.textGrade_En.transform.position.x + 0.2f, uiArtifactInfo.textGrade_En.transform.position.y - 0.7f);
                    uiArtifactInfo_textAdjDef.verticalOverflow = VerticalWrapMode.Overflow;
                    uiArtifactInfo_textAdjDef.horizontalOverflow = HorizontalWrapMode.Overflow;
                    uiArtifactInfo_textAdjDef.alignment = TextAnchor.MiddleLeft;
                    uiArtifactInfo_textAdjDef.fontSize = 15;
                    uiArtifactInfo_textAdjDef.color = Color.white;
                    uiArtifactInfo_textAdjDef.text = $"+Def: {ExpertEvent.GetArtifactExpertDef(artifactConf.def, expertLvl, propsGrade, propsLevel)}";

                    uiArtifactInfo_mod = true;
                }
            }

            var uiMartialInfo = MonoBehaviour.FindObjectOfType<UIMartialInfo>();
            if (uiMartialInfo != null && !GameHelper.IsInBattlle())
            {
                if (!uiMartialInfo_mod)
                {
                    var player = g.world.playerUnit;
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
                    uiMartialExpertInfo_textExpertLvl.text = $"Expert Level: {expertLvl} ({expertExp}/{expertNeedExp})";

                    if (mType == MartialType.Ability)
                    {
                        uiMartialExpertInfo_textAdj1.text = $"+Atk: {ExpertEvent.GetAbilityExpertAtk(player.GetDynProperty(UnitDynPropertyEnum.Attack).baseValue, expertLvl, propsGrade, propsLevel)}";
                        uiMartialExpertInfo_textAdj2.text = $"+Def: {ExpertEvent.GetAbilityExpertDef(player.GetDynProperty(UnitDynPropertyEnum.Defense).baseValue, expertLvl, propsGrade, propsLevel)}";
                        uiMartialExpertInfo_textAdj3.text = $"+Hp: {ExpertEvent.GetAbilityExpertHp(player.GetDynProperty(UnitDynPropertyEnum.HpMax).baseValue, expertLvl, propsGrade, propsLevel)}";
                        uiMartialExpertInfo_textAdj4.text = $"+Mp: {ExpertEvent.GetAbilityExpertMp(player.GetDynProperty(UnitDynPropertyEnum.MpMax).baseValue, expertLvl, propsGrade, propsLevel)}";
                        uiMartialExpertInfo_textAdj5.text = $"+Sp: {ExpertEvent.GetAbilityExpertSp(player.GetDynProperty(UnitDynPropertyEnum.SpMax).baseValue, expertLvl, propsGrade, propsLevel)}";
                    }
                    else if (mType == MartialType.Step)
                    {
                        uiMartialExpertInfo_textAdj1.text = $"+Speed: {ExpertEvent.GetStepExpertSpeed(expertLvl, propsGrade, propsLevel)} (In Battle)";
                        uiMartialExpertInfo_textAdj2.text = $"+Evade: {ExpertEvent.GetStepExpertEvade(expertLvl, propsGrade, propsLevel)}% (In Battle)";
                        uiMartialExpertInfo_textAdj3.gameObject.SetActive(false);
                        uiMartialExpertInfo_textAdj4.gameObject.SetActive(false);
                        uiMartialExpertInfo_textAdj5.gameObject.SetActive(false);
                    }
                    else
                    {
                        uiMartialExpertInfo_textAdj1.text = $"+Dmg: {ExpertEvent.GetSkillExpertAtk(player.GetDynProperty(UnitDynPropertyEnum.Attack).baseValue, expertLvl, propsGrade, propsLevel, mType)} (In Battle)";
                        uiMartialExpertInfo_textAdj2.text = $"+MpCost: {ExpertEvent.GetSkillExpertMpCost(SkillHelper.GetSkillMpCost(uiMartialInfo.martialData), expertLvl, propsGrade, propsLevel)} (In Battle)";
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

                    uiMartialInfo_mod = true;
                }
                else
                {
                    uiMartialExpertInfo.transform.position = new Vector3(0f, 5f);
                }
            }

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
                    //string forceHideConfig;
                    //Configs.ForceHideConditions.TryGetValue($"{e.uiType.uiName}.{buttonConfig.Key}", out forceHideConfig);

                    var comp = ui.GetComponentsInChildren<MonoBehaviour>().Where(x => buttonConfig.Key == x.name);
                    foreach (var c in comp)
                    {
                        c.gameObject.SetActive(buttonConfig.Value == SelectOption.Show);
                        //c.gameObject.SetActive(buttonConfig.Value == SelectOption.Show && !IsForeHideConfigOK(forceHideConfig));
                    }
                }
            }
        }

        //public static void OnUIUpdate()
        //{
        //    if (GameHelper.IsInBattlle())
        //    {
        //    }
        //    else
        //    {
        //    }
        //}

        public static void OnUIClose(CloseUIEnd e)
        {
            if (e.uiType.uiName == UIType.ArtifactInfo.uiName && uiArtifactInfo_mod)
            {
                uiArtifactInfo_mod = false;
            }
            else if (e.uiType.uiName == UIType.MartialInfo.uiName && uiMartialInfo_mod)
            {
                g.ui.CloseUI(uiMartialExpertInfo);
                uiMartialInfo_mod = false;
            }
        }

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);

            OnUIOpen(e);
        }

        public override void OnCloseUIEnd(CloseUIEnd e)
        {
            base.OnCloseUIEnd(e);

            OnUIClose(e);
        }

        //public override void OnTimeUpdate500ms()
        //{
        //    base.OnTimeUpdate500ms();

        //    OnUIUpdate();
        //}
    }
}
