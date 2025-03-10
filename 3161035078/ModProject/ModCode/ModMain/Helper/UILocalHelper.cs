using ModLib.Object;

public static class UILocalHelper
{
    public const string LABEL = "?";

    public static UIItemButton AddToolTipButton(this UICustomBase ui, string content)
    {
        return ui.AddButton(0, 0, () =>
        {
            g.ui.MsgBox(string.Empty, content);
        }, LABEL).Align(UnityEngine.TextAnchor.MiddleCenter).Size(32, 32);
    }

    public static UIItemButton AddToolTipButton(this UICustomBase ui, int col, int row, string content)
    {
        return ui.AddButton(col, row, () =>
        {
            g.ui.MsgBox(string.Empty, content);
        }, LABEL).Align(UnityEngine.TextAnchor.MiddleCenter).Size(32, 32);
    }

    public static UIItemButton AddToolTipButton(this UICustomBase ui, float x, float y, string content)
    {
        return ui.AddButton(x, y, () =>
        {
            g.ui.MsgBox(string.Empty, content);
        }, LABEL).Align(UnityEngine.TextAnchor.MiddleCenter).Size(32, 32);
    }
}