using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.HIDE_BUTTON_EVENT)]
    public class HideButtonEvent : ModEvent
    {
        public readonly IDictionary<string, string[]> HideButtons = new Dictionary<string, string[]>()
        {
            ["GameMemu"] = new string[] { "G:btnSave", "G:btnReloadCache" },
            ["MartialPropInfo"] = new string[] { "G:btnPreview" },
        };

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            if (HideButtons.ContainsKey(e.uiType.uiName))
            {
                var ui = g.ui.GetUI(e.uiType);
                var hideButtons = HideButtons[e.uiType.uiName];
                foreach (var comp in ui.GetComponentsInChildren<MonoBehaviour>().Where(x => hideButtons.Contains(x.name)))
                {
                    comp.gameObject.active = false;
                }
            }
        }
    }
}
