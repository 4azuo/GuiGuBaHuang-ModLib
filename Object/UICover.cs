using System;

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
        public Action<UICover<T>> InitComp { get; private set; }

        public UICover(UIBase ui, Action<UICover<T>> initComp) : base()
        {
            Init(ui.uiType, initComp);
        }

        public UICover(UIType.UITypeBase uiType, Action<UICover<T>> initComp) : base()
        {
            Init(uiType, initComp);
        }

        protected virtual void Init(UIType.UITypeBase uiType, Action<UICover<T>> initComp)
        {
            UITypeBase = uiType;

            //init
            UI = g.ui.GetUI<T>(UIType.GetUIType(UITypeName()));
            UIBase = UI;

            InitComp = initComp;
            InitComp.Invoke(this);
            DeleteSampleUIs();

            UIHelper.UIs.Add(this);
        }

        public override void Dispose()
        {
            UIHelper.UIs.Remove(this);
            Clear();
        }
    }
}