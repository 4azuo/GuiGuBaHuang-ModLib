# ModConf Editor

## Overview

Tab 2 in Project Editor. Manage JSON config files in tree structure with syntax-highlighted editor.

## Key Functions

**Add Config:**
1. Click Add Conf
2. Search sample configs
3. Enter filename prefix
4. Creates: `<prefix>_<original>.json`

**Folders:**
- Create Folder: Organize configs by category
- Delete Folder: Removes folder and contents
- Max 3 levels recommended

**File Operations:**
- Clone: Creates copy with `_copy` suffix
- Rename: Change filename (auto-adds .json)
- Remove: Delete file permanently
- Open in Notepad++/Notepad: External editing

**Find/Replace:** (`Ctrl+F`)
- Find Next: Locate text
- Replace One: Replace current match
- Replace All: Replace all occurrences

## Common Configs

- **localText.json**: UI text translations (chinese/english keys)
- **BattleSkillAttack.json**: Attack skill definitions
- **ArtifactShape.json**: Item/artifact configs

## JSON Rules

- Keys in quotes: `"key"`
- Strings in quotes: `"value"`
- Numbers: `123`, `45.67`
- Boolean: `true`/`false`
- No trailing commas