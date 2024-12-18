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

        public static UICover<T> LastUICover { get; private set; }
        public T UI { get; private set; }
        public Action<UICover<T>> InitComp { get; private set; }

        public UICover(UIType.UITypeBase uiType, Action<UICover<T>> initComp) : base()
        {
            UITypeBase = uiType;

            //init
            DeleteLastUI();
            UI = g.ui.GetUI<T>(UIType.GetUIType(UITypeName()));
            LastUICover = this;
            UIBase = UI;
            UIHelper.UIs.Add(this);

            InitComp = initComp;
            InitComp.Invoke(this);
            DeleteSampleUIs();

            //test
            //for (var c = 0; c < Columns.Count; c++)
            //    for (var r = 0; r < Rows.Count; r++)
            //        AddText(c, r, "test");
        }

        private void DeleteLastUI()
        {
            if (LastUICover != null)
            {
                Clear();
                UIHelper.UIs.Add(LastUICover);
                LastUICover = null;
            }
        }

        public void Close()
        {
            DeleteLastUI();
            UIHelper.UIs.Add(this);
        }
    }
}