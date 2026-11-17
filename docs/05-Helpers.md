# Setup Helpers

Reusable utility functions in `Helper/` folder.

## Available Helpers

**Core Framework:**
- **ActionHelper** - Lambda action tracing and error handling
- **AssemblyHelper** - Assembly info, version, dynamic loading
- **AttributeHelper** - Custom attribute processing and reflection
- **CacheHelper** - Data caching and retrieval
- **ConfHelper** - Config file loading and error handling
- **DebugHelper** - Logging, debug mode, exception tracking
- **EventHelper** - Event system and lifecycle

**Game World:**
- **GameHelper** - Game state, save/load, level info
- **GamevarHelper** - Game variables management
- **WUnitHelper** - World units (global map type)
- **CUnitHelper** - Combat units (battle type)
- **DungeonHelper** - Dungeon mechanics
- **BuildHelper** - Building construction

**Game Content:**
- **ItemHelper** - Item operations and management
- **SkillHelper** - Skill system utilities
- **EffectHelper** - Effect processing
- **GodArtifactHelper** - Artifact management
- **EconomyHelper** - Economy and trading

**UI & Localization:**
- **UIHelper** - UI operations and dialogs
- **UISampleHelper** - UI sample components
- **CompHelper** - Component utilities
- **SpriteHelper** - Sprite and image handling
- **TranslateHelper** - Translation and localization

**Drama & Dialogue:**
- **DramaHelper** - Dialogue and drama system (`OpenDrama1(text, options, callback, left, right, speaker)`: `speaker` 1 or 2 lights the left or right portrait and dims the other, 0 both lit; 1 to 4 options use dialogues with exactly that many options, so a single "Continue" is a real button that calls the callback)
- **TextIconHelper** - Icons in texts: `[icon:NAME]` tokens of the localTexts become `<sprite name=NAME>` at load; `Text.Set` shows a text with a sprite through a TextMeshPro twin of the Text (rules in README, "LocalText - Rules")

**Data Processing:**
- **ObjectHelper** - Object operations (clone, compare, serialize)
- **ParseHelper** - Safe parsing and conversions
- **ValueHelper** - Value processing and validation
- **EnumerableHelper** - Collection operations
- **VectorHelper** - Vector math operations

**I/O & Network:**
- **FileHelper** - File I/O operations
- **HttpHelper** - HTTP requests and web operations
- **EncryptionHelper** - Data encryption/decryption

**Development:**
- **TraceHelper** - Execution tracing
- **ExceptionHelper** - Exception handling utilities
