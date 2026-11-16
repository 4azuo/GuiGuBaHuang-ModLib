# Create LocalText

Multi-language support for mods.

## Structure

**Main file:** `ModConf/<prefix>_localText.json`
- Contains main game languages: `en` (English), `ch` (简体中文), `tc` (繁體中文), `kr` (한국어)
- Properties: `id`, `key`, language codes (`en`, `ch`, `tc`, `kr`), `__name` (comment)

**Locale subfolders:** `ModConf/<lang>/<prefix>_localText.json`
- ModLib supports more languages than the 4 of the game: `de`, `es`, `fr`, `ja`, `la`, `ru`, `th`, `vi` (Translate/consts.py `DEFAULT_TARGET_LANGUAGES`)
- A locale file has the same `id`/`key` as the main file and one text in `"en|ch|tc|kr"` (it replaces all 4 game languages when that language is chosen)
- Every main `*_localText.json` needs a copy in **each** of the 8 folders, with every key (keep them in sync when adding texts)
- After adding/changing texts: remove the changed keys from the locale files (so they are translated again), then run
  `python Translate/run.py --project <ProjectId> --path . --file-type both --preserve-translations --create-locales all`
  (needs `pip install deep-translator`; `--preserve-translations` keeps existing translations and only translates missing keys)

## Usage

**In Code:** `GameTool.LS(key)`
**In Configs:** Use key for `name`/`desc` properties

Example:
```csharp
string text = GameTool.LS("mymod_feature_power_boost");
```

## Tools

Automated translation system available in `Translate/` folder for processing localText files.

See `Translate/README.md` for full documentation and usage instructions.
