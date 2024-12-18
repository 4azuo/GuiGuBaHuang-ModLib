using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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

        public UIItemSelect(UICustomBase ui, float x, float y, string[] selections, int def, Toggle copySource = null) : base(ui, (copySource ?? ui.UISample1.ui.tglLanguage).Copy(ui.UIBase).Pos(x + 0.15f, y - 0.01f).Size(160f, 28f).Align().Format(Color.black, 14))
        {
            Init(selections, def);
        }

        protected virtual void Init(string[] selections, int def)
        {
            SetSelections(selections);
            SelectIndex(def);

            Item.onValueChanged.AddListener((UnityAction<bool>)(v => OnMainChanged()));
        }

        public void SetSelections(string[] selections)
        {
            //clear
            foreach (var item in SelectionItems)
            {
                MonoBehaviour.Destroy(item);
            }
            SelectionItems.Clear();

            //add new
            Selections = selections;
            for (var i = 0; i < selections.Length; i++)
            {
                var comp = Item.Copy(UI.UIBase).Pos(Item.transform, 0f, -(Item.GetSize().y / 120f) * (i + 1)).Set(false, $"　{selections[i]}");
                comp.group = null;
                comp.gameObject.SetActive(false);
                comp.onValueChanged.AddListener((UnityAction<bool>)(v => OnSubChanged(comp)));
                SelectionItems.Add(comp);
            }
        }

        public void SelectIndex(int index)
        {
            var isChanged = SelectedIndex != index;
            SelectedIndex = index;
            UpdateSelectedIndex();
            if (isChanged)
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

        public override void Destroy()
        {
            SetSelections(new string[0]);
            base.Destroy();

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

        public override void Pos(float x, float y)
        {
            base.Pos(x, y);
            for (var i = 0; i < SelectionItems.Count; i++)
                SelectionItems[i].Pos(Item.transform, 0f, -(Item.GetSize().y / 120f) * (i + 1));
        }

        public override void Pos(UIItemBase org, float x, float y)
        {
            base.Pos(org, x, y);
            for (var i = 0; i < SelectionItems.Count; i++)
                SelectionItems[i].Pos(Item.transform, 0f, -(Item.GetSize().y / 120f) * (i + 1));
        }

        public override void Update()
        {
            base.Update();
            if (ItemWork?.EnableAct != null)
                Enable = ItemWork?.EnableAct?.Invoke(this) ?? false;
            Item.enabled = Enable;
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
            return this;
        }

        public UIItemSelect AddSize(float scaleX = 0f, float scaleY = 0f)
        {
            Item.AddSize(scaleX, scaleY);
            foreach (var comp in SelectionItems)
                comp.AddSize(scaleX, scaleY);
            return this;
        }
    }
}