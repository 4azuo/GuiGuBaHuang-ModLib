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
        private static UICheckPopup uiMartialExpertInfo;
        private static Text uiMartialExpertInfo_textExpertLvl;
        private static Text uiMartialExpertInfo_textAdjAtk;
        private static Text uiMartialExpertInfo_textAdjDef;
        private static Text uiMartialExpertInfo_textAdjHp;
        private static Text uiMartialExpertInfo_textAdjMp;
        private static Text uiMartialExpertInfo_textAdjSp;
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
            if (uiMartialInfo != null)
            {
                if (!uiMartialInfo_mod)
                {
                    var player = g.world.playerUnit;
                    var soleId = uiMartialInfo.martialData.martialInfo.propsData.soleID;
                    var propsGrade = uiMartialInfo.martialData.martialInfo.grade;
                    var propsLevel = uiMartialInfo.martialData.martialInfo.level;
                    var expertLvl = ExpertEvent.GetExpertLvl(soleId, propsGrade, propsLevel);
                    var expertExp = ExpertEvent.GetExpertExp(soleId);
                    var expertNeedExp = ExpertEvent.GetExpertNeedExp(expertLvl + 1, propsGrade, propsLevel);
                    
                    uiMartialExpertInfo = g.ui.OpenUI<UICheckPopup>(UIType.CheckPopup);
                    uiMartialExpertInfo.btn1.gameObject.SetActive(false);
                    uiMartialExpertInfo.btn2.gameObject.SetActive(false);
                    uiMartialExpertInfo.btn3.gameObject.SetActive(false);
                    uiMartialExpertInfo.textBtn1.gameObject.SetActive(false);
                    uiMartialExpertInfo.textBtn2.gameObject.SetActive(false);
                    uiMartialExpertInfo.textBtn3.gameObject.SetActive(false);
                    uiMartialExpertInfo.textTitle.gameObject.SetActive(false);
                    uiMartialExpertInfo.textContent.gameObject.SetActive(false);

                    uiMartialExpertInfo_textExpertLvl = MonoBehaviour.Instantiate(uiMartialExpertInfo.textContent, uiMartialExpertInfo.transform, false);
                    uiMartialExpertInfo_textExpertLvl.transform.position = new Vector3(uiMartialExpertInfo.textContent.transform.position.x, uiMartialExpertInfo.textContent.transform.position.y);
                    uiMartialExpertInfo_textExpertLvl.verticalOverflow = VerticalWrapMode.Overflow;
                    uiMartialExpertInfo_textExpertLvl.horizontalOverflow = HorizontalWrapMode.Overflow;
                    uiMartialExpertInfo_textExpertLvl.alignment = TextAnchor.MiddleLeft;
                    uiMartialExpertInfo_textExpertLvl.fontStyle = FontStyle.Bold;
                    uiMartialExpertInfo_textExpertLvl.fontSize = 16;
                    uiMartialExpertInfo_textExpertLvl.color = Color.white;
                    uiMartialExpertInfo_textExpertLvl.text = $"Expert Level: {expertLvl} ({expertExp}/{expertNeedExp})";

                    uiMartialExpertInfo_textAdjAtk = MonoBehaviour.Instantiate(uiMartialExpertInfo.textContent, uiMartialExpertInfo.transform, false);
                    uiMartialExpertInfo_textAdjAtk.transform.position = new Vector3(uiMartialExpertInfo.textContent.transform.position.x, uiMartialExpertInfo.textContent.transform.position.y);
                    uiMartialExpertInfo_textAdjAtk.verticalOverflow = VerticalWrapMode.Overflow;
                    uiMartialExpertInfo_textAdjAtk.horizontalOverflow = HorizontalWrapMode.Overflow;
                    uiMartialExpertInfo_textAdjAtk.alignment = TextAnchor.MiddleLeft;
                    uiMartialExpertInfo_textAdjAtk.fontSize = 15;
                    uiMartialExpertInfo_textAdjAtk.color = Color.white;
                    if (uiMartialInfo.martialData.martialType == MartialType.Ability)
                        uiMartialExpertInfo_textAdjAtk.text = $"+Atk: {ExpertEvent.GetAbilityExpertAtk(player.GetDynProperty(UnitDynPropertyEnum.Attack).value, expertLvl, propsGrade, propsLevel)}";
                    else
                        uiMartialExpertInfo_textAdjAtk.text = $"+Atk: {ExpertEvent.GetSkillExpertAtk(player.GetDynProperty(UnitDynPropertyEnum.Attack).value, expertLvl, propsGrade, propsLevel)}";

                    if (uiMartialInfo.martialData.martialType == MartialType.Ability)
                    {
                        uiMartialExpertInfo_textAdjDef = MonoBehaviour.Instantiate(uiMartialExpertInfo.textContent, uiMartialExpertInfo.transform, false);
                        uiMartialExpertInfo_textAdjDef.transform.position = new Vector3(uiMartialExpertInfo.textContent.transform.position.x, uiMartialExpertInfo.textContent.transform.position.y);
                        uiMartialExpertInfo_textAdjDef.verticalOverflow = VerticalWrapMode.Overflow;
                        uiMartialExpertInfo_textAdjDef.horizontalOverflow = HorizontalWrapMode.Overflow;
                        uiMartialExpertInfo_textAdjDef.alignment = TextAnchor.MiddleLeft;
                        uiMartialExpertInfo_textAdjDef.fontSize = 15;
                        uiMartialExpertInfo_textAdjDef.color = Color.white;
                        uiMartialExpertInfo_textAdjDef.text = $"+Def: {ExpertEvent.GetAbilityExpertDef(player.GetDynProperty(UnitDynPropertyEnum.Defense).value, expertLvl, propsGrade, propsLevel)}";

                        uiMartialExpertInfo_textAdjHp = MonoBehaviour.Instantiate(uiMartialExpertInfo.textContent, uiMartialExpertInfo.transform, false);
                        uiMartialExpertInfo_textAdjHp.transform.position = new Vector3(uiMartialExpertInfo.textContent.transform.position.x, uiMartialExpertInfo.textContent.transform.position.y);
                        uiMartialExpertInfo_textAdjHp.verticalOverflow = VerticalWrapMode.Overflow;
                        uiMartialExpertInfo_textAdjHp.horizontalOverflow = HorizontalWrapMode.Overflow;
                        uiMartialExpertInfo_textAdjHp.alignment = TextAnchor.MiddleLeft;
                        uiMartialExpertInfo_textAdjHp.fontSize = 15;
                        uiMartialExpertInfo_textAdjHp.color = Color.white;
                        uiMartialExpertInfo_textAdjHp.text = $"+Hp: {ExpertEvent.GetAbilityExpertHp(player.GetDynProperty(UnitDynPropertyEnum.Hp).value, expertLvl, propsGrade, propsLevel)}";

                        uiMartialExpertInfo_textAdjMp = MonoBehaviour.Instantiate(uiMartialExpertInfo.textContent, uiMartialExpertInfo.transform, false);
                        uiMartialExpertInfo_textAdjMp.transform.position = new Vector3(uiMartialExpertInfo.textContent.transform.position.x, uiMartialExpertInfo.textContent.transform.position.y);
                        uiMartialExpertInfo_textAdjMp.verticalOverflow = VerticalWrapMode.Overflow;
                        uiMartialExpertInfo_textAdjMp.horizontalOverflow = HorizontalWrapMode.Overflow;
                        uiMartialExpertInfo_textAdjMp.alignment = TextAnchor.MiddleLeft;
                        uiMartialExpertInfo_textAdjMp.fontSize = 15;
                        uiMartialExpertInfo_textAdjMp.color = Color.white;
                        uiMartialExpertInfo_textAdjMp.text = $"+Mp: {ExpertEvent.GetAbilityExpertMp(player.GetDynProperty(UnitDynPropertyEnum.Mp).value, expertLvl, propsGrade, propsLevel)}";

                        uiMartialExpertInfo_textAdjSp = MonoBehaviour.Instantiate(uiMartialExpertInfo.textContent, uiMartialExpertInfo.transform, false);
                        uiMartialExpertInfo_textAdjSp.transform.position = new Vector3(uiMartialExpertInfo.textContent.transform.position.x, uiMartialExpertInfo.textContent.transform.position.y);
                        uiMartialExpertInfo_textAdjSp.verticalOverflow = VerticalWrapMode.Overflow;
                        uiMartialExpertInfo_textAdjSp.horizontalOverflow = HorizontalWrapMode.Overflow;
                        uiMartialExpertInfo_textAdjSp.alignment = TextAnchor.MiddleLeft;
                        uiMartialExpertInfo_textAdjSp.fontSize = 15;
                        uiMartialExpertInfo_textAdjSp.color = Color.white;
                        uiMartialExpertInfo_textAdjSp.text = $"+Sp: {ExpertEvent.GetAbilityExpertSp(player.GetDynProperty(UnitDynPropertyEnum.Sp).value, expertLvl, propsGrade, propsLevel)}";
                    }

                    uiMartialInfo_mod = true;
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
            if (e.uiType.uiName == UIType.ArtifactInfo.uiName)
            {
                uiArtifactInfo_mod = false;
            }
            else if (e.uiType.uiName == UIType.MartialInfo.uiName)
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
