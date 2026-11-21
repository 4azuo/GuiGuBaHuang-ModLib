# BattleManashieldEvent

`3161035078/ModProject/ModCode/ModMain/Mod/BattleManashieldEvent.cs` - cache key `ModConst.BATTLE_MANASHIELD_EVENT` - group: Battle

Every human (player, NPCs) starts a battle with a mana shield that absorbs damage before HP. Also the shield API used by [SpecialMonsterEvent](SpecialMonsterEvent.md).

## Saved data

None.

## Switch

`battle_manashield_configs.json` (`BattleManashieldConfig`): `ManaShieldRate1` 0.15, `ManaShieldRate2` 0.05.

## Flow

| Hook | What happens |
|---|---|
| `OnBattleUnitInto` | Humans (`IsHuman`): `ShieldUp(unit, GetInitShield(wunit), max HP)`. |

## Formulas

```
shield = MP x 0.15
       + MaxMP x 0.05
       + HP x 0.05 x bloodEnergyLevel
       + BasisFist / 100 x Defense
       + refine bonus (CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.Manashield))
max shield = max HP
```

Blood energy level from lucks: 700094 -> 3, 700093 -> 2, 700092 -> 1, else 0.

## API

- `ShieldUp(cunit, shield, maxShield)`: adds effect `903151120` and a shield `Effect3017.AddShield(..., 903151121, shield, maxShield, int.MaxValue)` (no duration limit). Does nothing after the battle end.
- `GetInitShield`, `GetManashieldBase`, `GetManashieldBasePlus`: used by the damage window ([DamageInfo](DamageInfo.md)) and the player info.

## Notes

`IsWarlordPhantom` (luck 700026) is defined but not used.

## Tests

TG-19, TM-28.
