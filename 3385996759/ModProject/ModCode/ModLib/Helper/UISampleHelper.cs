using ModLib.Object;
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
            ButtonSample = s.ui.btnSystemOK.Copy(g.ui.canvas.transform);
            ButtonSample.gameObject.SetActive(false);
            SelectSample = s.ui.tglLanguage.Copy(g.ui.canvas.transform);
            SelectSample.gameObject.SetActive(false);
            SliderSample = s.ui.sliSoundMain.Copy(g.ui.canvas.transform);
            SliderSample.gameObject.SetActive(false);
            TextSample = s.ui.textSystemOK.Copy(g.ui.canvas.transform);
            TextSample.gameObject.SetActive(false);
            ToggleSample = s.ui.tglWindow.Copy(g.ui.canvas.transform);
            ToggleSample.gameObject.SetActive(false);
        }
        using (var s = new UISample<UIModWorkshopUpload>(UIType.ModWorkshopUpload))
        {
            InputSample = s.ui.iptDesc.Copy(g.ui.canvas.transform);
            InputSample.gameObject.SetActive(false);
        }
        using (var s = new UISample<UIPropInfo>(UIType.PropInfo))
        {
            ImageSample = s.ui.imgIcon.Copy(g.ui.canvas.transform);
            ImageSample.gameObject.SetActive(false);
        }
    }
}