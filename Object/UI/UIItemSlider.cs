using UnityEngine.Events;
using UnityEngine.UI;
using static ModLib.Object.UIItemBase;

namespace ModLib.Object
{
    public class UIItemSlider : UIItem<Slider>
    {
        public float Min { get; set; }
        public float Max { get; set; }

        public UIItemSlider(UICustomBase ui, float x, float y, float min, float max, float def, Slider copySource = null) : base(ui, (copySource ?? UISampleHelper.SliderSample).Copy(ui.UIBase))
        {
            Init(x, y, min, max, def);
        }

        protected virtual void Init(float x, float y, float min, float max, float def)
        {
            Pos(x, y);

            Min = min;
            Max = max;
            Set(def);

            Item.onValueChanged.AddListener((UnityAction<float>)(v => ItemWork?.ChangeAct?.Invoke(this, v)));
            Item.onValueChanged.AddListener((UnityEngine.Events.UnityAction<float>)((value) => UI.UpdateUI()));
        }

        public override object Get()
        {
            return Item.value;
        }

        public override void Set(object input)
        {
            Item.Set(Min, Max, (float)input);
        }

        public void SetPercent(float percent, float min = float.MinValue, float max = float.MaxValue)
        {
            min = min == float.MinValue ? Min : min;
            max = max == float.MaxValue ? Max : max;
            Set(((max - min) * percent) + min);
        }

        public override void Update()
        {
            base.Update();
            if (ItemWork?.EnableAct != null)
                Enable = ItemWork?.EnableAct?.Invoke(this) ?? false;
            Item.enabled = Enable;
        }
    }
}