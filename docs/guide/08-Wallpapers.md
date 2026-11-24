# Wallpapers on the title screen

A mod can add its own wallpapers to the title screen without code: an image in `ModImg` and one small JSON file per
wallpaper in `ModConf`. ModLib (`WallpaperHelper`, run by the built-in `ModTitleEvent`) does the rest:

- the images are added at the end of the game's **Change Theme** window (after the game's wallpapers);
- the one the player chooses there is shown on the title screen over the game's wallpaper (the game's animated
  wallpaper is hidden meanwhile) and saved in the ModLib global settings (`ModTitleEvent.Wallpaper`, key
  `modId/image`); choosing a wallpaper of the game removes it;
- the "Current" mark of the window follows the wallpaper shown;
- the game's title (the logo) can be hidden over a mod wallpaper, when the picture has its own title;
- the game can start on a mod wallpaper at random (this game session only, the player's choice is kept).

Reference example: Taoist (`3161035078/ModProject/ModConf/taoist_1_wallpaper.json`, `taoist_2_wallpaper.json`, images
`ModImg/taoist_1_wallpaper.png`, `taoist_2_wallpaper.png`).

## The wallpaper file

One file per wallpaper in `ModConf`, its name ending with `wallpaper.json` (case ignored), e.g.
`mymod_night_wallpaper.json`. The game does not load it (no config of the game is named "wallpaper"); ModLib reads it
on the title screen.

```json
{
  "image": "mymod_night_wallpaper.png",
  "name": "Night",
  "order": 1,
  "hideGameTitle": true,
  "startupRate": 0.25,
  "startupWhenChosen": false
}
```

| Field | Default | Meaning |
|---|---|---|
| `image` | - | Image file of the mod's `ModImg` folder: `.png`, `.jpg` or `.jpeg`. Missing or another type: the file is skipped (log line). |
| `name` | - | Text of the item, only when the item of the window has a text (the game's items are pictures only); else "Mod wallpaper n" (`libtxt999990032`). |
| `order` | `0` | Place among the wallpapers of the mod in the Change Theme window (then the file name). Mods follow their load order. |
| `hideGameTitle` | `true` | The game's title hidden while this wallpaper is shown (when the player did not choose with the toggle, below). |
| `startupRate` | `0` | Chance (0..1) that the game starts on this wallpaper. `0`: never at the start, still in the window. |
| `startupWhenChosen` | `false` | Startup draw even when the player already chose a wallpaper of the same mod (default: no draw then, the player's wallpaper stays). |

Property names are not case-sensitive. A broken JSON file is skipped (log line `Wallpaper modId/file: ...` in the mod
log); the wallpapers read are logged once (`Wallpapers: ...`). Two files with the same image: the first one counts.

Image size: the picture is stretched over the whole screen; use the screen ratio of the game (16:9, e.g. 2560x1440).

## Startup wallpaper

On the first title screen of a game session, ModLib draws once (`WallpaperHelper.ShowStartupWallpaper`), over all
the wallpapers of the loaded mods with a `startupRate`: each one has its own chance, the rest of the time no mod
wallpaper is drawn. E.g. Taoist: two wallpapers at `0.25` = half of the starts on one of them. When the rates add up
to more than 1 they are scaled down (a mod wallpaper every time). A wallpaper is left out when the player already chose
a wallpaper of the same mod in the window, unless its `startupWhenChosen`.

The result (`WallpaperHelper.StartupWallpaper`) lasts for this game session only and is never saved: the next start
draws again, and choosing any wallpaper in the Change Theme window drops it.

## Hiding the game's title

The game's title is hidden while a mod wallpaper is shown, when:

1. the player's own choice says so: the toggle "Hide the game's title on mod wallpapers" (`libtxt999990033`) above
   the list of the Change Theme window (only shown when there are mod wallpapers), saved in
   `ModTitleEvent.HideGameTitle`;
2. else (the player never used the toggle: `HideGameTitle` null) the `hideGameTitle` of the wallpaper shown.

On a wallpaper of the game the title is always shown.

## From code

Nothing is required, but `WallpaperHelper` gives:

| Member | Use |
|---|---|
| `GetWallpapers()` | The wallpapers of the loaded mods (read once, again when the loaded mods change); `Wallpaper.Conf` holds the settings of the file. |
| `Find(key)`, `MakeKey(modId, image)` | A wallpaper by key `modId/image` (case ignored). |
| `GetCurrentKey()` | The wallpaper the title screen shows: the startup one, else the player's choice (null: the game's). |
| `Choose(key)` | As the player in the window: saves the choice (null: the game's) and shows it. |
| `StartupWallpaper` | Set it before the title screen opens to show a wallpaper for this session only (`ShowOnTitle(key)` on an open title screen). |
| `ReadWallpaper(modId, file, json, imageExists)`, `DrawStartupWallpaper(wallpapers, chosenKey, random)` | The JSON reading and the startup draw alone (tests). |
| `ShouldHideGameTitle(w)`, `ShowOnTitle(key)`, `IsShownOnTitle()`, `UpdateGameTitle()`, `IsGameTitleHidden()` | The title screen. |
| `GetThemeWindowItems()`, `GetCurrentMarkItem()` | The open Change Theme window. |

## Tests

Taoist autotest TT-09 (wallpaper files, startup draw, Change Theme window, title screen, toggle) and TT-08 (the ModLib
buttons of the title screen); see `docs/Taoist/03-Test-Scenarios.md` of the Taoist repository.
