# Cultivation paths: feasibility (2026-11-20)

Survey of the side paths of the backlog (`05-Backlog.md`, "Cultivation paths") after body refining
([BodyCulEvent](Features/BodyCulEvent.md)). For each path: what the game already gives (types and members found in
`MelonLoader/Managed/Assembly-CSharp.dll`, ModLib events), what the mod already has, the work, the risks.

Scale: **easy** = mod data + existing ModLib events; **medium** = a new UI hook or a game member to set, testable by
autotest; **hard** = a Harmony patch of a game method, or game behaviour we cannot see.

## Summary

| Path | Feasibility | Main hook | Reuses | Suggested order |
|---|---|---|---|---|
| Artisan Dao (6 crafts) | medium (**done** for alchemy, forging, talismans: [ArtisanEvent](Features/ArtisanEvent.md)) | end-of-craft UIs / actions of the game | `NpcAutoCraftArtifact`, `CustomRefineEvent`, `RealMarketEvent`, `MissionDeclareEvent`, `FamilyEvent.Inherit` | 2 |
| Spirit sense | medium (map view: hard) (**done** without the map view: [SpiritSenseEvent](Features/SpiritSenseEvent.md); `playerViewRange` is a skill effect's, not the map's) | `playerViewRange`, `UpdateMapFog`; `WantedEvent` hunters; `SeclusionEvent.HeartDemon` | `WantedEvent`, `SeclusionEvent`, `EncounterEvent` | 4 |
| Dao heart | easy (value, sources) / hard (game breakthrough chance) (**done** without the game breakthrough: [DaoHeartEvent](Features/DaoHeartEvent.md)) | `OnUnitSetHeartState`, `StoryEvent` choices, relation changes | `StoryEvent`, `SeclusionEvent`, `FamilyEvent` | 3 |
| Merit | easy (**done**: [MeritEvent](Features/MeritEvent.md)) | `OnBattleUnitDie` (beasts), `EstateEvent` mortals, `RealTrialEvent` lightning damage | `EstateEvent`, `MapBuildPropertyEvent`, `RealTrialEvent`, `BodyCulEvent` tribulation | 1 |

The shared "Paths" frame (levels, exp sources, bottleneck, bonuses, at most 1-2 side paths a character) should be cut
out of `BodyCulEvent` when the second path is written: realm/layer formulas, `AddExp` with bottleneck and essence,
`Need`/`Materials`/`Consume`, the dose columns of the window, the Taoist log keys. Body refining stays the first user.

## Merit (công đức) - easy

- **Sources** (all on events the mod already handles):
  - beasts killed: `OnBattleUnitDie` of a monster (`ModBattleEvent`), more for a beast near a town (`MonstStrongerEvent`
    / `SpecialMonsterEvent` know the monster kinds); a town defended in a town war (`MapBuildBattleEvent`);
  - mortals helped: gifts to the town budget (`MapBuildPropertyEvent.AddBuildProperty`), mortal houses kept up
    (`EstateEvent` B1: population, plots), commissions done for mortals (`MissionDeclareEvent`);
  - loss: killing NPCs of the righteous side, bounties (`WantedEvent`) put on the player.
  The game has `StandUp` / `StandDown` (righteous / evil) in `UnitPropertyEnum`: merit can be a separate value, or
  feed `StandUp` a little.
- **Effects**: the lightning of the trial (`RealTrialEvent.OnBattleUnitInto` sets the attack of the trial units) and of
  the body tribulation is lower with merit: one formula line; a better reputation price in towns (`RealMarketEvent`);
  mortals of the estate grow faster.
