using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.HIDE_BUTTON_EVENT)]
    public class HideButtonEvent : ModEvent
    {
        public readonly IDictionary<string, string[]> HideButtons = new Dictionary<string, string[]>()
        {
            ["GameMemu"] = new string[] { "G:btnSave", "G:btnReloadCache" },
        };

        public override void OnOpenUIStart(OpenUIStart e)
        {
            if (HideButtons.ContainsKey(e.uiType.uiName))
            {
                var hideButtons = HideButtons[e.uiType.uiName];
                foreach (var btn in MonoBehaviour.FindObjectsOfType<Button>().Where(x => hideButtons.Contains(x.name)))
                {
                    btn.gameObject.active = false;
                }
            }
        }
    }
}
