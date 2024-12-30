using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine.Events;
using UnityEngine;
using Il2CppSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using MOD_nE7UL2.Object;
using System.Text;
using ModLib.Object;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.HIRE_PEOPLE_EVENT)]
    public class HirePeopleEvent : ModEvent
    {

        [EventCondition]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.TownBounty.uiName)
            {
                var ui = new UICover<UITownBounty>(e.ui);
                {
                    ui.AddButton(0, 0, OpenUIHirePeople, "Hire People").Size(160, 40).Pos(ui.UI.btnTaskPut.transform, 4f, 0f);
                }
                ui.UpdateUI();
            }
        }

        private void OpenUIHirePeople()
        {
            //var uiTownBounty = g.ui.GetUI<UITownBounty>(UIType.TownBounty);
            //if (uiTownBounty.IsExists())
            //{
            //    //open select ui
            //    var ui = g.ui.OpenUI<UINPCSearch>(UIType.NPCSearch);
            //    ui.
            //}
        }
    }
}
