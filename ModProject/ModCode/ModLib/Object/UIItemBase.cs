using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class UIItemBase
{
    public virtual UIBehaviour ItemBehaviour { get; private set; }
    public virtual UIItemData ItemData { get; set; }
    public virtual UIItemWork ItemWork { get; set; }
    public virtual object Tag { get; set; }
    public virtual object PrevValue { get; internal set; }

    public virtual void Update() { }
    public abstract object Get();
    public abstract void Set(object input);
    public abstract bool IsActive();
    public abstract bool IsEnable();
    public abstract void Destroy();

    public UIItemBase() { }

    public UIItemBase SetData(UIItemData data)
    {
        ItemData = data;
        return this;
    }

    public UIItemBase SetWork(UIItemWork wk)
    {
        ItemWork = wk;
        return this;
    }

    #region Data
    public class UIItemData
    {
        public dynamic CustomData { get; set; }
    }
    #endregion

    #region Work
    public class UIItemWork
    {
        public virtual Func<UIItemBase, bool> EnaAct { get; set; }
        public virtual Func<UIItemBase, object[]> Formatter { get; set; }
        public virtual Action<UIItemBase> Change { get; set; }
    }
    #endregion

    #region Item
    public abstract class UIItem<T> : UIItemBase where T : UIBehaviour
    {
        public T Item { get; private set; }

        public UIItem(T comp)
        {
            Item = comp;
            ItemBehaviour = Item;
        }

        public override bool IsActive()
        {
            return Item.IsActive();
        }

        public override bool IsEnable()
        {
            return Item.enabled;
        }

        public override void Destroy()
        {
            MonoBehaviour.Destroy(Item);
        }

        public override void Update()
        {
            base.Update();
            if (ItemWork?.EnaAct != null)
                Item.enabled = ItemWork?.EnaAct?.Invoke(this) ?? false;
        }
    }

    public class UIItemText : UIItem<Text>
    {
        public string FormatStr { get; private set; }

        public UIItemText(Transform t, float x, float y, string format) : base(UIHelper._sampleUI.textSystemOK.Create(t).Pos(UIHelper._originComp.gameObject, x, y))
        {
            FormatStr = format;
            Item.Setup(Get()?.ToString());
            Item.Align(TextAnchor.MiddleCenter);
            Item.Format();
        }

        public override object Get()
        {
            if (string.IsNullOrEmpty(FormatStr))
                return null;
            if (ItemWork?.Formatter == null)
                return FormatStr;
            return string.Format(FormatStr, ItemWork?.Formatter?.Invoke(this));
        }

        public override void Set(object input)
        {
            FormatStr = input.ToString();
            Item.Setup(Get()?.ToString());
        }

        public override void Update()
        {
            base.Update();
            Item.Setup(Get()?.ToString());
        }

        public UIItemText Align(TextAnchor tanchor = TextAnchor.MiddleLeft, VerticalWrapMode vMode = VerticalWrapMode.Overflow, HorizontalWrapMode hMode = HorizontalWrapMode.Overflow)
        {
            Item.Align(tanchor, vMode, hMode);
            return this;
        }

        public UIItemText Format(Color? color = null, int fsize = 15, FontStyle fstype = FontStyle.Normal)
        {
            Item.Format(color, fsize, fstype);
            return this;
        }
    }

    public class UIItemSlider : UIItem<Slider>
    {
        public float Min { get; private set; }
        public float Max { get; private set; }

        public UIItemSlider(Transform t, float x, float y, float min, float max, float def) : base(UIHelper._sampleUI.sliSoundMain.Create(t).Pos(UIHelper._originComp.gameObject, x, y))
        {
            Min = min;
            Max = max;
            Item.Setup(min, max, def);
            Item.onValueChanged.AddListener((UnityAction<float>)(v =>
            {
                this.PrevValue = v;
                ItemWork?.Change?.Invoke(this);
            }));
        }

        public override object Get()
        {
            return Item.value;
        }

        public override void Set(object input)
        {
            Item.Setup(Min, Max, (float)input);
        }
    }

    public class UIItemToggle : UIItem<Toggle>
    {
        public UIItemToggle(Transform t, float x, float y, bool def) : base(UIHelper._sampleUI.tglWindow.Create(t).Pos(UIHelper._originComp.gameObject, x, y))
        {
            Item.Setup(def);
            Item.onValueChanged.AddListener((UnityAction<bool>)(v =>
            {
                this.PrevValue = v;
                ItemWork?.Change?.Invoke(this);
            }));
        }

        public override object Get()
        {
            return Item.isOn;
        }

        public override void Set(object input)
        {
            Item.Setup((bool)input);
        }
    }

    public class UIItemButton : UIItem<Button>
    {
        public string FormatStr { get; private set; }
        public Text ButtonLabel { get; private set; }

        public UIItemButton(Transform t, float x, float y, Action act, string format) : base(UIHelper._sampleUI.btnSystemOK.Create(t).Pos(UIHelper._originComp.gameObject, x, y))
        {
            FormatStr = format;
            ButtonLabel = Item.GetComponentInChildren<Text>();
            ButtonLabel.Setup(Get()?.ToString());
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


        public override void Set(object input)
        {
            FormatStr = input.ToString();
        }

        public override void Update()
        {
            base.Update();
            ButtonLabel.Setup(Get()?.ToString());
        }
    }

    public class UIItemComposite : UIItemBase
    {
        public UIItemText Prefix { get; private set; }
        public UIItemBase MainComponent { get; private set; }
        public UIItemText Postfix { get; private set; }
        public bool HidePostfixIfDisabled { get; set; }

        public override UIBehaviour ItemBehaviour => MainComponent.ItemBehaviour;
        public override UIItemData ItemData
        {
            get => MainComponent.ItemData;
            set
            {
                Prefix.ItemData = value;
                MainComponent.ItemData = value;
                Postfix.ItemData = value;
            }
        }
        public override UIItemWork ItemWork
        {
            get => MainComponent.ItemWork;
            set
            {
                Prefix.ItemWork = value;
                MainComponent.ItemWork = value;
                Postfix.ItemWork = value;
            }
        }
        public override object Tag { get => MainComponent.Tag; set => MainComponent.Tag = value; }
        public override object PrevValue { get => MainComponent.PrevValue; internal set => MainComponent.PrevValue = value; }

        private UIItemComposite() { }

        public static UIItemComposite CreateSlider(Transform t, float x, float y, string prefix, float min, float max, float def, string postfix = null)
        {
            var rs = new UIItemComposite();
            if (prefix != null)
            {
                rs.Prefix = new UIItemText(t, x, y, prefix);
                rs.Prefix.Item.Align(TextAnchor.MiddleRight);
            }
            rs.MainComponent = new UIItemSlider(t, x + UIHelper.DELTA_X * 4, y, min, max, def);
            if (postfix != null)
            {
                rs.Postfix = new UIItemText(t, x + UIHelper.DELTA_X * 8, y, postfix);
                rs.Postfix.Item.Align(TextAnchor.MiddleLeft);
            }
            return rs;
        }

        public static UIItemComposite CreateToggle(Transform t, float x, float y, string prefix, bool def, string postfix = null)
        {
            var rs = new UIItemComposite();
            if (prefix != null)
            {
                rs.Prefix = new UIItemText(t, x, y, prefix);
                rs.Prefix.Item.Align(TextAnchor.MiddleRight);
            }
            rs.MainComponent = new UIItemToggle(t, x + UIHelper.DELTA_X * 2, y, def);
            if (postfix != null)
            {
                rs.Postfix = new UIItemText(t, x + UIHelper.DELTA_X * 4, y, postfix);
                rs.Postfix.Item.Align(TextAnchor.MiddleLeft);
            }
            return rs;
        }

        public override object Get()
        {
            return MainComponent.Get();
        }

        public override void Set(object input)
        {
            MainComponent.Set(input);
        }

        public override void Update()
        {
            base.Update();
            Prefix.Update();
            MainComponent.Update();
            Postfix.Update();

            if (HidePostfixIfDisabled)
                Postfix.Item.gameObject.SetActive(IsEnable());
        }

        public override void Destroy()
        {
            Prefix.Destroy();
            MainComponent.Destroy();
            Postfix.Destroy();
        }

        public override bool IsActive()
        {
            return MainComponent.IsActive();
        }

        public override bool IsEnable()
        {
            return MainComponent.IsEnable();
        }
    }
    #endregion
}