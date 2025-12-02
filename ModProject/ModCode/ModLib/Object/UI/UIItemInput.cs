using UnityEngine;
using UnityEngine.UI;
using static ModLib.Object.UIItemBase;
using ModLib.Helper;

namespace ModLib.Object
{
    public class UIItemInput : UIItem<InputField>
    {
        public override Text InnerText => Item.textComponent;

        public UIItemInput(UICustomBase ui, float x, float y, string def, InputField copySource = null) : base(ui, (copySource ?? UISampleHelper.InputSample).Copy(ui.UIBase).Pos(x, y).Size(180f, 26f).Align().Format(Color.black, 13))
        {
            Init(def);
        }

        protected virtual void Init(string def)
        {
            Set(def);
        }

        public override object Get()
        {
            return Item.text;
        }

        public override void Set(object input)
        {
            Item.text = input?.ToString();
        }

        public override void Update()
        {
            base.Update();
            if (ItemWork?.EnableAct != null)
                Enable = ItemWork?.EnableAct?.Invoke(this) ?? false;
            Item.enabled = Enable;
        }

        public UIItemInput Align(TextAnchor tanchor = TextAnchor.MiddleLeft, VerticalWrapMode vMode = VerticalWrapMode.Overflow, HorizontalWrapMode hMode = HorizontalWrapMode.Overflow)
        {
            Item.textComponent.Align(tanchor, vMode, hMode);
            return this;
        }

        public UIItemInput Format(Color? color = null, int fsize = 15, FontStyle fstype = FontStyle.Normal)
        {
            Item.textComponent.Format(color, fsize, fstype);
            return this;
        }

        public UIItemInput Size(float scaleX = 0f, float scaleY = 0f)
        {
            Item.Size(scaleX, scaleY);
            return this;
        }

        public UIItemInput AddSize(float scaleX = 0f, float scaleY = 0f)
        {
            Item.AddSize(scaleX, scaleY);
            return this;
        }
    }
}