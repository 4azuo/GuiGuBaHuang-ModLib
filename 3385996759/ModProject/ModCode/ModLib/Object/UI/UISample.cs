﻿using System;

namespace ModLib.Object
{
    public class UISample<T> : IDisposable where T : UIBase
    {
        public T ui;

        public UISample(UIType.UITypeBase uiType)
        {
            ui = g.ui.OpenUI<T>(uiType);
            ui.Init();
            ui.gameObject.SetActive(false);
        }

        public void Dispose()
        {
            ui.gameObject.SetActive(true);
            g.ui.CloseUI(ui);
        }
    }
}