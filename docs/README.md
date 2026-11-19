# Documentation

Everything about the mods of this repository: the ModLib framework (3385996759), the Taoist mod (3161035078), the
ModCreator tool, the game's config samples, and reusable pictures extracted from the game. Start with
`00-Getting-Started.md`; for Taoist, `Taoist/00-Overview.md`.

## Tree

```
docs/
├── README.md                     this file: the map of the documentation
├── 00-Getting-Started.md         setup, repository structure, first mod
├── 01-ModMain.md                 the ModMain class and the initialization
├── 02-Events.md                  mod events (ModEvent): creating them, their hooks
├── 03-Configs.md                 config files (ModConf/*.json)
├── 04-LocalText.md               localization: *_localText.json, locale folders, icon tokens
├── 05-Helpers.md                 the helpers of ModLib
├── 06-Debug.md                   debugging, game / mod log locations, run-game.ps1
├── 07-Build-Deploy.md            build and deploy scripts (Release, Steam Workshop)
├── Known-Issues.md               open bugs to check (reproduced by the unit tests' KnownIssues)
│
├── ModLib/                       ModLib internals
│   ├── 00-Architecture.md        dispatch, load sequence, monthly tick
│   ├── 01-Event-Reference.md     every event of ModEvent
│   ├── 02-Cache-Persistence.md   [Cache] events, global / save caches, CacheHelper
│   ├── 03-Builtin-Events.md      the events ModLib ships
│   ├── 04-UI-Framework.md        UICustom1 / UICustom2 / UICover, UI items, pages
│   └── 05-Attributes-Objects.md  attributes, objects and enums
│
├── ModConf/                      the game's config samples (3385996759/SampleConfs)
│   ├── 00-SampleConfs-List.md    index of every config type
│   └── 01..12-SampleConfs-*.md   by category: character, items, battle, world, schools, NPCs,
│                                 tasks, towns, advanced, DLC, system, mod
│
├── ModCreator/                   the WPF tool for mod projects (submodule GuiGuBaHuang-ModCreator)
│   ├── 00-Overview.md
│   ├── 01-Project-Management.md
│   ├── 02-ModConf-Editor.md
│   ├── 03-ModImg-Manager.md
│   ├── 04-Global-Variables.md
│   └── 05-ModEvent-Editor.md
│
├── Taoist/                       the Taoist mod
│   ├── 00-Overview.md            structure, event order
│   ├── 01-Features.md            every feature by event (hooks, saved data, S&M options, tests)
│   ├── 02-Configs.md             ModSettings / configs of Taoist
│   ├── 03-Test-Scenarios.md      the test scenarios (TT / TG / TB / TX / TS / TR ...) step by step
│   ├── 04-Autotest.md            the autotest runner, evidence, unit tests, viewers, promo, marketing video
│   ├── 05-Backlog.md             open work and former TODO notes
│   ├── 06-Storylines-Draft.md    design of the NPC storylines A-L and the links between them
│   ├── Features/                 one page per class of ModMain/Mod (saved data, switches, flow, formulas, UI)
│   │   └── README.md             index by group + points found while documenting
│   └── Storylines/               dialogue of every line
│       ├── 00-Conventions.md
│       ├── A-Cold-Sword.md .. F-Red-Lotus.md               lines A-F (early game)
│       └── G-Hundred-Year-Wager.md .. L-Heart-Demon.md     lines G-L (lifelong, middle and high realms)
│
├── Effects/                      effects of the game recorded by the galleries TE-xx (WebP, Git LFS)
│   ├── Scenes/  Skill/  Unit/    <id>_<name>.webp + index.json; browse: effect-viewer/index.html
│
└── GameAssets/                   pictures extracted from the game (Git LFS), reused instead of extracted again
    ├── index.html                gallery with a search box (click a picture: copies its name)
    ├── index.json                names and sizes
    ├── TextIcons/<name>.png      icons of the TextMeshPro sprite asset: "[icon:NAME]" in localTexts
    ├── ItemIcons/<icon>.png      item icons ("icon" of ItemProps / DLCItemProps) + items.json (icon -> item ids)
    └── DramaBackgrounds/<name>.webp   drama backgrounds: DramaHelper.OpenDrama2 / StoryLine.Tell background
```

## Where to look

| I want to... | Read |
|---|---|
| set up the environment, build, deploy | `00-Getting-Started.md`, `07-Build-Deploy.md` |
| write a feature (event, UI, config, text) | `02-Events.md`, `ModLib/04-UI-Framework.md`, `03-Configs.md`, `04-LocalText.md` |
| find a config type of the game and its fields | `ModConf/00-SampleConfs-List.md` |
| know what a Taoist feature does, its hooks and tests | `Taoist/01-Features.md` (summary), `Taoist/Features/<Class>.md` (details, formulas) |
| run / read the autotests, the viewers, the promo pictures, the video | `Taoist/04-Autotest.md` |
| see open work | `Taoist/05-Backlog.md`, `Known-Issues.md` |
| write or change a storyline | `Taoist/06-Storylines-Draft.md`, `Taoist/Storylines/` |
| pick an icon for a text, an item icon or a background for a drama | `GameAssets/index.html` |
| find a battle / skill effect for a custom skill | `effect-viewer/index.html` (pictures in `Effects/`) |
| debug the game, read the logs | `06-Debug.md` |

## Pictures extracted from the game

`python scripts/extract-game-assets.py` (needs `pip install UnityPy Pillow`; `--icons`, `--backgrounds`, `--items`, `--force`)
reads `guigubahuang_Data/resources.assets` and writes `GameAssets/`; pictures already there are kept, so run it only
after a game update (`--force`) or when new background names appear in the configs.

Effects are not extracted this way: an effect is a prefab of particle systems (10-16 per skill), and UnityPy only gives
their raw textures (glows, noise, masks, a fist, a heart), not the composed, animated
effect, which only the game draws. So `Effects/` keeps the recordings of the galleries TE-xx; the textures can still
help to recognize an effect.
