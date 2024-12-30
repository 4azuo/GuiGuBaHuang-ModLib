using UnityEngine;
using UnityEngine.UI;
using static ModLib.Object.UIItemBase;

namespace ModLib.Object
{
    public class UIItemText : UIItem<Text>, ITextFormat
    {
        public override Text InnerText => this.Item;
        public string FormatStr { get; set; }
        public Color Color { get; set; }

        public UIItemText(UICustomBase ui, float x, float y, string format, Text copySource = null) : base(ui, (copySource ?? UISampleHelper.TextSample).Copy(ui.UIBase))
        {
            Init(x, y, format);
        }

        protected virtual void Init(float x, float y, string format)
        {
            Pos(x, y);
            Set(format);
            Align(TextAnchor.MiddleCenter);
            Format();
        }

        public override object Get()
        {
            if (string.IsNullOrEmpty(FormatStr))
                return null;
            if (ItemWork?.Formatter == null)
                return FormatStr;
            return string.Format(FormatStr, ItemWork?.Formatter?.Invoke(this));
        }

        public string GetFormat()
        {
            return FormatStr;
        }

        public override void Set(object input)
        {
            FormatStr = input?.ToString();
            Item.Set(Get()?.ToString());
        }

        public void SetFormat(string format)
        {
            Set(format);
        }

        public override void Update()
        {
            base.Update();
            Item.Set(Get()?.ToString());
            if (ItemWork?.EnableAct != null)
                Enable = ItemWork?.EnableAct?.Invoke(this) ?? false;
            Item.color = Enable ? Color : Color.gray;
        }

        public UIItemText Align(TextAnchor tanchor = TextAnchor.MiddleLeft, VerticalWrapMode vMode = VerticalWrapMode.Overflow, HorizontalWrapMode hMode = HorizontalWrapMode.Overflow)
        {
            Item.Align(tanchor, vMode, hMode);
            return this;
        }

        public UIItemText Format(Color? color = null, int fsize = 15, FontStyle fstype = FontStyle.Normal)
        {
            Item.Format(color, fsize, fstype);
            Color = Item.color;
            return this;
        }
    }
}