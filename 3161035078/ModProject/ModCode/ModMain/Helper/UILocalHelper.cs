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

    /// <summary>
    /// Adds the game's icon of an item (frame of its grade, as in the bag) centred at (x, y), about px wide: a
    /// transparent image item holds it, so it follows the page of the window.
    /// </summary>
    public static UIItemImage AddPropIcon(this UICustomBase ui, float x, float y, int propId, float px = 44f)
    {
        var holder = ui.AddImage(x, y, null);
        holder.Item.color = UnityEngine.Color.clear;
        holder.Size(px, px);
        try
        {
            var props = DataProps.PropsData.NewProps(propId, 1);
            var icon = UIIconTool.CreatePropsIcon(g.world.playerUnit, props, holder.Item.transform, 1f, true, false, false);
            var rt = icon?.GetComponent<UnityEngine.RectTransform>();
            if (rt != null && rt.rect.width > 1f)
            {
                var k = px / rt.rect.width;
                rt.localScale = new UnityEngine.Vector3(k, k, 1f);
                rt.anchoredPosition = UnityEngine.Vector2.zero;
            }
        }
        catch (System.Exception ex)
        {
            DebugHelper.WriteLine($"AddPropIcon {propId}: {ex.Message}");
        }
        return holder;
    }
}