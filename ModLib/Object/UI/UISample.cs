using System;
using ModLib.Helper;

namespace ModLib.Object
{
    /// <summary>
    /// Hidden UI holding sample components that new items are copied from.
    /// </summary>
    public class UISample<T> : IDisposable where T : UIBase
    {
        public T ui;

        /// <summary>
        /// Opens the sample UI.
        /// </summary>
        public UISample(UIType.UITypeBase uiType)
        {
            ui = g.ui.OpenUISafe<T>(uiType);
            ui.gameObject.SetActive(false);
        }

        /// <summary>
        /// Closes the sample UI.
        /// </summary>
        public void Dispose()
        {
            ui.gameObject.SetActive(true);
            g.ui.CloseUI(ui);
        }
    }
}