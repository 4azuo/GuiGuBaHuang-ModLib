using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using static Il2CppSystem.Uri;
using ModLib.Enum;
using System;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.ARTIFACT_AFFECT_EVENT)]
    public class ArtifactAffectEvent : ModEvent
    {
        public override void OnTimeUpdate1s()
        {
            base.OnTimeUpdate1s();

            var uiInfo1 = MonoBehaviour.FindObjectOfType<UINPCInfo>();
            if (uiInfo1 != null)
            {
                var artAdjustValues = ArtifactAffectEvent.GetAdjustValues(uiInfo1.unit);
                var texts = uiInfo1.uiProperty.goItem4.GetComponentsInChildren<Text>();
                if (artAdjustValues[0] > 0)
                {
                    texts[1].text = $"{uiInfo1.unit.GetDynProperty(UnitDynPropertyEnum.Attack).value + artAdjustValues[0]} (+{artAdjustValues[0]})";
                }
                if (artAdjustValues[1] > 0)
                {
                    texts[4].text = $"{uiInfo1.unit.GetDynProperty(UnitDynPropertyEnum.Defense).value + artAdjustValues[1]} (+{artAdjustValues[1]})";
                }
            }

            var uiInfo2 = MonoBehaviour.FindObjectOfType<UIPlayerInfo>();
            if (uiInfo2 != null)
            {
                var artAdjustValues = ArtifactAffectEvent.GetAdjustValues(uiInfo2.unit);
                var texts = uiInfo2.uiPropertyCommon.goItem4_En.GetComponentsInChildren<Text>();
                if (artAdjustValues[0] > 0)
                {
                    texts[1].text = $"{uiInfo2.unit.GetDynProperty(UnitDynPropertyEnum.Attack).value + artAdjustValues[0]} (+{artAdjustValues[0]})";
                }
                else
                {
                    texts[1].text = uiInfo2.unit.GetDynProperty(UnitDynPropertyEnum.Attack).value.ToString();
                }
                if (artAdjustValues[1] > 0)
                {
                    texts[4].text = $"{uiInfo2.unit.GetDynProperty(UnitDynPropertyEnum.Defense).value + artAdjustValues[1]} (+{artAdjustValues[1]})";
                }
                else
                {
                    texts[4].text = uiInfo2.unit.GetDynProperty(UnitDynPropertyEnum.Defense).value.ToString();
                }
            }
        }

        public static int[] GetAdjustValues(WorldUnitBase wunit)
        {
            var rs = new int[] { 0, 0 };

            var artifacts = wunit.data.unitData.propData.GetEquipProps().ToArray().Where(x => x?.propsItem?.IsArtifact() != null).ToArray();
            foreach (var artifact in artifacts)
            {
                var a = artifact.To<DataProps.PropsArtifact>();
                if (a.durable > 0)
                {
                    var aconf = artifact.propsItem.IsArtifact();
                    var r1 = 0.01f + (0.001f * Math.Pow(2, a.level)) + (0.02f * a.grade);
                    var r2 = (4.00f + (0.006f * Math.Pow(3, a.level)) + (1.00f * a.grade)) / 100.0f;
                    rs[0] += (r1 * wunit.GetDynProperty(UnitDynPropertyEnum.Attack).value + r2 * aconf.atk).Parse<int>();
                    rs[1] += (r1 * wunit.GetDynProperty(UnitDynPropertyEnum.Defense).value + r2 * aconf.def).Parse<int>();
                }
            }

            return rs;
        }
    }
}
