using ModLib.Object;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ModLib.Helper
{
    /// <summary>
    /// A pulsing gold frame around the chosen item of a window (a row, an icon, a tab): a transparent holder with 4 thin
    /// gold edges (the same width on every side). Add it once (after the items it frames, so it is drawn over them), then
    /// Place it on the chosen button whenever the choice changes. It pulses while the window updates (IsAutoUpdate).
    /// First made for the windows of Dao of Creation.
    /// </summary>
    public static class UIGlowHelper
    {
        /// <summary>Width of an edge, space between the button and the frame (px).</summary>
        public const float LINE = 2f, PAD = 3f;

        public static Color GOLD => new Color(1f, 0.74f, 0.15f, 1f);

        /// <summary>The frame (hidden until Place).</summary>
        public static UIItemImage Add(UICustomBase ui, float width = 68f, float height = 68f)
        {
            var glow = ui.AddImage(0f, 0f, null).Size(width, height);
            glow.Item.raycastTarget = false;
            glow.Item.color = new Color(0f, 0f, 0f, 0f);
            var edges = new List<Image>();
            // top, bottom, left, right: anchored to the sides of the holder, LINE thick
            foreach (var (min, max, offMin, offMax) in new[]
            {
                (new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -LINE), Vector2.zero),
                (Vector2.zero, new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, LINE)),
                (Vector2.zero, new Vector2(0f, 1f), Vector2.zero, new Vector2(LINE, 0f)),
                (new Vector2(1f, 0f), Vector2.one, new Vector2(-LINE, 0f), Vector2.zero),
            })
            {
                var go = new GameObject("ModLib_GlowEdge");
                go.transform.SetParent(glow.Item.transform, false);
                var img = go.AddComponent<Image>();
                img.raycastTarget = false;
                img.color = GOLD;
                var rt = img.rectTransform;
                rt.anchorMin = min;
                rt.anchorMax = max;
                rt.offsetMin = offMin;
                rt.offsetMax = offMax;
                edges.Add(img);
            }
            glow.SetWork(new UIItemWork
            {
                UpdateAct = ActionHelper.WTracedAction<UIItemBase>(x =>
                {
                    var c = GOLD;
                    c.a = 0.6f + 0.4f * Mathf.Sin(Time.unscaledTime * 5f);
                    foreach (var e in edges) e.color = c;
                }),
            });
            glow.Active(false);
            return glow;
        }

        /// <summary>Frames a button (centered on it, PAD larger on every side); hidden when show is false or no button.</summary>
        public static void Place(UIItemImage glow, UIItemBase button, bool show = true)
        {
            if (glow == null) return;
            glow.Active(show && button != null);
            if (!show || button == null) return;
            var b = button.Component.GetComponent<RectTransform>();
            var g = glow.Item.rectTransform;
            var size = b.rect.size;
            g.anchorMin = b.anchorMin;
            g.anchorMax = b.anchorMax;
            g.pivot = new Vector2(0.5f, 0.5f);
            g.sizeDelta = new Vector2(size.x + 2f * PAD, size.y + 2f * PAD);
            g.anchoredPosition = b.anchoredPosition + Vector2.Scale(new Vector2(0.5f, 0.5f) - b.pivot, size);
        }
    }
}
