using UnityEngine;
using UnityEngine.UI;
using static ModLib.Object.UIItemBase;
using ModLib.Helper;

namespace ModLib.Object
{
    public class UIItemImage : UIItem<Image>
    {
        public UIItemImage(UICustomBase ui, float x, float y, Sprite s, Image copySource = null) : base(ui, (copySource ?? UISampleHelper.ImageSample).Copy(ui.UIBase))
        {
            Init(x, y, s);
        }

        protected virtual void Init(float x, float y, Sprite s)
        {
            Size(64f, 64f);
            Pos(x, y);
            Set(s);
        }

        public override object Get()
        {
            return Item.sprite;
        }

        public Sprite GetMainSprite()
        {
            return Get() as Sprite;
        }

        public Sprite GetActiveSprite()
        {
            return Item.activeSprite;
        }

        //use SpriteTool to create a sprite
        public override void Set(object input)
        {
            Item.sprite = (Sprite)input;
        }

        public void SetSprite(Sprite s)
        {
            Item.sprite = s;
        }

        public override void Update()
        {
            base.Update();
            if (ItemWork?.EnableAct != null)
                Enable = ItemWork?.EnableAct?.Invoke(this) ?? false;
            Item.enabled = Enable;
        }

        public UIItemImage Size(float scaleX = 0f, float scaleY = 0f)
        {
            Item.Size(scaleX, scaleY);
            return this;
        }

        public UIItemImage AddSize(float scaleX = 0f, float scaleY = 0f)
        {
            Item.AddSize(scaleX, scaleY);
            return this;
        }
    }
}