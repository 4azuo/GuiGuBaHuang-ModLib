# BattleModifyEvent

`3161035078/ModProject/ModCode/ModMain/Mod/BattleModifyEvent.cs` - cache key `ModConst.BATTLE_MODIFY_EVENT` - group: Battle

The Taoist damage pipeline: every hit is rewritten step by step (nullify, instant kill, evasion, time multiplier, skill / basis / MP / SP / DP damage, critical, block, basis defense, MP / SP / DP block, minimum damage, life steal). Also: extra MP costs of skills and steps, HP/MP/SP/DP regeneration in battle, defense and speed from the bases, and the battle screen additions (damage multiplier, damage window button).

Each step is recorded by `Trace` for the damage window ([DamageInfo](DamageInfo.md)); `TraceDamage = true` also logs it (autotest TB-04).

## Saved data

None. Runtime: `_nullify` (nullify charges left per unit, cleared on the map).

## Switches

- `HideBattleMap` (S&M): hides the battle minimap.
- `UISettingScreenEvent.HideDamageMultiplier` (Taoist Settings): hides the multiplier text.
- All "Plus" values come from the refine adjustment slots ([CustomRefineEvent](CustomRefineEvent.md)).

## Battle screen (`OnOpenUIEnd(BattleInfo)`)

- `DamageInfo.Reset()`; button "Damage Calc" (`dmginfo_button`) at the top center -> `DamageInfo.Toggle`.
- Damage multiplier text (`other500020089`, live, `#,##0.00`) unless hidden.
- Monster counters hidden (or replaced when `IsShowCustomMonstCount`, used by [MapBuildBattleEvent](MapBuildBattleEvent.md)).

## Unit enters (`OnBattleUnitInto`, humans)

```
Def   += (Fist + Palm + Finger) / 2 / 1000 x Def + Earth / 1000 x Def
Speed += Wind / 100
nullify charges = adjustment Nullify
```

## Damage pipeline (`OnBattleUnitHitDynIntHandler`)

`atk` = attacker base attack, `def` = defender base defense, `B(x)` = basis of the damage type of the hit (blade, fire...), `G_a` / `G_d` = grade of attacker / defender. "World unit" = a cultivator (player / NPC).

| # | Step | Rule |
|---|---|---|
| 1 | Nullify | defender is a world unit with charges: one used, damage 0, stop |
| - | Instant kill | world unit hits a **monster** (BUG-01): chance `InstantKill` adjustment % -> damage int.Max, stop |
| 2 | Evasion | world unit hit: `min(12 + EvadeChanceMax adj, sqrt(Wind / 18)) + step expertise evade + EvadeChance adj` %; monster hit: `min(12, sqrt(Wind / 18))` %. Evaded: 0, stop |
| 3 | Time | `+ dmg x multiplier x G_a / 3`, multiplier = `1.001^battleTime x 1.003^roomTime` (seconds) |
| 4 | Skill | world unit attacker with a casting skill: `+ skill expertise damage x SkillDamage adj` |
| 4 | Basis | `+ atk x B_attacker / 1000` |
| 5 | MP | world unit attacker with MP: `+ atk x MP / maxMP`, costs `G_a / 5` MP |
| 6 | SP | grade >= 4: `+ atk x SP / maxSP` |
| 7 | DP | grade >= 8: `+ atk x DP / maxDP` |
| 8 | Critical | not already critical. World unit: chance `min(8 + SCritChanceMax adj, sqrt(Thunder / 50)) + SCritChance adj` %, damage `+ dmg x (2 + Fire / 1000 + SCritDamage adj)`. Monster: 0.4 % x4, 2 % x3, 8 % x2 (one roll) |
| 9 | Block | chance `min(18 + BlockChanceMax adj, sqrt(B_defender / 12))` % (monster: max 18): `- (Def + BlockDmg adj)` |
| 10 | Basis defense | if above min: `- def x B_defender / 400` |
| 11 | MP block | world unit hit, MP > 20 % max, above min: `MP/maxMP x G_d` tries; each costs `max(G_d, dmg / (physical bases / 50))` MP and blocks `def x MP/maxMP x blockRatio` |
| 12 | SP block | grade >= 4: `- def x SP/maxSP x blockRatio`, -1 SP |
| 13 | DP block | grade >= 8: `- def x DP/maxDP x blockRatio` |
| final | Minimum | `minDmg x G_d`: world unit attacker `grade (+ rebirth grades for the player) + MinDamage adj`; monster `grade x gameLevel` |
| - | Steal | world unit attacker: HP / MP / SP `+ damage x Steal* adj` |

`blockRatio` (world units only, else 0) = by grade 1..10: 0.50, 0.55, 0.60, 0.70, 0.80, 0.95, 1.10, 1.30, 1.50, 2.00, `+ magic bases sum / 10,000`.

## MP costs

| Hook | Extra MP |
|---|---|
| `OnBattleUnitUseSkill` | `max(skill MP cost x L x (0.06G + 0.01l), grade x L / 10)` |
| `OnBattleUnitUseStep` | `max(step MP cost x L x (0.08G + 0.01l), grade x L / 5)` |

`L` = expertise level of the skill / step ([ExpertEvent](ExpertEvent.md)), 0 when no expertise.

## Regeneration (`OnTimeUpdate200ms`, every 0.2 s)

| Unit | Rule |
|---|---|
| Monster grade >= 3 | HP `+ sqrt(sqrt(maxHP / 3)) + sqrt(Wood / 100)` |
| Human HP | `sqrt(Wood / 100)` + RHp adj |
| Human MP | `sqrt(Froze / 200)` + RMp adj |
| Human SP / DP | RSp / RDp adj only |

## Public bases

`GetEvadeBase`, `GetEvadeMaxBase`, `GetBlockMaxBase`, `GetBlockDmgBase`, `GetSCritChanceBase`, `GetSCritChanceMaxBase`, `GetSCritDamageBase`, `GetMinDmgBase`, `GetHpRecoveryBase`, `GetMpRecoveryBase` and their `*Plus`: used as the base of the refine adjustments (`AdjTypeEnum`) and by the damage window.

## Tests

TG-18, TM-27, TB-04.
