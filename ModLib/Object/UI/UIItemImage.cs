using UnityEngine;
using UnityEngine.UI;
using static ModLib.Object.UIItemBase;
using ModLib.Helper;

namespace ModLib.Object
{
    /// <summary>
    /// Custom image.
    /// </summary>
    public class UIItemImage : UIItem<Image>
    {
        /// <summary>
        /// Creates the item and registers it in the owner UI (and the current page when paging is used).
        /// </summary>
        public UIItemImage(UICustomBase ui, float x, float y, Sprite s, Image copySource = null) : base(ui, (copySource ?? UISampleHelper.ImageSample).Copy(ui.UIBase))
        {
            Init(x, y, s);
        }

        /// <summary>
        /// Creates the underlying Unity component (copied from a sample or copySource) and places it at the given position.
        /// </summary>
        protected virtual void Init(float x, float y, Sprite s)
        {
            Size(64f, 64f);
            Pos(x, y);
            Set(s);
        }

        /// <summary>
        /// Returns the current value of the component.
        /// </summary>
        public override object Get()
        {
            return Item.sprite;
        }

        /// <summary>
        /// Sprite set on the item.
        /// </summary>
        public Sprite GetMainSprite()
        {
            return Get() as Sprite;
        }

        /// <summary>
        /// Sprite currently displayed.
        /// </summary>
        public Sprite GetActiveSprite()
        {
            return Item.activeSprite;
        }

        //use SpriteTool to create a sprite
        /// <summary>
        /// Sets the value of the component.
        /// </summary>
        public override void Set(object input)
        {
            Item.sprite = (Sprite)input;
        }

        /// <summary>
        /// Changes the sprite.
        /// </summary>
        public void SetSprite(Sprite s)
        {
            Item.sprite = s;
        }

        /// <summary>
        /// Refreshes the component from its UIItemData/UIItemWork (formatter, enable/visible conditions).
        /// </summary>
        public override void Update()
        {
            base.Update();
            if (ItemWork?.EnableAct != null)
                Enable = ItemWork?.EnableAct?.Invoke(this) ?? false;
            Item.enabled = Enable;
        }

        /// <summary>
        /// Sets the size of the component (0 keeps the current value, fluent).
        /// </summary>
        public UIItemImage Size(float scaleX = 0f, float scaleY = 0f)
        {
            Item.Size(scaleX, scaleY);
            return this;
        }

        /// <summary>
        /// Adds to the current size of the component (fluent).
        /// </summary>
        public UIItemImage AddSize(float scaleX = 0f, float scaleY = 0f)
        {
            Item.AddSize(scaleX, scaleY);
            return this;
        }
    }
}