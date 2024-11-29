using System;
using UnityEngine.EventSystems;

namespace MOD_nE7UL2.Object
{
    public class SMItemBase
    {
        public SMItem<T> Try<T>() where T : UIBehaviour
        {
            return this as SMItem<T>;
        }
    }

    public class SMItem<T> : SMItemBase where T : UIBehaviour
    {
        public Func<T> Comp { get; set; }
        public Func<T, bool> Cond { get; set; }
        public Func<T, int> Cal { get; set; }
    }
}
