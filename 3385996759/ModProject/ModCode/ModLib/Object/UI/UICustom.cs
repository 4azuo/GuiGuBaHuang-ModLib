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
                PrevButton = AddButton(FirstCol, LastRow, () => PrevPage(), "≪").Format(Color.black, 13).Size(60, 30).Active(IsShowNavigationButtons);
                NextButton = AddButton(LastCol, LastRow, () => NextPage(), "≫").Format(Color.black, 13).Size(60, 30).Active(IsShowNavigationButtons);
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
        public override float MinWidth => -(UIHelper.GetScreenWidth() / 2) * 0.62f;
        public override float MaxWidth => +(UIHelper.GetScreenWidth() / 2) * 0.66f;
        public override float MinHeight => +(UIHelper.GetScreenHeight() / 2) * 0.34f;
        public override float MaxHeight => -(UIHelper.GetScreenHeight() / 2) * 0.35f;

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
        public override float MinWidth => -(UIHelper.GetScreenWidth() / 2) * 0.26f;
        public override float MaxWidth => +(UIHelper.GetScreenWidth() / 2) * 0.30f;
        public override float MinHeight => +(UIHelper.GetScreenHeight() / 2) * 0.135f;
        public override float MaxHeight => -(UIHelper.GetScreenHeight() / 2) * 0.130f;

        public UICustom2(string title, string btnText = "", Action okAct = null) : base()
        {
            UI.InitData(title, string.Empty, btnText, okAct);
        }
    }
}