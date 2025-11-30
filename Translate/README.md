# 🌍 LocalText Translation System

Automated translation system for `*localText.json` files in GuiGuBaHuang ModLib game with capability to process both main files and locale files.

## 📁 Project Structure

```
Translate/
├── run.py                    # Main script to run and translate
├── translation_service.py    # Service for text translation processing
├── local_text_processor.py   # Processor for handling localText files  
├── translate_utils.py        # Utilities for translation logic
├── terminology_utils.py      # Utilities for terminology protection
├── consts.py                 # Constants and system configuration
├── data_types.py             # Class and types definitions
├── file_utils.py             # Utilities for files and directories handling
├── json_utils.py             # Utilities for JSON data processing
├── progressbar_utils.py      # Utilities for progress bars and UI
├── README.md                 # Usage instructions
└── __pycache__/             # Python cache directory
```

## 🚀 How to Use

### Install Dependencies

```bash
pip install deep-translator
```

### Basic Syntax

```bash
python run.py --project <PROJECT_ID> --path <PATH> [OPTIONS]
```

### Parameters

- `--project`: **Required**. Project directory name or full path to project folder
  - Can be project name: `3385996759` (will look in parent directory)
  - Can be full path: `C:\git\GuiGuBaHuang-ModLib\3385996759`
  - Can be relative path: `..\3385996759`
- `--path`: **Required**. Relative path within ModConf folder or file
  - `.` - Process entire ModConf directory
  - `game_localText.json` - Process specific file
  - `subfolder` - Process subdirectory
- `--dry-run`: Only display list of files to be processed, don't perform translation
- `--create-locales`: List of languages to create locale files for (e.g.: `vi,es,fr` or `all`)
- `--file-type`: Type of files to process (choices: `main`, `locale`, `both`)
  - `main` - Only process main files (original files with en/ch/tc/kr keys)
  - `locale` - Only process locale files (translated files in subdirectories)  
  - `both` - Process both types (default)
- `--preserve-translations`: Keep existing translations, only translate new terms that haven't been translated yet
- `--workers`: Number of parallel threads for file processing (default: 4). Reduce if encountering rate-limits from translation service
- `--source_lan`: Source language for translation (default: `en`). Examples: `en`, `ch`, `tc`, `kr`

## 📖 Usage Examples

### 1. Process entire ModConf directory

```bash
python run.py --project 3385996759 --path .
```

### 2. Process specific file

```bash
python run.py --project 3385996759 --path game_localText.json
```

### 3. Check files before running (dry-run)

```bash
python run.py --project 3385996759 --path . --dry-run
```

### 4. Create specific language translations

```bash
python run.py --project 3385996759 --path . --create-locales vi,es,fr
```

### 5. Create all supported language translations

```bash
python run.py --project 3385996759 --path . --create-locales all
```

### 6. Only process main files (original files)

```bash
python run.py --project 3385996759 --path . --file-type main
```

### 7. Only process locale files (translated files)

```bash
python run.py --project 3385996759 --path . --file-type locale
```

### 8. Preserve existing translations, only translate new content

```bash
python run.py --project 3385996759 --path . --preserve-translations
```

### 9. Adjust worker threads for performance

```bash
python run.py --project 3385996759 --path . --workers 8
python run.py --project 3385996759 --path . --workers 1  # Sequential processing
```

### 10. Combine preserve mode with specific file type

```bash
python run.py --project 3385996759 --path . --file-type main --preserve-translations
python run.py --project 3385996759 --path . --file-type locale --preserve-translations
```

### 11. Use custom source language

```bash
# Translate from Chinese instead of English
python run.py --project 3385996759 --path . --source_lan ch

# Translate from Korean
python run.py --project 3385996759 --path . --source_lan kr

# Translate from Traditional Chinese
python run.py --project 3385996759 --path . --source_lan tc
```

### 12. Use full path or relative path for project

```bash
# Using full path
python run.py --project "C:\git\GuiGuBaHuang-ModLib\3385996759" --path .

# Using relative path
python run.py --project "..\3385996759" --path .

# Using project name (default behavior)
python run.py --project 3385996759 --path .
```

## 🎯 When to use File Type?

### `--file-type main`
Use when:
- ✅ Have new original files that need first-time translation
- ✅ Updated content in main files and need to recreate locale files
- ✅ Want to create completely new translation set for project
- ⚠️ **Note**: Will delete all old locale files and create new ones

### `--file-type locale`
Use when:
- ✅ Only want to update/fix existing locale files
- ✅ Add new languages to existing locale files
- ✅ Don't want to touch main files or refresh locale files
- ⚠️ **Note**: Doesn't create new locale files, only updates existing ones

