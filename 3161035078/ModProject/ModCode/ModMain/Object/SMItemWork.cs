using ModLib.Helper;
using ModLib.Object;

namespace MOD_nE7UL2.Object
{
    public class SMItemWork : UIItemWork
    {
        public ActionHelper.TracedFunc<UIItemBase> Comp { get; set; }
        public ActionHelper.TracedFunc<UIItemBase, int> Cal { get; set; }
        public ActionHelper.TracedFunc<UIItemBase, bool> Cond { get; set; }
    }
}
