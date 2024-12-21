using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ModLib.Object.UIItemBase;

namespace ModLib.Object
{
    public abstract class UICustomBase : IDisposable
    {
        public UIBase UIBase { get; set; }
        public List<float> Columns { get; } = new List<float>();
        public List<float> Rows { get; } = new List<float>();
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
            for (var i = MinWidth(); i <= MaxWidth(); i += UIHelper.UICUSTOM_DELTA_X)
                Columns.Add(i);
            for (var i = MinHeight(); i >= MaxHeight(); i -= UIHelper.UICUSTOM_DELTA_Y)
                Rows.Add(i);
        }

        public UICustomBase()
        {
            InitGrid();
        }

        public abstract void Dispose();

        public UIItem Add(UIBehaviour comp)
        {
            return new UIItem(this, comp);
        }

        public UIItemText AddText(float x, float y, string format, Text copySource = null)
        {
            return new UIItemText(this, x, y, format, copySource);
        }

        public UIItemSlider AddSlider(float x, float y, float min, float max, float def, Slider copySource = null)
        {
            return new UIItemSlider(this, x, y, min, max, def, copySource);
        }

        public UIItemToggle AddToggle(float x, float y, bool def, Toggle copySource = null)
        {
            return new UIItemToggle(this, x, y, def, copySource);
        }

        public UIItemButton AddButton(float x, float y, Action act, string format, Button copySource = null)
        {
            return new UIItemButton(this, x, y, act, format, copySource);
        }

        public UIItemSelect AddSelect(float x, float y, string[] selections, int def, Toggle copySource = null)
        {
            return new UIItemSelect(this, x, y, selections, def, copySource);
        }

        public UIItemImage AddImage(float x, float y, Sprite def, Image copySource = null)
        {
            return new UIItemImage(this, x, y, def, copySource);
        }

        public UIItemInput AddInput(float x, float y, string def, InputField copySource = null)
        {
            return new UIItemInput(this, x, y, def, copySource);
        }

        public UIItemComposite AddCompositeSlider(float x, float y, string prefix, float min, float max, float def, string postfix = null, Slider copySource = null)
        {
            return UIItemComposite.CreateSlider(this, x, y, prefix, min, max, def, postfix, copySource);
        }

        public UIItemComposite AddCompositeToggle(float x, float y, string prefix, bool def, string postfix = null, Toggle copySource = null)
        {
            return UIItemComposite.CreateToggle(this, x, y, prefix, def, postfix, copySource);
        }

        public UIItemComposite AddCompositeSelect(float x, float y, string prefix, string[] selections, int def, string postfix = null, Toggle copySource = null)
        {
            return UIItemComposite.CreateSelect(this, x, y, prefix, selections, def, postfix, copySource);
        }

        public UIItemComposite AddCompositeInput(float x, float y, string prefix, string def, string postfix = null, InputField copySource = null)
        {
            return UIItemComposite.CreateInput(this, x, y, prefix, def, postfix, copySource);
        }

        public UIItemText AddText(int col, int row, string format, Text copySource = null)
        {
            FixPosition(ref col, ref row);
            return AddText(Columns[col], Rows[row], format, copySource);
        }

        public UIItemSlider AddSlider(int col, int row, float min, float max, float def, Slider copySource = null)
        {
            FixPosition(ref col, ref row);
            return AddSlider(Columns[col], Rows[row], min, max, def, copySource);
        }

        public UIItemToggle AddToggle(int col, int row, bool def, Toggle copySource = null)
        {
            FixPosition(ref col, ref row);
            return AddToggle(Columns[col], Rows[row], def, copySource);
        }

        public UIItemButton AddButton(int col, int row, Action act, string format, Button copySource = null)
        {
            FixPosition(ref col, ref row);
            return AddButton(Columns[col], Rows[row], act, format, copySource);
        }

        public UIItemSelect AddSelect(int col, int row, string[] selections, int def, Toggle copySource = null)
        {
            FixPosition(ref col, ref row);
            return AddSelect(Columns[col], Rows[row], selections, def, copySource);
        }

        public UIItemImage AddImage(int col, int row, Sprite def, Image copySource = null)
        {
            FixPosition(ref col, ref row);
            return AddImage(Columns[col], Rows[row], def, copySource);
        }

        public UIItemInput AddInput(int col, int row, string def, InputField copySource = null)
        {
            FixPosition(ref col, ref row);
            return AddInput(Columns[col], Rows[row], def, copySource);
        }

        public UIItemComposite AddCompositeSlider(int col, int row, string prefix, float min, float max, float def, string postfix = null, Slider copySource = null)
        {
            FixPosition(ref col, ref row);
            return AddCompositeSlider(Columns[col], Rows[row], prefix, min, max, def, postfix, copySource);
        }

        public UIItemComposite AddCompositeToggle(int col, int row, string prefix, bool def, string postfix = null, Toggle copySource = null)
        {
            FixPosition(ref col, ref row);
            return AddCompositeToggle(Columns[col], Rows[row], prefix, def, postfix, copySource);
        }

        public UIItemComposite AddCompositeSelect(int col, int row, string prefix, string[] selections, int def, string postfix = null, Toggle copySource = null)
        {
            FixPosition(ref col, ref row);
            return AddCompositeSelect(Columns[col], Rows[row], prefix, selections, def, postfix, copySource);
        }

        public UIItemComposite AddCompositeInput(int col, int row, string prefix, string def, string postfix = null, InputField copySource = null)
        {
            FixPosition(ref col, ref row);
            return AddCompositeInput(Columns[col], Rows[row], prefix, def, postfix, copySource);
        }

        public void FixPosition(ref int col, ref int row)
        {
            col = col.FixValue(0, Columns.Count - 1);
            row = row.FixValue(0, Rows.Count - 1);
        }

        public void Clear()
        {
            foreach (var item in Items.ToArray())
            {
                item?.Dispose();
            }
        }

        public void UpdateUI()
        {
            foreach (var item in Items.ToArray())
            {
                item?.Update();
            }
        }
    }
}