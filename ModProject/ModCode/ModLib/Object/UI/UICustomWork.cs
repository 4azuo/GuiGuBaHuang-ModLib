using System;

namespace ModLib.Object
{
    /// <summary>
    /// Callbacks of a custom UI (UpdateAct runs on every UpdateUI).
    /// </summary>
    public class UICustomWork
    {
        public virtual Action<UICustomBase> UpdateAct { get; set; }
    }
}