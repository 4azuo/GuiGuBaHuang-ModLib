using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for managing custom UI elements.
    /// Provides utilities for opening, closing, and tracking custom UI windows.
    /// </summary>
    [ActionCat("UI")]
    public static class UIHelper
    {
        public static List<UICustomBase> UIs { get; } = new List<UICustomBase>();

        private const float UI_DELTA_X_RATE = 0.0240f;
        private const float UI_DELTA_Y_RATE = 0.0240f;

        /// <summary>
        /// Gets the screen width from game settings.
        /// </summary>
        /// <returns>Screen width</returns>
        public static float GetSettingScreenWidth()
        {
            return g.data.globle.gameSetting.screenWidth;
        }

        /// <summary>
        /// Gets the screen height from game settings.
        /// </summary>
        /// <returns>Screen height</returns>
        public static float GetSettingScreenHeight()
        {
            return g.data.globle.gameSetting.screenHeight; ;
        }

        /// <summary>
        /// Gets the UI canvas width.
        /// </summary>
        /// <returns>Canvas width</returns>
        public static float GetUIScreenWidth()
        {
            return g.ui.canvas.GetComponent<RectTransform>().sizeDelta.x;
        }

        /// <summary>
        /// Gets the UI canvas height.
        /// </summary>
        /// <returns>Canvas height</returns>
        public static float GetUIScreenHeight()
        {
            return g.ui.canvas.GetComponent<RectTransform>().sizeDelta.y;
        }

        /// <summary>
        /// Gets the horizontal UI delta offset.
        /// </summary>
        /// <returns>Delta X</returns>
        public static float GetUIDeltaX()
        {
            return GetUIScreenWidth() * UI_DELTA_X_RATE;
        }

        /// <summary>
        /// Gets the vertical UI delta offset.
        /// </summary>
        /// <returns>Delta Y</returns>
        public static float GetUIDeltaY()
        {
            return GetUIScreenHeight() * UI_DELTA_Y_RATE;
        }

        /// <summary>
        /// Opens UI with safe error handling.
        /// </summary>
        /// <typeparam name="T">UI type</typeparam>
        /// <param name="g">UI manager</param>
        /// <param name="t">UI type base</param>
        /// <returns>Opened UI</returns>
        public static T OpenUISafe<T>(this UIMgr g, UIType.UITypeBase t) where T : UIBase
        {
            var ui = g.OpenUI<T>(t);
            //...
            return ui;
        }

        /// <summary>
        /// Checks if UI is currently open.
        /// </summary>
        /// <param name="g">UI manager</param>
        /// <param name="t">UI type</param>
        /// <returns>True if open</returns>
        public static bool HasUI(this UIMgr g, UIType.UITypeBase t)
        {
            return (g.GetUI(t)?.gameObject?.active).Is(true) == 1;
        }

        /// <summary>
        /// Checks if UI exists and is active.
        /// </summary>
        /// <param name="ui">UI instance</param>
        /// <returns>True if exists</returns>
        public static bool IsExists(this UIBase ui)
        {
            return (ui?.gameObject?.active).Is(true) == 1;
        }

        /// <summary>
        /// Shows a message box dialog.
        /// </summary>
        /// <param name="g">UI manager</param>
        /// <param name="title">Title text</param>
        /// <param name="content">Content text</param>
        /// <param name="type">Button type</param>
        /// <param name="onYesCall">Yes button callback</param>
        /// <param name="onNoCall">No button callback</param>
        public static void MsgBox(this UIMgr g, string title, string content, MsgBoxButtonEnum type = MsgBoxButtonEnum.Ok, Action onYesCall = null, Action onNoCall = null)
        {
            var uiConfirm = g.OpenUISafe<UICheckPopup>(UIType.CheckPopup);
            uiConfirm.InitData(title, content, type.Parse<int>(),
                ActionHelper.TracedIl2Action(() => onYesCall?.Invoke()),
                ActionHelper.TracedIl2Action(() => onNoCall?.Invoke()));
        }

        /// <summary>
        /// Updates all custom UIs matching predicate.
        /// </summary>
        /// <param name="predicate">Filter predicate (null = all)</param>
        public static void UpdateAllUI(Func<UICustomBase, bool> predicate = null)
        {
            if (predicate == null)
                predicate = (x) => true;
            foreach (var ui in UIs.Where(predicate).ToArray())
            {
                try
                {
                    if (g.ui.HasUI(ui.UIBase.uiType))
                        ui?.UpdateUI();
                    else
                        ui?.Dispose();
                }
                catch
                {
                    ui?.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets all custom UIs of specified type.
        /// </summary>
        /// <param name="uiType">UI type</param>
        /// <returns>List of custom UIs</returns>
        public static List<UICustomBase> GetUICustomBase(UIType.UITypeBase uiType)
        {
            return UIs.Where(x => x.UITypeName == uiType.uiName).ToList();
        }

        /// <summary>
        /// Disposes all UIs in collection.
        /// </summary>
        /// <param name="uis">UIs to dispose</param>
        public static void Dispose(this IEnumerable<UICustomBase> uis)
        {
            foreach (var ui in uis)
            {
                ui.Dispose();
            }
        }
    }
}