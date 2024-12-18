using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ModLib.Object
{
    public abstract class UIItemBase
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
        public virtual UIBehaviour ItemBehaviour { get; set; }
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

        public virtual Vector3 Pos()
        {
            return ItemBehaviour.Pos();
        }

        public virtual void Pos(float x, float y)
        {
            ItemBehaviour.Pos(x, y);
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
            public virtual Func<UIItemBase, bool> EnableAct { get; set; }
            public virtual Func<UIItemBase, object[]> Formatter { get; set; }
            public virtual Action<UIItemBase, object> ChangeAct { get; set; }
        }
        #endregion

        #region Item
        public abstract class UIItem<T> : UIItemBase where T : UIBehaviour
        {
            public T Item { get; private set; }

            public UIItem(UICustomBase ui, T comp)
            {
                UI = ui;
                Item = comp;
                ItemBehaviour = Item;

                UI.Items.Add(this);
                UIHelper.Items.Add(this);
                UIHelper.AllItems.Add(this);
            }

            public override bool IsActive()
            {
                return Item.IsActive();
            }

            public override void Destroy()
            {
                UI.Items.Remove(this);
                UIHelper.Items.Remove(this);
                UIHelper.AllItems.Remove(this);
                MonoBehaviour.Destroy(Item);
            }
        }
        #endregion
    }
}