using System;

namespace ModLib.Object
{
    public abstract class UISample<T> : IDisposable where T : UIBase
    {
        protected UIType.UITypeBase _uiSampleType;
        protected abstract UIType.UITypeBase GetUISampleType();
        public virtual UIType.UITypeBase UISampleType
        {
            get
            {
                if (_uiSampleType == null)
                    _uiSampleType = GetUISampleType();
                return _uiSampleType;
            }
        }

        public T ui;

        public UISample()
        {
            ui = g.ui.OpenUI<T>(UISampleType);
            ui.gameObject.SetActive(false);
        }

        public virtual void Dispose()
        {
            ui.gameObject.SetActive(true);
            g.ui.CloseUI(ui);
        }
    }

    public class UISample1 : UISample<UIGameSetting>
    {
        protected override UIType.UITypeBase GetUISampleType()
        {
            return new UIType.UITypeBase(UIType.GameSetting.uiName, UILayer.TempUI);
        }
    }

    public class UISample2 : UISample<UIPropInfo>
    {
        protected override UIType.UITypeBase GetUISampleType()
        {
            return new UIType.UITypeBase(UIType.PropInfo.uiName, UILayer.TempUI);
        }
    }

    public class UISample3 : UISample<UIModWorkshopUpload>
    {
        protected override UIType.UITypeBase GetUISampleType()
        {
            return new UIType.UITypeBase(UIType.ModWorkshopUpload.uiName, UILayer.TempUI);
        }
    }
}