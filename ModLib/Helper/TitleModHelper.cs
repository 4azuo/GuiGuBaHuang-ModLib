using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModLib.Helper
{
    /// <summary>
    /// The buttons of the mods on the title screen (their name and version: opens a folder, a page...), side by side in one
    /// row at the bottom, centered: each mod adds its own in OnOpenUIEnd of the Login UI (AddModButton) and the row is laid
    /// out again, whatever the order the mods add them. The buttons of an older title screen (destroyed) are dropped.
    /// </summary>
    public static class TitleModHelper
    {
        /// <summary>Space between two buttons, their height (UI units).</summary>
        public const float GAP = 16f, HEIGHT = 40f;
        /// <summary>Height of the row: this share of the screen height above its bottom.</summary>
        public const float BOTTOM_SHARE = 0.1f;

        private static readonly List<UIItemButton> _buttons = new List<UIItemButton>();

        /// <summary>The last layout (tests, logs): canvas, camera, centre, positions.</summary>
        public static string LastLayout { get; private set; } = string.Empty;

        /// <summary>The buttons of the row (left to right).</summary>
        public static IReadOnlyList<UIItemButton> Buttons => _buttons;

        /// <summary>
        /// A button of a mod in the row of the title screen: <paramref name="text"/> (its name and version), the frame of
        /// the game's buttons, white text; its width from the text (at least <paramref name="minWidth"/>). Null when the
        /// title screen is not open.
        /// </summary>
        public static UIItemButton AddModButton(string text, Action onClick, int fontSize = 18, float minWidth = 200f)
        {
            if (!g.ui.HasUI(UIType.Login)) return null;
            Prune();
            var ui = new UICover<UILogin>(UIType.Login);
            var btn = (UIItemButton)ui.AddButton(0f, 0f, onClick, text, ui.UI.btnPaperChange);
            btn.Align(TextAnchor.MiddleCenter).Format(Color.white, fontSize);
            btn.SetParentTransform(ui.UI.btnSet.transform.parent);
            btn.Size(Math.Max(minWidth, text.Length * fontSize * 0.55f + 40f), HEIGHT);
            _buttons.Add(btn);
            Layout();
            return btn;
        }

        /// <summary>
        /// The row laid out again: centered, BOTTOM_SHARE of the screen height above its bottom (screen point -> world point
        /// of the parent of the buttons: whatever the canvas and its scale).
        /// </summary>
        public static void Layout()
        {
            Prune();
            if (_buttons.Count == 0) return;
            var rts = _buttons.Select(b => ((Component)b.Component).GetComponent<RectTransform>()).ToList();
            var parent = rts[0].parent == null ? null : rts[0].parent.GetComponent<RectTransform>();
            var canvas = rts[0].GetComponentInParent<Canvas>();
            var cam = canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            if (parent == null || !RectTransformUtility.ScreenPointToWorldPointInRectangle(parent, new Vector2(Screen.width / 2f, Screen.height * BOTTOM_SHARE), cam, out var centre))
            {
                DebugHelper.WriteLine($"Title mod buttons: no point of the screen in the parent ({parent?.name}, canvas {canvas?.renderMode}, camera {cam?.name})");
                return;
            }
            var widths = rts.Select(r => r.rect.width * r.lossyScale.x).ToList();
            var gap = GAP * rts[0].lossyScale.x;
            var x = centre.x - (widths.Sum() + gap * (widths.Count - 1)) / 2f;
            for (var i = 0; i < rts.Count; i++)
            {
                rts[i].position = new Vector3(x + widths[i] / 2f, centre.y, rts[i].position.z);
                x += widths[i] + gap;
            }
            LastLayout = $"canvas {canvas?.renderMode}, camera {cam?.name}, centre {centre.x:0.##},{centre.y:0.##},{centre.z:0.##}, buttons " +
                string.Join("; ", rts.Select(r => $"{r.position.x:0.##},{r.position.y:0.##} w {r.rect.width:0}"));
            DebugHelper.WriteLine("Title mod buttons: " + LastLayout);
        }

        /// <summary>The buttons of a title screen that is gone (Unity objects destroyed) dropped.</summary>
        private static void Prune()
        {
            _buttons.RemoveAll(b =>
            {
                try { return b?.Component == null || ((Component)b.Component) == null || ((Component)b.Component).gameObject == null; }
                catch { return true; }
            });
        }
    }
}
