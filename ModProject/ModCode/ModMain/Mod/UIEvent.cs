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
        private static bool uiMartialInfo_mod = false;
        private static Text uiMartialInfo_textExpertLvl;
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
                    var expertLvl = ExpertEvent.GetExpertLvl(uiArtifactInfo.unit, uiArtifactInfo.shapeProp.soleID, uiArtifactInfo.shapeProp.propsInfoBase.grade, uiArtifactInfo.shapeProp.propsInfoBase.level);
                    var expertExp = ExpertEvent.GetExpertExp(uiArtifactInfo.unit, uiArtifactInfo.shapeProp.soleID);
                    var expertNeedExp = ExpertEvent.GetSkillExpertNeedExp(expertLvl + 1, uiArtifactInfo.shapeProp.propsInfoBase.grade, uiArtifactInfo.shapeProp.propsInfoBase.level);

                    uiArtifactInfo_textExpertLvl = MonoBehaviour.Instantiate(uiArtifactInfo.textGrade_En, uiArtifactInfo.transform, false);
                    uiArtifactInfo_textExpertLvl.transform.position = new Vector3(uiArtifactInfo.textGrade_En.transform.position.x, uiArtifactInfo.textGrade_En.transform.position.y - 0.3f);
                    uiArtifactInfo_textExpertLvl.verticalOverflow = VerticalWrapMode.Overflow;
                    uiArtifactInfo_textExpertLvl.horizontalOverflow = HorizontalWrapMode.Overflow;
                    uiArtifactInfo_textExpertLvl.alignment = TextAnchor.MiddleLeft;
                    uiArtifactInfo_textExpertLvl.fontSize = 15;
                    uiArtifactInfo_textExpertLvl.color = Color.red;
                    uiArtifactInfo_textExpertLvl.text = $"Expert Level: {expertLvl} ({expertExp}/{expertNeedExp})";

                    uiArtifactInfo_mod = true;
                }
            }

            var uiMartialInfo = MonoBehaviour.FindObjectOfType<UIMartialInfo>();
            if (uiMartialInfo != null)
            {
                if (!uiMartialInfo_mod)
                {
                    var expertLvl = ExpertEvent.GetExpertLvl(uiMartialInfo.unit, uiMartialInfo.martialData.martialInfo.propsData.soleID, uiMartialInfo.martialData.martialInfo.grade, uiMartialInfo.martialData.martialInfo.level);
                    var expertExp = ExpertEvent.GetExpertExp(uiMartialInfo.unit, uiMartialInfo.martialData.martialInfo.propsData.soleID);
                    var expertNeedExp = ExpertEvent.GetSkillExpertNeedExp(expertLvl + 1, uiMartialInfo.martialData.martialInfo.grade, uiMartialInfo.martialData.martialInfo.level);

                    uiMartialInfo_textExpertLvl = MonoBehaviour.Instantiate(uiMartialInfo.textGrade_En, uiMartialInfo.transform, false);
                    uiMartialInfo_textExpertLvl.transform.position = new Vector3(uiMartialInfo.textGrade_En.transform.position.x + 2.0f, uiMartialInfo.textGrade_En.transform.position.y);
                    uiMartialInfo_textExpertLvl.verticalOverflow = VerticalWrapMode.Overflow;
                    uiMartialInfo_textExpertLvl.horizontalOverflow = HorizontalWrapMode.Overflow;
                    uiMartialInfo_textExpertLvl.alignment = TextAnchor.MiddleLeft;
                    uiMartialInfo_textExpertLvl.fontSize = 15;
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
