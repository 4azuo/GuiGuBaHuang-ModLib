namespace ModLib.Object
{
    public class UICover<T> : UICustomBase where T : UIBase
    {
        public UIType.UITypeBase UITypeBase { get; private set; }
        protected override string UITypeName() => UITypeBase.uiName;
        protected override float MinWidth() => UIHelper.SCREEN_X_LEFT;
        protected override float MaxWidth() => UIHelper.SCREEN_X_RIGHT;
        protected override float MinHeight() => UIHelper.SCREEN_Y_TOP;
        protected override float MaxHeight() => UIHelper.SCREEN_Y_BOTTOM;

        public T UI { get; private set; }

        public UICover(UIBase ui) : base()
        {
            Init(ui.uiType);
        }

        public UICover(UIType.UITypeBase uiType) : base()
        {
            Init(uiType);
        }

        protected virtual void Init(UIType.UITypeBase uiType)
        {
            UITypeBase = uiType;

            //init
            UI = g.ui.GetUI<T>(UIType.GetUIType(UITypeName()));
            UIBase = UI;

            UIHelper.UIs.Add(this);
        }

        public override void Dispose()
        {
            UIHelper.UIs.Remove(this);
            Clear();
        }
    }
}