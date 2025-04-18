using ModLib.Object;
using UnityEngine;
using UnityEngine.UI;

public static class UISampleHelper
{
    public static Button ButtonSample { get; private set; }
    public static Image ImageSample { get; private set; }
    public static InputField InputSample { get; private set; }
    public static Toggle SelectSample { get; private set; }
    public static Slider SliderSample { get; private set; }
    public static Text TextSample { get; private set; }
    public static Toggle ToggleSample { get; private set; }

    public static void LoadUISampples()
    {
        using (var s = new UISample<UIGameSetting>(UIType.GameSetting))
        {
            ButtonSample = s.ui.btnSystemOK.Create();
            Object.DontDestroyOnLoad(ButtonSample);
            SelectSample = s.ui.tglLanguage.Create();
            Object.DontDestroyOnLoad(SelectSample);
            SliderSample = s.ui.sliSoundMain.Create();
            Object.DontDestroyOnLoad(SliderSample);
            TextSample = s.ui.textSystemOK.Create();
            Object.DontDestroyOnLoad(TextSample);
            ToggleSample = s.ui.tglWindow.Create();
            Object.DontDestroyOnLoad(ToggleSample);
        }
        using (var s = new UISample<UIModWorkshopUpload>(UIType.ModWorkshopUpload))
        {
            InputSample = s.ui.iptDesc.Create();
            Object.DontDestroyOnLoad(InputSample);
        }
        using (var s = new UISample<UIPropInfo>(UIType.PropInfo))
        {
            ImageSample = s.ui.imgIcon.Create();
            Object.DontDestroyOnLoad(ImageSample);
        }
    }
}