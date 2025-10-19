using ModLib.Helper;

namespace ModLib.Object
{
    public class UIItemWork
    {
        public UIItemBase Item { get; set; }
        public virtual ActionHelper.TracedAction<UIItemBase> UpdateAct { get; set; }
        public virtual ActionHelper.TracedFunc<UIItemBase, bool> EnableAct { get; set; }
        public virtual ActionHelper.TracedFunc<UIItemBase, object[]> Formatter { get; set; }
        public virtual ActionHelper.TracedAction<UIItemBase, object> ChangeAct { get; set; }
    }
}