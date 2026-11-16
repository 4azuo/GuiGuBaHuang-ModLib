# Create LocalText

Multi-language support for mods.

## Structure

**Main file:** `ModConf/<prefix>_localText.json`
- Contains main game languages: `en` (English), `ch` (简体中文), `tc` (繁體中文), `kr` (한국어)
- Properties: `id`, `key`, language codes (`en`, `ch`, `tc`, `kr`), `__name` (comment)

**Locale subfolders:** `ModConf/vi/`, `ModConf/es/`, `ModConf/fr/`, etc.
- Additional languages supported by ModLib
- Example: `ModConf/vi/<prefix>_localText.json` for Vietnamese

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
