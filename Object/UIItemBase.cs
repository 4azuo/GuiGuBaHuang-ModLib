using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ModLib.Object
{
    public abstract class UIItemBase : IDisposable
    {
        public interface ITextFormat
        {
            string GetFormat();
            void SetFormat(string format);
        }

        public virtual Text InnerText => null;
        public virtual bool HasText => InnerText != null;
        public virtual UICustomBase UI { get; set; }
        public virtual UIItemBase Parent { get; set; }
        public virtual Component ItemBehaviour { get; set; }
        public virtual UIItemData ItemData { get; set; }
        public virtual UIItemWork ItemWork { get; set; }
        public virtual bool Enable { get; set; } = true;
        public virtual object Tag { get; set; }

        public abstract object Get();

        public abstract void Set(object input);

        public abstract bool IsActive();

        public abstract void Dispose();

        public virtual void Update()
        {
            if (ItemWork?.UpdateAct != null)
                ItemWork?.UpdateAct?.Invoke(this);
        }

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

        public virtual Vector3 Pos()
        {
            return ItemBehaviour.Pos();
        }

        public virtual void Pos(float x, float y)
        {
            ItemBehaviour.Pos(x, y);
        }

        public virtual void Pos(int col, int row)
        {
            this.UI.FixPosition(ref col, ref row);
            ItemBehaviour.Pos(this.UI.Columns[col], this.UI.Rows[row]);
        }

        public virtual void Pos(UIItemBase org, float x, float y)
        {
            ItemBehaviour.Pos(org.ItemBehaviour.transform, x, y);
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

            public UIItem(UICustomBase ui, UIBehaviour comp)
            {
                UI = ui;
                Item = comp;
                ItemBehaviour = Item;

                UI.Items.Add(this);
            }

            public override bool IsActive()
            {
                return Item.IsActive();
            }

            public override void Dispose()
            {
                UI.Items.Remove(this);
                MonoBehaviour.DestroyImmediate(Item);
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

            public UIItem(UICustomBase ui, T comp)
            {
                UI = ui;
                Item = comp;
                ItemBehaviour = Item;

                UI.Items.Add(this);
            }

            public override bool IsActive()
            {
                return Item.IsActive();
            }

            public override void Dispose()
            {
                UI.Items.Remove(this);
                MonoBehaviour.DestroyImmediate(Item);
            }
        }
        #endregion
    }
}