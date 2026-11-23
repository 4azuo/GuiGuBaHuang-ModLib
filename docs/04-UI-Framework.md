# ModLib - UI framework

`ModLib.Object.UI*` builds custom UI on top of the game's uGUI by **copying sample components** (from `UISample`, loaded at `OnInitMod`) and placing them on a grid.

## Containers

| Class | Base game UI | Use |
|---|---|---|
| `UICover<T>` | the UI you cover (`T : UIBase`) | Add items on top of an existing game UI (grid = whole screen). Create it in `OnOpenUIEnd` for that UI |
| `UICoverTextInfoLong` | an opened `UITextInfoLong` | `UICover` with the grid of `UICustom1` (the dialog area): items land where they would in a `UICustom1` window. Used by the exception dialog for its red note at the bottom |
| `UICustom1` | `UITextInfoLong` | Big custom window (title, OK button, optional Cancel) |
| `UICustom2` | `UITextInfo` | Small custom window |

All containers derive from `UICustomBase` and register themselves in `UIHelper.UIs` (`UIHelper.GetUICustomBase(UIType.X)` returns the custom UIs of a game UI).

Grid: `Columns`/`Rows` are coordinates built from the bounds and `UIHelper.GetUIDeltaX/Y()`; use `FirstCol/MidCol/LastCol`, `FirstRow/MidRow/LastRow`. Every `AddXxx` has an absolute `(float x, float y)` and a grid `(int col, int row)` overload.

## Items

`AddText`, `AddButton(…, Action act, string text)`, `AddToggle`, `AddSlider`, `AddInput`, `AddSelect` (dropdown made of toggles), `AddImage`, and composites `AddCompositeSlider/Toggle/Select/Input` ("prefix + component + postfix"). Each item is fluent:

```csharp
var ui = new UICover<UIFateFeature>(e.ui);
ui.AddButton(ui.MidCol, ui.MidRow + 4, OnClick, GameTool.LS("mytext001"))
  .Size(200f, 40f)
  .Format(Color.black, 16)
  .Align(TextAnchor.MiddleCenter)
  .SetWork(new UIItemWork
  {
      Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>(x => new object[] { g.world.playerUnit.GetUnitMoney() }),
      UpdateAct = ActionHelper.WTracedAction<UIItemBase>(x => x.Active(!g.world.playerUnit.IsFullExp())),
  });
ui.IsAutoUpdate = true;   // refreshed every 100 ms by ModUIEvent
```

* `UIItemWork`: `Formatter` (format args of the text), `ChangeAct` (value changed), `UpdateAct`, `EnableAct`, `ActiveAct`.
* `Pos(...)` overloads place an item relative to another object (`Pos(gameObject, dx, dy)`); `SetParentTransform` re-parents it.
* Paging: `AddPage()` / `AddPage(loader)` then add items; `IsShowNavigationButtons = true` shows prev/next; `Pages[i].Active()`.
* Wrap callbacks with `ActionHelper.TracedAction/TracedUnityAction/TracedIl2Action` so exceptions are logged instead of crashing the game.

## Helpers

`g.ui.OpenUISafe<T>(UIType.X)`, `g.ui.HasUI(UIType.X)`, `ui.IsExists()`, `g.ui.MsgBox(title, text, MsgBoxButtonEnum.YesNo, onYes)`, `UIHelper.UpdateAllUI()`. Existing game components can be cloned with `Copy()` / replaced with `Replace()` (see `CompHelper`).
