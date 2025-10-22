using ModLib.Helper;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace ModLib.Object
{
    public abstract class UICustom<T> : UICustomBase where T : UIBase
    {
        public static UICustom<T> LastUICustom { get; private set; } = null;
        public T UI { get; private set; }

        public UICustom() : base()
        {
            //init
            DeleteLastUI();
            UI = g.ui.OpenUISafe<T>(UIType.GetUIType(UITypeName));
            LastUICustom = this;
            UIBase = UI;

            //navigation buttons
            UnPaging = true;
            {
                PrevButton = AddButton(FirstCol, LastRow, () => PrevPage(), GameTool.LS("libtxt999990008")).Format(Color.black, 13).Size(80, 40)
                    .Active(IsShowNavigationButtons).SetWork(new UIItemWork
                {
                    Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>((item) => new object[] { CurrentPageIndex + 0 })
                });
                NextButton = AddButton(LastCol, LastRow, () => NextPage(), GameTool.LS("libtxt999990008")).Format(Color.black, 13).Size(80, 40)
                    .Active(IsShowNavigationButtons).SetWork(new UIItemWork
                {
                    Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>((item) => new object[] { CurrentPageIndex + 2 })
                });
            }
            UnPaging = false;

            //test
            //for (var c = 0; c < Columns.Count; c++)
            //    for (var r = 0; r < Rows.Count; r++)
            //        AddText(c, r, $"{c}/{r}");

            UIHelper.UIs.Add(this);
            DebugHelper.WriteLine($"Create a UICustom for {UI.uiType.uiName}");
        }

        private void DeleteLastUI()
        {
            if (LastUICustom != null)
            {
                LastUICustom.Dispose();
                LastUICustom = null;
            }
        }

        public override void Dispose()
        {
            DebugHelper.WriteLine($"Dispose the UICustom of {UI.uiType.uiName}");
            UIHelper.UIs.Remove(this);
            Clear();
            if (this?.UI?.uiType != null && g.ui.HasUI(this.UI.uiType))
                g.ui.CloseUI(this.UI);
            GC.Collect();
        }
    }

    public class UICustom1 : UICustom<UITextInfoLong>
    {
        public override string UITypeName => UIType.TextInfoLong.uiName;
        public override float MinWidth => -(UIHelper.GetUIScreenWidth() / 2) * 0.630f;
        public override float MaxWidth => +(UIHelper.GetUIScreenWidth() / 2) * 0.660f;
        public override float MinHeight => +(UIHelper.GetUIScreenHeight() / 2) * 0.640f;
        public override float MaxHeight => -(UIHelper.GetUIScreenHeight() / 2) * 0.660f;

        public UICustom1(string title, string btnText = "", Action okAct = null, bool showCancel = false, Action cancelAct = null) : base()
        {
            UI.InitData(title, string.Empty, btnText, okAct, showCancel);
            if (cancelAct != null)
                UI.btnCancel.onClick.AddListener(ActionHelper.TracedUnityAction(cancelAct));
        }
    }

    public class UICustom2 : UICustom<UITextInfo>
    {
        public override string UITypeName => UIType.TextInfo.uiName;
        public override float MinWidth => -(UIHelper.GetUIScreenWidth() / 2) * 0.270f;
        public override float MaxWidth => +(UIHelper.GetUIScreenWidth() / 2) * 0.300f;
        public override float MinHeight => +(UIHelper.GetUIScreenHeight() / 2) * 0.200f;
        public override float MaxHeight => -(UIHelper.GetUIScreenHeight() / 2) * 0.200f;

        public UICustom2(string title, string btnText = "", Action okAct = null) : base()
        {
            UI.InitData(title, string.Empty, btnText, okAct);
        }
    }
}