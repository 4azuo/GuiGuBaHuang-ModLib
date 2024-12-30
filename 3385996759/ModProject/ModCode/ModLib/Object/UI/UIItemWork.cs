using System;

namespace ModLib.Object
{
    public class UIItemWork
    {
        public UIItemBase Item { get; set; }
        public virtual Action<UIItemBase> UpdateAct { get; set; }
        public virtual Func<UIItemBase, bool> EnableAct { get; set; }
        public virtual Func<UIItemBase, object[]> Formatter { get; set; }
        public virtual Action<UIItemBase, object> ChangeAct { get; set; }
    }
}