using ModLib.Enum;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;

public static class UIHelper
{
    public static List<UICustomBase> UIs { get; } = new List<UICustomBase>();

    public const float UICUSTOM_DELTA_X = 0.4f;
    public const float UICUSTOM_DELTA_Y = 0.24f;
    public const float SCREEN_X_LEFT = -10f;
    public const float SCREEN_X_MID = 0f;
    public const float SCREEN_X_RIGHT = 10f;
    public const float SCREEN_Y_TOP = 5f;
    public const float SCREEN_Y_MIDDLE = 0f;
    public const float SCREEN_Y_BOTTOM = -5f;

    public static T OpenUISafe<T>(this UIMgr g, UIType.UITypeBase t) where T : UIBase
    {
        var ui = g.OpenUI<T>(t);
        ui.Init();
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
        uiConfirm.InitData(title, content, type.Parse<int>(), (Il2CppSystem.Action)(() => onYesCall?.Invoke()), (Il2CppSystem.Action)(() => onNoCall?.Invoke()));
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