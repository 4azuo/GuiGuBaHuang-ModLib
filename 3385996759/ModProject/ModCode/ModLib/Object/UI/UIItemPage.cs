using System;
using System.Collections.Generic;

namespace ModLib.Object
{
    /// <summary>
    /// A page of items in a custom UI; only the active page is visible.
    /// </summary>
    public class UIItemPage
    {
        public UICustomBase Parent { get; private set; }
        public Action<UICustomBase> Loader { get; private set; }
        public bool IsLoaded { get; private set; }
        public List<UIItemBase> Items { get; } = new List<UIItemBase>();

        /// <summary>
        /// Creates the item and registers it in the owner UI (and the current page when paging is used).
        /// </summary>
        public UIItemPage(UICustomBase parent)
        {
            Parent = parent;
            IsLoaded = true;
        }

        /// <summary>
        /// Creates the item and registers it in the owner UI (and the current page when paging is used).
        /// </summary>
        public UIItemPage(UICustomBase parent, Action<UICustomBase> loader)
        {
            Parent = parent;
            Loader = loader;
            IsLoaded = false;
        }

        /// <summary>
        /// Shows this page (loading it lazily) and hides the others.
        /// </summary>
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