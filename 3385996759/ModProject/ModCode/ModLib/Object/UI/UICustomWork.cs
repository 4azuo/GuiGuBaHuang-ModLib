using System;

namespace ModLib.Object
{
    public class UICustomWork
    {
        public virtual Action<UICustomBase> UpdateAct { get; set; }
    }
}