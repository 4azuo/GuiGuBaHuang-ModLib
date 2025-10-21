using ModLib.Helper;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static ModLib.Object.UIItemBase;

namespace ModLib.Object
{
    public class UIItemSelect : UIItem<Toggle>
    {
        public string[] Selections { get; set; }
        public List<Toggle> SelectionItems { get; } = new List<Toggle>();
        public int SelectedIndex { get; set; } = 0;
        public bool IsShownList => Item.isOn;

        public UIItemSelect(UICustomBase ui, float x, float y, string[] selections, int def, Toggle copySource = null) : base(ui, (copySource ?? UISampleHelper.SelectSample).Copy(ui.UIBase).Pos(x, y).Size(170f, 26f).Align().Format(Color.black, 13))
        {
            Init(selections, def);
        }

        protected virtual void Init(string[] selections, int def)
        {
            SetSelections(selections);
            SelectIndex(def, true);

            Item.onValueChanged.AddListener(ActionHelper.TracedUnityAction<bool>((v) => OnMainChanged(), fin: (v) => UI.UpdateUI()));
        }

        public void SetSelections(string[] selections)
        {
            //clear
            foreach (var item in SelectionItems)
            {
                UnityEngine.Object.Destroy(item);
            }
            SelectionItems.Clear();

            //add new
            Selections = selections;
            for (var i = 0; i < selections.Length; i++)
            {
                var comp = Item.Copy(UI.UIBase).Set(false, $"　{selections[i]}");
                comp.gameObject.SetActive(false);
                comp.onValueChanged.AddListener(ActionHelper.TracedUnityAction<bool>((v) => OnSubChanged(comp), fin: (v) => UI.UpdateUI()));
                SelectionItems.Add(comp);
            }
            UpdatePos();
        }

        public void SelectIndex(int index, bool ignoreNotify = false)
        {
            var isChanged = SelectedIndex != index;
            SelectedIndex = index;
            UpdateSelectedIndex();
            if (isChanged && !ignoreNotify)
                ItemWork?.ChangeAct?.Invoke(this, SelectedIndex);
        }

        public void UpdateSelectedIndex()
        {
            Item.Set(false, SelectionItems[SelectedIndex].GetComponentInChildren<Text>().text);
        }

        public override object Get()
        {
            return SelectedIndex;
        }

        public override void Set(object input)
        {
            SelectIndex((int)input);
        }

        public override void Dispose()
        {
            SetSelections(new string[0]);
            base.Dispose();

        }

        public void ShowList()
        {
            foreach (var comp in SelectionItems)
            {
                comp.transform.SetAsLastSibling();
                comp.gameObject.SetActive(true);
                comp.SetIsOnWithoutNotify(false);
            }
        }

        public void CloseList()
        {
            foreach (var comp in SelectionItems)
            {
                comp.gameObject.SetActive(false);
            }
        }

        private void OnMainChanged()
        {
            if (Item.isOn)
                ShowList();
            else
                CloseList();
        }

        private void OnSubChanged(Toggle comp)
        {
            SelectIndex(SelectionItems.IndexOf(comp));
            CloseList();
        }

        public override void Update()
        {
            base.Update();
            if (ItemWork?.EnableAct != null)
                Enable = ItemWork?.EnableAct?.Invoke(this) ?? false;
            Item.enabled = Enable;
        }

        public virtual void UpdatePos()
        {
            var y = (SelectionItems.FirstOrDefault()?.GetSize().y ?? 24) - 4;
            for (var i = 0; i < SelectionItems.Count; i++)
                SelectionItems[i].Pos(Item, 0f, -y * (i + 1));
        }

        public override Vector2 Pos()
        {
            return base.Pos();
        }

        public override UIItemBase Pos(float x, float y)
        {
            base.Pos(x, y);
            UpdatePos();
            return this;
        }

        public override UIItemBase Pos(int col, int row)
        {
            throw new System.NotImplementedException();
        }

        public override UIItemBase Pos(Vector2 org, float x, float y)
        {
            base.Pos(org, x, y);
            UpdatePos();
            return this;
        }

        public override UIItemBase Pos(GameObject org, float x, float y)
        {
            return Pos(org.Pos(), x, y);
        }

        public override UIItemBase Pos(Component org, float x, float y)
        {
            return Pos(org.gameObject, x, y);
        }

        public override UIItemBase Pos(UIItemBase org, float x, float y)
        {
            base.Pos(org, x, y);
            UpdatePos();
            return this;
        }

        public UIItemSelect Align(TextAnchor tanchor = TextAnchor.MiddleLeft, VerticalWrapMode vMode = VerticalWrapMode.Overflow, HorizontalWrapMode hMode = HorizontalWrapMode.Overflow)
        {
            Item.Align(tanchor, vMode, hMode);
            foreach (var comp in SelectionItems)
                comp.Align(tanchor, vMode, hMode);
            return this;
        }

        public UIItemSelect Format(Color? color = null, int fsize = 14, FontStyle fstype = FontStyle.Normal)
        {
            Item.Format(color, fsize, fstype);
            foreach (var comp in SelectionItems)
                comp.Format(color, fsize, fstype);
            return this;
        }

        public UIItemSelect Size(float scaleX = 0f, float scaleY = 0f)
        {
            Item.Size(scaleX, scaleY);
            foreach (var comp in SelectionItems)
                comp.Size(scaleX, scaleY);
            UpdatePos();
            return this;
        }

        public UIItemSelect AddSize(float scaleX = 0f, float scaleY = 0f)
        {
            Item.AddSize(scaleX, scaleY);
            foreach (var comp in SelectionItems)
                comp.AddSize(scaleX, scaleY);
            UpdatePos();
            return this;
        }

        public override UIItemBase SetParentTransform(Transform t)
        {
            foreach (var comp in SelectionItems)
            {
                comp.transform.SetParent(t);
            }
            base.SetParentTransform(t);
            UpdatePos();
            return this;
        }

        public override UIItemBase SetParentTransform(GameObject t)
        {
            return SetParentTransform(t.transform);
        }

        public override UIItemBase SetParentTransform(Component t)
        {
            return SetParentTransform(t.gameObject);
        }
    }
}