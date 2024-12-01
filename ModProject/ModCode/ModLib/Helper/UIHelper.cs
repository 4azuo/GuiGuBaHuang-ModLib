using Harmony;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using static UIItemBase;

public static class UIHelper
{
    public const float DELTA_X = 0.4f;
    public const float DELTA_Y = 0.25f;

    public static UIGameSetting _sampleUI;
    public static UIBehaviour _originComp;

    public abstract class UICustomBase
    {
        public IList<float> Columns { get; internal set; } = new List<float>();
        public IList<float> Rows { get; internal set; } = new List<float>();

        protected abstract float MinWidth();
        protected abstract float MaxWidth();
        protected abstract float MinHeight();
        protected abstract float MaxHeight();

        protected void InitGrid()
        {
            Columns = new List<float>();
            for (var i = MinWidth(); i <= MaxWidth(); i += DELTA_X)
                Columns.Add(i);
            Rows = new List<float>();
            for (var i = MinHeight(); i >= MaxHeight(); i -= DELTA_Y)
                Rows.Add(i);
        }

        public UICustomBase()
        {
            InitGrid();
        }
    }

    public abstract class UICustom<T> : UICustomBase where T : UIBase
    {
        public T UI { get; internal set; }
        public Canvas Canvas { get; internal set; }
        public List<UIItemBase> Items { get; } = new List<UIItemBase>();

        public UIItemText AddText(float x, float y, string format)
        {
            var rs = new UIItemText(Canvas.transform, x, y, format);
            Items.Add(rs);
            return rs;
        }

        public UIItemSlider AddSlider(float x, float y, float min, float max, float def)
        {
            var rs = new UIItemSlider(Canvas.transform, x, y, min, max, def);
            Items.Add(rs);
            return rs;
        }

        public UIItemToggle AddToggle(float x, float y, bool def)
        {
            var rs = new UIItemToggle(Canvas.transform, x, y, def);
            Items.Add(rs);
            return rs;
        }

        public UIItemButton AddButton(float x, float y, Action act, string format)
        {
            var rs = new UIItemButton(Canvas.transform, x, y, act, format);
            Items.Add(rs);
            return rs;
        }

        public UIItemComposite AddCompositeSlider(float x, float y, string prefix, float min, float max, float def, string postfix = null)
        {
            var rs = UIItemComposite.CreateSlider(Canvas.transform, x, y, prefix, min, max, def, postfix);
            Items.Add(rs);
            return rs;
        }

        public UIItemComposite AddCompositeToggle(float x, float y, string prefix, bool def, string postfix = null)
        {
            var rs = UIItemComposite.CreateToggle(Canvas.transform, x, y, prefix, def, postfix);
            Items.Add(rs);
            return rs;
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

        private void FixPosition(ref int col, ref int row)
        {
            col = col.FixValue(0, Columns.Count - 1);
            row = row.FixValue(0, Rows.Count - 1);
        }

        public void Clear()
        {
            foreach (var item in Items)
                item.Destroy();
            Items.Clear();
        }

        public void UpdateUI()
        {
            foreach (var item in Items)
                item.Update();
        }
    }

    public class UICustom1 : UICustom<UITextInfoLong>
    {
        protected override float MinWidth() => -6.0f;
        protected override float MaxWidth() => +6.0f;
        protected override float MinHeight() => -0.5f;
        protected override float MaxHeight() => -10.0f;

        public static UICustom1 Create(string title, Action okAct, bool showCancel = false, Action cancelAct = null)
        {
            var rs = new UICustom1();
            {
                _sampleUI = g.ui.OpenUI<UIGameSetting>(UIType.GameSetting);
                _sampleUI.gameObject.SetActive(false);
                {
                    rs.UI = g.ui.OpenUI<UITextInfoLong>(UIType.TextInfoLong);
                    _originComp = rs.UI.textTitle;
                    rs.Canvas = rs.UI.canvas;
                    {
                        rs.UI.InitData(title, string.Empty, isShowCancel: showCancel);
                        rs.UI.btnOK.onClick.AddListener((UnityAction)okAct);
                        if (cancelAct != null)
                            rs.UI.btnCancel.onClick.AddListener((UnityAction)cancelAct);
                    }
                }
                _sampleUI.gameObject.SetActive(true);
                g.ui.CloseUI(_sampleUI);
            }
            return rs;
        }
    }
}