### `--file-type both` (default)
Use when:
- ✅ Want comprehensive processing of both main and locale files
- ✅ Not sure which type needs processing
- ✅ First time running script on project

## 🔄 Preserve Mode (`--preserve-translations`)

### When to use Preserve Mode?

**✅ Use when:**
- Already have old translations and want to keep them
- Only want to translate new words/sentences that were added
- Save time and API calls for Google Translate
- Avoid losing manually edited translations

**❌ Don't use when:**
- Want to refresh all translations
- Old translations have poor quality and need replacement
- First time running translation on new project

### How it works:

1. **Main Files**: Check language fields (`ch`, `tc`, `kr`)
   - If field already has content → Keep as is
   - If field is empty/missing → Translate new from English

2. **Locale Files**: Compare with existing locale file
   - Find old translations based on ID/key or English text
   - If found → Use old translation
   - If not found → Translate new from English

### Real examples:

```bash
# First run - create all translations
python run.py --project 3385996759 --path game_localText.json

# After adding new content to file - only translate new parts
python run.py --project 3385996759 --path game_localText.json --preserve-translations
```

## 🌍 Supported Languages

By default, the system supports the following languages:

- `vi` - Vietnamese
- `es` - Spanish
- `fr` - French
- `de` - German
- `ru` - Russian
- `ja` - Japanese
- `la` - Latin

You can use any ISO language code supported by Google Translate.

## 📂 File Structure

### Main Files (Root ModConf directory)
Contains original languages: `en`, `ch`, `tc`, `kr`

```json
[
  {
    "id": 1,
    "en": "English text",
    "ch": "简体中文",
    "tc": "繁體中文", 
    "kr": "한국어"
  }
]
```

### Locale Files (Subdirectories)
Contains translations with combined key `en|ch|tc|kr`

```json
[
  {
    "id": 1,
    "en|ch|tc|kr": "Translated text"
  }
]
```

## 📝 Development & Maintenance

### Adding new languages

Edit `DEFAULT_TARGET_LANGUAGES` in `consts.py`:

```python
DEFAULT_TARGET_LANGUAGES = ['vi', 'es', 'fr', 'de', 'ru', 'ja', 'la', 'pt']  # Add Portuguese
```

### Adding text to skip translation

The system can skip translating specific texts. To add new text to skip:

```python
# In consts.py
SKIP_TRANSLATION_TEXTS = [
    "DEFAULT_DRAMA_OPT",
    "YOUR_TEXT_HERE"  # Add new text to skip here
]
```

### Format String Protection Configuration

The system automatically protects format strings from being converted to zenkaku characters during translation. To add new format patterns:

```python
# In consts.py - FORMAT_PROTECTION_CONFIG
'patterns': [
    r'{\d+(?::[^}]+)?}',  # {0}, {1}, {0:#,##0} - Already exists
    r'%[sdf]',            # %s, %d, %f - Already exists  
    r'%\d+[sdf]',         # %1s, %2d, %3f - Already exists
    r'\$\{\w+\}',         # ${variable} - Already exists
    r'YOUR_PATTERN_HERE', # Add new pattern here
]
```

**Format strings protected by default:**
- `{0}`, `{1}`, `{0:#,##0}` - C# format strings  
- `%s`, `%d`, `%f` - Printf style formats
- `${variable}` - Template strings

**Examples of issues that have been fixed:**
- ❌ `{0:#,##0}` → `｛０：＃，＃＃０｝` (zenkaku error)
- ✅ `{0:#,##0}` → `{0:#,##0}` (protected)

### Terminology Protection

The system automatically protects cultivation/martial arts terminology from being mistranslated:

```python
# In consts.py - TERMINOLOGY_CONFIG
'terms': [
    'Qi Refining', 'Foundation Establishment', 'Core Formation',
    'Dantian', 'Spiritual Qi', 'Elder', 'Disciple'
    # ... and many other terms
]
```

**How it works:**
1. **Protect**: `"Elder taught Qi Refining"` → `"Elder taught {\uF8B3}0{/\uF8B3}"`
2. **Translate**: `"Trưởng lão dạy {\uF8B3}0{/\uF8B3}"`
3. **Restore**: `"Trưởng lão dạy Qi Refining"`

**Benefits:**
- ✅ Terms are not mistranslated or changed
- ✅ Consistency throughout the entire project  
- ✅ Uses indexed markers `{\uF8B3}0{/\uF8B3}` to avoid conflicts

---

**Version**: 3.5 | **Updated**: 2025/11
**Dependencies**: `deep-translator>=1.9.0`

## 📄 License

This project belongs to the GuiGuBaHuang ModLib community.