using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MOD_nE7UL2.Object.InGameStts;
using static MOD_nE7UL2.Object.InGameStts._HideButtonConfigs;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.HIDE_BUTTON_EVENT)]
    public class HideButtonEvent : ModEvent
    {
        public static _HideButtonConfigs Configs => ModMain.ModObj.InGameCustomSettings.HideButtonConfigs;

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);

            if (e?.uiType?.uiName == null || ModMain.ModObj?.InGameCustomSettings?.HideButtonConfigs?.ButtonConfigs == null)
                return;

            IDictionary<string, SelectOption> buttonConfigs;
            if (Configs.ButtonConfigs.TryGetValue(e.uiType.uiName, out buttonConfigs))
            {
                var ui = g.ui.GetUI(e.uiType);
                if (ui == null)
                    return;

                foreach (var buttonConfig in buttonConfigs)
                {
                    string forceHideConfig;
                    Configs.ForceHideConditions.TryGetValue($"{e.uiType.uiName}.{buttonConfig.Key}", out forceHideConfig);

                    var comp = ui.GetComponentsInChildren<MonoBehaviour>().Where(x => buttonConfig.Key == x.name);
                    foreach (var c in comp)
                    {
                        c.gameObject.SetActive(buttonConfig.Value == SelectOption.Show && !IsForeHideConfigOK(forceHideConfig));
                    }
                }
            }
        }

        private bool IsForeHideConfigOK(string conf)
        {
            if (string.IsNullOrEmpty(conf))
                return false;
            conf = conf.Replace("${gradelevel}", g.world.playerUnit.GetGradeLvl().ToString())
                .Replace("${gamelevel}", g.data.dataWorld.data.gameLevel.Parse<int>().ToString());
            return Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScript.EvaluateAsync<bool>(conf).Result;
        }
    }
}
