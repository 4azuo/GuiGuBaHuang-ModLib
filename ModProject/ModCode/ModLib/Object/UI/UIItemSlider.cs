using ModLib.Helper;
using UnityEngine.UI;
using static ModLib.Object.UIItemBase;

namespace ModLib.Object
{
    /// <summary>
    /// Custom slider.
    /// </summary>
    public class UIItemSlider : UIItem<Slider>
    {
        public float Min { get; set; }
        public float Max { get; set; }

        /// <summary>
        /// Creates the item and registers it in the owner UI (and the current page when paging is used).
        /// </summary>
        public UIItemSlider(UICustomBase ui, float x, float y, float min, float max, float def, Slider copySource = null) : base(ui, (copySource ?? UISampleHelper.SliderSample).Copy(ui.UIBase))
        {
            Init(x, y, min, max, def);
        }

        /// <summary>
        /// Creates the underlying Unity component (copied from a sample or copySource) and places it at the given position.
        /// </summary>
        protected virtual void Init(float x, float y, float min, float max, float def)
        {
            Pos(x, y);

            Min = min;
            Max = max;
            Set(def);

            Item.onValueChanged.AddListener(ActionHelper.TracedUnityAction<float>((v) => ItemWork?.ChangeAct?.Invoke(this, v), fin: (v) => UI.UpdateUI()));
        }

        /// <summary>
        /// Returns the current value of the component.
        /// </summary>
        public override object Get()
        {
            return Item.value;
        }

        /// <summary>
        /// Sets the value of the component.
        /// </summary>
        public override void Set(object input)
        {
            Item.Set(Min, Max, (float)input);
        }

        /// <summary>
        /// Sets the value by percentage of the range.
        /// </summary>
        public void SetPercent(float percent, float min = float.MinValue, float max = float.MaxValue)
        {
            min = min == float.MinValue ? Min : min;
            max = max == float.MaxValue ? Max : max;
            Set(((max - min) * percent) + min);
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
        /// Sets the minimum.
        /// </summary>
        public void SetMin(float min)
        {
            Min = min;
            Item.minValue = Min;
        }

        /// <summary>
        /// Sets the maximum.
        /// </summary>
        public void SetMax(float max)
        {
            Max = max;
            Item.maxValue = Max;
        }
    }
}