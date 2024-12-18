using ModLib.Object;
using System.Collections.Generic;

public static class UIHelper
{
    public static List<UICustomBase> UIs { get; } = new List<UICustomBase>();
    public static List<UIItemBase> Items { get; } = new List<UIItemBase>();
    public static List<UIItemBase> AllItems { get; } = new List<UIItemBase>();

    public const float SCREEN_Y_TOP = 5f;
    public const float SCREEN_Y_MIDDLE = 0f;
    public const float SCREEN_Y_BOTTOM = -5f;
    public const float SCREEN_X_LEFT = -10f;
    public const float SCREEN_X_MID = 0f;
    public const float SCREEN_X_RIGHT = 10f;

    public const float UICUSTOM_DELTA_X = 0.4f;
    public const float UICUSTOM_DELTA_Y = 0.24f;

    public static bool HasUI(this UIMgr g, UIType.UITypeBase t)
    {
        return (g.GetUI(t)?.gameObject?.active).Is(true) == 1;
    }

    public static bool IsExists(this UIBase ui)
    {
        return (ui?.gameObject?.active).Is(true) == 1;
    }
}