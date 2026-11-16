using ModLib.Object;
using ModLib.Helper;

/// <summary>
/// Taoist UI shortcuts.
/// </summary>
public static class UILocalHelper
{
    public const string LABEL = "?";

    /// <summary>
    /// Adds a "?" button showing a tooltip text.
    /// </summary>
    public static UIItemButton AddToolTipButton(this UICustomBase ui, string content)
    {
        return ui.AddButton(0, 0, () =>
        {
            g.ui.MsgBox(string.Empty, content);
        }, LABEL).Align(UnityEngine.TextAnchor.MiddleCenter).Size(32, 32);
    }

    /// <summary>
    /// Adds a "?" button showing a tooltip text.
    /// </summary>
    public static UIItemButton AddToolTipButton(this UICustomBase ui, int col, int row, string content)
    {
        return ui.AddButton(col, row, () =>
        {
            g.ui.MsgBox(string.Empty, content);
        }, LABEL).Align(UnityEngine.TextAnchor.MiddleCenter).Size(32, 32);
    }

    /// <summary>
    /// Adds a "?" button showing a tooltip text.
    /// </summary>
    public static UIItemButton AddToolTipButton(this UICustomBase ui, float x, float y, string content)
    {
        return ui.AddButton(x, y, () =>
        {
            g.ui.MsgBox(string.Empty, content);
        }, LABEL).Align(UnityEngine.TextAnchor.MiddleCenter).Size(32, 32);
    }
}