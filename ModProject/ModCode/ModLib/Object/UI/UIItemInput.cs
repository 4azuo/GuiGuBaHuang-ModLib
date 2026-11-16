using UnityEngine;
using UnityEngine.UI;
using static ModLib.Object.UIItemBase;
using ModLib.Helper;

namespace ModLib.Object
{
    /// <summary>
    /// Custom text input field.
    /// </summary>
    public class UIItemInput : UIItem<InputField>
    {
        public override Text InnerText => Item.textComponent;

        /// <summary>
        /// Creates the item and registers it in the owner UI (and the current page when paging is used).
        /// </summary>
        public UIItemInput(UICustomBase ui, float x, float y, string def, InputField copySource = null) : base(ui, (copySource ?? UISampleHelper.InputSample).Copy(ui.UIBase).Pos(x, y).Size(180f, 26f).Align().Format(Color.black, 13))
        {
            Init(def);
        }

        /// <summary>
        /// Creates the underlying Unity component (copied from a sample or copySource) and places it at the given position.
        /// </summary>
        protected virtual void Init(string def)
        {
            Set(def);
        }

        /// <summary>
        /// Returns the current value of the component.
        /// </summary>
        public override object Get()
        {
            return Item.text;
        }

        /// <summary>
        /// Sets the value of the component.
        /// </summary>
        public override void Set(object input)
        {
            Item.text = input?.ToString();
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
        /// Sets text alignment and wrap modes (fluent).
        /// </summary>
        public UIItemInput Align(TextAnchor tanchor = TextAnchor.MiddleLeft, VerticalWrapMode vMode = VerticalWrapMode.Overflow, HorizontalWrapMode hMode = HorizontalWrapMode.Overflow)
        {
            Item.textComponent.Align(tanchor, vMode, hMode);
            return this;
        }

        /// <summary>
        /// Sets text color, font size and style (fluent).
        /// </summary>
        public UIItemInput Format(Color? color = null, int fsize = 15, FontStyle fstype = FontStyle.Normal)
        {
            Item.textComponent.Format(color, fsize, fstype);
            return this;
        }

        /// <summary>
        /// Sets the size of the component (0 keeps the current value, fluent).
        /// </summary>
        public UIItemInput Size(float scaleX = 0f, float scaleY = 0f)
        {
            Item.Size(scaleX, scaleY);
            return this;
        }

        /// <summary>
        /// Adds to the current size of the component (fluent).
        /// </summary>
        public UIItemInput AddSize(float scaleX = 0f, float scaleY = 0f)
        {
            Item.AddSize(scaleX, scaleY);
            return this;
        }
    }
}