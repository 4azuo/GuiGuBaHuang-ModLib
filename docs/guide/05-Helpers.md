# Setup Helpers

Reusable utility functions in `Helper/` folder.

## Available Helpers

**Core Framework:**
- **ActionHelper** - Lambda action tracing and error handling
- **AssemblyHelper** - Assembly info, version, dynamic loading (`VersionGreaterOrEqual(cur, req)`)
- **AttributeHelper** - Custom attribute processing and reflection
- **CacheHelper** - Data caching and retrieval
- **ConfHelper** - Config file loading and error handling; `ReadConfJson<T>(modId, file, fallback)` (a JSON data file of the ModConf of a mod: the fallback when missing or broken, logged)
- **PrivateModHelper** - Private mods (`public const bool PRIVATE_MOD = true` in `MOD_<id>.ModMain`: no Steam Workshop upload in the mod editor, see [07-Build-Deploy](07-Build-Deploy.md#private-mods)): `IsPrivate(modProject)`, `ReadPrivateConst(type)`, `FindEntryType(modId, dlls)`
- **LocalTextHelper** - `LSF(key, args)` (a localText with its format items), `HasText(key)`, `RandomLS(key)` / `Variants(key)` (random variants `key_1`, `key_2`...)
- **DebugHelper** - Logging, debug mode, exception tracking
- **EventHelper** - Event system and lifecycle (`IsLoaded(cacheId)`: the event was accepted by its OnCacheHandler)

**Game World:**
- **GameHelper** - Game state, save/load, level info (`Mute()` / `Unmute()` / `IsMuted`: Unity's master volume, not saved)
- **GamevarHelper** - Game variables management
- **WUnitHelper** - World units (global map type); `GetAllMartialProps()` (martial arts learnt and carried, by soleID)
- **CUnitHelper** - Combat units (battle type); `cunit.GetGradeLvl()` (realm of a battle unit: its world unit's, else the grade of its data), `AddPercentUntil(dynInt, percent, untilTime, key)` (+/- percent of a battle value - attack, defense, moveSpeed... - until a time: a DynInt add handler, the scale of the value counted), `IsBoss()` (MonstType.BOSS), `IsControlImmune()` (Stoic, StrongStoic or Invincible now: the game refuses control states)
- **HitHelper** - Hits made by code of a kind the game and the mods understand: `NewHit(attacker, weaponType, magicType, damage, skillBase)` / `NewHit(attacker, "sword", damage, skillBase)` (`skillBase`: the hit counts as that martial art - the mods add its mastery), `Deal(target, hit)`; never weapon 0 / element 0 (the game throws IndexOutOfRange on a hit of no kind: the kind of `skillBase`, else fire); `TypesOf(attributes)` / `ATTRIBUTES` (the 12 attributes of the game: blade..finger = weapon type 1..6, fire, frost, thunder, wind, earth, wood = element 1..6)
- **ModApiHelper** - What a mod offers others without a reference to its assembly: a public static class with a `VERSION` const (e.g. `MOD_rLvJjS.Helper.DocApi` of Dao of Creation); `Has(type, minVersion)`, `VersionOf`, `Get<T>(type, member, fallback)`, `Call<T>(type, method, fallback, args...)`, `Invoke(type, method, args...)` - every call safe (the fallback when the mod is not loaded, the member is missing or the call throws)
- **DungeonHelper** - Dungeon mechanics
- **BattleHelper** - Drive a battle from code: `ClickFight()` / `LeaveBattle()` (call until true), `EndBattle(win)`, `ResumeIfPaused()` (the pause menu UIBattleExit closed with Resume), `StartRealDuel(npc)`, `KeepAlive(unit)` (NotDie + hp refill), `Hit(target, dmg, attacker)`, `Kill(target, attacker)` (real UnitHit/UnitDie events), `worldUnit.GetBattleUnit()`, `InBattle` / `BattleStarted` / `OnMap`; `GetMonsterName(attrId)` (name of a BattleUnitAttr monster in the language of the game, null without one)
- **BattleSceneHelper** - The battle scene: `GetMapBounds()` / `GetMapCentre()`, `unit.MoveUnit(pos)`, `PlaceCamera(centre, orthoSize)` (controller off) / `ReleaseCamera()`, `GetOrthoSizeFor(size)`; `HideMouse()`, `HideHud()`, `HideWeather()`, `HidePlayerView()` (the dark cover around the player), `unit.HideUnitBody()`, `StopAndHideOtherUnits()` return what they turned off, `ShowAgain(objects)` restores; `DumpHierarchy(transform)` (what draws what)
- **BuildHelper** - Building construction

**Game Content:**
- **ItemHelper** - Item operations and management
- **SkillHelper** - Skill system utilities; `NewMartial(type, baseId, grade, level)` (a martial art as the game makes its rewards: prefixes drawn for the quality), `GetMartialBaseIds(type, grade)` (the bases a player can get at a realm), `IsSameMartial(a, b)` (same kind and base)
- **EffectHelper** - Effect processing
- **BattleEffectHelper** - Visual effects of a battle (`Effect\Battle\Skill\...`): `CreateEffect(path, pos, lifeSec, onCreated, disarm)` (`disarm`: an effect created alone, outside a skill, has its unit commands removed - they would throw in the game's TimerMgr and stop every timer), `RemoveAllEffects()`, `GetVisibleBounds(go)`, `GetComponentSummary(go)`, `RemoveUnitParts(go)` (an effect that is a unit of a skill - a `UnitCtrlBaseMono` in it, e.g. heluoyuciji -: created alone it throws in the TimerMgr and stops every timer; its unit mono removed, its body turned off; `CreateEffect(disarm: true)` does it), `NoteUnitEffect(path)` / `IsUnitEffect(path)` / `UnitEffects` (such effects of the session: the mod reports them from its own code), `ListEffects("Effect/Battle/Skill")` (the game's prefabs; gallery: `docs/Effects`), `CreateEffectSafe(path, pos, lifeSec, out unitParts, out firstNoted, onCreated)` (created at once, disarmed, unit parts and camera controls taken out; `firstNoted`: a unit effect noted for the first time, to report)
- **GodArtifactHelper** - Artifact management
- **EconomyHelper** - Economy and trading

**UI & Localization:**
- **UIHelper** - UI operations and dialogs
- **UIClickHelper** - Drive the game's UIs like a player: `ClickButton(labels)` (any open UI but the title / map), `ClickIn(ui, labels)`, `ClickOption(label)`, `FindButton(ui, labelStart)`, `ScreenTexts()`, `FindTexts(ui, text)`, `CloseAll(types)`, `OpenFresh<T>(type, init)`, `CONFIRM_LABELS`
- **UISampleHelper** - UI sample components
- **CompHelper** - Component utilities
- **SpriteHelper** - Sprite and image handling
- **WallpaperHelper** - Wallpapers of the mods on the title screen, one file `ModConf/*_wallpaper.json` per wallpaper (its image of ModImg, order, hide the game's title, chance to start on it): added to the game's Change Theme window and shown over the game's wallpaper when chosen (saved in `ModTitleEvent.Wallpaper`, key `modId/file`). No code needed in the mod: see [08-Wallpapers](08-Wallpapers.md)
- **TitleModHelper** - The buttons of the mods on the title screen (name and version) side by side in one row at the bottom, centered: `TitleModHelper.AddModButton(text, onClick)` in `OnOpenUIEnd` of the Login UI; the row is laid out again whatever the order the mods add their buttons
- **ModOptionsHelper** / **ModOptionsPage** - Options a player may change: the schema `ModConf/<prefix>_options.json` (`ModOptions`: Title, Note, Options - Key = dotted path of a property of the mod's settings, Type float / int / bool / select, Min, Max, Step, Default, Choices, Label / Desc / Group = localText keys; one file per mod), the values in `options.json` of the global cache folder of the mod (outside the Workshop folder; global for every save; an unreadable file is never overwritten, unknown keys kept). The mod calls `ModOptionsHelper.Load(modId, settings)` at its load and at every world load, with its settings just read again from its own file (they are the defaults): only the values that differ from the defaults are kept in the file (`$format` 2, since 2.3.01: a new default of the mod reaches its players; `FormerDefaults` of an option: earlier defaults, dropped from the files of before 2.3.01, which hold every value), then applied within their bounds. `ReadValues` gives every option its value (chosen, else default), `Defaults(modId)` the defaults. A settings window lists `LoadedOptions()` and adds `ModOptionsPage.AddPages(ui, modId, schema, ReadValues(modId), columns, onPage)` (wraps to the next column / a new page; returns the getter of the edited values for `WriteValues`). Without such a window the player edits the values file
- **BalanceStatsHelper** - Numbers of the play for the balance of a mod (since 2.3.03), kept only on the player's computer: `balance-stats.json` of the global cache folder of the mod. `SetModVersion(modId, version)` at init (a new version starts a new file, the old one kept as `balance-stats-<version>.json`), `SetEnabled(modId, bool)` (an option of the players), `Add(modId, group, key, value)` / `Count(modId, group, key)`: series with n, sum, min, max, mean, p50, p90 (a random sample of 200 values); written after every save and battle end (ModMaster: `FlushAll`). A mod that may run with an older ModLib calls it from a `NoInlining` method in a try/catch (Dao of Creation `Helper/DocStats`)
- **TranslateHelper** - Translation and localization

**Drama & Dialogue:**
- **DramaHelper** - Dialogue and drama system (read and drive an open drama: `GetOpenDrama()`, `GetDramaText()`, `GetDramaOptions()`, `ChooseDramaOption(label)`, `NextDramaLine()`, `DismissDramas()`; `OpenDrama1(text, options, callback, left, right, speaker)`: `speaker` 1 or 2 lights the left or right portrait and dims the other, 0 both lit; 1 to 9 options use dialogues with exactly that many options (ModLib `dramaDialogue.json` 999890011-19 type 1, 999890021-29 type 2; 5-9 added in 2.2.34: the default dialogue showed no option at all), so a single "Continue" is a real button that calls the callback)
- **PortraitHelper** - Characters drawn with the game's portrait model into a RawImage (`Draw(parent, looks or unit, size, pos)`, `DrawOnScreen(looks, heightShare)` on its own overlay canvas, `RandomModelData(sex)` random looks without a world); the art goes from the head to the thighs; the texture takes the size of the RawImage when drawing
- **TextIconHelper** - Icons in texts: `[icon:NAME]` tokens of the localTexts become `<sprite name=NAME>` at load; `Text.Set` shows a text with a sprite through a TextMeshPro twin of the Text (same rect as the Text: a block starts at its top; rules: `04-LocalText.md`, "Rules"); `Icon(name)`, `Paint(color, text)`, `AttributeIcon("fire")` (the icon of one of the 12 attributes)
- **UIPanelHelper** - Drawing kit of the information windows of a UICustom (canvas units of the window, its Columns / Rows give the area): `Box` (a colored rectangle by its top left corner: cards, highlighted rows), `Text` / `RightText` / `Center` (a text by its left edge / right edge / center), `Group` (a group title with a game icon and a line under it), `Bar` (a progress bar and its text), `Grid` / `GridBottom` (top left corners of n cards in columns), `Table` (header + striped rows of cells at column shares, a row highlighted, a color a row); colors `PanelColors` (defaults for the parchment of the game windows), `Icon(name)`, `RowHeight`, `Left` / `Right` / `Top` / `Bottom` (2.3.04)
- **EffectStage** (`ModLib.Object`) - Battle effects in a window outside a battle: `new EffectStage(parent, pos, size, backgroundSprite)`, `Show(path, extent, durationMs, loop)` (one effect; `Tick()` plays it again) or `Spawn(path, pos, scale, angle)` / `Remove(go)` for a scene of several, `SetView`, `ToPicture`, `Update(dt)` every frame (its timed parts: the `DelayActiveSubobject` commands of the effect; the random direction of `EffectRandomDireCtrl` is drawn at the spawn; the other scripts of the battle are removed and counted in `EffectStage.RemovedScripts`), `Dispose`; subclass it for a moving scene (`EffectPreview` of Dao of Creation)
- **UIHelper.AddHelpButton** - `ui.AddHelpButton(x, y, title, content)`: a small "?" of a window, a message box with an explanation; `UIHelper.GetWorldCorners(rectTransform)` (the 4 corners in world space)
- **EffectCatalog** / **EffectCatalogItem** / **SkillIconTags** (`ModLib.Object`) - Catalogs a mod makes from `docs/Effects` and `docs/GameAssets/SkillIcons/tags.json`: `new EffectCatalog(modId, file)` (`All`, `Find(path)` - '/' or '\\', any case -, `Search(kind, words)`; an item: `Attributes()`, `ShowSec()`), `new SkillIconTags(modId, file)` (`Score(icon, wanted)`, `Suited(ids, wanted, iconOf)`: the ids whose icon shows the wanted attributes, the best first; Dao of Creation uses both)
- **UIGlowHelper** - A pulsing gold frame around the chosen item of a window: `UIGlowHelper.Add(ui, w, h)` once (after the items), `Place(glow, button, show)` when the choice changes, `FramedIndex(glow, buttons)` (which one is framed: tests)

**Data Processing:**
- **ObjectHelper** - Object operations (clone, compare, serialize)
- **ParseHelper** - Safe parsing and conversions
- **ValueHelper** - Value processing and validation
- **EnumerableHelper** - Collection operations
- **VectorHelper** - Vector math operations (`move.ToDirection8(minLength)`: "right", "up-left"... or "static"; `v.Rotate(deg)`, `v.AngleDeg()`)

**I/O & Network:**
- **FileHelper** - File I/O operations
- **HttpHelper** - HTTP requests and web operations
- **EncryptionHelper** - Data encryption/decryption

**Development:**
- **TraceHelper** - Execution tracing
- **ExceptionHelper** - Exception handling utilities
- **KnownErrorHelper** - Errors the player can fix himself: the error dialog (small TextInfo) shows the reason and what to do instead of the stack trace (stack kept in the log, no error report). Known: a `ModUserException` (thrown by a mod, e.g. `throw KnownErrorHelper.ConfigMissing("roleCreateFeature", id)` for a config id of the mod's ModConf that is not loaded), a missing file of the ModConf / ModImg / ModCode folder of a mod (incomplete install), a JSON error of a config file wrapped with `KnownErrorHelper.InConfFile(path, ex)` (damaged file; done by ConfHelper for the game configs of the mods), a missing member / type at run time ("Method not found", "Could not load type"...: of ModLib = ModLib too old, else game version not matching the mods)
