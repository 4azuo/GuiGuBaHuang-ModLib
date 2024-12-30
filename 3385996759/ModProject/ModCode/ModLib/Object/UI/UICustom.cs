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
            UI = g.ui.OpenUI<T>(UIType.GetUIType(UITypeName));
            LastUICustom = this;
            UIBase = UI;

            //navigation buttons
            UnPaging = true;
            {
                PrevButton = AddButton(FirstCol, LastRow, () => PrevPage(), "≪").Format(Color.black, 13).Size(60, 30).Active(IsShowNavigationButtons);
                NextButton = AddButton(LastCol, LastRow, () => NextPage(), "≫").Format(Color.black, 13).Size(60, 30).Active(IsShowNavigationButtons);
            }
            UnPaging = false;

            //test
            //for (var c = 0; c < Columns.Count; c++)
            //    for (var r = 0; r < Rows.Count; r++)
            //        AddText(c, r, "test");

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
        public override float MinWidth => -6.2f;
        public override float MaxWidth => +6.6f;
        public override float MinHeight => 3.4f;
        public override float MaxHeight => -3.5f;

        public UICustom1(string title, string btnText = "", Action okAct = null, bool showCancel = false, Action cancelAct = null) : base()
        {
            UI.InitData(title, string.Empty, btnText, okAct, showCancel);
            if (cancelAct != null)
                UI.btnCancel.onClick.AddListener((UnityAction)cancelAct);
        }
    }

    public class UICustom2 : UICustom<UITextInfo>
    {
        public override string UITypeName => UIType.TextInfo.uiName;
        public override float MinWidth => -2.6f;
        public override float MaxWidth => +3.0f;
        public override float MinHeight => 1.35f;
        public override float MaxHeight => -1.30f;

        public UICustom2(string title, string btnText = "", Action okAct = null) : base()
        {
            UI.InitData(title, string.Empty, btnText, okAct);
        }
    }
}