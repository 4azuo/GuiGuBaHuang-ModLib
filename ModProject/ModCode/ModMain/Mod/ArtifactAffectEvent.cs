using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using static Il2CppSystem.Uri;
using ModLib.Enum;

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
                    texts[1].text = (uiInfo1.unit.GetDynProperty(UnitDynPropertyEnum.Attack).value + artAdjustValues[0]).ToString();
                    texts[2].text = $"(+{artAdjustValues[0]})";
                }
                if (artAdjustValues[1] > 0)
                {
                    texts[4].text = (uiInfo1.unit.GetDynProperty(UnitDynPropertyEnum.Defense).value + artAdjustValues[1]).ToString();
                    texts[5].text = $"(+{artAdjustValues[1]})";
                }
            }

            var uiInfo2 = MonoBehaviour.FindObjectOfType<UIPlayerInfo>();
            if (uiInfo2 != null)
            {
                var artAdjustValues = ArtifactAffectEvent.GetAdjustValues(uiInfo2.unit);
                var texts = uiInfo2.uiPropertyCommon.goItem4_En.GetComponentsInChildren<Text>();
                if (artAdjustValues[0] > 0)
                {
                    texts[1].text = (uiInfo2.unit.GetDynProperty(UnitDynPropertyEnum.Attack).value + artAdjustValues[0]).ToString();
                    texts[0].text = $"(+{artAdjustValues[0]})";
                }
                else
                {
                    texts[1].text = uiInfo2.unit.GetDynProperty(UnitDynPropertyEnum.Attack).value.ToString();
                    texts[0].text = string.Empty;
                }
                if (artAdjustValues[1] > 0)
                {
                    texts[4].text = (uiInfo2.unit.GetDynProperty(UnitDynPropertyEnum.Defense).value + artAdjustValues[1]).ToString();
                    texts[3].text = $"(+{artAdjustValues[1]})";
                }
                else
                {
                    texts[4].text = uiInfo2.unit.GetDynProperty(UnitDynPropertyEnum.Defense).value.ToString();
                    texts[3].text = string.Empty;
                }
            }
        }

        public static int[] GetAdjustValues(WorldUnitBase wunit)
        {
            var rs = new int[] { 0, 0 };

            var artifacts = wunit.data.unitData.propData.GetEquipProps().ToArray().Where(x => x?.propsItem?.IsArtifact() != null).ToArray();
            foreach (var artifact in artifacts)
            {
                var artifactInfo = artifact?.propsItem?.IsArtifact();
                if (artifactInfo.durable > 0)
                {
                    rs[0] += artifact.propsInfoBase.level * artifactInfo.atk / 12;
                    rs[1] += artifact.propsInfoBase.level * artifactInfo.def / 24;
                }
            }

            return rs;
        }
    }
}
