# UITitleScreenEvent

`3161035078/ModProject/ModCode/ModMain/Mod/UITitleScreenEvent.cs` - cache key `ModConst.UI_TITLE_SCREEN_EVENT` (Global) - group: UI

Title screen additions: a version button, the Workshop description, and a warning when ModLib is too old.

## Flow

| Hook | What happens |
|---|---|
| `OnOpenUIEnd(Login)` | 1. Button `Taoist {TAOIST_VERSION}` (white, 22, styled like `btnPaperChange`) in the button group: opens the Taoist cache folder in Explorer (`CacheHelper.GetCacheFolderName`). 2. Opens a `UITextInfoLong` titled `other500020046` with the Workshop description downloaded from the Workshop page (`HttpHelper.GetWorkshopDescription`, id 3161035078). 3. If ModLib's version < `ModConst.MODLIB_REQUIRED_VERSION`: a `UITextInfo` warning `ui_modlib_too_old` (Taoist version, ModLib version, required). |

## Version compare

`CompareVersion(req, cur)`: both split on non-digits, compared part by part; equal prefix = OK.

## Notes

- The description is downloaded at every title screen (network access); a failure is handled by `HttpHelper`.
- No saved data, no switch.

## Tests

TT-01.
