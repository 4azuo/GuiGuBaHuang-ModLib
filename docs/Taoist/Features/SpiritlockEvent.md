# SpiritlockEvent

`3161035078/ModProject/ModCode/ModMain/Mod/SpiritlockEvent.cs` - cache key `ModConst.SPIRITLOCK_EVENT` - group: Battle

Balance change for the spiritlock circle fight.

## Saved data

None.

## Flow

| Hook | What happens |
|---|---|
| `OnBattleUnitInto` | Monsters only (`UnitDataMonst`). Venerable tortoises (`unitAttrItem.id` 12005-12011): move speed 0, defense halved. Spiritlock circle (12012): move speed 0. |

## Notes

The halved max HP is commented out in the code.

## Tests

TM-24.
