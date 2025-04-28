using ModLib.Object;
using UnityEngine;
using UnityEngine.UI;

public static class UISampleHelper
{
    public const string PERSISTENT_CANVAS_NAME = "PersistentCanvas";
    public static Button ButtonSample { get; private set; }
    public static Image ImageSample { get; private set; }
    public static InputField InputSample { get; private set; }
    public static Toggle SelectSample { get; private set; }
    public static Slider SliderSample { get; private set; }
    public static Text TextSample { get; private set; }
    public static Toggle ToggleSample { get; private set; }

    public static void LoadUISampples()
    {
        g.ui.Init((Il2CppSystem.Action)(() =>
        {
            // Create Canvas
            Il2CppSystem.Type canvasType = Il2CppSystem.Type.GetType("UnityEngine.Canvas, UnityEngine");
            var canvasGO = new GameObject(PERSISTENT_CANVAS_NAME, new Il2CppSystem.Type[] { canvasType });
            var canvas = canvasGO.GetComponent<Canvas>();

            // Prevent Canvas from being destroyed across scenes
            Object.DontDestroyOnLoad(canvasGO);

            // Hide this canvas
            canvas.gameObject.SetActive(false);

            using (var s = new UISample<UIGameSetting>(UIType.GameSetting))
            {
                ButtonSample = s.ui.btnSystemOK.Create(canvasGO.transform);
                SelectSample = s.ui.tglLanguage.Create(canvasGO.transform);
                SliderSample = s.ui.sliSoundMain.Create(canvasGO.transform);
                TextSample = s.ui.textSystemOK.Create(canvasGO.transform);
                ToggleSample = s.ui.tglWindow.Create(canvasGO.transform);
            }

            using (var s = new UISample<UIModWorkshopUpload>(UIType.ModWorkshopUpload))
            {
                InputSample = s.ui.iptDesc.Create(canvasGO.transform);
            }

            using (var s = new UISample<UIPropInfo>(UIType.PropInfo))
            {
                ImageSample = s.ui.imgIcon.Create(canvasGO.transform);
            }
        }));
    }
}