using static ModLib.Helper.ActionHelper;

namespace ModLib.Object
{
    /// <summary>
    /// Callbacks of an item: Formatter (text args), ChangeAct (value changed), UpdateAct, EnableAct, ActiveAct.
    /// </summary>
    public class UIItemWork
    {
        public UIItemBase Item { get; set; }
        public virtual TracedActionWrapper<UIItemBase> UpdateAct { get; set; }
        public virtual TracedFuncWrapper<UIItemBase, bool> EnableAct { get; set; }
        public virtual TracedFuncWrapper<UIItemBase, object[]> Formatter { get; set; }
        public virtual TracedActionWrapper<UIItemBase, object> ChangeAct { get; set; }
    }
}