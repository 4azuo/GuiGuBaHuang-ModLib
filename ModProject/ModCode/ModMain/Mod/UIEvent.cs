using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using EGameTypeData;
using static MOD_nE7UL2.Object.InGameStts._HideButtonConfigs;
using static MOD_nE7UL2.Object.InGameStts;
using System.Collections.Generic;

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
        private static Text uiMartialInfo_textExpertLvl;
        private static Text uiMartialInfo_textAdjAtk;
        private static Text uiMartialInfo_textAdjDef;
        private static Text uiMartialInfo_textAdjHp;
        private static Text uiMartialInfo_textAdjMp;
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
                    var soleId = uiArtifactInfo.shapeProp.soleID;
                    var propsGrade = uiArtifactInfo.shapeProp.propsInfoBase.grade;
                    var propsLevel = uiArtifactInfo.shapeProp.propsInfoBase.level;
                    var expertLvl = ExpertEvent.GetExpertLvl(uiArtifactInfo.unit, soleId, propsGrade, propsLevel);
                    var expertExp = ExpertEvent.GetExpertExp(uiArtifactInfo.unit, soleId);
                    var expertNeedExp = ExpertEvent.GetSkillExpertNeedExp(expertLvl + 1, propsGrade, propsLevel);

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
                    uiArtifactInfo_textAdjAtk.text = $"+Atk: {ExpertEvent.GetArtifactExpertAtkRate(expertLvl, propsGrade, propsLevel) * 100f:0.0}%";

                    uiArtifactInfo_textAdjDef = MonoBehaviour.Instantiate(uiArtifactInfo.textGrade_En, uiArtifactInfo.transform, false);
                    uiArtifactInfo_textAdjDef.transform.position = new Vector3(uiArtifactInfo.textGrade_En.transform.position.x + 0.2f, uiArtifactInfo.textGrade_En.transform.position.y - 0.7f);
                    uiArtifactInfo_textAdjDef.verticalOverflow = VerticalWrapMode.Overflow;
                    uiArtifactInfo_textAdjDef.horizontalOverflow = HorizontalWrapMode.Overflow;
                    uiArtifactInfo_textAdjDef.alignment = TextAnchor.MiddleLeft;
                    uiArtifactInfo_textAdjDef.fontSize = 15;
                    uiArtifactInfo_textAdjDef.color = Color.white;
                    uiArtifactInfo_textAdjDef.text = $"+Def: {ExpertEvent.GetArtifactExpertDefRate(expertLvl, propsGrade, propsLevel) * 100f:0.0}%";

                    uiArtifactInfo_mod = true;
                }
            }

            var uiMartialInfo = MonoBehaviour.FindObjectOfType<UIMartialInfo>();
            if (uiMartialInfo != null)
            {
                if (!uiMartialInfo_mod)
                {
                    var soleId = uiMartialInfo.martialData.martialInfo.propsData.soleID;
                    var propsGrade = uiMartialInfo.martialData.martialInfo.grade;
                    var propsLevel = uiMartialInfo.martialData.martialInfo.level;
                    var expertLvl = ExpertEvent.GetExpertLvl(uiMartialInfo.unit, soleId, propsGrade, propsLevel);
                    var expertExp = ExpertEvent.GetExpertExp(uiMartialInfo.unit, soleId);
                    var expertNeedExp = ExpertEvent.GetSkillExpertNeedExp(expertLvl + 1, propsGrade, propsLevel);

                    uiMartialInfo_textExpertLvl = MonoBehaviour.Instantiate(uiMartialInfo.textGrade_En, uiMartialInfo.transform, false);
                    uiMartialInfo_textExpertLvl.transform.position = new Vector3(uiMartialInfo.textGrade_En.transform.position.x + 2.0f, uiMartialInfo.textGrade_En.transform.position.y);
                    uiMartialInfo_textExpertLvl.verticalOverflow = VerticalWrapMode.Overflow;
                    uiMartialInfo_textExpertLvl.horizontalOverflow = HorizontalWrapMode.Overflow;
                    uiMartialInfo_textExpertLvl.alignment = TextAnchor.MiddleLeft;
                    uiArtifactInfo_textExpertLvl.fontStyle = FontStyle.Bold;
                    uiArtifactInfo_textExpertLvl.fontSize = 16;
                    uiMartialInfo_textExpertLvl.color = Color.red;
                    uiMartialInfo_textExpertLvl.text = $"Expert Level: {expertLvl} ({expertExp}/{expertNeedExp})";

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

        public static void OnUIUpdate()
        {
            if (GameHelper.IsInBattlle())
            {
            }
            else
            {
            }
        }

        public static void OnUIClose(CloseUIEnd e)
        {
            if (e.uiType.uiName == UIType.ArtifactInfo.uiName)
                uiArtifactInfo_mod = false;
            else if (e.uiType.uiName == UIType.MartialInfo.uiName)
                uiMartialInfo_mod = false;
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

        public override void OnTimeUpdate500ms()
        {
            base.OnTimeUpdate500ms();

            OnUIUpdate();
        }
    }
}
