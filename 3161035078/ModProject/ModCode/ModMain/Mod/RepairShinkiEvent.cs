using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Mod;
using ModLib.Object;
using System;
using UnityEngine;
using ModLib.Helper;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Optional (S&amp;M): pay to repair god artifacts (Devil Demon, God Eye) or add fish jade to the Pisces Pendant.
    /// </summary>
    [Cache(ModConst.REPAIR_SHINKI_EVENT)]
    public class RepairShinkiEvent : ModEvent
    {
        /// <summary>
        /// Loaded only when AllowRepairGodWeapon is on.
        /// </summary>
        public override bool OnCacheHandler()
        {
            return SMLocalConfigsEvent.Instance.Configs.AllowRepairGodWeapon;
        }

        /// <summary>
        /// Adds the repair button to the god artifact UI.
        /// </summary>
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.DevilDemon.uiName && GodArtifactHelper.IsPotmonDamaged())
            {
                var ui = new UICover<UIDevilDemon>(e.ui);
                {
                    ui.AddButton(ui.MidCol, ui.FirstRow + 2, RepairDevilDemon, GameTool.LS("other500020113")).Size(200, 40);
                }
            }
            else
            if (e.uiType.uiName == UIType.GodEye.uiName && GodArtifactHelper.IsGodEyeDamaged())
            {
                var ui = new UICover<UIGodEye>(e.ui);
                {
                    ui.AddButton(ui.MidCol, ui.FirstRow + 2, RepairGodEye, GameTool.LS("other500020113")).Size(200, 40);
                }
            }
            else
            if (e.uiType.uiName == UIType.PiscesPendant.uiName)
            {
                var ui = new UICover<UIPiscesPendant>(e.ui);
                {
                    ui.AddButton(ui.MidCol, ui.FirstRow + 2, AddFishJade, GameTool.LS("other500020114")).Size(200, 40);
                }
            }
        }

        /// <summary>
        /// Pays and repairs the Devil Demon.
        /// </summary>
        private void RepairDevilDemon()
        {
            var cost = (g.world.playerUnit.GetGradeLvl() + RebirthEvent.Instance.TotalGradeLvl) * 100 * Math.Pow(2, g.world.playerUnit.GetGradeLvl()).Parse<int>();
            cost = InflationaryEvent.CalculateInflationary(cost);
            g.ui.MsgBox(string.Empty, string.Format(GameTool.LS("other500020115"), cost), ModLib.Enum.MsgBoxButtonEnum.YesNo, () =>
            {
                if (g.world.playerUnit.GetUnitMoney() >= cost)
                {
                    g.world.playerUnit.AddUnitMoney(-cost);
                    GodArtifactHelper.RepairPotmonDamaged(1);
                    g.ui.CloseUI(UIType.DevilDemon);
                }
                else
                {
                    g.ui.MsgBox(string.Empty, GameTool.LS("other500020116"));
                }
            });
        }

        /// <summary>
        /// Pays and repairs the God Eye.
        /// </summary>
        private void RepairGodEye()
        {
            var cost = (g.world.playerUnit.GetGradeLvl() + RebirthEvent.Instance.TotalGradeLvl) * 100 * Math.Pow(2, g.world.playerUnit.GetGradeLvl()).Parse<int>();
            cost = InflationaryEvent.CalculateInflationary(cost);
            g.ui.MsgBox(string.Empty, string.Format(GameTool.LS("other500020115"), cost), ModLib.Enum.MsgBoxButtonEnum.YesNo, () =>
            {
                if (g.world.playerUnit.GetUnitMoney() >= cost)
                {
                    g.world.playerUnit.AddUnitMoney(-cost);
                    GodArtifactHelper.RepairGodEyeDamaged(1);
                    g.ui.CloseUI(UIType.GodEye);
                }
                else
                {
                    g.ui.MsgBox(string.Empty, GameTool.LS("other500020116"));
                }
            });
        }

        /// <summary>
        /// Adds one fish jade.
        /// </summary>
        private void AddFishJade()
        {
            var cost = (g.world.playerUnit.GetGradeLvl() + (g.world.unit.GetUnit(GodArtifactHelper.GetPiscesPendantNpcId())?.GetGradeLvl() ?? 0)) * 100 * Math.Pow(2, g.world.playerUnit.GetGradeLvl()).Parse<int>();
            cost = InflationaryEvent.CalculateInflationary(cost);
            g.ui.MsgBox(string.Empty, string.Format(GameTool.LS("other500020115"), cost), ModLib.Enum.MsgBoxButtonEnum.YesNo, () =>
            {
                if (g.world.playerUnit.GetUnitMoney() >= cost)
                {
                    g.world.playerUnit.AddUnitMoney(-cost); // (BUG-05)
                    GodArtifactHelper.AddFishJade(1);
                    g.ui.CloseUI(UIType.PiscesPendant);
                }
                else
                {
                    g.ui.MsgBox(string.Empty, GameTool.LS("other500020116"));
                }
            });
        }
    }
}
