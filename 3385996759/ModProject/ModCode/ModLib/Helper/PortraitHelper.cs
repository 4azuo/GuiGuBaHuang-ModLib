using ModLib.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace ModLib.Helper
{
    /// <summary>
    /// Draws characters with the game's portrait model (the art of the NPC info / dramas) into a RawImage.
    /// What the game gives:
    /// - the art goes from the head to the thighs (no feet: the game's own screens cut it there too);
    /// - the texture is created at the size of the RawImage when drawing: set the size first (Draw does it);
    /// - the model is drawn from the top of the texture, centred; modelScale 0.5 shows the whole art in a 0.6 : 1
    ///   (width : height) image (about the top 12-54 % of its height), bigger values zoom on the chest.
    /// Looks: a unit's (WorldUnitBase), or random looks of the game by sex (RandomModelData: no world needed, e.g. on
    /// the title screen).
    /// </summary>
    [ActionCat("Portrait")]
    public static class PortraitHelper
    {
        /// <summary>Width : height of an image that shows the whole art at DEFAULT_MODEL_SCALE.</summary>
        public const float DEFAULT_RATIO = 0.6f;
        /// <summary>Model scale that shows the art from the head to the thighs.</summary>
        public const float DEFAULT_MODEL_SCALE = 0.5f;

        /// <summary>Random looks of the game (ConfRoleDress) for a sex and a grade.</summary>
        public static PortraitModelData RandomModelData(UnitSexType sex, int grade = 1)
        {
            return g.conf.roleDress.RandomDress((int)sex, false, grade, false)?.modelData;
        }

        /// <summary>The looks of a unit.</summary>
        public static PortraitModelData GetModelData(this WorldUnitBase unit)
        {
            return unit?.data?.dynUnitData?.modelData;
        }

        /// <summary>
        /// Draws looks into a new RawImage child of parent: size (its texture is that size), bottom-left anchored at pos.
        /// Returns the RawImage (null without looks).
        /// </summary>
        public static RawImage Draw(Transform parent, PortraitModelData data, Vector2 size, Vector2 pos,
            float modelScale = DEFAULT_MODEL_SCALE, Vector2? modelOffset = null, string name = "Portrait")
        {
            if (data == null || parent == null) return null;
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rimg = go.AddComponent<RawImage>();
            rimg.raycastTarget = false;
            var rt = rimg.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.zero;
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            PortraitModel.CreateTextureInModelData(data, rimg, modelOffset ?? Vector2.zero, modelScale, false, false, null);
            rimg.uvRect = new Rect(0f, 0f, 1f, 1f);
            return rimg;
        }

        /// <summary>Draws a unit (see Draw).</summary>
        public static RawImage Draw(Transform parent, WorldUnitBase unit, Vector2 size, Vector2 pos,
            float modelScale = DEFAULT_MODEL_SCALE, Vector2? modelOffset = null)
        {
            return Draw(parent, unit.GetModelData(), size, pos, modelScale, modelOffset, "Portrait:" + unit?.GetUnitId());
        }

        /// <summary>
        /// Draws looks on the screen, above every UI (own overlay canvas: screen pixels, no scale or clipping of a
        /// window): height = share of the screen height, width by DEFAULT_RATIO, bottom-left at (x, y) pixels.
        /// Returns the root object: destroy it (UnityEngine.Object.Destroy) to remove the portrait.
        /// </summary>
        public static GameObject DrawOnScreen(PortraitModelData data, float heightShare, float x = 0f, float y = 0f,
            int sortingOrder = 100, float modelScale = DEFAULT_MODEL_SCALE)
        {
            if (data == null) return null;
            var root = new GameObject("PortraitOverlay");
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var h = Screen.height * heightShare;
            Draw(root.transform, data, new Vector2(h * DEFAULT_RATIO, h), new Vector2(x, y), modelScale);
            return root;
        }
    }
}
