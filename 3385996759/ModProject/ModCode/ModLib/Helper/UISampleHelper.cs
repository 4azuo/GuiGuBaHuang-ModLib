using ModLib.Attributes;
using ModLib.Object;
using UnityEngine;
using UnityEngine.UI;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for accessing sample UI components.
    /// Provides template UI elements (buttons, images, inputs, etc.) for cloning in custom UIs.
    /// </summary>
    [ActionCatIgn]
    public static class UISampleHelper
    {
        public const string PERSISTENT_CANVAS_NAME = "PersistentCanvas";
        /// <summary>Sample button for cloning</summary>
        public static Button ButtonSample { get; private set; }
        /// <summary>Sample image for cloning</summary>
        public static Image ImageSample { get; private set; }
        /// <summary>Sample input field for cloning</summary>
        public static InputField InputSample { get; private set; }
        /// <summary>Sample select (toggle) for cloning</summary>
        public static Toggle SelectSample { get; private set; }
        /// <summary>Sample slider for cloning</summary>
        public static Slider SliderSample { get; private set; }
        /// <summary>Sample text for cloning</summary>
        public static Text TextSample { get; private set; }
        /// <summary>Sample toggle for cloning</summary>
        public static Toggle ToggleSample { get; private set; }

        /// <summary>
        /// Loads UI sample components from game UIs for cloning.
        /// Called during mod initialization.
        /// </summary>
        public static void LoadUISampples()
        {
            g.ui.Init(ActionHelper.TracedIl2Action(() =>
            {
                // Create Canvas
                Il2CppSystem.Type canvasType = Il2CppSystem.Type.GetType("UnityEngine.Canvas, UnityEngine");
                var canvasGO = new GameObject(PERSISTENT_CANVAS_NAME, new Il2CppSystem.Type[] { canvasType });
                var canvas = canvasGO.GetComponent<Canvas>();

                // Prevent Canvas from being destroyed across scenes
                UnityEngine.Object.DontDestroyOnLoad(canvasGO);

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
}