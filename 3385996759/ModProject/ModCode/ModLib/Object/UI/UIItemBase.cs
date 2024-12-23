﻿using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ModLib.Object
{
    public abstract class UIItemBase : IDisposable
    {
        protected EventTrigger _trigger;
        public virtual EventTrigger Trigger
        {
            get
            {
                if (_trigger == null)
                    _trigger = Component.gameObject.AddComponent<EventTrigger>();
                return _trigger;
            }
        }

        public virtual Text InnerText => null;
        public virtual bool HasText => InnerText != null;
        public virtual UICustomBase UI { get; set; }
        public virtual UIItemBase Parent { get; set; }
        public virtual Component Component { get; set; }
        public virtual UIItemData ItemData { get; set; }
        public virtual UIItemWork ItemWork { get; set; }
        public virtual bool Enable { get; set; } = true;
        public virtual object Tag { get; set; }

        public abstract object Get();

        public abstract void Set(object input);

        public abstract bool IsActive();

        public virtual void Dispose()
        {
            UI.Items.Remove(this);
            UnityEngine.Object.DestroyImmediate(Component);
        }

        public virtual void Update()
        {
            if (ItemWork?.UpdateAct != null)
                ItemWork?.UpdateAct?.Invoke(this);
        }

        public UIItemBase(UICustomBase ui)
        {
            UI = ui;
        }

        public UIItemBase(UICustomBase ui, Component comp)
        {
            UI = ui;
            Component = comp;
        }

        public UIItemBase SetData(UIItemData data)
        {
            ItemData = data;
            ItemData.Item = this;
            return this;
        }

        public UIItemBase SetWork(UIItemWork wk)
        {
            ItemWork = wk;
            ItemWork.Item = this;
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

        public virtual Vector3 Pos()
        {
            return Component.Pos();
        }

        public virtual UIItemBase Pos(float x, float y)
        {
            Component.Pos(x, y);
            return this;
        }

        public virtual UIItemBase Pos(int col, int row)
        {
            this.UI.FixPosition(ref col, ref row);
            return Pos(this.UI.Columns[col], this.UI.Rows[row]);
        }

        public virtual UIItemBase Pos(Transform org, float x, float y)
        {
            Component.Pos(org, x, y);
            return this;
        }

        public virtual UIItemBase Pos(GameObject org, float x, float y)
        {
            return Pos(org.transform, x, y);
        }

        public virtual UIItemBase Pos(UIItemBase org, float x, float y)
        {
            return Pos(org.Component.transform, x, y);
        }

        public virtual UIItemBase SetParentTransform(Transform t)
        {
            Component.transform.SetParent(t);
            return this;
        }

        public virtual UIItemBase SetParentTransform(GameObject t)
        {
            return SetParentTransform(t.transform);
        }

        #region Data
        public class UIItemData
        {
            public UIItemBase Item { get; set; }
            public dynamic CustomData { get; set; }
        }
        #endregion

        #region Work
        public class UIItemWork
        {
            public UIItemBase Item { get; set; }
            public virtual Action<UIItemBase> UpdateAct { get; set; }
            public virtual Func<UIItemBase, bool> EnableAct { get; set; }
            public virtual Func<UIItemBase, object[]> Formatter { get; set; }
            public virtual Action<UIItemBase, object> ChangeAct { get; set; }
        }
        #endregion

        #region Item
        public class UIItem : UIItemBase
        {
            public UIBehaviour Item { get; set; }

            public UIItem(UICustomBase ui, UIBehaviour comp) : base(ui, comp)
            {
                Item = comp;
                UI.Items.Add(this);
            }

            public override bool IsActive()
            {
                return Item.IsActive();
            }

            public override object Get()
            {
                return Item;
            }

            public override void Set(object input)
            {
                Item = (UIBehaviour)input;
            }
        }

        public abstract class UIItem<T> : UIItemBase where T : UIBehaviour
        {
            public T Item { get; private set; }

            public UIItem(UICustomBase ui, T comp) : base(ui, comp)
            {
                Item = comp;
                UI.Items.Add(this);
            }

            public override bool IsActive()
            {
                return Item.IsActive();
            }
        }
        #endregion
    }
}