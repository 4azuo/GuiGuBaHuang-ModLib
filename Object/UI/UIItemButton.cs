using ModLib.Helper;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static ModLib.Object.UIItemBase;

namespace ModLib.Object
{
    /// <summary>
    /// Custom button with a click action and formatted text.
    /// </summary>
    public class UIItemButton : UIItem<Button>, ITextFormat
    {
        public override Text InnerText => ButtonLabel;
        public string FormatStr { get; set; }
        public Text ButtonLabel { get; set; }

        /// <summary>
        /// Creates the item and registers it in the owner UI (and the current page when paging is used).
        /// </summary>
        public UIItemButton(UICustomBase ui, float x, float y, Action act, string format, Button copySource = null) : base(ui, (copySource ?? UISampleHelper.ButtonSample).Copy(ui.UIBase))
        {
            Init(x, y, act, format);
        }

        /// <summary>
        /// Creates the underlying Unity component (copied from a sample or copySource) and places it at the given position.
        /// </summary>
        protected virtual void Init(float x, float y, Action act, string format)
        {
            Pos(x, y);

            ButtonLabel = Item.GetComponentInChildren<Text>();
            Set(format);

            if (act != null)
                Item.onClick.AddListener(ActionHelper.TracedUnityAction(act, fin: UI.UpdateUI));
        }

        /// <summary>
        /// Returns the current value of the component.
        /// </summary>
        public override object Get()
        {
            if (string.IsNullOrEmpty(FormatStr))
                return null;
            if (ItemWork?.Formatter == null)
                return FormatStr;
            return string.Format(FormatStr, ItemWork?.Formatter?.Invoke(this));
        }

        /// <summary>
        /// Text format string.
        /// </summary>
        public string GetFormat()
        {
            return FormatStr;
        }

        /// <summary>
        /// Sets the value of the component.
        /// </summary>
        public override void Set(object input)
        {
            FormatStr = input?.ToString();
            ButtonLabel.Set(Get()?.ToString());
        }

        /// <summary>
        /// Sets the text format string.
        /// </summary>
        public void SetFormat(string format)
        {
            Set(format);
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
            ButtonLabel.Set(Get()?.ToString());
        }

        /// <summary>
        /// Sets text alignment and wrap modes (fluent).
        /// </summary>
        public UIItemButton Align(TextAnchor tanchor = TextAnchor.MiddleLeft, VerticalWrapMode vMode = VerticalWrapMode.Overflow, HorizontalWrapMode hMode = HorizontalWrapMode.Overflow)
        {
            var txt = Item.GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.Align(tanchor, vMode, hMode);
            }
            return this;
        }

        /// <summary>
        /// Sets text color, font size and style (fluent).
        /// </summary>
        public UIItemButton Format(Color? color = null, int fsize = 15, FontStyle fstype = FontStyle.Normal)
        {
            var txt = Item.GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.Format(color, fsize, fstype);
            }
            return this;
        }

        /// <summary>
        /// Sets the size of the component (0 keeps the current value, fluent).
        /// </summary>
        public UIItemButton Size(float scaleX = 0f, float scaleY = 0f)
        {
            Parallel.ForEach(Item.GetComponentsInChildren<RectTransform>(), ActionHelper.TracedAction<RectTransform>(s => s.sizeDelta = new Vector2(scaleX, scaleY)));
            return this;
        }

        /// <summary>
        /// Adds to the current size of the component (fluent).
        /// </summary>
        public UIItemButton AddSize(float scaleX = 0f, float scaleY = 0f)
        {
            Parallel.ForEach(Item.GetComponentsInChildren<RectTransform>(), ActionHelper.TracedAction<RectTransform>(s => s.sizeDelta = new Vector2(s.sizeDelta.x + scaleX, s.sizeDelta.y + scaleY)));
            return this;
        }
    }
}