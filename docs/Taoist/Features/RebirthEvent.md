# RebirthEvent

`3161035078/ModProject/ModCode/ModMain/Mod/RebirthEvent.cs` - cache key `ModConst.REBIRTH_EVENT` - group: World and NPCs

Rebirth (a button of the player info, beside the damage calculation; until 6.0 at the pub): the player goes back to grade 1 and loses exp and reputation, and gets a permanent luck that grows with each rebirth. Monsters get stronger with the total of the grades given up.

## Saved data

| Property | Meaning |
|---|---|
| `RebirthLevel` | Grade of the last rebirth (the next one needs a higher grade) |
| `RebirthCount` | Number of rebirths |
| `TotalGradeLvl` | Sum of the grades given up; read by [MonstStrongerEvent](MonstStrongerEvent.md) |

## Switch

`NoRebirth` (S&M): no button.

## Flow

| Hook | What happens |
|---|---|
| Player info (UIEvent, the player's own) | Button `RebirthEvent.ButtonText` (`rebirth{420101001 + RebirthCount}`) left of "Damage Calc", tooltip `rebirth420103103`; shown unless NoRebirth. A click: `RebirthEvent.Open` - `Missing(player)` tells what is missing (`rebirth_need_grade`: grade above RebirthLevel; `rebirth_need_town`: in a town with `areaBaseID > RebirthLevel` and `>= grade`; `rebirth420103100`: no sect), else the confirmation, the rebirth, the player info closed. |

Click:

1. Player in a sect -> message `rebirth420103100` (leave the sect first), stop.
2. Yes/No confirmation (`rebirth420103101`). On yes:
   - `RebirthCount + 1`, `RebirthLevel = grade`, `TotalGradeLvl += grade`;
   - grade reset (`ResetGradeLevel`), exp cleared, reputation 0;
   - lucks `420101001..420101010` removed, luck `420101000 + RebirthCount` added;
   - drama `420109999`.

## Notes

- Items, money, skills and properties stay: rebirth trades time for a permanent destiny.
- Max luck level 10 (`MAX_LUCK_LVL`); an 11th rebirth would add a luck id that does not exist.
- Because the next rebirth needs a higher grade and a higher town than the last one, there are at most about 9 rebirths.

## Tests

TG-22, TM-14.
