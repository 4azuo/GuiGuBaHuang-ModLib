# SpiritSenseEvent

`3161035078/ModProject/ModCode/ModMain/Mod/SpiritSenseEvent.cs` - cache key `ModConst.SPIRIT_SENSE_EVENT` - group: World and NPCs

Spirit sense (thần thức): the reach of the mind, for the player and the NPCs. Mainly **max SP** (and max MP). Off with the
S&M switch `NoSpiritSense` (+500 points).

## Saved data

| Property | Type | Meaning |
|---|---|---|
| `Senses` | `Dictionary<unitId, SenseData>` | `Exp` (0-50,000), `Essence` (above the top), `LastScanMonth`, `Avoided` (ambushes avoided) |

## Levels and effects

| Level | Name | Exp | Max SP | Max MP | Ambush sensed | Heart demons |
|---|---|---|---|---|---|---|
| 0 | Dormant (Chưa khai mở) | 0 | - | - | - | x1 |
| 1 | Spirit Perception (Linh giác) | 200 | +4 % | +2 % | 20 % | x0.85 |
| 2 | Divine Thought (Thần niệm) | 1,000 | +8 % | +4 % | 40 % | x0.70 |
| 3 | Sense Beyond the Body (Thần thức ly thể) | 4,000 | +12 % | +6 % | 60 % | x0.55 |
| 4 | Sense Domain (Thần vực) | 15,000 | +16 % | +8 % | 80 % | x0.40 |
| 5 | Sense of Heaven (Thông thiên thần thức) | 50,000 | +20 % | +10 % | 90 % | x0.25 |

- Max MP / SP are shares of the base value (`UnitModifyEvent`); with the Dao heart they add up.
- **Essence** above the top: each 10,000 is a step of max SP +1 %.
- **Ambush sensed** (`SenseAmbush`, from `WantedEvent.Ambush`): a bounty hunter comes for the player; with the chance of
  the level the hunter is sensed and the player slips away (drama, Taoist log `log_sense_ambush`), no ambush.
- **Heart demons**: the monthly chance of the forced heart demon seclusion x the rate of the level (`SeclusionEvent.HeartDemonRate`).
- **Scan** (window button, once a month, +20 exp): range 6 + 3 a level; the cultivators around, the hostile ones (intimacy
  -100 or less), the strongest one and its grade, the herbs and ores of the cells, and the nearest mine vein / herb valley
  ([ArtisanEvent](ArtisanEvent.md)) with its distance and direction.
- **Scan window** (`SpiritSenseEvent.Scan.cs`, `OpenScanWindow`, a big `UICustom1`): the summary line; up to 10
  cultivators around as cards (portrait by `PortraitHelper.Draw`, hostile first then by grade; the name is a button that
  opens the game's NPC info `UINPCInfo`; grade and attitude under it; "... and n more"); up to 4 places in reach (mine
  veins, herb valleys, towns; the nearest spot when none is in reach) with a picture (`ModImg/scan_mine.png`,
  `scan_herb.png`, `scan_town.png`, from the drama backgrounds of the game), area, distance and direction.
  `ScanResult.UnitList` / `Places` keep what the window shows (TG-85 checks them and opens an NPC from the window).
- The map view of the game cannot be widened: `playerViewRange` belongs to the effect of one skill, not to the map (a
  Harmony patch would be needed).

## Exp

Every month 2 + Talent / 20 (player and NPCs); a seclusion 5 a month; a heart demon beaten +200; a scan +20.

## UI

"Spirit Sense" button on the cultivation screen (under "Dao Heart") -> window: level, exp / next, essence, effects, the
scan button, the table of the levels. NPC info: "Spirit sense: Divine Thought".

## Tests

TG-85 (autotest), `FormulaTests.Mind_Formulas`.
