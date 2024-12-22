using UnityEngine.Events;
using UnityEngine.UI;
using static ModLib.Object.UIItemBase;

namespace ModLib.Object
{
    public class UIItemToggle : UIItem<Toggle>
    {
        public UIItemToggle(UICustomBase ui, float x, float y, bool def, Toggle copySource = null) : base(ui, (copySource ?? ui.UISample1.ui.tglWindow).Copy(ui.UIBase))
        {
            Init(x, y, def);
        }

        protected virtual void Init(float x, float y, bool def)
        {
            Pos(x, y);
            Set(def);
            Item.onValueChanged.AddListener((UnityAction<bool>)(v =>
            {
                ItemWork?.ChangeAct?.Invoke(this, v);
            }));
        }

        public override object Get()
        {
            return Item.isOn;
        }

        public override void Set(object input)
        {
            Item.Set((bool)input);
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