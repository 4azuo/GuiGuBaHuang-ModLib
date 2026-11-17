using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ModLib.Helper;

namespace ModLib.Object
{
    /// <summary>
    /// Base of all custom UI items: owner UI, data/work callbacks, position and visibility helpers.
    /// </summary>
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

        /// <summary>
        /// Returns the current value of the component.
        /// </summary>
        public abstract object Get();

        /// <summary>
        /// Sets the value of the component.
        /// </summary>
        public abstract void Set(object input);

        /// <summary>
        /// Shows or hides the component (fluent).
        /// </summary>
        public abstract UIItemBase Active(bool flg);

        /// <summary>
        /// True when the component is visible.
        /// </summary>
        public abstract bool IsActive();

        /// <summary>
        /// Destroys the created Unity objects and unregisters the item/UI.
        /// </summary>
        public virtual void Dispose()
        {
            UI.Items.Remove(this);
            foreach (var page in UI.Pages)
                page.Items.Remove(this);
            try
            {
                if (Component == null || Component.gameObject == null)
                    return; // already destroyed with its UI
                var go = Component.gameObject;
                go.SetActive(false);
                // a clone made by CompHelper (Create/Copy/Replace): the whole object (its texts, images...);
                // a component of the game wrapped by UICustomBase.Add: only the component, as before
                if (go.name.StartsWith(CompHelper.CLONE_PREFIX))
                    UnityEngine.Object.Destroy(go);
                else
                    UnityEngine.Object.Destroy(Component);
            }
            catch
            {
                // the Unity object was destroyed meanwhile (its UI closed)
            }
            finally
            {
                Component = null;
            }
        }

        /// <summary>
        /// Refreshes the component from its UIItemData/UIItemWork (formatter, enable/visible conditions).
        /// </summary>
        public virtual void Update()
        {
            if (ItemWork?.UpdateAct != null)
                ItemWork?.UpdateAct?.Invoke(this);
        }

        /// <summary>
        /// Registers the item in its owner UI.
        /// </summary>
        public UIItemBase(UICustomBase ui)
        {
            UI = ui;
        }

        /// <summary>
        /// Registers the item in its owner UI.
        /// </summary>
        public UIItemBase(UICustomBase ui, Component comp)
        {
            UI = ui;
            Component = comp;
            if (!ui.UnPaging)
                UI.AddPageItem(this);
        }

        /// <summary>
        /// Attaches custom data (fluent).
        /// </summary>
        public UIItemBase SetData(UIItemData data)
        {
            ItemData = data;
            ItemData.Item = this;
            return this;
        }

        /// <summary>
        /// Attaches callbacks: formatter, change/update actions, enable/active conditions (fluent).
        /// </summary>
        public UIItemBase SetWork(UIItemWork wk)
        {
            ItemWork = wk;
            ItemWork.Item = this;
            return this;
        }

        /// <summary>
        /// True when the component is interactable.
        /// </summary>
        public virtual bool IsEnable()
        {
            return Enable;
        }

        /// <summary>
        /// Enables or disables interaction with the component.
        /// </summary>
        public virtual void SetEnable(bool value)
        {
            Enable = value;
        }

        /// <summary>
        /// Gets or moves the component position: absolute coordinates, grid cell, or offset relative to another object (fluent).
        /// </summary>
        public virtual Vector2 Pos()
        {
            return Component.Pos();
        }

        /// <summary>
        /// Gets or moves the component position: absolute coordinates, grid cell, or offset relative to another object (fluent).
        /// </summary>
        public virtual UIItemBase Pos(float x, float y)
        {
            Component.Pos(x, y);
            return this;
        }

        /// <summary>
        /// Gets or moves the component position: absolute coordinates, grid cell, or offset relative to another object (fluent).
        /// </summary>
        public virtual UIItemBase Pos(int col, int row)
        {
            this.UI.FixPosition(ref col, ref row);
            return Pos(this.UI.Columns[col], this.UI.Rows[row]);
        }

        /// <summary>
        /// Gets or moves the component position: absolute coordinates, grid cell, or offset relative to another object (fluent).
        /// </summary>
        public virtual UIItemBase Pos(Vector2 org, float x, float y)
        {
            Component.Pos(org, x, y);
            return this;
        }

        /// <summary>
        /// Gets or moves the component position: absolute coordinates, grid cell, or offset relative to another object (fluent).
        /// </summary>
        public virtual UIItemBase Pos(GameObject org, float x, float y)
        {
            return Pos(org.Pos(), x, y);
        }

        /// <summary>
        /// Gets or moves the component position: absolute coordinates, grid cell, or offset relative to another object (fluent).
        /// </summary>
        public virtual UIItemBase Pos(Component org, float x, float y)
        {
            return Pos(org.gameObject, x, y);
        }

        /// <summary>
        /// Gets or moves the component position: absolute coordinates, grid cell, or offset relative to another object (fluent).
        /// </summary>
        public virtual UIItemBase Pos(UIItemBase org, float x, float y)
        {
            return Pos(org.Component, x, y);
        }

        /// <summary>
        /// Re-parents the component under another transform (fluent).
        /// </summary>
        public virtual UIItemBase SetParentTransform(Transform t)
        {
            Component.transform.SetParent(t);
            return this;
        }

        /// <summary>
        /// Re-parents the component under another transform (fluent).
        /// </summary>
        public virtual UIItemBase SetParentTransform(GameObject t)
        {
            return SetParentTransform(t.transform);
        }

        /// <summary>
        /// Re-parents the component under another transform (fluent).
        /// </summary>
        public virtual UIItemBase SetParentTransform(Component t)
        {
            return SetParentTransform(t.gameObject);
        }

        #region Item
        /// <summary>
        /// Item wrapping an arbitrary Unity component.
        /// </summary>
        public class UIItem : UIItemBase
        {
            public UIBehaviour Item { get; set; }

            /// <summary>
            /// Wraps a component.
            /// </summary>
            public UIItem(UICustomBase ui, UIBehaviour comp) : base(ui, comp)
            {
                Item = comp;
                UI.Items.Add(this);
            }

            /// <summary>
            /// Shows or hides the component (fluent).
            /// </summary>
            public override UIItemBase Active(bool flg)
            {
                Item.gameObject.SetActive(flg);
                return this;
            }

            /// <summary>
            /// True when the component is visible.
            /// </summary>
            public override bool IsActive()
            {
                return Item.IsActive();
            }

            /// <summary>
            /// Returns the current value of the component.
            /// </summary>
            public override object Get()
            {
                return Item;
            }

            /// <summary>
            /// Sets the value of the component.
            /// </summary>
            public override void Set(object input)
            {
                Item = (UIBehaviour)input;
            }
        }

        /// <summary>
        /// Item wrapping an arbitrary Unity component.
        /// </summary>
        public abstract class UIItem<T> : UIItemBase where T : UIBehaviour
        {
            public T Item { get; private set; }

            /// <summary>
            /// Wraps a component.
            /// </summary>
            public UIItem(UICustomBase ui, T comp) : base(ui, comp)
            {
                Item = comp;
                UI.Items.Add(this);
            }

            /// <summary>
            /// Shows or hides the component (fluent).
            /// </summary>
            public override UIItemBase Active(bool flg)
            {
                Item.gameObject.SetActive(flg);
                return this;
            }

            /// <summary>
            /// True when the component is visible.
            /// </summary>
            public override bool IsActive()
            {
                return Item.IsActive();
            }
        }
        #endregion
    }
}