# BattleAfterEvent

`3161035078/ModProject/ModCode/ModMain/Mod/BattleAfterEvent.cs` - cache key `ModConst.BATTLE_AFTER_EVENT` - group: Battle

Stalkers: after a battle with a valuable drop, strong ill-intentioned cultivators around may follow the player (with their team) to rob them later. The stalkers then join the player's battles as enemies ([BattleEvent](BattleEvent.md)).

## Saved data

| Property | Meaning |
|---|---|
| `Stalkers` | `Dictionary<unitId, StalkReasonEnum>`: units following the player and why (`KillBoss`, market deals...) |

## Switch

`NoStalker` (S&M): no new stalkers after battles.

## Flow

| Hook | What happens |
|---|---|
| `OnBattleEndHandler` | Best drop of the battle (by item value) of level > 2: for every unit around the player (range = player grade) that `IsStalker` and could rob that item (`CheckItemCouldBeRobbed`): 30 % (`STALKE_RATE`) -> the unit and its whole team become stalkers (`KillBoss`); otherwise it hates the player (+100 hate). |
| `OnMonthly` | Every stalker's team members are moved to a random position within 4 of the player. |

`IsStalker(wunit)`:

- HP > 70 % max, MP > 50 % max, SP > 30 % max;
- battle power >= the player's (`UnitPowerCalHelper.CalWUnitBattlePower`);
- inner trait Wicked, Selfish, Evil, Glory Hound or Power-hungry, **or** intimacy with the player <= -100.

## API

`AddStalker(reason, params wunits)`: adds units and their teams (no duplicates). Used by the player market ([RealMarketEvent3](RealMarketEvent3.md): a seller / buyer may stalk after a deal), encounters, etc.

## Notes

- A unit already stalking is not added twice (BUG-03).
- Stalkers are removed when they join / are beaten in a battle ([BattleEvent](BattleEvent.md)); dead units stay in the dictionary until then (skipped as `GetUnit` returns null).

## Tests

TB-07.
