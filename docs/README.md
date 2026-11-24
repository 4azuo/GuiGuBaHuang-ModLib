# ModLib documentation

The internals of ModLib, the game's config samples and pictures / effects of the game, reused by the mods.
How to write a mod with ModLib (setup, ModMain, events, configs, texts, helpers, debug, build) and the Taoist mod are
documented in the main repository: [GuiGuBaHuang-Taoist/docs](https://github.com/4azuo/GuiGuBaHuang-Taoist/tree/main/docs),
where this repository is the `GuiGuBaHuang-ModLib` submodule.

## Tree

```
docs/
├── README.md                     this file
├── 00-Architecture.md            dispatch, load sequence, monthly tick
├── 01-Event-Reference.md         every event of ModEvent
├── 02-Cache-Persistence.md       [Cache] events, global / save caches, CacheHelper
├── 03-Builtin-Events.md          the events ModLib ships
├── 04-UI-Framework.md            UICustom1 / UICustom2 / UICover, UI items, pages
├── 05-Attributes-Objects.md      attributes, objects and enums
├── 06-Shared-Tools.md            tools shared by the mod repositories (build, game runs, autotest, viewers, Translate, unit tests)
│
├── ModConf/                      the game's config samples (samples: 3385996759/SampleConfs of this repository)
│   ├── 00-SampleConfs-List.md    index of every config type
│   └── 01..12-SampleConfs-*.md   by category: character, items, battle, world, schools, NPCs,
│                                 tasks, towns, advanced, DLC, system, mod
│
├── Effects/                      effects of the game recorded by the galleries TE-xx (WebP, Git LFS)
│   ├── Scenes/  Skill/  Unit/    <id>_<name>.webp + index.json; browse: ../effect-viewer/index.html
│
└── GameAssets/                   pictures extracted from the game (Git LFS), reused instead of extracted again
    ├── index.html                gallery with a search box (click a picture: copies its name)
    ├── index.json                names and sizes
    ├── TextIcons/<name>.png      icons of the TextMeshPro sprite asset: "[icon:NAME]" in localTexts
    ├── ItemIcons/<icon>.png      item icons ("icon" of ItemProps / DLCItemProps) + items.json (icon -> item ids)
    ├── Buildings/<atlas>/<name>.png  sprites of the atlases TownCommon, CommonMapGrid, VillageCommon (buildings, places)
    └── DramaBackgrounds/<name>.webp   drama backgrounds: DramaHelper.OpenDrama2 / StoryLine.Tell background
```

`Effects/` and `GameAssets/` are kept with Git LFS: install it (`git lfs install`) before cloning to get the pictures.

The effect viewer (`effect-viewer/` at the root of this repository) shows `Effects/` with search and filters: run
`python effect-viewer/update-data.py` (writes `effect-viewer/data/effects.js`, not in git), then open
`effect-viewer/index.html`. To save your own assessment of an effect (`user-*` fields of index.json), open it through
the viewer server: `python GuiGuBaHuang-ModLib/scripts/effect-viewer-server.py --open` (from the mod repository).

## Where to look

| I want to... | Read |
|---|---|
| understand the dispatch of the events, the load sequence, the monthly tick | `00-Architecture.md` |
| find an event and when it is called | `01-Event-Reference.md`, `03-Builtin-Events.md` |
| keep data of a mod (global / per save) | `02-Cache-Persistence.md` |
| build a window or cover a game window | `04-UI-Framework.md` |
| find a config type of the game and its fields | `ModConf/00-SampleConfs-List.md` |
| pick an icon for a text, an item icon or a background for a drama | `GameAssets/index.html` |
| build, run the game, run the autotest, view the results, translate (any mod repository) | `06-Shared-Tools.md` |
| find a battle / skill effect for a custom skill | `../effect-viewer/index.html` (data: `python effect-viewer/update-data.py`; pictures and measures in `Effects/<kind>/`) |

## Pictures extracted from the game

The scripts live in `scripts/` of this repository and write into it (commit the pictures here, then the new
submodule commit in the mod repository).

`python GuiGuBaHuang-ModLib/scripts/extract-game-assets.py` (needs `pip install UnityPy Pillow`; `--icons`, `--backgrounds`, `--items`,
`--buildings`, `--force`) reads `guigubahuang_Data/resources.assets` and writes `GameAssets/`; pictures already there
are kept, so run it only after a game update (`--force`) or when new background names appear in the configs.

Effects are not extracted this way: an effect is a prefab of particle systems (10-16 per skill), and UnityPy only gives
their raw textures (glows, noise, masks, a fist, a heart), not the composed, animated effect, which only the game
draws. So `Effects/` keeps the recordings of the galleries TE-xx (`scripts/game/run-effect-gallery.ps1`,
`collect-effects.ps1`; details: [docs/Taoist/04-Autotest.md](https://github.com/4azuo/GuiGuBaHuang-Taoist/blob/main/docs/Taoist/04-Autotest.md) of the main repository); the textures can still help to
recognize an effect.
