﻿using System;
using System.Collections.Generic;
using UnityEngine.Events;
using static UIItemBase;

public static class UIHelper
{
    public static List<UIItemBase> Items { get; } = new List<UIItemBase>();
    public static List<UIItemBase> AllItems { get; } = new List<UIItemBase>();

    public const float SCREEN_Y_TOP = 5f;
    public const float SCREEN_Y_MIDDLE = 0f;
    public const float SCREEN_Y_BOTTOM = -5f;
    public const float SCREEN_X_LEFT = -10f;
    public const float SCREEN_X_MID = 0f;
    public const float SCREEN_X_RIGHT = 10f;

    public static bool HasUI(this UIMgr g, UIType.UITypeBase t)
    {
        return (g.GetUI(t)?.gameObject?.active).Is(true) == 1;
    }

    public static bool IsExists(this UIBase ui)
    {
        return (ui?.gameObject?.active).Is(true) == 1;
    }

    public const float UICUSTOM_DELTA_X = 0.4f;
    public const float UICUSTOM_DELTA_Y = 0.24f;

    public abstract class UICustomBase
    {
        internal UISample uiSample;
        public UIBase UIBase { get; internal set; }
        public IList<float> Columns { get; internal set; } = new List<float>();
        public IList<float> Rows { get; internal set; } = new List<float>();
        public List<UIItemBase> Items { get; } = new List<UIItemBase>();

        protected abstract string UITypeName();
        protected abstract float MinWidth();
        protected abstract float MaxWidth();
        protected abstract float MinHeight();
        protected abstract float MaxHeight();
        public int FirstCol => 0;
        public int LastCol => Columns.Count - 1;
        public int MidCol => Columns.Count / 2;
        public int FirstRow => 0;
        public int LastRow => Rows.Count - 1;
        public int MidRow => Rows.Count / 2;

        protected void InitGrid()
        {
            Columns = new List<float>();
            for (var i = MinWidth(); i <= MaxWidth(); i += UICUSTOM_DELTA_X)
                Columns.Add(i);
            Rows = new List<float>();
            for (var i = MinHeight(); i >= MaxHeight(); i -= UICUSTOM_DELTA_Y)
                Rows.Add(i);
        }

        public UICustomBase()
        {
            InitGrid();
        }

        public UIItemText AddText(float x, float y, string format)
        {
            return new UIItemText(this, x, y, format);
        }

        public UIItemSlider AddSlider(float x, float y, float min, float max, float def)
        {
            return new UIItemSlider(this, x, y, min, max, def);
        }

        public UIItemToggle AddToggle(float x, float y, bool def)
        {
            return new UIItemToggle(this, x, y, def);
        }

        public UIItemButton AddButton(float x, float y, Action act, string format)
        {
            return new UIItemButton(this, x, y, act, format);
        }

        public UIItemSelect AddSelect(float x, float y, string[] selections, int def)
        {
            return new UIItemSelect(this, x, y, selections, def);
        }

        public UIItemComposite AddCompositeSlider(float x, float y, string prefix, float min, float max, float def, string postfix = null)
        {
            return UIItemComposite.CreateSlider(this, x, y, prefix, min, max, def, postfix);
        }

        public UIItemComposite AddCompositeToggle(float x, float y, string prefix, bool def, string postfix = null)
        {
            return UIItemComposite.CreateToggle(this, x, y, prefix, def, postfix);
        }

        public UIItemComposite AddCompositeSelect(float x, float y, string prefix, string[] selections, int def, string postfix = null)
        {
            return UIItemComposite.CreateSelect(this, x, y, prefix, selections, def, postfix);
        }

        public UIItemText AddText(int col, int row, string format)
        {
            FixPosition(ref col, ref row);
            return AddText(Columns[col], Rows[row], format);
        }

        public UIItemSlider AddSlider(int col, int row, float min, float max, float def)
        {
            FixPosition(ref col, ref row);
            return AddSlider(Columns[col], Rows[row], min, max, def);
        }

        public UIItemToggle AddToggle(int col, int row, bool def)
        {
            FixPosition(ref col, ref row);
            return AddToggle(Columns[col], Rows[row], def);
        }

        public UIItemButton AddButton(int col, int row, Action act, string format)
        {
            FixPosition(ref col, ref row);
            return AddButton(Columns[col], Rows[row], act, format);
        }

        public UIItemSelect AddSelect(int col, int row, string[] selections, int def)
        {
            FixPosition(ref col, ref row);
            return AddSelect(Columns[col], Rows[row], selections, def);
        }

        public UIItemComposite AddCompositeSlider(int col, int row, string prefix, float min, float max, float def, string postfix = null)
        {
            FixPosition(ref col, ref row);
            return AddCompositeSlider(Columns[col], Rows[row], prefix, min, max, def, postfix);
        }

