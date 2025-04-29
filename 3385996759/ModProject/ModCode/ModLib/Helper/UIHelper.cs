using ModLib.Enum;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class UIHelper
{
    public static List<UICustomBase> UIs { get; } = new List<UICustomBase>();

    private const float UI_DELTA_X_RATE = 0.0240f;
    private const float UI_DELTA_Y_RATE = 0.0240f;

    public static float GetSettingScreenWidth()
    {
        return g.data.globle.gameSetting.screenWidth;
    }

    public static float GetSettingScreenHeight()
    {
        return g.data.globle.gameSetting.screenHeight;;
    }

    public static float GetUIScreenWidth()
    {
        return g.ui.canvas.GetComponent<RectTransform>().sizeDelta.x;
    }

    public static float GetUIScreenHeight()
    {
        return g.ui.canvas.GetComponent<RectTransform>().sizeDelta.y;
    }

    public static float GetUIDeltaX()
    {
        return GetUIScreenWidth() * UI_DELTA_X_RATE;
    }

    public static float GetUIDeltaY()
    {
        return GetUIScreenHeight() * UI_DELTA_Y_RATE;
    }

    public static T OpenUISafe<T>(this UIMgr g, UIType.UITypeBase t) where T : UIBase
    {
        var ui = g.OpenUI<T>(t);
        //...
        return ui;
    }

    public static bool HasUI(this UIMgr g, UIType.UITypeBase t)
    {
        return (g.GetUI(t)?.gameObject?.active).Is(true) == 1;
    }

    public static bool IsExists(this UIBase ui)
    {
        return (ui?.gameObject?.active).Is(true) == 1;
    }

    public static void MsgBox(this UIMgr g, string title, string content, MsgBoxButtonEnum type = MsgBoxButtonEnum.Ok, Action onYesCall = null, Action onNoCall = null)
    {
        var uiConfirm = g.OpenUISafe<UICheckPopup>(UIType.CheckPopup);
        uiConfirm.InitData(title, content, type.Parse<int>(), 
            (Il2CppSystem.Action)(() =>
            {
                try
                {
                    onYesCall?.Invoke();
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }), 
            (Il2CppSystem.Action)(() =>
            {
                try
                {
                    onNoCall?.Invoke();
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine(ex);
                }
            }));
    }

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

    public static List<UICustomBase> GetUICustomBase(UIType.UITypeBase uiType)
    {
        return UIs.Where(x => x.UITypeName == uiType.uiName).ToList();
    }

    public static void Dispose(this IEnumerable<UICustomBase> uis)
    {
        foreach (var ui in uis)
        {
            ui.Dispose();
        }
    }
}