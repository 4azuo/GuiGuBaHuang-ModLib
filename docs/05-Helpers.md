# Setup Helpers

Reusable utility functions in `Helper/` folder.

## Available Helpers

**Core Framework:**
- **ActionHelper** - Lambda action tracing and error handling
- **AssemblyHelper** - Assembly info, version, dynamic loading (`VersionGreaterOrEqual(cur, req)`)
- **AttributeHelper** - Custom attribute processing and reflection
- **CacheHelper** - Data caching and retrieval
- **ConfHelper** - Config file loading and error handling
- **DebugHelper** - Logging, debug mode, exception tracking
- **EventHelper** - Event system and lifecycle (`IsLoaded(cacheId)`: the event was accepted by its OnCacheHandler)

**Game World:**
- **GameHelper** - Game state, save/load, level info (`Mute()` / `Unmute()` / `IsMuted`: Unity's master volume, not saved)
- **GamevarHelper** - Game variables management
- **WUnitHelper** - World units (global map type)
- **CUnitHelper** - Combat units (battle type)
- **DungeonHelper** - Dungeon mechanics
- **BattleHelper** - Drive a battle from code: `ClickFight()` / `LeaveBattle()` (call until true), `EndBattle(win)`, `ResumeIfPaused()` (the pause menu UIBattleExit closed with Resume), `StartRealDuel(npc)`, `KeepAlive(unit)` (NotDie + hp refill), `Hit(target, dmg, attacker)`, `Kill(target, attacker)` (real UnitHit/UnitDie events), `worldUnit.GetBattleUnit()`, `InBattle` / `BattleStarted` / `OnMap`
- **BattleSceneHelper** - The battle scene: `GetMapBounds()` / `GetMapCentre()`, `unit.MoveUnit(pos)`, `PlaceCamera(centre, orthoSize)` (controller off) / `ReleaseCamera()`, `GetOrthoSizeFor(size)`; `HideMouse()`, `HideHud()`, `HideWeather()`, `HidePlayerView()` (the dark cover around the player), `unit.HideUnitBody()`, `StopAndHideOtherUnits()` return what they turned off, `ShowAgain(objects)` restores; `DumpHierarchy(transform)` (what draws what)
- **BuildHelper** - Building construction

**Game Content:**
- **ItemHelper** - Item operations and management
- **SkillHelper** - Skill system utilities
- **EffectHelper** - Effect processing
- **BattleEffectHelper** - Visual effects of a battle (`Effect\Battle\Skill\...`): `CreateEffect(path, pos, lifeSec, onCreated, disarm)` (`disarm`: an effect created alone, outside a skill, has its unit commands removed - they would throw in the game's TimerMgr and stop every timer), `RemoveAllEffects()`, `GetVisibleBounds(go)`, `GetComponentSummary(go)`, `ListEffects("Effect/Battle/Skill")` (the game's prefabs; gallery: `docs/Effects`)
- **GodArtifactHelper** - Artifact management
- **EconomyHelper** - Economy and trading

**UI & Localization:**
- **UIHelper** - UI operations and dialogs
- **UIClickHelper** - Drive the game's UIs like a player: `ClickButton(labels)` (any open UI but the title / map), `ClickIn(ui, labels)`, `ClickOption(label)`, `FindButton(ui, labelStart)`, `ScreenTexts()`, `FindTexts(ui, text)`, `CloseAll(types)`, `OpenFresh<T>(type, init)`, `CONFIRM_LABELS`
- **UISampleHelper** - UI sample components
- **CompHelper** - Component utilities
- **SpriteHelper** - Sprite and image handling
- **TranslateHelper** - Translation and localization

**Drama & Dialogue:**
- **DramaHelper** - Dialogue and drama system (read and drive an open drama: `GetOpenDrama()`, `GetDramaText()`, `GetDramaOptions()`, `ChooseDramaOption(label)`, `NextDramaLine()`, `DismissDramas()`; `OpenDrama1(text, options, callback, left, right, speaker)`: `speaker` 1 or 2 lights the left or right portrait and dims the other, 0 both lit; 1 to 9 options use dialogues with exactly that many options (ModLib `dramaDialogue.json` 999890011-19 type 1, 999890021-29 type 2; 5-9 added in 2.2.34: the default dialogue showed no option at all), so a single "Continue" is a real button that calls the callback)
- **PortraitHelper** - Characters drawn with the game's portrait model into a RawImage (`Draw(parent, looks or unit, size, pos)`, `DrawOnScreen(looks, heightShare)` on its own overlay canvas, `RandomModelData(sex)` random looks without a world); the art goes from the head to the thighs; the texture takes the size of the RawImage when drawing
- **TextIconHelper** - Icons in texts: `[icon:NAME]` tokens of the localTexts become `<sprite name=NAME>` at load; `Text.Set` shows a text with a sprite through a TextMeshPro twin of the Text (rules in README, "LocalText - Rules")

**Data Processing:**
- **ObjectHelper** - Object operations (clone, compare, serialize)
- **ParseHelper** - Safe parsing and conversions
- **ValueHelper** - Value processing and validation
- **EnumerableHelper** - Collection operations
- **VectorHelper** - Vector math operations (`move.ToDirection8(minLength)`: "right", "up-left"... or "static")

**I/O & Network:**
- **FileHelper** - File I/O operations
- **HttpHelper** - HTTP requests and web operations
- **EncryptionHelper** - Data encryption/decryption

**Development:**
- **TraceHelper** - Execution tracing
- **ExceptionHelper** - Exception handling utilities

## Taoist: `InfoDialog` (`3161035078/.../Helper/InfoDialog.cs`)

Message windows of the Taoist mod; every `g.ui.MsgBox` of `Mod/` goes through it. `InfoDialog.Msg(title, text)` and
`Msg(title, text, MsgBoxButtonEnum.YesNo, yes)` keep the game's message box for a short text (90 characters or less, one
line) and open a text window (UITextInfo, `UICustom2`) for a longer one: the text wrapped by words (by characters in
Chinese / Japanese / Thai), the buttons under it (Yes / No, OK). `InfoDialog.Show(title, text, (label, action)...)` opens
the text window with any buttons (a button closes the window then runs its action). `LastText` for tests.
