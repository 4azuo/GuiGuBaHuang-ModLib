# DaoHeartEvent

`3161035078/ModProject/ModCode/ModMain/Mod/DaoHeartEvent.cs` - cache key `ModConst.DAO_HEART_EVENT` - group: World and NPCs

Dao heart (đạo tâm): the steadiness of the mind, for the player and the NPCs. Mainly **max SP and max MP**. Off with the
S&M switch `NoDaoHeart` (+500 points).

## Saved data

| Property | Type | Meaning |
|---|---|---|
| `Hearts` | `Dictionary<unitId, HeartData>` | `Value` (0-2,000, a new unit starts at 100: Steady), `Essence` (above the top) |

## Levels and effects

| Level | Name | Value | Max SP | Max MP | Heart demon beaten |
|---|---|---|---|---|---|
| 0 | Wavering (Dao động) | 0 | - | - | 50 % |
| 1 | Steady (Kiên định) | 100 | +2 % | +2 % | 58 % |
| 2 | Firm (Kiên cố) | 300 | +4 % | +4 % | 66 % |
| 3 | Unshaken (Bất động) | 600 | +6 % | +6 % | 74 % |
| 4 | Diamond Heart (Kim cương đạo tâm) | 1,000 | +8 % | +8 % | 82 % |
| 5 | Heart of Heaven (Thiên tâm) | 2,000 | +10 % | +10 % | 90 % |

- Max SP / MP are shares of the base value (`UnitModifyEvent.GetAdjustMaxSp / GetAdjustMaxMp`).
- **Essence** above the top: each 1,000 is a step of max SP +1 %.
- A little more: the body breakthrough chance +2 a level (`BodyCulEvent.Breakthrough`), the backlash of the medicinal
  baths and the furnace accidents of Artisan Dao -8 % a level (`RiskRate`), the heart demon seclusion is beaten with
  `HeartDemonWin` instead of 50 % (`SeclusionEvent.Reward`).

## Sources

| Deed | Points |
|---|---|
| heart demon beaten / lost (seclusion) | +100 / -60 |
| a seclusion | +10 a year |
| a body breakthrough / a failed one (player and NPCs) | +15 / -10 |
| a commission delivered / late (Artisan Dao: a promise) | +20 / -30 |
| killing a bond in a battle (family, master, spouse, student, intimacy 200+): betrayal | -150 |
| the player's mood, every month: 80 %+ / 20 % or less of the max | +1 / -1 |
| NPCs every month | +0..2 |

Taoist log: `log_heart_up`, `log_heart_down` (with the deed), `log_heart_level`.

## UI

"Dao Heart" button on the cultivation screen (left of the figure, under "Artisan Dao") -> window: level, value / next,
essence, effects, the table of the levels. NPC info: "Dao heart: Firm".

## Tests

TG-85 (autotest), `FormulaTests.Mind_Formulas`.
