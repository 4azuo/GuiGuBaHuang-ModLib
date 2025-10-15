using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using ModLib.Object;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.REPAIR_SHINKI_EVENT)]
    public class RepairShinkiEvent : ModEvent
    {
        public override bool OnCacheHandler()
        {
            return base.OnCacheHandler();
        }

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.DevilDemon.uiName && GodArtifactHelper.IsPotmonDamaged())
            {
                var ui = new UICover<UIDevilDemon>(e.ui);
                {
                    ui.AddButton(ui.MidCol, ui.FirstRow + 2, RepairDevilDemon, GameTool.LS("other500020113"));
                }
            }
            else
            if (e.uiType.uiName == UIType.GodEye.uiName && GodArtifactHelper.IsGodEyeDamaged())
            {
                var ui = new UICover<UIGodEye>(e.ui);
                {
                    ui.AddButton(ui.MidCol, ui.FirstRow + 2, RepairGodEye, GameTool.LS("other500020113"));
                }
            }
            else
            if (e.uiType.uiName == UIType.PiscesPendant.uiName)
            {
                var ui = new UICover<UIPiscesPendant>(e.ui);
                {
                    ui.AddButton(ui.MidCol, ui.FirstRow + 2, Test, GameTool.LS("other500020113"));
                }
            }
        }

        private void RepairDevilDemon()
        {
            GodArtifactHelper.RepairPotmonDamaged(1);
            g.ui.CloseUI(UIType.DevilDemon);
        }

        private void RepairGodEye()
        {
            GodArtifactHelper.RepairGodEyeDamaged(1);
            g.ui.CloseUI(UIType.GodEye);
        }

        private void Test()
        {
            //g.data.dataWorld.data.re
        }
    }
}
