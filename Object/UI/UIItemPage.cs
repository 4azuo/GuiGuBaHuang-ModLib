using System.Collections.Generic;

namespace ModLib.Object
{
    public class UIItemPage
    {
        public UICustomBase Parent { get; private set; }
        public List<UIItemBase> Items { get; } = new List<UIItemBase>();

        public UIItemPage(UICustomBase parent)
        {
            Parent = parent;
        }

        public void Active()
        {
            foreach (var page in Parent.Pages)
            {
                if (page == this)
                {
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
            Parent.UpdateUI();
        }
    }
}