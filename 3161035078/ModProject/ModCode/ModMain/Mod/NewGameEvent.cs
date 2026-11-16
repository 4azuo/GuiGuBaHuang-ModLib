using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Helper;
using ModLib.Mod;
using ModLib.Object;
using System.Collections.Generic;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// New game options on the character creation screen: the divine artifacts to start with (Bagua Jade, God Eye,
    /// Devil Demon pot; one, two or all three, by checkbox) and "skip the intro and the first battle".
    /// The choices are remembered globally (next new game) and applied to the new save on the map.
    /// </summary>
    [Cache(ModConst.NEW_GAME_EVENT, CacheType = CacheAttribute.CType.Global, WorkOn = CacheAttribute.WType.All)]
    public class NewGameEvent : ModEvent
    {
        public static NewGameEvent Instance { get; set; }

        /// <summary>Start with the Bagua Jade (Pisces Pendant).</summary>
        public bool PiscesPendant { get; set; } = false;
        /// <summary>Start with the God Eye.</summary>
        public bool GodEye { get; set; } = false;
        /// <summary>Start with the Devil Demon pot.</summary>
        public bool DevilDemon { get; set; } = false;
        /// <summary>Skip the intro screens (IntoGameInfo, IntoGameDrama) and so the first battle.</summary>
        public bool SkipIntro { get; set; } = false;

        // when the intro was skipped (the game then opens an artifact window by itself: closed too)
        private static System.DateTime skippedAt = System.DateTime.MinValue;
        private static bool IsArtifactUI(string uiName) => uiName == UIType.GodEye.uiName || uiName == UIType.PiscesPendant.uiName || uiName == UIType.DevilDemon.uiName;

        // the artifacts are given once per new game, after the intro (its drama can pick an artifact by itself);
        // later map openings (after a battle, a trainer change...) are left alone
        private enum Stage { None, NewGame, IntroEnded }
        private static Stage stage = Stage.None;

        /// <summary>The checked artifacts (empty: the game's own choice is kept).</summary>
        public GameAnimaWeapon[] SelectedWeapons()
        {
            var rs = new List<GameAnimaWeapon>();
            if (PiscesPendant) rs.Add(GameAnimaWeapon.PiscesPendant);
            if (GodEye) rs.Add(GameAnimaWeapon.HootinEye);
            if (DevilDemon) rs.Add(GameAnimaWeapon.DevilDemon);
            return rs.ToArray();
        }

        /// <summary>A new game starts: the artifacts are applied on its map (and again once the intro is over).</summary>
        public override void OnLoadNewGame()
        {
            base.OnLoadNewGame();
            stage = Stage.NewGame;
        }

        /// <summary>
        /// Character creation: adds the artifact checkboxes and the skip option.
        /// New game: gives the checked artifacts on the map; with SkipIntro closes the intro screens (and so the first battle).
        /// </summary>
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.CreatePlayer.uiName)
            {
                AddOptions(e.ui);
                return;
            }
            if (!GameHelper.IsInGame())
                return;
            if (e.uiType.uiName == UIType.MapMain.uiName)
            {
                if (stage == Stage.None)
                    return;
                ApplyWeapons();
                // the map after the intro (its battle): done for this game
                if (stage == Stage.IntroEnded)
                    stage = Stage.None;
            }
            else if (stage != Stage.None && SkipIntro && (e.uiType.uiName == UIType.IntoGameInfo.uiName || e.uiType.uiName == UIType.IntoGameDrama.uiName))
            {
                var type = e.uiType;
                skippedAt = System.DateTime.Now;
                // closed on the next update: closing a UI inside its own open event is not safe
                ModDelayEvent.Instance.DelayEvent(this, () =>
                {
                    if (g.ui.HasUI(type))
                    {
                        g.ui.CloseUI(type);
                        DebugHelper.WriteLine($"NewGameEvent: intro skipped ({type.uiName})");
                    }
                }, 100);
            }
            else if (SkipIntro && IsArtifactUI(e.uiType.uiName) && (System.DateTime.Now - skippedAt).TotalSeconds < 10)
            {
                var type = e.uiType;
                ModDelayEvent.Instance.DelayEvent(this, () =>
                {
                    if (g.ui.HasUI(type))
                    {
                        g.ui.CloseUI(type);
                        DebugHelper.WriteLine($"NewGameEvent: intro skipped ({type.uiName})");
                    }
                }, 100);
            }
        }

        /// <summary>
        /// End of the intro drama (skipped or played): the artifacts are given again (the drama may have set its own choice).
        /// Skipped: no battle follows, done. Played: once more on the map after the first battle.
        /// </summary>
        public override void OnCloseUIEnd(CloseUIEnd e)
        {
            base.OnCloseUIEnd(e);
            if (stage == Stage.None || !GameHelper.IsInGame() || e.uiType.uiName != UIType.IntoGameDrama.uiName)
                return;
            ApplyWeapons();
            stage = SkipIntro ? Stage.None : Stage.IntroEnded;
        }

        /// <summary>Gives the checked artifacts (no change when none is checked).</summary>
        public void ApplyWeapons()
        {
            var weapons = SelectedWeapons();
            if (weapons.Length == 0)
                return;
            GodArtifactHelper.SetAnimaWeapon(weapons);
            GodArtifactHelper.UpdateUI();
            DebugHelper.WriteLine($"NewGameEvent: divine artifacts {string.Join(", ", weapons)}");
        }

        private void AddOptions(UIBase ui)
        {
            var cover = new UICover<UICreatePlayer>(ui);
            {
                // one line level with the START GAME button (bottom right), from the right of the left pillar;
                // the hint just below. Items are laid out from their measured widths: same gap between a checkbox and
                // its label, and the same gap between two items
                var dy = UIHelper.GetUIDeltaY();
                var y = StartButtonY(cover) ?? cover.Rows[cover.LastRow - 5] + dy * 0.5f;
                var x = cover.Columns[cover.FirstCol + 6];
                var header = cover.AddText(x, y, GameTool.LS("newgame000")).Format(Color.black, 15).Align(TextAnchor.MiddleLeft);
                x = PlaceLeft(header, x, y) + ITEM_GAP;
                x = AddToggle(cover, x, y, "newgame001", PiscesPendant, v => PiscesPendant = v) + ITEM_GAP;
                x = AddToggle(cover, x, y, "newgame002", GodEye, v => GodEye = v) + ITEM_GAP;
                x = AddToggle(cover, x, y, "newgame003", DevilDemon, v => DevilDemon = v) + ITEM_GAP * 1.5f;
                AddToggle(cover, x, y, "newgame004", SkipIntro, v => SkipIntro = v);
                var hint = cover.AddText(0f, 0f, GameTool.LS("newgame005")).Format(Color.black, 12).Align(TextAnchor.MiddleLeft);
                PlaceLeft(hint, cover.Columns[cover.FirstCol + 6], y - dy * 0.9f);
            }
        }

        // gaps between a checkbox and its label, and between two items (canvas units)
        private const float LABEL_GAP = 8f;
        private const float ITEM_GAP = 36f;

        /// <summary>Height of the START GAME button in the cover coordinates (null when not found).</summary>
        private static float? StartButtonY(UICover<UICreatePlayer> cover)
        {
            var btn = cover.UI.btnOK_En != null && cover.UI.btnOK_En.gameObject.activeInHierarchy ? cover.UI.btnOK_En : cover.UI.btnOK;
            var canvas = g.ui.canvas?.GetComponent<RectTransform>();
            if (btn == null || canvas == null)
                return null;
            return canvas.InverseTransformPoint(btn.transform.position).y;
        }

        /// <summary>Puts a left aligned text with its left edge at x; returns the x of its right end.</summary>
        private static float PlaceLeft(UIItemText text, float x, float y)
        {
            var rect = text.Item.rectTransform.rect.width;
            text.Pos(x + rect / 2f, y);
            return x + text.Item.preferredWidth;
        }

        /// <summary>Checkbox with its box left edge at x, then its label; returns the x of the label end.</summary>
        private float AddToggle(UICover<UICreatePlayer> cover, float x, float y, string labelKey, bool value, System.Action<bool> set)
        {
            var comp = cover.AddCompositeToggle(x, y, string.Empty, value, GameTool.LS(labelKey));
            comp.SetWork(new UIItemWork
            {
                ChangeAct = ActionHelper.WTracedAction<UIItemBase, object>((s, v) =>
                {
                    set(v.Parse<bool>());
                    CacheHelper.SaveGlobalCache(this); // remembered for the next new game
                })
            });
            // the visible box is the toggle's target graphic (not always centered on the toggle)
            var toggle = (comp.MainComponent as UIItemToggle).Item;
            var box = toggle.targetGraphic != null ? toggle.targetGraphic.rectTransform : toggle.GetComponent<RectTransform>();
            var boxWidth = box.rect.width * box.localScale.x;
            var boxOffset = box == toggle.transform ? 0f : box.localPosition.x;
            comp.MainComponent.Pos(x + boxWidth / 2f - boxOffset, y);
            return PlaceLeft(comp.Postfix, x + boxWidth + LABEL_GAP, y);
        }
    }
}
