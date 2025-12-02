using System;
using ModLib.Helper;

namespace ModLib.Object
{
    public class UICover<T> : UICustomBase where T : UIBase
    {
        public UIType.UITypeBase UITypeBase { get; private set; }
        public override string UITypeName => UITypeBase.uiName;
        public override float MinWidth => -(UIHelper.GetUIScreenWidth() / 2);
        public override float MaxWidth => +(UIHelper.GetUIScreenWidth() / 2);
        public override float MinHeight => +(UIHelper.GetUIScreenHeight() / 2);
        public override float MaxHeight => -(UIHelper.GetUIScreenHeight() / 2);

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

            //test
            //for (var c = 0; c < Columns.Count; c++)
            //    for (var r = 0; r < Rows.Count; r++)
            //        AddText(c, r, $"{c}/{r}");

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