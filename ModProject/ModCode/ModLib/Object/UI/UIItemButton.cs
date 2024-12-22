using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static ModLib.Object.UIItemBase;

namespace ModLib.Object
{
    public class UIItemButton : UIItem<Button>, ITextFormat
    {
        public override Text InnerText => ButtonLabel;
        public string FormatStr { get; set; }
        public Text ButtonLabel { get; set; }

        public UIItemButton(UICustomBase ui, float x, float y, Action act, string format, Button copySource = null) : base(ui, (copySource ?? UISampleHelper.ButtonSample).Copy(ui.UIBase))
        {
            Init(x, y, act, format);
        }

        protected virtual void Init(float x, float y, Action act, string format)
        {
            Pos(x, y);

            ButtonLabel = Item.GetComponentInChildren<Text>();
            Set(format);

            if (act != null)
                Item.onClick.AddListener(act);
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
            ButtonLabel.Set(Get()?.ToString());
        }

        public void SetFormat(string format)
        {
            Set(format);
        }

        public override void Update()
        {
            base.Update();
            if (ItemWork?.EnableAct != null)
                Enable = ItemWork?.EnableAct?.Invoke(this) ?? false;
            Item.enabled = Enable;
            ButtonLabel.Set(Get()?.ToString());
        }

        public UIItemButton Align(TextAnchor tanchor = TextAnchor.MiddleLeft, VerticalWrapMode vMode = VerticalWrapMode.Overflow, HorizontalWrapMode hMode = HorizontalWrapMode.Overflow)
        {
            var txt = Item.GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.Align(tanchor, vMode, hMode);
            }
            return this;
        }

        public UIItemButton Format(Color? color = null, int fsize = 15, FontStyle fstype = FontStyle.Normal)
        {
            var txt = Item.GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.Format(color, fsize, fstype);
            }
            return this;
        }

        public UIItemButton Size(float scaleX = 0f, float scaleY = 0f)
        {
            Parallel.ForEach(Item.GetComponentsInChildren<RectTransform>(), s => s.sizeDelta = new Vector2(scaleX, scaleY));
            return this;
        }

        public UIItemButton AddSize(float scaleX = 0f, float scaleY = 0f)
        {
            Parallel.ForEach(Item.GetComponentsInChildren<RectTransform>(), s => s.sizeDelta = new Vector2(s.sizeDelta.x + scaleX, s.sizeDelta.y + scaleY));
            return this;
        }
    }
}