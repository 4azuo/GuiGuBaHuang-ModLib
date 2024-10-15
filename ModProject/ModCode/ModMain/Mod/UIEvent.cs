using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using ModLib.Enum;
using System;
using EGameTypeData;
using static MOD_nE7UL2.Object.InGameStts._HideButtonConfigs;
using static MOD_nE7UL2.Object.InGameStts;
using System.Collections.Generic;
using EBattleTypeData;
using static Il2CppSystem.Uri;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.UI_EVENT)]
    public class UIEvent : ModEvent
    {
        public override int OrderIndex => 6100;

        public const int ADJUSTED_VALUE_ORG_ATK = 0;
        public const int ADJUSTED_VALUE_ORG_DEF = 1;
        public const int ADJUSTED_VALUE_ADJ_ATK = 2;
        public const int ADJUSTED_VALUE_ADJ_DEF = 3;
        public const int ADJUSTED_VALUE_LST_ATK = 4;
        public const int ADJUSTED_VALUE_LST_DEF = 5;
        public const int ADJUSTED_VALUE_HP = 6;
        public const int ADJUSTED_VALUE_MP = 7;
        public const int ADJUSTED_VALUE_SP = 8;
        public const int ADJUSTED_VALUE_MAX_HP = 9;
        public const int ADJUSTED_VALUE_MAX_MP = 10;
        public const int ADJUSTED_VALUE_MAX_SP = 11;
        public const int ADJUSTED_VALUE_ADJ_HP = 12;
        public const int ADJUSTED_VALUE_ADJ_MP = 13;
        public const int ADJUSTED_VALUE_ADJ_SP = 14;
        public const int ADJUSTED_VALUE_LST_HP = 15;
        public const int ADJUSTED_VALUE_LST_MP = 16;
        public const int ADJUSTED_VALUE_LST_SP = 17;
        public const int ADJUSTED_VALUE_PER_HP = 18;
        public const int ADJUSTED_VALUE_PER_MP = 19;
        public const int ADJUSTED_VALUE_PER_SP = 20;
        public const int ADJUSTED_VALUE_CUR_HP = 21;
        public const int ADJUSTED_VALUE_CUR_MP = 22;
        public const int ADJUSTED_VALUE_CUR_SP = 23;

        public static _HideButtonConfigs Configs => ModMain.ModObj.InGameCustomSettings.HideButtonConfigs;

        public static int[] GetAdjustValues(WorldUnitBase wunit)
        {
            var rs = new int[]
            {
                /*00. origin atk*/  0,
                /*01. origin def*/  0,
                /*02. adjust atk*/  0,
                /*03. adjust def*/  0,
                /*04. last atk*/    0,
                /*05. last def*/    0,
                /*06. hp*/          0,
                /*07. mp*/          0,
                /*08. sp*/          0,
                /*09. hpMax*/       0,
                /*10. mpMax*/       0,
                /*11. spMax*/       0,
                /*12. hpMaxAdj*/    0,
                /*13. mpMaxAdj*/    0,
                /*14. spMaxAdj*/    0,
                /*15. hpMaxLst*/    0,
                /*16. mpMaxLst*/    0,
                /*17. spMaxLst*/    0,
                /*18. hpPercent*/   0,
                /*19. mpPercent*/   0,
                /*20. spPercent*/   0,
                /*21. hpCur*/       0,
                /*22. mpCur*/       0,
                /*23. spCur*/       0,
            };

            if (wunit == null)
                return rs;

            //origin
            var atk = wunit.GetDynProperty(UnitDynPropertyEnum.Attack).value;
            rs[ADJUSTED_VALUE_ORG_ATK] = atk;
            var def = wunit.GetDynProperty(UnitDynPropertyEnum.Defense).value;
            rs[ADJUSTED_VALUE_ORG_DEF] = def;
            var hp = wunit.GetDynProperty(UnitDynPropertyEnum.Hp).value;
            rs[ADJUSTED_VALUE_HP] = hp;
            var mp = wunit.GetDynProperty(UnitDynPropertyEnum.Mp).value;
            rs[ADJUSTED_VALUE_MP] = mp;
            var sp = wunit.GetDynProperty(UnitDynPropertyEnum.Sp).value;
            rs[ADJUSTED_VALUE_SP] = sp;
            var hpMax = wunit.GetDynProperty(UnitDynPropertyEnum.HpMax).value;
            rs[ADJUSTED_VALUE_MAX_HP] = hpMax;
            var mpMax = wunit.GetDynProperty(UnitDynPropertyEnum.MpMax).value;
            rs[ADJUSTED_VALUE_MAX_MP] = mpMax;
            var spMax = wunit.GetDynProperty(UnitDynPropertyEnum.SpMax).value;
            rs[ADJUSTED_VALUE_MAX_SP] = spMax;

            //adjust
            var artifacts = wunit.data.unitData.propData.GetEquipProps().ToArray().Where(x => x?.propsItem?.IsArtifact() != null).ToArray();
            foreach (var artifact in artifacts)
            {
                var a = artifact.To<DataProps.PropsArtifact>();
                if (a.durable > 0)
                {
                    var aconf = artifact.propsItem.IsArtifact();
                    var r = 0.01f + (0.001f * Math.Pow(2, a.level)) + (0.02f * a.grade);

                    var r1 = (4.00f + (0.006f * Math.Pow(3, a.level)) + (1.00f * a.grade)) / 100.0f;
                    rs[ADJUSTED_VALUE_ADJ_ATK] += (r * atk + r1 * aconf.atk).Parse<int>();

                    var r2 = (3.00f + (0.005f * Math.Pow(3, a.level)) + (0.80f * a.grade)) / 100.0f;
                    rs[ADJUSTED_VALUE_ADJ_DEF] += (r * def + r2 * aconf.def).Parse<int>();

                    var r3 = 0.01f + 0.01f * a.level + 0.03f * a.grade;
                    rs[ADJUSTED_VALUE_ADJ_HP] += (r3 * hpMax + a.level * a.grade * aconf.hp / 10).Parse<int>();
                }
            }

            //last
            var atkLast = atk + rs[ADJUSTED_VALUE_ADJ_ATK];
            rs[ADJUSTED_VALUE_LST_ATK] = atkLast;
            var defLast = def + rs[ADJUSTED_VALUE_ADJ_DEF];
            rs[ADJUSTED_VALUE_LST_DEF] = defLast;
            var hpLast = hpMax + rs[ADJUSTED_VALUE_ADJ_HP];
            rs[ADJUSTED_VALUE_LST_HP] = hpLast;
            var mpLast = mpMax + rs[ADJUSTED_VALUE_ADJ_MP];
            rs[ADJUSTED_VALUE_LST_MP] = mpLast;
            var spLast = spMax + rs[ADJUSTED_VALUE_ADJ_SP];
            rs[ADJUSTED_VALUE_LST_SP] = spLast;

            //percent
            var hpPercent = hp.Parse<float>() / hpMax.Parse<float>();
            rs[ADJUSTED_VALUE_PER_HP] = (hpPercent * 1000.0f).Parse<int>();
            var mpPercent = mp.Parse<float>() / mpMax.Parse<float>();
            rs[ADJUSTED_VALUE_PER_MP] = (mpPercent * 1000.0f).Parse<int>();
            var spPercent = sp.Parse<float>() / spMax.Parse<float>();
            rs[ADJUSTED_VALUE_PER_SP] = (spPercent * 1000.0f).Parse<int>();

            rs[ADJUSTED_VALUE_CUR_HP] = (hpPercent * rs[ADJUSTED_VALUE_LST_HP]).Parse<int>();
            rs[ADJUSTED_VALUE_CUR_MP] = (mpPercent * rs[ADJUSTED_VALUE_LST_MP]).Parse<int>();
            rs[ADJUSTED_VALUE_CUR_SP] = (spPercent * rs[ADJUSTED_VALUE_LST_SP]).Parse<int>();

            return rs;
        }

        public static T ReplaceObject<T>(T obj) where T : MonoBehaviour
        {
            var newObj = MonoBehaviour.Instantiate(obj, obj.transform, false);
            newObj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y);
            obj.enabled = false;
            return newObj;
        }

        //public static bool IsForeHideConfigOK(string conf)
        //{
        //    if (string.IsNullOrEmpty(conf))
        //        return false;
        //    conf = conf.Replace("${gradelevel}", g.world.playerUnit.GetGradeLvl().ToString())
        //        .Replace("${gamelevel}", g.data.dataWorld.data.gameLevel.Parse<int>().ToString());
        //    return Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScript.EvaluateAsync<bool>(conf).Result;
        //}

        private static bool uiMapMain_mod = false;
        private static Text uiMapMain_playerInfo_textHPValue;
        private static Text uiMapMain_playerInfo_textMPValue;
        private static Text uiMapMain_playerInfo_textSPValue;
        private static bool uiInfo1_mod = false;
        private static Text uiInfo1_textATKValue;
        private static Text uiInfo1_textDEFValue;
        private static Text uiInfo1_textHPValue;
        private static Text uiInfo1_textMPValue;
        private static Text uiInfo1_textSPValue;
        private static bool uiInfo2_mod = false;
        private static Text uiInfo2_textATKValue;
        private static Text uiInfo2_textDEFValue;
        private static Text uiInfo2_textHPValue;
        private static Text uiInfo2_textMPValue;
        private static Text uiInfo2_textSPValue;
        private static bool uiMapBattlePre_mod = false;
        private static Text uiMapBattlePre_textLeftHPValue;
        private static Text uiMapBattlePre_textLeftMPValue;
        private static Text uiMapBattlePre_textLeftSPValue;
        private static Text uiMapBattlePre_textRightHPValue;
        private static Text uiMapBattlePre_textRightMPValue;
        private static Text uiMapBattlePre_textRightSPValue;
        private static bool uiBattleInfo_mod = false;
        private static Text uiBattleInfo_textSHIELDValue;
        public static void OnUIOpen(OpenUIEnd e)
        {
            DebugHelper.WriteLine(e?.uiType?.uiName);

            var uiMapMain = MonoBehaviour.FindObjectOfType<UIMapMain>();
            if (uiMapMain != null)
            {
                if (!uiMapMain_mod)
                {
                    uiMapMain_playerInfo_textHPValue = ReplaceObject(uiMapMain.playerInfo.textHPValue);
                    uiMapMain_playerInfo_textMPValue = ReplaceObject(uiMapMain.playerInfo.textMPValue);
                    uiMapMain_playerInfo_textSPValue = ReplaceObject(uiMapMain.playerInfo.textSPValue);
                    uiMapMain_mod = true;
                }

                uiMapMain.playerInfo.textPiscesPendantCount.gameObject.SetActive(false);
                uiMapMain.playerInfo.goAddLuckRoot.SetActive(false);
            }

            var uiInfo1 = MonoBehaviour.FindObjectOfType<UINPCInfo>();
            if (uiInfo1 != null)
            {
                if (!uiInfo1_mod)
                {
                    var texts4 = uiInfo1.uiProperty.goItem4.GetComponentsInChildren<Text>();
                    uiInfo1_textATKValue = ReplaceObject(texts4[1]);
                    uiInfo1_textDEFValue = ReplaceObject(texts4[4]);
                    var texts2 = uiInfo1.uiProperty.goItem2.GetComponentsInChildren<Text>();
                    uiInfo1_textHPValue = ReplaceObject(texts2[1]);
                    uiInfo1_textHPValue.horizontalOverflow = HorizontalWrapMode.Overflow;
                    uiInfo1_textMPValue = ReplaceObject(texts2[4]);
                    uiInfo1_textMPValue.horizontalOverflow = HorizontalWrapMode.Overflow;
                    uiInfo1_textSPValue = ReplaceObject(texts2[7]);
                    uiInfo1_textSPValue.horizontalOverflow = HorizontalWrapMode.Overflow;
                    uiInfo1_mod = true;
                }
            }

            var uiInfo2 = MonoBehaviour.FindObjectOfType<UIPlayerInfo>();
            if (uiInfo2 != null)
            {
                if (!uiInfo2_mod)
                {
                    var texts4 = uiInfo2.uiPropertyCommon.goItem4_En.GetComponentsInChildren<Text>();
                    uiInfo2_textATKValue = ReplaceObject(texts4[1]);
                    uiInfo2_textDEFValue = ReplaceObject(texts4[4]);
                    var texts2 = uiInfo2.uiPropertyCommon.goItem2_En.GetComponentsInChildren<Text>();
                    uiInfo2_textHPValue = ReplaceObject(texts2[1]);
                    uiInfo2_textHPValue.horizontalOverflow = HorizontalWrapMode.Overflow;
                    uiInfo2_textMPValue = ReplaceObject(texts2[4]);
                    uiInfo2_textMPValue.horizontalOverflow = HorizontalWrapMode.Overflow;
                    uiInfo2_textSPValue = ReplaceObject(texts2[7]);
                    uiInfo2_textSPValue.horizontalOverflow = HorizontalWrapMode.Overflow;
                    uiInfo2_mod = true;
                }
            }

            var uiMapBattlePre = MonoBehaviour.FindObjectOfType<UIMapBattlePre>();
            if (uiMapBattlePre != null)
            {
                if (!uiMapBattlePre_mod)
                {
                    uiMapBattlePre_textLeftHPValue = ReplaceObject(uiMapBattlePre.textLeftHPValue);
                    uiMapBattlePre_textLeftMPValue = ReplaceObject(uiMapBattlePre.textLeftMPValue);
                    uiMapBattlePre_textLeftSPValue = ReplaceObject(uiMapBattlePre.textLeftSPValue);
                    uiMapBattlePre_textRightHPValue = ReplaceObject(uiMapBattlePre.textRightHPValue);
                    uiMapBattlePre_textRightMPValue = ReplaceObject(uiMapBattlePre.textRightMPValue);
                    uiMapBattlePre_textRightSPValue = ReplaceObject(uiMapBattlePre.textRightSPValue);
                    uiMapBattlePre_mod = true;
                }
            }

            var uiBattleInfo = MonoBehaviour.FindObjectOfType<UIBattleInfo>();
            if (uiBattleInfo != null)
            {
                if (!uiBattleInfo_mod)
                {
                    var textHP = uiBattleInfo.uiInfo.textHP;
                    uiBattleInfo_textSHIELDValue = ReplaceObject(textHP);
                    uiBattleInfo_textSHIELDValue.transform.position = new Vector3(textHP.transform.position.x, textHP.transform.position.y + 0.2f);
                    uiBattleInfo_mod = true;
                }

                uiBattleInfo.uiInfo.goMonstCount1.SetActive(false);
                uiBattleInfo.uiInfo.goMonstCount2.SetActive(false);
                uiBattleInfo.uiMap.goGroupRoot.SetActive(false);
            }

            var uiPlayerAddHMS = MonoBehaviour.FindObjectOfType<UIPlayerAddHMS>();
            if (uiPlayerAddHMS != null)
            {
                var editedValues = GetAdjustValues(g.world.playerUnit);
                if (uiPlayerAddHMS.textName.text == "Vitality")
                {
                    uiPlayerAddHMS.barItem.value = editedValues[ADJUSTED_VALUE_CUR_HP];
                    uiPlayerAddHMS.barItem.size = editedValues[ADJUSTED_VALUE_LST_HP];
                }
                else if (uiPlayerAddHMS.textName.text == "Energy")
                {
                    uiPlayerAddHMS.barItem.value = editedValues[ADJUSTED_VALUE_CUR_MP];
                    uiPlayerAddHMS.barItem.size = editedValues[ADJUSTED_VALUE_LST_MP];
                }
                else if (uiPlayerAddHMS.textName.text == "Focus")
                {
                    uiPlayerAddHMS.barItem.value = editedValues[ADJUSTED_VALUE_CUR_SP];
                    uiPlayerAddHMS.barItem.size = editedValues[ADJUSTED_VALUE_LST_SP];
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
            var uiMapMain = MonoBehaviour.FindObjectOfType<UIMapMain>();
            if (uiMapMain != null && uiMapMain_mod)
            {
                var editedValues = GetAdjustValues(g.world.playerUnit);
                uiMapMain_playerInfo_textHPValue.text = $"{editedValues[ADJUSTED_VALUE_CUR_HP]}/{editedValues[ADJUSTED_VALUE_LST_HP]}";
                uiMapMain_playerInfo_textMPValue.text = $"{editedValues[ADJUSTED_VALUE_CUR_MP]}/{editedValues[ADJUSTED_VALUE_LST_MP]}";
                uiMapMain_playerInfo_textSPValue.text = $"{editedValues[ADJUSTED_VALUE_CUR_SP]}/{editedValues[ADJUSTED_VALUE_LST_SP]}";
            }

            var uiInfo1 = MonoBehaviour.FindObjectOfType<UINPCInfo>();
            if (uiInfo1 != null && uiInfo1_mod)
            {
                var editedValues = GetAdjustValues(uiInfo1.unit);
                uiInfo1_textATKValue.text = $"{editedValues[ADJUSTED_VALUE_LST_ATK]}";
                uiInfo1_textDEFValue.text = $"{editedValues[ADJUSTED_VALUE_LST_DEF]}";
                uiInfo1_textHPValue.text = $"{editedValues[ADJUSTED_VALUE_CUR_HP]}/{editedValues[ADJUSTED_VALUE_LST_HP]}";
                uiInfo1_textMPValue.text = $"{editedValues[ADJUSTED_VALUE_CUR_MP]}/{editedValues[ADJUSTED_VALUE_LST_MP]}";
                uiInfo1_textSPValue.text = $"{editedValues[ADJUSTED_VALUE_CUR_SP]}/{editedValues[ADJUSTED_VALUE_LST_SP]}";
            }

            var uiInfo2 = MonoBehaviour.FindObjectOfType<UIPlayerInfo>();
            if (uiInfo2 != null && uiInfo2_mod)
            {
                var editedValues = GetAdjustValues(uiInfo2.unit);
                uiInfo2_textATKValue.text = $"{editedValues[ADJUSTED_VALUE_LST_ATK]}";
                uiInfo2_textDEFValue.text = $"{editedValues[ADJUSTED_VALUE_LST_DEF]}";
                uiInfo2_textHPValue.text = $"{editedValues[ADJUSTED_VALUE_CUR_HP]}/{editedValues[ADJUSTED_VALUE_LST_HP]}";
                uiInfo2_textMPValue.text = $"{editedValues[ADJUSTED_VALUE_CUR_MP]}/{editedValues[ADJUSTED_VALUE_LST_MP]}";
                uiInfo2_textSPValue.text = $"{editedValues[ADJUSTED_VALUE_CUR_SP]}/{editedValues[ADJUSTED_VALUE_LST_SP]}";
            }

            var uiMapBattlePre = MonoBehaviour.FindObjectOfType<UIMapBattlePre>();
            if (uiMapBattlePre != null && uiMapBattlePre_mod)
            {
                var editedValuesLeft = GetAdjustValues(g.world.playerUnit);
                uiMapBattlePre_textLeftHPValue.text = $"{editedValuesLeft[ADJUSTED_VALUE_CUR_HP]}/{editedValuesLeft[ADJUSTED_VALUE_LST_HP]}";
                uiMapBattlePre_textLeftMPValue.text = $"{editedValuesLeft[ADJUSTED_VALUE_CUR_MP]}/{editedValuesLeft[ADJUSTED_VALUE_LST_MP]}";
                uiMapBattlePre_textLeftSPValue.text = $"{editedValuesLeft[ADJUSTED_VALUE_CUR_SP]}/{editedValuesLeft[ADJUSTED_VALUE_LST_SP]}";
                var editedValuesRight = GetAdjustValues(uiMapBattlePre.toUnit);
                uiMapBattlePre_textRightHPValue.text = $"{editedValuesRight[ADJUSTED_VALUE_CUR_HP]}/{editedValuesRight[ADJUSTED_VALUE_LST_HP]}";
                uiMapBattlePre_textRightMPValue.text = $"{editedValuesRight[ADJUSTED_VALUE_CUR_MP]}/{editedValuesRight[ADJUSTED_VALUE_LST_MP]}";
                uiMapBattlePre_textRightSPValue.text = $"{editedValuesRight[ADJUSTED_VALUE_CUR_SP]}/{editedValuesRight[ADJUSTED_VALUE_LST_SP]}";
            }

            var uiBattleInfo = MonoBehaviour.FindObjectOfType<UIBattleInfo>();
            if (uiBattleInfo != null && uiBattleInfo_mod)
            {
                if (ModBattleEvent.PlayerUnit.data.guardValue.value > 0)
                    uiBattleInfo_textSHIELDValue.text = $"{ModBattleEvent.PlayerUnit.data.guardValue.value}";
                else
                    uiBattleInfo_textSHIELDValue.text = string.Empty;
            }
        }

        public static void OnUIClose(CloseUIEnd e)
        {
            if (e.uiType.uiName == UIType.MapMain.uiName)
                uiMapMain_mod = false;
            else if (e.uiType.uiName == UIType.NPCInfo.uiName)
                uiInfo1_mod = false;
            else if (e.uiType.uiName == UIType.PlayerInfo.uiName)
                uiInfo2_mod = false;
            else if (e.uiType.uiName == UIType.MapBattlePre.uiName)
                uiMapBattlePre_mod = false;
            else if (e.uiType.uiName == UIType.BattleInfo.uiName)
                uiBattleInfo_mod = false;
        }

        public static void ConvertRealValues2AdjustValues(UnitCtrlBase e)
        {
            var data = e.data.TryCast<UnitDataHuman>();
            var wunit = data?.worldUnitData?.unit;
            var editedValues = GetAdjustValues(wunit);
            e.data.maxHP.baseValue += editedValues[ADJUSTED_VALUE_ADJ_HP];
            e.data.maxMP.baseValue += editedValues[ADJUSTED_VALUE_ADJ_MP];
            e.data.maxSP.baseValue += editedValues[ADJUSTED_VALUE_ADJ_SP];
            e.data.hp = editedValues[ADJUSTED_VALUE_CUR_HP];
            e.data.mp = editedValues[ADJUSTED_VALUE_CUR_MP];
            e.data.sp = editedValues[ADJUSTED_VALUE_CUR_SP];
        }

        public static void ConvertAdjustValues2RealValues(WorldUnitBase wunit)
        {
            var editedValues = GetAdjustValues(wunit);
            wunit.SetProperty<int>(UnitPropertyEnum.Hp, (editedValues[ADJUSTED_VALUE_PER_HP] / 1000.0f * editedValues[ADJUSTED_VALUE_MAX_HP]).Parse<int>());
            wunit.SetProperty<int>(UnitPropertyEnum.Mp, (editedValues[ADJUSTED_VALUE_PER_MP] / 1000.0f * editedValues[ADJUSTED_VALUE_MAX_MP]).Parse<int>());
            wunit.SetProperty<int>(UnitPropertyEnum.Sp, (editedValues[ADJUSTED_VALUE_PER_SP] / 1000.0f * editedValues[ADJUSTED_VALUE_MAX_SP]).Parse<int>());
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

        public override void OnIntoBattleFirst(UnitCtrlBase e)
        {
            base.OnIntoBattleFirst(e);

            ConvertRealValues2AdjustValues(e);
        }

        public override void OnBattleEnd(BattleEnd e)
        {
            base.OnBattleEnd(e);

            foreach (var cunit in ModBattleEvent.DungeonUnits)
            {
                var data = cunit.data.TryCast<UnitDataHuman>();
                var wunit = data?.worldUnitData?.unit;
                if (wunit != null)
                {
                    ConvertAdjustValues2RealValues(wunit);
                }
            }
        }
    }
}
