using System;

namespace ModLib.Object
{
    /// <summary>
    /// Custom data attached to an item.
    /// </summary>
    public class UIItemData
    {
        public UIItemBase Item { get; set; }
        public dynamic CustomData { get; set; }
    }
}