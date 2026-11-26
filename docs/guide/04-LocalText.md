# Create LocalText

Multi-language support for mods.

## Structure

**Main file:** `ModConf/<prefix>_localText.json`
- Contains main game languages: `en` (English), `ch` (简体中文), `tc` (繁體中文), `kr` (한국어)
- Properties: `id`, `key`, language codes (`en`, `ch`, `tc`, `kr`), `__name` (comment)

**Locale subfolders:** `ModConf/<lang>/<prefix>_localText.json`
- ModLib supports more languages than the 4 of the game: `de`, `es`, `fr`, `ja`, `la`, `ru`, `th`, `vi` (GuiGuBaHuang-ModLib/Translate/consts.py `DEFAULT_TARGET_LANGUAGES`)
- A locale file has the same `id`/`key` as the main file and one text in `"en|ch|tc|kr"` (it replaces all 4 game languages when that language is chosen)
- Every main `*_localText.json` needs a copy in **each** of the 8 folders, with every key (keep them in sync when adding texts)
- After adding/changing texts: remove the changed keys from the locale files (so they are translated again), then run
  `python GuiGuBaHuang-ModLib/Translate/run.py --project <ProjectId> --path . --file-type both --preserve-translations --create-locales all`
  (needs `pip install deep-translator`; `--preserve-translations` keeps existing translations and only translates missing keys)
- The translation tool waits 1 s between requests (`--delay`), backs off when the service answers 429 (30 s, 60 s... up to 10 min),
  leaves a text missing when it still fails (the next run retries it) and caches every translation (`.translate-cache/` of this repository, in git:
  a text is never requested twice). Google blocked this machine on 2026-11-16: `--provider mymemory` (free, ~5000 characters a day).
- **AI translation with few tokens** (`GuiGuBaHuang-ModLib/Translate/ai_bridge.py`):
  1. `python GuiGuBaHuang-ModLib/Translate/ai_bridge.py export --projects <ModProject> --out ai_todo` - the cached translations are applied at
     once; `ai_todo.json` keeps only the texts still missing, each English text once, and the rules for the AI are printed;
  2. the AI writes `<lang>.json` = `{"t1": "...", ...}` for each language listed;
  3. `python GuiGuBaHuang-ModLib/Translate/ai_bridge.py import --lang de --file de.json --out ai_todo` - validated (placeholders, `<color>` tags, line
     breaks) and appended to the locale files, then cached. `status` counts what is missing; `seed-cache` fills the cache from the
     locale files.

## Rules

1. **No hardcoded text** in the code: every text shown to the player is a localText key.
   - Code: `GameTool.LS(key)`; with format items `LocalTextHelper.LSF(key, args)` (= `string.Format(GameTool.LS(key), args)`); `LocalTextHelper.HasText(key)`, `RandomLS(key)` (the key or one of its variants `key_1`, `key_2`... at random), `Variants(key)`.
   - Never read a language field directly (`allText[key].en`): it shows English whatever the language.
2. **Which file**: one `<feature>_localText.json` per feature, key prefix = feature, one block of ids per file.

   | Project | File | Keys | Ids |
   |---|---|---|---|
   | ModLib (3385996759) | `localText.json` | `libtxt<id>` | `999990000`+ |
   | a mod | `<feature>_localText.json` | `<feature>…` | a block of its own per file (e.g. Taoist: `-1000001000`…, [docs/Taoist/02-Configs.md](https://github.com/4azuo/GuiGuBaHuang-Taoist/blob/main/docs/Taoist/02-Configs.md#localtext-files-and-id-blocks)) |

   A mod reserves blocks of ids for its files (negative ids stay away from the game's); a new feature with many texts
   gets a new file and the next free block; the ids of a block go down (-1000008000, -1000008001…).
3. **Languages**: the main file has `en`, `ch`, `tc`, `kr`; each of the 8 folders `de es fr ja la ru th vi` has the same file with every key
   (`"en|ch|tc|kr"`). Add a key to the 9 files at once (translation tools: "Structure" above).
4. **Numbers**: `{0:#,##0}` for amounts (`{0:0.00}` for rates).
5. **Icons**: `[icon:NAME]` shows the game's sprite NAME, e.g. `{0:#,##0} [icon:zm_lingshi]` (spirit stones). NAME is the name of a
   sprite of the game's TextMeshPro sprite asset (`zm_lingshi`, `zm_yaocai`, `zm_kuangshi`, `lingli`, `daodian`, `jinenglengque`…; no
   required prefix). ModLib turns the tokens into `<sprite name=NAME>` when the mod loads (and after a language change); dramas and
   TextInfo show them as is, the other texts (custom UI items, message boxes, drama options) through a TextMeshPro twin: set texts with
   `Text.Set(...)` (CompHelper), not `text.text = ...`.

## Usage

**In Code:** `GameTool.LS(key)`
**In Configs:** Use key for `name`/`desc` properties

Example:
```csharp
string text = GameTool.LS("mymod_feature_power_boost");
```

## Tools

Automated translation system in ModLib (`GuiGuBaHuang-ModLib/Translate/`, shared by the mods; projects and cache stay in the mod repository) for processing localText files.

See `GuiGuBaHuang-ModLib/Translate/README.md` for full documentation and usage instructions.
