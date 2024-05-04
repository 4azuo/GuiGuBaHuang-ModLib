﻿using EGameTypeData;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Object;
using ModLib.Mod;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MOD_nE7UL2.Object.InGameStts._HideButtonConfigs;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.HIDE_BUTTON_EVENT)]
    public class HideButtonEvent : ModEvent
    {
        public static InGameStts._HideButtonConfigs HideButtonConfigs
        {
            get
            {
                return ModMain.ModObj.InGameSettings.HideButtonConfigs;
            }
        }

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            IDictionary<string, SelectOption> buttonConfigs;
            if (HideButtonConfigs.ButtonConfigs.TryGetValue(e.uiType.uiName, out buttonConfigs))
            {
                var ui = g.ui.GetUI(e.uiType);
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
    }
}