        public UIItemComposite AddCompositeToggle(int col, int row, string prefix, bool def, string postfix = null)
        {
            FixPosition(ref col, ref row);
            return AddCompositeToggle(Columns[col], Rows[row], prefix, def, postfix);
        }

        public UIItemComposite AddCompositeSelect(int col, int row, string prefix, string[] selections, int def, string postfix = null)
        {
            FixPosition(ref col, ref row);
            return AddCompositeSelect(Columns[col], Rows[row], prefix, selections, def, postfix);
        }

        private void FixPosition(ref int col, ref int row)
        {
            col = col.FixValue(0, Columns.Count - 1);
            row = row.FixValue(0, Rows.Count - 1);
        }

        public void Clear()
        {
            foreach (var item in Items)
                item.Destroy();
        }

        public void UpdateUI()
        {
            foreach (var item in Items)
                item.Update();
        }
    }

    public class UICustom : UICustomBase
    {
        public UIType.UITypeBase UITypeBase { get; private set; }
        protected override string UITypeName() => UITypeBase.uiName;
        protected override float MinWidth() => SCREEN_X_LEFT;
        protected override float MaxWidth() => SCREEN_X_RIGHT;
        protected override float MinHeight() => SCREEN_Y_TOP;
        protected override float MaxHeight() => SCREEN_Y_BOTTOM;

        public static UICustom LastUICustom { get; private set; }
        public UIBase UI { get; internal set; }
        public Action<UICustom> InitComp { get; internal set; }

        public UICustom(UIType.UITypeBase uiType, Action<UICustom> initComp) : base()
        {
            try
            {
                UITypeBase = uiType;

                //init
                DeleteLastUI();
                UI = g.ui.GetUI(UIType.GetUIType(UITypeName()));
                LastUICustom = this;
                UIBase = UI;

                InitComp = initComp;

                using (uiSample = new UISample())
                {
                    InitComp.Invoke(this);
                }

                //test
                //for (var c = 0; c < Columns.Count; c++)
                //    for (var r = 0; r < Rows.Count; r++)
                //        AddText(c, r, "test");
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine(ex);
            }
        }

        private void DeleteLastUI()
        {
            if (LastUICustom != null)
            {
                Clear();
                LastUICustom = null;
            }
        }

        public void Close()
        {
            DeleteLastUI();
        }
    }

    public abstract class UICustom<T> : UICustomBase where T : UIBase
    {
        public static UICustom<T> LastUICustom { get; private set; }
        public T UI { get; private set; }
        public Action<UICustom<T>> InitComp { get; private set; }

        public UICustom(Action<UICustom<T>> initComp) : base()
        {
            try
            {
                //init
                DeleteLastUI();
                UI = g.ui.OpenUI<T>(UIType.GetUIType(UITypeName()));
                LastUICustom = this;
                UIBase = UI;

                InitComp = initComp;

                using (uiSample = new UISample())
                {
                    InitComp.Invoke(this);
                }

                //test
                //for (var c = 0; c < Columns.Count; c++)
                //    for (var r = 0; r < Rows.Count; r++)
                //        AddText(c, r, "test");
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine(ex);
            }
        }

        private void DeleteLastUI()
        {
            if (LastUICustom != null)
            {
                Clear();
                if (LastUICustom?.UI?.uiType != null && g.ui.HasUI(LastUICustom.UI.uiType))
                    g.ui.CloseUI(LastUICustom.UI);
                LastUICustom = null;
            }
        }

        public void Close()
        {
            DeleteLastUI();
        }
    }

    public class UISample : IDisposable
    {
        public UIGameSetting ui;

        public UISample()
        {
            ui = g.ui.OpenUI<UIGameSetting>(UIType.GameSetting);
            ui.gameObject.SetActive(false);
        }

        public void Dispose()
        {
            ui.gameObject.SetActive(true);
            g.ui.CloseUI(ui);
        }
    }

    public class UICustom1 : UICustom<UITextInfoLong>
    {
        protected override string UITypeName() => UIType.TextInfoLong.uiName;
        protected override float MinWidth() => -6.2f;
        protected override float MaxWidth() => +6.6f;
        protected override float MinHeight() => 3.4f;
        protected override float MaxHeight() => -3.5f;

        public UICustom1(string title, Action<UICustom<UITextInfoLong>> initComp, Action okAct, bool showCancel = false, Action cancelAct = null) : base(initComp)
        {
            UI.InitData(title, string.Empty, isShowCancel: showCancel);
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

        public UICustom2(string title, Action<UICustom<UITextInfo>> initComp, Action okAct) : base(initComp)
        {
            UI.InitData(title, string.Empty);
            UI.btnOK.onClick.AddListener((UnityAction)okAct);
        }
    }
}