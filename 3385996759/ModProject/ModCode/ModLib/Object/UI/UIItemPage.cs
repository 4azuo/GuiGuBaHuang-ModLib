using System;
using System.Collections.Generic;

namespace ModLib.Object
{
    public class UIItemPage
    {
        public UICustomBase Parent { get; private set; }
        public Action<UICustomBase> Loader { get; private set; }
        public bool IsLoaded { get; private set; }
        public List<UIItemBase> Items { get; } = new List<UIItemBase>();

        public UIItemPage(UICustomBase parent)
        {
            Parent = parent;
            IsLoaded = true;
        }

        public UIItemPage(UICustomBase parent, Action<UICustomBase> loader)
        {
            Parent = parent;
            Loader = loader;
            IsLoaded = false;
        }

        public void Active()
        {
            Parent.CurrentPage = this;
            foreach (var page in Parent.Pages)
            {
                if (page == this)
                {
                    if (!IsLoaded)
                    {
                        IsLoaded = true;
                        Loader.Invoke(Parent);
                    }
                    foreach (var item in page.Items)
                    {
                        item.Active(true);
                    }
                }
                else
                {
                    foreach (var item in page.Items)
                    {
                        item.Active(false);
                    }
                }
            }
            Parent.PrevButton?.Active(Parent.IsShowNavigationButtons && Parent.CurrentPageIndex > 0);
            Parent.NextButton?.Active(Parent.IsShowNavigationButtons && Parent.CurrentPageIndex < Parent.Pages.Count - 1);
            Parent.UpdateUI();
        }
    }
}