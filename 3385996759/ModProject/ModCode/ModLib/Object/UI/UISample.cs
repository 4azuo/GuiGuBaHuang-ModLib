using System;

namespace ModLib.Object
{
    public abstract class UISample<T> : IDisposable where T : UIBase
    {
        public virtual UIType.UITypeBase UISampleType { get; }

        public T ui;

        public UISample()
        {
            ui = g.ui.OpenUI<T>(UISampleType);
            ui.gameObject.SetActive(false);
        }

        public void Dispose()
        {
            ui.gameObject.SetActive(true);
            g.ui.CloseUI(ui);
        }
    }

    public class UISample1 : UISample<UIGameSetting>
    {
        public override UIType.UITypeBase UISampleType => UIType.GameSetting;
    }

    public class UISample2 : UISample<UIPropInfo>
    {
        public override UIType.UITypeBase UISampleType => UIType.PropInfo;
    }

    public class UISample3 : UISample<UIModWorkshopUpload>
    {
        public override UIType.UITypeBase UISampleType => UIType.ModWorkshopUpload;
    }
}