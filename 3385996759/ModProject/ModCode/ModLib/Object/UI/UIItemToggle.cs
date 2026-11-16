using ModLib.Helper;
using UnityEngine.UI;
using static ModLib.Object.UIItemBase;

namespace ModLib.Object
{
    /// <summary>
    /// Custom toggle (checkbox).
    /// </summary>
    public class UIItemToggle : UIItem<Toggle>
    {
        /// <summary>
        /// Creates the item and registers it in the owner UI (and the current page when paging is used).
        /// </summary>
        public UIItemToggle(UICustomBase ui, float x, float y, bool def, Toggle copySource = null) : base(ui, (copySource ?? UISampleHelper.ToggleSample).Copy(ui.UIBase))
        {
            Init(x, y, def);
        }

        /// <summary>
        /// Creates the underlying Unity component (copied from a sample or copySource) and places it at the given position.
        /// </summary>
        protected virtual void Init(float x, float y, bool def)
        {
            Pos(x, y);
            Set(def);

            Item.onValueChanged.AddListener(ActionHelper.TracedUnityAction<bool>((v) => ItemWork?.ChangeAct?.Invoke(this, v), fin: (v) => UI.UpdateUI()));
        }

        /// <summary>
        /// Returns the current value of the component.
        /// </summary>
        public override object Get()
        {
            return Item.isOn;
        }

        /// <summary>
        /// Sets the value of the component.
        /// </summary>
        public override void Set(object input)
        {
            Item.Set((bool)input);
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
    }
}