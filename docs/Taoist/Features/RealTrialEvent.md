# RealTrialEvent

`3161035078/ModProject/ModCode/ModMain/Mod/RealTrialEvent.cs` - cache key `ModConst.REAL_TRIAL_EVENT` - group: Battle

Trial of lightning (the breakthrough tribulation dungeon): the lightning hits as hard as the player, scaled by the game level and grade; surviving makes the player permanently stronger.

## Saved data

None.

## Switch

`real_trial_configs.json` (`RealTrialConfig`): `PowerUpOnGameLevel` 0.64, `PowerUpOnGradeLevel` 0.08.

## Flow

`IsInTrial` = `DungeonHelper.IsTrailOfLightning()`.

| Hook | What happens |
|---|---|
| `OnBattleUnitInto` | In the trial, every non-player unit (the lightning) gets a new base attack. |
| `OnBattleEndHandler` | In the trial and the player is alive: the player gains the `Trial` role growth once. |

## Formulas

Lightning attack:

```
atk = player Atk x (gameLevel x 0.64 + grade x 0.08) - player Def x BasisThunder / 100
```

A high thunder basis lowers the lightning damage.

Reward (`UnitTypeEnum.Trial`, growth = `max(value x ratio, minimum)`):

| Property | Ratio | Minimum |
|---|---|---|
| Attack | 2 % | 1 |
| Defense | 2 % | 1 |
| Max HP | 5 % | 100 |
| Max MP | 2 % | 10 |

## Tests

TB-08.