- **Risks**: none found; no game member to patch. Balance only (merit farmed on weak beasts: count the beast grade
  against the player's).

## Artisan Dao (luyện kỹ nghệ) - medium

The game has the 6 crafts as properties (`UnitPropertyEnum.RefineElixir`, `RefineWeapon`, `Geomancy`, `Symbol`, `Herbal`,
`Mine`) and its own end-of-craft points:

| Craft | Game end point found | Note |
|---|---|---|
| Alchemy (luyện đan) | `UnitActionRefineElixir`, `OnRefineElixirEnd`, UIs `UIRefineElixir*`, town factory `UITownFactoryRefineElixir` | an action: patchable, or the UI close |
| Artifact forging (luyện khí) | `UnitActionPropArtifactShapeMade`, UI `ArtifactShapeMadeSuccess` | already hooked by `NpcAutoCraftArtifact.OnOpenUIEnd` |
| Talismans (phù) | `UIRefineRuneFormula`, `RuneDrawSuccess(ConfRuneFormulaItem)` | a private method: Harmony patch or UI close |
| Feng shui (phong thủy) | `UIGeomancy*`, `AddGeomancyValue`, `geomancyResult` | a mini-game; its result is in the UI |
| Herbs / mining | `UnitActionGridMine`; herbs are picked on map grids | a "masterpiece" makes little sense: exp from the grade of what is gathered |

- **Work**: a `CraftEvent` with a level a craft (exp from the grade and quality of what is made: the item conf after
  the craft, `ConfItemPropsItem.level`); "masterpiece" = an item of a grade above the craft level, which gives the
  cultivation exp (`AddExp`) and can replace a pill at a breakthrough; titles by a yearly exam (a drama in a city,
  `OnYearly`); market prices x by the title (`RealMarketEvent`); named masterpieces need a saved table soleID -> maker
  (the game item has no free text field: the name shown via `OnOpenUIEnd` of the item info); furnace accidents (above
  one's level: the materials lost, a small lifespan / HP loss, reuse of `BodyCulEvent.LoseLife`); apprentices
  (`HirePeopleEvent`); passed to heirs (`FamilyEvent.Inherit` copies the properties already).
- **Risks**: the end points of talismans and feng shui are private methods or mini-game UIs, so the first step is an
  autotest that crafts each kind and logs which hook fires (as TB-xx do for battles). Alchemy and forging are safe.

## Dao heart (đạo tâm) - easy for the value, hard for the game's breakthrough

- **Sources**: kept promises = storyline choices (`StoryEvent`, lines A-J, each choice already logged); heart demons
  beaten (`SeclusionEvent.Kind.HeartDemon` result, the game's own heart state through ModLib `OnUnitSetHeartState`);
  betrayal = a relation to a friend / spouse / master turned to hatred (`relationData`), a broken loan (`BankLoanEvent`),
  a sect left (`UnitActionSchoolExit`).
- **Effects that are easy**: the mod's own chances: body breakthrough (`BodyCulEvent.BreakChance`), the heart demon
  seclusion (50 % now), furnace accidents of Artisan Dao, the backlash of the strong doses.
- **Effect that is hard**: the game's own breakthrough. The game exposes only `UnitActionRoleUpGrade` with
  `upgradePhaseForceSuccess` (a yes / no, used by TG-65) and `OnPlayerRoleUpGradeBig` after the fact; the chance of a
  small breakthrough and the big breakthrough mini-game are inside the game. A Harmony patch of the action could turn a
  failure into a success with a chance from the Dao heart; to be tried in a test before a promise in the design.
- **Risks**: the value must stay readable (a short log of why it moved), else players feel it random.

## Spirit sense (thần thức) - medium; the map view is hard

- **Map view**: the game has `playerViewRange` (a property with a setter), `additionalVision`, `UpdateMapFog`,
  `RoleSightWipe` and the effect `WorldUnitEffectSpecialRoleSight`. Setting the range from the spirit sense level is a
  few lines, but when the game resets it (scene load, effects) is unknown: a test must watch the value after a load,
  a battle and a month. If it is reset, a Harmony patch on its getter is needed.
- **Hunters seen**: easy. `WantedEvent` makes the bounty hunters and stalkers (S&M `NoStalker`); with enough spirit
  sense a drama warns before the ambush and offers to avoid it (as the encounters of `EncounterEvent`).
- **Heart demon resistance**: easy, the same hook as the Dao heart (`SeclusionEvent.Kind.HeartDemon` chance).
- **Exp**: seclusions, a "spirit sense" medicine (a herb class), reading the scrolls of the libraries.
- **Risks**: the fog / vision is the only part that touches game behaviour we cannot see.

## Not surveyed in depth

- **Beast taming**: the game has the sect god beast (`MapBuildSchoolGodBeast`, `UISchoolGodBeast`, feed / rewards
  confs) and potmons (`UIPotmonRefine`); a tamed beast fighting with the player would need a summoned unit in battle:
  hard.
- **Formations** (protect the estate): the estate and town wars are the mod's own (`EstateEvent`, `MapBuildBattleEvent`):
  a formation room giving the town defense is easy; formations in battle are hard.

## Next step proposed

Merit first (smallest, all on the mod's own events, gives the body tribulation a second use), then cut the "Paths" frame
out of `BodyCulEvent` while writing Artisan Dao (alchemy and forging first, talismans / feng shui after a hook test),
then Dao heart, then spirit sense (hunters and heart demons first, the map view after a test of `playerViewRange`).
