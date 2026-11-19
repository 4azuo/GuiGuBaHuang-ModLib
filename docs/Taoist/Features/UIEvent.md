# UIEvent

`3161035078/ModProject/ModCode/ModMain/Mod/UIEvent.cs` - cache key `ModConst.UI_EVENT` - group: UI

UI additions that show the Taoist numbers in the game's windows: bonuses of the bases and ability points, expertise and refine of items and martial arts, the town / sect tax button, the damage window button and the S&M title in the player info; hides some map widgets and (optionally) the save / reload buttons.

## Saved data

None (only references to the texts it created).

## Switches (S&M)

`HideSaveButton`, `HideReloadButton`.

## Flow

| Hook | What happens |
|---|---|
| `OnOpenUIEnd` (in game, not in battle) | `OnUIOpen(e)` (table below) |
| `OnTimeUpdate100ms` | `OnUIUpdate()`: keeps the added texts placed on `PropInfo` / `ArtifactInfo` (the game moves them); only when the texts belong to the open window |
| `OnCloseUIEnd(MartialInfo)` | closes the expertise preview |

## Windows

| Window | Additions |
|---|---|
| `MapMain` | hides the Pisces Pendant count and the added-luck root |
| `Town` (1 frame later) | guardian / master: "Manage town" (`other500020005`) -> `MapBuildPropertyEvent.OpenUITownManage`; others: "Tax: N" (`ui_tax_town`) -> guardians list (`OpenUITownGuardians`); tooltip `other500020020` ([MapBuildPropertyEvent](MapBuildPropertyEvent.md)) |
| `School` (not a member) | disabled button showing the sect tax (`ui_tax`) |
| `NPCInfo` | under the physical / magic / artisan bases: +HP (`GetMartialAdjHp`), +MP (`GetSpiritualAdjMp`), +SP (`GetArtisanshipAdjSp`); under the ability points: +HP, +MP (live) |
| `PlayerInfo` | the same texts with tooltips (`other500020028`, `033`-`038`); for the player, on the top edge: "Damage Calc" (`dmginfo_button`, `DMG_BUTTON_DX` -190 from the close button) -> `DamageInfo.Open()`, and the S&M title line (`SCORE_DX` -1150) ([SMScoreEvent](SMScoreEvent.md)) |
| `ArtifactInfo` (from item select, player info, storage, or NPC info when the player is a Merchant) | window moved to the center; **basic** Atk / Def / HP bonuses, **expertise** level (exp / next) and Atk / Def bonus, **refine** level, Atk / Def and slots 1-3 (colored by quality) |
| `MartialInfo` (from player info) | 1 s later opens an `NPCInfoPreview` beside it as the **expertise** panel: level (exp / next); ability: Atk, Def, HP, MP, SP; step: speed, evade, MP cost; skill: damage, MP cost |
| `PropInfo` ring / outfit / mount (same contexts) | refine level, the item's refine bonus (ring HP; outfit HP and Def; mount MP), slots 1-2 |
| `GameMemu` | save / reload buttons hidden by the switches |

Refine slots of an item another unit sells are hidden (`other500020083`) unless the player is a Merchant (in the selector, or the market selling list for outfits / mounts).

Formulas: [UnitModifyEvent](UnitModifyEvent.md), [ExpertEvent](ExpertEvent.md), [CustomRefineEvent](CustomRefineEvent.md).

## Tests

TG-24, TG-26, TG-48.
