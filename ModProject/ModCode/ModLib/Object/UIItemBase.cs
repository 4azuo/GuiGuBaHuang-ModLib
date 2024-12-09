﻿using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class UIItemBase
{
    public virtual UIBase UI { get; private set; }
    public virtual UIItemBase Parent { get; private set; }
    public virtual UIBehaviour ItemBehaviour { get; private set; }
    public virtual UIItemData ItemData { get; set; }
    public virtual UIItemWork ItemWork { get; set; }
    public virtual bool Enable { get; set; } = true;
    public virtual object Tag { get; set; }

    public virtual void Update() { }
    public abstract object Get();
    public abstract void Set(object input);
    public abstract bool IsActive();
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

    public virtual bool IsEnable()
    {
        return Enable;
    }

    public virtual void SetEnable(bool value)
    {
        Enable = value;
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
        public virtual Func<UIItemBase, bool> EnableAct { get; set; }
        public virtual Func<UIItemBase, object[]> Formatter { get; set; }
        public virtual Action<UIItemBase, object> ChangeAct { get; set; }
    }
    #endregion

    #region Item
    public abstract class UIItem<T> : UIItemBase where T : UIBehaviour
    {
        public T Item { get; private set; }

        public UIItem(UIBase ui, T comp)
        {
            UI = ui;
            Item = comp;
            ItemBehaviour = Item;
        }

        public override bool IsActive()
        {
            return Item.IsActive();
        }

        public override void Destroy()
        {
            MonoBehaviour.Destroy(Item);
        }
    }

    public class UIItemText : UIItem<Text>
    {
        public string FormatStr { get; private set; }
        public Color Color { get; private set; }

        public UIItemText(UIBase ui, float x, float y, string format) : base(ui, UIHelper._sampleUI.textSystemOK.Copy(ui).Pos(UIHelper._originComp.gameObject, x, y))
        {
            FormatStr = format;
            Item.Set(Get()?.ToString());
            Item.Align(TextAnchor.MiddleCenter);
            Item.Format();
            Color = Item.color;
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
            Item.Set(Get()?.ToString());
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

    public class UIItemSlider : UIItem<Slider>
    {
        public float Min { get; private set; }
        public float Max { get; private set; }

        public UIItemSlider(UIBase ui, float x, float y, float min, float max, float def) : base(ui, UIHelper._sampleUI.sliSoundMain.Copy(ui).Pos(UIHelper._originComp.gameObject, x, y))
        {
            Min = min;
            Max = max;
            Item.Set(min, max, def);
            Item.onValueChanged.AddListener((UnityAction<float>)(v =>
            {
                ItemWork?.ChangeAct?.Invoke(this, v);
            }));
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

    public class UIItemToggle : UIItem<Toggle>
    {
        public UIItemToggle(UIBase ui, float x, float y, bool def) : base(ui, UIHelper._sampleUI.tglWindow.Copy(ui).Pos(UIHelper._originComp.gameObject, x, y))
        {
            CompHelper.Set(Item, def);
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
            CompHelper.Set(Item, (bool)input);
        }

        public override void Update()
        {
            base.Update();
            if (ItemWork?.EnableAct != null)
                Enable = ItemWork?.EnableAct?.Invoke(this) ?? false;
            Item.enabled = Enable;
        }
    }

    public class UIItemButton : UIItem<Button>
    {
        public string FormatStr { get; private set; }
        public Text ButtonLabel { get; private set; }

        public UIItemButton(UIBase ui, float x, float y, Action act, string format) : base(ui, UIHelper._sampleUI.btnSystemOK.Copy(ui).Pos(UIHelper._originComp.gameObject, x, y))
        {
            FormatStr = format;
            ButtonLabel = Item.GetComponentInChildren<Text>();
            ButtonLabel.Set(Get()?.ToString());
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

    public class UIItemComposite : UIItemBase
    {
        public UIItemText Prefix { get; private set; }
        public UIItemBase MainComponent { get; private set; }
        public UIItemText Postfix { get; private set; }
        public bool HidePostfixIfDisabled { get; set; } = false;

        public override UIBase UI => MainComponent.UI;
        public override UIItemBase Parent => MainComponent.Parent;
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
        public override bool Enable { get => MainComponent.Enable; set => MainComponent.Enable = value; }
        public override object Tag { get => MainComponent.Tag; set => MainComponent.Tag = value; }

        private UIItemComposite() { }

        public static UIItemComposite CreateSlider(UIBase ui, float x, float y, string prefix, float min, float max, float def, string postfix = null)
        {
            var rs = new UIItemComposite();
            rs.Prefix = new UIItemText(ui, x, y, prefix);
            rs.Prefix.Item.Align(TextAnchor.MiddleRight);
            rs.Prefix.Parent = rs;
            rs.MainComponent = new UIItemSlider(ui, x + UIHelper.DELTA_X * 4, y, min, max, def);
            rs.MainComponent.Parent = rs;
            rs.Postfix = new UIItemText(ui, x + UIHelper.DELTA_X * 8, y, postfix);
            rs.Postfix.Item.Align(TextAnchor.MiddleLeft);
            rs.Postfix.Parent = rs;
            return rs;
        }

        public static UIItemComposite CreateToggle(UIBase ui, float x, float y, string prefix, bool def, string postfix = null)
        {
            var rs = new UIItemComposite();
            rs.Prefix = new UIItemText(ui, x, y, prefix);
            rs.Prefix.Item.Align(TextAnchor.MiddleRight);
            rs.Prefix.Parent = rs;
            rs.MainComponent = new UIItemToggle(ui, x + UIHelper.DELTA_X * 2, y, def);
            rs.MainComponent.Parent = rs;
            rs.Postfix = new UIItemText(ui, x + UIHelper.DELTA_X * 4, y, postfix);
            rs.Postfix.Item.Align(TextAnchor.MiddleLeft);
            rs.Postfix.Parent = rs;
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

        public void SetPercent(float percent, float min = float.MinValue, float max = float.MaxValue)
        {
            (MainComponent as UIItemSlider)?.SetPercent(percent, min, max);
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

        public override void SetEnable(bool value)
        {
            Prefix.SetEnable(value);
            MainComponent.SetEnable(value);
            Postfix.SetEnable(value);
        }
    }
    #endregion
}