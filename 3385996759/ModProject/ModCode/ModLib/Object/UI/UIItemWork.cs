using System;

namespace ModLib.Object
{
    /// <summary>
    /// Should use ActionHelper to create traced actions and funcs
    /// </summary>
    public class UIItemWork
    {
        public UIItemBase Item { get; set; }
        public virtual Action<UIItemBase> UpdateAct { get; set; }
        public virtual Func<UIItemBase, bool> EnableAct { get; set; }
        public virtual Func<UIItemBase, object[]> Formatter { get; set; }
        public virtual Action<UIItemBase, object> ChangeAct { get; set; }
    }
}