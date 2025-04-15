using ModLib.Object;
using UnityEngine;
using UnityEngine.UI;

public static class UISampleHelper
{
    private static Button _ButtonSample;
    private static Image _ImageSample;
    private static InputField _InputSample;
    private static Toggle _SelectSample;
    private static Slider _SliderSample;
    private static Text _TextSample;
    private static Toggle _ToggleSample;

    public static Button ButtonSample
    {
        get
        {
            if (_ButtonSample == null)
            {
                using (var s = new UISample<UIGameSetting>(UIType.GameSetting))
                {
                    _ButtonSample = s.ui.btnSystemOK.Create();
                    Object.DontDestroyOnLoad(_ButtonSample);
                }
            }
            return _ButtonSample;
        }
    }
    public static Image ImageSample
    {
        get
        {
            if (_ImageSample == null)
            {
                using (var s = new UISample<UIPropInfo>(UIType.PropInfo))
                {
                    _ImageSample = s.ui.imgIcon.Create();
                    Object.DontDestroyOnLoad(_ImageSample);
                }
            }
            return _ImageSample;
        }
    }
    public static InputField InputSample
    {
        get
        {
            if (_InputSample == null)
            {
                using (var s = new UISample<UIModWorkshopUpload>(UIType.ModWorkshopUpload))
                {
                    _InputSample = s.ui.iptDesc.Create();
                    Object.DontDestroyOnLoad(_InputSample);
                }
            }
            return _InputSample;
        }
    }
    public static Toggle SelectSample
    {
        get
        {
            if (_SelectSample == null)
            {
                using (var s = new UISample<UIGameSetting>(UIType.GameSetting))
                {
                    _SelectSample = s.ui.tglLanguage.Create();
                    Object.DontDestroyOnLoad(_SelectSample);
                }
            }
            return _SelectSample;
        }
    }
    public static Slider SliderSample
    {
        get
        {
            if (_SliderSample == null)
            {
                using (var s = new UISample<UIGameSetting>(UIType.GameSetting))
                {
                    _SliderSample = s.ui.sliSoundMain.Create();
                    Object.DontDestroyOnLoad(_SliderSample);
                }
            }
            return _SliderSample;
        }
    }
    public static Text TextSample
    {
        get
        {
            if (_TextSample == null)
            {
                using (var s = new UISample<UIGameSetting>(UIType.GameSetting))
                {
                    _TextSample = s.ui.textSystemOK.Create();
                    Object.DontDestroyOnLoad(_TextSample);
                }
            }
            return _TextSample;
        }
    }
    public static Toggle ToggleSample
    {
        get
        {
            if (_ToggleSample == null)
            {
                using (var s = new UISample<UIGameSetting>(UIType.GameSetting))
                {
                    _ToggleSample = s.ui.tglWindow.Create();
                    Object.DontDestroyOnLoad(_ToggleSample);
                }
            }
            return _ToggleSample;
        }
    }
}