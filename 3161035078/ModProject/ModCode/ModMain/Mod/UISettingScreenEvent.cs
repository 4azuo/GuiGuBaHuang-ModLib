using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Mod;
using ModLib.Object;
using UnityEngine;
using ModLib.Helper;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Taoist settings in the game settings UI (large window): hide damage multiplier, hide exception logs, market offer
    /// notifications, the Taoist monthly log. The divine artifact hotkeys are bound in the game's Bindings tab
    /// (AnimaWeaponEvent).
    /// </summary>
    [Cache(ModConst.UI_SETTING_SCREEN_EVENT)]
    public class UISettingScreenEvent : ModEvent
    {
        public static UISettingScreenEvent Instance { get; set; }

        public bool HideDamageMultiplier { get; set; } = false;
        public bool HideExceptionLogs { get; set; } = false;
        /// <summary>A message when an NPC makes or raises an offer on a player listing (RealMarketEvent3).</summary>
        public bool NotifyMarketOffers { get; set; } = true;

        /// <summary>
        /// Applies the exception log setting.
        /// </summary>
        public override void OnLoadGame()
        {
            base.OnLoadGame();
            DebugHelper.IsDebugMode = !HideExceptionLogs;
        }

        /// <summary>
        /// GameSetting: adds the Taoist settings button.
        /// </summary>
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.GameSetting.uiName)
            {
                var ui = new UICover<UIGameSetting>(e.ui);
                {
                    ui.AddButton(ui.MidCol, ui.MidRow - 11, OpenTaoistSettings, GameTool.LS("setting000"), ui.UI.btnSystemOK).Size(200, 40);
                }
            }
        }

        /// <summary>Taoist settings window.</summary>
        public void OpenTaoistSettings()
        {
            var ui = new UICustom1(GameTool.LS("setting000"));
            {
                var col = ui.MidCol;
                var row = ui.FirstRow + 1;
                ui.AddText(col, row++, GameTool.LS("setting010")).Format(null, 17, FontStyle.Italic);
                AddToggle(ui, col, row++, "setting002", HideDamageMultiplier, v => HideDamageMultiplier = v);
                AddToggle(ui, col, row++, "setting003", !DebugHelper.IsDebugMode, v =>
                {
                    DebugHelper.IsDebugMode = !v;
                    HideExceptionLogs = v;
                });
                ui.AddText(col, row++, GameTool.LS("setting004")).Format(null, 13);
                row++;
                ui.AddText(col, row++, GameTool.LS("setting011")).Format(null, 17, FontStyle.Italic);
                AddToggle(ui, col, row++, "setting012", NotifyMarketOffers, v => NotifyMarketOffers = v);
                AddToggle(ui, col, row++, "setting014", EncounterEvent.Instance?.Enabled ?? true, v => { if (EncounterEvent.Instance != null) EncounterEvent.Instance.Enabled = v; });
                ui.AddButton(col - 3, row, () => TaoistLogEvent.Instance?.OpenLog(), GameTool.LS("setting013")).Size(240, 40);
                ui.AddButton(col + 3, row++, SMScoreEvent.OpenRecords, GameTool.LS("setting016")).Size(240, 40);
                row++;
                // divine artifact hotkeys, then the other hotkeys of the mod (damage calculation)
                ui.AddText(col, row++, GameTool.LS("setting005")).Format(null, 17, FontStyle.Italic);
                foreach (var line in BankLoanEvent.WrapWords(GameTool.LS("setting006"), 110))
                    ui.AddText(col, row++, line).Format(null, 13);
                row = AnimaWeaponEvent.Instance?.AddBindingRows(ui, row, AnimaWeaponEvent.WEAPONS) ?? row;
                ui.AddText(col, row++, GameTool.LS("setting015")).Format(null, 17, FontStyle.Italic);
                row = AnimaWeaponEvent.Instance?.AddBindingRows(ui, row, new[] { AnimaWeaponEvent.DamageInfoRow }, false) ?? row;
                ui.AddText(col, ui.LastRow, GameTool.LS("setting001")).Format(Color.red, 15);
            }
        }

        private static void AddToggle(UICustom1 ui, int col, int row, string labelKey, bool value, System.Action<bool> set)
        {
            ui.AddCompositeToggle(col, row, GameTool.LS(labelKey), value).SetWork(new UIItemWork
            {
                ChangeAct = ActionHelper.WTracedAction<UIItemBase, object>((s, v) => set(v.Parse<bool>()))
            });
        }
    }
}
