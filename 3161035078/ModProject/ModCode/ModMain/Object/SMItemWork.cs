using ModLib.Object;
using System;

namespace MOD_nE7UL2.Object
{
    public class SMItemWork : UIItemWork
    {
        public Func<UIItemBase> Comp { get; set; }
        public Func<UIItemBase, int> Cal { get; set; }
        public Func<UIItemBase, bool> Cond { get; set; }
    }
}
