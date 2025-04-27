using System;

namespace ModLib.Object
{
    public class UICover<T> : UICustomBase where T : UIBase
    {
        public UIType.UITypeBase UITypeBase { get; private set; }
        public override string UITypeName => UITypeBase.uiName;
        public override float MinWidth => -(UIHelper.GetScreenWidth() / 2);
        public override float MaxWidth => +(UIHelper.GetScreenWidth() / 2);
        public override float MinHeight => +(UIHelper.GetScreenHeight() / 2);
        public override float MaxHeight => -(UIHelper.GetScreenHeight() / 2);

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
            UI = g.ui.GetUI<T>(UIType.GetUIType(UITypeName));
            UIBase = UI;

            UIHelper.UIs.Add(this);
            DebugHelper.WriteLine($"Create a cover for {UI.uiType.uiName}");
        }

        public override void Dispose()
        {
            DebugHelper.WriteLine($"Dispose the cover of {UI.uiType.uiName}");
            UIHelper.UIs.Remove(this);
            Clear();
            GC.Collect();
        }
    }
}