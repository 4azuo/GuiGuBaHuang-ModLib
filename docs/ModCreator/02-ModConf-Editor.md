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

## Pattern Element Format

When creating patterns in `modconf-patterns.json`, you can specify element behavior using attributes and formats:

**Full Format:**
```
<ElementName>[Disabled][ReadOnly][Required]:<format>
```

**Components:**

1. **`<ElementName>`** - Element name from `modconfs.json` (e.g., `RoleEffect.effectType`)

2. **Attributes** (optional, can combine):
   - `[Disabled]` - Element is visible but cannot be edited (grayed out)
   - `[ReadOnly]` - Element is read-only (no user input allowed)
   - `[Required]` - Field must have a value before saving

3. **`:<format>`** - Element display format (optional):

   **a) Dynamic Selection - `[SourceKey]`**
   - Format: `ElementName:[SourceKey]`
   - Example: `RoleCreateFeature.name:[LocalText.key]`
   - Behavior: Dropdown list populated from values in the source element
   - User selects from existing values in `LocalText.key` field

   **b) Multi-Select - `#SourceKey#`**
   - Format: `ElementName:#SourceKey#`
   - Example: `RoleCreateFeature.effect:#RoleEffect.id#`
   - Behavior: Multi-select dropdown with checkboxes
   - Allows selecting multiple values separated by `|`
   - Only shows IDs from the specified source (e.g., only `RoleEffect.id`, not `LocalText.id`)

   **c) Auto-Generated - `{PlaceholderKey}`**
   - Format: `ElementName:template{PlaceholderKey}`
   - Example: `LocalText.key:localtext{LocalText.id}`
   - Behavior: Field is auto-generated and read-only
   - Text automatically formats using template and referenced values
   - Updates when referenced fields change

**Examples:**

```json
"Elements": [
  "RoleCreateFeature.id",
  "RoleCreateFeature.name:[LocalText.key]",
  "RoleCreateFeature.effect:#RoleEffect.id#",
  "RoleEffect2.effectType[Disabled]",
  "LocalText.key:localtext{LocalText.id}",
  "RoleEffect.value[Required]"
]
```

**Attribute Combinations:**
- `[Disabled][ReadOnly]` - Both disabled and read-only
- `[Required][ReadOnly]` - Must exist but cannot be edited
- Multiple attributes can be combined in any order

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