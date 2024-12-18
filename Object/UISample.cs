using System;

namespace ModLib.Object
{
    public abstract class UISample : IDisposable
    {
        public abstract void Dispose();
    }

    public class UISample1 : UISample
    {
        public UIGameSetting ui;

        public UISample1()
        {
            ui = g.ui.OpenUI<UIGameSetting>(UIType.GameSetting);
            ui.gameObject.SetActive(false);
        }

        public override void Dispose()
        {
            ui.gameObject.SetActive(true);
            g.ui.CloseUI(ui);
        }
    }

    public class UISample2 : UISample
    {
        public UIModMainShop ui;

        public UISample2()
        {
            ui = g.ui.OpenUI<UIModMainShop>(UIType.ModMainShop);
            ui.gameObject.SetActive(false);
        }

        public override void Dispose()
        {
            ui.gameObject.SetActive(true);
            g.ui.CloseUI(ui);
        }
    }

    public class UISample3 : UISample
    {
        public UIModWorkshopUpload ui;

        public UISample3()
        {
            ui = g.ui.OpenUI<UIModWorkshopUpload>(UIType.ModWorkshopUpload);
            ui.gameObject.SetActive(false);
        }

        public override void Dispose()
        {
            ui.gameObject.SetActive(true);
            g.ui.CloseUI(ui);
        }
    }
}