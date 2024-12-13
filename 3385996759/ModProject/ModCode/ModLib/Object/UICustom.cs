﻿using System;
using UnityEngine.Events;

namespace ModLib.Object
{
    public abstract class UICustom<T> : UICustomBase where T : UIBase
    {
        public static UICustom<T> LastUICustom { get; private set; } = null;
        public T UI { get; private set; }
        public Action<UICustom<T>> InitComp { get; private set; }

        public UICustom(Action<UICustom<T>> initComp) : base()
        {
            //init
            DeleteLastUI();
            UI = g.ui.OpenUI<T>(UIType.GetUIType(UITypeName()));
            LastUICustom = this;
            UIBase = UI;

            InitComp = initComp;
            InitComp.Invoke(this);
            DeleteSampleUIs();

            //test
            //for (var c = 0; c < Columns.Count; c++)
            //    for (var r = 0; r < Rows.Count; r++)
            //        AddText(c, r, "test");

            UIHelper.UIs.Add(this);
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
            UIHelper.UIs.Remove(this);
            Clear();
            if (this?.UI?.uiType != null && g.ui.HasUI(this.UI.uiType))
                g.ui.CloseUI(this.UI);
        }
    }

    public class UICustom1 : UICustom<UITextInfoLong>
    {
        protected override string UITypeName() => UIType.TextInfoLong.uiName;
        protected override float MinWidth() => -6.2f;
        protected override float MaxWidth() => +6.6f;
        protected override float MinHeight() => 3.4f;
        protected override float MaxHeight() => -3.5f;

        public UICustom1(string title, Action<UICustom<UITextInfoLong>> initComp, Action okAct = null, bool showCancel = false, Action cancelAct = null) : base(initComp)
        {
            UI.InitData(title, string.Empty, isShowCancel: showCancel);
            if (okAct != null)
                UI.btnOK.onClick.AddListener((UnityAction)okAct);
            if (cancelAct != null)
                UI.btnCancel.onClick.AddListener((UnityAction)cancelAct);
        }
    }

    public class UICustom2 : UICustom<UITextInfo>
    {
        protected override string UITypeName() => UIType.TextInfo.uiName;
        protected override float MinWidth() => -2.6f;
        protected override float MaxWidth() => +3.0f;
        protected override float MinHeight() => 1.35f;
        protected override float MaxHeight() => -1.30f;

        public UICustom2(string title, Action<UICustom<UITextInfo>> initComp, Action okAct = null) : base(initComp)
        {
            UI.InitData(title, string.Empty);
            if (okAct != null)
                UI.btnOK.onClick.AddListener((UnityAction)okAct);
        }
    }
}